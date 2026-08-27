using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents a send-only custom music share. / 表示仅发送的自定义音乐分享。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class CustomMusicSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes a custom music share. / 初始化自定义音乐分享。</summary>
    public CustomMusicSendSegment(
        string url,
        string audio,
        string title,
        string? content = null,
        string? image = null) : base(MessageSegmentTypes.Music)
    {
        Url = Require(url, nameof(url));
        Audio = Require(audio, nameof(audio));
        Title = Require(title, nameof(title));
        Content = content;
        Image = image;
    }

    /// <summary>Gets the click target URL. / 获取点击目标 URL。</summary>
    public string Url { get; }

    /// <summary>Gets the audio URL. / 获取音频 URL。</summary>
    public string Audio { get; }

    /// <summary>Gets the title. / 获取标题。</summary>
    public string Title { get; }

    /// <summary>Gets the optional description. / 获取可选描述。</summary>
    public string? Content { get; }

    /// <summary>Gets the optional cover URL. / 获取可选封面 URL。</summary>
    public string? Image { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject
        {
            ["type"] = "custom",
            ["url"] = Url,
            ["audio"] = Audio,
            ["title"] = Title
        };
        Add(data, "content", Content);
        Add(data, "image", Image);
        return data;
    }
}
