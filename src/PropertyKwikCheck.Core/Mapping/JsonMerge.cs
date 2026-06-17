using System.Text.Json.Nodes;

namespace PropertyKwikCheck.Core.Mapping;

/// <summary>
/// Merges report <c>data</c> patches the way the prototype does:
/// <c>Object.assign({}, old, patch)</c> — top-level keys from the patch overwrite
/// the existing object (spec §7).
/// </summary>
public static class JsonMerge
{
    public static JsonObject Merge(JsonObject? existing, JsonObject? patch)
    {
        var result = new JsonObject();
        if (existing is not null)
            foreach (var kv in existing)
                result[kv.Key] = kv.Value?.DeepClone();

        if (patch is not null)
            foreach (var kv in patch)
                result[kv.Key] = kv.Value?.DeepClone();

        return result;
    }
}
