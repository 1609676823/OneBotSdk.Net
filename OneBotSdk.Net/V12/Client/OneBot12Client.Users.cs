using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Gets information about the selected bot account. / 获取所选机器人账号信息。</summary>
    public Task<OneBot12Response<OneBot12SelfInfoData>> GetSelfInfoAsync(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetSelfInfo,
            null,
            OneBot12SelfInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets information about a friend or stranger. / 获取好友或陌生用户信息。</summary>
    public Task<OneBot12Response<OneBot12UserInfoData>> GetUserInfoAsync(
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetUserInfo,
            new JsonObject { ["user_id"] = Require(userId, nameof(userId)) },
            OneBot12UserInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets the selected bot's friends or followers. / 获取所选机器人的好友或关注者列表。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12UserInfoData>>> GetFriendListAsync(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12UserInfoData>>(
            OneBot12Actions.GetFriendList,
            null,
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12UserInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }
}
