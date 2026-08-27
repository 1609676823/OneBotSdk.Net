using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents a received generic file reference. / 表示收到的通用文件引用。</summary>
public sealed class OneBot12FileReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12FileReceivedSegment(string? type, JsonObject data, JsonObject rawJson, string? fileId)
        : base(type, data, rawJson) => FileId = fileId;

    /// <summary>Gets the received file ID. / 获取收到的文件 ID。</summary>
    public string? FileId { get; }
}
