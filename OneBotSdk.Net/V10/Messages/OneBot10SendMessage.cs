using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Identifies an outgoing message JSON shape accepted by OneBot APIs. / 标识 OneBot API 接受的出站消息 JSON 形态。</summary>
public enum OneBot10SendMessageKind
{
    /// <summary>A CQ-code string. / CQ 码字符串。</summary>
    String,
    /// <summary>A single message-segment object. / 单个消息段对象。</summary>
    Segment,
    /// <summary>An ordered message-segment array. / 有序消息段数组。</summary>
    SegmentArray
}

/// <summary>
/// Represents an outgoing OneBot 10 message and acts as a strongly typed message-chain builder.
/// 表示出站 OneBot 10 消息，并作为强类型消息链构建器使用。
/// </summary>
[JsonConverter(typeof(OneBot10SendMessageJsonConverter))]
public sealed class OneBot10SendMessage : IReadOnlyList<OneBot10SendSegment>
{
    private readonly List<OneBot10SendSegment> _segments;

    /// <summary>Initializes an empty array-format message suitable for collection initializers. / 初始化适合集合初始化器的空数组格式消息。</summary>
    public OneBot10SendMessage()
        : this(OneBot10SendMessageKind.SegmentArray, null, new List<OneBot10SendSegment>())
    {
    }

    /// <summary>Initializes an array-format message from ordered outgoing segments. / 通过有序出站消息段初始化数组格式消息。</summary>
    public OneBot10SendMessage(IEnumerable<OneBot10SendSegment> segments)
        : this()
    {
        AddRange(segments);
    }

    private OneBot10SendMessage(
        OneBot10SendMessageKind kind,
        string? stringValue,
        List<OneBot10SendSegment> segments)
    {
        Kind = kind;
        StringValue = stringValue;
        _segments = segments;
    }

    /// <summary>Gets the selected outgoing wire shape. / 获取所选出站线协议形态。</summary>
    public OneBot10SendMessageKind Kind { get; }

    /// <summary>Gets the CQ-code value for a string-format message. / 获取字符串格式消息的 CQ 码值。</summary>
    public string? StringValue { get; }

    /// <inheritdoc />
    public int Count => _segments.Count;

    /// <inheritdoc />
    public OneBot10SendSegment this[int index] => _segments[index];

    /// <summary>Creates a message that preserves a CQ-code string. / 创建保留 CQ 码字符串的消息。</summary>
    public static OneBot10SendMessage FromString(string value)
    {
        return new OneBot10SendMessage(
            OneBot10SendMessageKind.String,
            value ?? throw new ArgumentNullException(nameof(value)),
            new List<OneBot10SendSegment>());
    }

    /// <summary>Creates the single-segment shape accepted by OneBot API parameters. / 创建 OneBot API 参数接受的单消息段形态。</summary>
    public static OneBot10SendMessage FromSegment(OneBot10SendSegment segment)
    {
        if (segment == null)
        {
            throw new ArgumentNullException(nameof(segment));
        }

        return new OneBot10SendMessage(
            OneBot10SendMessageKind.Segment,
            null,
            new List<OneBot10SendSegment> { segment });
    }

    /// <summary>Creates an array-format message from ordered outgoing segments. / 通过有序出站消息段创建数组格式消息。</summary>
    public static OneBot10SendMessage FromSegments(IEnumerable<OneBot10SendSegment> segments)
    {
        return new OneBot10SendMessage(segments);
    }

    /// <summary>Creates an array-format message from supplied outgoing segments. / 通过给定出站消息段创建数组格式消息。</summary>
    public static OneBot10SendMessage FromSegments(params OneBot10SendSegment[] segments)
    {
        return FromSegments((IEnumerable<OneBot10SendSegment>)segments);
    }

    /// <summary>Adds an outgoing segment to an array-format message. / 向数组格式消息添加出站消息段。</summary>
    public void Add(OneBot10SendSegment segment)
    {
        EnsureArrayBuilder();
        _segments.Add(segment ?? throw new ArgumentNullException(nameof(segment)));
    }

    /// <summary>Adds ordered outgoing segments and returns this message. / 添加有序出站消息段并返回当前消息。</summary>
    public OneBot10SendMessage AddRange(IEnumerable<OneBot10SendSegment> segments)
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
    public OneBot10SendMessage Text(string text)
    {
        Add(new TextSendSegment(text));
        return this;
    }

    /// <summary>Adds a QQ face. / 添加 QQ 表情。</summary>
    public OneBot10SendMessage Face(string id)
    {
        Add(new FaceSendSegment(id));
        return this;
    }

    /// <summary>Adds an at-mention by QQ ID. / 通过 QQ 号添加 @ 消息。</summary>
    public OneBot10SendMessage At(long userId)
    {
        Add(new AtSendSegment(userId));
        return this;
    }

