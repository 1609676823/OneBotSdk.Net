using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received at-mention. / 表示收到的 @ 消息。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class AtReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the QQ ID or <c>all</c>. / 获取 QQ 号或 <c>all</c>。</summary>
    public string? Target { get; internal set; }
}
