using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received rock-paper-scissors magic face. / 表示收到的猜拳魔法表情。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class RpsReceivedSegment : OneBot11ReceivedSegment
{
}
