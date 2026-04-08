using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using System.Text;
using System.Text.Json;
using twf_ai_framework.Core.Models;

namespace TwfAiFramework.Nodes.AI;

/// <summary>
/// Calls any OpenAI-compatible LLM API (OpenAI, Anthropic, Ollama, Azure OpenAI, etc.)
/// 
/// Reads from WorkflowData:
///   - "prompt" or "messages" : the prompt to send
///   - "system_prompt"        : optional system instruction
///
/// Writes to WorkflowData:
///   - "llm_response"    : the model's text response
///   - "llm_model"       : model used
///   - "prompt_tokens"   : tokens used in prompt
///   - "completion_tokens": tokens used in completion
/// </summary>
public sealed class LlmNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "AI";
    public override string Description =>
        $"Calls {_config.Provider} ({_config.Model}) with the current prompt";

    /// <inheritdoc/>
    public override string IdPrefix => "llm";

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> InputPorts =>
    [
        new("prompt",        typeof(string), Required: true,  "Prompt text to send to the model"),
        new("system_prompt", typeof(string), Required: false, "System instruction (overrides node config)")
    ];

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> OutputPorts =>
    [
        new("llm_response",      typeof(string), Description: "Model's text response"),
        new("llm_model",         typeof(string), Description: "Model name used"),
        new("prompt_tokens",     typeof(int),    Description: "Tokens consumed by the prompt"),
        new("completion_tokens", typeof(int),    Description: "Tokens in the completion")
    ];

    private readonly LlmConfig _config;
    private readonly HttpClient _httpClient;

    public LlmNode(string name, LlmConfig config, HttpClient? httpClient = null)
    {
        Name = name;
        _config = config;
        _httpClient = httpClient ?? new HttpClient();
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        // Build the messages list
        var messages = BuildMessages(input, context);

        nodeCtx.Log($"Sending {messages.Count} messages to {_config.Model}");
        nodeCtx.Log($"Last message: {messages.Last().Content[..Math.Min(100, messages.Last().Content.Length)]}...");

        var requestBody = BuildRequestBody(messages);

        string llmResponse;
        (int PromptTokens, int CompletionTokens) usage;

        if (_config.Stream)
        {
            (llmResponse, usage) = await StreamApiAsync(requestBody, nodeCtx, context.CancellationToken);
        }
        else
        {
            var response = await CallApiAsync(requestBody, context.CancellationToken);
            llmResponse = ExtractResponse(response);
            usage = ExtractUsage(response);
        }

        nodeCtx.SetMetadata("model", _config.Model);
        nodeCtx.SetMetadata("prompt_tokens", usage.PromptTokens);
        nodeCtx.SetMetadata("completion_tokens", usage.CompletionTokens);
        nodeCtx.Log($"Response length: {llmResponse.Length} chars | Tokens: {usage.PromptTokens}+{usage.CompletionTokens}");

        // Optionally append to conversation history
        if (_config.MaintainHistory)
        {
            context.AppendMessage(ChatMessage.Assistant(llmResponse));
        }

        return input.Clone()
            .Set("llm_response", llmResponse)
            .Set("llm_model", _config.Model)
            .Set("prompt_tokens", usage.PromptTokens)
            .Set("completion_tokens", usage.CompletionTokens);
    }

    private List<(string Role, string Content)> BuildMessages(
        WorkflowData input, WorkflowContext context)
    {
        var messages = new List<(string Role, string Content)>();

        // Add system prompt
        var systemPrompt = input.GetString("system_prompt") ?? _config.DefaultSystemPrompt;
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(("system", systemPrompt));

        // Optionally include conversation history
        if (_config.MaintainHistory)
        {
            foreach (var msg in context.GetChatHistory())
                messages.Add((msg.Role, msg.Content));
        }

        // Add current prompt or raw messages
        if (input.TryGet<List<(string, string)>>("messages", out var rawMessages) && rawMessages is not null)
        {
            messages.AddRange(rawMessages);
        }
        else
        {
            var prompt = input.GetRequiredString("prompt");

            if (_config.MaintainHistory)
                context.AppendMessage(ChatMessage.User(prompt));

            messages.Add(("user", prompt));
        }

        return messages;
    }

    private Dictionary<string, object?> BuildRequestBody(List<(string Role, string Content)> messages)
    {
        return _config.Provider switch
        {
            LlmProvider.OpenAI or LlmProvider.AzureOpenAI or LlmProvider.Ollama or LlmProvider.Custom =>
                BuildOpenAiStyleBody(messages),
            LlmProvider.Anthropic =>
                BuildAnthropicBody(messages),
            _ => throw new NotSupportedException($"Provider {_config.Provider} not supported")
        };
    }

    private Dictionary<string, object?> BuildOpenAiStyleBody(List<(string Role, string Content)> messages)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = _config.Model,
            ["messages"] = messages.Select(m => new { role = m.Role, content = m.Content }),
            ["temperature"] = _config.Temperature,
            ["max_tokens"] = _config.MaxTokens,
            ["stream"] = _config.Stream
        };
        // include_usage in the final chunk — supported by OpenAI and AzureOpenAI only
        if (_config.Stream && _config.Provider is LlmProvider.OpenAI or LlmProvider.AzureOpenAI)
            body["stream_options"] = new { include_usage = true };
        return body;
    }

    private Dictionary<string, object?> BuildAnthropicBody(List<(string Role, string Content)> messages)
    {
        return new Dictionary<string, object?>
        {
            ["model"] = _config.Model,
            ["max_tokens"] = _config.MaxTokens,
            ["temperature"] = _config.Temperature,
            ["system"] = messages.FirstOrDefault(m => m.Role == "system").Content ?? "",
            ["messages"] = messages
                .Where(m => m.Role != "system")
                .Select(m => new { role = m.Role, content = m.Content }),
            ["stream"] = _config.Stream
        };
    }

    private async Task<JsonDocument> CallApiAsync(object requestBody, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.ApiEndpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        AddAuthHeaders(request);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonDocument.Parse(content);
    }

    private void AddAuthHeaders(HttpRequestMessage request)
    {
        if (string.IsNullOrEmpty(_config.ApiKey)) return;

        if (_config.Provider == LlmProvider.Anthropic)
        {
            request.Headers.Add("x-api-key", _config.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
        }
        else
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }
    }

    private async Task<(string Text, (int PromptTokens, int CompletionTokens) Usage)> StreamApiAsync(
        object requestBody, NodeExecutionContext nodeCtx, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.ApiEndpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        AddAuthHeaders(request);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var sb = new StringBuilder();
        int promptTokens = 0, completionTokens = 0;

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            try
            {
                using var doc = JsonDocument.Parse(data);
                if (_config.Provider == LlmProvider.Anthropic)
                    ParseAnthropicChunk(doc, sb, ref promptTokens, ref completionTokens);
                else
                    ParseOpenAiChunk(doc, sb, ref promptTokens, ref completionTokens);
            }
            catch (JsonException) { /* skip malformed SSE chunks */ }
        }

        nodeCtx.Log($"Stream complete: {sb.Length} chars");
        return (sb.ToString(), (promptTokens, completionTokens));
    }

    private void ParseOpenAiChunk(
        JsonDocument doc, StringBuilder sb, ref int promptTokens, ref int completionTokens)
    {
        var root = doc.RootElement;

        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var delta = choices[0].GetProperty("delta");
            if (delta.TryGetProperty("content", out var contentEl))
            {
                var chunk = contentEl.GetString() ?? "";
                if (chunk.Length > 0)
                {
                    sb.Append(chunk);
                    _config.OnChunk?.Invoke(chunk);
                }
            }
        }

        // Final chunk carries usage when stream_options.include_usage = true
        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
        {
            if (usage.TryGetProperty("prompt_tokens", out var ptEl)) promptTokens = ptEl.GetInt32();
            if (usage.TryGetProperty("completion_tokens", out var ctEl)) completionTokens = ctEl.GetInt32();
        }
    }

    private void ParseAnthropicChunk(
        JsonDocument doc, StringBuilder sb, ref int promptTokens, ref int completionTokens)
    {
        var root = doc.RootElement;
        if (!root.TryGetProperty("type", out var typeEl)) return;

        switch (typeEl.GetString())
        {
            case "message_start":
                if (root.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("usage", out var startUsage) &&
                    startUsage.TryGetProperty("input_tokens", out var it))
                    promptTokens = it.GetInt32();
                break;

            case "content_block_delta":
                if (root.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("type", out var dt) && dt.GetString() == "text_delta" &&
                    delta.TryGetProperty("text", out var textEl))
                {
                    var chunk = textEl.GetString() ?? "";
                    if (chunk.Length > 0)
                    {
                        sb.Append(chunk);
                        _config.OnChunk?.Invoke(chunk);
                    }
                }
                break;

            case "message_delta":
                if (root.TryGetProperty("usage", out var endUsage) &&
                    endUsage.TryGetProperty("output_tokens", out var ot))
                    completionTokens = ot.GetInt32();
                break;
        }
    }

    private string ExtractResponse(JsonDocument doc)
    {
        return _config.Provider switch
        {
            LlmProvider.Anthropic =>
                doc.RootElement
                   .GetProperty("content")[0]
                   .GetProperty("text").GetString() ?? "",
            _ =>
                doc.RootElement
                   .GetProperty("choices")[0]
                   .GetProperty("message")
                   .GetProperty("content").GetString() ?? ""
        };
    }

    private (int PromptTokens, int CompletionTokens) ExtractUsage(JsonDocument doc)
    {
        try
        {
            var usage = doc.RootElement.GetProperty("usage");
            return _config.Provider switch
            {
                LlmProvider.Anthropic => (
                    usage.GetProperty("input_tokens").GetInt32(),
                    usage.GetProperty("output_tokens").GetInt32()),
                _ => (
                    usage.GetProperty("prompt_tokens").GetInt32(),
                    usage.GetProperty("completion_tokens").GetInt32())
            };
        }
        catch { return (0, 0); }
    }
}

