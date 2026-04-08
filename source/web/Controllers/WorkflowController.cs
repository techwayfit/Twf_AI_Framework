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
    private readonly INodeTypeRepository _nodeTypeRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly WorkflowDefinitionRunner _runner;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IWorkflowRepository repository,
        INodeTypeRepository nodeTypeRepository,
        IWorkflowInstanceRepository instanceRepository,
        WorkflowDefinitionRunner runner,
        ILogger<WorkflowController> logger)
    {
        _repository = repository;
        _nodeTypeRepository = nodeTypeRepository;
        _instanceRepository = instanceRepository;
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
    public async Task<IActionResult> GetAvailableNodes()
    {
        var nodes = await _nodeTypeRepository.GetAllAsync();
        var result = nodes
            .Where(n => n.IsEnabled)
            .Select(n => new
            {
                type        = n.NodeType,
                category    = n.Category,
                name        = n.Name,
                description = n.Description,
                color       = n.Color,
                icon        = n.Icon,
            });
        return Json(result);
    }

    // GET: /Workflow/GetNodeSchema/{nodeType}
    [HttpGet]
    public async Task<IActionResult> GetNodeSchema(string nodeType)
    {
        var entity = await _nodeTypeRepository.GetByNodeTypeAsync(nodeType);
        if (entity == null)
            return NotFound(new { error = $"Schema for node type '{nodeType}' not found" });

        // Deserialize stored schema JSON back to the typed object so the response
        // shape is identical to the old static provider.
        var schema = DeserializeSchema(entity);
        return Json(schema);
    }

    // GET: /Workflow/GetAllNodeSchemas
    [HttpGet]
    public async Task<IActionResult> GetAllNodeSchemas()
    {
        var nodes = await _nodeTypeRepository.GetAllAsync();
        var schemas = nodes.ToDictionary(n => n.NodeType, DeserializeSchema);
        return Json(schemas);
    }

    private static readonly JsonSerializerOptions _schemaReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static NodeParameterSchema? DeserializeSchema(Data.NodeTypeEntity entity)
    {
        try
        {
            return JsonSerializer.Deserialize<NodeParameterSchema>(entity.SchemaJson, _schemaReadOptions);
        }
        catch
        {
            return null;
        }
    }

    // ─── Workflow Execution ───────────────────────────────────────────────────

    // GET: /Workflow/Runner/{id}
    public async Task<IActionResult> Runner(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow is null) return NotFound();
        return View(workflow);
    }

    // GET: /Workflow/Runs/{id}
    public async Task<IActionResult> Runs(Guid id)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow is null) return NotFound();

        var instances = await _instanceRepository.GetByWorkflowIdAsync(id);
        ViewBag.WorkflowName = workflow.Name;
        ViewBag.WorkflowId   = id;
        return View(instances);
    }

    // GET: /Workflow/RunDetail/{instanceId}
    public async Task<IActionResult> RunDetail(Guid instanceId)
    {
        var instance = await _instanceRepository.GetByIdAsync(instanceId);
        if (instance is null) return NotFound();
        return View(instance);
    }

    // POST: /Workflow/RunStream/{id}
    // Streams per-node execution events as Server-Sent Events (text/event-stream).
    // Creates a WorkflowInstance before running and updates it on completion.
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

        var seedDict = request?.InitialData ?? new Dictionary<string, object?>();
        var initialData = new WorkflowData();
        foreach (var (k, v) in seedDict)
            initialData.Set(k, v);

        // ── Create instance record ────────────────────────────────────────────
        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = workflow.Id,
            WorkflowName         = workflow.Name,
            Status               = WorkflowRunStatus.Pending,
            StartedAt            = DateTime.UtcNow,
            InitialData          = seedDict
        };
        await _instanceRepository.CreateAsync(instance);

        instance.Status = WorkflowRunStatus.Running;
        await _instanceRepository.UpdateAsync(instance);
        _logger.LogInformation("▶ WorkflowInstance {InstanceId} created for '{Name}'", instance.Id, workflow.Name);

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
                async step =>
                {
                    // Log each step and accumulate in the instance
                    var level = step.EventType == "node_error" ? "WARN" : "INFO";
                    _logger.LogInformation(
                        "[{Level}] {EventType} — {NodeType} '{NodeName}'{Error}",
                        level, step.EventType, step.NodeType, step.NodeName,
                        step.ErrorMessage is null ? "" : $" | {step.ErrorMessage}");

                    instance.Steps.Add(step);
                    await WriteEvent(step.EventType, step);
                });

            instance.Status      = result.IsSuccess ? WorkflowRunStatus.Completed : WorkflowRunStatus.Failed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.OutputData  = result.OutputData;
            instance.ErrorMessage   = result.ErrorMessage;
            instance.FailedNodeName = result.FailedNodeName;
            await _instanceRepository.UpdateAsync(instance);

            _logger.LogInformation(
                "{Status} WorkflowInstance {InstanceId} for '{Name}'",
                instance.Status, instance.Id, workflow.Name);

            var finalEvent = result.IsSuccess ? "workflow_done" : "workflow_error";
            await WriteEvent(finalEvent, new { result, instanceId = instance.Id });
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — mark instance as failed
            instance.Status      = WorkflowRunStatus.Failed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.ErrorMessage = "Client disconnected";
            await _instanceRepository.UpdateAsync(instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error streaming workflow {WorkflowId}", id);

            instance.Status      = WorkflowRunStatus.Failed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.ErrorMessage = ex.Message;
            await _instanceRepository.UpdateAsync(instance);

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
    // Loads the saved WorkflowDefinition, creates a WorkflowInstance, runs it,
    // persists the outcome, and returns a WorkflowRunResult.
    [HttpPost]
    public async Task<IActionResult> Run(Guid id, [FromBody] WorkflowRunRequest? request = null)
    {
        var workflow = await _repository.GetByIdAsync(id);
        if (workflow is null)
            return NotFound(new { error = $"Workflow {id} not found." });

        var seedDict = request?.InitialData ?? new Dictionary<string, object?>();
        var initialData = new WorkflowData();
        foreach (var (k, v) in seedDict)
            initialData.Set(k, v);

        // ── Create instance record ────────────────────────────────────────────
        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = workflow.Id,
            WorkflowName         = workflow.Name,
            Status               = WorkflowRunStatus.Running,
            StartedAt            = DateTime.UtcNow,
            InitialData          = seedDict
        };
        await _instanceRepository.CreateAsync(instance);
        _logger.LogInformation("▶ WorkflowInstance {InstanceId} created for '{Name}'", instance.Id, workflow.Name);

        try
        {
            var result = await _runner.RunWithCallbackAsync(
                workflow,
                initialData,
                step =>
                {
                    _logger.LogInformation(
                        "{EventType} — {NodeType} '{NodeName}'{Error}",
                        step.EventType, step.NodeType, step.NodeName,
                        step.ErrorMessage is null ? "" : $" | {step.ErrorMessage}");

                    instance.Steps.Add(step);
                    return Task.CompletedTask;
                });

            instance.Status         = result.IsSuccess ? WorkflowRunStatus.Completed : WorkflowRunStatus.Failed;
            instance.CompletedAt    = DateTime.UtcNow;
            instance.OutputData     = result.OutputData;
            instance.ErrorMessage   = result.ErrorMessage;
            instance.FailedNodeName = result.FailedNodeName;
            await _instanceRepository.UpdateAsync(instance);

            _logger.LogInformation(
                "{Status} WorkflowInstance {InstanceId} for '{Name}'",
                instance.Status, instance.Id, workflow.Name);

            var statusCode = result.IsSuccess ? 200 : 422;
            return StatusCode(statusCode, new { result, instanceId = instance.Id });
        }
        catch (Exception ex)
        {
            instance.Status      = WorkflowRunStatus.Failed;
            instance.CompletedAt = DateTime.UtcNow;
            instance.ErrorMessage = ex.Message;
            await _instanceRepository.UpdateAsync(instance);

            _logger.LogError(ex, "Unhandled error running workflow {WorkflowId}", id);
            return StatusCode(500, new { error = ex.Message, instanceId = instance.Id });
        }
    }
}

