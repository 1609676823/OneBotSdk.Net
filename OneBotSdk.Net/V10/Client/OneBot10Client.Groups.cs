using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

public sealed partial class OneBot10Client
{
    /// <summary>Kicks a member from a group. / 将成员踢出群。</summary>
    /// <remarks>
    /// This action changes group membership and cannot be automatically undone. Do not use the current login account
    /// or a group owner as an automatic test target.
    /// 此动作会改变群成员关系且无法自动撤销。自动测试时不要使用当前登录账号或群主作为目标。
    /// </remarks>
    public Task<OneBot10Response> SetGroupKickAsync(
        long groupId,
        long userId,
        bool rejectAddRequest = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupKick,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["reject_add_request"] = rejectAddRequest
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Bans or unbans a group member; a zero duration removes the ban. / 禁言或解除群成员禁言；时长为零时解除禁言。</summary>
    public Task<OneBot10Response> SetGroupBanAsync(
        long groupId,
        long userId,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupBan,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["duration"] = duration
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Bans an anonymous group user by the event-provided flag. / 使用事件提供的 flag 禁言群匿名用户。</summary>
    public Task<OneBot10Response> SetGroupAnonymousBanAsync(
        long groupId,
        string anonymousFlag,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (anonymousFlag == null)
        {
            throw new ArgumentNullException(nameof(anonymousFlag));
        }

        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupAnonymousBan,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["anonymous_flag"] = anonymousFlag,
                ["duration"] = duration
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Bans an anonymous group user using the complete event anonymous object. / 使用完整事件匿名对象禁言群匿名用户。</summary>
    public Task<OneBot10Response> SetGroupAnonymousBanAsync(
        long groupId,
        JsonObject anonymous,
        long duration = 1800,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (anonymous == null)
        {
            throw new ArgumentNullException(nameof(anonymous));
        }

        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupAnonymousBan,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["anonymous"] = Clone(anonymous),
                ["duration"] = duration
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Enables or disables whole-group mute. / 启用或禁用全员禁言。</summary>
    public Task<OneBot10Response> SetGroupWholeBanAsync(
        long groupId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupWholeBan,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["enable"] = enable
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Sets or removes a group administrator. / 设置或取消群管理员。</summary>
    public Task<OneBot10Response> SetGroupAdminAsync(
        long groupId,
        long userId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupAdmin,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["enable"] = enable
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Enables or disables anonymous group chat. / 启用或禁用群匿名聊天。</summary>
    public Task<OneBot10Response> SetGroupAnonymousAsync(
        long groupId,
        bool enable = true,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupAnonymous,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["enable"] = enable
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Sets a member's group card; an empty value removes it. / 设置成员群名片；空值将其删除。</summary>
    public Task<OneBot10Response> SetGroupCardAsync(
        long groupId,
        long userId,
        string card = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card));
        }

        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupCard,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["card"] = card
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Leaves a group or dismisses it when the account is owner and dismissal is requested.
    /// 退出群；账号为群主且请求解散时解散群。
    /// </summary>
    /// <remarks>
    /// This action is destructive and cannot be automatically undone. Some implementations may handle a group-owner
    /// request differently even when <paramref name="isDismiss"/> is <see langword="false"/>; do not call it from an
    /// automatic example or test against a group that must be preserved.
    /// 此动作具有破坏性且无法自动撤销。部分实现端即使在 <paramref name="isDismiss"/> 为 <see langword="false"/>
    /// 时也可能以不同方式处理群主请求；不要在自动示例中调用，也不要用于必须保留的群。
    /// </remarks>
    public Task<OneBot10Response> SetGroupLeaveAsync(
        long groupId,
        bool isDismiss = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupLeave,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["is_dismiss"] = isDismiss
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Sets or removes a member's special group title. / 设置或删除成员群专属头衔。</summary>
    public Task<OneBot10Response> SetGroupSpecialTitleAsync(
        long groupId,
        long userId,
        string specialTitle = "",
        long duration = -1,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (specialTitle == null)
        {
            throw new ArgumentNullException(nameof(specialTitle));
        }

        return SendWithoutDataAsync(
            OneBot10Actions.SetGroupSpecialTitle,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["user_id"] = userId,
                ["special_title"] = specialTitle,
                ["duration"] = duration
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Leaves a discussion group. This changes membership and cannot be automatically undone.
    /// 退出讨论组。此操作会改变成员关系且无法自动撤销。
    /// </summary>
    public Task<OneBot10Response> SetDiscussLeaveAsync(
        long discussId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetDiscussLeave,
            new JsonObject { ["discuss_id"] = discussId },
            invocationMode,
            echo,
            cancellationToken);
    }
}
