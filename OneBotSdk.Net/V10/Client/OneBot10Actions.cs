using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Defines exactly the 37 public base actions in the official OneBot 10 specification.
/// 精确定义 OneBot 10 官方规范中的 37 个公开基础动作。
/// </summary>
public static class OneBot10Actions
{
    /// <summary>Sends a private message. / 发送私聊消息。</summary>
    public const string SendPrivateMessage = "send_private_msg";

    /// <summary>Sends a group message. / 发送群消息。</summary>
    public const string SendGroupMessage = "send_group_msg";

    /// <summary>Sends a discussion-group message. / 发送讨论组消息。</summary>
    public const string SendDiscussMessage = "send_discuss_msg";

    /// <summary>Sends a message to a target selected by message type. / 按消息类型向指定目标发送消息。</summary>
    public const string SendMessage = "send_msg";

    /// <summary>Deletes a previously sent message. / 撤回一条已发送的消息。</summary>
    public const string DeleteMessage = "delete_msg";

    /// <summary>Sends one or more likes to a user. / 向用户发送一次或多次赞。</summary>
    public const string SendLike = "send_like";

    /// <summary>Removes a member from a group. / 将成员移出群。</summary>
    public const string SetGroupKick = "set_group_kick";

    /// <summary>Sets or removes an individual group ban. / 设置或解除单个群成员禁言。</summary>
    public const string SetGroupBan = "set_group_ban";

    /// <summary>Bans an anonymous group participant. / 禁言群内匿名参与者。</summary>
    public const string SetGroupAnonymousBan = "set_group_anonymous_ban";

    /// <summary>Enables or disables whole-group muting. / 开启或关闭全群禁言。</summary>
    public const string SetGroupWholeBan = "set_group_whole_ban";

    /// <summary>Grants or revokes group administrator status. / 设置或取消群管理员。</summary>
    public const string SetGroupAdmin = "set_group_admin";

    /// <summary>Enables or disables anonymous group messaging. / 开启或关闭群匿名功能。</summary>
    public const string SetGroupAnonymous = "set_group_anonymous";

    /// <summary>Sets a member's group card. / 设置群成员名片。</summary>
    public const string SetGroupCard = "set_group_card";

    /// <summary>
    /// Leaves or dismisses a group. Passing <c>is_dismiss=true</c> as the owner may irreversibly dissolve it.
    /// 退出或解散群。群主传入 <c>is_dismiss=true</c> 可能不可逆地解散群。
    /// </summary>
    public const string SetGroupLeave = "set_group_leave";

    /// <summary>Sets a member's special group title. / 设置群成员专属头衔。</summary>
    public const string SetGroupSpecialTitle = "set_group_special_title";

    /// <summary>Leaves a discussion group. / 退出讨论组。</summary>
    public const string SetDiscussLeave = "set_discuss_leave";

    /// <summary>Processes a friend-add request. / 处理加好友请求。</summary>
    public const string SetFriendAddRequest = "set_friend_add_request";

    /// <summary>Processes a group-add or group-invitation request. / 处理加群或邀请入群请求。</summary>
    public const string SetGroupAddRequest = "set_group_add_request";

    /// <summary>Gets information about the logged-in account. / 获取登录账号信息。</summary>
    public const string GetLoginInfo = "get_login_info";

    /// <summary>Gets information about a stranger. / 获取陌生人信息。</summary>
    public const string GetStrangerInfo = "get_stranger_info";

    /// <summary>Gets the friend list. / 获取好友列表。</summary>
    public const string GetFriendList = "get_friend_list";

    /// <summary>Gets the group list. / 获取群列表。</summary>
    public const string GetGroupList = "get_group_list";

    /// <summary>Gets information about a group. / 获取群信息。</summary>
    public const string GetGroupInfo = "get_group_info";

    /// <summary>Gets information about one group member. / 获取单个群成员信息。</summary>
    public const string GetGroupMemberInfo = "get_group_member_info";

    /// <summary>Gets the member list of a group. / 获取群成员列表。</summary>
    public const string GetGroupMemberList = "get_group_member_list";

    /// <summary>Gets cookies for the requested domain. / 获取指定域名的 Cookie。</summary>
    public const string GetCookies = "get_cookies";

    /// <summary>Gets the CSRF token. / 获取 CSRF Token。</summary>
    public const string GetCsrfToken = "get_csrf_token";

    /// <summary>Gets cookies and the CSRF token together. / 同时获取 Cookie 与 CSRF Token。</summary>
    public const string GetCredentials = "get_credentials";

    /// <summary>Gets or converts a received audio file. / 获取或转换已接收的语音文件。</summary>
    public const string GetRecord = "get_record";

    /// <summary>Gets information about a received image file. / 获取已接收图片的文件信息。</summary>
    public const string GetImage = "get_image";

    /// <summary>Checks whether image messages can be sent. / 检查是否可以发送图片消息。</summary>
    public const string CanSendImage = "can_send_image";

    /// <summary>Checks whether audio messages can be sent. / 检查是否可以发送语音消息。</summary>
    public const string CanSendRecord = "can_send_record";

    /// <summary>Gets the runtime status of the implementation. / 获取实现的运行状态。</summary>
    public const string GetStatus = "get_status";

    /// <summary>Gets implementation version information. / 获取实现版本信息。</summary>
    public const string GetVersionInfo = "get_version_info";

    /// <summary>
    /// Restarts the CQHTTP plug-in and may terminate the current connection before a response is observed.
    /// 重启 CQHTTP 插件，并可能在收到响应前中断当前连接。
    /// </summary>
    public const string SetRestartPlugin = "set_restart_plugin";

    /// <summary>
    /// Deletes files from a CQHTTP data directory and cannot be automatically undone.
    /// 删除 CQHTTP 数据目录中的文件，且无法自动撤销。
    /// </summary>
    public const string CleanDataDirectory = "clean_data_dir";

    /// <summary>
    /// Clears the CQHTTP plug-in log and cannot be automatically undone.
    /// 清空 CQHTTP 插件日志，且无法自动撤销。
    /// </summary>
    public const string CleanPluginLog = "clean_plugin_log";

    /// <summary>Gets all official public base actions in specification order. / 按规范顺序获取全部官方公开基础动作。</summary>
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[]
    {
        SendPrivateMessage, SendGroupMessage, SendDiscussMessage, SendMessage, DeleteMessage, SendLike,
        SetGroupKick, SetGroupBan, SetGroupAnonymousBan, SetGroupWholeBan, SetGroupAdmin,
        SetGroupAnonymous, SetGroupCard, SetGroupLeave, SetGroupSpecialTitle, SetDiscussLeave,
        SetFriendAddRequest, SetGroupAddRequest, GetLoginInfo, GetStrangerInfo, GetFriendList,
        GetGroupList, GetGroupInfo, GetGroupMemberInfo, GetGroupMemberList, GetCookies,
        GetCsrfToken, GetCredentials, GetRecord, GetImage, CanSendImage, CanSendRecord,
        GetStatus, GetVersionInfo, SetRestartPlugin, CleanDataDirectory, CleanPluginLog
    });
}

/// <summary>
/// Defines the sole hidden action in the official OneBot 10 specification.
/// 定义 OneBot 10 官方规范中的唯一隐藏动作。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class OneBot10HiddenActions
{
    /// <summary>Handles a quick operation for an event reported through HTTP POST. / 处理 HTTP POST 上报事件的快速操作。</summary>
    public const string HandleQuickOperation = ".handle_quick_operation";
}
