using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Represents one parsed incoming OneBot 10 segment without exposing outgoing-only parameters.
/// 表示一个已解析的入站 OneBot 10 消息段，并且不公开仅发送参数。
/// </summary>
[JsonConverter(typeof(OneBot10ReceivedSegmentJsonConverter))]
public abstract class OneBot10ReceivedSegment : OneBot10JsonModel
{
    /// <summary>Gets the original segment discriminator. / 获取原始消息段判别值。</summary>
    [JsonPropertyName("type")]
    public string? Type { get; internal set; }

    /// <summary>Gets an independent copy of the complete data object. / 获取完整数据对象的独立副本。</summary>
    [JsonPropertyName("data")]
    public JsonObject? Data { get; internal set; }

    /// <summary>Gets the known standard kind while retaining the original discriminator. / 获取已知标准类型，同时保留原始判别值。</summary>
    [JsonIgnore]
    public OneBot10MessageSegmentKind Kind
    {
        get
        {
            switch (Type)
            {
                case MessageSegmentTypes.Text: return OneBot10MessageSegmentKind.Text;
                case MessageSegmentTypes.Face: return OneBot10MessageSegmentKind.Face;
                case MessageSegmentTypes.Image: return OneBot10MessageSegmentKind.Image;
                case MessageSegmentTypes.Record: return OneBot10MessageSegmentKind.Record;
                case MessageSegmentTypes.At: return OneBot10MessageSegmentKind.At;
                case MessageSegmentTypes.Rps: return OneBot10MessageSegmentKind.Rps;
                case MessageSegmentTypes.Dice: return OneBot10MessageSegmentKind.Dice;
                case MessageSegmentTypes.Shake: return OneBot10MessageSegmentKind.Shake;
                case MessageSegmentTypes.Share: return OneBot10MessageSegmentKind.Share;
                case MessageSegmentTypes.Contact: return OneBot10MessageSegmentKind.Contact;
                case MessageSegmentTypes.Location: return OneBot10MessageSegmentKind.Location;
                case MessageSegmentTypes.Rich: return OneBot10MessageSegmentKind.Rich;
                default: return OneBot10MessageSegmentKind.Unknown;
            }
        }
    }

    /// <summary>Creates an independent wire object including retained implementation extensions. / 创建包含已保留实现端扩展的独立线协议对象。</summary>
    public JsonObject ToJsonObject()
    {
        var result = RawJson.Count == 0 ? new JsonObject() : TolerantJson.CloneObject(RawJson);
        result["type"] = Type;
        result["data"] = TolerantJson.Clone(Data);
        return result;
    }

    /// <summary>
    /// Parses one incoming segment into its concrete received type while preserving unknown fields.
    /// 将单个入站消息段解析为具体接收类型，同时保留未知字段。
    /// </summary>
    public static OneBot10ReceivedSegment? Parse(JsonNode? node)
    {
        return OneBot10ReceivedSegmentParser.Parse(node);
    }
}
