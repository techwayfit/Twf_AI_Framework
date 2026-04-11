using System.Text.Json;
using Microsoft.Extensions.Logging;
using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using TwfAiFramework.Nodes.AI;
using TwfAiFramework.Nodes.Control;
using TwfAiFramework.Nodes.Data;
using TwfAiFramework.Nodes.IO;
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
/// Node execution options (retry, timeout, continue-on-error) are honoured per node.
/// </summary>
public sealed class WorkflowDefinitionRunner
{
    private readonly ILogger<WorkflowDefinitionRunner> _logger;

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

        // Resolve {{variable}} placeholders against current WorkflowData.
        string? Resolve(string? raw, WorkflowData d) =>
            raw is null ? null
            : System.Text.RegularExpressions.Regex.Replace(raw, @"\{\{(\w+)\}\}", m =>
            {
                var key = m.Groups[1].Value;
                return d.TryGet<object>(key, out var val) && val is not null ? val.ToString()! : m.Value;
            });

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
                // Pass-through — just follow the output connection
                if (!routing.TryGetValue((currentId, "output"), out var nextId))
                    break;
                currentId = nextId;
                continue;
            }

            if (nodeDef.Type is "EndNode")
            {
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
                var subIdStr = GetString(nodeDef.Parameters, "subWorkflowId");
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
                var itemsKey    = Resolve(GetString(nodeDef.Parameters, "itemsKey") ?? "items", data);
                var outputKey   = GetString(nodeDef.Parameters, "outputKey")            ?? "results";
                var loopItemKey = GetString(nodeDef.Parameters, "loopItemKey")          ?? "__item__";
                var maxIter     = GetInt(nodeDef.Parameters,    "maxIterations");

                var rawItems = data.Get<IEnumerable<object>>(itemsKey!)?.ToList();
                if (rawItems is null)
                {
                    _logger.LogWarning("  ⚠ LoopNode '{Name}': key '{Key}' not found or empty — skipping loop.",
                        nodeDef.Name, itemsKey);
                    if (!routing.TryGetValue((currentId, "output"), out var loopSkipId)) break;
                    currentId = loopSkipId;
                    continue;
                }

                if (maxIter > 0 && rawItems.Count > maxIter)
                    rawItems = rawItems.Take(maxIter).ToList();

                var loopOutputs = new List<WorkflowData>(rawItems.Count);
                var loopBodyNodes   = nodeDef.SubWorkflow?.Nodes        ?? [];
                var loopBodyConns   = nodeDef.SubWorkflow?.Connections   ?? [];
                var loopBodyStart   = loopBodyNodes.FirstOrDefault(n => n.Type == "StartNode");

                for (var loopIdx = 0; loopIdx < rawItems.Count; loopIdx++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var itemData = data.Clone()
                        .Set(loopItemKey, rawItems[loopIdx])
                        .Set("__loop_index__", loopIdx);

                    if (loopBodyStart is not null)
                    {
                        var bodyNodeMap  = loopBodyNodes.ToDictionary(n => n.Id);
                        var bodyRouting  = BuildRouting(loopBodyConns);
                        var bodyWrapDef  = new WorkflowDefinition
                        {
                            Id = nodeDef.Id, Name = $"{nodeDef.Name}/Body",
                            Nodes = loopBodyNodes, Connections = loopBodyConns,
                            SubWorkflows = definition.SubWorkflows
                        };

                        var (iterData, iterOk, iterErr, iterFailed) =
                            await WalkGraphAsync(loopBodyStart.Id, itemData, context,
                                bodyNodeMap, bodyRouting, bodyWrapDef, onStep);

                        if (!iterOk)
                            return (data, false,
                                $"LoopNode '{nodeDef.Name}' iteration {loopIdx} failed: {iterErr}",
                                iterFailed ?? nodeDef.Name);

                        loopOutputs.Add(iterData);
                    }
                    else
                    {
                        loopOutputs.Add(itemData); // no body — collect item data as-is
                    }
                }

                data = data.Clone()
                    .Set(outputKey!, loopOutputs)
                    .Set("loop_iteration_count", rawItems.Count);

                if (!string.IsNullOrEmpty(nodeDef.NodeId))
                {
                    data.Set($"{nodeDef.NodeId}.{outputKey}", loopOutputs);
                    data.Set($"{nodeDef.NodeId}.loop_iteration_count", rawItems.Count);
                }

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

            // ── Validate required input ports before executing ────────────────
            var missingRequired = node.DataIn
                .Where(p => p.Required && !data.Keys.Contains(p.Key))
                .Select(p => p.Key)
                .ToList();

            if (missingRequired.Count > 0)
            {
                var msg = $"Node '{nodeDef.Name}' ({nodeDef.NodeId}) is missing required input(s): " +
                          string.Join(", ", missingRequired);
                _logger.LogError("  ✘ {Message}", msg);
                return (data, false, msg, nodeDef.Name);
            }

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

            // ── Notify UI: node is starting ───────────────────────────────────
            await onStep(new NodeStepEvent
            {
                EventType       = "node_start",
                NodeId          = nodeDef.Id,
                NodeName        = nodeDef.Name,
                NodeType        = nodeDef.Type,
                NodeRefId       = nodeDef.NodeId,
                InputData       = filteredInput,
                OutputData      = new Dictionary<string, object?>(),
                DataInConfigured  = hasDataIn,
                DataOutConfigured = hasDataOut,
                Timestamp       = DateTimeOffset.UtcNow
            });

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

                // ── Notify UI: node completed ─────────────────────────────────
                await onStep(new NodeStepEvent
                {
                    EventType         = "node_done",
                    NodeId            = nodeDef.Id,
                    NodeRefId         = nodeDef.NodeId,
                    NodeName          = nodeDef.Name,
                    NodeType          = nodeDef.Type,
                    InputData         = filteredInput,
                    OutputData        = filteredOutput,
                    DataInConfigured  = hasDataIn,
                    DataOutConfigured = hasDataOut,
                    Timestamp         = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex) when (opts.ContinueOnError)
            {
                _logger.LogWarning(ex, "  ⚠ Node '{Name}' failed but ContinueOnError=true.", nodeDef.Name);
                await onStep(new NodeStepEvent
                {
                    EventType         = "node_error",
                    NodeId            = nodeDef.Id,
                    NodeRefId         = nodeDef.NodeId,
                    NodeName          = nodeDef.Name,
                    NodeType          = nodeDef.Type,
                    InputData         = filteredInput,
                    OutputData        = new Dictionary<string, object?>(),
                    DataInConfigured  = hasDataIn,
                    DataOutConfigured = hasDataOut,
                    ErrorMessage      = ex.Message,
                    Timestamp         = DateTimeOffset.UtcNow
                });
                resultData    = opts.FallbackData ?? data;
                activatedPort = "output";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "  ✘ Node '{Name}' failed.", nodeDef.Name);
                await onStep(new NodeStepEvent
                {
                    EventType         = "node_error",
                    NodeId            = nodeDef.Id,
                    NodeRefId         = nodeDef.NodeId,
                    NodeName          = nodeDef.Name,
                    NodeType          = nodeDef.Type,
                    InputData         = filteredInput,
                    OutputData        = new Dictionary<string, object?>(),
                    DataInConfigured  = hasDataIn,
                    DataOutConfigured = hasDataOut,
                    ErrorMessage      = ex.Message,
                    Timestamp    = DateTimeOffset.UtcNow
                });

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

    // ─── Node Factory ─────────────────────────────────────────────────────────

    private INode? CreateNode(NodeDefinition nodeDef, WorkflowData data)
    {
        var p    = nodeDef.Parameters;
        var name = nodeDef.Name;

        // Resolve {{variable}} placeholders in a parameter string against current WorkflowData.
        // Intentionally excluded: apiKey and other sensitive credential fields.
        string? Resolve(string? raw) =>
            raw is null ? null
            : System.Text.RegularExpressions.Regex.Replace(raw, @"\{\{(\w+)\}\}", m =>
            {
                var key = m.Groups[1].Value;
                return data.TryGet<object>(key, out var val) && val is not null
                    ? val.ToString()!
                    : m.Value; // leave unreplaced if not found
            });

        return nodeDef.Type switch
        {
            // ── AI ────────────────────────────────────────────────────────────
            "LlmNode" => new LlmNode(name, BuildLlmConfig(p, Resolve)),

            "PromptBuilderNode" => new PromptBuilderNode(
                name,
                promptTemplate : Resolve(GetString(p, "promptTemplate")) ?? "",
                systemTemplate : Resolve(GetString(p, "systemTemplate"))),

            "EmbeddingNode" => new EmbeddingNode(name, new EmbeddingConfig
            {
                Model       = GetString(p, "model") ?? "text-embedding-3-small",
                ApiKey      = GetString(p, "apiKey") ?? "",
                ApiEndpoint = GetString(p, "apiUrl")
                              ?? "https://api.openai.com/v1/embeddings"
            }),

            "OutputParserNode" => new OutputParserNode(
                name : name,
                fieldMapping : GetStringDict(p, "fieldMapping"),
                strict       : GetBool(p, "strict")),

            // ── Control ────────────────────────────────────────────────────────
            // Condition predicates are code-side; in JSON mode, downstream branch
            // nodes must read the flags already present in WorkflowData.
            // Pass WorkflowData through unchanged so routing can read any flags set
            // by a prior Condition node in the pipeline.
            "ConditionNode" => new TransformNode(name, data => data.Clone()),

            "BranchNode" => new BranchNode(
                name         : name,
                valueKey     : GetString(p, "valueKey") ?? "",
                case1Value   : GetString(p, "case1Value"),
                case2Value   : GetString(p, "case2Value"),
                case3Value   : GetString(p, "case3Value"),
                caseSensitive: GetBool(p, "caseSensitive")),

            "DelayNode" => new DelayNode(
                TimeSpan.FromMilliseconds(GetDouble(p, "durationMs", 1000)),
                GetString(p, "reason")),

            "MergeNode" => new MergeNode(
                name      : name,
                outputKey : GetString(p, "outputKey") ?? "merged",
                separator : GetString(p, "separator") ?? "\n",
                sourceKeys: GetStringList(p, "sourceKeys")?.ToArray() ?? []),

            "LogNode" => new LogNode(
                label     : name,
                keysToLog : GetStringList(p, "keysToLog")?.ToArray()),

            // ── Data ──────────────────────────────────────────────────────────
            "TransformNode" => BuildTransformNode(name, p),

            "DataMapperNode" => new DataMapperNode(
                name           : name,
                mappings       : GetStringDict(p, "mappings") ?? new Dictionary<string, string>(),
                throwOnMissing : GetBool(p, "throwOnMissing"),
                removeUnmapped : GetBool(p, "removeUnmapped")),

            "FilterNode" => BuildFilterNode(name, p),

            "ChunkTextNode" => new ChunkTextNode(new ChunkConfig
            {
                ChunkSize = GetInt(p, "chunkSize", 500),
                Overlap   = GetInt(p, "overlap", 50),
                Strategy  = Enum.TryParse<ChunkStrategy>(
                    GetString(p, "strategy"), true, out var strat)
                    ? strat : ChunkStrategy.Character
            }),

            "MemoryNode" => GetString(p, "mode")?.Equals("Write", StringComparison.OrdinalIgnoreCase) == true
                ? MemoryNode.Write((GetStringList(p, "keys") ?? []).ToArray())
                : MemoryNode.Read((GetStringList(p, "keys") ?? []).ToArray()),

            "SetVariableNode" => new SetVariableNode(
                name,
                (GetStringDict(p, "assignments") ?? new Dictionary<string, string>())
                    .ToDictionary(kv => kv.Key, kv => (object?)Resolve(kv.Value))),

            // ── IO ────────────────────────────────────────────────────────────
            "HttpRequestNode" => new HttpRequestNode(name, new HttpRequestConfig
            {
                Method        = (GetString(p, "method") ?? "GET").ToUpperInvariant(),
                UrlTemplate   = Resolve(GetString(p, "url") ?? GetString(p, "urlTemplate")) ?? "",
                Headers       = GetStringDict(p, "headers") ?? new Dictionary<string, string>(),
                ThrowOnError  = GetBool(p, "throwOnError", true),
                Timeout       = TimeSpan.FromMilliseconds(GetDouble(p, "timeoutMs", 30_000))
            }),

            "FileReadNode"  => new FileReaderNode(Resolve(GetString(p, "filePath"))),
            "FileWriteNode" => new FileWriterNode(
                outputPath : Resolve(GetString(p, "filePath")) ?? "output.txt",
                dataKey    : GetString(p, "contentKey") ?? "llm_response"),

            // ── Control (routable) ────────────────────────────────────────────
            "ErrorRouteNode" => new ErrorRouteNode(
                name                 : name,
                errorMessageKey      : GetString(p, "errorMessageKey")  ?? "error_message",
                statusCodeKey        : GetString(p, "statusCodeKey")    ?? "http_status_code",
                errorStatusThreshold : GetInt(p,    "errorStatusThreshold", 400)),

            _ => null
        };
    }

    // ─── Helpers: node config builders ────────────────────────────────────────

    private static LlmConfig BuildLlmConfig(Dictionary<string, object?> p, Func<string?, string?> resolve)
    {
        var provider   = GetString(p, "provider") ?? "openai";
        var model      = GetString(p, "model")    ?? "gpt-4o";
        var apiKey     = GetString(p, "apiKey")   ?? ""; // credentials are never variable-substituted
        var apiUrl     = GetString(p, "apiUrl");
        var systemPmt  = resolve(GetString(p, "systemPrompt"));
        var temp       = (float)GetDouble(p, "temperature", 0.7);
        var maxTokens  = GetInt(p, "maxTokens", 1000);
        var history    = GetBool(p, "maintainHistory");

        return provider.ToLowerInvariant() switch
        {
            "anthropic" => LlmConfig.Anthropic(apiKey, model) with
            {
                DefaultSystemPrompt = systemPmt,
                Temperature         = temp,
                MaxTokens           = maxTokens,
                MaintainHistory     = history
            },
            "ollama" => LlmConfig.Ollama(model, apiUrl ?? "http://localhost:11434") with
            {
                DefaultSystemPrompt = systemPmt,
                Temperature         = temp,
                MaxTokens           = maxTokens
            },
            _ => LlmConfig.OpenAI(apiKey, model) with
            {
                ApiEndpoint         = apiUrl ?? "https://api.openai.com/v1/chat/completions",
                DefaultSystemPrompt = systemPmt,
                Temperature         = temp,
                MaxTokens           = maxTokens,
                MaintainHistory     = history
            }
        };
    }

    private static FilterNode BuildFilterNode(string name, Dictionary<string, object?> p)
    {
        var node = new FilterNode(name, throwOnFail: GetBool(p, "throwOnFail", true));

        var requireKey = GetString(p, "requireKey");
        if (!string.IsNullOrEmpty(requireKey))
            node.RequireNonEmpty(requireKey);

        var maxLengthKey  = GetString(p, "maxLengthKey");
        var maxLengthVal  = GetInt(p, "maxLength", 0);
        if (!string.IsNullOrEmpty(maxLengthKey) && maxLengthVal > 0)
            node.MaxLength(maxLengthKey, maxLengthVal);

        return node;
    }

    private static TransformNode BuildTransformNode(string name, Dictionary<string, object?> p)
    {
        var preset  = GetString(p, "preset");
        var fromKey = GetString(p, "fromKey") ?? "";
        var toKey   = GetString(p, "toKey")   ?? "";
        var sep     = GetString(p, "separator") ?? " ";
        var keys    = GetStringList(p, "keys");

        return preset?.ToLowerInvariant() switch
        {
            "rename"        => TransformNode.Rename(fromKey, toKey),
            "selectkey"     => TransformNode.SelectKey(fromKey, toKey),
            "concatstrings" => TransformNode.ConcatStrings(toKey, sep,
                                   keys?.ToArray() ?? []),
            _ => new TransformNode(name, data => data.Clone())   // pass-through
        };
    }

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

    // ─── Parameter extraction helpers ─────────────────────────────────────────
    // Parameters arrive from JSON deserialization as JsonElement or boxed primitives.

    private static string? GetString(Dictionary<string, object?> p, string key, string? def = null)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is JsonElement je) return je.GetString() ?? def;
        return raw.ToString() ?? def;
    }

    private static bool GetBool(Dictionary<string, object?> p, string key, bool def = false)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is bool b) return b;
        if (raw is JsonElement je && je.ValueKind == JsonValueKind.True)  return true;
        if (raw is JsonElement je2 && je2.ValueKind == JsonValueKind.False) return false;
        return bool.TryParse(raw.ToString(), out var parsed) ? parsed : def;
    }

    private static int GetInt(Dictionary<string, object?> p, string key, int def = 0)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is JsonElement je && je.TryGetInt32(out var v)) return v;
        if (raw is int i) return i;
        return int.TryParse(raw.ToString(), out var parsed) ? parsed : def;
    }

    private static double GetDouble(Dictionary<string, object?> p, string key, double def = 0)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is JsonElement je && je.TryGetDouble(out var v)) return v;
        if (raw is double d) return d;
        if (raw is float f)  return f;
        return double.TryParse(raw.ToString(), out var parsed) ? parsed : def;
    }

    private static Dictionary<string, string>? GetStringDict(Dictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return null;

        if (raw is JsonElement je && je.ValueKind == JsonValueKind.Object)
        {
            return je.EnumerateObject()
                     .Where(prop => prop.Value.ValueKind == JsonValueKind.String)
                     .ToDictionary(prop => prop.Name, prop => prop.Value.GetString()!);
        }

        if (raw is Dictionary<string, string> dict) return dict;
        if (raw is Dictionary<string, object?> objDict)
            return objDict.Where(kv => kv.Value is not null)
                          .ToDictionary(kv => kv.Key, kv => kv.Value!.ToString()!);

        return null;
    }

    private static List<string>? GetStringList(Dictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return null;

        if (raw is JsonElement je && je.ValueKind == JsonValueKind.Array)
            return je.EnumerateArray()
                     .Where(e => e.ValueKind == JsonValueKind.String)
                     .Select(e => e.GetString()!)
                     .ToList();

        if (raw is List<string> list)        return list;
        if (raw is string[] arr)             return arr.ToList();
        if (raw is IEnumerable<string> ienum) return ienum.ToList();

        return null;
    }
}
