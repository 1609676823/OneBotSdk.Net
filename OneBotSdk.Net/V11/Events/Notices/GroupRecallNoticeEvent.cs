using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a recalled group message. / 表示群消息撤回通知。</summary>
public sealed class GroupRecallNoticeEvent : OneBot11NoticeEvent
{
    internal GroupRecallNoticeEvent()
    {
    }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the original sender QQ identifier. / 获取原消息发送者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the recalling operator QQ identifier. / 获取撤回操作者 QQ 号。</summary>
    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; internal set; }

    /// <summary>Gets the recalled message identifier. / 获取被撤回消息 ID。</summary>
    [JsonPropertyName("message_id")]
    public long? MessageId { get; internal set; }
}
