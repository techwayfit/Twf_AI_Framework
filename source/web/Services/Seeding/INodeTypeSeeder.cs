using TwfAiFramework.Web.Repositories;

namespace TwfAiFramework.Web.Services.Seeding;

/// <summary>
/// Service responsible for seeding node type definitions into the database.
/// Discovers node types via INodeSchemaProvider and maintains the NodeTypes table.
/// </summary>
public interface INodeTypeSeeder
{
    /// <summary>
    /// Seeds or updates all discovered node types in the database.
    /// Disables node types that no longer exist in the assembly.
    /// </summary>
    Task SeedAsync();
}
