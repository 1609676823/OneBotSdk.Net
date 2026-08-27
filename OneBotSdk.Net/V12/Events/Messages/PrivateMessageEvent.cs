using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a standard private-message event. / 表示标准私聊消息事件。</summary>
public sealed class PrivateMessageEvent : OneBot12MessageEvent
{
    internal PrivateMessageEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }
}
