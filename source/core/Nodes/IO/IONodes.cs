using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using System.Text;
using System.Text.Json;

namespace TwfAiFramework.Nodes.IO;

// ═══════════════════════════════════════════════════════════════════════════════
// HttpRequestNode — Make HTTP API calls from within a workflow
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Makes HTTP requests to external APIs — REST APIs, webhooks, data sources.
/// Like n8n's HTTP Request node.
///
/// Reads from WorkflowData:
///   - Keys specified in UrlTemplate as {{variable}} placeholders
///   - "request_body" : if method is POST/PUT and no body configured
///
/// Writes to WorkflowData:
///   - "http_response"      : response body (parsed JSON or raw string)
///   - "http_status_code"   : HTTP status code
///   - "http_headers"       : response headers dictionary
/// </summary>
public sealed class HttpRequestNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "IO";
    public override string Description =>
        $"HTTP {_config.Method} {_config.UrlTemplate}";

    /// <inheritdoc/>
    public override string IdPrefix => "http";

    /// <inheritdoc/>
    // Input ports = {{variable}} placeholders extracted from UrlTemplate at construction time.
    public override IReadOnlyList<NodePort> InputPorts
    {
        get
        {
            var ports = System.Text.RegularExpressions.Regex
                .Matches(_config.UrlTemplate, @"\{\{(\w+)\}\}")
                .Select(m => new NodePort(m.Groups[1].Value, typeof(string), Required: false,
                    "URL template variable"))
                .DistinctBy(p => p.Key)
                .ToList<NodePort>();
            if (_config.Method is "POST" or "PUT" or "PATCH")
                ports.Add(new NodePort("request_body", typeof(object), Required: false, "Request body (if no static body configured)"));
            return ports;
        }
    }

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> OutputPorts =>
    [
        new("http_response",    typeof(object), Description: "Parsed JSON or raw response string"),
        new("http_status_code", typeof(int),    Description: "HTTP status code"),
        new("http_headers",     typeof(Dictionary<string,string>), Required: false, "Response headers")
    ];

    private readonly HttpRequestConfig _config;
    private readonly HttpClient _httpClient;

    public HttpRequestNode(string name, HttpRequestConfig config, HttpClient? httpClient = null)
    {
        Name = name;
        _config = config;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = config.Timeout;
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        // Build URL with variable substitution
        var url = RenderUrl(_config.UrlTemplate, input);
        nodeCtx.Log($"{_config.Method} {url}");

        using var request = new HttpRequestMessage(
            new HttpMethod(_config.Method), url);

        // Add headers
        foreach (var (k, v) in _config.Headers)
            request.Headers.TryAddWithoutValidation(k, v);

        // Add body for POST/PUT/PATCH
        if (_config.Method is "POST" or "PUT" or "PATCH")
        {
            var body = _config.Body ?? input.Get<object>("request_body");
            if (body is not null)
            {
                var json = body is string s ? s : JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
        }

        var response = await _httpClient.SendAsync(request, context.CancellationToken);
        var statusCode = (int)response.StatusCode;

        nodeCtx.Log($"Response: {statusCode} {response.StatusCode}");
        nodeCtx.SetMetadata("status_code", statusCode);

        if (!response.IsSuccessStatusCode && _config.ThrowOnError)
            throw new HttpRequestException(
                $"HTTP {_config.Method} {url} failed with status {statusCode}");

        var responseBody = await response.Content.ReadAsStringAsync(context.CancellationToken);

        // Parse JSON if response is JSON
        object parsedResponse = responseBody;
        if (response.Content.Headers.ContentType?.MediaType == "application/json")
        {
            try
            {
                parsedResponse = JsonSerializer.Deserialize<object>(responseBody)!;
            }
            catch { /* keep as string */ }
        }

        var headers = response.Headers.ToDictionary(
            h => h.Key, h => string.Join(", ", h.Value));

        return input.Clone()
            .Set("http_response", parsedResponse)
            .Set("http_status_code", statusCode)
            .Set("http_headers", headers);
    }

    private string RenderUrl(string template, WorkflowData data)
    {
        return System.Text.RegularExpressions.Regex.Replace(template, @"\{\{(\w+)\}\}", m =>
        {
            var key = m.Groups[1].Value;
            return data.GetString(key) ?? m.Value;
        });
    }

    // ─── Prebuilt HTTP nodes ──────────────────────────────────────────────────

    public static HttpRequestNode Get(string name, string url,
        Dictionary<string, string>? headers = null) =>
        new(name, new HttpRequestConfig
        {
            Method = "GET",
            UrlTemplate = url,
            Headers = headers ?? new()
        });

    public static HttpRequestNode Post(string name, string url,
        object? body = null,
        Dictionary<string, string>? headers = null) =>
        new(name, new HttpRequestConfig
        {
            Method = "POST",
            UrlTemplate = url,
            Body = body,
            Headers = headers ?? new()
        });
}

public sealed class HttpRequestConfig
{
    public string Method { get; init; } = "GET";
    public string UrlTemplate { get; init; } = "";
    public Dictionary<string, string> Headers { get; init; } = new();
    public object? Body { get; init; }
    public bool ThrowOnError { get; init; } = true;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}

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
