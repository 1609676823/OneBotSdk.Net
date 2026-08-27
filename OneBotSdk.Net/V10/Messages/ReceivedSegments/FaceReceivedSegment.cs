using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents a received QQ face. / 表示收到的 QQ 表情。</summary>
[JsonConverter(typeof(OneBot10ReceivedSegmentJsonConverter))]
public sealed class FaceReceivedSegment : OneBot10ReceivedSegment
{
    /// <summary>Gets the face ID. / 获取表情 ID。</summary>
    public string? Id { get; internal set; }
}
