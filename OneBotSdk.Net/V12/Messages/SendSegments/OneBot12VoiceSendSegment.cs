using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents an outgoing recorded-voice file reference. / 表示出站录制语音文件引用。</summary>
public sealed class OneBot12VoiceSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes an outgoing recorded-voice reference. / 初始化出站录制语音引用。</summary>
    public OneBot12VoiceSendSegment(string fileId) : base(OneBot12MessageSegmentTypes.Voice)
    {
        FileId = Require(fileId, nameof(fileId));
    }

    /// <summary>Gets the uploaded file ID. / 获取已上传文件 ID。</summary>
    public string FileId { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["file_id"] = FileId };
}
