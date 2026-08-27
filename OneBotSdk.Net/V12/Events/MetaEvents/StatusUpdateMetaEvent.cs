using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Represents an implementation or bot-account status update. / 表示实现端或机器人账号状态更新。</summary>
public sealed class StatusUpdateMetaEvent : OneBot12MetaEvent
{
    internal StatusUpdateMetaEvent(JsonObject rawJson) : base(rawJson) { }

    /// <summary>Gets the implementation status snapshot. / 获取实现端状态快照。</summary>
    [JsonPropertyName("status")]
    public OneBot12StatusData? Status { get; internal set; }
}
