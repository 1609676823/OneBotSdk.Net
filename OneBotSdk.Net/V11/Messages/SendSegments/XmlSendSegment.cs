using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents outgoing XML rich content. / 表示出站 XML 富文本。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class XmlSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes XML rich content. / 初始化 XML 富文本。</summary>
    public XmlSendSegment(string xml) : base(MessageSegmentTypes.Xml)
    {
        Xml = xml ?? throw new ArgumentNullException(nameof(xml));
    }

    /// <summary>Gets the XML payload. / 获取 XML 内容。</summary>
    public string Xml { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["data"] = Xml };
}
