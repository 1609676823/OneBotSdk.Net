using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>
/// Represents a received OneBot 12 message, whose standard wire shape is always a segment array.
/// 表示接收的 OneBot 12 消息；其标准线协议形态始终为消息段数组。
/// </summary>
[JsonConverter(typeof(OneBot12ReceivedMessageJsonConverter))]
public sealed class OneBot12ReceivedMessage : IReadOnlyList<OneBot12ReceivedSegment>
{
    private readonly IReadOnlyList<OneBot12ReceivedSegment> _segments;

    private OneBot12ReceivedMessage(IReadOnlyList<OneBot12ReceivedSegment> segments, JsonArray rawJson)
    {
        _segments = segments;
        RawJson = rawJson;
    }

    /// <summary>Gets a new empty received message. / 获取一个新的空接收消息。</summary>
    public static OneBot12ReceivedMessage Empty =>
        new OneBot12ReceivedMessage(Array.Empty<OneBot12ReceivedSegment>(), new JsonArray());

    /// <summary>Gets the detached original segment array. / 获取独立的原始消息段数组。</summary>
    [JsonIgnore]
    public JsonArray RawJson { get; }

    /// <inheritdoc />
    public int Count => _segments.Count;
    /// <summary>Gets all parsed segments in wire order. / 按线协议顺序获取全部已解析消息段。</summary>
    public IReadOnlyList<OneBot12ReceivedSegment> Segments => _segments;
    /// <inheritdoc />
    public OneBot12ReceivedSegment this[int index] => _segments[index];

    /// <summary>Gets concatenated text without flattening non-text segments. / 获取连接后的纯文本，并且不展平非文本消息段。</summary>
    public string PlainText
    {
        get
        {
            var result = new StringBuilder();
            foreach (var segment in _segments)
            {
                if (segment is OneBot12TextReceivedSegment text)
                {
                    result.Append(text.Text);
                }
            }

            return result.ToString();
        }
    }

    /// <summary>Parses only the array shape required for received OneBot 12 messages. / 只解析 OneBot 12 接收消息所要求的数组形态。</summary>
    public static OneBot12ReceivedMessage? Parse(JsonNode? node)
    {
        var array = TolerantJson.Array(node);
        if (array == null)
        {
            return null;
        }

        var segments = new List<OneBot12ReceivedSegment>();
        for (var index = 0; index < TolerantJson.Count(array); index++)
        {
            try
            {
                // Isolate malformed siblings while retaining unknown object segments.
                // 隔离异常同级元素，同时保留未知对象消息段。
                var segment = OneBot12ReceivedSegment.Parse(TolerantJson.Item(array, index));
                if (segment != null)
                {
                    segments.Add(segment);
                }
            }
            catch (Exception)
            {
                // Continue parsing the remaining received segments.
                // 继续解析其余接收消息段。
            }
        }

        return new OneBot12ReceivedMessage(
            segments.AsReadOnly(),
            TolerantJson.Clone(array) as JsonArray ?? new JsonArray());
    }

    /// <summary>Gets received segments assignable to a concrete type. / 获取可赋值给指定具体类型的接收消息段。</summary>
    public IEnumerable<TSegment> GetSegments<TSegment>() where TSegment : OneBot12ReceivedSegment
    {
        foreach (var segment in _segments)
        {
            if (segment is TSegment typed)
            {
                yield return typed;
            }
        }
    }

    /// <summary>Gets the first segment assignable to a concrete type. / 获取首个可赋值给指定具体类型的消息段。</summary>
    public TSegment? FirstOrDefault<TSegment>() where TSegment : OneBot12ReceivedSegment
    {
        foreach (var segment in GetSegments<TSegment>())
        {
            return segment;
        }

        return null;
    }

    /// <summary>Creates a detached original wire array. / 创建原始线协议数组的独立副本。</summary>
    public JsonArray ToJsonArray() => TolerantJson.Clone(RawJson) as JsonArray ?? new JsonArray();
    /// <inheritdoc />
    public IEnumerator<OneBot12ReceivedSegment> GetEnumerator() => _segments.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Reads and writes received message arrays. / 读写接收消息数组。</summary>
public sealed class OneBot12ReceivedMessageJsonConverter : JsonConverter<OneBot12ReceivedMessage>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override OneBot12ReceivedMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var parsed = OneBot12ReceivedMessage.Parse(JsonNode.Parse(ref reader));
        return parsed ?? throw new JsonException("A received OneBot 12 message must be a segment array.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneBot12ReceivedMessage value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        value.ToJsonArray().WriteTo(writer, options);
    }
}
