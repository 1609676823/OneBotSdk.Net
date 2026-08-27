using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents an outgoing short video and only exposes video send parameters. / 表示出站短视频，并且只公开短视频发送参数。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class VideoSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes an outgoing short video. / 初始化出站短视频。</summary>
    public VideoSendSegment(
        string file,
        bool? cache = null,
        bool? proxy = null,
        long? timeoutSeconds = null) : base(MessageSegmentTypes.Video)
    {
        File = Require(file, nameof(file));
        Cache = cache;
        Proxy = proxy;
        TimeoutSeconds = timeoutSeconds;
    }

    /// <summary>Gets the video source. / 获取短视频来源。</summary>
    public string File { get; }

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
        AddBoolean(data, "cache", Cache);
        AddBoolean(data, "proxy", Proxy);
        AddInteger(data, "timeout", TimeoutSeconds);
        return data;
    }
}
