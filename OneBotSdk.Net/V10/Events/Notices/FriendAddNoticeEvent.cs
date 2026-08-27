using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Represents a friend being added. / 表示好友添加通知。</summary>
public sealed class FriendAddNoticeEvent : OneBot10NoticeEvent
{
    internal FriendAddNoticeEvent()
    {
    }

    /// <summary>Gets the newly added friend QQ identifier. / 获取新添加好友 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }
}
