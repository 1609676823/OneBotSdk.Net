using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Defines common request-event fields. / 定义请求事件公共字段。</summary>
public abstract class OneBot11RequestEvent : OneBot11Event
{
    /// <summary>Gets the request discriminator. / 获取请求判别值。</summary>
    [JsonPropertyName("request_type")]
    public string? RequestType { get; internal set; }

    /// <summary>Gets the requester QQ identifier. / 获取请求者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the verification message. / 获取验证信息。</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; internal set; }

    /// <summary>Gets the opaque flag required when handling the request. / 获取处理请求时所需的不透明 flag。</summary>
    [JsonPropertyName("flag")]
    public string? Flag { get; internal set; }
}
