using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Defines sender fields common to private and group message events.
/// 定义私聊和群消息事件共用的发送者字段。
/// </summary>
public abstract class OneBot10MessageSender : OneBot10JsonModel
{
    /// <summary>Gets the sender QQ identifier when supplied. / 获取实现端提供的发送者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the best-effort nickname. / 获取尽力提供的昵称。</summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; internal set; }

    /// <summary>Gets <c>male</c>, <c>female</c>, <c>unknown</c>, or an extension value. / 获取性别标准值或扩展值。</summary>
    [JsonPropertyName("sex")]
    public string? Sex { get; internal set; }

    /// <summary>Gets the best-effort age. / 获取尽力提供的年龄。</summary>
    [JsonPropertyName("age")]
    public long? Age { get; internal set; }
}
