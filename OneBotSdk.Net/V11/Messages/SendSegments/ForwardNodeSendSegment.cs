using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a send-only merged-forward node referencing an existing message. / 表示仅发送且引用已有消息的合并转发节点。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class ForwardNodeSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a node reference. / 初始化节点引用。</summary>
    public ForwardNodeSendSegment(string messageId) : base(MessageSegmentTypes.Node)
    {
        MessageId = Require(messageId, nameof(messageId));
    }

    /// <summary>Initializes a node reference by numeric message ID. / 通过数字消息 ID 初始化节点引用。</summary>
    public ForwardNodeSendSegment(long messageId) : this(messageId.ToString(CultureInfo.InvariantCulture))
    {
    }

    /// <summary>Gets the referenced message ID. / 获取引用的消息 ID。</summary>
    public string MessageId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["id"] = MessageId };
}
