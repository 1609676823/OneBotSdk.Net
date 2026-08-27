using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Represents the send-only anonymous marker. / 表示仅发送的匿名标记。</summary>
[JsonConverter(typeof(OneBot10SendSegmentJsonConverter))]
public sealed class AnonymousSendSegment : OneBot10SendSegment
{
    /// <summary>Initializes the marker. / 初始化匿名标记。</summary>
    public AnonymousSendSegment(bool? ignoreFailure = null) : base(MessageSegmentTypes.Anonymous)
    {
        IgnoreFailure = ignoreFailure;
    }

    /// <summary>Gets whether sending continues when anonymous mode is unavailable. / 获取匿名模式不可用时是否继续发送。</summary>
    public bool? IgnoreFailure { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData()
    {
        var data = new JsonObject();
        AddBoolean(data, "ignore", IgnoreFailure);
        return data;
    }
}
