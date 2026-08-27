using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Events;

/// <summary>
/// Defines the fields shared by every OneBot 12 event.
/// 定义所有 OneBot 12 事件共享的字段。
/// </summary>
public abstract class OneBot12Event : OneBot12JsonModel
{
    internal OneBot12Event(JsonObject rawJson)
        : base(rawJson)
    {
    }

    /// <summary>Gets the globally unique event identifier. / 获取全局唯一的事件标识。</summary>
    [JsonPropertyName("id")]
    public string? Id { get; internal set; }

    /// <summary>Gets the Unix timestamp in seconds, including its fractional part. / 获取以秒为单位且可包含小数部分的 Unix 时间戳。</summary>
    [JsonPropertyName("time")]
    public double? Time { get; internal set; }

    /// <summary>Gets the top-level event type. / 获取顶层事件类型。</summary>
    [JsonPropertyName("type")]
    public string? Type { get; internal set; }

    /// <summary>Gets the concrete event discriminator. / 获取具体事件判别值。</summary>
    [JsonPropertyName("detail_type")]
    public string? DetailType { get; internal set; }

    /// <summary>Gets the optional event subtype. / 获取可选事件子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }

    /// <summary>Gets the bot identity for non-meta events. / 获取非元事件对应的机器人身份。</summary>
    [JsonPropertyName("self")]
    public OneBot12Self? Self { get; internal set; }
}
