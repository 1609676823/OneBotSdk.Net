using System;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Identifies the standard target type accepted by <c>send_msg</c>.
/// 标识 <c>send_msg</c> 接受的标准目标类型。
/// </summary>
public enum OneBot10MessageType
{
    /// <summary>A private message target. / 私聊消息目标。</summary>
    Private,
    /// <summary>A group message target. / 群消息目标。</summary>
    Group,
    /// <summary>A discussion-group message target. / 讨论组消息目标。</summary>
    Discuss
}

/// <summary>
/// Identifies a CQHTTP data directory accepted by <c>clean_data_dir</c>.
/// 标识 <c>clean_data_dir</c> 接受的 CQHTTP 数据目录。
/// </summary>
public enum OneBot10DataDirectory
{
    /// <summary>The received-image directory. / 已接收图片目录。</summary>
    Image,

    /// <summary>The received-audio directory. / 已接收语音目录。</summary>
    Record,

    /// <summary>The show directory. / 演示数据目录。</summary>
    Show,

    /// <summary>The BFace directory. / BFace 数据目录。</summary>
    Bface
}

/// <summary>
/// Identifies the standard group request subtype.
/// 标识标准加群请求子类型。
/// </summary>
public enum OneBot10GroupRequestType
{
    /// <summary>A user requests to join a group. / 用户申请加入群。</summary>
    Add,
    /// <summary>The bot account is invited to a group. / 机器人账号被邀请入群。</summary>
    Invite
}

/// <summary>
/// Identifies a standard output format accepted by <c>get_record</c>.
/// 标识 <c>get_record</c> 接受的标准输出格式。
/// </summary>
public enum OneBot10RecordFormat
{
    /// <summary>MP3 audio. / MP3 音频。</summary>
    Mp3,
    /// <summary>AMR audio. / AMR 音频。</summary>
    Amr,
    /// <summary>WMA audio. / WMA 音频。</summary>
    Wma,
    /// <summary>M4A audio. / M4A 音频。</summary>
    M4a,
    /// <summary>Speex audio. / Speex 音频。</summary>
    Spx,
    /// <summary>Ogg audio. / Ogg 音频。</summary>
    Ogg,
    /// <summary>WAV audio. / WAV 音频。</summary>
    Wav,
    /// <summary>FLAC audio. / FLAC 音频。</summary>
    Flac
}

internal static class OneBot10ActionEnumValues
{
    internal static string ToProtocolValue(this OneBot10MessageType value)
    {
        switch (value)
        {
            case OneBot10MessageType.Private: return "private";
            case OneBot10MessageType.Group: return "group";
            case OneBot10MessageType.Discuss: return "discuss";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown message type.");
        }
    }

    internal static string ToProtocolValue(this OneBot10DataDirectory value)
    {
        switch (value)
        {
            case OneBot10DataDirectory.Image: return "image";
            case OneBot10DataDirectory.Record: return "record";
            case OneBot10DataDirectory.Show: return "show";
            case OneBot10DataDirectory.Bface: return "bface";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown data directory.");
        }
    }

    internal static string ToProtocolValue(this OneBot10GroupRequestType value)
    {
        switch (value)
        {
            case OneBot10GroupRequestType.Add: return "add";
            case OneBot10GroupRequestType.Invite: return "invite";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown group request type.");
        }
    }

    internal static string ToProtocolValue(this OneBot10RecordFormat value)
    {
        switch (value)
        {
            case OneBot10RecordFormat.Mp3: return "mp3";
            case OneBot10RecordFormat.Amr: return "amr";
            case OneBot10RecordFormat.Wma: return "wma";
            case OneBot10RecordFormat.M4a: return "m4a";
            case OneBot10RecordFormat.Spx: return "spx";
            case OneBot10RecordFormat.Ogg: return "ogg";
            case OneBot10RecordFormat.Wav: return "wav";
            case OneBot10RecordFormat.Flac: return "flac";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown record format.");
        }
    }
}
