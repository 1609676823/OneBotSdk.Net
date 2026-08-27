using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Represents the discussion-group message event defined by OneBot 10.
/// 表示 OneBot 10 定义的讨论组消息事件。
/// </summary>
public sealed class DiscussMessageEvent : OneBot10MessageEvent
{
    internal DiscussMessageEvent()
    {
    }

    /// <summary>Gets the discussion-group identifier when it can be parsed. / 获取尽力解析的讨论组标识。</summary>
    [JsonPropertyName("discuss_id")]
    public long? DiscussId { get; internal set; }

    /// <summary>Gets the best-effort sender information. / 获取尽力解析的发送者信息。</summary>
    [JsonPropertyName("sender")]
    public DiscussMessageSender? Sender { get; internal set; }
}
