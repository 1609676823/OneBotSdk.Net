using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents outgoing JSON rich content stored as a JSON string. / 表示以 JSON 字符串存储的出站 JSON 富文本。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class JsonSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes JSON rich content. / 初始化 JSON 富文本。</summary>
    public JsonSendSegment(string json) : base(MessageSegmentTypes.Json)
    {
        Json = json ?? throw new ArgumentNullException(nameof(json));
    }

    /// <summary>Gets the JSON string payload. / 获取 JSON 字符串内容。</summary>
    public string Json { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["data"] = Json };
}
