using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Data;

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