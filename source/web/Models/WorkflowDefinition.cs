using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TwfAiFramework.Web.Models;

/// <summary>
/// Represents a complete workflow definition with nodes and connections
/// </summary>
public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<NodeDefinition> Nodes { get; set; } = new();
    
    public List<ConnectionDefinition> Connections { get; set; } = new();

    public Dictionary<string, object> Variables { get; set; } = new();

    public WorkflowMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents a single node in the workflow
/// </summary>
public class NodeDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty; // LlmNode, PromptBuilderNode, etc.

    public string Category { get; set; } = string.Empty; // AI, Data, IO, Control

    public Dictionary<string, object?> Parameters { get; set; } = new();

    // Visual editor properties
    public NodePosition Position { get; set; } = new();

    public string? Color { get; set; }
}

/// <summary>
/// Represents a connection between two nodes
/// </summary>
public class ConnectionDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SourceNodeId { get; set; }

    public string SourcePort { get; set; } = "output";

    [Required]
    public Guid TargetNodeId { get; set; }

    public string TargetPort { get; set; } = "input";
}

/// <summary>
/// Node position on the canvas
/// </summary>
public class NodePosition
{
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>
/// Workflow metadata
/// </summary>
public class WorkflowMetadata
{
    public string? Author { get; set; }
    public List<string> Tags { get; set; } = new();
    public int Version { get; set; } = 1;
 public bool IsActive { get; set; } = true;
}
