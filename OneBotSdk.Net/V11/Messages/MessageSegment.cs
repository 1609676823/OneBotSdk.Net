using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Lists the message segment type names defined by OneBot 11.
/// 列出 OneBot 11 定义的消息段类型名称。
/// </summary>
public static class MessageSegmentTypes
{
    /// <summary>The text segment type. / 纯文本消息段类型。</summary>
    public const string Text = "text";
    /// <summary>The QQ face segment type. / QQ 表情消息段类型。</summary>
    public const string Face = "face";
    /// <summary>The image segment type. / 图片消息段类型。</summary>
    public const string Image = "image";
    /// <summary>The voice record segment type. / 语音消息段类型。</summary>
    public const string Record = "record";
    /// <summary>The short-video segment type. / 短视频消息段类型。</summary>
    public const string Video = "video";
    /// <summary>The at-mention segment type. / @ 消息段类型。</summary>
    public const string At = "at";
    /// <summary>The rock-paper-scissors segment type. / 猜拳消息段类型。</summary>
    public const string Rps = "rps";
    /// <summary>The dice segment type. / 掷骰子消息段类型。</summary>
    public const string Dice = "dice";
    /// <summary>The window-shake segment type. / 窗口抖动消息段类型。</summary>
    public const string Shake = "shake";
    /// <summary>The poke segment type. / 戳一戳消息段类型。</summary>
    public const string Poke = "poke";
    /// <summary>The anonymous marker segment type. / 匿名标记消息段类型。</summary>
    public const string Anonymous = "anonymous";
    /// <summary>The link-share segment type. / 链接分享消息段类型。</summary>
    public const string Share = "share";
    /// <summary>The recommended-contact segment type. / 推荐联系人消息段类型。</summary>
    public const string Contact = "contact";
    /// <summary>The location segment type. / 位置消息段类型。</summary>
    public const string Location = "location";
    /// <summary>The music-share segment type. / 音乐分享消息段类型。</summary>
    public const string Music = "music";
    /// <summary>The reply segment type. / 回复消息段类型。</summary>
    public const string Reply = "reply";
    /// <summary>The merged-forward reference segment type. / 合并转发引用消息段类型。</summary>
    public const string Forward = "forward";
    /// <summary>The merged-forward node segment type. / 合并转发节点消息段类型。</summary>
    public const string Node = "node";
    /// <summary>The XML rich-message segment type. / XML 富消息段类型。</summary>
    public const string Xml = "xml";
    /// <summary>The JSON rich-message segment type. / JSON 富消息段类型。</summary>
    public const string Json = "json";
}

/// <summary>
/// Identifies every message-segment kind defined by the OneBot 11 standard while retaining an unknown fallback.
/// 标识 OneBot 11 标准定义的每一种消息段，同时保留未知回退值。
/// </summary>
public enum OneBot11MessageSegmentKind
{
    /// <summary>An extension or malformed segment type. / 扩展或格式异常的消息段类型。</summary>
    Unknown,
    /// <summary>Plain text. / 纯文本。</summary>
    Text,
    /// <summary>QQ face. / QQ 表情。</summary>
    Face,
    /// <summary>Image. / 图片。</summary>
    Image,
    /// <summary>Voice record. / 语音。</summary>
    Record,
    /// <summary>Short video. / 短视频。</summary>
    Video,
    /// <summary>At mention. / @ 提及。</summary>
    At,
    /// <summary>Rock-paper-scissors magic face. / 猜拳魔法表情。</summary>
    Rps,
    /// <summary>Dice magic face. / 骰子魔法表情。</summary>
    Dice,
    /// <summary>Window shake. / 窗口抖动。</summary>
    Shake,
    /// <summary>Poke. / 戳一戳。</summary>
    Poke,
    /// <summary>Anonymous marker. / 匿名标记。</summary>
    Anonymous,
    /// <summary>Link share. / 链接分享。</summary>
    Share,
    /// <summary>Recommended contact. / 推荐联系人。</summary>
    Contact,
    /// <summary>Location. / 位置。</summary>
    Location,
    /// <summary>Music share. / 音乐分享。</summary>
    Music,
    /// <summary>Reply reference. / 回复引用。</summary>
    Reply,
    /// <summary>Merged-forward reference. / 合并转发引用。</summary>
    Forward,
    /// <summary>Merged-forward node. / 合并转发节点。</summary>
    Node,
    /// <summary>XML rich message. / XML 富消息。</summary>
    Xml,
    /// <summary>JSON rich message. / JSON 富消息。</summary>
    Json
}

