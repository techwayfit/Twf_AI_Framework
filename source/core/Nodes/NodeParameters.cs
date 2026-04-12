using System.Text.Json;

namespace TwfAiFramework.Nodes;

/// <summary>
/// Shared parameter-parsing helpers used by all node dictionary constructors.
/// Handles the JSON deserialization cases that arise when parameters arrive
/// from the web designer (JsonElement) or from code (boxed primitives).
/// </summary>
public static class NodeParameters
{
    /// <summary>
    /// Retrieves a string value from the parameter dictionary.
    /// </summary>
    /// <param name="p">The parameter dictionary to search.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="def">The default value to return if the key is not found or the value is null.</param>
    /// <returns>The string value associated with the key, or the default value if not found.</returns>
    public static string? GetString(IReadOnlyDictionary<string, object?> p, string key, string? def = null)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is JsonElement je) return je.GetString() ?? def;
        return raw.ToString() ?? def;
    }
    /// <summary>
    /// Retrieves a boolean value from the parameter dictionary, handling various input formats.
    /// </summary>
    /// <param name="p">The parameter dictionary to search.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="def">The default value to return if the key is not found or the value is null.</param>
    /// <returns>The boolean value associated with the key, or the default value if not found.</returns>
    public static bool GetBool(IReadOnlyDictionary<string, object?> p, string key, bool def = false)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is bool b) return b;
        if (raw is JsonElement je)  
        {
            if (je.ValueKind == JsonValueKind.True)  return true;
            if (je.ValueKind == JsonValueKind.False) return false;
        }
        return bool.TryParse(raw.ToString(), out var parsed) ? parsed : def;
    }
    /// <summary>
    /// Retrieves an integer value from the parameter dictionary, handling various input formats.
    /// </summary>
    /// <param name="p">The parameter dictionary to search.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="def">The default value to return if the key is not found or the value is null.</param>
    /// <returns>The integer value associated with the key, or the default value if not found.</returns>
    public static int GetInt(IReadOnlyDictionary<string, object?> p, string key, int def = 0)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is JsonElement je && je.TryGetInt32(out var v)) return v;
        if (raw is int i) return i;
        return int.TryParse(raw.ToString(), out var parsed) ? parsed : def;
    }

    /// <summary>
    /// Retrieves a double value from the parameter dictionary, handling various input formats.
    /// </summary>
    /// <param name="p">The parameter dictionary to search.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="def">The default value to return if the key is not found or the value is null.</param>
    /// <returns>The double value associated with the key, or the default value if not found.</returns>
    public static double GetDouble(IReadOnlyDictionary<string, object?> p, string key, double def = 0)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return def;
        if (raw is JsonElement je && je.TryGetDouble(out var v)) return v;
        if (raw is double d) return d;
        if (raw is float f)  return f;
        return double.TryParse(raw.ToString(), out var parsed) ? parsed : def;
    }
    /// <summary>
    /// Retrieves a dictionary of strings from the parameter dictionary, handling various input formats.
    /// </summary>
    /// <param name="p">The parameter dictionary to search.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <returns>The dictionary of strings associated with the key, or null if not found.</returns>
    public static Dictionary<string, string>? GetStringDict(IReadOnlyDictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return null;

        if (raw is JsonElement je && je.ValueKind == JsonValueKind.Object)
            return je.EnumerateObject()
                     .Where(prop => prop.Value.ValueKind == JsonValueKind.String)
                     .ToDictionary(prop => prop.Name, prop => prop.Value.GetString()!);

        if (raw is Dictionary<string, string> dict) return dict;
        if (raw is Dictionary<string, object?> objDict)
            return objDict.Where(kv => kv.Value is not null)
                          .ToDictionary(kv => kv.Key, kv => kv.Value!.ToString()!);

        return null;
    }

    /// <summary>
    /// Retrieves a list of strings from the parameter dictionary, handling various input formats.
    /// </summary>
    /// <param name="p">The parameter dictionary to search.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <returns>The list of strings associated with the key, or null if not found.</returns>

    public static List<string>? GetStringList(IReadOnlyDictionary<string, object?> p, string key)
    {
        if (!p.TryGetValue(key, out var raw) || raw is null) return null;

        if (raw is JsonElement je && je.ValueKind == JsonValueKind.Array)
            return je.EnumerateArray()
                     .Where(e => e.ValueKind == JsonValueKind.String)
                     .Select(e => e.GetString()!)
                     .ToList();

        if (raw is List<string> list)         return list;
        if (raw is string[] arr)              return arr.ToList();
        if (raw is IEnumerable<string> ienum) return ienum.ToList();

        return null;
    }
}
