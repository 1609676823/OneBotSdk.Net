using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a recalled private message. / 表示好友消息撤回通知。</summary>
public sealed class FriendRecallNoticeEvent : OneBot11NoticeEvent
{
    internal FriendRecallNoticeEvent()
    {
    }

    /// <summary>Gets the friend QQ identifier. / 获取好友 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the recalled message identifier. / 获取被撤回消息 ID。</summary>
    [JsonPropertyName("message_id")]
    public long? MessageId { get; internal set; }
}
