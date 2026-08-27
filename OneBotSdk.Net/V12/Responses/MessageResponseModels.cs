using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Contains the identity and timestamp of a successfully sent message. / 包含成功发送消息的标识和时间。</summary>
public sealed class OneBot12SendMessageData : OneBot12JsonModel
{
    private OneBot12SendMessageData(JsonObject raw, string? messageId, double? time) : base(raw)
    {
        MessageId = messageId;
        Time = time;
    }

    /// <summary>Gets the new message ID. / 获取新消息 ID。</summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; }

    /// <summary>Gets the successful send time as Unix seconds. / 获取成功发送时间（Unix 秒）。</summary>
    [JsonPropertyName("time")]
    public double? Time { get; }

    internal static OneBot12SendMessageData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12SendMessageData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "message_id"),
            TolerantJson.Double(source, "time"));
    }
}
