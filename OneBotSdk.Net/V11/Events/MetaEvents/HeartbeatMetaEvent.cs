using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents a OneBot heartbeat event. / 表示 OneBot 心跳事件。</summary>
public sealed class HeartbeatMetaEvent : OneBot11MetaEvent
{
    internal HeartbeatMetaEvent()
    {
    }

    /// <summary>Gets the independently parsed runtime status object. / 获取独立解析的运行状态对象。</summary>
    [JsonPropertyName("status")]
    public OneBot11Status? Status { get; internal set; }

    /// <summary>Gets the interval until the next heartbeat in milliseconds. / 获取距下一次心跳的间隔（毫秒）。</summary>
    [JsonPropertyName("interval")]
    public long? Interval { get; internal set; }
}
