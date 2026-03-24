using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Provides parameter schemas for all node types
/// </summary>
public static class NodeSchemaProvider
{
    public static Dictionary<string, NodeParameterSchema> GetAllSchemas()
    {
        return new Dictionary<string, NodeParameterSchema>
        {
            // AI Nodes
            ["LlmNode"] = new()
            {
                NodeType = "LlmNode",
                Parameters = new()
            {
            new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "openai",
            Options = new() {
            new() { Value = "openai", Label = "OpenAI" },
            new() { Value = "azure", Label = "Azure OpenAI" },
            new() { Value = "anthropic", Label = "Anthropic" },
            new() { Value = "ollama", Label = "Ollama (Local)" }
            }
            },
            new() { Name = "model", Label = "Model", Type = ParameterType.Text, Required = true, DefaultValue = "gpt-4o", Placeholder = "e.g., gpt-4o, claude-3-opus" },
            new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to use environment variable" },
            new() { Name = "apiUrl", Label = "API URL", Type = ParameterType.Text, Required = false, Placeholder = "Custom API endpoint (optional)" },
            new() { Name = "systemPrompt", Label = "System Prompt", Type = ParameterType.TextArea, Required = false, Placeholder = "You are a helpful assistant..." },
            new() { Name = "temperature", Label = "Temperature", Type = ParameterType.Number, Required = false, DefaultValue = 0.7, MinValue = 0, MaxValue = 2 },
            new() { Name = "maxTokens", Label = "Max Tokens", Type = ParameterType.Number, Required = false, DefaultValue = 1000, MinValue = 1, MaxValue = 128000 },
            new() { Name = "maintainHistory", Label = "Maintain Chat History", Type = ParameterType.Boolean, Required = false, DefaultValue = false }
            }
            },

            ["PromptBuilderNode"] = new()
            {
                NodeType = "PromptBuilderNode",
                Parameters = new()
            {
            new() { Name = "promptTemplate", Label = "Prompt Template", Type = ParameterType.TextArea, Required = true,
            Placeholder = "Use {{variable}} syntax, e.g., 'Summarize: {{content}}'" },
            new() { Name = "systemTemplate", Label = "System Template", Type = ParameterType.TextArea, Required = false,
            Placeholder = "Optional system prompt template" }
            }
            },

            ["EmbeddingNode"] = new()
            {
                NodeType = "EmbeddingNode",
                Parameters = new()
            {
            new() { Name = "model", Label = "Embedding Model", Type = ParameterType.Select, Required = true, DefaultValue = "text-embedding-3-small",
            Options = new() {
            new() { Value = "text-embedding-3-small", Label = "OpenAI Embedding Small" },
            new() { Value = "text-embedding-3-large", Label = "OpenAI Embedding Large" },
            new() { Value = "text-embedding-ada-002", Label = "OpenAI Ada 002 (Legacy)" }
            }
            },
            new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to use environment variable" },
            new() { Name = "apiUrl", Label = "API URL", Type = ParameterType.Text, Required = false, DefaultValue = "https://api.openai.com/v1/embeddings" }
            }
            },

            ["OutputParserNode"] = new()
            {
                NodeType = "OutputParserNode",
                Parameters = new()
            {
            new() { Name = "fieldMapping", Label = "Field Mapping (JSON)", Type = ParameterType.Json, Required = false,
            Placeholder = "{\"sentiment\": \"sentiment\", \"score\": \"confidence\"}" },
            new() { Name = "strictMode", Label = "Strict Mode", Type = ParameterType.Boolean, Required = false, DefaultValue = false }
            }
            },

            // Control Nodes
            ["ConditionNode"] = new()
            {
                NodeType = "ConditionNode",
                Parameters = new()
            {
            new() { Name = "conditions", Label = "Conditions (JSON)", Type = ParameterType.Json, Required = true,
            Placeholder = "{\"is_positive\": \"sentiment == 'positive'\", \"is_urgent\": \"priority > 7\"}" }
            }
            },

            ["DelayNode"] = new()
            {
                NodeType = "DelayNode",
                Parameters = new()
            {
            new() { Name = "delayMs", Label = "Delay (milliseconds)", Type = ParameterType.Number, Required = true, DefaultValue = 1000, MinValue = 0, MaxValue = 60000 },
            new() { Name = "reason", Label = "Reason", Type = ParameterType.Text, Required = false, Placeholder = "e.g., Rate limit" }
            }
            },

            ["MergeNode"] = new()
            {
                NodeType = "MergeNode",
                Parameters = new()
            {
            new() { Name = "sourceKeys", Label = "Source Keys (comma-separated)", Type = ParameterType.Text, Required = true, Placeholder = "key1, key2, key3" },
            new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, Placeholder = "merged_output" },
            new() { Name = "separator", Label = "Separator", Type = ParameterType.Text, Required = false, DefaultValue = "\n" }
            }
            },

