using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing video file reference. / 表示出站视频文件引用。</summary>
public sealed class OneBot12VideoSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing video reference. / 初始化出站视频引用。</summary>
    public OneBot12VideoSendSegment(string fileId) : base(OneBot12MessageSegmentTypes.Video)
    {
        FileId = Require(fileId, nameof(fileId));
    }

    /// <summary>Gets the uploaded file ID. / 获取已上传文件 ID。</summary>
    public string FileId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["file_id"] = FileId };
}
