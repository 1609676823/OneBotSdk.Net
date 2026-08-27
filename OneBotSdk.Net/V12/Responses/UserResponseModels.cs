using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Contains information about the current bot account. / 包含当前机器人账号信息。</summary>
public sealed class OneBot12SelfInfoData : OneBot12JsonModel
{
    private OneBot12SelfInfoData(JsonObject raw, string? userId, string? userName, string? displayName) : base(raw)
    {
        UserId = userId;
        UserName = userName;
        UserDisplayName = displayName;
    }

    /// <summary>Gets the bot user ID. / 获取机器人用户 ID。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; }

    /// <summary>Gets the bot name or nickname. / 获取机器人名称或昵称。</summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; }

    /// <summary>Gets the bot display name. / 获取机器人显示名称。</summary>
    [JsonPropertyName("user_displayname")]
    public string? UserDisplayName { get; }

    internal static OneBot12SelfInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12SelfInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "user_id"),
            TolerantJson.String(source, "user_name"),
            TolerantJson.String(source, "user_displayname"));
    }
}

/// <summary>Contains standard information about a user or friend. / 包含用户或好友的标准信息。</summary>
public sealed class OneBot12UserInfoData : OneBot12JsonModel
{
    private OneBot12UserInfoData(JsonObject raw, string? userId, string? userName, string? displayName, string? remark) : base(raw)
    {
        UserId = userId;
        UserName = userName;
        UserDisplayName = displayName;
        UserRemark = remark;
    }

    /// <summary>Gets the user ID. / 获取用户 ID。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; }

    /// <summary>Gets the user name or nickname. / 获取用户名称或昵称。</summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; }

    /// <summary>Gets the user's display name. / 获取用户显示名称。</summary>
    [JsonPropertyName("user_displayname")]
    public string? UserDisplayName { get; }

    /// <summary>Gets the bot account's remark for this user. / 获取机器人账号为此用户设置的备注。</summary>
    [JsonPropertyName("user_remark")]
    public string? UserRemark { get; }

    internal static OneBot12UserInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12UserInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "user_id"),
            TolerantJson.String(source, "user_name"),
            TolerantJson.String(source, "user_displayname"),
            TolerantJson.String(source, "user_remark"));
    }
}
