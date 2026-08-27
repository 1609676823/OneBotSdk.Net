using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>
/// Represents an easy-to-build and easy-to-enumerate ordered OneBot 11 message chain.
/// 表示便于构建和遍历的有序 OneBot 11 消息链。
/// </summary>
public sealed class OneBot11MessageChain : IReadOnlyList<MessageSegment>
{
    private readonly List<MessageSegment> _segments;

    /// <summary>Initializes an empty message chain. / 初始化空消息链。</summary>
    public OneBot11MessageChain()
    {
        _segments = new List<MessageSegment>();
    }

    /// <summary>Initializes a message chain by deeply copying an ordered segment sequence. / 通过深拷贝有序消息段序列初始化消息链。</summary>
    public OneBot11MessageChain(IEnumerable<MessageSegment> segments)
        : this()
    {
        AddRange(segments);
    }

    /// <summary>Gets the number of message segments. / 获取消息段数量。</summary>
    public int Count => _segments.Count;

    /// <summary>Gets the segment at an ordered position. / 获取指定顺序位置的消息段。</summary>
    public MessageSegment this[int index] => _segments[index];

    /// <summary>
    /// Gets the concatenated content of all text segments while ignoring non-text segments.
    /// 获取所有纯文本消息段的连接内容，并忽略非文本段。
    /// </summary>
    public string PlainText
    {
        get
        {
            var result = new StringBuilder();
            foreach (var segment in _segments)
            {
                if (segment.Kind == OneBot11MessageSegmentKind.Text)
                {
                    result.Append(segment.GetString("text"));
                }
            }

            return result.ToString();
        }
    }

    /// <summary>Adds an independent copy of a standard or extension segment. / 添加标准或扩展消息段的独立副本。</summary>
    public void Add(MessageSegment segment)
    {
        if (segment == null)
        {
            throw new ArgumentNullException(nameof(segment));
        }

        _segments.Add(segment.Clone());
    }

    /// <summary>Adds all supplied segments and returns this chain. / 添加全部给定消息段并返回当前消息链。</summary>
    public OneBot11MessageChain AddRange(IEnumerable<MessageSegment> segments)
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

    /// <summary>Adds a text segment. / 添加纯文本消息段。</summary>
    public OneBot11MessageChain Text(string text)
    {
        Add(MessageSegment.Text(text));
        return this;
    }

    /// <summary>Adds an at-mention by QQ identifier. / 通过 QQ 号添加 @ 消息段。</summary>
    public OneBot11MessageChain At(long userId)
    {
        return At(userId.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Adds an at-mention by protocol identifier. / 通过协议标识添加 @ 消息段。</summary>
    public OneBot11MessageChain At(string userId)
    {
        Add(MessageSegment.At(userId));
        return this;
    }

    /// <summary>Adds an at-all segment. / 添加 @ 全体成员消息段。</summary>
    public OneBot11MessageChain AtAll()
    {
        return At("all");
    }

    /// <summary>Adds a QQ face. / 添加 QQ 表情消息段。</summary>
    public OneBot11MessageChain Face(string id)
    {
        Add(MessageSegment.Face(id));
        return this;
    }

    /// <summary>Adds an image from a received file, URI, URL, or base64 URI. / 通过已接收文件、URI、URL 或 base64 URI 添加图片。</summary>
    public OneBot11MessageChain Image(string file, bool flash = false)
    {
        Add(MessageSegment.Image(file, flash));
        return this;
    }

    /// <summary>Adds a voice record. / 添加语音消息段。</summary>
    public OneBot11MessageChain Record(string file)
    {
        Add(MessageSegment.Record(file));
        return this;
    }

    /// <summary>Adds a short video. / 添加短视频消息段。</summary>
    public OneBot11MessageChain Video(string file)
    {
        Add(MessageSegment.Video(file));
        return this;
    }

    /// <summary>Adds a reply segment by message identifier. / 通过消息 ID 添加回复消息段。</summary>
    public OneBot11MessageChain Reply(long messageId)
    {
        return Reply(messageId.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Adds a reply segment by protocol identifier. / 通过协议标识添加回复消息段。</summary>
    public OneBot11MessageChain Reply(string messageId)
    {
        Add(MessageSegment.Reply(messageId));
        return this;
    }

    /// <summary>Adds an XML rich-message segment. / 添加 XML 富消息段。</summary>
    public OneBot11MessageChain Xml(string xml)
    {
        Add(MessageSegment.Xml(xml));
        return this;
    }

    /// <summary>Adds a JSON rich-message segment. / 添加 JSON 富消息段。</summary>
    public OneBot11MessageChain Json(string json)
    {
        Add(MessageSegment.Json(json));
        return this;
    }

    /// <summary>Returns segments whose discriminator exactly matches the requested type. / 返回判别值与请求类型完全匹配的消息段。</summary>
    public IEnumerable<MessageSegment> GetSegments(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("A message segment type is required.", nameof(type));
        }

        foreach (var segment in _segments)
        {
            if (string.Equals(segment.Type, type, StringComparison.Ordinal))
            {
                yield return segment;
            }
        }
    }

    /// <summary>Returns the first matching segment, or null. / 返回第一个匹配消息段；不存在时返回 null。</summary>
    public MessageSegment? FirstOrDefault(string type)
    {
        foreach (var segment in GetSegments(type))
        {
            return segment;
        }

        return null;
    }

    /// <summary>Builds an array-format protocol message. / 构建数组格式的协议消息。</summary>
    public OneBot11Message ToMessage()
    {
        var snapshot = new List<MessageSegment>(_segments.Count);
        foreach (var segment in _segments)
        {
            snapshot.Add(segment.Clone());
        }

        return OneBot11Message.FromSegments(snapshot);
    }

    /// <summary>Creates a logical message chain from any supported wire representation. / 从任意受支持的线协议表示创建逻辑消息链。</summary>
    public static OneBot11MessageChain FromMessage(OneBot11Message? message)
    {
        if (message == null)
        {
            return new OneBot11MessageChain();
        }

        // CQ strings are decoded and every segment is deeply copied, giving both wire shapes identical ownership.
        // 解码 CQ 字符串并深拷贝每个消息段，使两种线协议形式具有一致的所有权语义。
        var segments = message.Kind == OneBot11MessageKind.String
            ? CqCodeCodec.Decode(message.StringValue ?? string.Empty).Segments
            : message.Segments;
        return new OneBot11MessageChain(segments);
    }

    /// <summary>Converts a chain directly to the message type accepted by action APIs. / 将消息链直接转换为动作 API 接受的消息类型。</summary>
    public static implicit operator OneBot11Message(OneBot11MessageChain chain)
    {
        if (chain == null)
        {
            throw new ArgumentNullException(nameof(chain));
        }

        return chain.ToMessage();
    }

    /// <inheritdoc />
    public IEnumerator<MessageSegment> GetEnumerator()
    {
        return _segments.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
