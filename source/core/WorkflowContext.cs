using TwfAiFramework.Tracking;
using Microsoft.Extensions.Logging;

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

// ─── Supporting Types ─────────────────────────────────────────────────────────

/// <summary>Represents a single message in a conversation.</summary>
public record ChatMessage(string Role, string Content, DateTime Timestamp)
{
    public static ChatMessage System(string content) =>
        new("system", content, DateTime.UtcNow);
    public static ChatMessage User(string content) =>
        new("user", content, DateTime.UtcNow);
    public static ChatMessage Assistant(string content) =>
        new("assistant", content, DateTime.UtcNow);
}

/// <summary>Logger that prefixes all messages with a context string.</summary>
internal sealed class PrefixedLogger : ILogger
{
    private readonly ILogger _inner;
    private readonly string _prefix;

    public PrefixedLogger(ILogger inner, string prefix)
    {
        _inner = inner;
        _prefix = prefix;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        _inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _inner.Log(logLevel, eventId, state, exception,
            (s, ex) => $"{_prefix} {formatter(s, ex)}");
    }
}
