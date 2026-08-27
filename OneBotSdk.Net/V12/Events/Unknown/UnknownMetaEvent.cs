using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Preserves a meta event whose detail type is unknown. / 保留详细类型未知的元事件。</summary>
public sealed class UnknownMetaEvent : OneBot12MetaEvent
{
    internal UnknownMetaEvent(JsonObject rawJson) : base(rawJson) { }
}
