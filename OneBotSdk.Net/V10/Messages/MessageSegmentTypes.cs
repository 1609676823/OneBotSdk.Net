using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>Lists the message-segment type names defined by OneBot 10. / 列出 OneBot 10 定义的消息段类型名称。</summary>
public static class MessageSegmentTypes
{
    /// <summary>The text segment type. / 纯文本消息段类型。</summary>
    public const string Text = "text";
    /// <summary>The QQ face segment type. / QQ 表情消息段类型。</summary>
    public const string Face = "face";
    /// <summary>The image segment type. / 图片消息段类型。</summary>
    public const string Image = "image";
    /// <summary>The voice-record segment type. / 语音消息段类型。</summary>
    public const string Record = "record";
    /// <summary>The at-mention segment type. / @ 消息段类型。</summary>
    public const string At = "at";
    /// <summary>The rock-paper-scissors segment type. / 猜拳消息段类型。</summary>
    public const string Rps = "rps";
    /// <summary>The dice segment type. / 掷骰子消息段类型。</summary>
    public const string Dice = "dice";
    /// <summary>The window-shake segment type. / 窗口抖动消息段类型。</summary>
    public const string Shake = "shake";
    /// <summary>The anonymous-marker segment type. / 匿名标记消息段类型。</summary>
    public const string Anonymous = "anonymous";
    /// <summary>The link-share segment type. / 链接分享消息段类型。</summary>
    public const string Share = "share";
    /// <summary>The recommended-contact segment type. / 推荐联系人消息段类型。</summary>
    public const string Contact = "contact";
    /// <summary>The location segment type. / 位置消息段类型。</summary>
    public const string Location = "location";
    /// <summary>The music-share segment type. / 音乐分享消息段类型。</summary>
    public const string Music = "music";
    /// <summary>The receive-only rich-content segment type. / 仅接收的富文本消息段类型。</summary>
    public const string Rich = "rich";

    /// <summary>Gets the 14 standard OneBot 10 wire type names. / 获取 OneBot 10 的 14 个标准线协议类型名。</summary>
    public static IReadOnlyList<string> Standard { get; } = Array.AsReadOnly(new[]
    {
        Text, Face, Image, Record, At, Rps, Dice, Shake, Anonymous, Share, Contact, Location, Music, Rich
    });
}

/// <summary>Identifies every standard segment kind while retaining an unknown fallback. / 标识每种标准消息段，同时保留未知回退值。</summary>
public enum OneBot10MessageSegmentKind
{
    /// <summary>An extension or malformed segment type. / 扩展或格式异常的消息段类型。</summary>
    Unknown,
    /// <summary>Plain text. / 纯文本。</summary>
    Text,
    /// <summary>QQ face. / QQ 表情。</summary>
    Face,
    /// <summary>Image. / 图片。</summary>
    Image,
    /// <summary>Voice record. / 语音。</summary>
    Record,
    /// <summary>At mention. / @ 提及。</summary>
    At,
    /// <summary>Rock-paper-scissors magic face. / 猜拳魔法表情。</summary>
    Rps,
    /// <summary>Dice magic face. / 骰子魔法表情。</summary>
    Dice,
    /// <summary>Window shake. / 窗口抖动。</summary>
    Shake,
    /// <summary>Anonymous marker. / 匿名标记。</summary>
    Anonymous,
    /// <summary>Link share. / 链接分享。</summary>
    Share,
    /// <summary>Recommended contact. / 推荐联系人。</summary>
    Contact,
    /// <summary>Location. / 位置。</summary>
    Location,
    /// <summary>Music share. / 音乐分享。</summary>
    Music,
    /// <summary>Arbitrary receive-only rich content. / 任意仅接收富文本内容。</summary>
    Rich
}

