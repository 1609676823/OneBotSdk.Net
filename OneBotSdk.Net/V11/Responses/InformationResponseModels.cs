using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Contains information about the currently logged-in QQ account.
/// 包含当前登录 QQ 账号的信息。
/// </summary>
public sealed class OneBot11LoginInfoData : OneBot11JsonModel
{
    internal static OneBot11LoginInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11LoginInfoData
            {
                RawJson = TolerantJson.CloneObject(source),
                UserId = TolerantJson.Int64(source, "user_id"),
                Nickname = TolerantJson.String(source, "nickname")
            };
    }

    /// <summary>
    /// Gets the logged-in QQ identifier.
    /// 获取登录 QQ 标识。
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>
    /// Gets the logged-in QQ nickname.
    /// 获取登录 QQ 昵称。
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }
}

/// <summary>
/// Contains information about a QQ user outside the friend list contract.
/// 包含好友列表契约之外的 QQ 用户信息。
/// </summary>
public sealed class OneBot11StrangerInfoData : OneBot11JsonModel
{
    internal static OneBot11StrangerInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11StrangerInfoData
            {
                RawJson = TolerantJson.CloneObject(source),
                UserId = TolerantJson.Int64(source, "user_id"),
                Nickname = TolerantJson.String(source, "nickname"),
                Sex = TolerantJson.String(source, "sex"),
                Age = TolerantJson.Int64(source, "age")
            };
    }

    /// <summary>
    /// Gets the QQ identifier.
    /// 获取 QQ 标识。
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>
    /// Gets the nickname.
    /// 获取昵称。
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }

    /// <summary>
    /// Gets the raw sex value: male, female, unknown, or an implementation extension.
    /// 获取原始性别值：male、female、unknown 或实现扩展值。
    /// </summary>
    [JsonPropertyName("sex")]
    public string? Sex { get; private set; }

    /// <summary>
    /// Gets the age.
    /// 获取年龄。
    /// </summary>
    [JsonPropertyName("age")]
    public long? Age { get; private set; }
}

/// <summary>
/// Represents one friend-list item.
/// 表示一个好友列表项。
/// </summary>
public sealed class OneBot11FriendInfo : OneBot11JsonModel
{
    internal static OneBot11FriendInfo? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11FriendInfo
            {
                RawJson = TolerantJson.CloneObject(source),
                UserId = TolerantJson.Int64(source, "user_id"),
                Nickname = TolerantJson.String(source, "nickname"),
                Remark = TolerantJson.String(source, "remark")
            };
    }

    /// <summary>
    /// Gets the friend's QQ identifier.
    /// 获取好友 QQ 标识。
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>
    /// Gets the friend's nickname.
    /// 获取好友昵称。
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }

    /// <summary>
    /// Gets the friend remark.
    /// 获取好友备注。
    /// </summary>
    [JsonPropertyName("remark")]
    public string? Remark { get; private set; }
}

/// <summary>
/// Represents standard OneBot 11 group information.
/// 表示标准 OneBot 11 群信息。
/// </summary>
public sealed class OneBot11GroupInfo : OneBot11JsonModel
{
    internal static OneBot11GroupInfo? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11GroupInfo
            {
                RawJson = TolerantJson.CloneObject(source),
                GroupId = TolerantJson.Int64(source, "group_id"),
                GroupName = TolerantJson.String(source, "group_name"),
                MemberCount = TolerantJson.Int64(source, "member_count"),
                MaxMemberCount = TolerantJson.Int64(source, "max_member_count")
            };
    }

    /// <summary>
    /// Gets the group identifier.
    /// 获取群标识。
    /// </summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; private set; }

    /// <summary>
    /// Gets the group name.
    /// 获取群名称。
    /// </summary>
    [JsonPropertyName("group_name")]
    public string? GroupName { get; private set; }

    /// <summary>
    /// Gets the current member count.
    /// 获取当前成员数。
    /// </summary>
    [JsonPropertyName("member_count")]
    public long? MemberCount { get; private set; }

    /// <summary>
    /// Gets the maximum member count.
    /// 获取最大成员数。
    /// </summary>
    [JsonPropertyName("max_member_count")]
    public long? MaxMemberCount { get; private set; }
}

