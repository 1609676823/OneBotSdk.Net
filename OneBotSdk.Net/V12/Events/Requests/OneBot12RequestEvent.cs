using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Defines the reserved request-event category for standard extensions. / 定义供标准扩展使用的请求事件保留类别。</summary>
public abstract class OneBot12RequestEvent : OneBot12Event
{
    internal OneBot12RequestEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }
}