    /// <summary>Adds an at-mention by protocol target. / 通过协议目标添加 @ 消息。</summary>
    public OneBot10SendMessage At(string target)
    {
        Add(new AtSendSegment(target));
        return this;
    }

    /// <summary>Adds an at-all segment. / 添加 @ 全体成员消息段。</summary>
    public OneBot10SendMessage AtAll()
    {
        return At("all");
    }

    /// <summary>Adds an image with outgoing-only download options. / 添加包含发送专用下载选项的图片。</summary>
    public OneBot10SendMessage Image(
        string file,
        bool? cache = null,
        long? timeoutSeconds = null)
    {
        Add(new ImageSendSegment(file, cache, timeoutSeconds));
        return this;
    }

    /// <summary>Adds a voice record with outgoing-only download options. / 添加包含发送专用下载选项的语音。</summary>
    public OneBot10SendMessage Record(
        string file,
        bool? magic = null,
        bool? cache = null,
        long? timeoutSeconds = null)
    {
        Add(new RecordSendSegment(file, magic, cache, timeoutSeconds));
        return this;
    }

    /// <summary>Adds a rock-paper-scissors magic face. / 添加猜拳魔法表情。</summary>
    public OneBot10SendMessage Rps()
    {
        Add(new RpsSendSegment());
        return this;
    }

    /// <summary>Adds a dice magic face. / 添加掷骰子魔法表情。</summary>
    public OneBot10SendMessage Dice()
    {
        Add(new DiceSendSegment());
        return this;
    }

    /// <summary>Adds a window-shake segment. / 添加窗口抖动消息段。</summary>
    public OneBot10SendMessage Shake()
    {
        Add(new ShakeSendSegment());
        return this;
    }

    /// <summary>Adds the send-only anonymous marker. / 添加仅发送的匿名标记。</summary>
    public OneBot10SendMessage Anonymous(bool? ignoreFailure = null)
    {
        Add(new AnonymousSendSegment(ignoreFailure));
        return this;
    }

    /// <summary>Adds a link share. / 添加链接分享。</summary>
    public OneBot10SendMessage Share(
        string url,
        string title,
        string? content = null,
        string? image = null)
    {
        Add(new ShareSendSegment(url, title, content, image));
        return this;
    }

    /// <summary>Adds a recommended QQ friend. / 添加推荐 QQ 好友。</summary>
    public OneBot10SendMessage ContactFriend(string userId)
    {
        Add(new ContactSendSegment(OneBot10ContactTarget.Friend, userId));
        return this;
    }

    /// <summary>Adds a recommended QQ group. / 添加推荐 QQ 群。</summary>
    public OneBot10SendMessage ContactGroup(string groupId)
    {
        Add(new ContactSendSegment(OneBot10ContactTarget.Group, groupId));
        return this;
    }

    /// <summary>Adds a location share. / 添加位置分享。</summary>
    public OneBot10SendMessage Location(
        string latitude,
        string longitude,
        string? title = null,
        string? content = null)
    {
        Add(new LocationSendSegment(latitude, longitude, title, content));
        return this;
    }

    /// <summary>Adds a provider-backed music share. / 添加平台音乐分享。</summary>
    public OneBot10SendMessage Music(OneBot10MusicProvider provider, string id)
    {
        Add(new MusicSendSegment(provider, id));
        return this;
    }

    /// <summary>Adds a custom music share. / 添加自定义音乐分享。</summary>
    public OneBot10SendMessage CustomMusic(
        string url,
        string audio,
        string title,
        string? content = null,
        string? image = null)
    {
        Add(new CustomMusicSendSegment(url, audio, title, content, image));
        return this;
    }

    /// <summary>Creates an independent JSON value for an API parameter. / 为 API 参数创建独立 JSON 值。</summary>
    public JsonNode? ToJsonNode()
    {
        switch (Kind)
        {
            case OneBot10SendMessageKind.String:
                return JsonValue.Create(StringValue);
            case OneBot10SendMessageKind.Segment:
                return _segments.Count == 0 ? null : _segments[0].ToJsonObject();
            case OneBot10SendMessageKind.SegmentArray:
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
        return CqCodeCodec.Encode(this);
    }

    /// <inheritdoc />
    public IEnumerator<OneBot10SendSegment> GetEnumerator() => _segments.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public override string ToString() => ToCqCode();

    private void EnsureArrayBuilder()
    {
        if (Kind != OneBot10SendMessageKind.SegmentArray)
        {
            throw new InvalidOperationException("Segments can only be added to an array-format outgoing message.");
        }
    }
}

/// <summary>Writes outgoing messages and deliberately does not parse received payloads. / 写入出站消息，并且有意不解析接收负载。</summary>
public sealed class OneBot10SendMessageJsonConverter : JsonConverter<OneBot10SendMessage>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override OneBot10SendMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException("Use OneBot10ReceivedMessage to parse an incoming message payload.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneBot10SendMessage value, JsonSerializerOptions options)
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
