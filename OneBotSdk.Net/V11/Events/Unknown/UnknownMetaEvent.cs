using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Retains a meta event with an unknown type. / 保留类型未知的元事件。</summary>
public sealed class UnknownMetaEvent : OneBot11MetaEvent
{
    internal UnknownMetaEvent()
    {
    }

    /// <summary>Gets an implementation-specific subtype when present. / 获取存在的实现端特有子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }
}
