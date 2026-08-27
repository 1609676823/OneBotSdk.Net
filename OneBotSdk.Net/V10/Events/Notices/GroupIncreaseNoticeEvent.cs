using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Represents a member joining a group. / 表示群成员增加。</summary>
public sealed class GroupIncreaseNoticeEvent : OneBot10NoticeEvent
{
    internal GroupIncreaseNoticeEvent()
    {
    }

    /// <summary>Gets <c>approve</c> or <c>invite</c>. / 获取 <c>approve</c> 或 <c>invite</c>。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the operator QQ identifier. / 获取操作者 QQ 号。</summary>
    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; internal set; }

    /// <summary>Gets the joining member QQ identifier. / 获取加入者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }
}
