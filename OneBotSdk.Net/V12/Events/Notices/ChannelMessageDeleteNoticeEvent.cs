using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a channel-message deletion notice. / 表示频道消息删除通知。</summary>
public sealed class ChannelMessageDeleteNoticeEvent : OneBot12NoticeEvent
{
    internal ChannelMessageDeleteNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the containing guild identifier. / 获取所属群组标识。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; internal set; }

    /// <summary>Gets the channel identifier. / 获取频道标识。</summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; internal set; }

    /// <summary>Gets the deleted message identifier. / 获取已删除的消息标识。</summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; internal set; }

    /// <summary>Gets the message author identifier. / 获取消息作者标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
