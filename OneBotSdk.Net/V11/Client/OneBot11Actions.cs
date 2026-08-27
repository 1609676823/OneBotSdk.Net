using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Defines exactly the 38 public base actions in the official OneBot 11 specification.
/// 精确定义 OneBot 11 官方规范中的 38 个公开基础动作。
/// </summary>
public static class OneBot11Actions
{
    /// <summary>Sends a private message. / 发送私聊消息。</summary>
    public const string SendPrivateMessage = "send_private_msg";
    /// <summary>Sends a group message. / 发送群消息。</summary>
    public const string SendGroupMessage = "send_group_msg";
    /// <summary>Sends a message using a conditional target. / 按条件目标发送消息。</summary>
    public const string SendMessage = "send_msg";
    /// <summary>Deletes a message. / 撤回消息。</summary>
    public const string DeleteMessage = "delete_msg";
    /// <summary>Gets a message. / 获取消息。</summary>
    public const string GetMessage = "get_msg";
    /// <summary>Gets a merged-forward message. / 获取合并转发消息。</summary>
    public const string GetForwardMessage = "get_forward_msg";
    /// <summary>Sends friend likes. / 发送好友赞。</summary>
    public const string SendLike = "send_like";
    /// <summary>Kicks a group member. / 踢出群成员。</summary>
    public const string SetGroupKick = "set_group_kick";
    /// <summary>Bans a group member. / 禁言群成员。</summary>
    public const string SetGroupBan = "set_group_ban";
    /// <summary>Bans an anonymous group user. / 禁言群匿名用户。</summary>
    public const string SetGroupAnonymousBan = "set_group_anonymous_ban";
    /// <summary>Sets whole-group mute. / 设置全员禁言。</summary>
    public const string SetGroupWholeBan = "set_group_whole_ban";
    /// <summary>Sets a group administrator. / 设置群管理员。</summary>
    public const string SetGroupAdmin = "set_group_admin";
    /// <summary>Sets group anonymous chat. / 设置群匿名聊天。</summary>
    public const string SetGroupAnonymous = "set_group_anonymous";
    /// <summary>Sets a group card. / 设置群名片。</summary>
    public const string SetGroupCard = "set_group_card";
    /// <summary>Sets a group name. / 设置群名称。</summary>
    public const string SetGroupName = "set_group_name";
    /// <summary>
    /// Leaves or dismisses a group; this action is destructive and must be called with care.
    /// 退出或解散群；此动作具有破坏性，必须谨慎调用。
    /// </summary>
    public const string SetGroupLeave = "set_group_leave";
    /// <summary>Sets a group special title. / 设置群专属头衔。</summary>
    public const string SetGroupSpecialTitle = "set_group_special_title";
    /// <summary>Handles a friend request. / 处理好友请求。</summary>
    public const string SetFriendAddRequest = "set_friend_add_request";
    /// <summary>Handles a group request or invitation. / 处理加群请求或邀请。</summary>
    public const string SetGroupAddRequest = "set_group_add_request";
    /// <summary>Gets login information. / 获取登录信息。</summary>
    public const string GetLoginInfo = "get_login_info";
    /// <summary>Gets stranger information. / 获取陌生人信息。</summary>
    public const string GetStrangerInfo = "get_stranger_info";
    /// <summary>Gets the friend list. / 获取好友列表。</summary>
    public const string GetFriendList = "get_friend_list";
    /// <summary>Gets group information. / 获取群信息。</summary>
    public const string GetGroupInfo = "get_group_info";
    /// <summary>Gets the group list. / 获取群列表。</summary>
    public const string GetGroupList = "get_group_list";
    /// <summary>Gets group member information. / 获取群成员信息。</summary>
    public const string GetGroupMemberInfo = "get_group_member_info";
    /// <summary>Gets a group member list. / 获取群成员列表。</summary>
    public const string GetGroupMemberList = "get_group_member_list";
    /// <summary>Gets group honor information. / 获取群荣誉信息。</summary>
    public const string GetGroupHonorInfo = "get_group_honor_info";
    /// <summary>Gets cookies. / 获取 Cookies。</summary>
    public const string GetCookies = "get_cookies";
    /// <summary>Gets a CSRF token. / 获取 CSRF Token。</summary>
    public const string GetCsrfToken = "get_csrf_token";
    /// <summary>Gets combined credentials. / 获取组合凭证。</summary>
    public const string GetCredentials = "get_credentials";
    /// <summary>Gets and converts a record file. / 获取并转换语音文件。</summary>
    public const string GetRecord = "get_record";
    /// <summary>Gets an image file. / 获取图片文件。</summary>
    public const string GetImage = "get_image";
    /// <summary>Checks image-send capability. / 检查图片发送能力。</summary>
    public const string CanSendImage = "can_send_image";
    /// <summary>Checks record-send capability. / 检查语音发送能力。</summary>
    public const string CanSendRecord = "can_send_record";
    /// <summary>Gets implementation status. / 获取实现端状态。</summary>
    public const string GetStatus = "get_status";
    /// <summary>Gets implementation version information. / 获取实现端版本信息。</summary>
    public const string GetVersionInfo = "get_version_info";
    /// <summary>Restarts the OneBot implementation. / 重启 OneBot 实现端。</summary>
    public const string SetRestart = "set_restart";
    /// <summary>Cleans implementation caches. / 清理实现端缓存。</summary>
    public const string CleanCache = "clean_cache";

    /// <summary>
    /// Gets the immutable ordered collection of all standard public base actions.
    /// 获取全部标准公开基础动作的不可变有序集合。
    /// </summary>
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(new[]
    {
        SendPrivateMessage, SendGroupMessage, SendMessage, DeleteMessage, GetMessage,
        GetForwardMessage, SendLike, SetGroupKick, SetGroupBan, SetGroupAnonymousBan,
        SetGroupWholeBan, SetGroupAdmin, SetGroupAnonymous, SetGroupCard, SetGroupName,
        SetGroupLeave, SetGroupSpecialTitle, SetFriendAddRequest, SetGroupAddRequest,
        GetLoginInfo, GetStrangerInfo, GetFriendList, GetGroupInfo, GetGroupList,
        GetGroupMemberInfo, GetGroupMemberList, GetGroupHonorInfo, GetCookies,
        GetCsrfToken, GetCredentials, GetRecord, GetImage, CanSendImage, CanSendRecord,
        GetStatus, GetVersionInfo, SetRestart, CleanCache
    });
}

/// <summary>
/// Defines official hidden actions intended only for frameworks and advanced integrations.
/// 定义仅供框架和高级集成使用的官方隐藏动作。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class OneBot11HiddenActions
{
    /// <summary>
    /// Executes a quick operation against an event context.
    /// 针对事件上下文执行快速操作。
    /// </summary>
    public const string HandleQuickOperation = ".handle_quick_operation";
}
