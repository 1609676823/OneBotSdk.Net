using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a group join request or invitation. / 表示加群请求或邀请事件。</summary>
public sealed class GroupRequestEvent : OneBot11RequestEvent
{
    internal GroupRequestEvent()
    {
    }

    /// <summary>Gets <c>add</c> or <c>invite</c>. / 获取 <c>add</c> 或 <c>invite</c>。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the target group identifier. / 获取目标群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }
}
