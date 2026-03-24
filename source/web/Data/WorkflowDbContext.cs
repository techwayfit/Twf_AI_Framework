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
