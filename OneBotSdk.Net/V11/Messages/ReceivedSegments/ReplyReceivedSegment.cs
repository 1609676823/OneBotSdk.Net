using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received reply reference. / 表示收到的回复引用。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ReplyReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the referenced message ID. / 获取引用的消息 ID。</summary>
    public string? MessageId { get; internal set; }
}
