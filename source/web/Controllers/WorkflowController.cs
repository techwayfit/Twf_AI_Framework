using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TwfAiFramework.Core;
using TwfAiFramework.Web.Models;
using TwfAiFramework.Web.Repositories;
using TwfAiFramework.Web.Services;

namespace TwfAiFramework.Web.Controllers;

public class WorkflowController : Controller
{
    private readonly IWorkflowRepository _repository;
    private readonly WorkflowDefinitionRunner _runner;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IWorkflowRepository repository,
        WorkflowDefinitionRunner runner,
        ILogger<WorkflowController> logger)
    {
        _repository = repository;
        _runner = runner;
        _logger = logger;
    }

    // GET: /Workflow
    public async Task<IActionResult> Index(string? search)
    {
        IEnumerable<WorkflowDefinition> workflows;

        if (!string.IsNullOrWhiteSpace(search))
        {
            workflows = await _repository.SearchAsync(search);
            ViewBag.SearchQuery = search;
        }
        else
        {
            workflows = await _repository.GetAllAsync();
        }

        return View(workflows);
    }

    // GET: /Workflow/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        return View(workflow);
    }

    // GET: /Workflow/Create
    public IActionResult Create()
    {
        return View(new WorkflowDefinition { Name = "New Workflow" });
    }

    // POST: /Workflow/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkflowDefinition workflow)
    {
        if (ModelState.IsValid)
        {
            await _repository.CreateAsync(workflow);
            return RedirectToAction(nameof(Designer), new { id = workflow.Id });
        }

        return View(workflow);
    }

    // GET: /Workflow/Designer/{id}
    // GET: /{id}/{subWorkflowId}
    public async Task<IActionResult> Designer(Guid id, Guid? subWorkflowId = null)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        if (subWorkflowId.HasValue)
        {
            var exists = workflow.SubWorkflows.Any(sw => sw.Id == subWorkflowId.Value);
            if (exists)
            {
                ViewBag.InitialSubWorkflowId = subWorkflowId.Value;
            }
        }

        return View(workflow);
    }

    // GET: /Workflow/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        return View(workflow);
    }

    // POST: /Workflow/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, WorkflowDefinition workflow)
    {
        if (id != workflow.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _repository.UpdateAsync(workflow);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
                ModelState.AddModelError("", "An error occurred while updating the workflow.");
            }
        }

        return View(workflow);
    }

    // GET: /Workflow/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        return View(workflow);
    }

    // POST: /Workflow/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _repository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // API endpoints for the designer

    // GET: /Workflow/GetWorkflow/5
    [HttpGet]
    public async Task<IActionResult> GetWorkflow(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        return Json(workflow);
    }

    // POST: /Workflow/SaveWorkflow
    [HttpPost]
    public async Task<IActionResult> SaveWorkflow([FromBody] WorkflowDefinition? workflow)
    {
        if (workflow == null)
            return Json(new { success = false, error = "Invalid workflow payload. Node and connection IDs must be valid GUIDs." });

        try
        {
            if (workflow.Id == Guid.Empty)
            {
                await _repository.CreateAsync(workflow);
            }
            else
            {
                await _repository.UpdateAsync(workflow);
            }

            return Json(new { success = true, id = workflow.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workflow");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // GET: /Workflow/GetAvailableNodes
    [HttpGet]
    public IActionResult GetAvailableNodes()
    {
        var nodes = new[]
                {
   // Control Nodes
    new { type = "StartNode", category = "Control", name = "Start", description = "Workflow entry point (required)", color = "#2ecc71", icon = "bi-play-circle-fill" },
     new { type = "EndNode", category = "Control", name = "End", description = "Workflow exit point", color = "#e74c3c", icon = "bi-stop-circle-fill" },
      new { type = "ErrorNode", category = "Control", name = "Error Handler", description = "Workflow-level error entry point (max 1 per workflow)", color = "#e74c3c", icon = "bi-exclamation-triangle-fill" },
       new { type = "ConditionNode", category = "Control", name = "Condition", description = "Evaluate a boolean expression and route success or failure", color = "#F5A623", icon = "bi-question-diamond" },
       new { type = "SwitchNode", category = "Control", name = "Switch", description = "Route to first matching boolean condition (up to 4 cases)", color = "#F5A623", icon = "bi-diagram-3" },
       new { type = "SubWorkflowNode", category = "Control", name = "Sub Workflow", description = "Execute a child workflow and branch success/error", color = "#8e44ad", icon = "bi-box-arrow-in-right" },
       new { type = "DelayNode", category = "Control", name = "Delay", description = "Pause execution for a fixed duration", color = "#F5A623", icon = "bi-clock" },
         new { type = "MergeNode", category = "Control", name = "Merge", description = "Merge multiple data keys into a single output key", color = "#F5A623", icon = "bi-intersect" },
    new { type = "LogNode", category = "Control", name = "Log", description = "Log a message or workflow state for debugging", color = "#F5A623", icon = "bi-journal-text" },
        new { type = "LoopNode", category = "Control", name = "Loop (ForEach)", description = "Iterate over each item in a collection", color = "#f39c12", icon = "bi-arrow-repeat" },
  new { type = "ParallelNode", category = "Control", name = "Parallel", description = "Execute multiple branches simultaneously and wait for all", color = "#9b59b6", icon = "bi-lightning-charge" },
  new { type = "BranchNode", category = "Control", name = "Branch (Switch)", description = "Route flow based on value matching against up to 3 cases", color = "#e67e22", icon = "bi-signpost-split" },
  new { type = "WaitNode", category = "Control", name = "Wait", description = "Pause until a duration, event, or webhook resumes the workflow", color = "#F5A623", icon = "bi-hourglass-split" },
  new { type = "RetryNode", category = "Control", name = "Retry", description = "Retry a sub-path with configurable backoff on failure", color = "#F5A623", icon = "bi-arrow-counterclockwise" },
  new { type = "TimeoutNode", category = "Control", name = "Timeout", description = "Abort a sub-path if it exceeds a time limit", color = "#F5A623", icon = "bi-alarm" },
  new { type = "EventTriggerNode", category = "Control", name = "Event Trigger", description = "Emit or listen for a named workflow event", color = "#8e44ad", icon = "bi-broadcast" },
  // Data Nodes
    new { type = "TransformNode", category = "Data", name = "Transform", description = "Apply a custom expression to transform data", color = "#7ED321", icon = "bi-arrow-left-right" },
 new { type = "DataMapperNode", category = "Data", name = "Data Mapper", description = "Map output fields and paths to explicit input keys", color = "#7ED321", icon = "bi-map" },
 new { type = "FilterNode", category = "Data", name = "Filter", description = "Validate or filter data using a condition expression", color = "#7ED321", icon = "bi-funnel" },
            new { type = "ChunkTextNode", category = "Data", name = "Chunk Text", description = "Split text into overlapping chunks for RAG pipelines", color = "#7ED321", icon = "bi-file-break" },
     new { type = "MemoryNode", category = "Data", name = "Memory", description = "Read or write values from global workflow memory", color = "#7ED321", icon = "bi-memory" },
     new { type = "SetVariableNode", category = "Data", name = "Set Variable", description = "Write literal or interpolated values into workflow data keys", color = "#7ED321", icon = "bi-pencil" },
     new { type = "ParseJsonNode", category = "Data", name = "Parse JSON", description = "Parse a raw JSON string into a structured object", color = "#7ED321", icon = "bi-code-slash" },
     new { type = "AggregateNode", category = "Data", name = "Aggregate", description = "Compute sum, count, avg, min, or max over a collection", color = "#7ED321", icon = "bi-calculator" },
     new { type = "SortNode", category = "Data", name = "Sort", description = "Sort a collection by a specified field", color = "#7ED321", icon = "bi-sort-down" },
     new { type = "JoinNode", category = "Data", name = "Join", description = "Join two collections on a shared key field", color = "#7ED321", icon = "bi-link-45deg" },
     new { type = "SchemaValidateNode", category = "Data", name = "Schema Validate", description = "Validate data against a JSON Schema definition", color = "#7ED321", icon = "bi-check2-square" },
     new { type = "TemplateNode", category = "Data", name = "Template", description = "Render a Handlebars or Mustache template string", color = "#7ED321", icon = "bi-file-earmark-code" },
     new { type = "CsvParseNode", category = "Data", name = "CSV Parse", description = "Parse a CSV string into an array of row objects", color = "#7ED321", icon = "bi-filetype-csv" },
     new { type = "XmlParseNode", category = "Data", name = "XML Parse", description = "Parse an XML string into a JSON object", color = "#7ED321", icon = "bi-filetype-xml" },
     new { type = "Base64Node", category = "Data", name = "Base64", description = "Encode or decode a value with Base64", color = "#7ED321", icon = "bi-file-binary" },
     new { type = "HashNode", category = "Data", name = "Hash", description = "Compute MD5, SHA-256, SHA-512, or HMAC hash of a value", color = "#7ED321", icon = "bi-hash" },
     new { type = "DateTimeNode", category = "Data", name = "Date & Time", description = "Parse, format, or perform arithmetic on date/time values", color = "#7ED321", icon = "bi-calendar-event" },
     new { type = "RandomNode", category = "Data", name = "Random", description = "Generate a UUID, random number, or pick from a list", color = "#7ED321", icon = "bi-shuffle" },

            // IO Nodes
     new { type = "HttpRequestNode", category = "IO", name = "HTTP Request", description = "Make a REST API call with configurable method and headers", color = "#BD10E0", icon = "bi-globe" },
     new { type = "HttpResponseNode", category = "IO", name = "HTTP Response", description = "Return an HTTP response with status code and body", color = "#BD10E0", icon = "bi-reply-fill" },
     new { type = "DbQueryNode", category = "IO", name = "DB Query", description = "Execute SQL queries against a database connection", color = "#BD10E0", icon = "bi-database" },
     new { type = "FileReadNode", category = "IO", name = "File Read", description = "Read a file from the local file system", color = "#BD10E0", icon = "bi-file-earmark-text" },
     new { type = "FileWriteNode", category = "IO", name = "File Write", description = "Write workflow data to a local file", color = "#BD10E0", icon = "bi-file-earmark-arrow-down" },
     new { type = "EmailSendNode", category = "IO", name = "Email Send", description = "Send an email via SMTP, SendGrid, Mailgun, or SES", color = "#BD10E0", icon = "bi-envelope" },
     new { type = "WebhookNode", category = "IO", name = "Webhook", description = "Expose an HTTP endpoint that triggers this workflow", color = "#BD10E0", icon = "bi-plug" },
     new { type = "QueueNode", category = "IO", name = "Queue", description = "Publish or consume messages via RabbitMQ, Service Bus, or SQS", color = "#BD10E0", icon = "bi-collection" },
     new { type = "CacheNode", category = "IO", name = "Cache", description = "Read or write values in Redis or in-memory cache with TTL", color = "#BD10E0", icon = "bi-lightning" },
     new { type = "NotificationNode", category = "IO", name = "Notification", description = "Send a message to Slack, Teams, or Discord", color = "#BD10E0", icon = "bi-bell" },
     new { type = "StorageNode", category = "IO", name = "Cloud Storage", description = "Read or write objects in S3, Azure Blob, or GCS", color = "#BD10E0", icon = "bi-cloud-arrow-up" },

            // Logic Nodes
            new { type = "FunctionNode", category = "Logic", name = "Function", description = "Invoke a named function or method with parameters", color = "#20c997", icon = "bi-braces" },
            new { type = "ProcessNode", category = "Logic", name = "Process", description = "Execute a shell command or external process", color = "#20c997", icon = "bi-terminal" },
            new { type = "StepNode", category = "Logic", name = "Step", description = "A discrete named action step with success/failure routing", color = "#20c997", icon = "bi-play-btn" },
            new { type = "ScriptNode", category = "Logic", name = "Script", description = "Execute an inline JavaScript or Python code snippet", color = "#20c997", icon = "bi-file-code" },
            new { type = "RateLimiterNode", category = "Logic", name = "Rate Limiter", description = "Throttle execution with a token-bucket or fixed-window limit", color = "#20c997", icon = "bi-speedometer2" },

            // AI Nodes
new { type = "LlmNode", category = "AI", name = "LLM", description = "Send a prompt to a Large Language Model and get a response", color = "#4A90E2", icon = "bi-chat-left-dots" },
   new { type = "PromptBuilderNode", category = "AI", name = "Prompt Builder", description = "Build a prompt from a template with {{variable}} placeholders", color = "#4A90E2", icon = "bi-pencil-square" },
      new { type = "EmbeddingNode", category = "AI", name = "Embedding", description = "Generate vector embeddings for use in RAG pipelines", color = "#4A90E2", icon = "bi-diagram-2" },
  new { type = "OutputParserNode", category = "AI", name = "Output Parser", description = "Extract and parse structured JSON from LLM output", color = "#4A90E2", icon = "bi-list-check" },
  new { type = "VectorSearchNode", category = "AI", name = "Vector Search", description = "Perform semantic similarity search over a vector store", color = "#4A90E2", icon = "bi-search" },
  new { type = "AgentNode", category = "AI", name = "Agent", description = "ReAct-style AI agent with autonomous tool use and planning", color = "#4A90E2", icon = "bi-robot" },
  new { type = "TextSplitterNode", category = "AI", name = "Text Splitter", description = "Split text using token-aware, markdown, or code strategies", color = "#4A90E2", icon = "bi-scissors" },
  new { type = "SummariseNode", category = "AI", name = "Summarise", description = "Summarise documents using stuff, map-reduce, or refine", color = "#4A90E2", icon = "bi-file-text" },

            // Visual Nodes
            new { type = "ContainerNode", category = "Visual", name = "Container", description = "Logically group related nodes (visual only, no ports)", color = "#6366f1", icon = "bi-bounding-box" },
            new { type = "NoteNode", category = "Visual", name = "Note", description = "Sticky note annotation on the canvas", color = "#f59e0b", icon = "bi-sticky" },
            new { type = "AnchorNode", category = "Visual", name = "Anchor", description = "Named jump point to reduce edge spaghetti in large workflows", color = "#6366f1", icon = "bi-geo-alt" },
        };

        return Json(nodes);
    }

    // GET: /Workflow/GetNodeSchema/{nodeType}
    [HttpGet]
    public IActionResult GetNodeSchema(string nodeType)
    {
        var schemas = NodeSchemaProvider.GetAllSchemas();
        var schema = schemas[nodeType];
        if (schema == null)
        {
            return NotFound(new { error = $"Schema for node type '{nodeType}' not found" });
        }

        return Json(schema);
    }

    // GET: /Workflow/GetAllNodeSchemas
    [HttpGet]
    public IActionResult GetAllNodeSchemas()
    {
        var schemas = NodeSchemaProvider.GetAllSchemas();
        return Json(schemas);
    }

    // ─── Workflow Execution ───────────────────────────────────────────────────

    // GET: /Workflow/Runner/{id}
    public async Task<IActionResult> Runner(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow is null) return NotFound();
        return View(workflow);
    }

    // POST: /Workflow/RunStream/{id}
    // Streams per-node execution events as Server-Sent Events (text/event-stream).
    [HttpPost]
    public async Task RunStream(Guid id, [FromBody] WorkflowRunRequest? request = null)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow is null)
        {
            Response.StatusCode = 404;
            return;
        }

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no"; // disable Nginx proxy buffering

        var ct = HttpContext.RequestAborted;

        var initialData = new WorkflowData();
        if (request?.InitialData is { Count: > 0 } seed)
        {
            foreach (var (k, v) in seed)
                initialData.Set(k, v);
        }

        var sseJsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        async Task WriteEvent(string eventName, object payload)
        {
            if (ct.IsCancellationRequested) return;
            var json = JsonSerializer.Serialize(payload, sseJsonOptions);
            await Response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }

        try
        {
            var result = await _runner.RunWithCallbackAsync(
                workflow,
                initialData,
                step => WriteEvent(step.EventType, step));

            var finalEvent = result.IsSuccess ? "workflow_done" : "workflow_error";
            await WriteEvent(finalEvent, result);
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — nothing to emit
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error streaming workflow {WorkflowId}", id);
            try
            {
                await WriteEvent("workflow_error",
                    WorkflowRunResult.Failure(workflow.Name, new WorkflowData(), ex.Message, null));
            }
            catch { /* response may already be gone */ }
        }
    }

    // POST: /Workflow/Run/{id}
    // Body (optional JSON): { "initialData": { "key": "value" } }
    //
    // Loads the saved WorkflowDefinition and runs it through the core engine.
    // Returns a WorkflowRunResult with success/failure, output data, and error details.
    [HttpPost]
    public async Task<IActionResult> Run(Guid id, [FromBody] WorkflowRunRequest? request = null)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow is null)
            return NotFound(new { error = $"Workflow {id} not found." });

        var initialData = new WorkflowData();
        if (request?.InitialData is { Count: > 0 } seed)
        {
            foreach (var (k, v) in seed)
                initialData.Set(k, v);
        }

        try
        {
            var result = await _runner.RunAsync(workflow, initialData);
            var status = result.IsSuccess ? 200 : 422;
            return StatusCode(status, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error running workflow {WorkflowId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

