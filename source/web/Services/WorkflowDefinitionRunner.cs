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

    public async Task<WorkflowRunResult> RunAsync(
        WorkflowDefinition definition,
        WorkflowData? initialData = null)
    {
        var startNode = definition.Nodes.FirstOrDefault(n => n.Type == "StartNode")
            ?? throw new InvalidOperationException(
                $"Workflow '{definition.Name}' has no Start node. " +
                "Drag a Start node onto the canvas and save before running.");

        var nodeMap   = definition.Nodes.ToDictionary(n => n.Id);
        var routing   = BuildRouting(definition.Connections);
        var context   = new WorkflowContext(definition.Name, _logger);

        // Seed workflow-level variables into context global state
        foreach (var (k, v) in definition.Variables)
            context.SetState(k, v);

        var data = initialData?.Clone() ?? new WorkflowData();

        _logger.LogInformation("▶ Running workflow '{Name}' from JSON definition", definition.Name);

        var (resultData, success, error, failedNode) = await WalkGraphAsync(
            startNode.Id, data, context, nodeMap, routing, definition);

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

    // ─── Graph Walker ─────────────────────────────────────────────────────────

    private async Task<(WorkflowData data, bool success, string? error, string? failedNode)>
        WalkGraphAsync(
            Guid startNodeId,
            WorkflowData data,
            WorkflowContext context,
            Dictionary<Guid, NodeDefinition> nodeMap,
            Dictionary<(Guid, string), Guid> routing,
            WorkflowDefinition definition)
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
                                    childNodeMap, childRouting, childWrapDef);

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

            // ── Regular nodes ────────────────────────────────────────────────

            var node = CreateNode(nodeDef, definition);
            if (node is null)
            {
                _logger.LogWarning("  ⚠ Unknown node type '{Type}' — skipping.", nodeDef.Type);
                if (!routing.TryGetValue((currentId, "output"), out var skipId))
                    break;
                currentId = skipId;
                continue;
            }

            var opts = BuildNodeOptions(nodeDef.ExecutionOptions);

            WorkflowData resultData;
            string activatedPort;

            try
            {
                resultData    = await ExecuteWithOptionsAsync(node, data, context, opts);
                activatedPort = GetActivatedPort(nodeDef, resultData);
            }
            catch (Exception ex) when (opts.ContinueOnError)
            {
                _logger.LogWarning(ex, "  ⚠ Node '{Name}' failed but ContinueOnError=true.", nodeDef.Name);
                resultData    = opts.FallbackData ?? data;
                activatedPort = "output";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "  ✘ Node '{Name}' failed.", nodeDef.Name);

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

    private INode? CreateNode(NodeDefinition nodeDef, WorkflowDefinition definition)
    {
        var p    = nodeDef.Parameters;
        var name = nodeDef.Name;

        return nodeDef.Type switch
        {
            // ── AI ────────────────────────────────────────────────────────────
            "LlmNode" => new LlmNode(name, BuildLlmConfig(p)),

            "PromptBuilderNode" => new PromptBuilderNode(
                name,
                promptTemplate : GetString(p, "promptTemplate") ?? "",
                systemTemplate : GetString(p, "systemTemplate")),

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

            // ── IO ────────────────────────────────────────────────────────────
            "HttpRequestNode" => new HttpRequestNode(name, new HttpRequestConfig
            {
                Method        = (GetString(p, "method") ?? "GET").ToUpperInvariant(),
                UrlTemplate   = GetString(p, "url") ?? GetString(p, "urlTemplate") ?? "",
                Headers       = GetStringDict(p, "headers") ?? new Dictionary<string, string>(),
                ThrowOnError  = GetBool(p, "throwOnError", true),
                Timeout       = TimeSpan.FromMilliseconds(GetDouble(p, "timeoutMs", 30_000))
            }),

            _ => null
        };
    }

    // ─── Helpers: node config builders ────────────────────────────────────────

    private static LlmConfig BuildLlmConfig(Dictionary<string, object?> p)
    {
        var provider   = GetString(p, "provider") ?? "openai";
        var model      = GetString(p, "model")    ?? "gpt-4o";
        var apiKey     = GetString(p, "apiKey")   ?? "";
        var apiUrl     = GetString(p, "apiUrl");
        var systemPmt  = GetString(p, "systemPrompt");
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
