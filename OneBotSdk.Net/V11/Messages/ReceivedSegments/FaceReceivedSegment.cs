using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received QQ face. / 表示收到的 QQ 表情。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class FaceReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the face ID. / 获取表情 ID。</summary>
    public string? Id { get; internal set; }
}
