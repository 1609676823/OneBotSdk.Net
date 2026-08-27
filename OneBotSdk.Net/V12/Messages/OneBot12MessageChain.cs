using System;
using System.Collections;
using System.Collections.Generic;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Provides a concise fluent builder for an array-format outgoing message. / 为数组格式出站消息提供简洁的流式构建器。</summary>
public sealed class OneBot12MessageChain : IReadOnlyList<OneBot12SendSegment>
{
    private readonly List<OneBot12SendSegment> _segments = new List<OneBot12SendSegment>();

    /// <summary>Gets the number of ordered segments. / 获取有序消息段数量。</summary>
    public int Count => _segments.Count;
    /// <summary>Gets a segment by position. / 按位置获取消息段。</summary>
    public OneBot12SendSegment this[int index] => _segments[index];

    /// <summary>Adds a standard or extension segment. / 添加标准或扩展消息段。</summary>
    public OneBot12MessageChain Add(OneBot12SendSegment segment)
    {
        _segments.Add(segment ?? throw new ArgumentNullException(nameof(segment)));
        return this;
    }

    /// <summary>Adds plain text. / 添加纯文本。</summary>
    public OneBot12MessageChain Text(string text) => Add(new OneBot12TextSendSegment(text));
    /// <summary>Adds a user mention. / 添加用户提及。</summary>
    public OneBot12MessageChain Mention(string userId) => Add(new OneBot12MentionSendSegment(userId));
    /// <summary>Adds a mention-all segment. / 添加提及全体消息段。</summary>
    public OneBot12MessageChain MentionAll() => Add(new OneBot12MentionAllSendSegment());
    /// <summary>Adds an image file reference. / 添加图片文件引用。</summary>
    public OneBot12MessageChain Image(string fileId) => Add(new OneBot12ImageSendSegment(fileId));
    /// <summary>Adds a recorded-voice reference. / 添加录制语音引用。</summary>
    public OneBot12MessageChain Voice(string fileId) => Add(new OneBot12VoiceSendSegment(fileId));
    /// <summary>Adds an audio reference. / 添加音频引用。</summary>
    public OneBot12MessageChain Audio(string fileId) => Add(new OneBot12AudioSendSegment(fileId));
    /// <summary>Adds a video reference. / 添加视频引用。</summary>
    public OneBot12MessageChain Video(string fileId) => Add(new OneBot12VideoSendSegment(fileId));
    /// <summary>Adds a generic file reference. / 添加通用文件引用。</summary>
    public OneBot12MessageChain File(string fileId) => Add(new OneBot12FileSendSegment(fileId));
    /// <summary>Adds a geographical location. / 添加地理位置。</summary>
    public OneBot12MessageChain Location(double latitude, double longitude, string title, string content) => Add(new OneBot12LocationSendSegment(latitude, longitude, title, content));
    /// <summary>Adds a reply reference. / 添加回复引用。</summary>
    public OneBot12MessageChain Reply(string messageId, string? userId = null) => Add(new OneBot12ReplySendSegment(messageId, userId));

    /// <summary>Builds an independent array-format message. / 构建独立的数组格式消息。</summary>
    public OneBot12SendMessage ToMessage() => new OneBot12SendMessage(_segments);
    /// <summary>Converts a chain into the action message type. / 将消息链转换为动作消息类型。</summary>
    public static implicit operator OneBot12SendMessage(OneBot12MessageChain chain) =>
        chain?.ToMessage() ?? throw new ArgumentNullException(nameof(chain));

    /// <inheritdoc />
    public IEnumerator<OneBot12SendSegment> GetEnumerator() => _segments.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
