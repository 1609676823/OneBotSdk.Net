namespace OneBotSdk.Net.V12.Messages;

/// <summary>Defines all standard OneBot 12 message-segment discriminators. / 定义全部 OneBot 12 标准消息段判别值。</summary>
public static class OneBot12MessageSegmentTypes
{
    /// <summary>Plain-text discriminator. / 纯文本判别值。</summary>
    public const string Text = "text";
    /// <summary>User-mention discriminator. / 用户提及判别值。</summary>
    public const string Mention = "mention";
    /// <summary>Mention-all discriminator. / 提及全体判别值。</summary>
    public const string MentionAll = "mention_all";
    /// <summary>Image discriminator. / 图片判别值。</summary>
    public const string Image = "image";
    /// <summary>Recorded-voice discriminator. / 录制语音判别值。</summary>
    public const string Voice = "voice";
    /// <summary>Audio discriminator. / 音频判别值。</summary>
    public const string Audio = "audio";
    /// <summary>Video discriminator. / 视频判别值。</summary>
    public const string Video = "video";
    /// <summary>Generic-file discriminator. / 通用文件判别值。</summary>
    public const string File = "file";
    /// <summary>Location discriminator. / 位置判别值。</summary>
    public const string Location = "location";
    /// <summary>Reply discriminator. / 回复判别值。</summary>
    public const string Reply = "reply";
}
