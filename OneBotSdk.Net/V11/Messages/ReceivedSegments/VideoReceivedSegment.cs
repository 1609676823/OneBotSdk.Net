using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received short video with receive-only URL metadata. / 表示带有接收专用 URL 元数据的收到短视频。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class VideoReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the reusable received file name. / 获取可复用的已接收文件名。</summary>
    public string? File { get; internal set; }

    /// <summary>Gets the receive-only video URL. / 获取仅接收的短视频 URL。</summary>
    public string? Url { get; internal set; }
}
