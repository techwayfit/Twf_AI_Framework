using System.Text.Json;
using Microsoft.Extensions.Logging;
using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Interprets a <see cref="WorkflowDefinition"/> (as saved by the UI designer) and executes
/// it using the core TwfAiFramework runtime engine.
///
/// Execution model:
///   - Walks the node graph starting from the Start node, following named output ports.
///   - Routes to the correct branch based on each node's activated output port.
///   - BranchNode   → reads branch_selected_port from result data.
///   - SubWorkflow  → recursively executes the child workflow; routes success/error.
///   - ErrorNode    → invoked automatically when an unhandled exception occurs.
///   - All other nodes → always activate the "output" port.
///
/// Node instantiation uses reflection: each node type is looked up in the core assembly
/// and constructed via its Dictionary&lt;string, object?&gt; constructor.
/// Node execution options (retry, timeout, continue-on-error) are honoured per node.
/// </summary>
public sealed class WorkflowDefinitionRunner
{
    private readonly ILogger<WorkflowDefinitionRunner> _logger;

    /// <summary>
    /// Registry of all INode implementations in the core assembly, keyed by class name.
    /// Built once at startup via reflection — no switch-case required.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Type> _nodeTypeRegistry =
        typeof(BaseNode).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(INode).IsAssignableFrom(t))
            .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parameter keys whose values are never treated as {{variable}} templates.
    /// Only block keys where the stored value is a literal secret that should
    /// never be accidentally expanded — not where the user intentionally typed
    /// a {{variable}} reference to inject a secret from workflow variables.
    /// </summary>
    private static readonly HashSet<string> _noResolveKeys =
        new(StringComparer.OrdinalIgnoreCase) { };

    public WorkflowDefinitionRunner(ILogger<WorkflowDefinitionRunner> logger)
    {
        _logger = logger;
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Execute the workflow and fire <paramref name="onStep"/> before and after each node.
    /// Use this overload when streaming real-time progress to a UI (e.g. Server-Sent Events).
    /// </summary>
    public async Task<WorkflowRunResult> RunWithCallbackAsync(
        WorkflowDefinition definition,
        WorkflowData? initialData,
        Func<NodeStepEvent, Task> onStep)
    {
        var startNode = definition.Nodes.FirstOrDefault(n => n.Type == "StartNode")
            ?? throw new InvalidOperationException(
                $"Workflow '{definition.Name}' has no Start node. " +
                "Drag a Start node onto the canvas and save before running.");

        var nodeMap   = definition.Nodes.ToDictionary(n => n.Id);
        var routing   = BuildRouting(definition.Connections);
        var context   = new WorkflowContext(definition.Name, _logger);

        var data = initialData?.Clone() ?? new WorkflowData();

        // Seed workflow-level variables into both WorkflowContext (code-API compat)
        // and WorkflowData so {{variable}} substitution works in PromptBuilderNode, etc.
        foreach (var (k, v) in definition.Variables)
        {
            context.SetState(k, v);
            if (!data.Keys.Contains(k)) // initial data takes priority
                data.Set(k, v);
        }

        _logger.LogInformation("▶ Running workflow '{Name}' from JSON definition", definition.Name);

        var (resultData, success, error, failedNode) = await WalkGraphAsync(
            startNode.Id, data, context, nodeMap, routing, definition, onStep);

        if (success)
        {
            _logger.LogInformation("✅ Workflow '{Name}' completed successfully", definition.Name);
            return WorkflowRunResult.Success(definition.Name, resultData);
        }
        else
        {
            _logger.LogError("❌ Workflow '{Name}' failed at '{Node}': {Error}",
                definition.Name, failedNode, error);
            return WorkflowRunResult.Failure(definition.Name, resultData, error, failedNode);
        }
    }

    /// <summary>Execute the workflow without a step callback (single-shot JSON result).</summary>
    public Task<WorkflowRunResult> RunAsync(
        WorkflowDefinition definition,
        WorkflowData? initialData = null)
        => RunWithCallbackAsync(definition, initialData, _ => Task.CompletedTask);

    // ─── Graph Walker ─────────────────────────────────────────────────────────

    private async Task<(WorkflowData data, bool success, string? error, string? failedNode)>
        WalkGraphAsync(
            Guid startNodeId,
            WorkflowData data,
            WorkflowContext context,
            Dictionary<Guid, NodeDefinition> nodeMap,
            Dictionary<(Guid, string), Guid> routing,
            WorkflowDefinition definition,
            Func<NodeStepEvent, Task> onStep)
    {
        // Find error node ID for this workflow (used for fallback routing)
        var errorNodeId = definition.ErrorNodeId;

        var currentId = startNodeId;
        const int maxSteps = 500; // guard against infinite loops
        var steps = 0;

        while (steps++ < maxSteps)
        {
            if (!nodeMap.TryGetValue(currentId, out var nodeDef))
            {
                _logger.LogWarning("Node {Id} not found in workflow — stopping.", currentId);
                break;
            }

            _logger.LogDebug("  → [{Type}] {Name}", nodeDef.Type, nodeDef.Name);

            // ── Structural nodes ──────────────────────────────────────────────

            if (nodeDef.Type is "StartNode")
            {
                await using var s = await NodeStepScope.StartAsync(nodeDef, [], false, false, onStep);
                s.Complete([]);
                if (!routing.TryGetValue((currentId, "output"), out var nextId))
                    break;
                currentId = nextId;
                continue;
            }

            if (nodeDef.Type is "EndNode")
            {
                await using var s = await NodeStepScope.StartAsync(nodeDef, [], false, false, onStep);
                s.Complete([]);
                return (data, true, null, null); // success!
            }

            if (nodeDef.Type is "ErrorNode")
            {
                // ErrorNodes are walk-entry-points, not normal targets.
                // If we land here it means something explicitly connected to it.
                if (!routing.TryGetValue((currentId, "output"), out var nextId))
                    return (data, true, null, null);
                currentId = nextId;
                continue;
            }

            // ── SubWorkflowNode ────────────────────────────────────────────────

            if (nodeDef.Type is "SubWorkflowNode")
            {
                var subIdStr = NodeParameters.GetString(nodeDef.Parameters, "subWorkflowId");
                if (Guid.TryParse(subIdStr, out var subId))
                {
                    var childDef = definition.SubWorkflows.FirstOrDefault(sw => sw.Id == subId);
                    if (childDef != null)
                    {
                        var childNodeMap   = childDef.Nodes.ToDictionary(n => n.Id);
                        var childRouting   = BuildRouting(childDef.Connections);
                        var childStart     = childDef.Nodes.FirstOrDefault(n => n.Type == "StartNode");

                        if (childStart != null)
                        {
                            // Build a transient WorkflowDefinition for the child so the walker
                            // can resolve its sub-sub-workflows if needed.
                            var childWrapDef = new WorkflowDefinition
                            {
                                Id          = childDef.Id,
                                Name        = childDef.Name,
                                Nodes       = childDef.Nodes,
                                Connections = childDef.Connections,
                                Variables   = childDef.Variables,
                                ErrorNodeId = childDef.ErrorNodeId,
                                SubWorkflows = definition.SubWorkflows // share the root registry
                            };

                            var (childData, childSuccess, childError, childFailed) =
                                await WalkGraphAsync(childStart.Id, data.Clone(), context,
                                    childNodeMap, childRouting, childWrapDef, onStep);

                            data = childData;
                            var selectedPort = childSuccess ? "success" : "error";

                            if (!routing.TryGetValue((currentId, selectedPort), out var nextId))
                            {
                                if (!childSuccess)
                                    return (data, false, childError, childFailed);
                                break;
                            }

                            currentId = nextId;
                            continue;
                        }
                    }
                }

                // Sub-workflow config missing — pass through on output
                if (!routing.TryGetValue((currentId, "output"), out var fallbackId))
                    break;
                currentId = fallbackId;
                continue;
            }

            // ── LoopNode — ForEach over a collection ──────────────────────────

            if (nodeDef.Type is "LoopNode")
            {
                // itemsKey may be a literal key ("search_results") or a {{node.key}} reference.
                // Strip {{ }} to get the actual data key — do NOT call ResolveVariables here
                // because that converts the stored list to a string via .ToString().
                var itemsKeyRaw = NodeParameters.GetString(nodeDef.Parameters, "itemsKey") ?? "items";
                var itemsKeyMatch = System.Text.RegularExpressions.Regex.Match(
                    itemsKeyRaw, @"^\{\{([\w.]+)\}\}$");
                var itemsKey    = itemsKeyMatch.Success ? itemsKeyMatch.Groups[1].Value : itemsKeyRaw;
                var outputKey   = NodeParameters.GetString(nodeDef.Parameters, "outputKey")    ?? "results";
                var loopItemKey = NodeParameters.GetString(nodeDef.Parameters, "loopItemKey")  ?? "__item__";
                var maxIter     = NodeParameters.GetInt(nodeDef.Parameters,    "maxIterations");

                var loopInput = new Dictionary<string, object?> { ["itemsKey"] = itemsKey };
                await using var loopScope = await NodeStepScope.StartAsync(nodeDef, loopInput, true, true, onStep);

                // Try typed get first; fall back to object and cast (handles List<T> → IEnumerable<object>)
                var rawItems = (data.Get<IEnumerable<object>>(itemsKey)
                    ?? data.Get<object>(itemsKey) as IEnumerable<object>
                    ?? (data.Get<object>(itemsKey) is System.Collections.IEnumerable ie
                        ? ie.Cast<object>() : null))?.ToList();
                if (rawItems is null)
                {
                    _logger.LogWarning("  ⚠ LoopNode '{Name}': key '{Key}' not found or empty — skipping loop.",
                        nodeDef.Name, itemsKey);
                    loopScope.Fail($"Key '{itemsKey}' not found or is not a collection — loop skipped.");
                    if (!routing.TryGetValue((currentId, "output"), out var loopSkipId)) break;
                    currentId = loopSkipId;
                    continue;
                }

                if (maxIter > 0 && rawItems.Count > maxIter)
                    rawItems = rawItems.Take(maxIter).ToList();

                var loopOutputs = new List<WorkflowData>(rawItems.Count);

                // Body entry is the node connected to the "body" output port in the main graph.
                // Body nodes live in the same flat graph; the walk terminates when their chain
                // has no further connections. Each iteration shares parent context: changes made
                // inside the body are propagated back so subsequent iterations and the post-loop
                // path can read them.
                var hasBodyPort = routing.TryGetValue((currentId, "body"), out var bodyEntryId);

                for (var loopIdx = 0; loopIdx < rawItems.Count; loopIdx++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var itemData = data.Clone()
                        .Set(loopItemKey, rawItems[loopIdx])
                        .Set("__loop_index__", loopIdx);

                    if (hasBodyPort)
                    {
                        // Signal the UI to re-anchor lastDoneNodeRef to the LoopNode so the
                        // LoopNode→firstBodyNode edge re-animates at the start of each iteration.
                        await onStep(new NodeStepEvent
                        {
                            EventType = "loop_iteration_start",
                            NodeId    = nodeDef.Id,
                            NodeName  = nodeDef.Name,
                            NodeType  = nodeDef.Type,
                        });

                        var (iterData, iterOk, iterErr, iterFailed) =
                            await WalkGraphAsync(bodyEntryId, itemData, context,
                                nodeMap, routing, definition, onStep);

                        if (!iterOk)
                        {
                            loopScope.Fail($"Iteration {loopIdx} failed: {iterErr}");
                            return (data, false,
                                $"LoopNode '{nodeDef.Name}' iteration {loopIdx} failed: {iterErr}",
                                iterFailed ?? nodeDef.Name);
                        }

                        // Write body changes back to parent context so subsequent iterations
                        // and post-loop nodes can read them.
                        data = iterData;
                        loopOutputs.Add(iterData);
                    }
                    else
                    {
                        loopOutputs.Add(itemData); // no body wired — collect item data as-is
                    }
                }

                data = data.Clone()
                    .Set(outputKey, loopOutputs)
                    .Set("loop_iteration_count", rawItems.Count);

                if (!string.IsNullOrEmpty(nodeDef.NodeId))
                {
                    data.Set($"{nodeDef.NodeId}.{outputKey}", loopOutputs);
                    data.Set($"{nodeDef.NodeId}.loop_iteration_count", rawItems.Count);
                }

                loopScope.Complete(new()
                {
                    [outputKey]              = loopOutputs,
                    ["loop_iteration_count"] = rawItems.Count
                });

                if (!routing.TryGetValue((currentId, "output"), out var loopNextId)) break;
                currentId = loopNextId;
                continue;
            }

            // ── Regular nodes ────────────────────────────────────────────────

            var node = CreateNode(nodeDef, data);
            if (node is null)
            {
                _logger.LogWarning("  ⚠ Unknown node type '{Type}' — skipping.", nodeDef.Type);
                if (!routing.TryGetValue((currentId, "output"), out var skipId))
                    break;
                currentId = skipId;
                continue;
            }

            var opts = BuildNodeOptions(nodeDef.ExecutionOptions);

            // ── Snapshot helpers scoped to declared DataIn / DataOut keys ────────
            var dataInKeys  = node.DataIn .Select(p => p.Key).ToHashSet();
            var dataOutKeys = node.DataOut.Select(p => p.Key).ToHashSet();
            var hasDataIn   = dataInKeys.Count  > 0;
            var hasDataOut  = dataOutKeys.Count > 0;

            var inputSnapshot = Snapshot(data);

            // When DataIn ports are declared, filter to those keys only.
            // When none are declared, send empty dict — UI will show "No DataIn configured".
            var filteredInput = hasDataIn
                ? inputSnapshot
                    .Where(kv => dataInKeys.Contains(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value)
                : new Dictionary<string, object?>();

            await using var scope = await NodeStepScope.StartAsync(
                nodeDef, filteredInput, hasDataIn, hasDataOut, onStep);

            // ── Validate required input ports — after node_start so UI shows the error ──
            var missingRequired = node.DataIn
                .Where(p => p.Required && !data.Keys.Contains(p.Key))
                .Select(p => p.Key)
                .ToList();

            if (missingRequired.Count > 0)
            {
                var missing = string.Join(", ", missingRequired);
                var msg = $"Missing required input(s): {missing}";
                _logger.LogError("  ✘ Node '{Name}' ({NodeId}): Missing required input(s): {Missing}",
                    nodeDef.Name, nodeDef.NodeId, missing);
                scope.Fail(msg);
                return (data, false, $"Node '{nodeDef.Name}' ({nodeDef.NodeId}): {msg}", nodeDef.Name);
            }

            WorkflowData resultData;
            string activatedPort;

            try
            {
                resultData    = await ExecuteWithOptionsAsync(node, data, context, opts);

                // ── Write scoped outputs: nodeId.key alongside flat key ───────
                if (!string.IsNullOrEmpty(nodeDef.NodeId))
                {
                    foreach (var port in node.DataOut)
                    {
                        if (resultData.Keys.Contains(port.Key))
                            resultData.Set($"{nodeDef.NodeId}.{port.Key}",
                                resultData.Get<object>(port.Key));
                    }
                }

                activatedPort = GetActivatedPort(nodeDef, resultData);

                var fullOutputSnapshot = Snapshot(resultData);

                // When DataOut ports are declared, filter to those keys only.
                // When none are declared, send empty dict — UI will show "No DataOut configured".
                var filteredOutput = hasDataOut
                    ? fullOutputSnapshot
                        .Where(kv => dataOutKeys.Contains(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value)
                    : new Dictionary<string, object?>();

                scope.Complete(filteredOutput);
            }
            catch (Exception ex) when (opts.ContinueOnError)
            {
                _logger.LogWarning(ex, "  ⚠ Node '{Name}' failed but ContinueOnError=true.", nodeDef.Name);
                scope.Fail(ex.Message);
                resultData    = opts.FallbackData ?? data;
                activatedPort = "output";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "  ✘ Node '{Name}' failed.", nodeDef.Name);
                scope.Fail(ex.Message);

                // Try the node's own "error" port first
                if (routing.TryGetValue((currentId, "error"), out var nodeErrorId))
                {
                    data = data.Clone()
                        .Set("error_message", ex.Message)
                        .Set("failed_node", nodeDef.Name);
                    currentId = nodeErrorId;
                    continue;
                }

                // Fall back to the workflow-level ErrorNode
                if (errorNodeId.HasValue &&
                    nodeMap.ContainsKey(errorNodeId.Value) &&
                    routing.TryGetValue((errorNodeId.Value, "output"), out var errorHandlerId))
                {
                    data = data.Clone()
                        .Set("error_message", ex.Message)
                        .Set("failed_node", nodeDef.Name);
                    currentId = errorHandlerId;
                    continue;
                }

                return (data, false, ex.Message, nodeDef.Name);
            }

            data = resultData;

            if (!routing.TryGetValue((currentId, activatedPort), out var nextNodeId))
            {
                // No outgoing connection on this port — end of path
                _logger.LogDebug(
                    "  No connection from port '{Port}' on '{Name}' — stopping.", activatedPort, nodeDef.Name);
                break;
            }

            currentId = nextNodeId;
        }

        if (steps >= maxSteps)
            return (data, false, "Workflow exceeded maximum step limit (possible infinite loop).", null);

        return (data, true, null, null);
    }

    // ─── Node Factory (reflection-based) ─────────────────────────────────────

    /// <summary>
    /// Dynamically creates a node instance by:
    ///   1. Resolving {{variable}} placeholders in all non-credential string parameters.
    ///   2. Looking up the node's Type from the core assembly registry.
    ///   3. Invoking the node's Dictionary&lt;string, object?&gt; constructor.
    /// No switch-case required — adding a new node only needs the constructor.
    /// </summary>
    private INode? CreateNode(NodeDefinition nodeDef, WorkflowData data)
    {
        if (!_nodeTypeRegistry.TryGetValue(nodeDef.Type, out var type))
        {
            _logger.LogWarning("No INode implementation found for type '{Type}'.", nodeDef.Type);
            return null;
        }

        var ctor = type.GetConstructor([typeof(Dictionary<string, object?>)]);
        if (ctor is null)
        {
            _logger.LogWarning("Node '{Type}' has no Dictionary<string, object?> constructor.", nodeDef.Type);
            return null;
        }

        // Pre-resolve {{variable}} placeholders; inject node name so constructors can read it.
        var resolvedParams = ResolveParameters(nodeDef.Parameters, data);
        resolvedParams["name"] = nodeDef.Name;

        try
        {
            return (INode)ctor.Invoke([resolvedParams]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to instantiate node '{Type}'.", nodeDef.Type);
            return null;
        }
    }

    /// <summary>
    /// Applies {{variable}} substitution to all string-valued parameters except credential keys.
    /// </summary>
    private static Dictionary<string, object?> ResolveParameters(
        Dictionary<string, object?> raw,
        WorkflowData data)
    {
        var result = new Dictionary<string, object?>(raw.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in raw)
        {
            if (_noResolveKeys.Contains(key))
            {
                result[key] = value; // never resolve credentials
                continue;
            }

            result[key] = value switch
            {
                string str                                           => ResolveVariables(str, data),
                JsonElement { ValueKind: JsonValueKind.String } je  => ResolveVariables(je.GetString() ?? "", data),
                _                                                    => value
            };
        }
        return result;
    }

    private static string ResolveVariables(string raw, WorkflowData data) =>
        System.Text.RegularExpressions.Regex.Replace(raw, @"\{\{([\w.]+)\}\}", m =>
        {
            var key     = m.Groups[1].Value;
            var dataDict = data.Keys.ToDictionary(k => k, data.Get<object>);
            var val      = NodeParameters.GetNestedValue(dataDict, key);
            return val is not null ? val.ToString()! : m.Value;
        });

    // ─── Execution with retry/timeout ─────────────────────────────────────────

    private async Task<WorkflowData> ExecuteWithOptionsAsync(
        INode node,
        WorkflowData data,
        WorkflowContext context,
        NodeOptions opts)
    {
        Exception? lastException = null;

        for (var attempt = 0; attempt <= opts.MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var backoff = TimeSpan.FromMilliseconds(
                    opts.RetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                _logger.LogInformation(
                    "  ↻ Retrying '{Name}' — attempt {Attempt}/{Max} (backoff {ms}ms)",
                    node.Name, attempt, opts.MaxRetries, (int)backoff.TotalMilliseconds);
                await Task.Delay(backoff, context.CancellationToken);
            }

            try
            {
                NodeResult result;

                if (opts.Timeout.HasValue)
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                        context.CancellationToken);
                    cts.CancelAfter(opts.Timeout.Value);
                    result = await node.ExecuteAsync(data, context);
                }
                else
                {
                    result = await node.ExecuteAsync(data, context);
                }

                if (result.IsSuccess) return result.Data;

                lastException = new InvalidOperationException(
                    result.ErrorMessage ?? $"Node '{node.Name}' returned a failure result.");
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        throw lastException!;
    }

    // ─── Port selection ───────────────────────────────────────────────────────

    private static string GetActivatedPort(NodeDefinition nodeDef, WorkflowData resultData)
    {
        return nodeDef.Type switch
        {
            "BranchNode" =>
                resultData.GetString("branch_selected_port") ?? "default",

            // ErrorRouteNode: read the route it wrote
            "ErrorRouteNode" =>
                resultData.GetString("error_route") ?? "success",

            _ => "output"
        };
    }

    // ─── WorkflowData snapshot helper ──────────────────────────────────────────

    private static Dictionary<string, object?> Snapshot(WorkflowData data)
        => data.Keys.ToDictionary(k => k, k => data.Get<object?>(k));

    // ─── Routing table ─────────────────────────────────────────────────────────

    private static Dictionary<(Guid nodeId, string port), Guid> BuildRouting(
        List<ConnectionDefinition> connections)
    {
        var table = new Dictionary<(Guid, string), Guid>();
        foreach (var c in connections)
        {
            var key = (c.SourceNodeId, c.SourcePort);
            if (!table.ContainsKey(key))
                table[key] = c.TargetNodeId;
        }
        return table;
    }

    // ─── NodeOptions from UI execution options ────────────────────────────────

    private static NodeOptions BuildNodeOptions(NodeExecutionOptions? opts)
    {
        if (opts is null) return NodeOptions.Default;

        var result = NodeOptions.Default;

        if (opts.MaxRetries > 0)
            result = result.AndRetry(opts.MaxRetries,
                TimeSpan.FromMilliseconds(opts.RetryDelayMs));

        if (opts.TimeoutMs.HasValue)
            result = result.AndTimeout(TimeSpan.FromMilliseconds(opts.TimeoutMs.Value));

        if (opts.ContinueOnError)
        {
            WorkflowData? fallback = null;
            if (opts.FallbackData?.Count > 0)
            {
                fallback = new WorkflowData();
                foreach (var (k, v) in opts.FallbackData)
                    fallback.Set(k, v);
            }
            result = result.AndContinueOnError(fallback);
        }

        return result;
    }

}
