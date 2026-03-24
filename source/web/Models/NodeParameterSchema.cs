using System.ComponentModel.DataAnnotations;

namespace TwfAiFramework.Web.Models;

/// <summary>
/// Defines the schema for a node type's parameters
/// </summary>
public class NodeParameterSchema
{
    public string NodeType { get; set; } = string.Empty;
    public List<ParameterDefinition> Parameters { get; set; } = new();
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
    Text, // Single-line text input
    TextArea,       // Multi-line text
    Number,         // Numeric input
    Boolean,        // Checkbox
    Select,   // Dropdown
    Json,      // JSON editor
    KeyValueList,   // List of key-value pairs
    StringList      // List of strings
}
