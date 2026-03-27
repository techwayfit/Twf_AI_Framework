using Microsoft.Extensions.Logging;
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
// BranchNode — Switch/Case router that writes selected route metadata
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Evaluates a value key and selects one route from case1/case2/case3/default.
/// Writes explicit routing keys into WorkflowData so downstream branching can
/// be driven via Workflow.Branch(...) predicates.
///
/// Writes:
///   - "branch_selected_port" : "case1" | "case2" | "case3" | "default"
///   - "branch_input_value"   : string representation of input value
///   - "branch_selected_value": matched case value (if matched), else null
///   - "branch_case1" / "branch_case2" / "branch_case3" / "branch_default" : bool flags
/// </summary>
public sealed class BranchNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description =>
        $"Routes by '{_valueKey}' using switch/case matching";

    private readonly string _valueKey;
    private readonly string? _case1Value;
    private readonly string? _case2Value;
    private readonly string? _case3Value;
    private readonly bool _caseSensitive;
    private readonly StringComparison _comparison;

    public BranchNode(
        string name,
        string valueKey,
        string? case1Value = null,
        string? case2Value = null,
        string? case3Value = null,
        bool caseSensitive = false)
    {
        Name = name;
        _valueKey = valueKey;
        _case1Value = case1Value;
        _case2Value = case2Value;
        _case3Value = case3Value;
        _caseSensitive = caseSensitive;
        _comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var inputValue = input.Get<object>(_valueKey)?.ToString();
        var selectedPort = ResolveRoute(inputValue);
        var selectedValue = GetCaseValue(selectedPort);

        var output = input.Clone()
            .Set("branch_selected_port", selectedPort)
            .Set("branch_input_value", inputValue ?? string.Empty)
            .Set("branch_selected_value", selectedValue)
            .Set("branch_case1", selectedPort == "case1")
            .Set("branch_case2", selectedPort == "case2")
            .Set("branch_case3", selectedPort == "case3")
            .Set("branch_default", selectedPort == "default");

        nodeCtx.Log(
            $"Branch route selected: {selectedPort} (valueKey='{_valueKey}', input='{inputValue ?? "(null)"}', caseSensitive={_caseSensitive})");
        nodeCtx.SetMetadata("selected_port", selectedPort);
        nodeCtx.SetMetadata("value_key", _valueKey);

        return Task.FromResult(output);
    }

    public static BranchNode Switch(
        string name,
        string valueKey,
        string? case1Value = null,
        string? case2Value = null,
        string? case3Value = null,
        bool caseSensitive = false)
    {
        return new BranchNode(
            name,
            valueKey,
            case1Value,
            case2Value,
            case3Value,
            caseSensitive);
    }

    private string ResolveRoute(string? inputValue)
    {
        if (IsMatch(inputValue, _case1Value)) return "case1";
        if (IsMatch(inputValue, _case2Value)) return "case2";
        if (IsMatch(inputValue, _case3Value)) return "case3";
        return "default";
    }

    private string? GetCaseValue(string selectedPort) => selectedPort switch
    {
        "case1" => _case1Value,
        "case2" => _case2Value,
        "case3" => _case3Value,
        _ => null
    };

    private bool IsMatch(string? inputValue, string? expectedValue)
    {
        if (string.IsNullOrWhiteSpace(expectedValue) || string.IsNullOrWhiteSpace(inputValue))
            return false;

        return inputValue.Equals(expectedValue, _comparison);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ErrorRouteNode — Routes data to success/error based on error indicators
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Detects whether current WorkflowData indicates an error and writes explicit
/// routing flags for downstream conditional branching.
///
/// Error detection rules:
///   1. Non-empty error message in <see cref="_errorMessageKey"/>
///   2. HTTP status code in <see cref="_statusCodeKey"/> >= <see cref="_errorStatusThreshold"/>
///
/// Writes:
///   - "error_route"   : "error" | "success"
///   - "route_error"   : bool
///   - "route_success" : bool
///   - "routed_error_message" : optional synthesized message when route is error
/// </summary>
public sealed class ErrorRouteNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description =>
        $"Routes by error indicators (message='{_errorMessageKey}', status='{_statusCodeKey}')";

    private readonly string _errorMessageKey;
    private readonly string _statusCodeKey;
    private readonly int _errorStatusThreshold;

    public ErrorRouteNode(
        string name = "ErrorRoute",
        string errorMessageKey = "error_message",
        string statusCodeKey = "http_status_code",
        int errorStatusThreshold = 400)
    {
        Name = name;
        _errorMessageKey = errorMessageKey;
        _statusCodeKey = statusCodeKey;
        _errorStatusThreshold = errorStatusThreshold;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var errorMessage = input.GetString(_errorMessageKey);
        var hasErrorMessage = !string.IsNullOrWhiteSpace(errorMessage);

        var hasStatusCode = TryGetStatusCode(input, _statusCodeKey, out var statusCode);
        var isErrorStatus = hasStatusCode && statusCode >= _errorStatusThreshold;

        var isError = hasErrorMessage || isErrorStatus;
        var route = isError ? "error" : "success";

        if (isError && string.IsNullOrWhiteSpace(errorMessage) && isErrorStatus)
            errorMessage = $"HTTP status {statusCode} (threshold {_errorStatusThreshold})";

        nodeCtx.Log(
            $"Error route={route} (hasErrorMessage={hasErrorMessage}, hasStatusCode={hasStatusCode}, statusCode={(hasStatusCode ? statusCode : 0)})");
        nodeCtx.SetMetadata("route", route);
        nodeCtx.SetMetadata("status_code", hasStatusCode ? statusCode : -1);
        nodeCtx.SetMetadata("threshold", _errorStatusThreshold);

        return Task.FromResult(input.Clone()
            .Set("error_route", route)
            .Set("route_error", isError)
            .Set("route_success", !isError)
            .Set("routed_error_message", errorMessage));
    }

    public static ErrorRouteNode HttpStatusAware(
        string name = "ErrorRoute",
        string statusCodeKey = "http_status_code",
        int errorStatusThreshold = 400)
    {
        return new ErrorRouteNode(
            name: name,
            errorMessageKey: "error_message",
            statusCodeKey: statusCodeKey,
            errorStatusThreshold: errorStatusThreshold);
    }

    private static bool TryGetStatusCode(WorkflowData input, string key, out int statusCode)
    {
        statusCode = 0;
        if (!input.Has(key))
            return false;

        var raw = input.Get<object>(key);
        if (raw is null)
            return false;

        if (raw is int intValue)
        {
            statusCode = intValue;
            return true;
        }

        return int.TryParse(raw.ToString(), out statusCode);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// TryCatchNode — Container node with embedded try/catch workflows
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Executes an embedded try workflow and, if it fails, executes an embedded catch workflow.
/// This behaves like a containerized try/catch block for workflow composition.
///
/// On try failure, catch workflow input is enriched with:
///   - "caught_error_message"
///   - "caught_failed_node"
///   - "caught_exception_type"
///
/// Writes:
///   - "try_catch_route" : "success" | "catch"
///   - "try_success"     : bool
///   - "try_error"       : bool
/// </summary>
public sealed class TryCatchNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description => "Executes try workflow and catches failures with fallback workflow";

    private readonly Workflow _tryWorkflow;
    private readonly Workflow? _catchWorkflow;

    public TryCatchNode(
        string name,
        Action<Workflow> tryBuilder,
        Action<Workflow>? catchBuilder = null)
    {
        Name = name;

        _tryWorkflow = Workflow.Create($"{name}/Try");
        tryBuilder(_tryWorkflow);

        if (catchBuilder is not null)
        {
            _catchWorkflow = Workflow.Create($"{name}/Catch");
            catchBuilder(_catchWorkflow);
        }
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var tryResult = await _tryWorkflow.RunAsync(input.Clone(), context);
        if (tryResult.IsSuccess)
        {
            nodeCtx.Log("Try workflow completed successfully");
            return tryResult.Data.Clone()
                .Set("try_catch_route", "success")
                .Set("try_success", true)
                .Set("try_error", false);
        }

        nodeCtx.Log($"Try workflow failed at '{tryResult.FailedNodeName}': {tryResult.ErrorMessage}");

        var catchInput = tryResult.Data.Clone()
            .Set("caught_error_message", tryResult.ErrorMessage ?? string.Empty)
            .Set("caught_failed_node", tryResult.FailedNodeName ?? string.Empty)
            .Set("caught_exception_type", tryResult.Exception?.GetType().Name ?? string.Empty);

        if (_catchWorkflow is null)
        {
            throw new InvalidOperationException(
                $"[{Name}] Try workflow failed and no catch workflow is configured. " +
                $"Failed node: {tryResult.FailedNodeName}, Error: {tryResult.ErrorMessage}");
        }

        var catchResult = await _catchWorkflow.RunAsync(catchInput, context);
        if (catchResult.IsSuccess)
        {
            nodeCtx.Log("Catch workflow handled the failure successfully");
            return catchResult.Data.Clone()
                .Set("try_catch_route", "catch")
                .Set("try_success", false)
                .Set("try_error", true);
        }

        throw new InvalidOperationException(
            $"[{Name}] Catch workflow failed while handling try failure. " +
            $"Failed node: {catchResult.FailedNodeName}, Error: {catchResult.ErrorMessage}");
    }

    public static TryCatchNode Create(
        string name,
        Action<Workflow> tryBuilder,
        Action<Workflow>? catchBuilder = null)
    {
        return new TryCatchNode(name, tryBuilder, catchBuilder);
    }
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
