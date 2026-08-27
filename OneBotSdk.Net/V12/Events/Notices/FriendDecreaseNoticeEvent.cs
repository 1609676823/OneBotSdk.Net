using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a friend-decrease notice. / 表示好友减少通知。</summary>
public sealed class FriendDecreaseNoticeEvent : OneBot12NoticeEvent
{
    internal FriendDecreaseNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the removed friend's user identifier. / 获取已移除好友的用户标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }
}
