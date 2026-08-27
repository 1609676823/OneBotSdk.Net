using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12;

/// <summary>
/// Uniquely identifies a bot account on a OneBot 12 connection.
/// 唯一标识 OneBot 12 连接上的机器人账号。
/// </summary>
public sealed class OneBot12Self : OneBot12JsonModel
{
    /// <summary>Initializes a bot identity. / 初始化机器人身份。</summary>
    public OneBot12Self(string platform, string userId)
        : this(Require(platform, nameof(platform)), Require(userId, nameof(userId)), new JsonObject())
    {
    }

    private OneBot12Self(string? platform, string? userId, JsonObject rawJson)
        : base(rawJson)
    {
        Platform = platform;
        UserId = userId;
    }

    /// <summary>Gets the normalized bot-platform name. / 获取规范化的机器人平台名称。</summary>
    [JsonPropertyName("platform")]
    public string? Platform { get; }

    /// <summary>Gets the platform-specific bot user identifier. / 获取平台内机器人用户标识。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; }

    /// <summary>Creates a detached protocol object. / 创建独立的协议对象。</summary>
    public JsonObject ToJsonObject()
    {
        var result = TolerantJson.CloneObject(RawJson);
        result["platform"] = Platform;
        result["user_id"] = UserId;
        return result;
    }

    /// <summary>Parses a bot identity without allowing one malformed field to hide another. / 按字段容错解析机器人身份。</summary>
    public static OneBot12Self? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new OneBot12Self(
            TolerantJson.String(source, "platform"),
            TolerantJson.String(source, "user_id"),
            TolerantJson.CloneObject(source));
    }

    internal OneBot12Self Clone()
    {
        return new OneBot12Self(Platform, UserId, TolerantJson.CloneObject(RawJson));
    }

    private static string Require(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty OneBot identity value is required.", parameterName);
        }

        return value!;
    }
}
