using System.Reflection;
using Microsoft.Extensions.Logging;
using TwfAiFramework.Core;
using TwfAiFramework.Nodes;

namespace TwfAiFramework.Web.Services.Schema;

/// <summary>
/// Discovers NodeParameterSchema definitions by scanning the core assembly for
/// every INode subclass that exposes a static Schema property.
/// No manual registration is required — adding a Schema property to a node class
/// is sufficient.
/// The assembly is scanned once (lazily) and the results are cached for the lifetime
/// of the service (Singleton).
/// </summary>
public class ReflectionNodeSchemaProvider : INodeSchemaProvider
{
    private readonly ILogger<ReflectionNodeSchemaProvider> _logger;

    /// <summary>
    /// Lazy-loaded discovered schemas. Thread-safe initialization.
    /// </summary>
    private readonly Lazy<IReadOnlyList<(NodeParameterSchema Schema, Type NodeClass)>> _discovered;

    public ReflectionNodeSchemaProvider(ILogger<ReflectionNodeSchemaProvider> logger)
    {
     _logger = logger;
  _discovered = new Lazy<IReadOnlyList<(NodeParameterSchema Schema, Type NodeClass)>>(
            DiscoverSchemas,
         LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Dictionary<string, NodeParameterSchema> GetAllSchemas()
    {
        return _discovered.Value.ToDictionary(x => x.Schema.NodeType, x => x.Schema);
    }

    public Type? GetNodeClass(string nodeType)
    {
        return _discovered.Value
       .FirstOrDefault(x => x.Schema.NodeType.Equals(nodeType, StringComparison.OrdinalIgnoreCase))
   .NodeClass;
    }

    public NodeParameterSchema? GetSchema(string nodeType)
    {
        return _discovered.Value
       .FirstOrDefault(x => x.Schema.NodeType.Equals(nodeType, StringComparison.OrdinalIgnoreCase))
            .Schema;
    }

    public IReadOnlyCollection<string> GetRegisteredNodeTypes()
 {
        return _discovered.Value.Select(x => x.Schema.NodeType).ToList();
}

    /// <summary>
    /// Discovers all node schemas via reflection.
    /// Called once when the Lazy is first accessed.
/// </summary>
    private IReadOnlyList<(NodeParameterSchema Schema, Type NodeClass)> DiscoverSchemas()
    {
        _logger.LogInformation("Starting node schema discovery from core assembly");

     var schemas = typeof(BaseNode).Assembly
            .GetTypes()
  .Where(t => t.IsClass && !t.IsAbstract && typeof(INode).IsAssignableFrom(t))
   .Select(t => (
            schema: t.GetProperty("Schema", BindingFlags.Public | BindingFlags.Static)
   ?.GetValue(null) as NodeParameterSchema,
       type: t
   ))
 .Where(x => x.schema is not null)
       .Select(x => (x.schema!, x.type))
            .ToList();

   _logger.LogInformation(
            "Node schema discovery completed: {Count} schemas discovered",
     schemas.Count);

  return schemas;
    }
}
