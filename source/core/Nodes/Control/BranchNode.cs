using TwfAiFramework.Core;

namespace TwfAiFramework.Nodes.Control;

// ═══════════════════════════════════════════════════════════════════════════════
// BranchNode — Switch/Case router that writes selected route metadata
// ═══════════════════════════════════════════════════════════════════════════════
/// <summary>
/// Evaluates a value key and selects one route from case1/case2/case3/default.
/// Writes explicit routing keys into WorkflowData so downstream branching can
/// be driven via Workflow.Branch(...) predicates.
///
/// Writes:
///   - "branch_selected_port" : "case1" | "case2" | "case3" | "default"
///   - "branch_input_value"   : string representation of input value
///   - "branch_selected_value": matched case value (if matched), else null
///   - "branch_case1" / "branch_case2" / "branch_case3" / "branch_default" : bool flags
/// </summary>
public sealed class BranchNode : BaseNode
{
    public override string Name { get; }
    public override string Category => "Control";
    public override string Description =>
        $"Routes by '{_valueKey}' using switch/case matching";

    /// <inheritdoc/>
    public override string IdPrefix => "branch";

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> InputPorts =>
    [
        new(_valueKey, typeof(string), Required: true, "Value used to select a branch")
    ];

    /// <inheritdoc/>
    public override IReadOnlyList<NodePort> OutputPorts =>
    [
        new("branch_selected_port", typeof(string), Description: "Matched case key (case1/case2/default)"),
        new("branch_input_value",   typeof(string), Description: "String representation of the input value"),
        new("branch_status",        typeof(string), Description: "success or failure")
    ];
    public Dictionary<string, Workflow?> _branchWorkflows { get; } = new(StringComparer.OrdinalIgnoreCase);

    private readonly string _valueKey; 
    
    
    public BranchNode(
        string name,
        string valueKey,
        string? case1Value = null,
        string? case2Value = null,
        string? case3Value = null,
        bool caseSensitive = false)
    {
        Name = name;
        _valueKey = valueKey;
    }

    public BranchNode(string name, string valueKey, params KeyValuePair<string, Workflow>[] branches)
    {
        Name = name;
        _valueKey = valueKey;
        foreach (var branch in branches) 
            _branchWorkflows[branch.Key] = branch.Value; //Add or set
    }

    protected override async Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var inputValue = input.Get<object>(_valueKey)?.ToString();
        if (string.IsNullOrEmpty(inputValue))
        {
            nodeCtx.Log("Branch route failure.");
            input.Set("branch_route_status", "failure");
            return input;
        }
        if (!_branchWorkflows.TryGetValue(inputValue, out Workflow flow))
        {
            inputValue = "default";
            _branchWorkflows.TryGetValue(inputValue, out flow);
        }
        
        if(null != flow)
        {
            var result= await flow.RunAsync(input, context);
            var data = result.Data.Clone();
            if (!result.IsSuccess)
            {
                nodeCtx.Log($"Branch route failure: {result.ErrorMessage}");
                data.Set("branch_status", "failure");
            }
            else
            {
                nodeCtx.Log("Branch route success.");
                data.Set("branch_status", "success");

            }
            data.Set("branch_route", flow.Name)
                .Set("branch_selected_port", inputValue);

            return data;
        }
    
        nodeCtx.Log("Branch route failure.");
        input.Set("branch_route_status", "failure");
        
        return input;
    }

}