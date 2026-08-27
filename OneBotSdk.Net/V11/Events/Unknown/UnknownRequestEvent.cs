using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Retains a request event with an unknown request type. / 保留请求类型未知的请求事件。</summary>
public sealed class UnknownRequestEvent : OneBot11RequestEvent
{
    internal UnknownRequestEvent()
    {
    }

    /// <summary>Gets an implementation-specific subtype when present. / 获取存在的实现端特有子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }
}
