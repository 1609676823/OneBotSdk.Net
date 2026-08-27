using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a member poking another member in a group. / 表示群内成员戳一戳通知。</summary>
public sealed class GroupPokeNoticeEvent : OneBot11NoticeEvent
{
    internal GroupPokeNoticeEvent()
    {
    }

    /// <summary>Gets the fixed <c>poke</c> subtype. / 获取固定的 <c>poke</c> 子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the initiating member QQ identifier. / 获取发送者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the poked member QQ identifier. / 获取被戳者 QQ 号。</summary>
    [JsonPropertyName("target_id")]
    public long? TargetId { get; internal set; }
}
