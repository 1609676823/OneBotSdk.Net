using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Messages;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Defines fields shared by standard and extended message events. / 定义标准及扩展消息事件共享的字段。</summary>
public abstract class OneBot12MessageEvent : OneBot12Event
{
    internal OneBot12MessageEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }

    /// <summary>Gets the platform message identifier. / 获取平台消息标识。</summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; internal set; }

    /// <summary>Gets the parsed incoming message chain. / 获取已解析的入站消息链。</summary>
    [JsonPropertyName("message")]
    public OneBot12ReceivedMessage? Message { get; internal set; }

    /// <summary>Gets the textual alternative representation. / 获取消息的文本替代表示。</summary>
    [JsonPropertyName("alt_message")]
    public string? AltMessage { get; internal set; }

    /// <summary>Gets the sender user identifier. / 获取发送者用户标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; internal set; }
}
