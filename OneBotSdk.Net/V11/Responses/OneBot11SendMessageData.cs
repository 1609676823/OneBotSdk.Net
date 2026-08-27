using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Contains the identifier returned after a message is sent.
/// 包含消息发送后返回的消息标识。
/// </summary>
public sealed class OneBot11SendMessageData : OneBot11JsonModel
{
    internal static OneBot11SendMessageData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new OneBot11SendMessageData
        {
            RawJson = TolerantJson.CloneObject(source),
            MessageId = TolerantJson.Int64(source, "message_id")
        };
    }

    /// <summary>
    /// Gets the message identifier.
    /// 获取消息标识。
    /// </summary>
    [JsonPropertyName("message_id")]
    public long? MessageId { get; private set; }
}
