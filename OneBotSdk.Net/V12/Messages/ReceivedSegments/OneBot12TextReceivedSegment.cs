using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents received plain text. / 表示收到的纯文本。</summary>
public sealed class OneBot12TextReceivedSegment : OneBot12ReceivedSegment
{
    internal OneBot12TextReceivedSegment(string? type, JsonObject data, JsonObject rawJson, string? text)
        : base(type, data, rawJson) => Text = text;

    /// <summary>Gets the received text. / 获取收到的文本。</summary>
    public string? Text { get; }
}
