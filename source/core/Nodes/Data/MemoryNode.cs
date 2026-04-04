using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Data;

/// <summary>
/// Reads from or writes to the workflow's global state memory.
/// Use for maintaining context across pipeline runs — user preferences,
/// accumulated results, session state, etc.
/// </summary>
public sealed class MemoryNode : BaseNode
{
    public override string Name => $"Memory:{_mode}";
    public override string Category => "Data";
    public override string Description =>
        _mode == MemoryMode.Read
            ? $"Reads {string.Join(", ", _keys)} from global memory"
            : $"Writes {string.Join(", ", _keys)} to global memory";

    private readonly MemoryMode _mode;
    private readonly string[] _keys;

    private MemoryNode(MemoryMode mode, string[] keys)
    {
        _mode = mode;
        _keys = keys;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var output = input.Clone();

        if (_mode == MemoryMode.Read)
        {
            foreach (var key in _keys)
            {
                var val = context.GetState<object>(key);
                if (val is not null)
                {
                    output.Set(key, val);
                    nodeCtx.Log($"Read '{key}' from memory");
                }
                else
                {
                    nodeCtx.Log($"⚠️  Key '{key}' not found in memory");
                }
            }
        }
        else // Write
        {
            foreach (var key in _keys)
            {
                var val = input.Get<object>(key);
                if (val is not null)
                {
                    context.SetState(key, val);
                    nodeCtx.Log($"Wrote '{key}' to memory");
                }
            }
        }

        return Task.FromResult(output);
    }

    public static MemoryNode Read(params string[] keys) => new(MemoryMode.Read, keys);
    public static MemoryNode Write(params string[] keys) => new(MemoryMode.Write, keys);
}