using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents received plain text. / 表示收到的纯文本。</summary>
[JsonConverter(typeof(OneBot10ReceivedSegmentJsonConverter))]
public sealed class TextReceivedSegment : OneBot10ReceivedSegment
{
    /// <summary>Gets the text content. / 获取文本内容。</summary>
    public string? Text { get; internal set; }
}
