using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing at-mention. / 表示出站 @ 消息。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class AtSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes an at-mention by protocol target, including <c>all</c>. / 通过协议目标初始化 @ 消息，包括 <c>all</c>。</summary>
    public AtSendSegment(string target) : base(MessageSegmentTypes.At)
    {
        Target = Require(target, nameof(target));
    }

    /// <summary>Initializes an at-mention by QQ ID. / 通过 QQ 号初始化 @ 消息。</summary>
    public AtSendSegment(long userId) : this(userId.ToString(CultureInfo.InvariantCulture))
    {
    }

    /// <summary>Gets the QQ ID or <c>all</c>. / 获取 QQ 号或 <c>all</c>。</summary>
    public string Target { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["qq"] = Target };
}
