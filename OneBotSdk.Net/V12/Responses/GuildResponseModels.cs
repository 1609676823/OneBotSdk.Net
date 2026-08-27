using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Contains standard two-level guild information. / 包含标准两级群组信息。</summary>
public sealed class OneBot12GuildInfoData : OneBot12JsonModel
{
    private OneBot12GuildInfoData(JsonObject raw, string? id, string? name) : base(raw) { GuildId = id; GuildName = name; }

    /// <summary>Gets the guild ID. / 获取群组 ID。</summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; }

    /// <summary>Gets the guild name. / 获取群组名称。</summary>
    [JsonPropertyName("guild_name")]
    public string? GuildName { get; }

    internal static OneBot12GuildInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12GuildInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "guild_id"),
            TolerantJson.String(source, "guild_name"));
    }
}

/// <summary>Contains standard guild-member information. / 包含标准群组成员信息。</summary>
public sealed class OneBot12GuildMemberInfoData : OneBot12JsonModel
{
    private OneBot12GuildMemberInfoData(JsonObject raw, string? id, string? name, string? displayName) : base(raw)
    {
        UserId = id;
        UserName = name;
        UserDisplayName = displayName;
    }

    /// <summary>Gets the member user ID. / 获取成员用户 ID。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; }

    /// <summary>Gets the member name. / 获取成员名称。</summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; }

    /// <summary>Gets the guild display name. / 获取群组内显示名称。</summary>
    [JsonPropertyName("user_displayname")]
    public string? UserDisplayName { get; }

    internal static OneBot12GuildMemberInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12GuildMemberInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "user_id"),
            TolerantJson.String(source, "user_name"),
            TolerantJson.String(source, "user_displayname"));
    }
}

/// <summary>Contains standard channel information. / 包含标准频道信息。</summary>
public sealed class OneBot12ChannelInfoData : OneBot12JsonModel
{
    private OneBot12ChannelInfoData(JsonObject raw, string? id, string? name) : base(raw) { ChannelId = id; ChannelName = name; }

    /// <summary>Gets the channel ID. / 获取频道 ID。</summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; }

    /// <summary>Gets the channel name. / 获取频道名称。</summary>
    [JsonPropertyName("channel_name")]
    public string? ChannelName { get; }

    internal static OneBot12ChannelInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12ChannelInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "channel_id"),
            TolerantJson.String(source, "channel_name"));
    }
}

/// <summary>Contains standard channel-member information. / 包含标准频道成员信息。</summary>
public sealed class OneBot12ChannelMemberInfoData : OneBot12JsonModel
{
    private OneBot12ChannelMemberInfoData(JsonObject raw, string? id, string? name, string? displayName) : base(raw)
    {
        UserId = id;
        UserName = name;
        UserDisplayName = displayName;
    }

    /// <summary>Gets the member user ID. / 获取成员用户 ID。</summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; }

    /// <summary>Gets the member name. / 获取成员名称。</summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; }

    /// <summary>Gets the channel display name. / 获取频道内显示名称。</summary>
    [JsonPropertyName("user_displayname")]
    public string? UserDisplayName { get; }

    internal static OneBot12ChannelMemberInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null ? null : new OneBot12ChannelMemberInfoData(
            TolerantJson.CloneObject(source),
            TolerantJson.String(source, "user_id"),
            TolerantJson.String(source, "user_name"),
            TolerantJson.String(source, "user_displayname"));
    }
}
