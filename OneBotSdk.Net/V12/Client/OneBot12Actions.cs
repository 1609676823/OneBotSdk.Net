using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V12.Client;

/// <summary>Defines all 31 standard OneBot 12 action names. / 定义全部 31 个 OneBot 12 标准动作名称。</summary>
public static class OneBot12Actions
{
    /// <summary>Gets buffered events by HTTP polling. / 通过 HTTP 轮询获取缓冲事件。</summary>
    public const string GetLatestEvents = "get_latest_events";
    /// <summary>Gets the implementation's supported action names. / 获取实现端支持的动作名称。</summary>
    public const string GetSupportedActions = "get_supported_actions";
    /// <summary>Gets implementation and bot status. / 获取实现端及机器人状态。</summary>
    public const string GetStatus = "get_status";
    /// <summary>Gets implementation version information. / 获取实现端版本信息。</summary>
    public const string GetVersion = "get_version";
    /// <summary>Sends a private, group, channel, or extended message. / 发送私聊、群、频道或扩展类型消息。</summary>
    public const string SendMessage = "send_message";
    /// <summary>Deletes a message. / 删除消息。</summary>
    public const string DeleteMessage = "delete_message";
    /// <summary>Gets current bot account information. / 获取当前机器人账号信息。</summary>
    public const string GetSelfInfo = "get_self_info";
    /// <summary>Gets user information. / 获取用户信息。</summary>
    public const string GetUserInfo = "get_user_info";
    /// <summary>Gets the friend or follower list. / 获取好友或关注者列表。</summary>
    public const string GetFriendList = "get_friend_list";
    /// <summary>Gets one group. / 获取一个群。</summary>
    public const string GetGroupInfo = "get_group_info";
    /// <summary>Gets joined groups. / 获取已加入群列表。</summary>
    public const string GetGroupList = "get_group_list";
    /// <summary>Gets one group member. / 获取一个群成员。</summary>
    public const string GetGroupMemberInfo = "get_group_member_info";
    /// <summary>Gets a group's members. / 获取群成员列表。</summary>
    public const string GetGroupMemberList = "get_group_member_list";
    /// <summary>Sets a group name. / 设置群名称。</summary>
    public const string SetGroupName = "set_group_name";
    /// <summary>Leaves a group; this can irreversibly affect membership. / 退出群；此操作可能不可逆地影响群成员关系。</summary>
    public const string LeaveGroup = "leave_group";
    /// <summary>Gets one guild. / 获取一个群组。</summary>
    public const string GetGuildInfo = "get_guild_info";
    /// <summary>Gets joined guilds. / 获取已加入群组列表。</summary>
    public const string GetGuildList = "get_guild_list";
    /// <summary>Sets a guild name. / 设置群组名称。</summary>
    public const string SetGuildName = "set_guild_name";
    /// <summary>Gets one guild member. / 获取一个群组成员。</summary>
    public const string GetGuildMemberInfo = "get_guild_member_info";
    /// <summary>Gets a guild's members. / 获取群组成员列表。</summary>
    public const string GetGuildMemberList = "get_guild_member_list";
    /// <summary>Leaves a guild; this can irreversibly affect membership. / 退出群组；此操作可能不可逆地影响成员关系。</summary>
    public const string LeaveGuild = "leave_guild";
    /// <summary>Gets one channel. / 获取一个频道。</summary>
    public const string GetChannelInfo = "get_channel_info";
    /// <summary>Gets channels visible in a guild. / 获取群组内可见频道列表。</summary>
    public const string GetChannelList = "get_channel_list";
    /// <summary>Sets a channel name. / 设置频道名称。</summary>
    public const string SetChannelName = "set_channel_name";
    /// <summary>Gets one channel member. / 获取一个频道成员。</summary>
    public const string GetChannelMemberInfo = "get_channel_member_info";
    /// <summary>Gets a channel's members. / 获取频道成员列表。</summary>
    public const string GetChannelMemberList = "get_channel_member_list";
    /// <summary>Leaves a channel; this can irreversibly affect membership. / 退出频道；此操作可能不可逆地影响成员关系。</summary>
    public const string LeaveChannel = "leave_channel";
    /// <summary>Uploads a complete file. / 上传完整文件。</summary>
    public const string UploadFile = "upload_file";
    /// <summary>Uploads a file in prepare, transfer, and finish stages. / 按准备、传输和结束阶段分片上传文件。</summary>
    public const string UploadFileFragmented = "upload_file_fragmented";
    /// <summary>Gets a complete file as URL, path, or data. / 以 URL、路径或数据形式获取完整文件。</summary>
    public const string GetFile = "get_file";
    /// <summary>Gets a file in prepare and transfer stages. / 按准备和传输阶段分片获取文件。</summary>
    public const string GetFileFragmented = "get_file_fragmented";

    /// <summary>Gets all standard action names in official documentation order. / 按官方文档顺序获取全部标准动作名称。</summary>
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[]
    {
        GetLatestEvents,
        GetSupportedActions,
        GetStatus,
        GetVersion,
        SendMessage,
        DeleteMessage,
        GetSelfInfo,
        GetUserInfo,
        GetFriendList,
        GetGroupInfo,
        GetGroupList,
        GetGroupMemberInfo,
        GetGroupMemberList,
        SetGroupName,
        LeaveGroup,
        GetGuildInfo,
        GetGuildList,
        SetGuildName,
        GetGuildMemberInfo,
        GetGuildMemberList,
        LeaveGuild,
        GetChannelInfo,
        GetChannelList,
        SetChannelName,
        GetChannelMemberInfo,
        GetChannelMemberList,
        LeaveChannel,
        UploadFile,
        UploadFileFragmented,
        GetFile,
        GetFileFragmented
    });
}
