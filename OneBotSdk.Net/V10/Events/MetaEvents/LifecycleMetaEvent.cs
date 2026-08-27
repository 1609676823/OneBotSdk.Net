using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Represents a OneBot lifecycle event. / 表示 OneBot 生命周期事件。</summary>
public sealed class LifecycleMetaEvent : OneBot10MetaEvent
{
    internal LifecycleMetaEvent()
    {
    }

    /// <summary>Gets <c>enable</c>, <c>disable</c>, <c>connect</c>, or an extension value. / 获取生命周期标准子类型或扩展值。</summary>
    [JsonPropertyName("sub_type")]
    public string? SubType { get; internal set; }
}
