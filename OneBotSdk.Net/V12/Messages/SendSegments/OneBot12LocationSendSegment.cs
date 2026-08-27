using System;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing geographical location. / 表示出站地理位置。</summary>
public sealed class OneBot12LocationSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing geographical location. / 初始化出站地理位置。</summary>
    public OneBot12LocationSendSegment(double latitude, double longitude, string title, string content)
        : base(OneBot12MessageSegmentTypes.Location)
    {
        Latitude = latitude;
        Longitude = longitude;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>Gets the latitude. / 获取纬度。</summary>
    public double Latitude { get; }
    /// <summary>Gets the longitude. / 获取经度。</summary>
    public double Longitude { get; }
    /// <summary>Gets the location title. / 获取位置标题。</summary>
    public string Title { get; }
    /// <summary>Gets the address content. / 获取地址内容。</summary>
    public string Content { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        return new JsonObject
        {
            ["latitude"] = Latitude,
            ["longitude"] = Longitude,
            ["title"] = Title,
            ["content"] = Content
        };
    }
}
