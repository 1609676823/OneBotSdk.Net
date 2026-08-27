using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents a received rock-paper-scissors magic face. / 表示收到的猜拳魔法表情。</summary>
[JsonConverter(typeof(OneBot10ReceivedSegmentJsonConverter))]
public sealed class RpsReceivedSegment : OneBot10ReceivedSegment
{
}
