using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received recommended friend or group. / 表示收到的推荐好友或群。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class ContactReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets <c>qq</c>, <c>group</c>, or an implementation value. / 获取 <c>qq</c>、<c>group</c> 或实现端值。</summary>
    public string? ContactType { get; internal set; }

    /// <summary>Gets the friend or group ID. / 获取好友或群 ID。</summary>
    public string? Id { get; internal set; }
}
