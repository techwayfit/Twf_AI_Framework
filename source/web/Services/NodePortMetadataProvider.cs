using TwfAiFramework.Core;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Creates a minimal skeleton instance of each node type and reads its
/// DataIn / DataOut directly from the core implementation.
/// Uses the reflection registry in NodeSchemaProvider — no switch-case required.
/// </summary>
public static class NodeDataMetadataProvider
{
    /// <summary>
    /// Returns (DataInputs, DataOutputs) for the given node type string,
    /// or empty lists if the type is unknown or has no declared ports.
    /// </summary>
    public static (List<DataPortInfo> inputs, List<DataPortInfo> outputs) GetPorts(string nodeType)
    {
        var type = NodeSchemaProvider.GetNodeClass(nodeType);
        if (type is null) return ([], []);

        try
        {
            var ctor = type.GetConstructor([typeof(Dictionary<string, object?>)]);
            if (ctor is null) return ([], []);

            var node = (INode)ctor.Invoke([new Dictionary<string, object?>()]);
            return (
                node.DataIn .Select(ToPortInfo).ToList(),
                node.DataOut.Select(ToPortInfo).ToList()
            );
        }
        catch
        {
            return ([], []);
        }
    }

    private static DataPortInfo ToPortInfo(NodeData p) => new()
    {
        Key         = p.Key,
        Required    = p.Required,
        IsDynamic   = false,
        Description = p.Description,
    };
}
