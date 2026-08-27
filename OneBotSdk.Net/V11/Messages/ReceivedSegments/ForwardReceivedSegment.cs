using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents the receive-only merged-forward reference used in regular messages. / 表示普通消息中仅接收的合并转发引用。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ForwardReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the identifier accepted by <c>get_forward_msg</c>. / 获取 <c>get_forward_msg</c> 接受的标识。</summary>
    public string? ForwardId { get; internal set; }
}
