using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Represents one parsed incoming OneBot 11 segment without exposing outgoing-only parameters.
/// 表示一个已解析的入站 OneBot 11 消息段，并且不公开仅发送参数。
/// </summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public abstract class OneBot11ReceivedSegment : OneBot11JsonModel
{
    /// <summary>Gets the original segment discriminator. / 获取原始消息段判别值。</summary>
    [JsonPropertyName("type")]
    public string? Type { get; internal set; }

    /// <summary>Gets an independent copy of the complete data object. / 获取完整数据对象的独立副本。</summary>
    [JsonPropertyName("data")]
    public JsonObject? Data { get; internal set; }

    /// <summary>Gets the known standard kind while retaining the original discriminator. / 获取已知标准类型，同时保留原始判别值。</summary>
    [JsonIgnore]
    public OneBot11MessageSegmentKind Kind
    {
        get
        {
            switch (Type)
            {
                case MessageSegmentTypes.Text: return OneBot11MessageSegmentKind.Text;
                case MessageSegmentTypes.Face: return OneBot11MessageSegmentKind.Face;
                case MessageSegmentTypes.Image: return OneBot11MessageSegmentKind.Image;
                case MessageSegmentTypes.Record: return OneBot11MessageSegmentKind.Record;
                case MessageSegmentTypes.Video: return OneBot11MessageSegmentKind.Video;
                case MessageSegmentTypes.At: return OneBot11MessageSegmentKind.At;
                case MessageSegmentTypes.Rps: return OneBot11MessageSegmentKind.Rps;
                case MessageSegmentTypes.Dice: return OneBot11MessageSegmentKind.Dice;
                case MessageSegmentTypes.Shake: return OneBot11MessageSegmentKind.Shake;
                case MessageSegmentTypes.Poke: return OneBot11MessageSegmentKind.Poke;
                case MessageSegmentTypes.Share: return OneBot11MessageSegmentKind.Share;
                case MessageSegmentTypes.Contact: return OneBot11MessageSegmentKind.Contact;
                case MessageSegmentTypes.Location: return OneBot11MessageSegmentKind.Location;
                case MessageSegmentTypes.Reply: return OneBot11MessageSegmentKind.Reply;
                case MessageSegmentTypes.Forward: return OneBot11MessageSegmentKind.Forward;
                case MessageSegmentTypes.Node: return OneBot11MessageSegmentKind.Node;
                case MessageSegmentTypes.Xml: return OneBot11MessageSegmentKind.Xml;
                case MessageSegmentTypes.Json: return OneBot11MessageSegmentKind.Json;
                default: return OneBot11MessageSegmentKind.Unknown;
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
    public static OneBot11ReceivedSegment? Parse(JsonNode? node)
    {
        return OneBot11ReceivedSegmentParser.Parse(node);
    }
}
