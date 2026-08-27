using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received image with receive-only URL metadata. / 表示带有接收专用 URL 元数据的收到图片。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ImageReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the reusable received file name. / 获取可复用的已接收文件名。</summary>
    public string? File { get; internal set; }

    /// <summary>Gets the received image type such as <c>flash</c>. / 获取收到的图片类型，例如 <c>flash</c>。</summary>
    public string? ImageType { get; internal set; }

    /// <summary>Gets the receive-only image URL. / 获取仅接收的图片 URL。</summary>
    public string? Url { get; internal set; }
}
