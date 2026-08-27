using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Identifies the JSON representation retained by a OneBot 11 message.
/// 标识 OneBot 11 消息所保留的 JSON 表示形式。
/// </summary>
public enum OneBot11MessageKind
{
    /// <summary>A CQ-code string. / CQ 码字符串。</summary>
    String,

    /// <summary>A single message segment accepted by API parameters. / API 参数可接受的单个消息段。</summary>
    Segment,

    /// <summary>An array of message segments. / 消息段数组。</summary>
    SegmentArray
}

/// <summary>
/// Represents the OneBot 11 message union: CQ string, one segment, or a segment array.
/// 表示 OneBot 11 消息联合类型：CQ 字符串、单个消息段或消息段数组。
/// </summary>
[JsonConverter(typeof(OneBot11MessageJsonConverter))]
public sealed class OneBot11Message
{
    private readonly IReadOnlyList<MessageSegment> _segments;

    private OneBot11Message(OneBot11MessageKind kind, string? stringValue, IReadOnlyList<MessageSegment> segments, JsonNode? rawJson)
    {
        Kind = kind;
        StringValue = stringValue;
        _segments = segments;
        RawJson = rawJson;
    }

    /// <summary>
    /// Gets the retained wire representation.
    /// 获取所保留的线协议表示形式。
    /// </summary>
    public OneBot11MessageKind Kind { get; }

    /// <summary>
    /// Gets the CQ-code value when <see cref="Kind"/> is <see cref="OneBot11MessageKind.String"/>.
    /// 当 <see cref="Kind"/> 为 <see cref="OneBot11MessageKind.String"/> 时获取 CQ 码值。
    /// </summary>
    public string? StringValue { get; }

    /// <summary>
    /// Gets the segments for either the single-segment or array representation.
    /// 获取单消息段或消息段数组表示中的消息段。
    /// </summary>
    public IReadOnlyList<MessageSegment> Segments => _segments;

    /// <summary>
    /// Gets the original JSON value retained during tolerant parsing.
    /// 获取容错解析期间保留的原始 JSON 值。
    /// </summary>
    [JsonIgnore]
    public JsonNode? RawJson { get; }

    /// <summary>
    /// Creates a message that retains its CQ string representation.
    /// 创建保留 CQ 字符串表示的消息。
    /// </summary>
    public static OneBot11Message FromString(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new OneBot11Message(
            OneBot11MessageKind.String,
            value,
            Array.Empty<MessageSegment>(),
            JsonValue.Create(value));
    }

    /// <summary>
    /// Creates the single-segment representation accepted by OneBot API parameters.
    /// 创建 OneBot API 参数可接受的单消息段表示。
    /// </summary>
    public static OneBot11Message FromSegment(MessageSegment segment)
    {
        if (segment == null)
        {
            throw new ArgumentNullException(nameof(segment));
        }

        var segments = new[] { segment };
        return new OneBot11Message(
            OneBot11MessageKind.Segment,
            null,
            segments,
            segment.ToJsonObject());
    }

    /// <summary>
    /// Creates an array-format message from an ordered segment sequence.
    /// 从有序消息段序列创建数组格式消息。
    /// </summary>
    public static OneBot11Message FromSegments(IEnumerable<MessageSegment> segments)
    {
        if (segments == null)
        {
            throw new ArgumentNullException(nameof(segments));
        }

        var copy = new List<MessageSegment>();
        foreach (var segment in segments)
        {
            if (segment == null)
            {
                throw new ArgumentException("A message cannot contain a null segment.", nameof(segments));
            }

            copy.Add(segment);
        }

        var array = new JsonArray();
        foreach (var segment in copy)
        {
            array.Add(segment.ToJsonObject());
        }

        return new OneBot11Message(OneBot11MessageKind.SegmentArray, null, copy.AsReadOnly(), array);
    }

    /// <summary>
    /// Creates an array-format message from the supplied segments.
    /// 从给定消息段创建数组格式消息。
    /// </summary>
    public static OneBot11Message FromSegments(params MessageSegment[] segments)
    {
        return FromSegments((IEnumerable<MessageSegment>)segments);
    }

    /// <summary>
    /// Parses any protocol-supported message JSON shape without throwing for field-level drift.
    /// 解析协议支持的任意消息 JSON 形态，不会因字段级类型漂移而抛出异常。
    /// </summary>
    public static OneBot11Message? Parse(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        if (node is JsonObject objectValue)
        {
            var segment = MessageSegment.Parse(objectValue);
            if (segment == null)
            {
                return null;
            }

            return new OneBot11Message(
                OneBot11MessageKind.Segment,
                null,
                new[] { segment },
                TolerantJson.Clone(node));
        }

        if (node is JsonArray arrayValue)
        {
            var segments = new List<MessageSegment>();
            var count = TolerantJson.Count(arrayValue);
            for (var index = 0; index < count; index++)
            {
                // A malformed element is isolated while valid siblings remain available.
                // 隔离异常数组元素，同时保留其它有效消息段。
                try
                {
                    var segment = MessageSegment.Parse(TolerantJson.Item(arrayValue, index));
                    if (segment != null)
                    {
                        segments.Add(segment);
                    }
                }
                catch (Exception)
                {
                    // Continue parsing the remaining message segments.
                    // 继续解析其余消息段。
                }
            }

            return new OneBot11Message(
                OneBot11MessageKind.SegmentArray,
                null,
                segments.AsReadOnly(),
                TolerantJson.Clone(node));
        }

        var text = TolerantJson.String(node);
        return text == null
            ? null
            : new OneBot11Message(
                OneBot11MessageKind.String,
                text,
                Array.Empty<MessageSegment>(),
                TolerantJson.Clone(node));
    }

    /// <summary>
    /// Creates an independent JSON value in the same representation as this message.
    /// 使用与当前消息相同的表示形式创建独立 JSON 值。
    /// </summary>
    public JsonNode? ToJsonNode()
    {
        switch (Kind)
        {
            case OneBot11MessageKind.String:
                return JsonValue.Create(StringValue);
            case OneBot11MessageKind.Segment:
                return _segments.Count == 0 ? null : _segments[0].ToJsonObject();
            case OneBot11MessageKind.SegmentArray:
                var array = new JsonArray();
                foreach (var segment in _segments)
                {
                    array.Add(segment.ToJsonObject());
                }

                return array;
            default:
                return TolerantJson.Clone(RawJson);
        }
    }

    /// <summary>
    /// Converts this message to the OneBot CQ-code string representation.
    /// 将当前消息转换为 OneBot CQ 码字符串表示。
    /// </summary>
    public string ToCqCode()
    {
        return CqCodeCodec.Encode(this);
    }

    /// <summary>
    /// Converts a string directly into a OneBot message.
    /// 将字符串直接转换为 OneBot 消息。
    /// </summary>
    public static implicit operator OneBot11Message(string value)
    {
        return FromString(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToCqCode();
    }
}

/// <summary>
/// Serializes and deserializes the three JSON shapes of <see cref="OneBot11Message"/>.
/// 序列化和反序列化 <see cref="OneBot11Message"/> 的三种 JSON 形态。
/// </summary>
public sealed class OneBot11MessageJsonConverter : JsonConverter<OneBot11Message>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override OneBot11Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        try
        {
            return OneBot11Message.Parse(JsonNode.Parse(ref reader));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneBot11Message value, JsonSerializerOptions options)
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
