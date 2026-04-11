using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.IO;

// ═══════════════════════════════════════════════════════════════════════════════
// FileWriterNode — Write output to a file
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Writes WorkflowData content to a file.
/// </summary>
public sealed class FileWriterNode : BaseNode
{
    public override string Name => "FileWriter";
    public override string Category => "IO";
    public override string Description => "Writes content from WorkflowData to a file";

    private readonly string _outputPath;
    private readonly string _dataKey;

    public FileWriterNode(string outputPath, string dataKey = "llm_response")
    {
        _outputPath = outputPath;
        _dataKey = dataKey;
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var content = input.Get<object>(_dataKey)?.ToString()
            ?? throw new InvalidOperationException(
                $"FileWriterNode: Key '{_dataKey}' not found in WorkflowData");

        var dir = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(_outputPath, content, context.CancellationToken);

        nodeCtx.Log($"Wrote {content.Length} chars to {_outputPath}");
        nodeCtx.SetMetadata("output_path", _outputPath);
        nodeCtx.SetMetadata("bytes_written", content.Length);

        return input.Clone().Set("output_file", _outputPath);
    }
}
