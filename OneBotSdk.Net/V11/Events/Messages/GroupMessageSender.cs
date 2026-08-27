using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>Represents best-effort group-message sender information. / 表示尽力提供的群消息发送者信息。</summary>
public sealed class GroupMessageSender : OneBot11MessageSender
{
    internal GroupMessageSender()
    {
    }

    /// <summary>Gets the group card or remark. / 获取群名片或备注。</summary>
    [JsonPropertyName("card")]
    public string? Card { get; internal set; }

    /// <summary>Gets the reported area. / 获取上报地区。</summary>
    [JsonPropertyName("area")]
    public string? Area { get; internal set; }

    /// <summary>Gets the member level string. / 获取成员等级字符串。</summary>
    [JsonPropertyName("level")]
    public string? Level { get; internal set; }

    /// <summary>Gets <c>owner</c>, <c>admin</c>, <c>member</c>, or an extension value. / 获取群角色标准值或扩展值。</summary>
    [JsonPropertyName("role")]
    public string? Role { get; internal set; }

    /// <summary>Gets the special title. / 获取专属头衔。</summary>
    [JsonPropertyName("title")]
    public string? Title { get; internal set; }
}
