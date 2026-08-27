using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents an outgoing dice magic face. / 表示出站骰子魔法表情。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class DiceSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes the segment. / 初始化消息段。</summary>
    public DiceSendSegment() : base(MessageSegmentTypes.Dice)
    {
    }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject();
}
