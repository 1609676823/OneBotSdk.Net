using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents received XML rich content. / 表示收到的 XML 富文本。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class XmlReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the XML payload. / 获取 XML 内容。</summary>
    public string? Xml { get; internal set; }
}
