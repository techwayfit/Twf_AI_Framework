using Microsoft.Extensions.Logging;
using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Control;

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