namespace OneBotSdk.Net.V12.Messages;

/// <summary>Identifies a known standard received segment without discarding its raw type. / 标识已知标准接收消息段，同时不丢弃原始类型。</summary>
public enum OneBot12MessageSegmentKind
{
    /// <summary>An unknown or implementation-defined segment. / 未知或实现扩展消息段。</summary>
    Unknown,
    /// <summary>Plain text. / 纯文本。</summary>
    Text,
    /// <summary>A user mention. / 用户提及。</summary>
    Mention,
    /// <summary>A mention of all users. / 提及全体用户。</summary>
    MentionAll,
    /// <summary>An image. / 图片。</summary>
    Image,
    /// <summary>A recorded voice. / 录制语音。</summary>
    Voice,
    /// <summary>An audio file. / 音频文件。</summary>
    Audio,
    /// <summary>A video. / 视频。</summary>
    Video,
    /// <summary>A generic file. / 通用文件。</summary>
    File,
    /// <summary>A geographical location. / 地理位置。</summary>
    Location,
    /// <summary>A reply reference. / 回复引用。</summary>
    Reply
}
