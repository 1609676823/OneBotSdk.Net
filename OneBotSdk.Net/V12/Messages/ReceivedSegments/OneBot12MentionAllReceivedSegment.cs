using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents a received mention-all segment. / 表示收到的提及全体成员消息段。</summary>
public sealed class OneBot12MentionAllReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12MentionAllReceivedSegment(string? type, JsonObject data, JsonObject rawJson)
        : base(type, data, rawJson)
    {
    }
}
