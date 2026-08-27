using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Messages;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Defines common message-event fields. / 定义消息事件公共字段。</summary>
public abstract class OneBot10MessageEvent : OneBot10Event
{
    /// <summary>Gets <c>private</c>, <c>group</c>, or <c>discuss</c>. / 获取 <c>private</c>、<c>group</c> 或 <c>discuss</c>。</summary>
    [JsonPropertyName("message_type")]
    public string? MessageType { get; internal set; }

    /// <summary>Gets the message subtype. / 获取消息子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the message identifier. / 获取消息 ID。</summary>
    [JsonPropertyName("message_id")]
    public long? MessageId { get; internal set; }

    /// <summary>Gets the sender QQ identifier. / 获取发送者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>
    /// Gets the strongly typed received message chain parsed from either a CQ-code string or a segment array.
    /// 获取从 CQ 码字符串或消息段数组解析得到的强类型入站消息链。
    /// </summary>
    [JsonPropertyName("message")]
    public OneBot10ReceivedMessage MessageChain { get; internal set; } = OneBot10ReceivedMessage.Empty;

    /// <summary>Gets the implementation's original message string. / 获取实现端提供的原始消息字符串。</summary>
    [JsonPropertyName("raw_message")]
    public string? RawMessage { get; internal set; }

    /// <summary>Gets the legacy font identifier. / 获取旧版字体标识。</summary>
    [JsonPropertyName("font")]
    public long? Font { get; internal set; }

}
