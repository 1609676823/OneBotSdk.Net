using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a channel-delete notice. / 表示频道删除通知。</summary>
public sealed class ChannelDeleteNoticeEvent : OneBot12NoticeEvent
{
    internal ChannelDeleteNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the guild that contained the deleted channel. / 获取原先包含已删除频道的群组标识。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; internal set; }

    /// <summary>Gets the deleted channel identifier. / 获取已删除的频道标识。</summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
