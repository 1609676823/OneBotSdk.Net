using System;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Exposes one-to-one subscriptions for every concrete OneBot 11 notice, request, and meta-event object.
/// 为每个具体 OneBot 11 通知、请求和元事件对象提供一对一订阅。
/// </summary>
public sealed partial class OneBot11EventDispatcher
{
    private readonly OneBot11EventStream<GroupUploadNoticeEvent> _groupUploadNotices = new OneBot11EventStream<GroupUploadNoticeEvent>();
    private readonly OneBot11EventStream<GroupAdminNoticeEvent> _groupAdminNotices = new OneBot11EventStream<GroupAdminNoticeEvent>();
    private readonly OneBot11EventStream<GroupDecreaseNoticeEvent> _groupDecreaseNotices = new OneBot11EventStream<GroupDecreaseNoticeEvent>();
    private readonly OneBot11EventStream<GroupIncreaseNoticeEvent> _groupIncreaseNotices = new OneBot11EventStream<GroupIncreaseNoticeEvent>();
    private readonly OneBot11EventStream<GroupBanNoticeEvent> _groupBanNotices = new OneBot11EventStream<GroupBanNoticeEvent>();
    private readonly OneBot11EventStream<FriendAddNoticeEvent> _friendAddNotices = new OneBot11EventStream<FriendAddNoticeEvent>();
    private readonly OneBot11EventStream<GroupRecallNoticeEvent> _groupRecallNotices = new OneBot11EventStream<GroupRecallNoticeEvent>();
    private readonly OneBot11EventStream<FriendRecallNoticeEvent> _friendRecallNotices = new OneBot11EventStream<FriendRecallNoticeEvent>();
    private readonly OneBot11EventStream<GroupPokeNoticeEvent> _groupPokeNotices = new OneBot11EventStream<GroupPokeNoticeEvent>();
    private readonly OneBot11EventStream<LuckyKingNoticeEvent> _luckyKingNotices = new OneBot11EventStream<LuckyKingNoticeEvent>();
    private readonly OneBot11EventStream<GroupHonorNoticeEvent> _groupHonorNotices = new OneBot11EventStream<GroupHonorNoticeEvent>();
    private readonly OneBot11EventStream<FriendRequestEvent> _friendRequests = new OneBot11EventStream<FriendRequestEvent>();
    private readonly OneBot11EventStream<GroupRequestEvent> _groupRequests = new OneBot11EventStream<GroupRequestEvent>();
    private readonly OneBot11EventStream<LifecycleMetaEvent> _lifecycleEvents = new OneBot11EventStream<LifecycleMetaEvent>();
    private readonly OneBot11EventStream<HeartbeatMetaEvent> _heartbeats = new OneBot11EventStream<HeartbeatMetaEvent>();

    /// <summary>Occurs when a group file is uploaded. / 在群文件上传时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupUploadNoticeEvent>>? GroupUploadNoticeReceived;

    /// <summary>Occurs when group administrator status changes. / 在群管理员状态变更时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupAdminNoticeEvent>>? GroupAdminNoticeReceived;

    /// <summary>Occurs when a member leaves or is removed from a group. / 在群成员退出或被移出时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupDecreaseNoticeEvent>>? GroupDecreaseNoticeReceived;

    /// <summary>Occurs when a member joins a group. / 在群成员加入时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupIncreaseNoticeEvent>>? GroupIncreaseNoticeReceived;

    /// <summary>Occurs when group mute status changes. / 在群禁言状态变更时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupBanNoticeEvent>>? GroupBanNoticeReceived;

    /// <summary>Occurs when a friend is added. / 在添加好友时发生。</summary>
    public event EventHandler<OneBot11EventArgs<FriendAddNoticeEvent>>? FriendAddNoticeReceived;

    /// <summary>Occurs when a group message is recalled. / 在群消息被撤回时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupRecallNoticeEvent>>? GroupRecallNoticeReceived;

