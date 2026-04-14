using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using twf_ai_framework.Core.Models;
using twf_ai_framework.Nodes;

namespace TwfAiFramework.Core;

/// <summary>
/// Fluent builder for constructing and executing workflows.
/// 
/// Usage:
///   var result = await Workflow.Create("MyBot")
///       .AddNode(new PromptBuilderNode(...))
///       .AddNode(new LlmNode(config))
///       .AddNode(new ResponseFormatterNode())
///       .RunAsync(initialData);
/// </summary>
public sealed class Workflow
{
    private readonly string _name;
    private readonly List<PipelineStep> _steps = new();
    private ILogger _logger = NullLogger.Instance;
    private Action<WorkflowResult>? _onComplete;
    private Action<string, Exception?>? _onError;
    private GlobalErrorStrategy _errorStrategy = GlobalErrorStrategy.StopOnFirstFailure;

    public string Name => _name;

    private Workflow(string name) { _name = name; }

    // ─── Entry Point ─────────────────────────────────────────────────────────

    public static Workflow Create(string workflowName) => new(workflowName);

    // ─── Configuration ────────────────────────────────────────────────────────

    public Workflow UseLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public Workflow OnComplete(Action<WorkflowResult> handler)
    {
        _onComplete = handler;
        return this;
    }

    public Workflow OnError(Action<string, Exception?> handler)
    {
        _onError = handler;
        return this;
    }

    public Workflow ContinueOnErrors()
    {
        _errorStrategy = GlobalErrorStrategy.ContinueOnFailure;
        return this;
    }

    // ─── Node Registration ────────────────────────────────────────────────────

    /// <summary>Add a node with default options.</summary>
    public Workflow AddNode(INode node) =>
        AddNode(node, NodeOptions.Default);

    /// <summary>Add a node with custom options (retry, timeout, condition).</summary>
    public Workflow AddNode(INode node, NodeOptions options)
    {
        _steps.Add(new PipelineStep(StepType.Node, node, options));
        return this;
    }

    /// <summary>Add an inline lambda node without creating a class.</summary>
    public Workflow AddStep(string name,
        Func<WorkflowData, WorkflowContext, Task<WorkflowData>> func,
        NodeOptions? options = null)
    {
        var node = new LambdaNode(name, func);
        _steps.Add(new PipelineStep(StepType.Node, node, options ?? NodeOptions.Default));
        return this;
    }

    // ─── Branching ───────────────────────────────────────────────────────────

    /// <summary>
    /// Conditional branching — like an IF node in n8n.
    /// Evaluates condition against the current WorkflowData.
    /// Only one branch executes.
    /// </summary>
    public Workflow Branch(
        Func<WorkflowData, bool> condition,
        Action<Workflow> trueBranch,
        Action<Workflow>? falseBranch = null)
    {
        var truePipeline = Create($"{_name}/Branch:True");
        trueBranch(truePipeline);

        Workflow? falsePipeline = null;
        if (falseBranch is not null)
        {
            falsePipeline = Create($"{_name}/Branch:False");
            falseBranch(falsePipeline);
        }

        _steps.Add(new PipelineStep(StepType.Branch)
        {
            BranchCondition = condition,
            TrueBranch = truePipeline,
            FalseBranch = falsePipeline
        });
        return this;
    }

    // ─── Parallel Execution ───────────────────────────────────────────────────

    /// <summary>
    /// Run multiple nodes in parallel. Each receives a clone of the current data.
    /// Results are merged back (later keys overwrite earlier ones).
    /// </summary>
    public Workflow Parallel(params INode[] nodes)
    {
        _steps.Add(new PipelineStep(StepType.Parallel) { ParallelNodes = nodes });
        return this;
    }

    // ─── Loop ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Iterate over a list in WorkflowData and run a sub-workflow for each item.
    /// Results are collected into a list under 'outputKey'.
    /// </summary>
    public Workflow ForEach(
        string itemsKey,
        string outputKey,
        Action<Workflow> bodyBuilder)
    {
        var bodyPipeline = Create($"{_name}/Loop");
        bodyBuilder(bodyPipeline);

        _steps.Add(new PipelineStep(StepType.Loop)
        {
            LoopItemsKey = itemsKey,
            LoopOutputKey = outputKey,
            LoopBody = bodyPipeline
        });
        return this;
    }

