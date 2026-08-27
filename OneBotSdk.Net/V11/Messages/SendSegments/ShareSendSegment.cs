using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing link share. / 表示出站链接分享。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class ShareSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes a link share. / 初始化链接分享。</summary>
    public ShareSendSegment(string url, string title, string? content = null, string? image = null)
        : base(MessageSegmentTypes.Share)
    {
        Url = Require(url, nameof(url));
        Title = Require(title, nameof(title));
        Content = content;
        Image = image;
    }

    /// <summary>Gets the target URL. / 获取目标 URL。</summary>
    public string Url { get; }

    /// <summary>Gets the title. / 获取标题。</summary>
    public string Title { get; }

    /// <summary>Gets the optional description. / 获取可选描述。</summary>
    public string? Content { get; }

    /// <summary>Gets the optional image URL. / 获取可选图片 URL。</summary>
    public string? Image { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject { ["url"] = Url, ["title"] = Title };
        Add(data, "content", Content);
        Add(data, "image", Image);
        return data;
    }
}
