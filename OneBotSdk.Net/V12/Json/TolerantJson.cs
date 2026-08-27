using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Json;

/// <summary>
/// Reads OneBot 12 fields independently so type drift remains local to one field.
/// 独立读取 OneBot 12 字段，使类型漂移仅影响单个字段。
/// </summary>
internal static class TolerantJson
{
    internal static JsonObject? Object(JsonNode? node) => node as JsonObject;

    internal static JsonArray? Array(JsonNode? node) => node as JsonArray;

    internal static JsonNode? Node(JsonObject? source, string name)
    {
        if (source == null)
        {
            return null;
        }

        try
        {
            JsonNode? value;
            return source.TryGetPropertyValue(name, out value) ? value : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static string? String(JsonObject? source, string name) => String(Node(source, name));

    internal static string? String(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            return node.GetValue<string>();
        }
        catch (Exception)
        {
            try
            {
                if (node is JsonValue value && value.TryGetValue<JsonElement>(out var element))
                {
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.Number:
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            return element.GetRawText();
                    }
                }

                return node is JsonValue ? node.ToString() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    internal static long? Int64(JsonObject? source, string name) => Int64(Node(source, name));

    internal static long? Int64(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<long>(out var longValue))
                {
                    return longValue;
                }

                if (value.TryGetValue<int>(out var intValue))
                {
                    return intValue;
                }

                if (value.TryGetValue<decimal>(out var decimalValue) &&
                    decimal.Truncate(decimalValue) == decimalValue &&
                    decimalValue >= long.MinValue && decimalValue <= long.MaxValue)
                {
                    return (long)decimalValue;
                }
            }

            return long.TryParse(String(node), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static double? Double(JsonObject? source, string name) => Double(Node(source, name));

    internal static double? Double(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<double>(out var doubleValue))
                {
                    return doubleValue;
                }

                if (value.TryGetValue<long>(out var longValue))
                {
                    return longValue;
                }
            }

            return double.TryParse(String(node), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static bool? Boolean(JsonObject? source, string name) => Boolean(Node(source, name));

    internal static bool? Boolean(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            if (node is JsonValue value)
            {
                if (value.TryGetValue<bool>(out var boolValue))
                {
                    return boolValue;
                }

                if (value.TryGetValue<long>(out var number))
                {
                    return number == 1 ? true : number == 0 ? false : null;
                }
            }

            var text = String(node);
            if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) || text == "1")
            {
                return true;
            }

            if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) || text == "0")
            {
                return false;
            }
        }
        catch (Exception)
        {
            // Ignore only this field and keep the containing object usable.
            // 仅忽略当前字段，并保持其所在对象可用。
        }

        return null;
    }

    internal static byte[]? Bytes(JsonObject? source, string name)
    {
        var text = String(source, name);
        if (text == null)
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(text);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    internal static JsonNode? Clone(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            return OneBot12Json.Clone(node);
        }
        catch (Exception)
        {
            if (node is JsonObject objectValue)
            {
                var clone = new JsonObject();
                foreach (var property in objectValue)
                {
                    try
                    {
                        clone[property.Key] = Clone(property.Value);
                    }
                    catch (Exception)
                    {
                        // Continue with serializable extension siblings.
                        // 继续保留可序列化的其它扩展字段。
                    }
                }

                return clone;
            }

            if (node is JsonArray arrayValue)
            {
                var clone = new JsonArray();
                for (var index = 0; index < Count(arrayValue); index++)
                {
                    clone.Add(Clone(Item(arrayValue, index)));
                }

                return clone;
            }

            return null;
        }
    }

    internal static JsonObject CloneObject(JsonObject source) => Clone(source) as JsonObject ?? new JsonObject();

    internal static int Count(JsonArray? source)
    {
        try
        {
            return source?.Count ?? 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    internal static JsonNode? Item(JsonArray? source, int index)
    {
        try
        {
            return source?[index];
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static IReadOnlyDictionary<string, string> StringMap(JsonNode? node)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var source = Object(node);
        if (source == null)
        {
            return result;
        }

        foreach (var property in source)
        {
            var value = String(property.Value);
            if (value != null)
            {
                result[property.Key] = value;
            }
        }

        return result;
    }
}
