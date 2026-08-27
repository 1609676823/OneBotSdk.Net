using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Gets one two-level guild. / 获取一个两级群组。</summary>
    public Task<OneBot12Response<OneBot12GuildInfoData>> GetGuildInfoAsync(
        string guildId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetGuildInfo,
            new JsonObject { ["guild_id"] = Require(guildId, nameof(guildId)) },
            OneBot12GuildInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets all guilds joined by the selected bot. / 获取所选机器人加入的全部群组。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12GuildInfoData>>> GetGuildListAsync(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12GuildInfoData>>(
            OneBot12Actions.GetGuildList,
            null,
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12GuildInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Changes a guild's name. / 修改群组名称。</summary>
    public Task<OneBot12Response> SetGuildNameAsync(
        string guildId,
        string guildName,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot12Actions.SetGuildName,
            new JsonObject
            {
                ["guild_id"] = Require(guildId, nameof(guildId)),
                ["guild_name"] = guildName ?? throw new ArgumentNullException(nameof(guildName))
            },
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets one guild member. / 获取一个群组成员。</summary>
    public Task<OneBot12Response<OneBot12GuildMemberInfoData>> GetGuildMemberInfoAsync(
        string guildId,
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetGuildMemberInfo,
            new JsonObject
            {
                ["guild_id"] = Require(guildId, nameof(guildId)),
                ["user_id"] = Require(userId, nameof(userId))
            },
            OneBot12GuildMemberInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets all members in a guild. / 获取群组中的全部成员。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12GuildMemberInfoData>>> GetGuildMemberListAsync(
        string guildId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12GuildMemberInfoData>>(
            OneBot12Actions.GetGuildMemberList,
            new JsonObject { ["guild_id"] = Require(guildId, nameof(guildId)) },
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12GuildMemberInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Leaves a guild. / 退出一个群组。</summary>
    /// <remarks>This can irreversibly remove the bot from the guild. / 此操作可能不可逆地将机器人移出群组。</remarks>
    public Task<OneBot12Response> LeaveGuildAsync(
        string guildId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot12Actions.LeaveGuild,
            new JsonObject { ["guild_id"] = Require(guildId, nameof(guildId)) },
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets one channel in a guild. / 获取群组中的一个频道。</summary>
    public Task<OneBot12Response<OneBot12ChannelInfoData>> GetChannelInfoAsync(
        string guildId,
        string channelId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot12Actions.GetChannelInfo,
            ChannelParameters(guildId, channelId),
            OneBot12ChannelInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets visible or joined channels in a guild. / 获取群组中可见或已加入的频道。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12ChannelInfoData>>> GetChannelListAsync(
        string guildId,
        bool joinedOnly = false,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12ChannelInfoData>>(
            OneBot12Actions.GetChannelList,
            new JsonObject
            {
                ["guild_id"] = Require(guildId, nameof(guildId)),
                ["joined_only"] = joinedOnly
            },
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12ChannelInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Changes a channel's name. / 修改频道名称。</summary>
    public Task<OneBot12Response> SetChannelNameAsync(
        string guildId,
        string channelId,
        string channelName,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = ChannelParameters(guildId, channelId);
        parameters["channel_name"] = channelName ?? throw new ArgumentNullException(nameof(channelName));
        return SendWithoutDataAsync(OneBot12Actions.SetChannelName, parameters, echo, self, cancellationToken);
    }

    /// <summary>Gets one member in a channel. / 获取频道中的一个成员。</summary>
    public Task<OneBot12Response<OneBot12ChannelMemberInfoData>> GetChannelMemberInfoAsync(
        string guildId,
        string channelId,
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = ChannelParameters(guildId, channelId);
        parameters["user_id"] = Require(userId, nameof(userId));
        return SendTypedAsync(
            OneBot12Actions.GetChannelMemberInfo,
            parameters,
            OneBot12ChannelMemberInfoData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Gets all members in a channel. / 获取频道中的全部成员。</summary>
    public Task<OneBot12Response<IReadOnlyList<OneBot12ChannelMemberInfoData>>> GetChannelMemberListAsync(
        string guildId,
        string channelId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync<IReadOnlyList<OneBot12ChannelMemberInfoData>>(
            OneBot12Actions.GetChannelMemberList,
            ChannelParameters(guildId, channelId),
            node => OneBot12ResponseDataParsers.ParseList(node, OneBot12ChannelMemberInfoData.Parse),
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Leaves a channel. / 退出一个频道。</summary>
    /// <remarks>This can irreversibly remove the bot from the channel. / 此操作可能不可逆地将机器人移出频道。</remarks>
    public Task<OneBot12Response> LeaveChannelAsync(
        string guildId,
        string channelId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot12Actions.LeaveChannel,
            ChannelParameters(guildId, channelId),
            echo,
            self,
            cancellationToken);
    }

    private static JsonObject ChannelParameters(string guildId, string channelId)
    {
        return new JsonObject
        {
            ["guild_id"] = Require(guildId, nameof(guildId)),
            ["channel_id"] = Require(channelId, nameof(channelId))
        };
    }
}
