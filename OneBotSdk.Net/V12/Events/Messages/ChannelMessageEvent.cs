using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents a standard channel-message event. / 表示标准频道消息事件。</summary>
public sealed class ChannelMessageEvent : OneBot12MessageEvent
{
    internal ChannelMessageEvent(JsonObject rawJson)
        : base(rawJson)
    {
    }

    /// <summary>Gets the guild identifier. / 获取群组标识。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; internal set; }

    /// <summary>Gets the channel identifier. / 获取频道标识。</summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; internal set; }
}
