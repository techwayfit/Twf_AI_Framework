using TwfAiFramework.Tracking;
using Microsoft.Extensions.Logging;
using twf_ai_framework.Core.Models;

namespace TwfAiFramework.Core;

/// <summary>
/// Immutable execution context shared across all nodes in a single workflow run.
/// Provides logging, tracking, shared state, and cancellation support.
/// Think of this as the "runtime environment" for the workflow.
/// </summary>
public sealed class WorkflowContext
{
    // ─── Identity ────────────────────────────────────────────────────────────

    /// <summary>The name of the workflow (e.g., "CustomerSupportBot").</summary>
    public string WorkflowName { get; }

    /// <summary>Unique ID for this specific execution run (UUID).</summary>
    public string RunId { get; }

    /// <summary>UTC timestamp when this run was started.</summary>
    public DateTime StartedAt { get; }

    // ─── Infrastructure ──────────────────────────────────────────────────────

    /// <summary>Structured logger. Automatically prefixes all messages with [WorkflowName][RunId].</summary>
    public ILogger Logger { get; }

    /// <summary>Execution tracker — records every node's start/end/status.</summary>
    public ExecutionTracker Tracker { get; }

    /// <summary>Cancellation token for the entire workflow run.</summary>
    public CancellationToken CancellationToken { get; }

    // ─── State ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Global state bag — survives across all nodes in the run.
    /// Use for things like "accumulated chat history", "user session", "running totals".
    /// Unlike WorkflowData (which is per-step), this persists for the entire run.
    /// </summary>
    private readonly Dictionary<string, object?> _globalState = new();

    /// <summary>Services injected into the context (e.g., IHttpClientFactory, IVectorStore).</summary>
    private readonly Dictionary<Type, object> _services = new();

    // ─── Constructor ─────────────────────────────────────────────────────────

    public WorkflowContext(
        string workflowName,
        ILogger logger,
        ExecutionTracker? tracker = null,
        CancellationToken cancellationToken = default)
    {
        WorkflowName = workflowName;
        RunId = Guid.NewGuid().ToString("N")[..12].ToUpper();
        StartedAt = DateTime.UtcNow;
        Logger = new PrefixedLogger(logger, $"[{workflowName}][{RunId}]");
        Tracker = tracker ?? new ExecutionTracker();
        CancellationToken = cancellationToken;
    }

    // ─── Global State ─────────────────────────────────────────────────────────

    public void SetState<T>(string key, T value) => _globalState[key] = value;

    public T? GetState<T>(string key)
    {
        if (!_globalState.TryGetValue(key, out var val) || val is null) return default;
        return val is T typed ? typed : default;
    }

    public bool HasState(string key) =>
        _globalState.TryGetValue(key, out var v) && v is not null;

    // ─── Conversation Memory (chat history shortcut) ──────────────────────────

    private const string ChatHistoryKey = "__chat_history__";

    public void AppendMessage(ChatMessage message)
    {
        var history = GetState<List<ChatMessage>>(ChatHistoryKey) ?? new();
        history.Add(message);
        SetState(ChatHistoryKey, history);
    }

    public List<ChatMessage> GetChatHistory() =>
        GetState<List<ChatMessage>>(ChatHistoryKey) ?? new();

    public void ClearChatHistory() => SetState(ChatHistoryKey, new List<ChatMessage>());

    // ─── Service Locator ─────────────────────────────────────────────────────

    public void RegisterService<T>(T service) where T : notnull =>
        _services[typeof(T)] = service;

    public T GetService<T>() where T : notnull
    {
        if (_services.TryGetValue(typeof(T), out var svc) && svc is T typed)
            return typed;
        throw new InvalidOperationException(
            $"Service '{typeof(T).Name}' not registered in WorkflowContext. " +
            $"Call context.RegisterService<{typeof(T).Name}>(instance) before running the workflow.");
    }

    public bool HasService<T>() => _services.ContainsKey(typeof(T));

    // ─── Builder Pattern ─────────────────────────────────────────────────────

    public WorkflowContext WithService<T>(T service) where T : notnull
    {
        RegisterService(service);
        return this;
    }

    public WorkflowContext WithState<T>(string key, T value)
    {
        SetState(key, value);
        return this;
    }
}