    // ─── Execution ────────────────────────────────────────────────────────────

    public async Task<WorkflowResult> RunAsync(
        WorkflowData? initialData = null,
        WorkflowContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var ctx = context ?? new WorkflowContext(_name, _logger,
            cancellationToken: cancellationToken);

        var startedAt = DateTime.UtcNow;
        var current = initialData?.Clone() ?? new WorkflowData();
        var allResults = new List<NodeResult>();

        ctx.Logger.LogInformation(
            "🚀 Starting workflow '{Workflow}' [RunId: {RunId}]", _name, ctx.RunId);

        foreach (var step in _steps)
        {
            var stepResult = await ExecuteStepAsync(step, current, ctx).ConfigureAwait(false);
            allResults.AddRange(stepResult.Results);

            if (stepResult.IsSuccess)
            {
                current = stepResult.Data;
            }
            else if (_errorStrategy == GlobalErrorStrategy.StopOnFirstFailure)
            {
                var report = ctx.Tracker.GenerateReport(_name, ctx.RunId);
                var failResult = WorkflowResult.Failure(_name, ctx.RunId, current,
                    allResults, stepResult.FailedNodeName ?? "Unknown",
                    stepResult.ErrorMessage ?? "Unknown error",
                    stepResult.Exception, startedAt, report);

                _onError?.Invoke(failResult.ErrorMessage!, failResult.Exception);
                ctx.Logger.LogError("💥 Workflow '{Workflow}' FAILED at [{Node}]: {Error}",
                    _name, failResult.FailedNodeName, failResult.ErrorMessage);

                return failResult;
            }
        }

        var successReport = ctx.Tracker.GenerateReport(_name, ctx.RunId);
        var successResult = WorkflowResult.Success(_name, ctx.RunId, current,
            allResults, startedAt, successReport);

        ctx.Logger.LogInformation(
            "🏁 Workflow '{Workflow}' completed in {DurationMs}ms ✅",
            _name, successResult.TotalDuration.TotalMilliseconds);

        _onComplete?.Invoke(successResult);
        return successResult;
    }

    // ─── Internal Step Execution ──────────────────────────────────────────────

    private async Task<StepExecutionResult> ExecuteStepAsync(
        PipelineStep step, WorkflowData data, WorkflowContext ctx)
    {
        switch (step.Type)
        {
            case StepType.Node:
                return await ExecuteNodeStepAsync(step, data, ctx).ConfigureAwait(false);

            case StepType.Branch:
                return await ExecuteBranchStepAsync(step, data, ctx).ConfigureAwait(false);

            case StepType.Parallel:
                return await ExecuteParallelStepAsync(step, data, ctx).ConfigureAwait(false);

            case StepType.Loop:
                return await ExecuteLoopStepAsync(step, data, ctx).ConfigureAwait(false);

            default:
                throw new InvalidOperationException($"Unknown step type: {step.Type}");
        }
    }

    private async Task<StepExecutionResult> ExecuteNodeStepAsync(
        PipelineStep step, WorkflowData data, WorkflowContext ctx)
    {
        var node = step.Node!;
        var opts = step.Options;

        // Check condition — skip if condition is false
        if (opts.RunCondition is not null && !opts.RunCondition(data))
        {
            ctx.Logger.LogInformation("⏭  [{Node}] Skipped (condition not met)", node.Name);
            var skipped = NodeResult.Skipped(node.Name, data);
            return StepExecutionResult.Ok(data, new[] { skipped });
        }

        // Track the node
        var record = ctx.Tracker.BeginNode(node.Name, node.Category);

        NodeResult result = null!;
        var attempts = 0;
        var maxAttempts = opts.MaxRetries + 1;

        while (attempts < maxAttempts)
        {
            attempts++;

            // Apply timeout if configured
            if (opts.Timeout.HasValue)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.CancellationToken);
                cts.CancelAfter(opts.Timeout.Value);
                var timeoutCtx = new WorkflowContext(ctx.WorkflowName, _logger,
                    ctx.Tracker, cts.Token);
                result = await node.ExecuteAsync(data, timeoutCtx).ConfigureAwait(false);
            }
            else
            {
                result = await node.ExecuteAsync(data, ctx).ConfigureAwait(false);
            }

