using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing location. / 表示出站位置消息。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class LocationSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a location. / 初始化位置消息。</summary>
    public LocationSendSegment(string latitude, string longitude, string? title = null, string? content = null)
        : base(MessageSegmentTypes.Location)
    {
        Latitude = Require(latitude, nameof(latitude));
        Longitude = Require(longitude, nameof(longitude));
        Title = title;
        Content = content;
    }

    /// <summary>Gets the latitude. / 获取纬度。</summary>
    public string Latitude { get; }

    /// <summary>Gets the longitude. / 获取经度。</summary>
    public string Longitude { get; }

    /// <summary>Gets the optional title. / 获取可选标题。</summary>
    public string? Title { get; }

    /// <summary>Gets the optional description. / 获取可选描述。</summary>
    public string? Content { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject { ["lat"] = Latitude, ["lon"] = Longitude };
        Add(data, "title", Title);
        Add(data, "content", Content);
        return data;
    }
}
