using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Preserves a message event whose detail type is unknown. / 保留详细类型未知的消息事件。</summary>
public sealed class UnknownMessageEvent : OneBot12MessageEvent
{
    internal UnknownMessageEvent(JsonObject rawJson) : base(rawJson) { }
}
