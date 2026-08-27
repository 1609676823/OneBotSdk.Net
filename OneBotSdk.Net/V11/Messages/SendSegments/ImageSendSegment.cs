using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing image and only exposes image send parameters. / 表示出站图片，并且只公开图片发送参数。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class ImageSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes an image from a received file name, file URI, URL, or base64 URI. / 通过已接收文件名、文件 URI、URL 或 base64 URI 初始化图片。</summary>
    public ImageSendSegment(
        string file,
        bool flash = false,
        bool? cache = null,
        bool? proxy = null,
        long? timeoutSeconds = null) : base(MessageSegmentTypes.Image)
    {
        File = Require(file, nameof(file));
        Flash = flash;
        Cache = cache;
        Proxy = proxy;
        TimeoutSeconds = timeoutSeconds;
    }

    /// <summary>Gets the image source. / 获取图片来源。</summary>
    public string File { get; }

    /// <summary>Gets whether the image is sent as flash. / 获取是否作为闪照发送。</summary>
    public bool Flash { get; }

    /// <summary>Gets the optional download-cache switch. / 获取可选下载缓存开关。</summary>
    public bool? Cache { get; }

    /// <summary>Gets the optional proxy switch. / 获取可选代理开关。</summary>
    public bool? Proxy { get; }

    /// <summary>Gets the optional download timeout in seconds. / 获取可选下载超时秒数。</summary>
    public long? TimeoutSeconds { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject { ["file"] = File };
        Add(data, "type", Flash ? "flash" : null);
        AddBoolean(data, "cache", Cache);
        AddBoolean(data, "proxy", Proxy);
        AddInteger(data, "timeout", TimeoutSeconds);
        return data;
    }
}
