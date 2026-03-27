using Microsoft.AspNetCore.Mvc;
using TwfAiFramework.Web.Models;
using TwfAiFramework.Web.Repositories;
using TwfAiFramework.Web.Services;

namespace TwfAiFramework.Web.Controllers;

public class WorkflowController : Controller
{
    private readonly IWorkflowRepository _repository;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(IWorkflowRepository repository, ILogger<WorkflowController> logger)
    {
        _repository = repository;
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
    public async Task<IActionResult> SaveWorkflow([FromBody] WorkflowDefinition workflow)
    {
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
     // AI Nodes
new { type = "LlmNode", category = "AI", name = "LLM", description = "Large Language Model API call", color = "#4A90E2" },
   new { type = "PromptBuilderNode", category = "AI", name = "Prompt Builder", description = "Build prompts from {{variable}} templates", color = "#4A90E2" },
      new { type = "EmbeddingNode", category = "AI", name = "Embedding", description = "Generate vector embeddings for RAG", color = "#4A90E2" },
  new { type = "OutputParserNode", category = "AI", name = "Output Parser", description = "Parse JSON from LLM output", color = "#4A90E2" },
      
   // Control Nodes
    new { type = "StartNode", category = "Control", name = "Start", description = "Workflow entry point (required)", color = "#2ecc71" },
     new { type = "EndNode", category = "Control", name = "End", description = "Workflow exit point", color = "#e74c3c" },
      new { type = "ErrorNode", category = "Control", name = "Error Handler", description = "Workflow-level error entry point (max 1 per workflow)", color = "#e74c3c" },
       new { type = "ConditionNode", category = "Control", name = "Condition", description = "Evaluate conditions and write boolean flags", color = "#F5A623" },
       new { type = "SubWorkflowNode", category = "Control", name = "Sub Workflow", description = "Execute a child workflow and branch success/error", color = "#8e44ad" },
       new { type = "DelayNode", category = "Control", name = "Delay", description = "Add delay for rate limiting", color = "#F5A623" },
         new { type = "MergeNode", category = "Control", name = "Merge", description = "Merge multiple keys into one", color = "#F5A623" },
    new { type = "LogNode", category = "Control", name = "Log", description = "Log workflow state for debugging", color = "#F5A623" },
            
     // Special Control Nodes (Phase 5: Container Nodes)
        new { type = "LoopNode", category = "Control", name = "Loop (ForEach)", description = "Iterate over collection items", color = "#f39c12" },
  new { type = "ParallelNode", category = "Control", name = "Parallel", description = "Execute multiple branches simultaneously", color = "#9b59b6" },
  new { type = "BranchNode", category = "Control", name = "Branch (Switch)", description = "Route based on value matching", color = "#e67e22" },
  // Data Nodes
    new { type = "TransformNode", category = "Data", name = "Transform", description = "Apply custom data transformation", color = "#7ED321" },
 new { type = "DataMapperNode", category = "Data", name = "Data Mapper", description = "Map output fields/paths to explicit input keys", color = "#7ED321" },
 new { type = "FilterNode", category = "Data", name = "Filter", description = "Validate data with conditions", color = "#7ED321" },
            new { type = "ChunkTextNode", category = "Data", name = "Chunk Text", description = "Split text into chunks for RAG", color = "#7ED321" },
     new { type = "MemoryNode", category = "Data", name = "Memory", description = "Read/write from global memory", color = "#7ED321" },
     
            // IO Nodes
     new { type = "HttpRequestNode", category = "IO", name = "HTTP Request", description = "Make REST API calls", color = "#BD10E0" },
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
}
