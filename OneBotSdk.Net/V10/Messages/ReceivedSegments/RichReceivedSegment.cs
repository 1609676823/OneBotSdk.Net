using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Represents the receive-only OneBot 10 <c>rich</c> segment.
/// 表示 OneBot 10 中仅接收的 <c>rich</c> 消息段。
/// </summary>
/// <remarks>
/// The standard intentionally defines no fixed parameters; use <c>Data</c> and <c>RawJson</c>
/// to inspect every implementation-specific field.
/// 规范有意不定义固定参数；请使用 <c>Data</c> 与 <c>RawJson</c>
/// 查看全部实现特有字段。
/// </remarks>
[JsonConverter(typeof(OneBot10ReceivedSegmentJsonConverter))]
public sealed class RichReceivedSegment : OneBot10ReceivedSegment
{
}
