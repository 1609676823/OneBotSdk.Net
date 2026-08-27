using System;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Messages;

/// <summary>Represents outgoing plain text. / 表示出站纯文本。</summary>
public sealed class OneBot12TextSendSegment : OneBot12SendSegment
{
    /// <summary>Initializes outgoing plain text. / 初始化出站纯文本。</summary>
    public OneBot12TextSendSegment(string text) : base(OneBot12MessageSegmentTypes.Text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Gets the text content. / 获取文本内容。</summary>
    public string Text { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["text"] = Text };
}
