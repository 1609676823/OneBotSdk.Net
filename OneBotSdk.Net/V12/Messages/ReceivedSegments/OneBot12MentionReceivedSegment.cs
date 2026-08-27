using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents a received user mention. / 表示收到的用户提及。</summary>
public sealed class OneBot12MentionReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12MentionReceivedSegment(string? type, JsonObject data, JsonObject rawJson, string? userId)
        : base(type, data, rawJson) => UserId = userId;

    /// <summary>Gets the mentioned user ID. / 获取被提及用户 ID。</summary>
    public string? UserId { get; }
}
