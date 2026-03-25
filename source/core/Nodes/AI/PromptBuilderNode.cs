using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using System.Text.RegularExpressions;

namespace TwfAiFramework.Nodes.AI;

/// <summary>
/// Builds dynamic prompts from templates with {{variable}} substitution.
/// 
/// Template example:
///   "You are a {{role}}. Answer the following question: {{question}}"
///
/// Reads from WorkflowData:
///   - Keys matching {{variable}} names in the template
///
/// Writes to WorkflowData:
///   - "prompt" : the fully substituted prompt string
///   - "system_prompt" : if a system template is also provided
/// </summary>
public sealed class PromptBuilderNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "AI";
    public override string Description =>
        "Builds a dynamic prompt from a template with variable substitution";

    private readonly string _promptTemplate;
    private readonly string? _systemTemplate;
    private readonly Dictionary<string, object?> _staticVariables;

    public PromptBuilderNode(
        string name,
        string promptTemplate,
        string? systemTemplate = null,
        Dictionary<string, object?>? staticVariables = null)
    {
        Name = name;
        _promptTemplate = promptTemplate;
        _systemTemplate = systemTemplate;
        _staticVariables = staticVariables ?? new();
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var prompt = Render(_promptTemplate, input, nodeCtx);
        var output = input.Clone().Set("prompt", prompt);

        nodeCtx.Log($"Prompt length: {prompt.Length} chars");

        if (_systemTemplate is not null)
        {
            var system = Render(_systemTemplate, input, nodeCtx);
            output.Set("system_prompt", system);
            nodeCtx.Log($"System prompt length: {system.Length} chars");
        }

        return Task.FromResult(output);
    }

    private string Render(string template, WorkflowData input, NodeExecutionContext nodeCtx)
    {
        return Regex.Replace(template, @"\{\{(\w+)\}\}", match =>
        {
            var key = match.Groups[1].Value;

            // Check static variables first
            if (_staticVariables.TryGetValue(key, out var staticVal))
                return staticVal?.ToString() ?? string.Empty;

            // Then check WorkflowData
            if (input.TryGet<object>(key, out var val) && val is not null)
                return val.ToString() ?? string.Empty;

            nodeCtx.Log($"⚠️  Template variable '{{{{key}}}}' not found in data");
            return $"{{{{MISSING:{key}}}}}";
        });
    }

    // ─── Convenience Factory Methods ──────────────────────────────────────────

    public static PromptBuilderNode Simple(string name, string template) =>
        new(name, template);

    public static PromptBuilderNode WithSystem(string name, string prompt, string system) =>
        new(name, prompt, system);

    public static PromptBuilderNode FromFile(string name, string templatePath, string? systemPath = null)
    {
        var prompt = File.ReadAllText(templatePath);
        var system = systemPath is not null ? File.ReadAllText(systemPath) : null;
        return new PromptBuilderNode(name, prompt, system);
    }
}
