using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Contains standard group information. / 包含标准群信息。</summary>
public sealed class OneBot12GroupInfoData : OneBot12JsonModel
{
    private OneBot12GroupInfoData(JsonObject raw, string? groupId, string? groupName) : base(raw)
    {
        GroupId = groupId;
        GroupName = groupName;
    }

    /// <summary>Gets the group ID. / 获取群 ID。</summary>
    [JsonPropertyName("group_id")]
    public string? GroupId { get; }

    /// <summary>Gets the group name. / 获取群名称。</summary>
    [JsonPropertyName("group_name")]
    public string? GroupName { get; }

    internal static OneBot12GroupInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12GroupInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "group_id"),
            TolerantJson.String(source, "group_name"));
    }
}

/// <summary>Contains standard information about one group member. / 包含一个群成员的标准信息。</summary>
public sealed class OneBot12GroupMemberInfoData : OneBot12JsonModel
{
    private OneBot12GroupMemberInfoData(JsonObject raw, string? userId, string? userName, string? displayName) : base(raw)
    {
        UserId = userId;
        UserName = userName;
        UserDisplayName = displayName;
    }

    /// <summary>Gets the member user ID. / 获取成员用户 ID。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; }

    /// <summary>Gets the member name. / 获取成员名称。</summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; }

    /// <summary>Gets the group display name. / 获取群内显示名称。</summary>
    [JsonPropertyName("user_displayname")]
    public string? UserDisplayName { get; }

    internal static OneBot12GroupMemberInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12GroupMemberInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "user_id"),
            TolerantJson.String(source, "user_name"),
            TolerantJson.String(source, "user_displayname"));
    }
}
