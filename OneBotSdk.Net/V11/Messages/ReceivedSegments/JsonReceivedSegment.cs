using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents received JSON rich content. / 表示收到的 JSON 富文本。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class JsonReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the JSON string payload. / 获取 JSON 字符串内容。</summary>
    public string? Json { get; internal set; }
}
