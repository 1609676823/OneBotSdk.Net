using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Gets information about one single-level group. / 获取一个单级群的信息。</summary>
    public Task<OneBot12Response<OneBot12GroupInfoData>> GetGroupInfoAsync(
        string groupId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetGroupInfo,
            new JsonObject { ["group_id"] = Require(groupId, nameof(groupId)) },
            OneBot12GroupInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets all single-level groups joined by the selected bot. / 获取所选机器人加入的全部单级群。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12GroupInfoData>>> GetGroupListAsync(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12GroupInfoData>>(
            OneBot12Actions.GetGroupList,
            null,
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12GroupInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets one member of a single-level group. / 获取单级群中的一个成员。</summary>
    public Task<OneBot12Response<OneBot12GroupMemberInfoData>> GetGroupMemberInfoAsync(
        string groupId,
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetGroupMemberInfo,
            new JsonObject
            {
                ["group_id"] = Require(groupId, nameof(groupId)),
                ["user_id"] = Require(userId, nameof(userId))
            },
            OneBot12GroupMemberInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets all members of a single-level group. / 获取单级群的全部成员。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12GroupMemberInfoData>>> GetGroupMemberListAsync(
        string groupId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12GroupMemberInfoData>>(
            OneBot12Actions.GetGroupMemberList,
            new JsonObject { ["group_id"] = Require(groupId, nameof(groupId)) },
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12GroupMemberInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Changes a single-level group's name. / 修改单级群名称。</summary>
    public Task<OneBot12Response> SetGroupNameAsync(
        string groupId,
        string groupName,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot12Actions.SetGroupName,
            new JsonObject
            {
                ["group_id"] = Require(groupId, nameof(groupId)),
                ["group_name"] = groupName ?? throw new System.ArgumentNullException(nameof(groupName))
            },
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Leaves a single-level group. / 退出一个单级群。</summary>
    /// <remarks>
    /// This operation changes membership and may be irreversible; never invoke it automatically against a group that must be preserved.
    /// 此操作会改变成员关系且可能无法撤销；切勿针对必须保留的群自动调用。
    /// </remarks>
    public Task<OneBot12Response> LeaveGroupAsync(
        string groupId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot12Actions.LeaveGroup,
            new JsonObject { ["group_id"] = Require(groupId, nameof(groupId)) },
            echo,
            self,
            cancellationToken);
    }
}