// ─── Configuration ────────────────────────────────────────────────────────────

public enum LlmProvider { OpenAI, Anthropic, AzureOpenAI, Ollama, Custom }

public sealed record LlmConfig
{
    public LlmProvider Provider { get; init; } = LlmProvider.OpenAI;
    public string Model { get; init; } = "gpt-4o";
    public string ApiKey { get; init; } = "";
    public string ApiEndpoint { get; init; } = "https://api.openai.com/v1/chat/completions";
    public float Temperature { get; init; } = 0.7f;
    public int MaxTokens { get; init; } = 2048;
    public string? DefaultSystemPrompt { get; init; }
    public bool MaintainHistory { get; init; } = false;
    /// <summary>When true, the node streams the response via SSE and delivers chunks via <see cref="OnChunk"/>.</summary>
    public bool Stream { get; init; } = false;
    /// <summary>Called with each text chunk as it arrives during streaming. Ignored when <see cref="Stream"/> is false.</summary>
    public Action<string>? OnChunk { get; init; }

    // ─── Presets ──────────────────────────────────────────────────────────────

    public static LlmConfig OpenAI(string apiKey, string model = "gpt-4o") => new()
    {
        Provider = LlmProvider.OpenAI,
        Model = model,
        ApiKey = apiKey,
        ApiEndpoint = "https://api.openai.com/v1/chat/completions"
    };

    public static LlmConfig Anthropic(string apiKey, string model = "claude-sonnet-4-20250514") => new()
    {
        Provider = LlmProvider.Anthropic,
        Model = model,
        ApiKey = apiKey,
        ApiEndpoint = "https://api.anthropic.com/v1/messages"
    };

    public static LlmConfig Ollama(string model = "llama3.2", string host = "http://localhost:11434") => new()
    {
        Provider = LlmProvider.Ollama,
        Model = model,
        ApiEndpoint = $"{host}/v1/chat/completions"
    };

    public static LlmConfig LmServer(string model, string apiKey, string apiEndpoint) => new()
    {
        Provider = LlmProvider.Custom,
        Model = model,
        ApiKey = apiKey,
        ApiEndpoint = apiEndpoint
    };
}
