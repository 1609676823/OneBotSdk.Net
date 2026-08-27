using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a periodic heartbeat event. / 表示周期性心跳事件。</summary>
public sealed class HeartbeatMetaEvent : OneBot12MetaEvent
{
    internal HeartbeatMetaEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the interval until the next heartbeat in milliseconds. / 获取距下一次心跳的间隔（毫秒）。</summary>
    [JsonPropertyName("interval")]
    public long? Interval { get; internal set; }
}
