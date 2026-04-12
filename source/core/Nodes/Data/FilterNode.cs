using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Data;

/// <summary>
/// Validates WorkflowData against conditions. If validation fails, the node
/// throws (stopping the pipeline) or sets an "is_valid" flag.
/// Use for input validation, safety checks, and guardrails.
/// </summary>
public sealed class FilterNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Data";
    public override string Description => $"Data validation filter: {Name}";

    /// <inheritdoc/>
    public override string IdPrefix => "filter";

    /// <inheritdoc/>
    public override IReadOnlyList<NodeData> DataIn =>
        _rules.Select(r => new NodeData(r.Key, typeof(object), Required: false, "Field subject to validation rule"))
              .DistinctBy(p => p.Key)
              .ToList<NodeData>();

    /// <inheritdoc/>
    public override IReadOnlyList<NodeData> DataOut =>
    [
        new("is_valid",          typeof(bool),         Description: "True if all rules passed"),
        new("validation_errors", typeof(List<string>), Required: false, "List of failure messages")
    ];

    /// <summary>UI schema: parameter form fields shown in the properties panel.</summary>
    public static NodeParameterSchema Schema { get; } = new()
    {
        NodeType    = "FilterNode",
        Description = "Validate data — fails or flags when rules are not met",
        Parameters  =
        [
            new() { Name = "throwOnFail", Label = "Throw on Failure", Type = ParameterType.Boolean, Required = false, DefaultValue = true,
                Description = "If false, writes is_valid=false instead of throwing" },
            new() { Name = "requireKey",  Label = "Require Non-Empty Key", Type = ParameterType.Text, Required = false, Placeholder = "e.g. prompt",
                Description = "Fail if this key is missing or blank" },
            new() { Name = "maxLengthKey",Label = "Max Length Key",        Type = ParameterType.Text, Required = false, Placeholder = "e.g. prompt" },
            new() { Name = "maxLength",   Label = "Max Length",            Type = ParameterType.Number, Required = false, DefaultValue = 0, MinValue = 0,
                Description = "Maximum allowed character length (0 = no limit)" },
        ]
    };

    private readonly List<FilterRule> _rules;
    private readonly bool _throwOnFail;

    public FilterNode(string name, bool throwOnFail = true)
    {
        Name = name;
        _throwOnFail = throwOnFail;
        _rules = new List<FilterRule>();
    }

    /// <summary>Dictionary constructor for dynamic instantiation.</summary>
    public FilterNode(Dictionary<string, object?> parameters)
        : this(
            NodeParameters.GetString(parameters, "name") ?? "Filter",
            NodeParameters.GetBool(parameters, "throwOnFail", true))
    {
        var requireKey = NodeParameters.GetString(parameters, "requireKey");
        if (!string.IsNullOrEmpty(requireKey)) RequireNonEmpty(requireKey);

        var maxLengthKey = NodeParameters.GetString(parameters, "maxLengthKey");
        var maxLength    = NodeParameters.GetInt(parameters, "maxLength", 0);
        if (!string.IsNullOrEmpty(maxLengthKey) && maxLength > 0)
            MaxLength(maxLengthKey, maxLength);
    }

    public FilterNode Require(string key, string? reason = null)
    {
        _rules.Add(new FilterRule(key, data => data.Has(key),
            reason ?? $"Required field '{key}' is missing"));
        return this;
    }

    public FilterNode RequireNonEmpty(string key)
    {
        _rules.Add(new FilterRule(key, data =>
            {
                var val = data.GetString(key);
                return !string.IsNullOrWhiteSpace(val);
            }, $"Field '{key}' must not be empty"));
        return this;
    }

    public FilterNode MaxLength(string key, int maxLength)
    {
        _rules.Add(new FilterRule(key, data =>
                (data.GetString(key)?.Length ?? 0) <= maxLength,
            $"Field '{key}' exceeds max length of {maxLength}"));
        return this;
    }

    public FilterNode Custom(string key, Func<WorkflowData, bool> predicate, string errorMessage)
    {
        _rules.Add(new FilterRule(key, predicate, errorMessage));
        return this;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var failures = new List<string>();

        foreach (var rule in _rules)
        {
            if (!rule.Predicate(input))
            {
                failures.Add(rule.ErrorMessage);
                nodeCtx.Log($"❌ Validation failed: {rule.ErrorMessage}");
            }
        }

        var isValid = failures.Count == 0;
        var output = input.Clone().Set("is_valid", isValid);

        if (!isValid)
        {
            var errorSummary = string.Join("; ", failures);
            output.Set("validation_errors", failures);

            if (_throwOnFail)
                throw new ValidationException(Name, errorSummary);
        }
        else
        {
            nodeCtx.Log($"✅ All {_rules.Count} validation rules passed");
        }

        nodeCtx.SetMetadata("rules_checked", _rules.Count);
        nodeCtx.SetMetadata("failures", failures.Count);

        return Task.FromResult(output);
    }

    private record FilterRule(string Key, Func<WorkflowData, bool> Predicate, string ErrorMessage);
}