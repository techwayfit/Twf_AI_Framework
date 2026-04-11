using TwfAiFramework.Core;
using TwfAiFramework.Nodes;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TwfAiFramework.Nodes.AI;

/// <summary>
/// Parses structured JSON output from LLM responses.
/// Handles markdown code fences, extracts JSON objects/arrays,
/// and maps extracted fields into WorkflowData keys.
///
/// Reads from WorkflowData:
///   - "llm_response" : raw text from the LLM
///
/// Writes to WorkflowData:
///   - The extracted JSON fields as individual keys
///   - "parsed_output" : the full parsed object
/// </summary>
public sealed class OutputParserNode : BaseNode
{
    private readonly string _name;

    public override string Name => _name;
    public override string Category => "AI";
    public override string Description =>
        "Parses structured JSON from LLM responses and maps fields to WorkflowData";

    /// <inheritdoc/>
    public override string IdPrefix => "parser";

    /// <inheritdoc/>
    public override IReadOnlyList<NodeData> DataIn =>
    [
        new("llm_response", typeof(string), Required: true, "Raw LLM text to parse JSON from")
    ];

    /// <inheritdoc/>
    public override IReadOnlyList<NodeData> DataOut
    {
        get
        {
            var ports = new List<NodeData>
            {
                new("parsed_output", typeof(Dictionary<string, object?>), Description: "Full parsed JSON object")
            };
            if (_fieldMapping is not null)
                foreach (var (_, dataKey) in _fieldMapping)
                    ports.Add(new NodeData(dataKey, typeof(object), Required: false, $"Mapped from JSON field"));
            return ports;
        }
    }

    private readonly Dictionary<string, string>? _fieldMapping;
    private readonly bool _strict;

    /// <param name="fieldMapping">Optional mapping of JSON keys to WorkflowData keys.
    ///     If null, all JSON keys are written directly to WorkflowData.</param>
    /// <param name="strict">If true, throw if JSON cannot be parsed. If false, pass through.</param>
    public OutputParserNode(
        string name = "OutputParser",
        Dictionary<string, string>? fieldMapping = null,
        bool strict = false)
    {
        _name = name;
        _fieldMapping = fieldMapping;
        _strict = strict;
    }

    protected override Task<WorkflowData> RunAsync(
        WorkflowData input, WorkflowContext context, NodeExecutionContext nodeCtx)
    {
        var rawResponse = input.GetRequiredString("llm_response");
        var json = ExtractJson(rawResponse);

        if (json is null)
        {
            if (_strict)
                throw new InvalidOperationException(
                    $"OutputParser: Could not extract JSON from LLM response. Raw: {rawResponse[..Math.Min(200, rawResponse.Length)]}");

            nodeCtx.Log("⚠️  No JSON found in response — passing data through unchanged");
            return Task.FromResult(input.Clone());
        }

        var output = input.Clone();

        try
        {
            var doc = JsonDocument.Parse(json);
            var parsedDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
                             ?? new Dictionary<string, object?>();

            output.Set("parsed_output", parsedDict);

            if (_fieldMapping is not null)
            {
                // Map specific JSON keys to WorkflowData keys
                foreach (var (jsonKey, dataKey) in _fieldMapping)
                {
                    if (parsedDict.TryGetValue(jsonKey, out var val))
                    {
                        output.Set(dataKey, val);
                        nodeCtx.Log($"Mapped '{jsonKey}' → '{dataKey}'");
                    }
                    else
                    {
                        nodeCtx.Log($"⚠️  JSON key '{jsonKey}' not found in response");
                    }
                }
            }
            else
            {
                // Write all JSON keys directly to WorkflowData
                foreach (var (k, v) in parsedDict)
                {
                    output.Set(k, v);
                }
                nodeCtx.Log($"Extracted {parsedDict.Count} fields from JSON");
            }

            nodeCtx.SetMetadata("parsed_keys", string.Join(", ", parsedDict.Keys));
        }
        catch (JsonException ex)
        {
            if (_strict)
                throw new InvalidOperationException($"OutputParser: Invalid JSON: {ex.Message}");

            nodeCtx.Log($"⚠️  JSON parse error: {ex.Message}");
        }

        return Task.FromResult(output);
    }

    /// <summary>
    /// Extracts a JSON object or array from text that may contain markdown fences,
    /// explanation text before/after, etc.
    /// </summary>
    private static string? ExtractJson(string text)
    {
        // Try markdown code fence first: ```json ... ``` or ``` ... ```
        var fenceMatch = Regex.Match(text, @"```(?:json)?\s*(\{[\s\S]*?\}|\[[\s\S]*?\])\s*```");
        if (fenceMatch.Success)
            return fenceMatch.Groups[1].Value.Trim();

        // Try to find a raw JSON object
        var objMatch = Regex.Match(text, @"\{[\s\S]*\}");
        if (objMatch.Success)
        {
            try
            {
                JsonDocument.Parse(objMatch.Value);
                return objMatch.Value;
            }
            catch { /* not valid JSON */ }
        }

        // Try to find a raw JSON array
        var arrMatch = Regex.Match(text, @"\[[\s\S]*\]");
        if (arrMatch.Success)
        {
            try
            {
                JsonDocument.Parse(arrMatch.Value);
                return arrMatch.Value;
            }
            catch { /* not valid JSON */ }
        }

        return null;
    }

    // ─── Convenience Factory Methods ──────────────────────────────────────────

    public static OutputParserNode WithMapping(string name,params (string JsonKey, string DataKey)[] mappings) =>
        new(name,mappings.ToDictionary(m => m.JsonKey, m => m.DataKey));

    public static OutputParserNode Strict(string name) => new(name,strict: true);
}
