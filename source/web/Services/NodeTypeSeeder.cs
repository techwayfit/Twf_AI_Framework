using System.Text.Json;
using TwfAiFramework.Web.Data;
using TwfAiFramework.Web.Repositories;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Seeds the NodeTypes table from the static NodeSchemaProvider data on first run.
/// </summary>
public static class NodeTypeSeeder
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(INodeTypeRepository repo)
    {
        if (await repo.AnyAsync()) return; // already seeded

        var schemas = NodeSchemaProvider.GetAllSchemas();

        foreach (var (nodeType, name, category, description, color, icon) in GetNodeMetadata())
        {
            schemas.TryGetValue(nodeType, out var schema);
            schema ??= new TwfAiFramework.Web.Models.NodeParameterSchema { NodeType = nodeType };

            // Populate DataInputs/DataOutputs from the core node's declared ports
            var (inputs, outputs) = NodePortMetadataProvider.GetPorts(nodeType);
            schema.DataInputs  = inputs;
            schema.DataOutputs = outputs;

            var schemaJson = JsonSerializer.Serialize(schema, _jsonOptions);

            await repo.CreateAsync(new NodeTypeEntity
            {
                NodeType    = nodeType,
                Name        = name,
                Category    = category,
                Description = description,
                Color       = color,
                Icon        = icon,
                SchemaJson  = schemaJson,
                IsEnabled   = true,
            });
        }
    }

    // Returns the same ordered list that GetAvailableNodes() used to hard-code.
    private static IEnumerable<(string NodeType, string Name, string Category, string Description, string Color, string Icon)> GetNodeMetadata()
    {
        return new[]
        {
            // ── Control ───────────────────────────────────────────────────────
            ("StartNode",       "Start",           "Control", "Workflow entry point (required)",                                  "#2ecc71", "bi-play-circle-fill"),
            ("EndNode",         "End",             "Control", "Workflow exit point",                                              "#e74c3c", "bi-stop-circle-fill"),
            ("ErrorNode",       "Error Handler",   "Control", "Workflow-level error entry point (max 1 per workflow)",            "#e74c3c", "bi-exclamation-triangle-fill"),
            ("ErrorRouteNode",  "Error Route",     "Control", "Detect errors and route to success or error path",                "#e67e22", "bi-arrow-right-circle-fill"),
            ("ConditionNode",   "Condition",       "Control", "Evaluate conditions and write boolean flags to workflow data",    "#F5A623", "bi-question-diamond"),
            ("BranchNode",      "Branch",          "Control", "Route flow based on value matching (up to 3 cases + default)",   "#e67e22", "bi-signpost-split"),
            ("SubWorkflowNode", "Sub Workflow",    "Control", "Execute a child workflow and branch on success/error",            "#8e44ad", "bi-box-arrow-in-right"),
            ("LoopNode",        "Loop (ForEach)",  "Control", "Iterate over each item in a list and collect results",           "#f39c12", "bi-arrow-repeat"),
            ("DelayNode",       "Delay",           "Control", "Pause execution for a fixed duration",                           "#F5A623", "bi-clock"),
            ("MergeNode",       "Merge",           "Control", "Concatenate multiple data keys into one output key",             "#F5A623", "bi-intersect"),
            ("LogNode",         "Log",             "Control", "Emit a log checkpoint for debugging",                            "#F5A623", "bi-journal-text"),

            // ── Data ──────────────────────────────────────────────────────────
            ("SetVariableNode", "Set Variable",    "Data", "Write literal or {{interpolated}} values into workflow data",       "#7ED321", "bi-pencil"),
            ("TransformNode",   "Transform",       "Data", "Apply a custom transformation to workflow data",                   "#7ED321", "bi-arrow-left-right"),
            ("DataMapperNode",  "Data Mapper",     "Data", "Map source paths to target keys with dot-path support",            "#7ED321", "bi-map"),
            ("FilterNode",      "Filter",          "Data", "Validate data — fails or flags when rules are not met",            "#7ED321", "bi-funnel"),
            ("ChunkTextNode",   "Chunk Text",      "Data", "Split text into overlapping chunks (character/word/sentence)",     "#7ED321", "bi-file-break"),
            ("MemoryNode",      "Memory",          "Data", "Read or write keys from persistent workflow memory (state)",       "#7ED321", "bi-memory"),

            // ── IO ────────────────────────────────────────────────────────────
            ("HttpRequestNode", "HTTP Request",    "IO", "Make an HTTP/REST API call with configurable method and headers",    "#BD10E0", "bi-globe"),
            ("FileReadNode",    "File Read",       "IO", "Read a file from the local file system into workflow data",          "#BD10E0", "bi-file-earmark-text"),
            ("FileWriteNode",   "File Write",      "IO", "Write a workflow data key to a local file",                         "#BD10E0", "bi-file-earmark-arrow-down"),

            // ── AI ────────────────────────────────────────────────────────────
            ("LlmNode",           "LLM",           "AI", "Send a prompt to any OpenAI-compatible language model",             "#4A90E2", "bi-chat-left-dots"),
            ("PromptBuilderNode", "Prompt Builder","AI", "Build a dynamic prompt from a template with {{variable}} slots",   "#4A90E2", "bi-pencil-square"),
            ("EmbeddingNode",     "Embedding",     "AI", "Generate vector embeddings for RAG and semantic search",           "#4A90E2", "bi-diagram-2"),
            ("OutputParserNode",  "Output Parser", "AI", "Extract structured JSON from LLM text responses",                  "#4A90E2", "bi-list-check"),

            // ── Visual (no execution) ─────────────────────────────────────────
            ("ContainerNode",   "Container",       "Visual", "Group related nodes visually (no ports, no execution)",        "#6366f1", "bi-bounding-box"),
            ("NoteNode",        "Note",            "Visual", "Sticky note annotation on the canvas",                         "#f59e0b", "bi-sticky"),
        };
    }
}
