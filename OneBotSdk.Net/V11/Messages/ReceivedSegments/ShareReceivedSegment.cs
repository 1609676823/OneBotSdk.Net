using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received link share. / 表示收到的链接分享。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ShareReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the target URL. / 获取目标 URL。</summary>
    public string? Url { get; internal set; }

    /// <summary>Gets the title. / 获取标题。</summary>
    public string? Title { get; internal set; }

    /// <summary>Gets the description. / 获取描述。</summary>
    public string? Content { get; internal set; }

    /// <summary>Gets the image URL. / 获取图片 URL。</summary>
    public string? Image { get; internal set; }
}
