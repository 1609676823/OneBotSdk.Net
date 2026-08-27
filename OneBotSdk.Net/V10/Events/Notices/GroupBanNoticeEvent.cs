using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Represents a group mute or unmute notice. / 表示群禁言或解除禁言通知。</summary>
public sealed class GroupBanNoticeEvent : OneBot10NoticeEvent
{
    internal GroupBanNoticeEvent()
    {
    }

    /// <summary>Gets <c>ban</c> or <c>lift_ban</c>. / 获取 <c>ban</c> 或 <c>lift_ban</c>。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the operator QQ identifier. / 获取操作者 QQ 号。</summary>
    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; internal set; }

    /// <summary>Gets the muted member QQ identifier. / 获取被禁言成员 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the mute duration in seconds. / 获取禁言时长（秒）。</summary>
    [JsonPropertyName("duration")]
    public long? Duration { get; internal set; }
}
