using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing mention-all segment. / 表示出站提及全体成员消息段。</summary>
public sealed class OneBot12MentionAllSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing mention-all segment. / 初始化出站提及全体消息段。</summary>
    public OneBot12MentionAllSendSegment() : base(OneBot12MessageSegmentTypes.MentionAll)
    {
    }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject();
}
