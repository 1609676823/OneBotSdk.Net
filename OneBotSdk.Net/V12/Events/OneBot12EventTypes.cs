namespace OneBotSdk.Net.V12.Events;

/// <summary>
/// Provides the official OneBot 12 event discriminator strings.
/// 提供 OneBot 12 官方事件判别字符串。
/// </summary>
public static class OneBot12EventTypes
{
    /// <summary>The top-level meta-event category. / 顶层元事件类别。</summary>
    public const string Meta = "meta";
    /// <summary>The top-level message-event category. / 顶层消息事件类别。</summary>
    public const string Message = "message";
    /// <summary>The top-level notice-event category. / 顶层通知事件类别。</summary>
    public const string Notice = "notice";
    /// <summary>The reserved top-level request-event category. / 保留的顶层请求事件类别。</summary>
    public const string Request = "request";

    /// <summary>The connection-established meta detail. / 连接已建立的元事件详细类型。</summary>
    public const string Connect = "connect";
    /// <summary>The heartbeat meta detail. / 心跳元事件详细类型。</summary>
    public const string Heartbeat = "heartbeat";
    /// <summary>The runtime-status update meta detail. / 运行状态更新元事件详细类型。</summary>
    public const string StatusUpdate = "status_update";

    /// <summary>The private-message detail. / 私聊消息详细类型。</summary>
    public const string Private = "private";
    /// <summary>The group-message detail. / 群消息详细类型。</summary>
    public const string Group = "group";
    /// <summary>The channel-message detail. / 频道消息详细类型。</summary>
    public const string Channel = "channel";

    /// <summary>The friend-increase notice detail. / 好友增加通知详细类型。</summary>
    public const string FriendIncrease = "friend_increase";
    /// <summary>The friend-decrease notice detail. / 好友减少通知详细类型。</summary>
    public const string FriendDecrease = "friend_decrease";
    /// <summary>The private-message deletion notice detail. / 私聊消息删除通知详细类型。</summary>
    public const string PrivateMessageDelete = "private_message_delete";
    /// <summary>The group-member increase notice detail. / 群成员增加通知详细类型。</summary>
    public const string GroupMemberIncrease = "group_member_increase";
    /// <summary>The group-member decrease notice detail. / 群成员减少通知详细类型。</summary>
    public const string GroupMemberDecrease = "group_member_decrease";
    /// <summary>The group-message deletion notice detail. / 群消息删除通知详细类型。</summary>
    public const string GroupMessageDelete = "group_message_delete";
    /// <summary>The guild-member increase notice detail. / 群组成员增加通知详细类型。</summary>
    public const string GuildMemberIncrease = "guild_member_increase";
    /// <summary>The guild-member decrease notice detail. / 群组成员减少通知详细类型。</summary>
    public const string GuildMemberDecrease = "guild_member_decrease";
    /// <summary>The channel-member increase notice detail. / 频道成员增加通知详细类型。</summary>
    public const string ChannelMemberIncrease = "channel_member_increase";
    /// <summary>The channel-member decrease notice detail. / 频道成员减少通知详细类型。</summary>
    public const string ChannelMemberDecrease = "channel_member_decrease";
    /// <summary>The channel-message deletion notice detail. / 频道消息删除通知详细类型。</summary>
    public const string ChannelMessageDelete = "channel_message_delete";
    /// <summary>The channel-create notice detail. / 频道创建通知详细类型。</summary>
    public const string ChannelCreate = "channel_create";
    /// <summary>The channel-delete notice detail. / 频道删除通知详细类型。</summary>
    public const string ChannelDelete = "channel_delete";
}