    /// <summary>Occurs when a friend message is recalled. / 在好友消息被撤回时发生。</summary>
    public event EventHandler<OneBot11EventArgs<FriendRecallNoticeEvent>>? FriendRecallNoticeReceived;

    /// <summary>Occurs for a group poke notification. / 在群内戳一戳通知到达时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupPokeNoticeEvent>>? GroupPokeNoticeReceived;

    /// <summary>Occurs for a lucky-king notification. / 在群红包运气王通知到达时发生。</summary>
    public event EventHandler<OneBot11EventArgs<LuckyKingNoticeEvent>>? LuckyKingNoticeReceived;

    /// <summary>Occurs when a group honor changes. / 在群成员荣誉变更时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupHonorNoticeEvent>>? GroupHonorNoticeReceived;

    /// <summary>Occurs for a friend-add request. / 在加好友请求到达时发生。</summary>
    public event EventHandler<OneBot11EventArgs<FriendRequestEvent>>? FriendRequestReceived;

    /// <summary>Occurs for a group join request or invitation. / 在加群请求或邀请到达时发生。</summary>
    public event EventHandler<OneBot11EventArgs<GroupRequestEvent>>? GroupRequestReceived;

    /// <summary>Occurs for a lifecycle meta-event. / 在生命周期元事件到达时发生。</summary>
    public event EventHandler<OneBot11EventArgs<LifecycleMetaEvent>>? LifecycleMetaEventReceived;

    /// <summary>Occurs for a heartbeat meta-event. / 在心跳元事件到达时发生。</summary>
    public event EventHandler<OneBot11EventArgs<HeartbeatMetaEvent>>? HeartbeatMetaEventReceived;

    /// <summary>Gets group-upload notices. / 获取群文件上传通知流。</summary>
    public IObservable<GroupUploadNoticeEvent> GroupUploadNotices => _groupUploadNotices;

    /// <summary>Gets group-administrator notices. / 获取群管理员变更通知流。</summary>
    public IObservable<GroupAdminNoticeEvent> GroupAdminNotices => _groupAdminNotices;

    /// <summary>Gets group-member decrease notices. / 获取群成员减少通知流。</summary>
    public IObservable<GroupDecreaseNoticeEvent> GroupDecreaseNotices => _groupDecreaseNotices;

    /// <summary>Gets group-member increase notices. / 获取群成员增加通知流。</summary>
    public IObservable<GroupIncreaseNoticeEvent> GroupIncreaseNotices => _groupIncreaseNotices;

    /// <summary>Gets group-ban notices. / 获取群禁言通知流。</summary>
    public IObservable<GroupBanNoticeEvent> GroupBanNotices => _groupBanNotices;

    /// <summary>Gets friend-add notices. / 获取好友添加通知流。</summary>
    public IObservable<FriendAddNoticeEvent> FriendAddNotices => _friendAddNotices;

    /// <summary>Gets group-message recall notices. / 获取群消息撤回通知流。</summary>
    public IObservable<GroupRecallNoticeEvent> GroupRecallNotices => _groupRecallNotices;

    /// <summary>Gets friend-message recall notices. / 获取好友消息撤回通知流。</summary>
    public IObservable<FriendRecallNoticeEvent> FriendRecallNotices => _friendRecallNotices;

    /// <summary>Gets group-poke notices. / 获取群内戳一戳通知流。</summary>
    public IObservable<GroupPokeNoticeEvent> GroupPokeNotices => _groupPokeNotices;

    /// <summary>Gets lucky-king notices. / 获取群红包运气王通知流。</summary>
    public IObservable<LuckyKingNoticeEvent> LuckyKingNotices => _luckyKingNotices;

    /// <summary>Gets group-honor notices. / 获取群成员荣誉通知流。</summary>
    public IObservable<GroupHonorNoticeEvent> GroupHonorNotices => _groupHonorNotices;

    /// <summary>Gets friend-add requests. / 获取加好友请求流。</summary>
    public IObservable<FriendRequestEvent> FriendRequests => _friendRequests;

