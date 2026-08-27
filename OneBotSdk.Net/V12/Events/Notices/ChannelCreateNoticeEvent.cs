using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a channel-create notice. / 表示频道创建通知。</summary>
public sealed class ChannelCreateNoticeEvent : OneBot12NoticeEvent
{
    internal ChannelCreateNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the guild containing the created channel. / 获取包含已创建频道的群组标识。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; internal set; }

    /// <summary>Gets the created channel identifier. / 获取已创建的频道标识。</summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
