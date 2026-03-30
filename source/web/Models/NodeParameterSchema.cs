using System.ComponentModel.DataAnnotations;

namespace TwfAiFramework.Web.Models;

/// <summary>
/// Defines the schema for a node type's parameters
/// </summary>
public class NodeParameterSchema
{
    public string NodeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterDefinition> Parameters { get; set; } = new();
    
    // Phase 1: Multi-port support
    public List<PortDefinition> InputPorts { get; set; } = new();
    public List<PortDefinition> OutputPorts { get; set; } = new();
    
    // Phase 1: Node capabilities
    public NodeCapabilities Capabilities { get; set; } = new();
    
    // Phase 1: Execution options schema
    public List<ExecutionOptionDefinition> ExecutionOptions { get; set; } = new();
}

/// <summary>
/// Defines a port (input or output) for a node
/// Phase 1: Multi-port support
/// </summary>
public class PortDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public PortType Type { get; set; } = PortType.Data;
    public bool Required { get; set; }
    public string? Condition { get; set; } // For conditional outputs (e.g., "true", "false")
    public string? Description { get; set; }
}

/// <summary>
/// Type of port for data flow
/// Phase 1: Port type definitions
/// </summary>
public enum PortType
{
  Data,        // Standard data flow
    Control,     // Control flow (for triggering)
    Conditional  // Conditional branches (true/false, match/no-match, etc.)
}

/// <summary>
/// Defines capabilities and features supported by a node type
/// Phase 1: Node capabilities
/// </summary>
public class NodeCapabilities
{
    public bool SupportsMultipleInputs { get; set; } = false;
    public bool SupportsMultipleOutputs { get; set; } = false;
    public bool SupportsConditionalRouting { get; set; } = false;
public bool SupportsRetry { get; set; } = true;
    public bool SupportsTimeout { get; set; } = true;
    public bool SupportsSubWorkflow { get; set; } = false;
    public bool SupportsDynamicPorts { get; set; } = false;
}

/// <summary>
/// Defines an execution option that can be configured for a node
/// Phase 1: Execution options
/// </summary>
public class ExecutionOptionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ParameterType Type { get; set; }
    public object? DefaultValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? MinValue { get; set; }
  public int? MaxValue { get; set; }
}

/// <summary>
/// Defines a single parameter for a node
/// </summary>
public class ParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ParameterType Type { get; set; }
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    
    // For select/dropdown types
    public List<SelectOption>? Options { get; set; }
    
    // Validation
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? Pattern { get; set; }
public string? Placeholder { get; set; }
}

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public enum ParameterType
{
    Text,  // Single-line text input
    TextArea,   // Multi-line text
    Number,   // Numeric input
 Boolean,        // Checkbox
    Select,         // Dropdown
  Json,           // JSON editor
    KeyValueList,   // List of key-value pairs
    StringList,     // List of strings
    Color,          // Colour picker
}
