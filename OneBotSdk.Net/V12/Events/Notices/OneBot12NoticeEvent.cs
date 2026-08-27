using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Defines the base type for OneBot 12 notice events. / 定义 OneBot 12 通知事件基类。</summary>
public abstract class OneBot12NoticeEvent : OneBot12Event
{
    internal OneBot12NoticeEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }
}
