using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents a received reply reference. / 表示收到的回复引用。</summary>
public sealed class OneBot12ReplyReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12ReplyReceivedSegment(
        string? type,
        JsonObject data,
        JsonObject rawJson,
        string? messageId,
        string? userId)
        : base(type, data, rawJson)
    {
        MessageId = messageId;
        UserId = userId;
    }

    /// <summary>Gets the replied-to message ID. / 获取被回复消息 ID。</summary>
    public string? MessageId { get; }
    /// <summary>Gets the original sender ID when provided. / 获取原发送者 ID（如提供）。</summary>
    public string? UserId { get; }
}
