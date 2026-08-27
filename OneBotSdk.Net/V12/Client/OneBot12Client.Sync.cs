using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Messages;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Synchronously calls a standard or extension action. / 同步调用标准或扩展动作。</summary>
    public OneBot12Response CallAction(
        string action,
        JsonObject? parameters = null,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        CallActionAsync(action, parameters, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously calls an action with a caller-provided data parser. / 使用调用方提供的数据解析器同步调用动作。</summary>
    public OneBot12Response<TData> CallAction<TData>(
        string action,
        Func<JsonNode?, TData?> dataParser,
        JsonObject? parameters = null,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        CallActionAsync(action, dataParser, parameters, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously polls buffered non-meta events. / 同步轮询已缓冲的非元事件。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12Event>> GetLatestEvents(
        long limit = 0,
        long timeoutSeconds = 0,
        string? echo = null,
        CancellationToken cancellationToken = default) =>
        GetLatestEventsAsync(limit, timeoutSeconds, echo, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets every advertised action. / 同步获取实现端声明的全部动作。</summary>
    public OneBot12Response<IReadOnlyList<string>> GetSupportedActions(
        string? echo = null,
        CancellationToken cancellationToken = default) =>
        GetSupportedActionsAsync(echo, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets implementation and bot status. / 同步获取实现端及机器人状态。</summary>
    public OneBot12Response<OneBot12StatusData> GetStatus(
        string? echo = null,
        CancellationToken cancellationToken = default) =>
        GetStatusAsync(echo, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets implementation and protocol version information. / 同步获取实现端及协议版本信息。</summary>
    public OneBot12Response<OneBot12VersionData> GetVersion(
        string? echo = null,
        CancellationToken cancellationToken = default) =>
        GetVersionAsync(echo, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about the selected bot. / 同步获取所选机器人信息。</summary>
    public OneBot12Response<OneBot12SelfInfoData> GetSelfInfo(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetSelfInfoAsync(echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a user. / 同步获取用户信息。</summary>
    public OneBot12Response<OneBot12UserInfoData> GetUserInfo(
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetUserInfoAsync(userId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets the selected bot's friends. / 同步获取所选机器人的好友列表。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12UserInfoData>> GetFriendList(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetFriendListAsync(echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously sends a message to a standard or extension destination. / 同步向标准或扩展目标发送消息。</summary>
    public OneBot12Response<OneBot12SendMessageData> SendMessage(
        string detailType,
        OneBot12SendMessage message,
        string? userId = null,
        string? groupId = null,
        string? guildId = null,
        string? channelId = null,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SendMessageAsync(
            detailType,
            message,
            userId,
            groupId,
            guildId,
            channelId,
            echo,
            self,
            cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously sends a private message. / 同步发送私聊消息。</summary>
    public OneBot12Response<OneBot12SendMessageData> SendPrivateMessage(
        string userId,
        OneBot12SendMessage message,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SendPrivateMessageAsync(userId, message, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously sends a group message. / 同步发送群消息。</summary>
    public OneBot12Response<OneBot12SendMessageData> SendGroupMessage(
        string groupId,
        OneBot12SendMessage message,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SendGroupMessageAsync(groupId, message, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously sends a channel message. / 同步发送频道消息。</summary>
    public OneBot12Response<OneBot12SendMessageData> SendChannelMessage(
        string guildId,
        string channelId,
        OneBot12SendMessage message,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SendChannelMessageAsync(guildId, channelId, message, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously deletes or recalls a message. / 同步删除或撤回消息。</summary>
    public OneBot12Response DeleteMessage(
        string messageId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        DeleteMessageAsync(messageId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a group. / 同步获取群信息。</summary>
    public OneBot12Response<OneBot12GroupInfoData> GetGroupInfo(
        string groupId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGroupInfoAsync(groupId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets all joined groups. / 同步获取已加入的全部群。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12GroupInfoData>> GetGroupList(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGroupListAsync(echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a group member. / 同步获取群成员信息。</summary>
    public OneBot12Response<OneBot12GroupMemberInfoData> GetGroupMemberInfo(
        string groupId,
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGroupMemberInfoAsync(groupId, userId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets all members of a group. / 同步获取群的全部成员。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12GroupMemberInfoData>> GetGroupMemberList(
        string groupId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGroupMemberListAsync(groupId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously changes a group's name. / 同步修改群名称。</summary>
    public OneBot12Response SetGroupName(
        string groupId,
        string groupName,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SetGroupNameAsync(groupId, groupName, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously leaves a group. / 同步退出群。</summary>
    public OneBot12Response LeaveGroup(
        string groupId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        LeaveGroupAsync(groupId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a guild. / 同步获取群组信息。</summary>
    public OneBot12Response<OneBot12GuildInfoData> GetGuildInfo(
        string guildId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGuildInfoAsync(guildId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets all joined guilds. / 同步获取已加入的全部群组。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12GuildInfoData>> GetGuildList(
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGuildListAsync(echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously changes a guild's name. / 同步修改群组名称。</summary>
    public OneBot12Response SetGuildName(
        string guildId,
        string guildName,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SetGuildNameAsync(guildId, guildName, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a guild member. / 同步获取群组成员信息。</summary>
    public OneBot12Response<OneBot12GuildMemberInfoData> GetGuildMemberInfo(
        string guildId,
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGuildMemberInfoAsync(guildId, userId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets all members of a guild. / 同步获取群组的全部成员。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12GuildMemberInfoData>> GetGuildMemberList(
        string guildId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetGuildMemberListAsync(guildId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously leaves a guild. / 同步退出群组。</summary>
    public OneBot12Response LeaveGuild(
        string guildId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        LeaveGuildAsync(guildId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a channel. / 同步获取频道信息。</summary>
    public OneBot12Response<OneBot12ChannelInfoData> GetChannelInfo(
        string guildId,
        string channelId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetChannelInfoAsync(guildId, channelId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets visible or joined channels. / 同步获取可见或已加入的频道。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12ChannelInfoData>> GetChannelList(
        string guildId,
        bool joinedOnly = false,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetChannelListAsync(guildId, joinedOnly, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously changes a channel's name. / 同步修改频道名称。</summary>
    public OneBot12Response SetChannelName(
        string guildId,
        string channelId,
        string channelName,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        SetChannelNameAsync(guildId, channelId, channelName, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets information about a channel member. / 同步获取频道成员信息。</summary>
    public OneBot12Response<OneBot12ChannelMemberInfoData> GetChannelMemberInfo(
        string guildId,
        string channelId,
        string userId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetChannelMemberInfoAsync(guildId, channelId, userId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets all members of a channel. / 同步获取频道的全部成员。</summary>
    public OneBot12Response<IReadOnlyList<OneBot12ChannelMemberInfoData>> GetChannelMemberList(
        string guildId,
        string channelId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetChannelMemberListAsync(guildId, channelId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously leaves a channel. / 同步退出频道。</summary>
    public OneBot12Response LeaveChannel(
        string guildId,
        string channelId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        LeaveChannelAsync(guildId, channelId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously uploads a complete file. / 同步上传完整文件。</summary>
    public OneBot12Response<OneBot12FileIdData> UploadFile(
        OneBot12UploadFileRequest request,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        UploadFileAsync(request, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously prepares a fragmented upload through its compatibility overload. / 通过兼容重载同步准备分片上传。</summary>
    public OneBot12Response<OneBot12FileIdData> UploadFileFragmented(
        string name,
        long totalSize,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        UploadFileFragmentedAsync(name, totalSize, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously transfers an upload fragment through its compatibility overload. / 通过兼容重载同步传输上传分片。</summary>
    public OneBot12Response UploadFileFragmented(
        string fileId,
        long offset,
        byte[] data,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        UploadFileFragmentedAsync(fileId, offset, data, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously finishes a fragmented upload through its compatibility overload. / 通过兼容重载同步完成分片上传。</summary>
    public OneBot12Response<OneBot12FileIdData> UploadFileFragmented(
        string fileId,
        string sha256,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        UploadFileFragmentedAsync(fileId, sha256, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously prepares a fragmented upload. / 同步准备分片上传。</summary>
    public OneBot12Response<OneBot12FileIdData> PrepareUploadFileFragmented(
        string name,
        long totalSize,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        PrepareUploadFileFragmentedAsync(name, totalSize, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously transfers one upload fragment. / 同步传输一个上传分片。</summary>
    public OneBot12Response TransferUploadFileFragment(
        string fileId,
        long offset,
        byte[] data,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        TransferUploadFileFragmentAsync(fileId, offset, data, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously finishes a fragmented upload. / 同步完成分片上传。</summary>
    public OneBot12Response<OneBot12FileIdData> FinishUploadFileFragmented(
        string fileId,
        string sha256,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        FinishUploadFileFragmentedAsync(fileId, sha256, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets a complete file using a standard representation. / 使用标准表示同步获取完整文件。</summary>
    public OneBot12Response<OneBot12FileData> GetFile(
        string fileId,
        OneBot12FileAccessType type,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetFileAsync(fileId, type, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets a complete file using a named representation. / 使用指定表示同步获取完整文件。</summary>
    public OneBot12Response<OneBot12FileData> GetFile(
        string fileId,
        string type,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetFileAsync(fileId, type, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously prepares a fragmented download through its compatibility overload. / 通过兼容重载同步准备分片下载。</summary>
    public OneBot12Response<OneBot12FileDownloadPreparationData> GetFileFragmented(
        string fileId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetFileFragmentedAsync(fileId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets a download fragment through its compatibility overload. / 通过兼容重载同步获取下载分片。</summary>
    public OneBot12Response<OneBot12FileFragmentData> GetFileFragmented(
        string fileId,
        long offset,
        long size,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetFileFragmentedAsync(fileId, offset, size, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously prepares a fragmented download. / 同步准备分片下载。</summary>
    public OneBot12Response<OneBot12FileDownloadPreparationData> PrepareGetFileFragmented(
        string fileId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        PrepareGetFileFragmentedAsync(fileId, echo, self, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronously gets one file fragment. / 同步获取一个文件分片。</summary>
    public OneBot12Response<OneBot12FileFragmentData> GetFileFragment(
        string fileId,
        long offset,
        long size,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default) =>
        GetFileFragmentAsync(fileId, offset, size, echo, self, cancellationToken).GetAwaiter().GetResult();
}
