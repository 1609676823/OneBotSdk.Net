using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

internal static class OneBot12ResponseDataParsers
{
    internal static IReadOnlyList<T> ParseList<T>(JsonNode? node, Func<JsonNode?, T?> parser)
        where T : class
    {
        var result = new List<T>();
        var array = TolerantJson.Array(node);
        for (var index = 0; index < TolerantJson.Count(array); index++)
        {
            try
            {
                // Invalid elements are skipped without hiding valid siblings or RawData.
                // 跳过异常元素，同时不隐藏有效同级元素或 RawData。
                var item = parser(TolerantJson.Item(array, index));
                if (item != null)
                {
                    result.Add(item);
                }
            }
            catch (Exception)
            {
                // Continue parsing the remaining elements.
                // 继续解析其余元素。
            }
        }

        return result.AsReadOnly();
    }

    internal static IReadOnlyList<string> ParseStrings(JsonNode? node)
    {
        var result = new List<string>();
        var array = TolerantJson.Array(node);
        for (var index = 0; index < TolerantJson.Count(array); index++)
        {
            var value = TolerantJson.String(TolerantJson.Item(array, index));
            if (value != null)
            {
                result.Add(value);
            }
        }

        return result.AsReadOnly();
    }

    internal static IReadOnlyList<JsonObject> ParseObjects(JsonNode? node)
    {
        return ParseList(node, item =>
        {
            var value = TolerantJson.Object(item);
            return value == null ? null : TolerantJson.CloneObject(value);
        });
    }
}
