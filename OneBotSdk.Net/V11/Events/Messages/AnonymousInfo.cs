using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents the anonymous identity attached to an anonymous group message. / 表示匿名群消息附带的匿名身份。</summary>
public sealed class AnonymousInfo : OneBot11JsonModel
{
    internal AnonymousInfo()
    {
    }

    /// <summary>Gets the anonymous user identifier. / 获取匿名用户 ID。</summary>
    [JsonPropertyName("id")]
    public long? Id { get; internal set; }

    /// <summary>Gets the anonymous display name. / 获取匿名用户名称。</summary>
    [JsonPropertyName("name")]
    public string? Name { get; internal set; }

    /// <summary>Gets the opaque flag required by anonymous-ban APIs. / 获取匿名禁言 API 所需的不透明 flag。</summary>
    [JsonPropertyName("flag")]
    public string? Flag { get; internal set; }
}
