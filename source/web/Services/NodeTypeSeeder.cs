using System.Text.Json;
using System.Text.RegularExpressions;
using TwfAiFramework.Core;
using TwfAiFramework.Web.Data;
using TwfAiFramework.Web.Repositories;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Seeds (and keeps up-to-date) the NodeTypes table from the core assembly.
///
/// Discovery is fully reflection-driven: every INode subclass with a static Schema
/// property is automatically picked up on startup — no manual registration needed.
///
/// The only web-layer input is <see cref="GetPaletteOverrides"/>, which carries
/// presentation-only values (display name, colour, icon) for nodes that need
/// a custom look in the palette. Nodes without an override get category defaults.
///
/// Nodes that exist in the DB but are no longer in the assembly are disabled
/// (IsEnabled = false) rather than deleted, so existing workflow definitions
/// referencing them are not broken.
/// </summary>
public static class NodeTypeSeeder
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(INodeTypeRepository repo)
    {
        var schemas  = NodeSchemaProvider.GetAllSchemas();
        var existing = (await repo.GetAllAsync())
            .ToDictionary(e => e.NodeType, StringComparer.OrdinalIgnoreCase);
        var overrides = GetPaletteOverrides();

        // ── Upsert all nodes discovered via reflection ────────────────────────
        foreach (var schema in schemas.Values)
        {
            var nodeType = schema.NodeType;
            overrides.TryGetValue(nodeType, out var pal);

            var (category, idPrefix) = GetNodeInstanceMeta(nodeType);
            var fullTypeName = GetFullTypeName(nodeType);

            var name  = pal?.Name  ?? DeriveDisplayName(nodeType);
            var color = pal?.Color ?? GetCategoryColor(category);
            var icon  = pal?.Icon  ?? GetCategoryIcon(category);

            // Populate DataIn/DataOut from live port declarations
            var (inputs, outputs) = NodeDataMetadataProvider.GetPorts(nodeType);
            schema.DataInputs  = inputs;
            schema.DataOutputs = outputs;
            var schemaJson = JsonSerializer.Serialize(schema, _jsonOptions);

            if (existing.TryGetValue(nodeType, out var entity))
            {
                entity.Name         = name;
                entity.Category     = category;
                entity.Description  = schema.Description;
                entity.Color        = color;
                entity.Icon         = icon;
                entity.SchemaJson   = schemaJson;
                entity.IsEnabled    = true;
                entity.IdPrefix     = idPrefix;
                entity.FullTypeName = fullTypeName;
                await repo.UpdateAsync(entity);
            }
            else
            {
                await repo.CreateAsync(new NodeTypeEntity
                {
                    NodeType     = nodeType,
                    Name         = name,
                    Category     = category,
                    Description  = schema.Description,
                    Color        = color,
                    Icon         = icon,
                    SchemaJson   = schemaJson,
                    IsEnabled    = true,
                    IdPrefix     = idPrefix,
                    FullTypeName = fullTypeName,
                });
            }
        }

        // ── Disable nodes no longer present in the assembly ───────────────────
        foreach (var (nodeType, entity) in existing)
        {
            if (!schemas.ContainsKey(nodeType) && entity.IsEnabled)
            {
                entity.IsEnabled = false;
                await repo.UpdateAsync(entity);
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates a skeleton node (via Dictionary constructor) to read
    /// its Category and IdPrefix — instance-level properties not in the schema.
    /// </summary>
    private static (string category, string idPrefix) GetNodeInstanceMeta(string nodeType)
    {
        var type = NodeSchemaProvider.GetNodeClass(nodeType);
        if (type is null) return ("Other", "node");

        try
        {
            var ctor = type.GetConstructor([typeof(Dictionary<string, object?>)]);
            if (ctor is not null)
            {
                var node = (INode)ctor.Invoke([new Dictionary<string, object?>()]);
                return (node.Category, node.IdPrefix);
            }
            var defaultCtor = type.GetConstructor(Type.EmptyTypes);
            if (defaultCtor is not null)
            {
                var node = (INode)defaultCtor.Invoke([]);
                return (node.Category, node.IdPrefix);
            }
        }
        catch { /* skeleton instantiation failed — use fallback */ }

        return ("Other", "node");
    }

    private static string? GetFullTypeName(string nodeType)
    {
        var type = NodeSchemaProvider.GetNodeClass(nodeType);
        return type is null ? null : $"{type.FullName}, {type.Assembly.GetName().Name}";
    }

    /// <summary>
    /// Converts a PascalCase type name to a display name.
    /// "HttpRequestNode" → "Http Request", "LlmNode" → "Llm".
    /// Override via <see cref="GetPaletteOverrides"/> for custom display names.
    /// </summary>
    private static string DeriveDisplayName(string nodeType)
    {
        var name = nodeType.EndsWith("Node", StringComparison.OrdinalIgnoreCase)
            ? nodeType[..^4]
            : nodeType;
        return Regex.Replace(name, "(?<!^)([A-Z])", " $1").Trim();
    }

    private static string GetCategoryColor(string category) => category switch
    {
        "AI"      => "#4A90E2",
        "Control" => "#F5A623",
        "Data"    => "#7ED321",
        "IO"      => "#BD10E0",
        "Visual"  => "#6366f1",
        _         => "#6c757d",
    };

    private static string GetCategoryIcon(string category) => category switch
    {
        "AI"      => "bi-cpu",
        "Control" => "bi-gear",
        "Data"    => "bi-table",
        "IO"      => "bi-globe",
        "Visual"  => "bi-bounding-box",
        _         => "bi-box",
    };

    // ── Palette overrides (web layer only) ────────────────────────────────────
    // Add an entry only when you need a custom display name, colour, or icon.
    // All three fields are optional — null means "use the category default".

    private sealed record PaletteOverride(
        string? Name  = null,
        string? Color = null,
        string? Icon  = null);

    private static Dictionary<string, PaletteOverride> GetPaletteOverrides() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Control ───────────────────────────────────────────────────────
            ["StartNode"]       = new("Start",         "#2ecc71", "bi-play-circle-fill"),
            ["EndNode"]         = new("End",            "#e74c3c", "bi-stop-circle-fill"),
            ["ErrorNode"]       = new("Error Handler",  "#e74c3c", "bi-exclamation-triangle-fill"),
            ["ErrorRouteNode"]  = new("Error Route",    "#e67e22", "bi-arrow-right-circle-fill"),
            ["ConditionNode"]   = new(Icon: "bi-question-diamond"),
            ["BranchNode"]      = new(Icon: "bi-signpost-split"),
            ["SubWorkflowNode"] = new("Sub Workflow",   "#8e44ad", "bi-box-arrow-in-right"),
            ["LoopNode"]        = new("Loop (ForEach)", "#f39c12", "bi-arrow-repeat"),
            ["DelayNode"]       = new(Icon: "bi-clock"),
            ["MergeNode"]       = new(Icon: "bi-intersect"),
            ["LogNode"]         = new(Icon: "bi-journal-text"),

            // ── Data ──────────────────────────────────────────────────────────
            ["SetVariableNode"] = new("Set Variable", Icon: "bi-pencil"),
            ["DataMapperNode"]  = new("Data Mapper",  Icon: "bi-map"),
            ["FilterNode"]      = new(Icon: "bi-funnel"),
            ["ChunkTextNode"]   = new("Chunk Text",   Icon: "bi-file-break"),
            ["MemoryNode"]      = new(Icon: "bi-memory"),

            // ── IO ────────────────────────────────────────────────────────────
            ["FileReaderNode"]  = new("File Read",     Icon: "bi-file-earmark-text"),
            ["FileWriterNode"]  = new("File Write",    Icon: "bi-file-earmark-arrow-down"),
            ["HttpRequestNode"] = new("HTTP Request",  Icon: "bi-globe"),

            // ── AI ────────────────────────────────────────────────────────────
            ["LlmNode"]           = new("LLM",            Icon: "bi-chat-left-dots"),
            ["PromptBuilderNode"] = new("Prompt Builder", Icon: "bi-pencil-square"),
            ["EmbeddingNode"]     = new(Icon: "bi-diagram-2"),
            ["OutputParserNode"]  = new("Output Parser",  Icon: "bi-list-check"),

            // ── Visual ────────────────────────────────────────────────────────
            ["ContainerNode"] = new("Container", "#6366f1", "bi-bounding-box"),
            ["NoteNode"]      = new("Note",      "#f59e0b", "bi-sticky"),
        };
}
