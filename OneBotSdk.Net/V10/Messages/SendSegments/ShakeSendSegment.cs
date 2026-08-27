using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents an outgoing window-shake segment. / 表示出站窗口抖动消息段。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class ShakeSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes the segment. / 初始化消息段。</summary>
    public ShakeSendSegment() : base(MessageSegmentTypes.Shake)
    {
    }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject();
}