/// <summary>
/// Represents a group member while preserving incomplete list responses.
/// 表示群成员，同时保留列表响应中可能不完整的字段。
/// </summary>
public sealed class OneBot11GroupMemberInfo : OneBot11JsonModel
{
    internal static OneBot11GroupMemberInfo? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new OneBot11GroupMemberInfo
        {
            RawJson = TolerantJson.CloneObject(source),
            GroupId = TolerantJson.Int64(source, "group_id"),
            UserId = TolerantJson.Int64(source, "user_id"),
            Nickname = TolerantJson.String(source, "nickname"),
            Card = TolerantJson.String(source, "card"),
            Sex = TolerantJson.String(source, "sex"),
            Age = TolerantJson.Int64(source, "age"),
            Area = TolerantJson.String(source, "area"),
            JoinTime = TolerantJson.Int64(source, "join_time"),
            LastSentTime = TolerantJson.Int64(source, "last_sent_time"),
            Level = TolerantJson.String(source, "level"),
            Role = TolerantJson.String(source, "role"),
            Unfriendly = TolerantJson.Boolean(source, "unfriendly"),
            Title = TolerantJson.String(source, "title"),
            TitleExpireTime = TolerantJson.Int64(source, "title_expire_time"),
            CardChangeable = TolerantJson.Boolean(source, "card_changeable")
        };
    }

    /// <summary>Gets the group identifier. / 获取群标识。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; private set; }

    /// <summary>Gets the member QQ identifier. / 获取成员 QQ 标识。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>Gets the nickname. / 获取昵称。</summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }

    /// <summary>Gets the group card. / 获取群名片。</summary>
    [JsonPropertyName("card")]
    public string? Card { get; private set; }

    /// <summary>Gets the raw sex value. / 获取原始性别值。</summary>
    [JsonPropertyName("sex")]
    public string? Sex { get; private set; }

    /// <summary>Gets the age. / 获取年龄。</summary>
    [JsonPropertyName("age")]
    public long? Age { get; private set; }

    /// <summary>Gets the area when the implementation provides it. / 获取实现端提供的地区。</summary>
    [JsonPropertyName("area")]
    public string? Area { get; private set; }

    /// <summary>Gets the join timestamp. / 获取加群时间戳。</summary>
    [JsonPropertyName("join_time")]
    public long? JoinTime { get; private set; }

    /// <summary>Gets the last message timestamp. / 获取最后发言时间戳。</summary>
    [JsonPropertyName("last_sent_time")]
    public long? LastSentTime { get; private set; }

    /// <summary>Gets the member level. / 获取成员等级。</summary>
    [JsonPropertyName("level")]
    public string? Level { get; private set; }

    /// <summary>Gets the raw role: owner, admin, member, or an extension. / 获取原始角色：owner、admin、member 或扩展值。</summary>
    [JsonPropertyName("role")]
    public string? Role { get; private set; }

    /// <summary>Gets whether the member has an unfriendly record. / 获取成员是否有不良记录。</summary>
    [JsonPropertyName("unfriendly")]
    public bool? Unfriendly { get; private set; }

    /// <summary>Gets the special title. / 获取专属头衔。</summary>
    [JsonPropertyName("title")]
    public string? Title { get; private set; }

    /// <summary>Gets the special-title expiration timestamp. / 获取专属头衔过期时间戳。</summary>
    [JsonPropertyName("title_expire_time")]
    public long? TitleExpireTime { get; private set; }

    /// <summary>Gets whether the group card can be changed. / 获取是否允许修改群名片。</summary>
    [JsonPropertyName("card_changeable")]
    public bool? CardChangeable { get; private set; }
}

/// <summary>
/// Contains the conditional group-honor fields returned by <c>get_group_honor_info</c>.
/// 包含 <c>get_group_honor_info</c> 返回的条件性群荣誉字段。
/// </summary>
public sealed class OneBot11GroupHonorInfoData : OneBot11JsonModel
{
    internal static OneBot11GroupHonorInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        if (source == null)
        {
            return null;
        }

