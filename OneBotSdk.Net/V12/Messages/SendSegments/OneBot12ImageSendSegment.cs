using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing image file reference. / 表示出站图片文件引用。</summary>
public sealed class OneBot12ImageSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing image reference. / 初始化出站图片引用。</summary>
    public OneBot12ImageSendSegment(string fileId) : base(OneBot12MessageSegmentTypes.Image)
    {
        FileId = Require(fileId, nameof(fileId));
    }

    /// <summary>Gets the uploaded file ID. / 获取已上传文件 ID。</summary>
    public string FileId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["file_id"] = FileId };
}
