using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a group-member decrease notice. / 表示群成员减少通知。</summary>
public sealed class GroupMemberDecreaseNoticeEvent : OneBot12NoticeEvent
{
    internal GroupMemberDecreaseNoticeEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the group identifier. / 获取群标识。</summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; internal set; }

    /// <summary>Gets the departing member identifier. / 获取离开的成员标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }

    /// <summary>Gets the operator identifier. / 获取操作者标识。</summary>
    [JsonPropertyName("operator_id")]
    public string? OperatorId { get; internal set; }
}