            ["LogNode"] = new()
            {
                NodeType = "LogNode",
                Parameters = new()
            {
            new() { Name = "label", Label = "Checkpoint Label", Type = ParameterType.Text, Required = true, Placeholder = "Checkpoint name" },
            new() { Name = "keysToLog", Label = "Keys to Log (comma-separated)", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty for all keys" },
            new() { Name = "logLevel", Label = "Log Level", Type = ParameterType.Select, Required = false, DefaultValue = "Information",
            Options = new() {
            new() { Value = "Trace", Label = "Trace" },
            new() { Value = "Debug", Label = "Debug" },
            new() { Value = "Information", Label = "Information" },
            new() { Value = "Warning", Label = "Warning" },
            new() { Value = "Error", Label = "Error" }
            }
            }
            }
            },

            // Data Nodes
            ["TransformNode"] = new()
            {
                NodeType = "TransformNode",
                Parameters = new()
            {
            new() { Name = "transformType", Label = "Transform Type", Type = ParameterType.Select, Required = true, DefaultValue = "custom",
            Options = new() {
            new() { Value = "rename", Label = "Rename Key" },
            new() { Value = "select", Label = "Select Key" },
            new() { Value = "concat", Label = "Concatenate Strings" },
            new() { Value = "custom", Label = "Custom (requires code)" }
            }
            },
            new() { Name = "fromKey", Label = "From Key", Type = ParameterType.Text, Required = false, Placeholder = "Source key name" },
            new() { Name = "toKey", Label = "To Key", Type = ParameterType.Text, Required = false, Placeholder = "Target key name" },
            new() { Name = "keys", Label = "Keys (comma-separated)", Type = ParameterType.Text, Required = false, Placeholder = "For concat: key1, key2, key3" },
            new() { Name = "separator", Label = "Separator", Type = ParameterType.Text, Required = false, DefaultValue = " " }
            }
            },

            ["FilterNode"] = new()
            {
                NodeType = "FilterNode",
                Parameters = new()
            {
            new() { Name = "throwOnFail", Label = "Throw on Validation Failure", Type = ParameterType.Boolean, Required = false, DefaultValue = true },
            new() { Name = "requiredKeys", Label = "Required Keys (comma-separated)", Type = ParameterType.Text, Required = false, Placeholder = "email, name, phone" },
            new() { Name = "maxLengths", Label = "Max Lengths (JSON)", Type = ParameterType.Json, Required = false, Placeholder = "{\"name\": 100, \"bio\": 500}" }
            }
            },

            ["ChunkTextNode"] = new()
            {
                NodeType = "ChunkTextNode",
                Parameters = new()
            {
            new() { Name = "chunkSize", Label = "Chunk Size", Type = ParameterType.Number, Required = false, DefaultValue = 500, MinValue = 50, MaxValue = 10000 },
            new() { Name = "overlap", Label = "Overlap", Type = ParameterType.Number, Required = false, DefaultValue = 50, MinValue = 0, MaxValue = 1000 },
            new() { Name = "strategy", Label = "Chunking Strategy", Type = ParameterType.Select, Required = false, DefaultValue = "Character",
            Options = new() {
            new() { Value = "Character", Label = "By Character" },
            new() { Value = "Word", Label = "By Word" },
            new() { Value = "Sentence", Label = "By Sentence" }
            }
            }
            }
            },

            ["MemoryNode"] = new()
            {
                NodeType = "MemoryNode",
                Parameters = new()
            {
            new() { Name = "mode", Label = "Mode", Type = ParameterType.Select, Required = true, DefaultValue = "Read",
            Options = new() {
            new() { Value = "Read", Label = "Read from Memory" },
            new() { Value = "Write", Label = "Write to Memory" }
            }
            },
            new() { Name = "keys", Label = "Keys (comma-separated)", Type = ParameterType.Text, Required = true, Placeholder = "user_id, session_state" }
            }
            },

            // IO Nodes
            ["HttpRequestNode"] = new()
            {
                NodeType = "HttpRequestNode",
                Parameters = new()
            {
            new() { Name = "method", Label = "HTTP Method", Type = ParameterType.Select, Required = true, DefaultValue = "GET",
            Options = new() {
            new() { Value = "GET", Label = "GET" },
            new() { Value = "POST", Label = "POST" },
            new() { Value = "PUT", Label = "PUT" },
            new() { Value = "PATCH", Label = "PATCH" },
            new() { Value = "DELETE", Label = "DELETE" }
            }
            },
            new() { Name = "urlTemplate", Label = "URL Template", Type = ParameterType.Text, Required = true,
            Placeholder = "https://api.example.com/users/{{user_id}}" },
            new() { Name = "headers", Label = "Headers (JSON)", Type = ParameterType.Json, Required = false,
            Placeholder = "{\"Authorization\": \"Bearer token\", \"Content-Type\": \"application/json\"}" },
            new() { Name = "body", Label = "Request Body Template", Type = ParameterType.TextArea, Required = false,
            Placeholder = "Use {{variable}} syntax" },
            new() { Name = "timeout", Label = "Timeout (seconds)", Type = ParameterType.Number, Required = false, DefaultValue = 30, MinValue = 1, MaxValue = 300 },
            new() { Name = "throwOnError", Label = "Throw on HTTP Error", Type = ParameterType.Boolean, Required = false, DefaultValue = true }
            }
            }
        };
    }

    public static NodeParameterSchema? GetSchema(string nodeType)
    {
        var schemas = GetAllSchemas();
        return schemas.TryGetValue(nodeType, out var schema) ? schema : null;
    }
}
