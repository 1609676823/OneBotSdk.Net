using System;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an implementation-defined outgoing segment while preserving all data. / 表示保留全部 data 的实现扩展出站消息段。</summary>
public sealed class OneBot12CustomSendSegment : OneBot12SendSegment
{
    private readonly JsonObject _data;

    /// <summary>Initializes an extension segment with detached data. / 使用独立 data 初始化扩展消息段。</summary>
    public OneBot12CustomSendSegment(string type, JsonObject data) : base(type)
    {
        _data = Clone(data ?? throw new ArgumentNullException(nameof(data)));
    }

    /// <summary>Gets a detached copy of the extension data. / 获取扩展 data 的独立副本。</summary>
    public JsonObject Data => Clone(_data);

    /// <inheritdoc />
    protected override JsonObject CreateData() => Clone(_data);
}
