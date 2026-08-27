namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Provides the discriminator strings used by standard OneBot 11 events.
/// 提供标准 OneBot 11 事件使用的判别字符串。
/// </summary>
public static class OneBot11EventTypes
{
    /// <summary>The message post type. / 消息上报类型。</summary>
    public const string Message = "message";
    /// <summary>The notice post type. / 通知上报类型。</summary>
    public const string Notice = "notice";
    /// <summary>The request post type. / 请求上报类型。</summary>
    public const string Request = "request";
    /// <summary>The meta-event post type. / 元事件上报类型。</summary>
    public const string MetaEvent = "meta_event";

    /// <summary>The private message type. / 私聊消息类型。</summary>
    public const string PrivateMessage = "private";
    /// <summary>The group message type. / 群消息类型。</summary>
    public const string GroupMessage = "group";

    /// <summary>The group-upload notice type. / 群文件上传通知类型。</summary>
    public const string GroupUpload = "group_upload";
    /// <summary>The group-administrator notice type. / 群管理员变动通知类型。</summary>
    public const string GroupAdmin = "group_admin";
    /// <summary>The group-decrease notice type. / 群成员减少通知类型。</summary>
    public const string GroupDecrease = "group_decrease";
    /// <summary>The group-increase notice type. / 群成员增加通知类型。</summary>
    public const string GroupIncrease = "group_increase";
    /// <summary>The group-ban notice type. / 群禁言通知类型。</summary>
    public const string GroupBan = "group_ban";
    /// <summary>The friend-add notice type. / 好友添加通知类型。</summary>
    public const string FriendAdd = "friend_add";
    /// <summary>The group-recall notice type. / 群消息撤回通知类型。</summary>
    public const string GroupRecall = "group_recall";
    /// <summary>The friend-recall notice type. / 好友消息撤回通知类型。</summary>
    public const string FriendRecall = "friend_recall";
    /// <summary>The notify notice type. / 提示通知类型。</summary>
    public const string Notify = "notify";

    /// <summary>The friend request type. / 加好友请求类型。</summary>
    public const string FriendRequest = "friend";
    /// <summary>The group request type. / 加群请求类型。</summary>
    public const string GroupRequest = "group";
    /// <summary>The lifecycle meta-event type. / 生命周期元事件类型。</summary>
    public const string Lifecycle = "lifecycle";
    /// <summary>The heartbeat meta-event type. / 心跳元事件类型。</summary>
    public const string Heartbeat = "heartbeat";
}
