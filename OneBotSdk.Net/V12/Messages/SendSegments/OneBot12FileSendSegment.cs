using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing generic file reference. / 表示出站通用文件引用。</summary>
public sealed class OneBot12FileSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing generic-file reference. / 初始化出站通用文件引用。</summary>
    public OneBot12FileSendSegment(string fileId) : base(OneBot12MessageSegmentTypes.File)
    {
        FileId = Require(fileId, nameof(fileId));
    }

    /// <summary>Gets the uploaded file ID. / 获取已上传文件 ID。</summary>
    public string FileId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["file_id"] = FileId };
}
