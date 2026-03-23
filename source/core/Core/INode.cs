namespace TwfAiFramework.Core;

/// <summary>
/// The fundamental unit of work in TwfAiFramework — equivalent to a node in n8n.
/// Every reusable operation (LLM call, HTTP request, transform, etc.) implements this.
/// </summary>
public interface INode
{
    /// <summary>Human-readable name shown in logs and execution reports.</summary>
    string Name { get; }

    /// <summary>Category grouping: AI, Data, IO, Control, etc.</summary>
    string Category { get; }

    /// <summary>A short description of what this node does.</summary>
    string Description { get; }

    /// <summary>
    /// Execute this node. Receives the current data packet and execution context.
    /// Returns a NodeResult containing the updated data and execution metadata.
    /// </summary>
    Task<NodeResult> ExecuteAsync(WorkflowData data, WorkflowContext context);
}

/// <summary>
/// Marker interface for nodes that can validate their configuration before execution.
/// </summary>
public interface IValidatableNode : INode
{
    /// <summary>Validate node configuration. Throws if invalid.</summary>
    void Validate();
}

/// <summary>Execution status of a node.</summary>
public enum NodeStatus
{
    Pending,
    Running,
    Success,
    Failed,
    Skipped,
    TimedOut,
    Cancelled
}
