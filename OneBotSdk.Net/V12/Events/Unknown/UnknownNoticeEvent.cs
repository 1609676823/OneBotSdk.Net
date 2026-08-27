using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Preserves a notice event whose detail type is unknown. / 保留详细类型未知的通知事件。</summary>
public sealed class UnknownNoticeEvent : OneBot12NoticeEvent
{
    internal UnknownNoticeEvent(JsonObject rawJson) : base(rawJson) { }
}
