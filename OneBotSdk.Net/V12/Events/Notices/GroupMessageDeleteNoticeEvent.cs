using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a group-message deletion notice. / 表示群消息删除通知。</summary>
public sealed class GroupMessageDeleteNoticeEvent : OneBot12NoticeEvent
{
    internal GroupMessageDeleteNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the group identifier. / 获取群标识。</summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; internal set; }

    /// <summary>Gets the deleted message identifier. / 获取已删除的消息标识。</summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; internal set; }

    /// <summary>Gets the message author identifier. / 获取消息作者标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
