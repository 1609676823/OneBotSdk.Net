using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing reply reference. / 表示出站回复引用。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class ReplySendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a reply by protocol message ID. / 通过协议消息 ID 初始化回复。</summary>
    public ReplySendSegment(string messageId) : base(MessageSegmentTypes.Reply)
    {
        MessageId = Require(messageId, nameof(messageId));
    }

    /// <summary>Initializes a reply by numeric message ID. / 通过数字消息 ID 初始化回复。</summary>
    public ReplySendSegment(long messageId) : this(messageId.ToString(CultureInfo.InvariantCulture))
    {
    }

    /// <summary>Gets the referenced message ID. / 获取引用的消息 ID。</summary>
    public string MessageId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["id"] = MessageId };
}
