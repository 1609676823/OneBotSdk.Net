using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing audio file reference. / 表示出站音频文件引用。</summary>
public sealed class OneBot12AudioSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing audio reference. / 初始化出站音频引用。</summary>
    public OneBot12AudioSendSegment(string fileId) : base(OneBot12MessageSegmentTypes.Audio)
    {
        FileId = Require(fileId, nameof(fileId));
    }

    /// <summary>Gets the uploaded file ID. / 获取已上传文件 ID。</summary>
    public string FileId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["file_id"] = FileId };
}
