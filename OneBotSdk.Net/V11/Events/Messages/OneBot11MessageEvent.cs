using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Defines common message-event fields. / 定义消息事件公共字段。</summary>
public abstract class OneBot11MessageEvent : OneBot11Event
{
    /// <summary>Gets <c>private</c> or <c>group</c>. / 获取 <c>private</c> 或 <c>group</c>。</summary>
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
    public OneBot11ReceivedMessage MessageChain { get; internal set; } = OneBot11ReceivedMessage.Empty;

    /// <summary>Gets the implementation's original message string. / 获取实现端提供的原始消息字符串。</summary>
    [JsonPropertyName("raw_message")]
    public string? RawMessage { get; internal set; }

    /// <summary>Gets the legacy font identifier. / 获取旧版字体标识。</summary>
    [JsonPropertyName("font")]
    public long? Font { get; internal set; }

}
