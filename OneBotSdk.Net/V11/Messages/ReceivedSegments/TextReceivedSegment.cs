using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents received plain text. / 表示收到的纯文本。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class TextReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the text content. / 获取文本内容。</summary>
    public string? Text { get; internal set; }
}
