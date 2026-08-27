using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Retains a notice event with an unknown discriminator combination. / 保留判别值组合未知的通知事件。</summary>
public sealed class UnknownNoticeEvent : OneBot11NoticeEvent
{
    internal UnknownNoticeEvent()
    {
    }

    /// <summary>Gets an implementation-specific subtype when present. / 获取存在的实现端特有子类型。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }
}
