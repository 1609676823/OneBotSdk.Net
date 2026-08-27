using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing dice magic face. / 表示出站骰子魔法表情。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class DiceSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes the segment. / 初始化消息段。</summary>
    public DiceSendSegment() : base(MessageSegmentTypes.Dice)
    {
    }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject();
}
