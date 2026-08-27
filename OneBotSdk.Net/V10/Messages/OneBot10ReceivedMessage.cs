using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Represents an independently parsed, strongly typed incoming message chain.
/// 表示独立解析的强类型入站消息链。
/// </summary>
[JsonConverter(typeof(OneBot10ReceivedMessageJsonConverter))]
public sealed class OneBot10ReceivedMessage : IReadOnlyList<OneBot10ReceivedSegment>
{
    private readonly IReadOnlyList<OneBot10ReceivedSegment> _segments;

    private OneBot10ReceivedMessage(
        OneBot10ReceivedMessageKind kind,
        string? stringValue,
        IReadOnlyList<OneBot10ReceivedSegment> segments,
        JsonNode rawJson)
    {
        Kind = kind;
        StringValue = stringValue;
        _segments = segments;
        RawJson = rawJson;
    }

    /// <summary>
    /// Gets a new empty received message chain whose raw value is an empty segment array.
    /// 获取一个新的空入站消息链，其原始值为空消息段数组。
    /// </summary>
    public static OneBot10ReceivedMessage Empty => new OneBot10ReceivedMessage(
        OneBot10ReceivedMessageKind.SegmentArray,
        null,
        Array.Empty<OneBot10ReceivedSegment>(),
        new JsonArray());

    /// <summary>Gets the incoming wire shape. / 获取入站线协议形态。</summary>
    public OneBot10ReceivedMessageKind Kind { get; }

    /// <summary>Gets the original CQ-code string when the wire value was a string. / 在线协议值为字符串时获取原始 CQ 码字符串。</summary>
    public string? StringValue { get; }

    /// <summary>Gets an independent copy of the original JSON value. / 获取原始 JSON 值的独立副本。</summary>
    [JsonIgnore]
    public JsonNode RawJson { get; }

    /// <inheritdoc />
    public int Count => _segments.Count;

    /// <summary>Gets the ordered concrete received segments. / 获取有序的具体入站消息段。</summary>
    public IReadOnlyList<OneBot10ReceivedSegment> Segments => _segments;

    /// <inheritdoc />
    public OneBot10ReceivedSegment this[int index] => _segments[index];

    /// <summary>Gets text from all received text segments without flattening other segment types. / 获取所有收到的纯文本段内容，并且不展平其它消息段。</summary>
    public string PlainText
    {
        get
        {
            var result = new StringBuilder();
            foreach (var segment in _segments)
            {
                if (segment is TextReceivedSegment text)
                {
                    result.Append(text.Text);
                }
            }

            return result.ToString();
        }
    }

    /// <summary>
    /// Parses a string, segment array, or tolerated single segment without letting malformed siblings invalidate valid segments.
    /// 解析字符串、消息段数组或容错单消息段，并避免异常同级元素使有效消息段失效。
    /// </summary>
    public static OneBot10ReceivedMessage? Parse(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        if (node is JsonArray array)
        {
            return new OneBot10ReceivedMessage(
                OneBot10ReceivedMessageKind.SegmentArray,
                null,
                ParseSegments(array),
                TolerantJson.Clone(node) ?? new JsonArray());
        }

        if (node is JsonObject objectValue)
        {
            var segment = OneBot10ReceivedSegment.Parse(objectValue);
            return segment == null
                ? null
                : new OneBot10ReceivedMessage(
                    OneBot10ReceivedMessageKind.Segment,
                    null,
                    new[] { segment },
                    TolerantJson.Clone(node) ?? new JsonObject());
        }

        var text = TolerantJson.String(node);
        if (text == null)
        {
            return null;
        }

        return CqCodeCodec.Decode(text);
    }

    /// <summary>Returns received segments assignable to a concrete type. / 返回可赋值给具体类型的入站消息段。</summary>
    public IEnumerable<TSegment> GetSegments<TSegment>()
        where TSegment : OneBot10ReceivedSegment
    {
        foreach (var segment in _segments)
        {
            if (segment is TSegment concrete)
            {
                yield return concrete;
            }
        }
    }

    /// <summary>Returns the first received segment of a concrete type, or null. / 返回首个指定具体类型的入站消息段，不存在时返回 null。</summary>
    public TSegment? FirstOrDefault<TSegment>()
        where TSegment : OneBot10ReceivedSegment
    {
        foreach (var segment in GetSegments<TSegment>())
        {
            return segment;
        }

        return null;
    }

    /// <summary>Creates an independent JSON value in the original incoming shape. / 以原始入站形态创建独立 JSON 值。</summary>
    public JsonNode ToJsonNode()
    {
        return TolerantJson.Clone(RawJson) ?? JsonValue.Create(StringValue ?? string.Empty)!;
    }

    /// <inheritdoc />
    public IEnumerator<OneBot10ReceivedSegment> GetEnumerator() => _segments.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public override string ToString()
    {
        return StringValue ?? CqCodeCodec.EncodeReceived(_segments);
    }

    internal static OneBot10ReceivedMessage FromCqCode(
        string source,
        IReadOnlyList<OneBot10ReceivedSegment> segments)
    {
        return new OneBot10ReceivedMessage(
            OneBot10ReceivedMessageKind.String,
            source,
            segments,
            JsonValue.Create(source)!);
    }

    private static IReadOnlyList<OneBot10ReceivedSegment> ParseSegments(JsonArray array)
    {
        var segments = new List<OneBot10ReceivedSegment>();
        var count = TolerantJson.Count(array);
        for (var index = 0; index < count; index++)
        {
            // Invalid array items are isolated while valid and unknown object segments remain available.
            // 隔离无效数组元素，同时保留有效对象消息段和未知对象消息段。
            try
            {
                var segment = OneBot10ReceivedSegment.Parse(TolerantJson.Item(array, index));
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

        return segments.AsReadOnly();
    }
}
