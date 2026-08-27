using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Defines the base type for OneBot 12 meta events. / 定义 OneBot 12 元事件基类。</summary>
public abstract class OneBot12MetaEvent : OneBot12Event
{
    internal OneBot12MetaEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }
}
