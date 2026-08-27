using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents a received video file reference. / 表示收到的视频文件引用。</summary>
public sealed class OneBot12VideoReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12VideoReceivedSegment(string? type, JsonObject data, JsonObject rawJson, string? fileId)
        : base(type, data, rawJson) => FileId = fileId;

    /// <summary>Gets the received file ID. / 获取收到的文件 ID。</summary>
    public string? FileId { get; }
}
