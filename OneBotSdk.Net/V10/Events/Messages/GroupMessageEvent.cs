using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Represents a standard group message event. / 表示标准群消息事件。</summary>
public sealed class GroupMessageEvent : OneBot10MessageEvent
{
    internal GroupMessageEvent()
    {
    }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets anonymous information, or null for a non-anonymous message. / 获取匿名信息；非匿名消息时为 null。</summary>
    [JsonPropertyName("anonymous")]
    public AnonymousInfo? Anonymous { get; internal set; }

    /// <summary>Gets best-effort sender information, which is unreliable for anonymous messages. / 获取尽力提供的发送者信息；匿名消息中该信息不可靠。</summary>
    [JsonPropertyName("sender")]
    public GroupMessageSender? Sender { get; internal set; }
}
