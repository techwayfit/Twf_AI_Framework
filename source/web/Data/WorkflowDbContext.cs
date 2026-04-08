using Microsoft.EntityFrameworkCore;
using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Data;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkflowEntity> Workflows { get; set; }
    public DbSet<NodeTypeEntity> NodeTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkflowEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.JsonData).IsRequired();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<NodeTypeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NodeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.SchemaJson).IsRequired();
            entity.HasIndex(e => e.NodeType).IsUnique();
            entity.HasIndex(e => e.Category);
        });
    }
}

/// <summary>
/// Entity for storing workflow in database
/// </summary>
public class WorkflowEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string JsonData { get; set; } = string.Empty; // Serialized WorkflowDefinition
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Entity for storing node type definitions in database
/// </summary>
public class NodeTypeEntity
{
    public int Id { get; set; }
    public string NodeType { get; set; } = string.Empty;   // e.g. "LlmNode"
    public string Name { get; set; } = string.Empty;       // e.g. "LLM"
    public string Category { get; set; } = string.Empty;   // e.g. "AI"
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#888888";
    public string Icon { get; set; } = "bi-box";
    public string SchemaJson { get; set; } = "{}";         // Serialized NodeParameterSchema
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
