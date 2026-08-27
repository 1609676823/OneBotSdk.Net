using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Identifies an outgoing message JSON shape accepted by OneBot APIs. / 标识 OneBot API 接受的出站消息 JSON 形态。</summary>
public enum OneBot11SendMessageKind
{
    /// <summary>A CQ-code string. / CQ 码字符串。</summary>
    String,
    /// <summary>A single message-segment object. / 单个消息段对象。</summary>
    Segment,
    /// <summary>An ordered message-segment array. / 有序消息段数组。</summary>
    SegmentArray
}

/// <summary>
/// Represents an outgoing OneBot 11 message and acts as a strongly typed message-chain builder.
/// 表示出站 OneBot 11 消息，并作为强类型消息链构建器使用。
/// </summary>
[JsonConverter(typeof(OneBot11SendMessageJsonConverter))]
public sealed class OneBot11SendMessage : IReadOnlyList<OneBot11SendSegment>
{
    private readonly List<OneBot11SendSegment> _segments;

    /// <summary>Initializes an empty array-format message suitable for collection initializers. / 初始化适合集合初始化器的空数组格式消息。</summary>
    public OneBot11SendMessage()
        : this(OneBot11SendMessageKind.SegmentArray, null, new List<OneBot11SendSegment>())
    {
    }

    /// <summary>Initializes an array-format message from ordered outgoing segments. / 通过有序出站消息段初始化数组格式消息。</summary>
    public OneBot11SendMessage(IEnumerable<OneBot11SendSegment> segments)
        : this()
    {
        AddRange(segments);
    }

    private OneBot11SendMessage(
        OneBot11SendMessageKind kind,
        string? stringValue,
        List<OneBot11SendSegment> segments)
    {
        Kind = kind;
        StringValue = stringValue;
        _segments = segments;
    }

    /// <summary>Gets the selected outgoing wire shape. / 获取所选出站线协议形态。</summary>
    public OneBot11SendMessageKind Kind { get; }

    /// <summary>Gets the CQ-code value for a string-format message. / 获取字符串格式消息的 CQ 码值。</summary>
    public string? StringValue { get; }

    /// <inheritdoc />
    public int Count => _segments.Count;

    /// <inheritdoc />
    public OneBot11SendSegment this[int index] => _segments[index];

    /// <summary>Creates a message that preserves a CQ-code string. / 创建保留 CQ 码字符串的消息。</summary>
    public static OneBot11SendMessage FromString(string value)
    {
        return new OneBot11SendMessage(
            OneBot11SendMessageKind.String,
            value ?? throw new ArgumentNullException(nameof(value)),
            new List<OneBot11SendSegment>());
    }

    /// <summary>Creates the single-segment shape accepted by OneBot API parameters. / 创建 OneBot API 参数接受的单消息段形态。</summary>
    public static OneBot11SendMessage FromSegment(OneBot11SendSegment segment)
    {
        if (segment == null)
        {
            throw new ArgumentNullException(nameof(segment));
        }

        return new OneBot11SendMessage(
            OneBot11SendMessageKind.Segment,
            null,
            new List<OneBot11SendSegment> { segment });
    }

    /// <summary>Creates an array-format message from ordered outgoing segments. / 通过有序出站消息段创建数组格式消息。</summary>
    public static OneBot11SendMessage FromSegments(IEnumerable<OneBot11SendSegment> segments)
    {
        return new OneBot11SendMessage(segments);
    }

    /// <summary>Creates an array-format message from supplied outgoing segments. / 通过给定出站消息段创建数组格式消息。</summary>
    public static OneBot11SendMessage FromSegments(params OneBot11SendSegment[] segments)
    {
        return FromSegments((IEnumerable<OneBot11SendSegment>)segments);
    }

    /// <summary>Adds an outgoing segment to an array-format message. / 向数组格式消息添加出站消息段。</summary>
    public void Add(OneBot11SendSegment segment)
    {
        EnsureArrayBuilder();
        _segments.Add(segment ?? throw new ArgumentNullException(nameof(segment)));
    }

    /// <summary>Adds ordered outgoing segments and returns this message. / 添加有序出站消息段并返回当前消息。</summary>
    public OneBot11SendMessage AddRange(IEnumerable<OneBot11SendSegment> segments)
    {
        if (segments == null)
        {
            throw new ArgumentNullException(nameof(segments));
        }

        foreach (var segment in segments)
        {
            Add(segment);
        }

        return this;
    }

    /// <summary>Adds plain text. / 添加纯文本。</summary>
    public OneBot11SendMessage Text(string text)
    {
        Add(new TextSendSegment(text));
        return this;
    }

    /// <summary>Adds a QQ face. / 添加 QQ 表情。</summary>
    public OneBot11SendMessage Face(string id)
    {
        Add(new FaceSendSegment(id));
        return this;
    }

    /// <summary>Adds an at-mention by QQ ID. / 通过 QQ 号添加 @ 消息。</summary>
    public OneBot11SendMessage At(long userId)
    {
        Add(new AtSendSegment(userId));
        return this;
    }

