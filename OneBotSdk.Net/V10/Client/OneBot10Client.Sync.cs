using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Threading;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Messages;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

public sealed partial class OneBot10Client
{
    /// <summary>Synchronously calls a standard or implementation-specific action. / 同步调用标准或实现特有动作。</summary>
    public OneBot10Response CallAction(
        string action,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CallActionAsync(action, parameters, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously calls an action with a caller-supplied data parser. / 使用调用方提供的数据解析器同步调用动作。</summary>
    public OneBot10Response<TData> CallAction<TData>(
        string action,
        Func<JsonNode?, TData?> dataParser,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CallActionAsync(action, dataParser, parameters, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes an event quick operation. / 同步执行事件快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot10Response HandleQuickOperation(
        JsonObject context,
        JsonObject operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes a private-message quick operation. / 同步执行私聊消息快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot10Response HandleQuickOperation(
        PrivateMessageEvent context,
        PrivateMessageQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes a group-message quick operation. / 同步执行群消息快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot10Response HandleQuickOperation(
        GroupMessageEvent context,
        GroupMessageQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes a discussion-message quick operation. / 同步执行讨论组消息快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot10Response HandleQuickOperation(
        DiscussMessageEvent context,
        DiscussMessageQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes a friend-request quick operation. / 同步执行好友请求快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot10Response HandleQuickOperation(
        FriendRequestEvent context,
        FriendRequestQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes a group-request quick operation. / 同步执行群请求快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot10Response HandleQuickOperation(
        GroupRequestEvent context,
        GroupRequestQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a private message. / 同步发送私聊消息。</summary>
    public OneBot10Response<OneBot10SendMessageResult> SendPrivateMessage(
        long userId,
        OneBot10SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendPrivateMessageAsync(userId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a group message. / 同步发送群消息。</summary>
    public OneBot10Response<OneBot10SendMessageResult> SendGroupMessage(
        long groupId,
        OneBot10SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendGroupMessageAsync(groupId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a discussion-group message. / 同步发送讨论组消息。</summary>
    public OneBot10Response<OneBot10SendMessageResult> SendDiscussMessage(
        long discussId,
        OneBot10SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendDiscussMessageAsync(discussId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a conditionally targeted message. / 同步发送条件目标消息。</summary>
    public OneBot10Response<OneBot10SendMessageResult> SendMessage(
        OneBot10SendMessage message,
        OneBot10MessageType? messageType = null,
        long? userId = null,
        long? groupId = null,
        long? discussId = null,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync(message, messageType, userId, groupId, discussId, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously deletes a message. / 同步撤回消息。</summary>
    public OneBot10Response DeleteMessage(
        long messageId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return DeleteMessageAsync(messageId, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends likes to a friend. / 同步向好友发送赞。</summary>
    public OneBot10Response SendLike(
        long userId,
        long times = 1,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendLikeAsync(userId, times, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously kicks a group member. / 同步踢出群成员。</summary>
    public OneBot10Response SetGroupKick(
        long groupId,
        long userId,
        bool rejectAddRequest = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupKickAsync(groupId, userId, rejectAddRequest, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously bans or unbans a group member. / 同步禁言或解除群成员禁言。</summary>
    public OneBot10Response SetGroupBan(
        long groupId,
        long userId,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupBanAsync(groupId, userId, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously bans an anonymous user by flag. / 使用 flag 同步禁言匿名用户。</summary>
    public OneBot10Response SetGroupAnonymousBan(
        long groupId,
        string anonymousFlag,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAnonymousBanAsync(groupId, anonymousFlag, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously bans an anonymous user by event object. / 使用事件对象同步禁言匿名用户。</summary>
    public OneBot10Response SetGroupAnonymousBan(
        long groupId,
        JsonObject anonymous,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAnonymousBanAsync(groupId, anonymous, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously changes whole-group mute. / 同步更改全员禁言状态。</summary>
    public OneBot10Response SetGroupWholeBan(
        long groupId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupWholeBanAsync(groupId, enable, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously changes a group administrator. / 同步更改群管理员。</summary>
    public OneBot10Response SetGroupAdmin(
        long groupId,
        long userId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAdminAsync(groupId, userId, enable, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously changes anonymous group messaging. / 同步更改群匿名功能。</summary>
    public OneBot10Response SetGroupAnonymous(
        long groupId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAnonymousAsync(groupId, enable, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sets a member's group card. / 同步设置成员群名片。</summary>
    public OneBot10Response SetGroupCard(
        long groupId,
        long userId,
        string card = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupCardAsync(groupId, userId, card, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously leaves or dismisses a group. / 同步退出或解散群。</summary>
    public OneBot10Response SetGroupLeave(
        long groupId,
        bool isDismiss = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupLeaveAsync(groupId, isDismiss, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sets a member's special title. / 同步设置成员专属头衔。</summary>
    public OneBot10Response SetGroupSpecialTitle(
        long groupId,
        long userId,
        string specialTitle = "",
        long duration = -1,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupSpecialTitleAsync(groupId, userId, specialTitle, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously leaves a discussion group. / 同步退出讨论组。</summary>
    public OneBot10Response SetDiscussLeave(
        long discussId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetDiscussLeaveAsync(discussId, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously handles a friend-add request. / 同步处理好友添加请求。</summary>
    public OneBot10Response SetFriendAddRequest(
        string flag,
        bool approve = true,
        string remark = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetFriendAddRequestAsync(flag, approve, remark, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously handles a group-add request. / 同步处理加群请求。</summary>
    public OneBot10Response SetGroupAddRequest(
        string flag,
        OneBot10GroupRequestType requestType,
        bool approve = true,
        string reason = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAddRequestAsync(flag, requestType, approve, reason, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets login information. / 同步获取登录信息。</summary>
    public OneBot10Response<OneBot10LoginInfoData> GetLoginInfo(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetLoginInfoAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets user information. / 同步获取用户信息。</summary>
    public OneBot10Response<OneBot10StrangerInfoData> GetStrangerInfo(
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetStrangerInfoAsync(userId, noCache, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets the friend list. / 同步获取好友列表。</summary>
    public OneBot10Response<IReadOnlyList<OneBot10FriendInfo>> GetFriendList(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetFriendListAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets group information. / 同步获取群信息。</summary>
    public OneBot10Response<OneBot10GroupInfo> GetGroupInfo(
        long groupId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupInfoAsync(groupId, noCache, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets the group list. / 同步获取群列表。</summary>
    public OneBot10Response<IReadOnlyList<OneBot10GroupListItem>> GetGroupList(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupListAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets group-member information. / 同步获取群成员信息。</summary>
    public OneBot10Response<OneBot10GroupMemberInfo> GetGroupMemberInfo(
        long groupId,
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupMemberInfoAsync(groupId, userId, noCache, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets a group's member list. / 同步获取群成员列表。</summary>
    public OneBot10Response<IReadOnlyList<OneBot10GroupMemberInfo>> GetGroupMemberList(
        long groupId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupMemberListAsync(groupId, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets cookies. / 同步获取 Cookies。</summary>
    public OneBot10Response<OneBot10CookiesData> GetCookies(
        string domain = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetCookiesAsync(domain, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets the CSRF token. / 同步获取 CSRF Token。</summary>
    public OneBot10Response<OneBot10CsrfTokenData> GetCsrfToken(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetCsrfTokenAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets cookies and the CSRF token. / 同步获取 Cookies 与 CSRF Token。</summary>
    public OneBot10Response<OneBot10CredentialsData> GetCredentials(
        string domain = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetCredentialsAsync(domain, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets and converts a record file. / 同步获取并转换语音文件。</summary>
    public OneBot10Response<OneBot10FileData> GetRecord(
        string file,
        OneBot10RecordFormat outputFormat,
        bool fullPath = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetRecordAsync(file, outputFormat, fullPath, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets an image file. / 同步获取图片文件。</summary>
    public OneBot10Response<OneBot10FileData> GetImage(
        string file,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetImageAsync(file, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously checks image-send capability. / 同步检查图片发送能力。</summary>
    public OneBot10Response<OneBot10CapabilityData> CanSendImage(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CanSendImageAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously checks record-send capability. / 同步检查语音发送能力。</summary>
    public OneBot10Response<OneBot10CapabilityData> CanSendRecord(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CanSendRecordAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets implementation status. / 同步获取实现端状态。</summary>
    public OneBot10Response<OneBot10StatusData> GetStatus(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetStatusAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets implementation version information. / 同步获取实现端版本信息。</summary>
    public OneBot10Response<OneBot10VersionInfoData> GetVersionInfo(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetVersionInfoAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously requests a plug-in restart. / 同步请求重启插件。</summary>
    public OneBot10Response SetRestartPlugin(
        long delay = 0,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetRestartPluginAsync(delay, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously cleans a data directory. / 同步清理数据目录。</summary>
    public OneBot10Response CleanDataDirectory(
        OneBot10DataDirectory dataDirectory,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CleanDataDirectoryAsync(dataDirectory, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously cleans the plug-in log. / 同步清理插件日志。</summary>
    public OneBot10Response CleanPluginLog(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CleanPluginLogAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }
}
