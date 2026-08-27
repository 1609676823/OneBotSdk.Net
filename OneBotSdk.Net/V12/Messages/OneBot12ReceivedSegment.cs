using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents one independently parsed incoming OneBot 12 segment. / 表示一个独立解析的 OneBot 12 入站消息段。</summary>
[JsonConverter(typeof(OneBot12ReceivedSegmentJsonConverter))]
public abstract class OneBot12ReceivedSegment : OneBot12JsonModel
{
    private protected OneBot12ReceivedSegment(string? type, JsonObject data, JsonObject rawJson)
        : base(rawJson)
    {
        Type = type;
        Data = data;
    }

    /// <summary>Gets the original segment discriminator. / 获取原始消息段判别值。</summary>
    [JsonPropertyName("type")]
    public string? Type { get; }

    /// <summary>Gets a detached complete data object, including extensions. / 获取包含扩展的完整独立 data 对象。</summary>
    [JsonPropertyName("data")]
    public JsonObject Data { get; }

    /// <summary>Gets the known standard kind. / 获取已知标准类型。</summary>
    [JsonIgnore]
    public OneBot12MessageSegmentKind Kind
    {
        get
        {
            switch (Type)
            {
                case OneBot12MessageSegmentTypes.Text: return OneBot12MessageSegmentKind.Text;
                case OneBot12MessageSegmentTypes.Mention: return OneBot12MessageSegmentKind.Mention;
                case OneBot12MessageSegmentTypes.MentionAll: return OneBot12MessageSegmentKind.MentionAll;
                case OneBot12MessageSegmentTypes.Image: return OneBot12MessageSegmentKind.Image;
                case OneBot12MessageSegmentTypes.Voice: return OneBot12MessageSegmentKind.Voice;
                case OneBot12MessageSegmentTypes.Audio: return OneBot12MessageSegmentKind.Audio;
                case OneBot12MessageSegmentTypes.Video: return OneBot12MessageSegmentKind.Video;
                case OneBot12MessageSegmentTypes.File: return OneBot12MessageSegmentKind.File;
                case OneBot12MessageSegmentTypes.Location: return OneBot12MessageSegmentKind.Location;
                case OneBot12MessageSegmentTypes.Reply: return OneBot12MessageSegmentKind.Reply;
                default: return OneBot12MessageSegmentKind.Unknown;
            }
        }
    }

    /// <summary>Parses one standard or extension received segment. / 解析一个标准或扩展接收消息段。</summary>
    public static OneBot12ReceivedSegment? Parse(JsonNode? node) => OneBot12ReceivedSegmentParser.Parse(node);

    /// <summary>Creates a detached wire object preserving unknown fields. / 创建保留未知字段的独立线协议对象。</summary>
    public JsonObject ToJsonObject()
    {
        var result = TolerantJson.CloneObject(RawJson);
        result["type"] = Type;
        result["data"] = TolerantJson.Clone(Data);
        return result;
    }
}
