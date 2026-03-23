# TwfAiFramework

A lightweight, node-based AI workflow engine for .NET 10. Build AI pipelines by chaining reusable nodes — LLM calls, HTTP requests, data transforms, embeddings, conditionals, and more — with built-in retry, timeout, parallel execution, and structured logging.

Inspired by tools like [n8n](https://n8n.io/), but designed as a C# library you embed directly in your application.

---

## Getting Started

```csharp
var result = await Workflow.Create("CustomerSupportBot")
    .UseLogger(logger)
    .AddNode(new PromptBuilderNode("You are a helpful assistant. User asked: {{question}}"))
    .AddNode(new LlmNode(new LlmConfig
    {
        Provider = "openai",
        Model    = "gpt-4o",
        ApiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
    }))
    .AddNode(new OutputParserNode())
    .RunAsync(new WorkflowData().Set("question", "What is the weather today?"));

if (result.IsSuccess)
    Console.WriteLine(result.Data.GetString("llm_response"));
```

---

## Core Concepts

### WorkflowData

The data packet that flows between nodes. Like a typed dictionary — nodes read from it, transform it, and write results back.

```csharp
var data = new WorkflowData()
    .Set("name", "Alice")
    .Set("score", 42);

var name   = data.GetString("name");          // "Alice"
var score  = data.GetRequired<int>("score");  // 42
var copy   = data.Clone();                    // immutable snapshot
```

| Method | Description |
|---|---|
| `.Set(key, value)` | Write a value |
| `.Get<T>(key)` | Read a value (returns `default` if missing) |
| `.GetRequired<T>(key)` | Read or throw `KeyNotFoundException` |
| `.GetString(key)` | Shorthand for `Get<string>` |
| `.Has(key)` | Check if key exists and is non-null |
| `.Clone()` | Deep copy the data bag |
| `.Remove(key)` | Remove a key |
| `.Merge(other)` | Merge another `WorkflowData` in |
| `.Keys` | All current keys |

---

### WorkflowContext

Immutable runtime environment shared across all nodes in a single run. Provides:

- **`Logger`** — `ILogger` prefixed with `[WorkflowName][RunId]`
- **`Tracker`** — `ExecutionTracker` recording all node timings
- **`CancellationToken`** — propagated to every node
- **`RunId`** — unique 12-char ID for the run
- **Global state bag** — survives across nodes (unlike `WorkflowData` which is per-step)
- **Conversation history** — built-in chat history management for multi-turn bots

```csharp
// Global state (persists across all nodes)
context.SetState("user_id", "u-123");
var userId = context.GetState<string>("user_id");

// Conversation history
context.AppendMessage(ChatMessage.User("Hello"));
context.AppendMessage(ChatMessage.Assistant("Hi there!"));
var history = context.GetChatHistory();
```

---

### INode / BaseNode

Every unit of work implements `INode`. For custom nodes, subclass `BaseNode`:

```csharp
public class MyNode : BaseNode
{
    public override string Name     => "MyNode";
    public override string Category => "Data";

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        nodeCtx.Log("Processing...");
        nodeCtx.SetMetadata("processed_at", DateTime.UtcNow);

        var value = input.GetRequiredString("input_key");
        var result = value.ToUpper();

        return input.Clone().Set("output_key", result);
    }
}
```

`BaseNode` automatically handles:
- Execution timing
- Exception catching and wrapping into `NodeResult`
- Structured logging with node context
- Cancellation token forwarding

For single-key transforms, subclass `SimpleTransformNode` instead:

```csharp
public class UpperCaseNode : SimpleTransformNode
{
    public override string Name     => "UpperCase";
    public override string Category => "Data";
    protected override string InputKey  => "text";
    protected override string OutputKey => "text";

    protected override Task<object?> TransformAsync(object? input, WorkflowContext context)
        => Task.FromResult<object?>(input?.ToString()?.ToUpper());
}
```

---

### NodeOptions

Per-node configuration for retry, timeout, and conditional execution:

```csharp
// Retry up to 3 times with exponential backoff
.AddNode(new LlmNode(config), NodeOptions.WithRetry(3))

// Set a 10-second timeout
.AddNode(new HttpRequestNode("FetchData", httpConfig), NodeOptions.WithTimeout(TimeSpan.FromSeconds(10)))

// Skip if a condition is false
.AddNode(new MyNode(), new NodeOptions
{
    RunCondition = data => data.GetString("mode") == "advanced"
})

// Continue pipeline even if this node fails
.AddNode(new OptionalNode(), new NodeOptions { ContinueOnError = true })
```

---

## Built-in Nodes

### AI Nodes (`TwfAiFramework.Nodes.AI`)

#### `LlmNode`
Calls any OpenAI-compatible LLM API (OpenAI, Azure OpenAI, Anthropic, Ollama, etc.).

```csharp
new LlmNode(new LlmConfig
{
    Provider            = "openai",
    Model               = "gpt-4o",
    ApiKey              = "sk-...",
    DefaultSystemPrompt = "You are a helpful assistant.",
    MaintainHistory     = true,   // enables multi-turn conversation
    Temperature         = 0.7f,
    MaxTokens           = 1000
})
```

**Reads:** `prompt` or `messages`, `system_prompt`  
**Writes:** `llm_response`, `llm_model`, `prompt_tokens`, `completion_tokens`

---

#### `PromptBuilderNode`
Builds dynamic prompts from `{{variable}}` templates.

```csharp
new PromptBuilderNode(
    promptTemplate: "Summarise the following text in {{language}}: {{content}}",
    systemTemplate: "You are an expert summariser."
)
```

**Reads:** Keys matching `{{variable}}` names in the template  
**Writes:** `prompt`, `system_prompt`

---

#### `OutputParserNode`
Extracts structured JSON from LLM responses (handles markdown code fences automatically).

```csharp
// Map specific JSON keys to WorkflowData keys
new OutputParserNode(fieldMapping: new()
{
    ["sentiment"] = "sentiment",
    ["score"]     = "anger_score"
})

// Or extract all JSON keys directly (no mapping)
new OutputParserNode()
```

**Reads:** `llm_response`  
**Writes:** `parsed_output`, plus individual mapped fields

---

#### `EmbeddingNode`
Generates vector embeddings for RAG pipelines and semantic search.

```csharp
new EmbeddingNode(new EmbeddingConfig
{
    Model  = "text-embedding-3-small",
    ApiKey = "sk-...",
    ApiUrl = "https://api.openai.com/v1/embeddings"
})
```

**Reads:** `text` (single string) or `texts` (`List<string>`)  
**Writes:** `embedding` (`float[]`) or `embeddings` (`List<float[]>`), `embedding_model`

---

### IO Nodes (`TwfAiFramework.Nodes.IO`)

#### `HttpRequestNode`
Makes HTTP requests — REST APIs, webhooks, data sources.

```csharp
new HttpRequestNode("FetchUser", new HttpRequestConfig
{
    Method      = "GET",
    UrlTemplate = "https://api.example.com/users/{{user_id}}",
    Headers     = new() { ["Authorization"] = "Bearer token" },
    Timeout     = TimeSpan.FromSeconds(30),
    ThrowOnError = true
})
```

URL template supports `{{variable}}` substitution from `WorkflowData`.  
**Writes:** `http_response`, `http_status_code`, `http_headers`

---

### Data Nodes (`TwfAiFramework.Nodes.Data`)

#### `TransformNode`
Applies custom data transformations.

```csharp
// Inline lambda
new TransformNode("Normalize", data =>
    data.Set("name", data.GetString("name")?.ToLower()))

// Prebuilt transforms
TransformNode.Rename("old_key", "new_key")
TransformNode.SelectKey("nested.value", "flat_value")
TransformNode.ConcatStrings("full_name", " ", "first_name", "last_name")
```

#### `FilterNode`
Validates `WorkflowData` against conditions — stops the pipeline or sets an `is_valid` flag on failure.

---

### Control Nodes (`TwfAiFramework.Nodes.Control`)

#### `ConditionNode`
Evaluates predicates and writes boolean results to `WorkflowData`. Use with `Workflow.Branch()`.

```csharp
new ConditionNode("CheckSentiment",
    ("is_positive",      data => data.GetString("sentiment") == "positive"),
    ("needs_escalation", data => data.Get<int>("anger_score") > 7))

// Prebuilt factories
ConditionNode.HasKey("has_email", "email")
ConditionNode.StringEquals("is_english", "language", "en")
ConditionNode.LengthExceeds("too_long", "content", 500)
```

#### `DelayNode`
Inserts a delay — useful for rate limiting.

```csharp
DelayNode.Milliseconds(500)
DelayNode.Seconds(2, reason: "Rate limit pause")
DelayNode.RateLimitDelay(requestsPerMinute: 20)
```

#### `MergeNode`
Merges multiple `WorkflowData` keys into a single aggregated value.

---

## Workflow Features

### Inline Steps (no class needed)

```csharp
.AddStep("Sanitize", async (data, ctx) =>
{
    var text = data.GetRequiredString("input");
    return data.Clone().Set("input", text.Trim());
})
```

### Conditional Branching

```csharp
.Branch(
    condition:   data => data.Get<bool>("is_positive"),
    trueBranch:  b => b.AddNode(new PositiveResponseNode()),
    falseBranch: b => b.AddNode(new EscalationNode())
)
```

### Parallel Execution

```csharp
.Parallel(
    new SentimentNode(),
    new KeywordExtractorNode(),
    new CategoryClassifierNode()
)
// Results merged back (later keys overwrite earlier ones)
```

### Loop (ForEach)

```csharp
.ForEach(
    itemsKey:  "documents",
    outputKey: "summaries",
    bodyBuilder: b => b
        .AddNode(new PromptBuilderNode("Summarise: {{item}}"))
        .AddNode(new LlmNode(config))
)
```

### Error Handling

```csharp
Workflow.Create("MyBot")
    .ContinueOnErrors()            // global: never stop on failure
    .OnError((nodeName, ex) =>
    {
        Console.WriteLine($"Node {nodeName} failed: {ex?.Message}");
    })
    .OnComplete(result =>
    {
        Console.WriteLine($"Done in {result.TotalDuration.TotalSeconds:F2}s");
    })
```

---

## Execution Results

```csharp
var result = await workflow.RunAsync(initialData);

result.IsSuccess          // bool
result.WorkflowName       // string
result.RunId              // "A3F7B2..."
result.TotalDuration      // TimeSpan
result.Data               // final WorkflowData
result.NodeResults        // per-node results with timing + logs
result.FailedNodeName     // which node caused failure
result.ErrorMessage       // error message
result.Report             // WorkflowReport with full breakdown
```

### WorkflowReport

```csharp
var report = result.Report;
report.TotalNodes     // int
report.SuccessCount   // int
report.FailureCount   // int
report.SkippedCount   // int
report.TotalDuration  // TimeSpan
report.NodeBreakdown  // List<NodeTimingEntry> with per-node ms, status, errors
```

---

## Project Structure

```
source/
└── core/
    ├── INode.cs                 # INode interface, NodeStatus enum
    ├── BaseNode.cs              # Abstract base + NodeExecutionContext + SimpleTransformNode
    ├── WorkflowBuilder.cs       # Workflow fluent builder and execution engine
    ├── WorkflowContext.cs       # Runtime context (logger, tracker, global state, chat history)
    ├── WorkflowData.cs          # Per-step data packet
    ├── WorkflowResult.cs        # Final workflow result
    ├── NodeResult.cs            # Per-node result + fluent OnSuccess/OnFailure
    ├── NodeOptions.cs           # Per-node retry/timeout/condition config
    ├── ExecutionTracker.cs      # Node timing recorder + report generator
    ├── LlmNode.cs               # LLM API calls
    ├── PromptBuilderNode.cs     # Prompt template rendering
    ├── OutputParserNode.cs      # LLM JSON output parsing
    ├── EmbeddingNode.cs         # Vector embedding generation
    ├── IONodes.cs               # HTTP request node
    ├── DataNodes.cs             # Transform and filter nodes
    └── ControlNodes.cs          # Condition, delay, merge nodes
```

---

## Requirements

- .NET 10
- `Microsoft.Extensions.Logging` 10.0.5

---

## License

Apache 2.0 — see [LICENSE](LICENSE).
