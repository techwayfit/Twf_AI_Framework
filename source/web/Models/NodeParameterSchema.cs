namespace TwfAiFramework.Web.Models;

/// <summary>
/// Schema stored per node type. Contains parameter field definitions
/// (for the properties panel form) and data port metadata (for WorkflowData
/// key reference hints). Routing handles live in the React nodeConfig.js.
/// </summary>
public class NodeParameterSchema
{
    public string NodeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterDefinition> Parameters { get; set; } = new();

    // WorkflowData keys this node reads/writes — shown in the properties panel
    // so users know what {{nodeId.key}} references are available.
    public List<DataPortInfo> DataInputs  { get; set; } = new();
    public List<DataPortInfo> DataOutputs { get; set; } = new();
}

/// <summary>
/// Describes a WorkflowData key that a node reads (input) or writes (output).
/// </summary>
public class DataPortInfo
{
    public string Key         { get; set; } = string.Empty;
    public bool   Required    { get; set; }
    public bool   IsDynamic   { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>Defines a single editable parameter for a node.</summary>
public class ParameterDefinition
{
    public string Name         { get; set; } = string.Empty;
    public string Label        { get; set; } = string.Empty;
    public string Description  { get; set; } = string.Empty;
    public ParameterType Type  { get; set; }
    public bool   Required     { get; set; }
    public object? DefaultValue { get; set; }
    public string? Placeholder { get; set; }

    // For Select type
    public List<SelectOption>? Options { get; set; }

    // For Number type
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
}

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public enum ParameterType
{
    Text,
    TextArea,
    Number,
    Boolean,
    Select,
    Json,
    Color,
}
