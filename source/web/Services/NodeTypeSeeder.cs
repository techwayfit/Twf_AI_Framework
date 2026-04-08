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
            var schemaJson = schema != null
                ? JsonSerializer.Serialize(schema, _jsonOptions)
                : "{}";

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
            // Control
            ("StartNode",        "Start",            "Control", "Workflow entry point (required)",                                           "#2ecc71", "bi-play-circle-fill"),
            ("EndNode",          "End",              "Control", "Workflow exit point",                                                       "#e74c3c", "bi-stop-circle-fill"),
            ("ErrorNode",        "Error Handler",    "Control", "Workflow-level error entry point (max 1 per workflow)",                     "#e74c3c", "bi-exclamation-triangle-fill"),
            ("ConditionNode",    "Condition",        "Control", "Evaluate a boolean expression and route success or failure",                "#F5A623", "bi-question-diamond"),
            ("SwitchNode",       "Switch",           "Control", "Route to first matching boolean condition (up to 4 cases)",                 "#F5A623", "bi-diagram-3"),
            ("SubWorkflowNode",  "Sub Workflow",     "Control", "Execute a child workflow and branch success/error",                        "#8e44ad", "bi-box-arrow-in-right"),
            ("DelayNode",        "Delay",            "Control", "Pause execution for a fixed duration",                                     "#F5A623", "bi-clock"),
            ("MergeNode",        "Merge",            "Control", "Merge multiple data keys into a single output key",                        "#F5A623", "bi-intersect"),
            ("LogNode",          "Log",              "Control", "Log a message or workflow state for debugging",                            "#F5A623", "bi-journal-text"),
            ("LoopNode",         "Loop (ForEach)",   "Control", "Iterate over each item in a collection",                                   "#f39c12", "bi-arrow-repeat"),
            ("ParallelNode",     "Parallel",         "Control", "Execute multiple branches simultaneously and wait for all",                "#9b59b6", "bi-lightning-charge"),
            ("BranchNode",       "Branch (Switch)",  "Control", "Route flow based on value matching against up to 3 cases",                "#e67e22", "bi-signpost-split"),
            ("WaitNode",         "Wait",             "Control", "Pause until a duration, event, or webhook resumes the workflow",           "#F5A623", "bi-hourglass-split"),
            ("RetryNode",        "Retry",            "Control", "Retry a sub-path with configurable backoff on failure",                   "#F5A623", "bi-arrow-counterclockwise"),
            ("TimeoutNode",      "Timeout",          "Control", "Abort a sub-path if it exceeds a time limit",                             "#F5A623", "bi-alarm"),
            ("EventTriggerNode", "Event Trigger",    "Control", "Emit or listen for a named workflow event",                               "#8e44ad", "bi-broadcast"),
            // Data
            ("TransformNode",      "Transform",       "Data", "Apply a custom expression to transform data",                              "#7ED321", "bi-arrow-left-right"),
            ("DataMapperNode",     "Data Mapper",     "Data", "Map output fields and paths to explicit input keys",                       "#7ED321", "bi-map"),
            ("FilterNode",         "Filter",          "Data", "Validate or filter data using a condition expression",                     "#7ED321", "bi-funnel"),
            ("ChunkTextNode",      "Chunk Text",      "Data", "Split text into overlapping chunks for RAG pipelines",                     "#7ED321", "bi-file-break"),
            ("MemoryNode",         "Memory",          "Data", "Read or write values from global workflow memory",                         "#7ED321", "bi-memory"),
            ("SetVariableNode",    "Set Variable",    "Data", "Write literal or interpolated values into workflow data keys",             "#7ED321", "bi-pencil"),
            ("ParseJsonNode",      "Parse JSON",      "Data", "Parse a raw JSON string into a structured object",                        "#7ED321", "bi-code-slash"),
            ("AggregateNode",      "Aggregate",       "Data", "Compute sum, count, avg, min, or max over a collection",                  "#7ED321", "bi-calculator"),
            ("SortNode",           "Sort",            "Data", "Sort a collection by a specified field",                                  "#7ED321", "bi-sort-down"),
            ("JoinNode",           "Join",            "Data", "Join two collections on a shared key field",                              "#7ED321", "bi-link-45deg"),
            ("SchemaValidateNode", "Schema Validate", "Data", "Validate data against a JSON Schema definition",                         "#7ED321", "bi-check2-square"),
            ("TemplateNode",       "Template",        "Data", "Render a Handlebars or Mustache template string",                         "#7ED321", "bi-file-earmark-code"),
            ("CsvParseNode",       "CSV Parse",       "Data", "Parse a CSV string into an array of row objects",                        "#7ED321", "bi-filetype-csv"),
            ("XmlParseNode",       "XML Parse",       "Data", "Parse an XML string into a JSON object",                                 "#7ED321", "bi-filetype-xml"),
            ("Base64Node",         "Base64",          "Data", "Encode or decode a value with Base64",                                   "#7ED321", "bi-file-binary"),
            ("HashNode",           "Hash",            "Data", "Compute MD5, SHA-256, SHA-512, or HMAC hash of a value",                 "#7ED321", "bi-hash"),
            ("DateTimeNode",       "Date & Time",     "Data", "Parse, format, or perform arithmetic on date/time values",               "#7ED321", "bi-calendar-event"),
            ("RandomNode",         "Random",          "Data", "Generate a UUID, random number, or pick from a list",                    "#7ED321", "bi-shuffle"),
            // IO
            ("HttpRequestNode",  "HTTP Request",   "IO", "Make a REST API call with configurable method and headers",                   "#BD10E0", "bi-globe"),
            ("HttpResponseNode", "HTTP Response",  "IO", "Return an HTTP response with status code and body",                          "#BD10E0", "bi-reply-fill"),
            ("DbQueryNode",      "DB Query",       "IO", "Execute SQL queries against a database connection",                          "#BD10E0", "bi-database"),
            ("FileReadNode",     "File Read",      "IO", "Read a file from the local file system",                                     "#BD10E0", "bi-file-earmark-text"),
            ("FileWriteNode",    "File Write",     "IO", "Write workflow data to a local file",                                        "#BD10E0", "bi-file-earmark-arrow-down"),
            ("EmailSendNode",    "Email Send",     "IO", "Send an email via SMTP, SendGrid, Mailgun, or SES",                          "#BD10E0", "bi-envelope"),
            ("WebhookNode",      "Webhook",        "IO", "Expose an HTTP endpoint that triggers this workflow",                        "#BD10E0", "bi-plug"),
            ("QueueNode",        "Queue",          "IO", "Publish or consume messages via RabbitMQ, Service Bus, or SQS",              "#BD10E0", "bi-collection"),
            ("CacheNode",        "Cache",          "IO", "Read or write values in Redis or in-memory cache with TTL",                  "#BD10E0", "bi-lightning"),
            ("NotificationNode", "Notification",   "IO", "Send a message to Slack, Teams, or Discord",                                "#BD10E0", "bi-bell"),
            ("StorageNode",      "Cloud Storage",  "IO", "Read or write objects in S3, Azure Blob, or GCS",                           "#BD10E0", "bi-cloud-arrow-up"),
            // Logic
            ("FunctionNode",    "Function",     "Logic", "Invoke a named function or method with parameters",                          "#20c997", "bi-braces"),
            ("ProcessNode",     "Process",      "Logic", "Execute a shell command or external process",                               "#20c997", "bi-terminal"),
            ("StepNode",        "Step",         "Logic", "A discrete named action step with success/failure routing",                 "#20c997", "bi-play-btn"),
            ("ScriptNode",      "Script",       "Logic", "Execute an inline JavaScript or Python code snippet",                       "#20c997", "bi-file-code"),
            ("RateLimiterNode", "Rate Limiter", "Logic", "Throttle execution with a token-bucket or fixed-window limit",              "#20c997", "bi-speedometer2"),
            // AI
            ("LlmNode",           "LLM",            "AI", "Send a prompt to a Large Language Model and get a response",              "#4A90E2", "bi-chat-left-dots"),
            ("PromptBuilderNode", "Prompt Builder", "AI", "Build a prompt from a template with {{variable}} placeholders",           "#4A90E2", "bi-pencil-square"),
            ("EmbeddingNode",     "Embedding",      "AI", "Generate vector embeddings for use in RAG pipelines",                     "#4A90E2", "bi-diagram-2"),
            ("OutputParserNode",  "Output Parser",  "AI", "Extract and parse structured JSON from LLM output",                      "#4A90E2", "bi-list-check"),
            ("VectorSearchNode",  "Vector Search",  "AI", "Perform semantic similarity search over a vector store",                 "#4A90E2", "bi-search"),
            ("AgentNode",         "Agent",          "AI", "ReAct-style AI agent with autonomous tool use and planning",             "#4A90E2", "bi-robot"),
            ("TextSplitterNode",  "Text Splitter",  "AI", "Split text using token-aware, markdown, or code strategies",             "#4A90E2", "bi-scissors"),
            ("SummariseNode",     "Summarise",      "AI", "Summarise documents using stuff, map-reduce, or refine",                 "#4A90E2", "bi-file-text"),
            // Visual
            ("ContainerNode", "Container", "Visual", "Logically group related nodes (visual only, no ports)", "#6366f1", "bi-bounding-box"),
            ("NoteNode",      "Note",      "Visual", "Sticky note annotation on the canvas",                  "#f59e0b", "bi-sticky"),
            ("AnchorNode",    "Anchor",    "Visual", "Named jump point to reduce edge spaghetti in large workflows", "#6366f1", "bi-geo-alt"),
        };
    }
}
