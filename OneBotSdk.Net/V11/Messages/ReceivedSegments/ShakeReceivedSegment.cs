using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received window-shake segment. / 表示收到的窗口抖动消息段。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ShakeReceivedSegment : OneBot11ReceivedSegment
{
}
