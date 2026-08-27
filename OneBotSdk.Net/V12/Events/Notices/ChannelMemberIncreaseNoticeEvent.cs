using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a channel-member increase notice. / 表示频道成员增加通知。</summary>
public sealed class ChannelMemberIncreaseNoticeEvent : OneBot12NoticeEvent
{
    internal ChannelMemberIncreaseNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the containing guild identifier. / 获取所属群组标识。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; internal set; }

    /// <summary>Gets the channel identifier. / 获取频道标识。</summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; internal set; }

    /// <summary>Gets the joining member identifier. / 获取加入的成员标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
