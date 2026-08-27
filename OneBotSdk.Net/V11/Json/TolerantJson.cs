using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V11.Json;

/// <summary>
/// Reads each JSON field independently so malformed fields cannot invalidate their siblings.
/// 独立读取每个 JSON 字段，避免异常字段使其它同级字段失效。
/// </summary>
internal static class TolerantJson
{
    internal static JsonObject? Object(JsonNode? node)
    {
        return node as JsonObject;
    }

    internal static JsonArray? Array(JsonNode? node)
    {
        return node as JsonArray;
    }

    internal static string? String(JsonObject? source, string propertyName)
    {
        return String(Node(source, propertyName));
    }

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
            // A scalar type drift is converted to its invariant JSON scalar representation.
            // 标量发生类型漂移时，转换为其不受区域影响的 JSON 标量表示。
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

    internal static long? Int64(JsonObject? source, string propertyName)
    {
        return Int64(Node(source, propertyName));
    }

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

                if (value.TryGetValue<decimal>(out var decimalValue) && decimal.Truncate(decimalValue) == decimalValue && decimalValue <= long.MaxValue && decimalValue >= long.MinValue)
                {
                    return (long)decimalValue;
                }
            }

            var text = String(node);
            return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static bool? Boolean(JsonObject? source, string propertyName)
    {
        return Boolean(Node(source, propertyName));
    }

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
                if (value.TryGetValue<bool>(out var booleanValue))
                {
                    return booleanValue;
                }

                if (value.TryGetValue<long>(out var numberValue))
                {
                    return numberValue == 1 ? true : numberValue == 0 ? false : null;
                }
            }

            var text = String(node);
            if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "1", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "no", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "0", StringComparison.Ordinal))
            {
                return false;
            }
        }
        catch (Exception)
        {
            // The field is deliberately ignored while the rest of the object remains usable.
            // 有意忽略异常字段，同时保证对象其余部分仍可使用。
        }

        return null;
    }

    internal static JsonNode? Node(JsonObject? source, string propertyName)
    {
        if (source == null)
        {
            return null;
        }

        try
        {
            JsonNode? value;
            return source.TryGetPropertyValue(propertyName, out value) ? value : null;
        }
        catch (Exception)
        {
            // Property lookup is isolated just like scalar conversion.
            // 属性查找与标量转换一样进行异常隔离。
            return null;
        }
    }

    internal static bool ContainsProperty(JsonObject? source, string propertyName)
    {
        if (source == null)
        {
            return false;
        }

        try
        {
            return source.ContainsKey(propertyName);
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static int Count(JsonArray? source)
    {
        if (source == null)
        {
            return 0;
        }

        try
        {
            return source.Count;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    internal static JsonNode? Item(JsonArray? source, int index)
    {
        if (source == null)
        {
            return null;
        }

        try
        {
            return source[index];
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static T? Parse<T>(JsonObject? source, string propertyName, Func<JsonNode?, T?> parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        try
        {
            return parser(Node(source, propertyName));
        }
        catch (Exception)
        {
            // A nested parser failure affects only its own property.
            // 嵌套解析器失败时仅影响它对应的属性。
            return default;
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
            return OneBot11Json.Clone(node);
        }
        catch (Exception)
        {
            if (node is JsonObject objectValue)
            {
                return CloneObjectFields(objectValue);
            }

            if (node is JsonArray arrayValue)
            {
                return CloneArrayItems(arrayValue);
            }

            return null;
        }
    }

    internal static JsonObject CloneObject(JsonObject source)
    {
        return Clone(source) as JsonObject ?? new JsonObject();
    }

    private static JsonObject CloneObjectFields(JsonObject source)
    {
        // Preserve every serializable sibling even if one implementation extension cannot be cloned.
        // 即使某个实现扩展无法克隆，也保留其它所有可序列化的同级字段。
        var partialClone = new JsonObject();
        try
        {
            foreach (var property in source)
            {
                try
                {
                    var clonedValue = Clone(property.Value);
                    if (property.Value == null || clonedValue != null)
                    {
                        partialClone[property.Key] = clonedValue;
                    }
                }
                catch (Exception)
                {
                    // Continue cloning the remaining extension fields.
                    // 继续克隆其余扩展字段。
                }
            }
        }
        catch (Exception)
        {
            // Concurrent mutation may stop enumeration; fields already cloned remain useful.
            // 并发修改可能中止枚举；已经克隆的字段仍然可用。
        }

        return partialClone;
    }

    private static JsonArray CloneArrayItems(JsonArray source)
    {
        // Keep array positions stable; an uncloneable item becomes null while valid siblings survive.
        // 保持数组位置稳定；无法克隆的元素变为 null，其它有效同级元素继续保留。
        var partialClone = new JsonArray();
        var count = Count(source);
        for (var index = 0; index < count; index++)
        {
            try
            {
                partialClone.Add(Clone(Item(source, index)));
            }
            catch (Exception)
            {
                partialClone.Add(null);
            }
        }

        return partialClone;
    }
}
