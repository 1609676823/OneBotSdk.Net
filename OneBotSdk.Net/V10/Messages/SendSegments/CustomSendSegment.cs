using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents an implementation-specific outgoing segment. / 表示实现端特有的出站消息段。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class CustomSendSegment : OneBot10SendSegment
{
    private readonly JsonObject? _data;

    /// <summary>Initializes an extension segment while taking an independent data snapshot. / 初始化扩展消息段，同时取得独立数据快照。</summary>
    public CustomSendSegment(string type, JsonObject? data = null) : base(type)
    {
        _data = TolerantJson.Clone(data) as JsonObject;
    }

    /// <inheritdoc />
    protected override JsonObject? CreateData()
    {
        return TolerantJson.Clone(_data) as JsonObject;
    }
}
