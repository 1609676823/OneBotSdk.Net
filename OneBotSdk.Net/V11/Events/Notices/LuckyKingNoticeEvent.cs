using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents the lucky king of a group red packet. / 表示群红包运气王通知。</summary>
public sealed class LuckyKingNoticeEvent : OneBot11NoticeEvent
{
    internal LuckyKingNoticeEvent()
    {
    }

    /// <summary>Gets the fixed <c>lucky_king</c> subtype. / 获取固定的 <c>lucky_king</c> 子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the red-packet sender QQ identifier. / 获取红包发送者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the lucky king QQ identifier. / 获取运气王 QQ 号。</summary>
    [JsonPropertyName("target_id")]
    public long? TargetId { get; internal set; }
}
