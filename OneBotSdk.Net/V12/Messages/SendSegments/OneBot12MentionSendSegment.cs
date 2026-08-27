using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing user mention. / 表示出站用户提及。</summary>
public sealed class OneBot12MentionSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing user mention. / 初始化出站用户提及。</summary>
    public OneBot12MentionSendSegment(string userId) : base(OneBot12MessageSegmentTypes.Mention)
    {
        UserId = Require(userId, nameof(userId));
    }

    /// <summary>Gets the mentioned user ID. / 获取被提及用户 ID。</summary>
    public string UserId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["user_id"] = UserId };
}
