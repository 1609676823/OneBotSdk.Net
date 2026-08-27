using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Preserves an event whose top-level type is unknown. / 保留顶层类型未知的事件。</summary>
public sealed class UnknownOneBot12Event : OneBot12Event
{
    internal UnknownOneBot12Event(JsonObject rawJson) : base(rawJson) { }
}
