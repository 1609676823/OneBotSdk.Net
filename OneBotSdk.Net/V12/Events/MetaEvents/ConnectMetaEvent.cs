using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents the first event on a successful WebSocket connection. / 表示 WebSocket 成功连接后的首个事件。</summary>
public sealed class ConnectMetaEvent : OneBot12MetaEvent
{
    internal ConnectMetaEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets implementation and protocol version information. / 获取实现端及协议版本信息。</summary>
    [JsonPropertyName("version")]
    public OneBot12VersionData? Version { get; internal set; }
}
