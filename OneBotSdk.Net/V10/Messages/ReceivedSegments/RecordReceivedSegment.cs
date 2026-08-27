using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents a received voice record with receive-only URL metadata. / 表示带有接收专用 URL 元数据的收到语音。</summary>
[JsonConverter(typeof(OneBot10ReceivedSegmentJsonConverter))]
public sealed class RecordReceivedSegment : OneBot10ReceivedSegment
{
    /// <summary>Gets the reusable received file name. / 获取可复用的已接收文件名。</summary>
    public string? File { get; internal set; }

    /// <summary>Gets whether the record is voice-changed. / 获取语音是否经过变声。</summary>
    public bool? Magic { get; internal set; }

    /// <summary>Gets the receive-only record URL. / 获取仅接收的语音 URL。</summary>
    public string? Url { get; internal set; }
}
