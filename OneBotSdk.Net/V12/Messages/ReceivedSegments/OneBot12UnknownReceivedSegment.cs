using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Preserves an implementation-defined or unknown received segment. / 保留实现扩展或未知的接收消息段。</summary>
public sealed class OneBot12UnknownReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12UnknownReceivedSegment(string? type, JsonObject data, JsonObject rawJson)
        : base(type, data, rawJson)
    {
    }
}