        return new OneBot11GroupHonorInfoData
        {
            RawJson = TolerantJson.CloneObject(source),
            GroupId = TolerantJson.Int64(source, "group_id"),
            CurrentTalkative = TolerantJson.Parse(source, "current_talkative", OneBot11CurrentTalkative.Parse),
            TalkativeList = TolerantJson.Parse(source, "talkative_list", node => OneBot11ResponseDataParsers.ParseList(node, OneBot11HonorMember.Parse)),
            PerformerList = TolerantJson.Parse(source, "performer_list", node => OneBot11ResponseDataParsers.ParseList(node, OneBot11HonorMember.Parse)),
            LegendList = TolerantJson.Parse(source, "legend_list", node => OneBot11ResponseDataParsers.ParseList(node, OneBot11HonorMember.Parse)),
            StrongNewbieList = TolerantJson.Parse(source, "strong_newbie_list", node => OneBot11ResponseDataParsers.ParseList(node, OneBot11HonorMember.Parse)),
            EmotionList = TolerantJson.Parse(source, "emotion_list", node => OneBot11ResponseDataParsers.ParseList(node, OneBot11HonorMember.Parse))
        };
    }

    /// <summary>Gets the group identifier. / 获取群标识。</summary>
    [JsonPropertyName("group_id")]
    public long? GroupId { get; private set; }

    /// <summary>Gets the current talkative member when requested. / 获取请求包含时的当前龙王。</summary>
    [JsonPropertyName("current_talkative")]
    public OneBot11CurrentTalkative? CurrentTalkative { get; private set; }

    /// <summary>Gets historical talkative honors when requested. / 获取请求包含时的历史龙王荣誉。</summary>
    [JsonPropertyName("talkative_list")]
    public IReadOnlyList<OneBot11HonorMember>? TalkativeList { get; private set; }

    /// <summary>Gets performer honors when requested. / 获取请求包含时的群聊之火荣誉。</summary>
    [JsonPropertyName("performer_list")]
    public IReadOnlyList<OneBot11HonorMember>? PerformerList { get; private set; }

    /// <summary>Gets legend honors when requested. / 获取请求包含时的群聊炽焰荣誉。</summary>
    [JsonPropertyName("legend_list")]
    public IReadOnlyList<OneBot11HonorMember>? LegendList { get; private set; }

    /// <summary>Gets strong-newbie honors when requested. / 获取请求包含时的冒尖小春笋荣誉。</summary>
    [JsonPropertyName("strong_newbie_list")]
    public IReadOnlyList<OneBot11HonorMember>? StrongNewbieList { get; private set; }

    /// <summary>Gets emotion honors when requested. / 获取请求包含时的快乐之源荣誉。</summary>
    [JsonPropertyName("emotion_list")]
    public IReadOnlyList<OneBot11HonorMember>? EmotionList { get; private set; }
}

/// <summary>
/// Represents the current talkative group member.
/// 表示当前群龙王成员。
/// </summary>
public sealed class OneBot11CurrentTalkative : OneBot11JsonModel
{
    internal static OneBot11CurrentTalkative? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11CurrentTalkative
            {
                RawJson = TolerantJson.CloneObject(source),
                UserId = TolerantJson.Int64(source, "user_id"),
                Nickname = TolerantJson.String(source, "nickname"),
                Avatar = TolerantJson.String(source, "avatar"),
                DayCount = TolerantJson.Int64(source, "day_count")
            };
    }

    /// <summary>Gets the QQ identifier. / 获取 QQ 标识。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>Gets the nickname. / 获取昵称。</summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }

    /// <summary>Gets the avatar URL. / 获取头像 URL。</summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; private set; }

    /// <summary>Gets the consecutive-day count. / 获取连续天数。</summary>
    [JsonPropertyName("day_count")]
    public long? DayCount { get; private set; }
}

/// <summary>
/// Represents one item in a group honor list.
/// 表示群荣誉列表中的一项。
/// </summary>
public sealed class OneBot11HonorMember : OneBot11JsonModel
{
    internal static OneBot11HonorMember? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11HonorMember
            {
                RawJson = TolerantJson.CloneObject(source),
                UserId = TolerantJson.Int64(source, "user_id"),
                Nickname = TolerantJson.String(source, "nickname"),
                Avatar = TolerantJson.String(source, "avatar"),
                Description = TolerantJson.String(source, "description")
            };
    }

    /// <summary>Gets the QQ identifier. / 获取 QQ 标识。</summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; private set; }

    /// <summary>Gets the nickname. / 获取昵称。</summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; private set; }

    /// <summary>Gets the avatar URL. / 获取头像 URL。</summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; private set; }

    /// <summary>Gets the honor description. / 获取荣誉描述。</summary>
    [JsonPropertyName("description")]
    public string? Description { get; private set; }
}
