using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Contains the strongly typed result of a send-message action.
/// 包含发送消息动作的强类型结果。
/// </summary>
public sealed class OneBot11SendMessageResult : OneBot11JsonModel
{
    internal static OneBot11SendMessageResult? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11SendMessageResult
            {
                RawJson = TolerantJson.CloneObject(source),
                MessageId = TolerantJson.Int64(source, "message_id")
            };
    }

    /// <summary>Gets the message identifier assigned by the implementation. / 获取实现端分配的消息标识。</summary>
    [JsonPropertyName("message_id")]
    public long? MessageId { get; private set; }
}
