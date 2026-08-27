using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a group administrator change. / 表示群管理员变动。</summary>
public sealed class GroupAdminNoticeEvent : OneBot11NoticeEvent
{
    internal GroupAdminNoticeEvent()
    {
    }

    /// <summary>Gets <c>set</c> or <c>unset</c>. / 获取 <c>set</c> 或 <c>unset</c>。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the administrator QQ identifier. / 获取管理员 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }
}
