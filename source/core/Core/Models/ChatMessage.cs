namespace twf_ai_framework.Core.Models;

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
