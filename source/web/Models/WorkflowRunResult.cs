using TwfAiFramework.Core;

namespace TwfAiFramework.Web.Models;

/// <summary>
/// Result of executing a <see cref="WorkflowDefinition"/> via
/// <see cref="TwfAiFramework.Web.Services.WorkflowDefinitionRunner"/>.
/// </summary>
public sealed class WorkflowRunResult
{
    public bool IsSuccess { get; init; }
    public string WorkflowName { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public string? FailedNodeName { get; init; }

    /// <summary>Final WorkflowData after execution, serialised to a dictionary for JSON responses.</summary>
    public Dictionary<string, object?> OutputData { get; init; } = new();

    public DateTime FinishedAt { get; init; } = DateTime.UtcNow;

    public static WorkflowRunResult Success(string workflowName, WorkflowData data) => new()
    {
        IsSuccess    = true,
        WorkflowName = workflowName,
        OutputData   = data.Keys.ToDictionary(k => k, k => data.Get<object>(k))
    };

    public static WorkflowRunResult Failure(
        string workflowName, WorkflowData data, string? error, string? failedNode) => new()
    {
        IsSuccess      = false,
        WorkflowName   = workflowName,
        ErrorMessage   = error,
        FailedNodeName = failedNode,
        OutputData     = data.Keys.ToDictionary(k => k, k => data.Get<object>(k))
    };
}

/// <summary>
/// Optional request body for POST /Workflow/Run/{id}.
/// Lets callers seed initial WorkflowData values before execution starts.
/// </summary>
public sealed class WorkflowRunRequest
{
    /// <summary>
    /// Key-value pairs written into the initial WorkflowData before the first node runs.
    /// Example: { "user_message": "Hello!", "language": "English" }
    /// </summary>
    public Dictionary<string, object?> InitialData { get; set; } = new();
}
