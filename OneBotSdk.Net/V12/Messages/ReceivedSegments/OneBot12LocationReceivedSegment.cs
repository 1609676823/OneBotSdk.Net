using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents a received geographical location. / 表示收到的地理位置。</summary>
public sealed class OneBot12LocationReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12LocationReceivedSegment(
        string? type,
        JsonObject data,
        JsonObject rawJson,
        double? latitude,
        double? longitude,
        string? title,
        string? content)
        : base(type, data, rawJson)
    {
        Latitude = latitude;
        Longitude = longitude;
        Title = title;
        Content = content;
    }

    /// <summary>Gets the latitude. / 获取纬度。</summary>
    public double? Latitude { get; }
    /// <summary>Gets the longitude. / 获取经度。</summary>
    public double? Longitude { get; }
    /// <summary>Gets the location title. / 获取位置标题。</summary>
    public string? Title { get; }
    /// <summary>Gets the address content. / 获取地址内容。</summary>
    public string? Content { get; }
}
