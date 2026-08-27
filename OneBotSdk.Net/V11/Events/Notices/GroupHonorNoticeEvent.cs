using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a group member honor change. / 表示群成员荣誉变更通知。</summary>
public sealed class GroupHonorNoticeEvent : OneBot11NoticeEvent
{
    internal GroupHonorNoticeEvent()
    {
    }

    /// <summary>Gets the fixed <c>honor</c> subtype. / 获取固定的 <c>honor</c> 子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets <c>talkative</c>, <c>performer</c>, <c>emotion</c>, or an extension value. / 获取荣誉类型标准值或扩展值。</summary>
    [JsonPropertyName("honor_type")]
    public string? HonorType { get; internal set; }

    /// <summary>Gets the honored member QQ identifier. / 获取获荣誉成员 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }
}
