using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received dice magic face. / 表示收到的骰子魔法表情。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class DiceReceivedSegment : OneBot11ReceivedSegment
{
}
