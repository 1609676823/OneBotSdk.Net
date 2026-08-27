using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Defines common meta-event fields. / 定义元事件公共字段。</summary>
public abstract class OneBot10MetaEvent : OneBot10Event
{
    /// <summary>Gets the meta-event discriminator. / 获取元事件判别值。</summary>
    [JsonPropertyName("meta_event_type")]
    public string? MetaEventType { get; internal set; }
}
