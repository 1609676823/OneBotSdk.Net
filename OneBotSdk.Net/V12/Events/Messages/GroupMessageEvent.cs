using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a standard group-message event. / 表示标准群消息事件。</summary>
public sealed class GroupMessageEvent : OneBot12MessageEvent
{
    internal GroupMessageEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }

    /// <summary>Gets the group identifier. / 获取群标识。</summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; internal set; }
}
