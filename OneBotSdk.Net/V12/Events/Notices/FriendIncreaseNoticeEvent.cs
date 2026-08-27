using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a friend-increase notice. / 表示好友增加通知。</summary>
public sealed class FriendIncreaseNoticeEvent : OneBot12NoticeEvent
{
    internal FriendIncreaseNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the added friend's user identifier. / 获取已添加好友的用户标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }
}