            if (result.IsSuccess || attempts >= maxAttempts) break;

            // Retry with exponential backoff
            var delay = TimeSpan.FromMilliseconds(
                opts.RetryDelay.TotalMilliseconds * Math.Pow(2, attempts - 1));
            ctx.Logger.LogWarning(
                "🔄 [{Node}] Attempt {Attempt}/{Max} failed. Retrying in {Delay}ms...",
                node.Name, attempts, maxAttempts, delay.TotalMilliseconds);

            await Task.Delay(delay, ctx.CancellationToken).ConfigureAwait(false);
        }

        ctx.Tracker.CompleteNode(record, result);

        if (result.IsSuccess)
            return StepExecutionResult.Ok(result.Data, new[] { result });

        // Handle failure
        if (opts.ContinueOnError)
        {
            ctx.Logger.LogWarning(
                "⚠️  [{Node}] Failed but ContinueOnError=true. Using fallback data.",
                node.Name);
            var fallback = opts.FallbackData ?? data;
            return StepExecutionResult.Ok(fallback, new[] { result });
        }

        return StepExecutionResult.Fail(result, data);
    }

    private async Task<StepExecutionResult> ExecuteBranchStepAsync(
        PipelineStep step, WorkflowData data, WorkflowContext ctx)
    {
        var condition = step.BranchCondition!;
        var branchTaken = condition(data);
        var pipeline = branchTaken ? step.TrueBranch : step.FalseBranch;

        ctx.Logger.LogInformation("🔀 Branch condition: {Result}", branchTaken ? "TRUE" : "FALSE");

        if (pipeline is null)
        {
            ctx.Logger.LogInformation("⏭  Branch {Branch} has no handler, skipping.",
                branchTaken ? "FALSE" : "FALSE");
            return StepExecutionResult.Ok(data, Array.Empty<NodeResult>());
        }

        var branchResult = await pipeline.RunAsync(data, ctx).ConfigureAwait(false);
        if (branchResult.IsSuccess)
            return StepExecutionResult.Ok(branchResult.Data, branchResult.NodeResults.ToList());
        return StepExecutionResult.Fail(branchResult.NodeResults.Last(), data);
    }

    private async Task<StepExecutionResult> ExecuteParallelStepAsync(
        PipelineStep step, WorkflowData data, WorkflowContext ctx)
    {
        var nodes = step.ParallelNodes!;
        ctx.Logger.LogInformation("⚡ Running {Count} nodes in parallel", nodes.Length);

        var tasks = nodes.Select(n => n.ExecuteAsync(data.Clone(), ctx)).ToArray();
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var merged = data.Clone();
        var allNodeResults = new List<NodeResult>();

        foreach (var r in results)
        {
            allNodeResults.Add(r);
            if (r.IsSuccess)
                merged.Merge(r.Data);
        }

        var firstFailure = results.FirstOrDefault(r => r.IsFailure);
        if (firstFailure is not null)
            return StepExecutionResult.Fail(firstFailure, data);

        return StepExecutionResult.Ok(merged, allNodeResults);
    }

    private async Task<StepExecutionResult> ExecuteLoopStepAsync(
        PipelineStep step, WorkflowData data, WorkflowContext ctx)
    {
        var items = data.Get<IEnumerable<object>>(step.LoopItemsKey!)?.ToList()
            ?? new List<object>();

        ctx.Logger.LogInformation("🔁 Loop over {Count} items in '{Key}'",
            items.Count, step.LoopItemsKey);

        var outputs = new List<WorkflowData>();
        var allResults = new List<NodeResult>();

        for (var i = 0; i < items.Count; i++)
        {
            var itemData = data.Clone()
                .Set("__loop_item__", items[i])
                .Set("__loop_index__", i)
                .Set("__loop_total__", items.Count);

            var loopResult = await step.LoopBody!.RunAsync(itemData, ctx).ConfigureAwait(false);
            allResults.AddRange(loopResult.NodeResults);

            if (loopResult.IsFailure)
                return StepExecutionResult.Fail(allResults.Last(), data);

            outputs.Add(loopResult.Data);
        }

        var resultData = data.Clone().Set(step.LoopOutputKey!, outputs);
        return StepExecutionResult.Ok(resultData, allResults);
    }
}