/// <summary>
/// Represents a OneBot 11 message segment while retaining unknown segment types and fields.
/// 表示 OneBot 11 消息段，同时保留未知消息段类型和字段。
/// </summary>
[JsonConverter(typeof(MessageSegmentJsonConverter))]
public sealed class MessageSegment : OneBot11JsonModel
{
    /// <summary>
    /// Initializes an empty segment for tolerant serializers and implementation extensions.
    /// 初始化空消息段，供容错序列化器和实现端扩展使用。
    /// </summary>
    public MessageSegment()
    {
    }

    /// <summary>
    /// Initializes a message segment with an arbitrary standard or extension type.
    /// 使用任意标准或扩展类型初始化消息段。
    /// </summary>
    public MessageSegment(string type, JsonObject? data = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("A message segment type is required.", nameof(type));
        }

        Type = type;
        Data = data;
        RawJson = ToJsonObject();
    }

    /// <summary>
    /// Gets or sets the segment discriminator.
    /// 获取或设置消息段判别值。
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the segment parameters. The protocol explicitly permits <see langword="null"/>.
    /// 获取或设置消息段参数。协议明确允许该值为 <see langword="null"/>。
    /// </summary>
    [JsonPropertyName("data")]
    public JsonObject? Data { get; set; }

    /// <summary>
    /// Gets the known standard kind without discarding the original <see cref="Type"/> string.
    /// 获取已知标准类型，同时不丢弃原始 <see cref="Type"/> 字符串。
    /// </summary>
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
                case MessageSegmentTypes.Anonymous: return OneBot11MessageSegmentKind.Anonymous;
                case MessageSegmentTypes.Share: return OneBot11MessageSegmentKind.Share;
                case MessageSegmentTypes.Contact: return OneBot11MessageSegmentKind.Contact;
                case MessageSegmentTypes.Location: return OneBot11MessageSegmentKind.Location;
                case MessageSegmentTypes.Music: return OneBot11MessageSegmentKind.Music;
                case MessageSegmentTypes.Reply: return OneBot11MessageSegmentKind.Reply;
                case MessageSegmentTypes.Forward: return OneBot11MessageSegmentKind.Forward;
                case MessageSegmentTypes.Node: return OneBot11MessageSegmentKind.Node;
                case MessageSegmentTypes.Xml: return OneBot11MessageSegmentKind.Xml;
                case MessageSegmentTypes.Json: return OneBot11MessageSegmentKind.Json;
                default: return OneBot11MessageSegmentKind.Unknown;
            }
        }
    }

    /// <summary>
    /// Reads a parameter as a tolerant string without invalidating the segment.
    /// 以容错字符串方式读取参数，不会使整个消息段失效。
    /// </summary>
    public string? GetString(string propertyName)
    {
        return TolerantJson.String(Data, propertyName);
    }

    /// <summary>
    /// Reads a OneBot boolean-like parameter such as 0/1, no/yes, or false/true.
    /// 读取 0/1、no/yes 或 false/true 等 OneBot 布尔风格参数。
    /// </summary>
    public bool? GetBoolean(string propertyName)
    {
        return TolerantJson.Boolean(Data, propertyName);
    }

    /// <summary>
    /// Creates an independent JSON object suitable for serialization.
    /// 创建适合序列化的独立 JSON 对象。
    /// </summary>
    public JsonObject ToJsonObject()
    {
        // Start from the original object so implementation-specific root fields round-trip.
        // 从原始对象开始构造，使实现端特有的根字段可往返保留。
        var result = RawJson.Count == 0 ? new JsonObject() : TolerantJson.CloneObject(RawJson);
        result["type"] = Type;
        result["data"] = TolerantJson.Clone(Data);
        return result;
    }

    /// <summary>
    /// Creates a deep copy that retains standard fields and implementation-specific extension fields.
    /// 创建深拷贝，同时保留标准字段与实现端特有扩展字段。
    /// </summary>
    public MessageSegment Clone()
    {
        // Round-trip through the tolerant object representation so nested JSON nodes are never shared.
        // 通过容错对象表示进行往返复制，确保嵌套 JSON 节点不会被共享。
        return Parse(ToJsonObject())!;
    }

    /// <summary>
    /// Parses one segment field by field and preserves unknown extension fields.
    /// 逐字段解析单个消息段，并保留未知扩展字段。
    /// </summary>
    public static MessageSegment? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new MessageSegment
        {
            Type = TolerantJson.String(source, "type"),
            Data = TolerantJson.Clone(TolerantJson.Node(source, "data")) as JsonObject,
            RawJson = TolerantJson.CloneObject(source)
        };
    }

    /// <summary>Creates a text segment. / 创建纯文本消息段。</summary>
    public static MessageSegment Text(string text)
    {
        return Create(MessageSegmentTypes.Text, "text", text);
    }

    /// <summary>Creates a QQ face segment. / 创建 QQ 表情消息段。</summary>
    public static MessageSegment Face(string id)
    {
        return Create(MessageSegmentTypes.Face, "id", id);
    }

    /// <summary>
    /// Creates an image segment using a received file name, file URI, network URL, or base64 URI.
    /// 使用已接收文件名、文件 URI、网络 URL 或 base64 URI 创建图片消息段。
    /// </summary>
    public static MessageSegment Image(string file, bool flash = false, bool? cache = null, bool? proxy = null, long? timeoutSeconds = null)
    {
        var data = Required("file", file);
        Add(data, "type", flash ? "flash" : null);
        AddBoolean(data, "cache", cache);
        AddBoolean(data, "proxy", proxy);
        AddNumber(data, "timeout", timeoutSeconds);
        return new MessageSegment(MessageSegmentTypes.Image, data);
    }

    /// <summary>Creates a voice record segment. / 创建语音消息段。</summary>
    public static MessageSegment Record(string file, bool? magic = null, bool? cache = null, bool? proxy = null, long? timeoutSeconds = null)
    {
        var data = Required("file", file);
        AddBoolean(data, "magic", magic);
        AddBoolean(data, "cache", cache);
        AddBoolean(data, "proxy", proxy);
        AddNumber(data, "timeout", timeoutSeconds);
        return new MessageSegment(MessageSegmentTypes.Record, data);
    }

    /// <summary>Creates a short-video segment. / 创建短视频消息段。</summary>
    public static MessageSegment Video(string file, bool? cache = null, bool? proxy = null, long? timeoutSeconds = null)
    {
        var data = Required("file", file);
        AddBoolean(data, "cache", cache);
        AddBoolean(data, "proxy", proxy);
        AddNumber(data, "timeout", timeoutSeconds);
        return new MessageSegment(MessageSegmentTypes.Video, data);
    }

    /// <summary>Creates an at-mention segment; use <c>all</c> to mention everyone. / 创建 @ 消息段；使用 <c>all</c> 表示全体成员。</summary>
    public static MessageSegment At(string qq)
    {
        return Create(MessageSegmentTypes.At, "qq", qq);
    }

    /// <summary>Creates a rock-paper-scissors magic face. / 创建猜拳魔法表情。</summary>
    public static MessageSegment Rps()
    {
        return Empty(MessageSegmentTypes.Rps);
    }

    /// <summary>Creates a dice magic face. / 创建掷骰子魔法表情。</summary>
    public static MessageSegment Dice()
    {
        return Empty(MessageSegmentTypes.Dice);
    }

    /// <summary>Creates a window-shake segment. / 创建窗口抖动消息段。</summary>
    public static MessageSegment Shake()
    {
        return Empty(MessageSegmentTypes.Shake);
    }

    /// <summary>Creates a poke segment. / 创建戳一戳消息段。</summary>
    public static MessageSegment Poke(string type, string id)
    {
        var data = Required("type", type);
        data["id"] = id;
        return new MessageSegment(MessageSegmentTypes.Poke, data);
    }

    /// <summary>Creates the send-only anonymous marker. / 创建仅用于发送的匿名标记消息段。</summary>
    public static MessageSegment Anonymous(bool? ignoreFailure = null)
    {
        var data = new JsonObject();
        AddBoolean(data, "ignore", ignoreFailure);
        return new MessageSegment(MessageSegmentTypes.Anonymous, data);
    }

    /// <summary>Creates a link-share segment. / 创建链接分享消息段。</summary>
    public static MessageSegment Share(string url, string title, string? content = null, string? image = null)
    {
        var data = Required("url", url);
        data["title"] = title;
        Add(data, "content", content);
        Add(data, "image", image);
        return new MessageSegment(MessageSegmentTypes.Share, data);
    }

    /// <summary>Creates a recommended-friend segment. / 创建推荐好友消息段。</summary>
    public static MessageSegment ContactFriend(string userId)
    {
        return Contact("qq", userId);
    }

    /// <summary>Creates a recommended-group segment. / 创建推荐群消息段。</summary>
    public static MessageSegment ContactGroup(string groupId)
    {
        return Contact("group", groupId);
    }

    /// <summary>Creates a location segment. / 创建位置消息段。</summary>
    public static MessageSegment Location(string latitude, string longitude, string? title = null, string? content = null)
    {
        var data = Required("lat", latitude);
        data["lon"] = longitude;
        Add(data, "title", title);
        Add(data, "content", content);
        return new MessageSegment(MessageSegmentTypes.Location, data);
    }

    /// <summary>Creates a provider-backed music share using qq, 163, or xm. / 使用 qq、163 或 xm 创建平台音乐分享。</summary>
    public static MessageSegment Music(string provider, string id)
    {
        var data = Required("type", provider);
        data["id"] = id;
        return new MessageSegment(MessageSegmentTypes.Music, data);
    }

    /// <summary>Creates a custom music share. / 创建自定义音乐分享。</summary>
    public static MessageSegment CustomMusic(string url, string audio, string title, string? content = null, string? image = null)
    {
        var data = Required("type", "custom");
        data["url"] = url;
        data["audio"] = audio;
        data["title"] = title;
        Add(data, "content", content);
        Add(data, "image", image);
        return new MessageSegment(MessageSegmentTypes.Music, data);
    }

    /// <summary>Creates a reply segment referencing a message ID. / 创建引用消息 ID 的回复消息段。</summary>
    public static MessageSegment Reply(string messageId)
    {
        return Create(MessageSegmentTypes.Reply, "id", messageId);
    }

    /// <summary>Creates the receive-only merged-forward reference representation. / 创建仅接收的合并转发引用表示。</summary>
    public static MessageSegment Forward(string forwardId)
    {
        return Create(MessageSegmentTypes.Forward, "id", forwardId);
    }

    /// <summary>Creates a merged-forward node referencing an existing message. / 创建引用已有消息的合并转发节点。</summary>
    public static MessageSegment Node(string messageId)
    {
        return Create(MessageSegmentTypes.Node, "id", messageId);
    }

    /// <summary>Creates a custom merged-forward node. / 创建自定义合并转发节点。</summary>
    public static MessageSegment CustomNode(string userId, string nickname, OneBot11Message content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var data = Required("user_id", userId);
        data["nickname"] = nickname;
        data["content"] = content.ToJsonNode();
        return new MessageSegment(MessageSegmentTypes.Node, data);
    }

    /// <summary>Creates an XML rich-message segment. / 创建 XML 富消息段。</summary>
    public static MessageSegment Xml(string xml)
    {
        return Create(MessageSegmentTypes.Xml, "data", xml);
    }

    /// <summary>Creates a JSON rich-message segment whose data remains a JSON string. / 创建 data 保持为 JSON 字符串的 JSON 富消息段。</summary>
    public static MessageSegment Json(string json)
    {
        return Create(MessageSegmentTypes.Json, "data", json);
    }

    private static MessageSegment Contact(string type, string id)
    {
        var data = Required("type", type);
        data["id"] = id;
        return new MessageSegment(MessageSegmentTypes.Contact, data);
    }

    private static MessageSegment Empty(string type)
    {
        return new MessageSegment(type, new JsonObject());
    }

    private static MessageSegment Create(string type, string propertyName, string value)
    {
        return new MessageSegment(type, Required(propertyName, value));
    }

    private static JsonObject Required(string propertyName, string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new JsonObject { [propertyName] = value };
    }

    private static void Add(JsonObject data, string propertyName, string? value)
    {
        if (value != null)
        {
            data[propertyName] = value;
        }
    }

    private static void AddBoolean(JsonObject data, string propertyName, bool? value)
    {
        if (value.HasValue)
        {
            // Message-segment booleans are strings in the canonical array representation.
            // 规范数组格式中的消息段布尔值使用字符串表示。
            data[propertyName] = value.Value ? "1" : "0";
        }
    }

    private static void AddNumber(JsonObject data, string propertyName, long? value)
    {
        if (value.HasValue)
        {
            data[propertyName] = value.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}

/// <summary>
/// Serializes message segments through their tolerant, extension-preserving JSON representation.
/// 通过容错且保留扩展字段的 JSON 表示序列化消息段。
/// </summary>
public sealed class MessageSegmentJsonConverter : JsonConverter<MessageSegment>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override MessageSegment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        try
        {
            return MessageSegment.Parse(JsonNode.Parse(ref reader));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MessageSegment value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        value.ToJsonObject().WriteTo(writer, options);
    }
}
