using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V10.Transports.Internal;

/// <summary>
/// Builds a stable correlation key while preserving JSON array and scalar semantics.
/// 在保留 JSON 数组和标量语义的同时构建稳定的关联键。
/// </summary>
internal static class OneBot10EchoKey
{
    internal static string Create(JsonNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var builder = new StringBuilder();
        Append(builder, node);
        return builder.ToString();
    }

    private static void Append(StringBuilder builder, JsonNode? node)
    {
        if (node == null)
        {
            builder.Append("null");
            return;
        }

        var jsonObject = node as JsonObject;
        if (jsonObject != null)
        {
            AppendObject(builder, jsonObject);
            return;
        }

        var jsonArray = node as JsonArray;
        if (jsonArray != null)
        {
            builder.Append('[');
            for (var index = 0; index < jsonArray.Count; index++)
            {
                if (index != 0)
                {
                    builder.Append(',');
                }

                Append(builder, jsonArray[index]);
            }

            builder.Append(']');
            return;
        }

        // Correlation keys use fixed serializer defaults so a global encoder change cannot orphan a pending request.
        // 关联键固定使用序列化默认值，避免全局编码器切换导致待处理请求无法匹配。
        builder.Append(JsonSerializer.Serialize(node));
    }

    private static void AppendObject(StringBuilder builder, JsonObject value)
    {
        var names = new List<string>();
        foreach (var property in value)
        {
            names.Add(property.Key);
        }

        names.Sort(StringComparer.Ordinal);
        builder.Append('{');
        for (var index = 0; index < names.Count; index++)
        {
            if (index != 0)
            {
                builder.Append(',');
            }

            var name = names[index];
            builder.Append(JsonSerializer.Serialize(name));
            builder.Append(':');

            JsonNode? child;
            value.TryGetPropertyValue(name, out child);
            Append(builder, child);
        }

        builder.Append('}');
    }
}
