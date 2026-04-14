using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using TwfAiFramework.Web.Models;
using TwfAiFramework.Web.Services.VariableResolution;

namespace TwfAiFramework.Web.Services.NodeFactory;

/// <summary>
/// Creates INode instances using reflection and caches constructors for performance.
/// Discovers all INode implementations from the core assembly at startup.
/// </summary>
public class ReflectionNodeFactory : INodeFactory
{
    private readonly IVariableResolver _variableResolver;
    private readonly ILogger<ReflectionNodeFactory> _logger;

    /// <summary>
    /// Registry of all INode implementations in the core assembly, keyed by class name.
    /// Built once at startup via reflection — no switch-case required.
    /// </summary>
    private readonly IReadOnlyDictionary<string, Type> _nodeTypeRegistry;

    /// <summary>
    /// Cached compiled constructor delegates for improved performance.
    /// Avoids repeated reflection calls during workflow execution.
    /// </summary>
    private readonly ConcurrentDictionary<Type, Func<Dictionary<string, object?>, INode>> _constructorCache;

    public ReflectionNodeFactory(
 IVariableResolver variableResolver,
   ILogger<ReflectionNodeFactory> logger)
    {
     _variableResolver = variableResolver;
        _logger = logger;
        _constructorCache = new ConcurrentDictionary<Type, Func<Dictionary<string, object?>, INode>>();

        // Discover all INode implementations from the core assembly
        _nodeTypeRegistry = typeof(BaseNode).Assembly
 .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && typeof(INode).IsAssignableFrom(t))
            .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation(
"Node factory initialized with {Count} registered node types",
       _nodeTypeRegistry.Count);
    }

    public INode? CreateNode(NodeDefinition nodeDefinition, WorkflowData workflowData)
    {
        if (!_nodeTypeRegistry.TryGetValue(nodeDefinition.Type, out var nodeType))
        {
         _logger.LogWarning(
         "No INode implementation found for type '{Type}'",
    nodeDefinition.Type);
            return null;
        }

     // Get or compile the constructor delegate
      var factory = _constructorCache.GetOrAdd(nodeType, type =>
 {
            var ctor = type.GetConstructor([typeof(Dictionary<string, object?>)]);
       if (ctor is null)
         {
        throw new InvalidOperationException(
     $"Node '{type.Name}' has no Dictionary<string, object?> constructor");
      }

      // Create a compiled delegate for better performance
          return parameters => (INode)ctor.Invoke([parameters]);
        });

        try
    {
            // Resolve variables in parameters
            var resolvedParams = _variableResolver.ResolveParameters(
             nodeDefinition.Parameters,
     workflowData);

   // Inject node name so constructors can read it
            resolvedParams["name"] = nodeDefinition.Name;

 // Create the node instance using cached factory
   var node = factory(resolvedParams);

       _logger.LogDebug(
    "Created node instance: Type={Type}, Name={Name}",
        nodeDefinition.Type,
  nodeDefinition.Name);

         return node;
   }
        catch (Exception ex)
        {
  _logger.LogError(
ex,
      "Failed to instantiate node '{Type}' (Name: {Name})",
     nodeDefinition.Type,
         nodeDefinition.Name);
         return null;
    }
    }

    public bool IsNodeTypeRegistered(string nodeType)
    {
        return _nodeTypeRegistry.ContainsKey(nodeType);
    }

    public IReadOnlyCollection<string> GetRegisteredNodeTypes()
    {
        return _nodeTypeRegistry.Keys.ToList();
    }
}
