using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Defines optional quick operations returned for a private message event.
/// 定义私聊消息事件响应中可选的快速操作。
/// </summary>
public sealed class PrivateMessageQuickOperation
{
    /// <summary>Gets or sets the optional reply. / 获取或设置可选回复。</summary>
    [JsonPropertyName("reply")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OneBot11SendMessage? Reply { get; set; }

    /// <summary>Gets or sets whether a string reply is sent as plain text. / 获取或设置字符串回复是否作为纯文本发送。</summary>
    [JsonPropertyName("auto_escape")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AutoEscape { get; set; }
}
