namespace TwfAiFramework.Nodes.AI;
/// <summary>
/// Llm Configuration
/// </summary>
public sealed record LlmConfig
{
    /// <summary>
    /// Llm Provider
    /// </summary>
    public LlmProvider Provider { get; init; } = LlmProvider.OpenAI;
    /// <summary>
    /// AI Model
    /// </summary>
    public string Model { get; init; } = "gpt-4o";
    /// <summary>
    /// Api Key
    /// </summary>
    public string ApiKey { get; init; } = "";
    /// <summary>
    /// Api End point
    /// </summary>
    public string ApiEndpoint { get; init; } = "https://api.openai.com/v1/chat/completions";
    /// <summary>
    /// Open AI Api Temperature (0.0 - 2.0). Higher values produce more creative responses. Default is 0.7.
    /// </summary>
    public float Temperature { get; init; } = 0.7f;
    /// <summary>
    /// Max tokens to generate in the response. Default is 2048.
    /// </summary>
    public int MaxTokens { get; init; } = 2048;
    /// <summary>
    /// Default system prompt to use if none is provided in the input.
    /// </summary>
    public string? DefaultSystemPrompt { get; init; }
    /// <summary>
    /// Whether to maintain conversation history.
    /// </summary>
    public bool MaintainHistory { get; init; } = false;
    /// <summary>
    /// When true, the node streams the response via SSE and delivers chunks via <see cref="OnChunk"/>.
    /// </summary>
    public bool Stream { get; init; } = false;
    /// <summary>Called with each text chunk as it arrives during streaming. Ignored when <see cref="Stream"/> is false.</summary>
    public Action<string>? OnChunk { get; init; }

    // ─── Presets ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Factory method for creating an OpenAI configuration with the specified API key and model.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static LlmConfig OpenAI(string apiKey, string model = "gpt-4o") => new()
    {
        Provider = LlmProvider.OpenAI,
        Model = model,
        ApiKey = apiKey,
        ApiEndpoint = "https://api.openai.com/v1/chat/completions"
    };
    /// <summary>
    /// Factory method for creating an Anthropic configuration with the specified API key and model.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static LlmConfig Anthropic(string apiKey, string model = "claude-sonnet-4-20250514") => new()
    {
        Provider = LlmProvider.Anthropic,
        Model = model,
        ApiKey = apiKey,
        ApiEndpoint = "https://api.anthropic.com/v1/messages"
    };
    /// <summary>
    /// Factory method for creating an Ollama configuration with the specified model and host. Assumes no API key is required.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="host"></param>
    /// <returns></returns>
    public static LlmConfig Ollama(string model = "llama3.2", string host = "http://localhost:11434") => new()
    {
        Provider = LlmProvider.Ollama,
        Model = model,
        ApiEndpoint = $"{host}/v1/chat/completions"
    };
    /// <summary>
    /// Factory method for creating a custom LLM server configuration with the specified model, API key, and endpoint.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="apiKey"></param>
    /// <param name="apiEndpoint"></param>
    /// <returns></returns>
    public static LlmConfig LmServer(string model, string apiKey, string apiEndpoint) => new()
    {
        Provider = LlmProvider.Custom,
        Model = model,
        ApiKey = apiKey,
        ApiEndpoint = apiEndpoint
    };
}
