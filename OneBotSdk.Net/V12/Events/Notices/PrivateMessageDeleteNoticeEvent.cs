using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a private-message deletion notice. / 表示私聊消息删除通知。</summary>
public sealed class PrivateMessageDeleteNoticeEvent : OneBot12NoticeEvent
{
    internal PrivateMessageDeleteNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the deleted message identifier. / 获取已删除的消息标识。</summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; internal set; }

    /// <summary>Gets the peer user identifier. / 获取对端用户标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }
}
