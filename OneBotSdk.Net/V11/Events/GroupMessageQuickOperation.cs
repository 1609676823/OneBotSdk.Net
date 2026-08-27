using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Defines optional quick operations returned for a group message event.
/// 定义群消息事件响应中可选的快速操作。
/// </summary>
public sealed class GroupMessageQuickOperation
{
    /// <summary>Gets or sets the optional reply. / 获取或设置可选回复。</summary>
    [JsonPropertyName("reply")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OneBot11SendMessage? Reply { get; set; }

    /// <summary>Gets or sets whether a string reply is sent as plain text. / 获取或设置字符串回复是否作为纯文本发送。</summary>
    [JsonPropertyName("auto_escape")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AutoEscape { get; set; }

    /// <summary>Gets or sets whether the sender is mentioned before the reply. / 获取或设置回复前是否自动 @ 发送者。</summary>
    [JsonPropertyName("at_sender")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AtSender { get; set; }

    /// <summary>Gets or sets whether the source message is recalled. / 获取或设置是否撤回原消息。</summary>
    [JsonPropertyName("delete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Delete { get; set; }

    /// <summary>Gets or sets whether the sender is removed from the group. / 获取或设置是否将发送者移出群。</summary>
    [JsonPropertyName("kick")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Kick { get; set; }

    /// <summary>Gets or sets whether the sender is muted. / 获取或设置是否禁言发送者。</summary>
    [JsonPropertyName("ban")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Ban { get; set; }

    /// <summary>Gets or sets the mute duration in seconds. / 获取或设置禁言时长（秒）。</summary>
    [JsonPropertyName("ban_duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? BanDuration { get; set; }
}
