using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Defines the fields shared by every OneBot 11 event.
/// 定义所有 OneBot 11 事件共享的字段。
/// </summary>
public abstract class OneBot11Event : OneBot11JsonModel
{
    /// <summary>Gets the Unix timestamp at which the event occurred. / 获取事件发生时的 Unix 时间戳。</summary>
    [JsonPropertyName("time")]
    public long? Time { get; internal set; }

    /// <summary>Gets the bot QQ identifier that received the event. / 获取收到事件的机器人 QQ 号。</summary>
    [JsonPropertyName("self_id")]
    public long? SelfId { get; internal set; }

    /// <summary>Gets the top-level event discriminator. / 获取顶层事件判别值。</summary>
    [JsonPropertyName("post_type")]
    public string? PostType { get; internal set; }
}
