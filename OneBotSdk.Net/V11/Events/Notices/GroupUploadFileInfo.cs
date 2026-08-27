using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents file metadata in a group-upload notice. / 表示群文件上传通知中的文件元数据。</summary>
public sealed class GroupUploadFileInfo : OneBot11JsonModel
{
    internal GroupUploadFileInfo()
    {
    }

    /// <summary>Gets the opaque file identifier. / 获取不透明文件 ID。</summary>
    [JsonPropertyName("id")]
    public string? Id { get; internal set; }

    /// <summary>Gets the file name. / 获取文件名。</summary>
    [JsonPropertyName("name")]
    public string? Name { get; internal set; }

    /// <summary>Gets the file size in bytes. / 获取文件大小（字节）。</summary>
    [JsonPropertyName("size")]
    public long? Size { get; internal set; }

    /// <summary>Gets the implementation-provided bus ID. / 获取实现端提供的 busid。</summary>
    [JsonPropertyName("busid")]
    public long? BusId { get; internal set; }
}
