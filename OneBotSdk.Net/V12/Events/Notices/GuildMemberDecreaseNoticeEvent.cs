using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a guild-member decrease notice. / 表示群组成员减少通知。</summary>
public sealed class GuildMemberDecreaseNoticeEvent : OneBot12NoticeEvent
{
    internal GuildMemberDecreaseNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the guild identifier. / 获取群组标识。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; internal set; }

    /// <summary>Gets the departing member identifier. / 获取离开的成员标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
