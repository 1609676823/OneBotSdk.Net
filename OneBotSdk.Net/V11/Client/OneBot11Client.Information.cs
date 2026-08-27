using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

public sealed partial class OneBot11Client
{
    /// <summary>Gets information about the currently logged-in QQ account. / 获取当前登录 QQ 账号信息。</summary>
    public Task<OneBot11Response<OneBot11LoginInfoData>> GetLoginInfoAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetLoginInfo,
            null,
            OneBot11LoginInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets information about a QQ user. / 获取 QQ 用户信息。</summary>
    public Task<OneBot11Response<OneBot11StrangerInfoData>> GetStrangerInfoAsync(
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetStrangerInfo,
            new JsonObject
            {
                ["user_id"] = userId,
                ["no_cache"] = noCache
            },
            OneBot11StrangerInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets the complete friend list. / 获取完整好友列表。</summary>
    public Task<OneBot11Response<IReadOnlyList<OneBot11FriendInfo>>> GetFriendListAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot11FriendInfo>>(
            OneBot11Actions.GetFriendList,
            null,
            node => OneBot11ResponseDataParsers.ParseList(node, OneBot11FriendInfo.Parse),
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets information about one group. / 获取一个群的信息。</summary>
    public Task<OneBot11Response<OneBot11GroupInfo>> GetGroupInfoAsync(
        long groupId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetGroupInfo,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["no_cache"] = noCache
            },
            OneBot11GroupInfo.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets the complete group list. / 获取完整群列表。</summary>
    public Task<OneBot11Response<IReadOnlyList<OneBot11GroupInfo>>> GetGroupListAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot11GroupInfo>>(
            OneBot11Actions.GetGroupList,
            null,
            node => OneBot11ResponseDataParsers.ParseList(node, OneBot11GroupInfo.Parse),
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets detailed information about one group member. / 获取一个群成员的详细信息。</summary>
    public Task<OneBot11Response<OneBot11GroupMemberInfo>> GetGroupMemberInfoAsync(
        long groupId,
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetGroupMemberInfo,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["no_cache"] = noCache
            },
            OneBot11GroupMemberInfo.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets a group's member list; some per-member fields may be absent. / 获取群成员列表；部分成员字段可能缺失。</summary>
    public Task<OneBot11Response<IReadOnlyList<OneBot11GroupMemberInfo>>> GetGroupMemberListAsync(
        long groupId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot11GroupMemberInfo>>(
            OneBot11Actions.GetGroupMemberList,
            new JsonObject { ["group_id"] = groupId },
            node => OneBot11ResponseDataParsers.ParseList(node, OneBot11GroupMemberInfo.Parse),
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets one or all standard group honor categories. / 获取一种或全部标准群荣誉类别。</summary>
    public Task<OneBot11Response<OneBot11GroupHonorInfoData>> GetGroupHonorInfoAsync(
        long groupId,
        OneBot11GroupHonorType honorType,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetGroupHonorInfo,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["type"] = honorType.ToProtocolValue()
            },
            OneBot11GroupHonorInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }
}
