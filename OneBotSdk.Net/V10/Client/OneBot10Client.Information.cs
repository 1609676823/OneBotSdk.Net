using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

public sealed partial class OneBot10Client
{
    /// <summary>Gets information about the currently logged-in QQ account. / 获取当前登录 QQ 账号信息。</summary>
    public Task<OneBot10Response<OneBot10LoginInfoData>> GetLoginInfoAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetLoginInfo,
            null,
            OneBot10LoginInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets information about a QQ user. / 获取 QQ 用户信息。</summary>
    public Task<OneBot10Response<OneBot10StrangerInfoData>> GetStrangerInfoAsync(
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetStrangerInfo,
            new JsonObject
            {
                ["user_id"] = userId,
                ["no_cache"] = noCache
            },
            OneBot10StrangerInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets the complete friend list. / 获取完整好友列表。</summary>
    public Task<OneBot10Response<IReadOnlyList<OneBot10FriendInfo>>> GetFriendListAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot10FriendInfo>>(
            OneBot10Actions.GetFriendList,
            null,
            node => OneBot10ResponseDataParsers.ParseList(node, OneBot10FriendInfo.Parse),
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets information about one group. / 获取一个群的信息。</summary>
    public Task<OneBot10Response<OneBot10GroupInfo>> GetGroupInfoAsync(
        long groupId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetGroupInfo,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["no_cache"] = noCache
            },
            OneBot10GroupInfo.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets the complete group list. / 获取完整群列表。</summary>
    public Task<OneBot10Response<IReadOnlyList<OneBot10GroupListItem>>> GetGroupListAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot10GroupListItem>>(
            OneBot10Actions.GetGroupList,
            null,
            node => OneBot10ResponseDataParsers.ParseList(node, OneBot10GroupListItem.Parse),
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets detailed information about one group member. / 获取一个群成员的详细信息。</summary>
    public Task<OneBot10Response<OneBot10GroupMemberInfo>> GetGroupMemberInfoAsync(
        long groupId,
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetGroupMemberInfo,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["no_cache"] = noCache
            },
            OneBot10GroupMemberInfo.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets a group's member list; some per-member fields may be absent. / 获取群成员列表；部分成员字段可能缺失。</summary>
    public Task<OneBot10Response<IReadOnlyList<OneBot10GroupMemberInfo>>> GetGroupMemberListAsync(
        long groupId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot10GroupMemberInfo>>(
            OneBot10Actions.GetGroupMemberList,
            new JsonObject { ["group_id"] = groupId },
            node => OneBot10ResponseDataParsers.ParseList(node, OneBot10GroupMemberInfo.Parse),
            invocationMode,
            echo,
            cancellationToken);
    }

}
