using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Represents the tolerant union of private and group sender fields.
/// 表示私聊与群聊发送者字段的容错联合模型。
/// </summary>
public sealed class OneBot11MessageSender : OneBot11JsonModel
{
    internal static OneBot11MessageSender? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new OneBot11MessageSender
        {
            RawJson = TolerantJson.CloneObject(source),
            UserId = TolerantJson.Int64(source, "user_id"),
            Nickname = TolerantJson.String(source, "nickname"),
            Card = TolerantJson.String(source, "card"),
            Sex = TolerantJson.String(source, "sex"),
            Age = TolerantJson.Int64(source, "age"),
            Area = TolerantJson.String(source, "area"),
            Level = TolerantJson.String(source, "level"),
            Role = TolerantJson.String(source, "role"),
            Title = TolerantJson.String(source, "title")
        };
    }

    /// <summary>
    /// Gets the sender's QQ identifier.
    /// 获取发送者 QQ 标识。
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>
    /// Gets the sender's nickname.
    /// 获取发送者昵称。
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }

    /// <summary>
    /// Gets the sender's group card when available.
    /// 获取可用时的发送者群名片。
    /// </summary>
    [JsonPropertyName("card")]
    public string? Card { get; private set; }

    /// <summary>
    /// Gets the raw sex value.
    /// 获取原始性别值。
    /// </summary>
    [JsonPropertyName("sex")]
    public string? Sex { get; private set; }

    /// <summary>
    /// Gets the sender's age.
    /// 获取发送者年龄。
    /// </summary>
    [JsonPropertyName("age")]
    public long? Age { get; private set; }

    /// <summary>
    /// Gets the sender's area when available.
    /// 获取可用时的发送者地区。
    /// </summary>
    [JsonPropertyName("area")]
    public string? Area { get; private set; }

    /// <summary>
    /// Gets the sender's group level when available.
    /// 获取可用时的发送者群等级。
    /// </summary>
    [JsonPropertyName("level")]
    public string? Level { get; private set; }

    /// <summary>
    /// Gets the raw group role, normally owner, admin, or member.
    /// 获取原始群角色，通常为 owner、admin 或 member。
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; private set; }

    /// <summary>
    /// Gets the sender's special group title when available.
    /// 获取可用时的发送者群专属头衔。
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; private set; }
}
