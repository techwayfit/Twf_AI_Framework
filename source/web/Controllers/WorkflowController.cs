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
    private readonly WorkflowDefinitionRunner _runner;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(
        IWorkflowRepository repository,
        INodeTypeRepository nodeTypeRepository,
        WorkflowDefinitionRunner runner,
        ILogger<WorkflowController> logger)
    {
        _repository = repository;
        _nodeTypeRepository = nodeTypeRepository;
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

