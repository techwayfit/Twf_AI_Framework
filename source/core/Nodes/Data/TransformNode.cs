using TwfAiFramework.Core;
using TwfAiFramework.Nodes;

namespace TwfAiFramework.Nodes.Data;

// ═══════════════════════════════════════════════════════════════════════════════
// TransformNode — Apply a custom transformation to WorkflowData
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Applies a custom synchronous or asynchronous transformation to WorkflowData.
/// Use this for data cleaning, reformatting, mapping, or any custom logic
/// that doesn't need a full custom node class.
///
/// Example:
///   new TransformNode("Normalize", data => data.Set("name", data.GetString("name")?.ToUpper()))
/// </summary>
public sealed class TransformNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Data";
    public override string Description => $"Custom data transformation: {Name}";

    private readonly Func<WorkflowData, Task<WorkflowData>> _transform;

    public TransformNode(string name, Func<WorkflowData, WorkflowData> transform)
    {
        Name = name;
        _transform = data => Task.FromResult(transform(data));
    }

    public TransformNode(string name, Func<WorkflowData, Task<WorkflowData>> transform)
    {
        Name = name;
        _transform = transform;
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var output = await _transform(input.Clone());
        nodeCtx.Log($"Transformed {input.Keys.Count} → {output.Keys.Count} keys");
        return output;
    }

    // ─── Prebuilt transforms ──────────────────────────────────────────────────

    /// <summary>Rename a key in the data bag.</summary>
    public static TransformNode Rename(string fromKey, string toKey) =>
        new($"Rename:{fromKey}→{toKey}", data =>
        {
            var val = data.Get<object>(fromKey);
            return data.Clone().Remove(fromKey).Set(toKey, val);
        });

    /// <summary>Extract nested JSON property into a top-level key.</summary>
    public static TransformNode SelectKey(string sourceKey, string targetKey) =>
        new($"Select:{sourceKey}→{targetKey}", data =>
        {
            var val = data.Get<object>(sourceKey);
            return data.Clone().Set(targetKey, val);
        });

    /// <summary>Combine multiple string keys into one.</summary>
    public static TransformNode ConcatStrings(
        string outputKey, string separator, params string[] keys) =>
        new($"Concat→{outputKey}", data =>
        {
            var parts = keys.Select(k => data.GetString(k) ?? "");
            return data.Clone().Set(outputKey, string.Join(separator, parts));
        });
}

// ═══════════════════════════════════════════════════════════════════════════════
// DataMapperNode — Explicit key/path mapping between node outputs and inputs
// ═══════════════════════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════════════════
// FilterNode — Validate/filter WorkflowData based on conditions
// ═══════════════════════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════════════════
// ChunkTextNode — Split large text into overlapping chunks for RAG
// ═══════════════════════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════════════════
// MemoryNode — Store and retrieve from conversation/session memory
// ═══════════════════════════════════════════════════════════════════════════════