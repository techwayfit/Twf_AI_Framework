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

    /// <inheritdoc/>
    public override string IdPrefix => "cond";

    /// <inheritdoc/>
    // Inputs are the data keys referenced by predicates — not statically knowable in all cases.
    public override IReadOnlyList<NodePort> InputPorts => [];

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> OutputPorts =>
        _conditions.Select(c => new NodePort(c.Key, typeof(bool), Description: "Condition result flag"))
                   .ToList<NodePort>();

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


