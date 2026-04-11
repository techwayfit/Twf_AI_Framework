using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.IO;

// ═══════════════════════════════════════════════════════════════════════════════
// FileReaderNode — Read files from disk
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Reads a file from the filesystem and puts its content into WorkflowData.
/// Supports plain text, JSON, and CSV files. PDF support requires a PDF library.
///
/// Reads from WorkflowData:
///   - "file_path" : path to the file (overrides static config)
///
/// Writes to WorkflowData:
///   - "text"          : file content as string
///   - "file_name"     : filename without path
///   - "file_size"     : file size in bytes
///   - "file_extension": e.g. "txt", "json", "csv"
/// </summary>
public sealed class FileReaderNode : BaseNode
{
    public override string Name => "FileReader";
    public override string Category => "IO";
    public override string Description => "Reads a file from disk into WorkflowData";

    private readonly string? _staticFilePath;

    public FileReaderNode(string? staticFilePath = null)
    {
        _staticFilePath = staticFilePath;
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var filePath = input.GetString("file_path") ?? _staticFilePath
            ?? throw new InvalidOperationException(
                "FileReaderNode requires 'file_path' in WorkflowData or static config");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var info = new FileInfo(filePath);
        nodeCtx.Log($"Reading file: {info.Name} ({info.Length} bytes)");

        var content = await File.ReadAllTextAsync(filePath, context.CancellationToken);
        var ext = info.Extension.TrimStart('.').ToLower();

        nodeCtx.SetMetadata("file_size", info.Length);
        nodeCtx.SetMetadata("file_extension", ext);

        return input.Clone()
            .Set("text", content)
            .Set("file_name", info.Name)
            .Set("file_size", info.Length)
            .Set("file_extension", ext);
    }
}
