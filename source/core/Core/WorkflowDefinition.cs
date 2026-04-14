namespace TwfAiFramework.Core;

/// <summary>
/// Immutable workflow definition representing the structure of a workflow.
/// Created by <see cref="WorkflowBuilder"/> and executed by <see cref="WorkflowExecutor"/>.
/// </summary>
/// <remarks>
/// This class follows the Single Responsibility Principle by only representing
/// the workflow structure without containing execution logic.
/// </remarks>
public sealed class WorkflowDefinition
{
    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The ordered sequence of steps in the workflow.
    /// </summary>
    public IReadOnlyList<PipelineStep> Steps { get; }

    /// <summary>
    /// Configuration options for workflow execution.
    /// </summary>
    public WorkflowConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new workflow definition.
    /// </summary>
    /// <param name="name">The workflow name.</param>
    /// <param name="steps">The workflow steps.</param>
    /// <param name="configuration">Execution configuration.</param>
  /// <remarks>
    /// Constructor is internal to enforce creation through <see cref="WorkflowBuilder"/>.
 /// </remarks>
    internal WorkflowDefinition(
        string name,
        IReadOnlyList<PipelineStep> steps,
        WorkflowConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(name))
    throw new ArgumentException("Workflow name cannot be empty", nameof(name));

        Name = name;
 Steps = steps ?? throw new ArgumentNullException(nameof(steps));
 Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets a summary of the workflow structure.
    /// </summary>
    public WorkflowSummary GetSummary()
{
 var nodeCount = Steps.Count(s => s.Type == StepType.Node);
      var branchCount = Steps.Count(s => s.Type == StepType.Branch);
        var loopCount = Steps.Count(s => s.Type == StepType.Loop);
        var parallelCount = Steps.Count(s => s.Type == StepType.Parallel);

    return new WorkflowSummary(
       Name,
            Steps.Count,
     nodeCount,
            branchCount,
 loopCount,
         parallelCount);
    }

    public override string ToString() => $"WorkflowDefinition(Name='{Name}', Steps={Steps.Count})";
}

/// <summary>
/// Summary information about a workflow's structure.
/// </summary>
public sealed record WorkflowSummary(
    string Name,
    int TotalSteps,
    int NodeCount,
    int BranchCount,
    int LoopCount,
int ParallelCount);
