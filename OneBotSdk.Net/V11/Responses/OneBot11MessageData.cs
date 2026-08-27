using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Contains the message information returned by <c>get_msg</c>.
/// 包含 <c>get_msg</c> 返回的消息信息。
/// </summary>
public sealed class OneBot11MessageData : OneBot11JsonModel
{
    internal static OneBot11MessageData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        // The nested sender and message are parsed independently so either may survive malformed siblings.
        // 嵌套 sender 与 message 独立解析，因此其中一项异常时另一项仍可使用。
        return new OneBot11MessageData
        {
            RawJson = TolerantJson.CloneObject(source),
            Time = TolerantJson.Int64(source, "time"),
            MessageType = TolerantJson.String(source, "message_type"),
            MessageId = TolerantJson.Int64(source, "message_id"),
            RealId = TolerantJson.Int64(source, "real_id"),
            Sender = TolerantJson.Parse(source, "sender", OneBot11MessageSender.Parse),
            MessageChain = TolerantJson.Parse(source, "message", OneBot11ReceivedMessage.Parse) ??
                           OneBot11ReceivedMessage.Empty
        };
    }

    /// <summary>
    /// Gets the Unix timestamp in seconds.
    /// 获取 Unix 秒级时间戳。
    /// </summary>
    [JsonPropertyName("time")]
    public long? Time { get; private set; }

    /// <summary>
    /// Gets the raw message type, normally <c>private</c> or <c>group</c>.
    /// 获取原始消息类型，通常为 <c>private</c> 或 <c>group</c>。
    /// </summary>
    [JsonPropertyName("message_type")]
    public string? MessageType { get; private set; }

    /// <summary>
    /// Gets the message identifier.
    /// 获取消息标识。
    /// </summary>
    [JsonPropertyName("message_id")]
    public long? MessageId { get; private set; }

    /// <summary>
    /// Gets the implementation's real message identifier.
    /// 获取实现端的真实消息标识。
    /// </summary>
    [JsonPropertyName("real_id")]
    public long? RealId { get; private set; }

    /// <summary>
    /// Gets the best-effort sender information.
    /// 获取尽力提供的发送者信息。
    /// </summary>
    [JsonPropertyName("sender")]
    public OneBot11MessageSender? Sender { get; private set; }

    /// <summary>
    /// Gets the strongly typed received message chain.
    /// 获取强类型入站消息链。
    /// </summary>
    [JsonPropertyName("message")]
    public OneBot11ReceivedMessage MessageChain { get; private set; } = OneBot11ReceivedMessage.Empty;
}
