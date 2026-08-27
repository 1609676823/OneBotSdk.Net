using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Centralizes tolerant action-data parsing used by the strongly typed client.
/// 集中管理强类型客户端使用的容错动作 data 解析。
/// </summary>
internal static class OneBot11ResponseDataParsers
{
    internal static IReadOnlyList<T>? ParseList<T>(JsonNode? node, Func<JsonNode?, T?> itemParser)
        where T : class
    {
        var source = TolerantJson.Array(node);
        if (source == null)
        {
            return null;
        }

        var result = new List<T>();
        var count = TolerantJson.Count(source);
        for (var index = 0; index < count; index++)
        {
            try
            {
                var item = TolerantJson.Item(source, index);
                var parsed = itemParser(item);
                if (parsed != null)
                {
                    result.Add(parsed);
                }
            }
            catch (Exception)
            {
                // A malformed collection item is skipped without discarding successfully parsed siblings.
                // 跳过异常集合项，同时保留已成功解析的其它同级项。
            }
        }

        return result;
    }
}
