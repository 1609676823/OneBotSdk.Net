using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents an outgoing voice record and only exposes record send parameters. / 表示出站语音，并且只公开语音发送参数。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class RecordSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes an outgoing voice record. / 初始化出站语音。</summary>
    public RecordSendSegment(
        string file,
        bool? magic = null,
        bool? cache = null,
        long? timeoutSeconds = null) : base(MessageSegmentTypes.Record)
    {
        File = Require(file, nameof(file));
        Magic = magic;
        Cache = cache;
        TimeoutSeconds = timeoutSeconds;
    }

    /// <summary>Gets the record source. / 获取语音来源。</summary>
    public string File { get; }

    /// <summary>Gets the optional voice-changing switch. / 获取可选变声开关。</summary>
    public bool? Magic { get; }

    /// <summary>Gets the optional download-cache switch. / 获取可选下载缓存开关。</summary>
    public bool? Cache { get; }

    /// <summary>Gets the optional download timeout in seconds. / 获取可选下载超时秒数。</summary>
    public long? TimeoutSeconds { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject { ["file"] = File };
        AddBoolean(data, "magic", Magic);
        AddBoolean(data, "cache", Cache);
        AddInteger(data, "timeout", TimeoutSeconds);
        return data;
    }
}
