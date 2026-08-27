using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received location. / 表示收到的位置消息。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class LocationReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the latitude. / 获取纬度。</summary>
    public string? Latitude { get; internal set; }

    /// <summary>Gets the longitude. / 获取经度。</summary>
    public string? Longitude { get; internal set; }

    /// <summary>Gets the title. / 获取标题。</summary>
    public string? Title { get; internal set; }

    /// <summary>Gets the description. / 获取描述。</summary>
    public string? Content { get; internal set; }
}
