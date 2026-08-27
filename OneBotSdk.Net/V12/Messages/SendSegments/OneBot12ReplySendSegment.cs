using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing reply reference. / 表示出站回复引用。</summary>
public sealed class OneBot12ReplySendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing reply reference. / 初始化出站回复引用。</summary>
    public OneBot12ReplySendSegment(string messageId, string? userId = null)
        : base(OneBot12MessageSegmentTypes.Reply)
    {
        MessageId = Require(messageId, nameof(messageId));
        UserId = userId;
    }

    /// <summary>Gets the replied-to message ID. / 获取被回复消息 ID。</summary>
    public string MessageId { get; }
    /// <summary>Gets the optional original sender ID. / 获取可选的原发送者 ID。</summary>
    public string? UserId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject { ["message_id"] = MessageId };
        if (UserId != null)
        {
            data["user_id"] = UserId;
        }

        return data;
    }
}
