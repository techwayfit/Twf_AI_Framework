using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Control;

// ═══════════════════════════════════════════════════════════════════════════════
// LoopNode — ForEach iteration over a collection in WorkflowData
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Iterates over each item in a WorkflowData collection, runs an embedded body
/// workflow per item, and collects per-item results into an output key.
///
/// Reads from WorkflowData:
///   - <see cref="_itemsKey"/> : IEnumerable of items to loop over (any element type)
///
/// Writes to WorkflowData:
///   - <see cref="_outputKey"/>   : List&lt;WorkflowData&gt; — one entry per item
///   - "loop_iteration_count"     : total number of items processed
///
/// Body workflow receives per-item WorkflowData that includes all current keys
/// plus <see cref="_loopItemKey"/> set to the current item.
///
/// Usage (code-first):
/// <code>
///   new LoopNode("ProcessFruits",
///       itemsKey:     "fruits",
///       outputKey:    "processed_fruits",
///       bodyBuilder:  loop => loop
///           .AddNode(new TransformNode("Upper",
///               d => d.Clone().Set("result", d.GetString("__item__")?.ToUpper()))))
/// </code>
/// </summary>
public sealed class LoopNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description =>
        $"Iterates over '{_itemsKey}', writing each result to '{_outputKey}'";

    /// <inheritdoc/>
    public override string IdPrefix => "loop";

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> InputPorts =>
    [
        new(_itemsKey, typeof(IEnumerable<object>), Required: true,
            "Collection to iterate — each element is placed in the loop body as loopItemKey")
    ];

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> OutputPorts =>
    [
        new(_outputKey,              typeof(List<WorkflowData>), Description: "Collected per-item WorkflowData results"),
        new("loop_iteration_count",  typeof(int),                Description: "Number of items iterated")
    ];

    private readonly string _itemsKey;
    private readonly string _outputKey;
    private readonly string _loopItemKey;
    private readonly int _maxIterations;
    private readonly Workflow? _body;

    /// <param name="name">Node name shown in logs.</param>
    /// <param name="itemsKey">WorkflowData key that holds the collection to iterate.</param>
    /// <param name="outputKey">WorkflowData key where per-item results are written.</param>
    /// <param name="loopItemKey">Key injected into each iteration's WorkflowData for the current item.</param>
    /// <param name="maxIterations">Safety cap (0 = unlimited).</param>
    /// <param name="bodyBuilder">Fluent builder for the per-item sub-workflow.</param>
    public LoopNode(
        string name,
        string itemsKey      = "items",
        string outputKey     = "results",
        string loopItemKey   = "__item__",
        int    maxIterations = 0,
        Action<Workflow>? bodyBuilder = null)
    {
        Name           = name;
        _itemsKey      = itemsKey;
        _outputKey     = outputKey;
        _loopItemKey   = loopItemKey;
        _maxIterations = maxIterations;

        if (bodyBuilder is not null)
        {
            _body = Workflow.Create($"{name}/Body");
            bodyBuilder(_body);
        }
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var rawItems = input.Get<IEnumerable<object>>(_itemsKey)
            ?? throw new InvalidOperationException(
                $"LoopNode '{Name}': key '{_itemsKey}' not found or is not a collection.");

        var items = rawItems.ToList();
        if (_maxIterations > 0 && items.Count > _maxIterations)
        {
            nodeCtx.Log($"⚠️  Capping iteration at {_maxIterations} (total={items.Count})");
            items = items.Take(_maxIterations).ToList();
        }

        nodeCtx.Log($"Iterating over {items.Count} item(s) in '{_itemsKey}'");

        var outputs = new List<WorkflowData>(items.Count);

        for (var i = 0; i < items.Count; i++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var itemData = input.Clone()
                .Set(_loopItemKey, items[i])
                .Set("__loop_index__", i);

            if (_body is not null)
            {
                var result = await _body.RunAsync(itemData, context);
                if (!result.IsSuccess)
                {
                    nodeCtx.Log($"  ✘ Iteration {i} failed: {result.ErrorMessage}");
                    throw new InvalidOperationException(
                        $"LoopNode '{Name}': iteration {i} failed — {result.ErrorMessage}");
                }
                outputs.Add(result.Data);
            }
            else
            {
                // No body configured — just collect the item data (runner injects body separately)
                outputs.Add(itemData);
            }
        }

        nodeCtx.SetMetadata("iteration_count", items.Count);
        nodeCtx.Log($"Loop complete: {items.Count} iteration(s)");

        return input.Clone()
            .Set(_outputKey, outputs)
            .Set("loop_iteration_count", items.Count);
    }
}
