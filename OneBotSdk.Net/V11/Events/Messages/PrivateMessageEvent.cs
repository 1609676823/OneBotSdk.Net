using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a standard private message event. / 表示标准私聊消息事件。</summary>
public sealed class PrivateMessageEvent : OneBot11MessageEvent
{
    internal PrivateMessageEvent()
    {
    }

    /// <summary>Gets best-effort sender information whose individual fields may be absent. / 获取各字段均可能缺失的尽力提供发送者信息。</summary>
    [JsonPropertyName("sender")]
    public PrivateMessageSender? Sender { get; internal set; }
}
