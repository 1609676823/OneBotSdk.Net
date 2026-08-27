using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Events;

/// <summary>
/// Preserves a request event; OneBot 12 currently reserves the category without defining standard detail types.
/// 保留请求事件；OneBot 12 当前保留此类别，但尚未定义标准详细类型。
/// </summary>
public sealed class UnknownRequestEvent : OneBot12RequestEvent
{
    internal UnknownRequestEvent(JsonObject rawJson) : base(rawJson) { }
}
