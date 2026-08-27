using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Defines common notice-event fields. / 定义通知事件公共字段。</summary>
public abstract class OneBot11NoticeEvent : OneBot11Event
{
    /// <summary>Gets the notice discriminator. / 获取通知判别值。</summary>
    [JsonPropertyName("notice_type")]
    public string? NoticeType { get; internal set; }
}