    /// <summary>Gets group requests and invitations. / 获取加群请求和邀请流。</summary>
    public IObservable<GroupRequestEvent> GroupRequests => _groupRequests;

    /// <summary>Gets lifecycle meta-events. / 获取生命周期元事件流。</summary>
    public IObservable<LifecycleMetaEvent> LifecycleEvents => _lifecycleEvents;

    /// <summary>Gets heartbeat meta-events. / 获取心跳元事件流。</summary>
    public IObservable<HeartbeatMetaEvent> Heartbeats => _heartbeats;

    private void PublishNotice(OneBot11NoticeEvent notice)
    {
        InvokeEventHandlerSafely(NoticeDispatched, notice);
        _notices.Publish(notice);

        switch (notice)
        {
            case GroupUploadNoticeEvent value:
                PublishConcreteEvent(GroupUploadNoticeReceived, _groupUploadNotices, value);
                break;
            case GroupAdminNoticeEvent value:
                PublishConcreteEvent(GroupAdminNoticeReceived, _groupAdminNotices, value);
                break;
            case GroupDecreaseNoticeEvent value:
                PublishConcreteEvent(GroupDecreaseNoticeReceived, _groupDecreaseNotices, value);
                break;
            case GroupIncreaseNoticeEvent value:
                PublishConcreteEvent(GroupIncreaseNoticeReceived, _groupIncreaseNotices, value);
                break;
            case GroupBanNoticeEvent value:
                PublishConcreteEvent(GroupBanNoticeReceived, _groupBanNotices, value);
                break;
            case FriendAddNoticeEvent value:
                PublishConcreteEvent(FriendAddNoticeReceived, _friendAddNotices, value);
                break;
            case GroupRecallNoticeEvent value:
                PublishConcreteEvent(GroupRecallNoticeReceived, _groupRecallNotices, value);
                break;
            case FriendRecallNoticeEvent value:
                PublishConcreteEvent(FriendRecallNoticeReceived, _friendRecallNotices, value);
                break;
            case GroupPokeNoticeEvent value:
                PublishConcreteEvent(GroupPokeNoticeReceived, _groupPokeNotices, value);
                break;
            case LuckyKingNoticeEvent value:
                PublishConcreteEvent(LuckyKingNoticeReceived, _luckyKingNotices, value);
                break;
            case GroupHonorNoticeEvent value:
                PublishConcreteEvent(GroupHonorNoticeReceived, _groupHonorNotices, value);
                break;
        }
    }

    private void PublishRequest(OneBot11RequestEvent request)
    {
        InvokeEventHandlerSafely(RequestDispatched, request);
        _requests.Publish(request);

        switch (request)
        {
            case FriendRequestEvent value:
                PublishConcreteEvent(FriendRequestReceived, _friendRequests, value);
                break;
            case GroupRequestEvent value:
                PublishConcreteEvent(GroupRequestReceived, _groupRequests, value);
                break;
        }
    }

    private void PublishMetaEvent(OneBot11MetaEvent metaEvent)
    {
        InvokeEventHandlerSafely(MetaEventDispatched, metaEvent);
        _metaEvents.Publish(metaEvent);

        switch (metaEvent)
        {
            case LifecycleMetaEvent value:
                PublishConcreteEvent(LifecycleMetaEventReceived, _lifecycleEvents, value);
                break;
            case HeartbeatMetaEvent value:
                PublishConcreteEvent(HeartbeatMetaEventReceived, _heartbeats, value);
                break;
        }
    }

    private void PublishConcreteEvent<TEvent>(
        EventHandler<OneBot11EventArgs<TEvent>>? handlers,
        OneBot11EventStream<TEvent> stream,
        TEvent value)
        where TEvent : OneBot11Event
    {
        // Keep EventHandler and IObservable delivery order identical for every official concrete event.
        // 保持每个官方具体事件的 EventHandler 与 IObservable 分发顺序一致。
        InvokeEventHandlerSafely(handlers, value);
        stream.Publish(value);
    }
}
