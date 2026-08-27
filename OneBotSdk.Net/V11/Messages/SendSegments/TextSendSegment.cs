using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents outgoing plain text. / 表示出站纯文本。</summary>
[JsonConverter(typeof(OneBot11SendSegmentJsonConverter))]
public sealed class TextSendSegment : OneBot11SendSegment
{
    /// <summary>Initializes outgoing plain text. / 初始化出站纯文本。</summary>
    public TextSendSegment(string text) : base(MessageSegmentTypes.Text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Gets the text content. / 获取文本内容。</summary>
    public string Text { get; }

    /// <inheritdoc />
    protected override JsonObject CreateData() => new JsonObject { ["text"] = Text };
}
