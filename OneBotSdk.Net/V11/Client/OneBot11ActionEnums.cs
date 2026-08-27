using System;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Identifies the standard target type accepted by <c>send_msg</c>.
/// 标识 <c>send_msg</c> 接受的标准目标类型。
/// </summary>
public enum OneBot11MessageType
{
    /// <summary>A private message target. / 私聊消息目标。</summary>
    Private,
    /// <summary>A group message target. / 群消息目标。</summary>
    Group
}

/// <summary>
/// Identifies the standard group request subtype.
/// 标识标准加群请求子类型。
/// </summary>
public enum OneBot11GroupRequestType
{
    /// <summary>A user requests to join a group. / 用户申请加入群。</summary>
    Add,
    /// <summary>The bot account is invited to a group. / 机器人账号被邀请入群。</summary>
    Invite
}

/// <summary>
/// Identifies a standard group honor query.
/// 标识标准群荣誉查询类型。
/// </summary>
public enum OneBot11GroupHonorType
{
    /// <summary>Current and historical talkative honors. / 当前及历史龙王荣誉。</summary>
    Talkative,
    /// <summary>Performer honors. / 群聊之火荣誉。</summary>
    Performer,
    /// <summary>Legend honors. / 群聊炽焰荣誉。</summary>
    Legend,
    /// <summary>Strong-newbie honors. / 冒尖小春笋荣誉。</summary>
    StrongNewbie,
    /// <summary>Emotion honors. / 快乐之源荣誉。</summary>
    Emotion,
    /// <summary>All standard honor categories. / 全部标准荣誉类别。</summary>
    All
}

/// <summary>
/// Identifies a standard output format accepted by <c>get_record</c>.
/// 标识 <c>get_record</c> 接受的标准输出格式。
/// </summary>
public enum OneBot11RecordFormat
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

internal static class OneBot11ActionEnumValues
{
    internal static string ToProtocolValue(this OneBot11MessageType value)
    {
        switch (value)
        {
            case OneBot11MessageType.Private: return "private";
            case OneBot11MessageType.Group: return "group";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown message type.");
        }
    }

    internal static string ToProtocolValue(this OneBot11GroupRequestType value)
    {
        switch (value)
        {
            case OneBot11GroupRequestType.Add: return "add";
            case OneBot11GroupRequestType.Invite: return "invite";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown group request type.");
        }
    }

    internal static string ToProtocolValue(this OneBot11GroupHonorType value)
    {
        switch (value)
        {
            case OneBot11GroupHonorType.Talkative: return "talkative";
            case OneBot11GroupHonorType.Performer: return "performer";
            case OneBot11GroupHonorType.Legend: return "legend";
            case OneBot11GroupHonorType.StrongNewbie: return "strong_newbie";
            case OneBot11GroupHonorType.Emotion: return "emotion";
            case OneBot11GroupHonorType.All: return "all";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown group honor type.");
        }
    }

    internal static string ToProtocolValue(this OneBot11RecordFormat value)
    {
        switch (value)
        {
            case OneBot11RecordFormat.Mp3: return "mp3";
            case OneBot11RecordFormat.Amr: return "amr";
            case OneBot11RecordFormat.Wma: return "wma";
            case OneBot11RecordFormat.M4a: return "m4a";
            case OneBot11RecordFormat.Spx: return "spx";
            case OneBot11RecordFormat.Ogg: return "ogg";
            case OneBot11RecordFormat.Wav: return "wav";
            case OneBot11RecordFormat.Flac: return "flac";
            default: throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown record format.");
        }
    }
}
