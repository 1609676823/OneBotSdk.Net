using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing rock-paper-scissors magic face. / 表示出站猜拳魔法表情。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class RpsSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes the segment. / 初始化消息段。</summary>
    public RpsSendSegment() : base(MessageSegmentTypes.Rps)
    {
    }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject();
}