    /// <summary>Adds an at-mention by protocol target. / 通过协议目标添加 @ 消息。</summary>
    public OneBot11SendMessage At(string target)
    {
        Add(new AtSendSegment(target));
        return this;
    }

    /// <summary>Adds an at-all segment. / 添加 @ 全体成员消息段。</summary>
    public OneBot11SendMessage AtAll()
    {
        return At("all");
    }

    /// <summary>Adds an image with outgoing-only download options. / 添加包含发送专用下载选项的图片。</summary>
    public OneBot11SendMessage Image(
        string file,
        bool flash = false,
        bool? cache = null,
        bool? proxy = null,
        long? timeoutSeconds = null)
    {
        Add(new ImageSendSegment(file, flash, cache, proxy, timeoutSeconds));
        return this;
    }

    /// <summary>Adds a voice record with outgoing-only download options. / 添加包含发送专用下载选项的语音。</summary>
    public OneBot11SendMessage Record(
        string file,
        bool? magic = null,
        bool? cache = null,
        bool? proxy = null,
        long? timeoutSeconds = null)
    {
        Add(new RecordSendSegment(file, magic, cache, proxy, timeoutSeconds));
        return this;
    }

    /// <summary>Adds a short video with outgoing-only download options. / 添加包含发送专用下载选项的短视频。</summary>
    public OneBot11SendMessage Video(
        string file,
        bool? cache = null,
        bool? proxy = null,
        long? timeoutSeconds = null)
    {
        Add(new VideoSendSegment(file, cache, proxy, timeoutSeconds));
        return this;
    }

    /// <summary>Adds a reply reference. / 添加回复引用。</summary>
    public OneBot11SendMessage Reply(long messageId)
    {
        Add(new ReplySendSegment(messageId));
        return this;
    }

    /// <summary>Adds an XML rich message. / 添加 XML 富消息。</summary>
    public OneBot11SendMessage Xml(string xml)
    {
        Add(new XmlSendSegment(xml));
        return this;
    }

    /// <summary>Adds a JSON rich message. / 添加 JSON 富消息。</summary>
    public OneBot11SendMessage Json(string json)
    {
        Add(new JsonSendSegment(json));
        return this;
    }

    /// <summary>Creates an independent JSON value for an API parameter. / 为 API 参数创建独立 JSON 值。</summary>
    public JsonNode? ToJsonNode()
    {
        switch (Kind)
        {
            case OneBot11SendMessageKind.String:
                return JsonValue.Create(StringValue);
            case OneBot11SendMessageKind.Segment:
                return _segments.Count == 0 ? null : _segments[0].ToJsonObject();
            case OneBot11SendMessageKind.SegmentArray:
                var array = new JsonArray();
                foreach (var segment in _segments)
                {
                    array.Add(segment.ToJsonObject());
                }

                return array;
            default:
                throw new InvalidOperationException("Unknown outgoing message representation.");
        }
    }

    /// <summary>Converts the outgoing message to CQ-code text. / 将出站消息转换为 CQ 码文本。</summary>
    public string ToCqCode()
    {
        if (Kind == OneBot11SendMessageKind.String)
        {
            return StringValue ?? string.Empty;
        }

        var result = new StringBuilder();
        foreach (var segment in _segments)
        {
            var legacy = MessageSegment.Parse(segment.ToJsonObject());
            if (legacy != null)
            {
                result.Append(CqCodeCodec.Encode(new[] { legacy }));
            }
        }

        return result.ToString();
    }

    /// <summary>Creates a compatibility message without exposing received-only fields. / 创建不公开接收专用字段的兼容消息。</summary>
    public OneBot11Message ToLegacyMessage()
    {
        if (Kind == OneBot11SendMessageKind.String)
        {
            return OneBot11Message.FromString(StringValue ?? string.Empty);
        }

        var segments = new List<MessageSegment>();
        foreach (var segment in _segments)
        {
            var legacy = MessageSegment.Parse(segment.ToJsonObject());
            if (legacy != null)
            {
                segments.Add(legacy);
            }
        }

        return Kind == OneBot11SendMessageKind.Segment && segments.Count != 0
            ? OneBot11Message.FromSegment(segments[0])
            : OneBot11Message.FromSegments(segments);
    }

    /// <inheritdoc />
    public IEnumerator<OneBot11SendSegment> GetEnumerator() => _segments.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public override string ToString() => ToCqCode();

    private void EnsureArrayBuilder()
    {
        if (Kind != OneBot11SendMessageKind.SegmentArray)
        {
            throw new InvalidOperationException("Segments can only be added to an array-format outgoing message.");
        }
    }
}

/// <summary>Writes outgoing messages and deliberately does not parse received payloads. / 写入出站消息，并且有意不解析接收负载。</summary>
public sealed class OneBot11SendMessageJsonConverter : JsonConverter<OneBot11SendMessage>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override OneBot11SendMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException("Use OneBot11ReceivedMessage to parse an incoming message payload.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneBot11SendMessage value, JsonSerializerOptions options)
    {
        var node = value?.ToJsonNode();
        if (node == null)
        {
            writer.WriteNullValue();
            return;
        }

        node.WriteTo(writer, options);
    }
}
