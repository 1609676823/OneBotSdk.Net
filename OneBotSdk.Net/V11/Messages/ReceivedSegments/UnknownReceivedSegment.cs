using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Retains an unknown, send-only, or malformed incoming segment. / 保留未知、仅发送或格式异常的入站消息段。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class UnknownReceivedSegment : OneBot11ReceivedSegment
{
}
