using TwfAiFramework.Core;
using TwfAiFramework.Nodes.AI;
using TwfAiFramework.Nodes.Control;
using TwfAiFramework.Nodes.Data;
using TwfAiFramework.Nodes.IO;
using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Creates a minimal skeleton instance of each node type and reads its
/// DataIn / DataOut directly from the core implementation.
/// This ensures the UI always reflects the actual data contract.
/// </summary>
public static class NodeDataMetadataProvider
{
    /// <summary>
    /// Returns (DataInputs, DataOutputs) for the given node type string,
    /// or empty lists if the type is unknown or has no declared ports.
    /// </summary>
    public static (List<DataPortInfo> inputs, List<DataPortInfo> outputs) GetPorts(string nodeType)
    {
        var node = CreateSkeleton(nodeType);
        if (node is null)
            return ([], []);

        var inputs  = node.DataIn .Select(ToDataPortInfo).ToList();
        var outputs = node.DataOut.Select(ToDataPortInfo).ToList();
        return (inputs, outputs);
    }

    private static DataPortInfo ToDataPortInfo(NodeData p) => new()
    {
        Key         = p.Key,
        Required    = p.Required,
        IsDynamic   = false,
        Description = p.Description,
    };

    /// <summary>
    /// Instantiates each node with representative defaults — enough to
    /// materialise the port lists without making real API calls.
    /// Nodes with fully dynamic ports (depend on runtime template content)
    /// return a single placeholder DataPortInfo with IsDynamic = true.
    /// </summary>
    private static INode? CreateSkeleton(string nodeType) => nodeType switch
    {
        // ── AI ───────────────────────────────────────────────────────────────
        "LlmNode" => new LlmNode("_", new LlmConfig()),

        // PromptBuilderNode ports depend on template content — return a representative
        // instance with a sample placeholder so the schema shows *something* useful.
        "PromptBuilderNode" => new PromptBuilderNode("_",
            promptTemplate: "{{input}}",
            systemTemplate: null),

        "EmbeddingNode"    => new EmbeddingNode("_", new EmbeddingConfig()),
        "OutputParserNode" => new OutputParserNode("_"),

        // ── Control ──────────────────────────────────────────────────────────
        "BranchNode"      => new BranchNode("_", valueKey: "value"),
        "ConditionNode"   => new ConditionNode("_"),           // empty = no dynamic flags
        "DelayNode"       => new DelayNode(TimeSpan.Zero),
        "MergeNode"       => new MergeNode("_", outputKey: "merged"),
        "LogNode"         => new LogNode("_"),
        "LoopNode"        => new LoopNode("_"),
        "ErrorRouteNode"  => new ErrorRouteNode(),

        // StartNode / EndNode / ErrorNode / SubWorkflowNode have no data ports
        "StartNode"       => null,
        "EndNode"         => null,
        "ErrorNode"       => null,
        "SubWorkflowNode" => null,

        // ── Data ─────────────────────────────────────────────────────────────
        // SetVariableNode ports depend on the assignments map — show dynamic placeholder
        "SetVariableNode"  => new SetVariableNode("_", new Dictionary<string, object?> { ["output_key"] = "example" }),
        "TransformNode"    => null,   // fully code-defined, no static ports
        "DataMapperNode"   => null,   // ports depend on mapping config
        "FilterNode"       => null,   // ports depend on rules config
        "ChunkTextNode"    => new ChunkTextNode(),
        "MemoryNode"       => null,   // ports depend on mode + keys config

        // ── IO ───────────────────────────────────────────────────────────────
        "HttpRequestNode"  => new HttpRequestNode("_", new HttpRequestConfig()),
        "FileReadNode"     => new FileReaderNode(),
        "FileWriteNode"    => new FileWriterNode("output.txt"),

        // ── Visual ───────────────────────────────────────────────────────────
        "ContainerNode" => null,
        "NoteNode"      => null,

        _ => null,
    };
}
