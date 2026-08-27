using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>Represents a group file upload notice. / 表示群文件上传通知。</summary>
public sealed class GroupUploadNoticeEvent : OneBot10NoticeEvent
{
    internal GroupUploadNoticeEvent()
    {
    }

    /// <summary>Gets the group identifier. / 获取群号。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; internal set; }

    /// <summary>Gets the uploader QQ identifier. / 获取上传者 QQ 号。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; internal set; }

    /// <summary>Gets the independently parsed file metadata. / 获取独立解析的文件元数据。</summary>
    [JsonPropertyName("file")]
    public GroupUploadFileInfo? File { get; internal set; }
}
