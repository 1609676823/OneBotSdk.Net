using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Responses;

/// <summary>
/// Represents the two-field group-list item defined by OneBot 10.
/// 表示 OneBot 10 定义的双字段群列表项。
/// </summary>
public sealed class OneBot10GroupListItem : OneBot10JsonModel
{
    internal static OneBot10GroupListItem? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10GroupListItem
            {
                RawJson = TolerantJson.CloneObject(source),
                GroupId = TolerantJson.Int64(source, "group_id"),
                GroupName = TolerantJson.String(source, "group_name")
            };
    }

    /// <summary>Gets the group identifier when it can be parsed. / 获取尽力解析的群标识。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; private set; }

    /// <summary>Gets the group name when it can be parsed. / 获取尽力解析的群名称。</summary>
    [JsonPropertyName("group_name")]
    public string? GroupName { get; private set; }
}
