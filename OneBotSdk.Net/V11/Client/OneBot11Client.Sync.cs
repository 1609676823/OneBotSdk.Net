using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Threading;
using OneBotSdk.Net.V11.Messages;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

public sealed partial class OneBot11Client
{
    /// <summary>Synchronously calls a standard or implementation-specific action. / 同步调用标准或实现特有动作。</summary>
    public OneBot11Response CallAction(
        string action,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CallActionAsync(action, parameters, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously calls an action with a caller-supplied tolerant data parser. / 使用调用方提供的容错 data 解析器同步调用动作。</summary>
    public OneBot11Response<TData> CallAction<TData>(
        string action,
        Func<JsonNode?, TData?> dataParser,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CallActionAsync<TData>(action, dataParser, parameters, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously executes the hidden event quick-operation action. / 同步执行事件快速操作的隐藏动作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public OneBot11Response HandleQuickOperation(
        JsonObject context,
        JsonObject operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a private message using the outgoing-only model. / 使用仅出站模型同步发送私聊消息。</summary>
    public OneBot11Response<OneBot11SendMessageResult> SendPrivateMessage(
        long userId,
        OneBot11SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendPrivateMessageAsync(userId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a private message using the compatibility model. / 使用兼容模型同步发送私聊消息。</summary>
    public OneBot11Response<OneBot11SendMessageData> SendPrivateMessage(
        long userId,
        OneBot11Message message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendPrivateMessageAsync(userId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a group message using the outgoing-only model. / 使用仅出站模型同步发送群消息。</summary>
    public OneBot11Response<OneBot11SendMessageResult> SendGroupMessage(
        long groupId,
        OneBot11SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendGroupMessageAsync(groupId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a group message using the compatibility model. / 使用兼容模型同步发送群消息。</summary>
    public OneBot11Response<OneBot11SendMessageData> SendGroupMessage(
        long groupId,
        OneBot11Message message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendGroupMessageAsync(groupId, message, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a conditionally targeted message using the outgoing-only model. / 使用仅出站模型同步发送条件目标消息。</summary>
    public OneBot11Response<OneBot11SendMessageResult> SendMessage(
        OneBot11SendMessage message,
        OneBot11MessageType? messageType = null,
        long? userId = null,
        long? groupId = null,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync(message, messageType, userId, groupId, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends a conditionally targeted message using the compatibility model. / 使用兼容模型同步发送条件目标消息。</summary>
    public OneBot11Response<OneBot11SendMessageData> SendMessage(
        OneBot11Message message,
        OneBot11MessageType? messageType = null,
        long? userId = null,
        long? groupId = null,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync(message, messageType, userId, groupId, autoEscape, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously deletes a message. / 同步撤回消息。</summary>
    public OneBot11Response DeleteMessage(
        long messageId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return DeleteMessageAsync(messageId, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets a message by identifier. / 按标识同步获取消息。</summary>
    public OneBot11Response<OneBot11MessageData> GetMessage(
        long messageId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetMessageAsync(messageId, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets a merged-forward message. / 同步获取合并转发消息。</summary>
    public OneBot11Response<OneBot11ForwardMessageData> GetForwardMessage(
        string id,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetForwardMessageAsync(id, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sends one or more likes. / 同步发送一次或多次赞。</summary>
    public OneBot11Response SendLike(
        long userId,
        long times = 1,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendLikeAsync(userId, times, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously kicks a member from a group. / 同步将成员踢出群。</summary>
    public OneBot11Response SetGroupKick(
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
    public OneBot11Response SetGroupBan(
        long groupId,
        long userId,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupBanAsync(groupId, userId, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously bans an anonymous group user by flag. / 使用 flag 同步禁言群匿名用户。</summary>
    public OneBot11Response SetGroupAnonymousBan(
        long groupId,
        string anonymousFlag,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAnonymousBanAsync(groupId, anonymousFlag, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously bans an anonymous group user by event object. / 使用事件对象同步禁言群匿名用户。</summary>
    public OneBot11Response SetGroupAnonymousBan(
        long groupId,
        JsonObject anonymous,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAnonymousBanAsync(groupId, anonymous, duration, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously enables or disables whole-group mute. / 同步启用或禁用全员禁言。</summary>
    public OneBot11Response SetGroupWholeBan(
        long groupId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupWholeBanAsync(groupId, enable, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sets or removes a group administrator. / 同步设置或取消群管理员。</summary>
    public OneBot11Response SetGroupAdmin(
        long groupId,
        long userId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAdminAsync(groupId, userId, enable, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously enables or disables anonymous group chat. / 同步启用或禁用群匿名聊天。</summary>
    public OneBot11Response SetGroupAnonymous(
        long groupId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAnonymousAsync(groupId, enable, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sets or removes a member's group card. / 同步设置或删除成员群名片。</summary>
    public OneBot11Response SetGroupCard(
        long groupId,
        long userId,
        string card = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupCardAsync(groupId, userId, card, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sets a group's name. / 同步设置群名称。</summary>
    public OneBot11Response SetGroupName(
        long groupId,
        string groupName,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupNameAsync(groupId, groupName, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously leaves or dismisses a group. / 同步退出或解散群。</summary>
    public OneBot11Response SetGroupLeave(
        long groupId,
        bool isDismiss = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupLeaveAsync(groupId, isDismiss, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously sets or removes a member's special group title. / 同步设置或删除成员群专属头衔。</summary>
    public OneBot11Response SetGroupSpecialTitle(
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

    /// <summary>Synchronously gets the current login information. / 同步获取当前登录信息。</summary>
    public OneBot11Response<OneBot11LoginInfoData> GetLoginInfo(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetLoginInfoAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets information about a QQ user. / 同步获取 QQ 用户信息。</summary>
    public OneBot11Response<OneBot11StrangerInfoData> GetStrangerInfo(
        long userId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetStrangerInfoAsync(userId, noCache, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets the complete friend list. / 同步获取完整好友列表。</summary>
    public OneBot11Response<IReadOnlyList<OneBot11FriendInfo>> GetFriendList(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetFriendListAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets information about one group. / 同步获取一个群的信息。</summary>
    public OneBot11Response<OneBot11GroupInfo> GetGroupInfo(
        long groupId,
        bool noCache = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupInfoAsync(groupId, noCache, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets the complete group list. / 同步获取完整群列表。</summary>
    public OneBot11Response<IReadOnlyList<OneBot11GroupInfo>> GetGroupList(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupListAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets detailed group-member information. / 同步获取群成员详细信息。</summary>
    public OneBot11Response<OneBot11GroupMemberInfo> GetGroupMemberInfo(
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
    public OneBot11Response<IReadOnlyList<OneBot11GroupMemberInfo>> GetGroupMemberList(
        long groupId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupMemberListAsync(groupId, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets group honor information. / 同步获取群荣誉信息。</summary>
    public OneBot11Response<OneBot11GroupHonorInfoData> GetGroupHonorInfo(
        long groupId,
        OneBot11GroupHonorType honorType,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetGroupHonorInfoAsync(groupId, honorType, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously approves or rejects a friend request. / 同步同意或拒绝好友请求。</summary>
    public OneBot11Response SetFriendAddRequest(
        string flag,
        bool approve = true,
        string remark = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetFriendAddRequestAsync(flag, approve, remark, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously approves or rejects a group request. / 同步同意或拒绝加群请求。</summary>
    public OneBot11Response SetGroupAddRequest(
        string flag,
        OneBot11GroupRequestType requestType,
        bool approve = true,
        string reason = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetGroupAddRequestAsync(flag, requestType, approve, reason, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets cookies for an optional domain. / 同步获取可选域名的 Cookies。</summary>
    public OneBot11Response<OneBot11CookiesData> GetCookies(
        string domain = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetCookiesAsync(domain, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets the QQ CSRF token. / 同步获取 QQ CSRF Token。</summary>
    public OneBot11Response<OneBot11CsrfTokenData> GetCsrfToken(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetCsrfTokenAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets cookies and the CSRF token. / 同步获取 Cookies 与 CSRF Token。</summary>
    public OneBot11Response<OneBot11CredentialsData> GetCredentials(
        string domain = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetCredentialsAsync(domain, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets and converts a received record file. / 同步获取并转换收到的语音文件。</summary>
    public OneBot11Response<OneBot11FileData> GetRecord(
        string file,
        OneBot11RecordFormat outputFormat,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetRecordAsync(file, outputFormat, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets a received image file. / 同步获取收到的图片文件。</summary>
    public OneBot11Response<OneBot11FileData> GetImage(
        string file,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetImageAsync(file, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously checks whether images can be sent. / 同步检查是否可以发送图片。</summary>
    public OneBot11Response<OneBot11CapabilityData> CanSendImage(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CanSendImageAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously checks whether records can be sent. / 同步检查是否可以发送语音。</summary>
    public OneBot11Response<OneBot11CapabilityData> CanSendRecord(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CanSendRecordAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets implementation health information. / 同步获取实现端健康信息。</summary>
    public OneBot11Response<OneBot11StatusData> GetStatus(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetStatusAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously gets implementation and protocol version information. / 同步获取实现端与协议版本信息。</summary>
    public OneBot11Response<OneBot11VersionInfoData> GetVersionInfo(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return GetVersionInfoAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously requests an implementation restart. / 同步请求重启实现端。</summary>
    public OneBot11Response SetRestart(
        long delay = 0,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SetRestartAsync(delay, invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Synchronously cleans implementation cache files. / 同步清理实现端缓存文件。</summary>
    public OneBot11Response CleanCache(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return CleanCacheAsync(invocationMode, echo, cancellationToken).GetAwaiter().GetResult();
    }
}
