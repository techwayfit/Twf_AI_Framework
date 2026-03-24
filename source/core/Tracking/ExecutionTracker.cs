using TwfAiFramework.Core;

namespace TwfAiFramework.Tracking;

/// <summary>
/// Records the execution of every node in a workflow run.
/// Provides timing breakdowns, status tracking, and generates reports.
/// </summary>
public sealed class ExecutionTracker
{
    private readonly List<NodeExecutionRecord> _records = new();
    private readonly object _lock = new();

    // ─── Recording ───────────────────────────────────────────────────────────

    public NodeExecutionRecord BeginNode(string nodeName, string nodeCategory)
    {
        var record = new NodeExecutionRecord
        {
            NodeName = nodeName,
            NodeCategory = nodeCategory,
            StartedAt = DateTime.UtcNow,
            Status = NodeStatus.Running
        };

        lock (_lock) { _records.Add(record); }
        return record;
    }

    public void CompleteNode(NodeExecutionRecord record, NodeResult result)
    {
        record.Status = result.Status;
        record.CompletedAt = DateTime.UtcNow;
        record.Duration = (record.CompletedAt - record.StartedAt) ?? TimeSpan.Zero;
        record.ErrorMessage = result.ErrorMessage;
        record.Metadata = result.Metadata;
        record.Logs = result.Logs.ToList();
    }

    // ─── Queries ─────────────────────────────────────────────────────────────

    public IReadOnlyList<NodeExecutionRecord> AllRecords => _records.AsReadOnly();

    public IEnumerable<NodeExecutionRecord> SuccessfulNodes =>
        _records.Where(r => r.Status == NodeStatus.Success);

    public IEnumerable<NodeExecutionRecord> FailedNodes =>
        _records.Where(r => r.Status == NodeStatus.Failed);

    public TimeSpan TotalDuration =>
        _records.Count == 0
            ? TimeSpan.Zero
            : _records.Max(r => r.CompletedAt ?? DateTime.UtcNow) -
              _records.Min(r => r.StartedAt);

    public NodeExecutionRecord? SlowestNode =>
        _records.OrderByDescending(r => r.Duration).FirstOrDefault();

    // ─── Report ───────────────────────────────────────────────────────────────

    public WorkflowReport GenerateReport(string workflowName, string runId) => new()
    {
        WorkflowName = workflowName,
        RunId = runId,
        GeneratedAt = DateTime.UtcNow,
        TotalNodes = _records.Count,
        SuccessCount = _records.Count(r => r.Status == NodeStatus.Success),
        FailureCount = _records.Count(r => r.Status == NodeStatus.Failed),
        SkippedCount = _records.Count(r => r.Status == NodeStatus.Skipped),
        TotalDuration = TotalDuration,
        NodeBreakdown = _records.Select(r => new NodeTimingEntry
        {
            NodeName = r.NodeName,
            Category = r.NodeCategory,
            Status = r.Status,
            DurationMs = r.Duration.TotalMilliseconds,
            StartedAt = r.StartedAt,
            ErrorMessage = r.ErrorMessage
        }).ToList()
    };
}

/// <summary>Mutable record of a single node's execution.</summary>
public sealed class NodeExecutionRecord
{
    public string NodeName { get; init; } = string.Empty;
    public string NodeCategory { get; init; } = string.Empty;
    public NodeStatus Status { get; set; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Logs { get; set; } = new();
}

/// <summary>Immutable report generated at the end of a workflow run.</summary>
public sealed class WorkflowReport
{
    public string WorkflowName { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public int TotalNodes { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public int SkippedCount { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public List<NodeTimingEntry> NodeBreakdown { get; init; } = new();

    public string ToTable()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"\n📊 Workflow Execution Report: {WorkflowName} [{RunId}]");
        sb.AppendLine($"   Total: {TotalNodes} nodes | ✅ {SuccessCount} | ❌ {FailureCount} | ⏭ {SkippedCount}");
        sb.AppendLine($"   Duration: {TotalDuration.TotalMilliseconds:F0}ms\n");
        sb.AppendLine($"   {"Node",-35} {"Category",-12} {"Status",-10} {"Duration",10}");
        sb.AppendLine($"   {new string('─', 75)}");
        foreach (var e in NodeBreakdown)
        {
            var icon = e.Status switch
            {
                NodeStatus.Success => "✅",
                NodeStatus.Failed => "❌",
                NodeStatus.Skipped => "⏭",
                _ => "⏳"
            };
            sb.AppendLine($"   {icon} {e.NodeName,-33} {e.Category,-12} {e.Status,-10} {e.DurationMs,8:F0}ms");
            if (e.ErrorMessage is not null)
                sb.AppendLine($"      ↳ Error: {e.ErrorMessage}");
        }
        return sb.ToString();
    }
}

public sealed class NodeTimingEntry
{
    public string NodeName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public NodeStatus Status { get; init; }
    public double DurationMs { get; init; }
    public DateTime StartedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
