using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Identifies an outgoing message shape accepted by OneBot 12 actions. / 标识 OneBot 12 动作接受的出站消息形态。</summary>
public enum OneBot12SendMessageKind
{
    /// <summary>A shorthand plain-text string. / 纯文本字符串简写。</summary>
    String,
    /// <summary>One segment object. / 单个消息段对象。</summary>
    Segment,
    /// <summary>An ordered segment array. / 有序消息段数组。</summary>
    SegmentArray
}

/// <summary>Represents a strongly typed outgoing OneBot 12 message. / 表示强类型 OneBot 12 出站消息。</summary>
[JsonConverter(typeof(OneBot12SendMessageJsonConverter))]
public sealed class OneBot12SendMessage : IReadOnlyList<OneBot12SendSegment>
{
    private readonly List<OneBot12SendSegment> _segments;

    /// <summary>Initializes an empty array-format message. / 初始化空数组格式消息。</summary>
    public OneBot12SendMessage()
        : this(OneBot12SendMessageKind.SegmentArray, null, new List<OneBot12SendSegment>())
    {
    }

    /// <summary>Initializes an array-format message from ordered segments. / 使用有序消息段初始化数组格式消息。</summary>
    public OneBot12SendMessage(IEnumerable<OneBot12SendSegment> segments)
        : this()
    {
        AddRange(segments);
    }

    private OneBot12SendMessage(OneBot12SendMessageKind kind, string? text, List<OneBot12SendSegment> segments)
    {
        Kind = kind;
        StringValue = text;
        _segments = segments;
    }

    /// <summary>Gets the selected wire representation. / 获取所选线协议表示。</summary>
    public OneBot12SendMessageKind Kind { get; }
    /// <summary>Gets shorthand text when the selected shape is a string. / 所选形态为字符串时获取简写文本。</summary>
    public string? StringValue { get; }
    /// <inheritdoc />
    public int Count => _segments.Count;
    /// <inheritdoc />
    public OneBot12SendSegment this[int index] => _segments[index];

    /// <summary>Creates the permitted shorthand string shape, interpreted only as text. / 创建允许的字符串简写形态，该字符串只解释为纯文本。</summary>
    public static OneBot12SendMessage FromString(string text)
    {
        return new OneBot12SendMessage(
            OneBot12SendMessageKind.String,
            text ?? throw new ArgumentNullException(nameof(text)),
            new List<OneBot12SendSegment>());
    }

    /// <summary>Creates the permitted single-segment shape. / 创建允许的单消息段形态。</summary>
    public static OneBot12SendMessage FromSegment(OneBot12SendSegment segment)
    {
        if (segment == null)
        {
            throw new ArgumentNullException(nameof(segment));
        }

        return new OneBot12SendMessage(
            OneBot12SendMessageKind.Segment,
            null,
            new List<OneBot12SendSegment> { segment });
    }

    /// <summary>Creates an array-format message. / 创建数组格式消息。</summary>
    public static OneBot12SendMessage FromSegments(params OneBot12SendSegment[] segments)
    {
        return new OneBot12SendMessage(segments ?? throw new ArgumentNullException(nameof(segments)));
    }

    /// <summary>Adds a segment to an array-format message. / 向数组格式消息添加消息段。</summary>
    public void Add(OneBot12SendSegment segment)
    {
        EnsureArrayBuilder();
        _segments.Add(segment ?? throw new ArgumentNullException(nameof(segment)));
    }

    /// <summary>Adds ordered segments and returns this message. / 添加有序消息段并返回当前消息。</summary>
    public OneBot12SendMessage AddRange(IEnumerable<OneBot12SendSegment> segments)
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
    public OneBot12SendMessage Text(string text) { Add(new OneBot12TextSendSegment(text)); return this; }
    /// <summary>Adds a user mention. / 添加用户提及。</summary>
    public OneBot12SendMessage Mention(string userId) { Add(new OneBot12MentionSendSegment(userId)); return this; }
    /// <summary>Adds a mention-all segment. / 添加提及全体消息段。</summary>
    public OneBot12SendMessage MentionAll() { Add(new OneBot12MentionAllSendSegment()); return this; }
    /// <summary>Adds an image file reference. / 添加图片文件引用。</summary>
    public OneBot12SendMessage Image(string fileId) { Add(new OneBot12ImageSendSegment(fileId)); return this; }
    /// <summary>Adds a recorded-voice reference. / 添加录制语音引用。</summary>
    public OneBot12SendMessage Voice(string fileId) { Add(new OneBot12VoiceSendSegment(fileId)); return this; }
    /// <summary>Adds an audio reference. / 添加音频引用。</summary>
    public OneBot12SendMessage Audio(string fileId) { Add(new OneBot12AudioSendSegment(fileId)); return this; }
    /// <summary>Adds a video reference. / 添加视频引用。</summary>
    public OneBot12SendMessage Video(string fileId) { Add(new OneBot12VideoSendSegment(fileId)); return this; }
    /// <summary>Adds a generic file reference. / 添加通用文件引用。</summary>
    public OneBot12SendMessage File(string fileId) { Add(new OneBot12FileSendSegment(fileId)); return this; }
    /// <summary>Adds a geographical location. / 添加地理位置。</summary>
    public OneBot12SendMessage Location(double latitude, double longitude, string title, string content) { Add(new OneBot12LocationSendSegment(latitude, longitude, title, content)); return this; }
    /// <summary>Adds a reply reference. / 添加回复引用。</summary>
    public OneBot12SendMessage Reply(string messageId, string? userId = null) { Add(new OneBot12ReplySendSegment(messageId, userId)); return this; }

    /// <summary>Creates a detached JSON value in the selected outgoing shape. / 以所选出站形态创建独立 JSON 值。</summary>
    public JsonNode? ToJsonNode()
    {
        if (Kind == OneBot12SendMessageKind.String)
        {
            return JsonValue.Create(StringValue);
        }

        if (Kind == OneBot12SendMessageKind.Segment)
        {
            return _segments.Count == 0 ? null : _segments[0].ToJsonObject();
        }

        var array = new JsonArray();
        foreach (var segment in _segments)
        {
            array.Add(segment.ToJsonObject());
        }

        return array;
    }

    /// <inheritdoc />
    public IEnumerator<OneBot12SendSegment> GetEnumerator() => _segments.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Converts text to the protocol shorthand string shape. / 将文本转换为协议字符串简写形态。</summary>
    public static implicit operator OneBot12SendMessage(string text) => FromString(text);
    /// <summary>Converts one segment to the protocol single-segment shape. / 将单个消息段转换为协议单段形态。</summary>
    public static implicit operator OneBot12SendMessage(OneBot12SendSegment segment) => FromSegment(segment);

    private void EnsureArrayBuilder()
    {
        if (Kind != OneBot12SendMessageKind.SegmentArray)
        {
            throw new InvalidOperationException("Segments can only be added to an array-format message.");
        }
    }
}

/// <summary>Writes outgoing messages and deliberately does not parse incoming payloads. / 写出出站消息，并且有意不解析入站负载。</summary>
public sealed class OneBot12SendMessageJsonConverter : JsonConverter<OneBot12SendMessage>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override OneBot12SendMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException("Use OneBot12ReceivedMessage to parse incoming messages.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneBot12SendMessage value, JsonSerializerOptions options)
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
