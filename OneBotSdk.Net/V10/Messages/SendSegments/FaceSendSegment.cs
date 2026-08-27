using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents an outgoing QQ face. / 表示出站 QQ 表情。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class FaceSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes a face by protocol ID. / 通过协议 ID 初始化表情。</summary>
    public FaceSendSegment(string id) : base(MessageSegmentTypes.Face)
    {
        Id = Require(id, nameof(id));
    }

    /// <summary>Initializes a face by numeric ID. / 通过数字 ID 初始化表情。</summary>
    public FaceSendSegment(long id) : this(id.ToString(CultureInfo.InvariantCulture))
    {
    }

    /// <summary>Gets the face ID. / 获取表情 ID。</summary>
    public string Id { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["id"] = Id };
}
