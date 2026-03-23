using TwfAiFramework.Core;
using TwfAiFramework.Nodes;

namespace TwfAiFramework.Nodes.Control;

// ═══════════════════════════════════════════════════════════════════════════════
// ConditionNode — Adds conditional flags to WorkflowData
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Evaluates conditions and writes boolean results to WorkflowData.
/// Use with Workflow.Branch() for conditional routing.
///
/// Example:
///   new ConditionNode("CheckSentiment",
///       ("is_positive", data => data.GetString("sentiment") == "positive"),
///       ("needs_escalation", data => data.Get&lt;int&gt;("anger_score") &gt; 7))
/// </summary>
public sealed class ConditionNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description =>
        $"Evaluates {_conditions.Count} condition(s) and writes results to WorkflowData";

    private readonly List<(string Key, Func<WorkflowData, bool> Predicate)> _conditions;

    public ConditionNode(string name,
        params (string Key, Func<WorkflowData, bool> Predicate)[] conditions)
    {
        Name = name;
        _conditions = conditions.ToList();
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var output = input.Clone();

        foreach (var (key, predicate) in _conditions)
        {
            var result = predicate(input);
            output.Set(key, result);
            nodeCtx.Log($"Condition '{key}' = {result}");
        }

        return Task.FromResult(output);
    }

    // ─── Common condition factories ───────────────────────────────────────────

    public static ConditionNode HasKey(string outputKey, string checkKey) =>
        new(outputKey, (outputKey, data => data.Has(checkKey)));

    public static ConditionNode StringEquals(
        string outputKey, string dataKey, string expectedValue) =>
        new(outputKey, (outputKey, data =>
            data.GetString(dataKey)?.Equals(expectedValue, StringComparison.OrdinalIgnoreCase) == true));

    public static ConditionNode LengthExceeds(
        string outputKey, string dataKey, int maxLength) =>
        new(outputKey, (outputKey, data =>
            (data.GetString(dataKey)?.Length ?? 0) > maxLength));
}

// ═══════════════════════════════════════════════════════════════════════════════
// DelayNode — Insert a delay between nodes
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Introduces a configurable delay in the pipeline.
/// Useful for rate limiting, waiting for async processes, or debugging.
/// </summary>
public sealed class DelayNode : BaseNode
{
    public override string Name => $"Delay:{_delay.TotalMilliseconds}ms";
    public override string Category => "Control";
    public override string Description =>
        $"Waits for {_delay.TotalMilliseconds}ms before passing data to next node";

    private readonly TimeSpan _delay;
    private readonly string? _reason;

    public DelayNode(TimeSpan delay, string? reason = null)
    {
        _delay = delay;
        _reason = reason;
    }

    public static DelayNode Milliseconds(int ms, string? reason = null) =>
        new(TimeSpan.FromMilliseconds(ms), reason);

    public static DelayNode Seconds(int seconds, string? reason = null) =>
        new(TimeSpan.FromSeconds(seconds), reason);

    public static DelayNode RateLimitDelay(int requestsPerMinute) =>
        Milliseconds((int)(60000.0 / requestsPerMinute), "Rate limit");

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        nodeCtx.Log($"Waiting {_delay.TotalMilliseconds}ms" +
                    (_reason is not null ? $" ({_reason})" : ""));

        await Task.Delay(_delay, context.CancellationToken);
        return input;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// MergeNode — Merge multiple data keys into a single value
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Merges multiple WorkflowData keys into a single aggregated value.
/// Useful for combining results from parallel branches or multiple sources.
/// </summary>
public sealed class MergeNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description => "Merges multiple keys into a single output";

    private readonly string[] _sourceKeys;
    private readonly string _outputKey;
    private readonly string _separator;

    public MergeNode(string name, string outputKey, string separator = "\n",
        params string[] sourceKeys)
    {
        Name = name;
        _outputKey = outputKey;
        _separator = separator;
        _sourceKeys = sourceKeys;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var parts = _sourceKeys
            .Select(k => input.Get<object>(k)?.ToString())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        var merged = string.Join(_separator, parts);
        nodeCtx.Log($"Merged {parts.Count}/{_sourceKeys.Length} keys into '{_outputKey}'");

        return Task.FromResult(input.Clone().Set(_outputKey, merged));
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// LogNode — Explicit logging checkpoint
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Logs the current WorkflowData state at a specific point in the pipeline.
/// Useful for debugging and monitoring.
/// </summary>
public sealed class LogNode : BaseNode
{
    public override string Name => $"Log:{_label}";
    public override string Category => "Control";
    public override string Description =>
        $"Logs WorkflowData state at checkpoint '{_label}'";

    private readonly string _label;
    private readonly string[]? _keysToLog;
    private readonly Microsoft.Extensions.Logging.LogLevel _level;

    public LogNode(string label,
        string[]? keysToLog = null,
        Microsoft.Extensions.Logging.LogLevel level =
            Microsoft.Extensions.Logging.LogLevel.Information)
    {
        _label = label;
        _keysToLog = keysToLog;
        _level = level;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var keys = _keysToLog ?? input.Keys.ToArray();

        context.Logger.Log(_level, "📍 Checkpoint [{Label}]", _label);

        foreach (var key in keys)
        {
            var val = input.Get<object>(key);
            var display = val?.ToString() ?? "(null)";
            if (display.Length > 200) display = display[..200] + "...";
            context.Logger.Log(_level, "   {Key}: {Value}", key, display);
        }

        return Task.FromResult(input);
    }

    public static LogNode All(string label) => new(label);
    public static LogNode Keys(string label, params string[] keys) => new(label, keys);
}
