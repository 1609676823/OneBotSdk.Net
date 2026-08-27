using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a member leaving or being removed from a group. / 表示群成员退出或被移出。</summary>
public sealed class GroupDecreaseNoticeEvent : OneBot11NoticeEvent
{
    internal GroupDecreaseNoticeEvent()
    {
    }

    /// <summary>Gets <c>leave</c>, <c>kick</c>, or <c>kick_me</c>. / 获取成员减少子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the operator QQ identifier. / 获取操作者 QQ 号。</summary>
    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; internal set; }

    /// <summary>Gets the departing member QQ identifier. / 获取离开者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }
}
