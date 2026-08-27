using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a custom node returned inside <c>get_forward_msg</c>. / 表示 <c>get_forward_msg</c> 内返回的自定义节点。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ForwardNodeReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the displayed sender ID. / 获取显示的发送者 ID。</summary>
    public string? UserId { get; internal set; }

    /// <summary>Gets the displayed nickname. / 获取显示昵称。</summary>
    public string? Nickname { get; internal set; }

    /// <summary>Gets the independently parsed received nested content. / 获取独立解析的入站嵌套内容。</summary>
    public OneBot11ReceivedMessage? Content { get; internal set; }
}
