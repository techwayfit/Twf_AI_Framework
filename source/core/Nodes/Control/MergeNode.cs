using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Control;

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

    /// <inheritdoc/>
    public override string IdPrefix => "merge";

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> InputPorts =>
        _sourceKeys.Select(k => new NodePort(k, typeof(string), Required: false, "Source key to merge"))
                   .ToList<NodePort>();

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> OutputPorts =>
    [
        new(_outputKey, typeof(string), Description: "Merged result")
    ];

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