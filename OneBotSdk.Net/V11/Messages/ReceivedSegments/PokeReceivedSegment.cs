using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Messages;

/// <summary>Represents a received poke including its receive-only display name. / 表示收到的戳一戳，包括其接收专用显示名称。</summary>
[JsonConverter(typeof(OneBot11ReceivedSegmentJsonConverter))]
public sealed class PokeReceivedSegment : OneBot11ReceivedSegment
{
    /// <summary>Gets the poke type. / 获取戳一戳类型。</summary>
    public string? PokeType { get; internal set; }

    /// <summary>Gets the poke ID. / 获取戳一戳 ID。</summary>
    public string? Id { get; internal set; }

    /// <summary>Gets the receive-only display name. / 获取仅接收的显示名称。</summary>
    public string? Name { get; internal set; }
}
