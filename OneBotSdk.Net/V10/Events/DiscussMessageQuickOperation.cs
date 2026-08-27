using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Messages;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Defines optional quick operations returned for a discussion-group message event.
/// 定义讨论组消息事件响应中可选的快速操作。
/// </summary>
public sealed class DiscussMessageQuickOperation
{
    /// <summary>Gets or sets the optional reply message. / 获取或设置可选的回复消息。</summary>
    [JsonPropertyName("reply")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OneBot10SendMessage? Reply { get; set; }

    /// <summary>Gets or sets whether the reply string should bypass CQ-code parsing. / 获取或设置回复字符串是否跳过 CQ 码解析。</summary>
    [JsonPropertyName("auto_escape")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AutoEscape { get; set; }

    /// <summary>Gets or sets whether the reply should mention the sender. / 获取或设置回复时是否提及发送者。</summary>
    [JsonPropertyName("at_sender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AtSender { get; set; }
}
