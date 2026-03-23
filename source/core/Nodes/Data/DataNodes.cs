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
// FilterNode — Validate/filter WorkflowData based on conditions
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Validates WorkflowData against conditions. If validation fails, the node
/// throws (stopping the pipeline) or sets an "is_valid" flag.
/// Use for input validation, safety checks, and guardrails.
/// </summary>
public sealed class FilterNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Data";
    public override string Description => $"Data validation filter: {Name}";

    private readonly List<FilterRule> _rules;
    private readonly bool _throwOnFail;

    public FilterNode(string name, bool throwOnFail = true)
    {
        Name = name;
        _throwOnFail = throwOnFail;
        _rules = new List<FilterRule>();
    }

    public FilterNode Require(string key, string? reason = null)
    {
        _rules.Add(new FilterRule(key, data => data.Has(key),
            reason ?? $"Required field '{key}' is missing"));
        return this;
    }

    public FilterNode RequireNonEmpty(string key)
    {
        _rules.Add(new FilterRule(key, data =>
        {
            var val = data.GetString(key);
            return !string.IsNullOrWhiteSpace(val);
        }, $"Field '{key}' must not be empty"));
        return this;
    }

    public FilterNode MaxLength(string key, int maxLength)
    {
        _rules.Add(new FilterRule(key, data =>
            (data.GetString(key)?.Length ?? 0) <= maxLength,
            $"Field '{key}' exceeds max length of {maxLength}"));
        return this;
    }

    public FilterNode Custom(string key, Func<WorkflowData, bool> predicate, string errorMessage)
    {
        _rules.Add(new FilterRule(key, predicate, errorMessage));
        return this;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var failures = new List<string>();

        foreach (var rule in _rules)
        {
            if (!rule.Predicate(input))
            {
                failures.Add(rule.ErrorMessage);
                nodeCtx.Log($"❌ Validation failed: {rule.ErrorMessage}");
            }
        }

        var isValid = failures.Count == 0;
        var output = input.Clone().Set("is_valid", isValid);

        if (!isValid)
        {
            var errorSummary = string.Join("; ", failures);
            output.Set("validation_errors", failures);

            if (_throwOnFail)
                throw new ValidationException(Name, errorSummary);
        }
        else
        {
            nodeCtx.Log($"✅ All {_rules.Count} validation rules passed");
        }

        nodeCtx.SetMetadata("rules_checked", _rules.Count);
        nodeCtx.SetMetadata("failures", failures.Count);

        return Task.FromResult(output);
    }

    private record FilterRule(string Key, Func<WorkflowData, bool> Predicate, string ErrorMessage);
}

public sealed class ValidationException : Exception
{
    public string NodeName { get; }
    public ValidationException(string nodeName, string message)
        : base($"[{nodeName}] Validation failed: {message}")
    {
        NodeName = nodeName;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ChunkTextNode — Split large text into overlapping chunks for RAG
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Splits large text into overlapping chunks suitable for embedding and RAG.
/// Supports character-based, word-based, and sentence-based chunking strategies.
///
/// Reads from WorkflowData:
///   - "text" : the source text to chunk
///
/// Writes to WorkflowData:
///   - "chunks"      : List&lt;TextChunk&gt; — the chunked result
///   - "chunk_count" : number of chunks created
/// </summary>
public sealed class ChunkTextNode : BaseNode
{
    public override string Name => "ChunkText";
    public override string Category => "Data";
    public override string Description =>
        $"Splits text into {_config.ChunkSize}-char chunks with {_config.Overlap}-char overlap";

    private readonly ChunkConfig _config;

    public ChunkTextNode(ChunkConfig? config = null)
    {
        _config = config ?? new ChunkConfig();
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var text = input.GetRequiredString("text");
        var source = input.GetString("source") ?? "unknown";

        var chunks = _config.Strategy switch
        {
            ChunkStrategy.Character => ChunkByCharacter(text, source),
            ChunkStrategy.Word => ChunkByWord(text, source),
            ChunkStrategy.Sentence => ChunkBySentence(text, source),
            _ => ChunkByCharacter(text, source)
        };

        nodeCtx.Log($"Split {text.Length} chars into {chunks.Count} chunks " +
                    $"(strategy={_config.Strategy}, size={_config.ChunkSize})");
        nodeCtx.SetMetadata("chunk_count", chunks.Count);
        nodeCtx.SetMetadata("avg_chunk_size",
            chunks.Count > 0 ? chunks.Average(c => c.Text.Length) : 0);

        return Task.FromResult(input.Clone()
            .Set("chunks", chunks)
            .Set("chunk_count", chunks.Count));
    }

    private List<TextChunk> ChunkByCharacter(string text, string source)
    {
        var chunks = new List<TextChunk>();
        var i = 0;
        while (i < text.Length)
        {
            var end = Math.Min(i + _config.ChunkSize, text.Length);
            chunks.Add(new TextChunk(
                text[i..end], source, chunks.Count, i, end));
            i += _config.ChunkSize - _config.Overlap;
        }
        return chunks;
    }

    private List<TextChunk> ChunkByWord(string text, string source)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<TextChunk>();
        var i = 0;
        while (i < words.Length)
        {
            var batch = words.Skip(i).Take(_config.ChunkSize).ToArray();
            chunks.Add(new TextChunk(
                string.Join(" ", batch), source, chunks.Count, i, i + batch.Length));
            i += _config.ChunkSize - _config.Overlap;
        }
        return chunks;
    }

    private List<TextChunk> ChunkBySentence(string text, string source)
    {
        var sentences = text.Split(new[] { ". ", "! ", "? " },
            StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<TextChunk>();
        var i = 0;
        while (i < sentences.Length)
        {
            var batch = sentences.Skip(i).Take(_config.ChunkSize);
            chunks.Add(new TextChunk(
                string.Join(". ", batch) + ".", source, chunks.Count, i, i + _config.ChunkSize));
            i += _config.ChunkSize - _config.Overlap;
        }
        return chunks;
    }
}

public record TextChunk(string Text, string Source, int Index, int StartPos, int EndPos);

public sealed class ChunkConfig
{
    public int ChunkSize { get; init; } = 500;
    public int Overlap { get; init; } = 50;
    public ChunkStrategy Strategy { get; init; } = ChunkStrategy.Character;
}

public enum ChunkStrategy { Character, Word, Sentence }

// ═══════════════════════════════════════════════════════════════════════════════
// MemoryNode — Store and retrieve from conversation/session memory
// ═══════════════════════════════════════════════════════════════════════════════

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

public enum MemoryMode { Read, Write }
