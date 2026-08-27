using System;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Exposes one-to-one EventHandler and Observable subscriptions for every non-message concrete OneBot 10 event.
/// 为每个非消息的 OneBot 10 具体事件提供一对一 EventHandler 与 Observable 订阅。
/// </summary>
public sealed partial class OneBot10EventDispatcher
{
    private readonly OneBot10EventStream<GroupUploadNoticeEvent> _groupUploadNotices = new OneBot10EventStream<GroupUploadNoticeEvent>();
    private readonly OneBot10EventStream<GroupAdminNoticeEvent> _groupAdminNotices = new OneBot10EventStream<GroupAdminNoticeEvent>();
    private readonly OneBot10EventStream<GroupDecreaseNoticeEvent> _groupDecreaseNotices = new OneBot10EventStream<GroupDecreaseNoticeEvent>();
    private readonly OneBot10EventStream<GroupIncreaseNoticeEvent> _groupIncreaseNotices = new OneBot10EventStream<GroupIncreaseNoticeEvent>();
    private readonly OneBot10EventStream<GroupBanNoticeEvent> _groupBanNotices = new OneBot10EventStream<GroupBanNoticeEvent>();
    private readonly OneBot10EventStream<FriendAddNoticeEvent> _friendAddNotices = new OneBot10EventStream<FriendAddNoticeEvent>();
    private readonly OneBot10EventStream<FriendRequestEvent> _friendRequests = new OneBot10EventStream<FriendRequestEvent>();
    private readonly OneBot10EventStream<GroupRequestEvent> _groupRequests = new OneBot10EventStream<GroupRequestEvent>();
    private readonly OneBot10EventStream<LifecycleMetaEvent> _lifecycleEvents = new OneBot10EventStream<LifecycleMetaEvent>();
    private readonly OneBot10EventStream<HeartbeatMetaEvent> _heartbeats = new OneBot10EventStream<HeartbeatMetaEvent>();

    /// <summary>Occurs when a group file-upload notice is received. / 收到群文件上传通知时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupUploadNoticeEvent>>? GroupUploadNoticeReceived;

    /// <summary>Occurs when a group administrator-change notice is received. / 收到群管理员变动通知时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupAdminNoticeEvent>>? GroupAdminNoticeReceived;

    /// <summary>Occurs when a group-member decrease notice is received. / 收到群成员减少通知时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupDecreaseNoticeEvent>>? GroupDecreaseNoticeReceived;

    /// <summary>Occurs when a group-member increase notice is received. / 收到群成员增加通知时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupIncreaseNoticeEvent>>? GroupIncreaseNoticeReceived;

    /// <summary>Occurs when a group-ban notice is received. / 收到群禁言通知时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupBanNoticeEvent>>? GroupBanNoticeReceived;

    /// <summary>Occurs when a friend-add notice is received. / 收到好友新增通知时发生。</summary>
    public event EventHandler<OneBot10EventArgs<FriendAddNoticeEvent>>? FriendAddNoticeReceived;

    /// <summary>Occurs when a friend request is received. / 收到加好友请求时发生。</summary>
    public event EventHandler<OneBot10EventArgs<FriendRequestEvent>>? FriendRequestReceived;

    /// <summary>Occurs when a group-add or group-invitation request is received. / 收到加群或邀请入群请求时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupRequestEvent>>? GroupRequestReceived;

    /// <summary>Occurs when a lifecycle meta-event is received. / 收到生命周期元事件时发生。</summary>
    public event EventHandler<OneBot10EventArgs<LifecycleMetaEvent>>? LifecycleMetaEventReceived;

    /// <summary>Occurs when a heartbeat meta-event is received. / 收到心跳元事件时发生。</summary>
    public event EventHandler<OneBot10EventArgs<HeartbeatMetaEvent>>? HeartbeatMetaEventReceived;

    /// <summary>Gets the observable group file-upload notice stream. / 获取可观察的群文件上传通知流。</summary>
    public IObservable<GroupUploadNoticeEvent> GroupUploadNotices => _groupUploadNotices;

    /// <summary>Gets the observable group administrator-change notice stream. / 获取可观察的群管理员变动通知流。</summary>
    public IObservable<GroupAdminNoticeEvent> GroupAdminNotices => _groupAdminNotices;

    /// <summary>Gets the observable group-member decrease notice stream. / 获取可观察的群成员减少通知流。</summary>
    public IObservable<GroupDecreaseNoticeEvent> GroupDecreaseNotices => _groupDecreaseNotices;

    /// <summary>Gets the observable group-member increase notice stream. / 获取可观察的群成员增加通知流。</summary>
    public IObservable<GroupIncreaseNoticeEvent> GroupIncreaseNotices => _groupIncreaseNotices;

    /// <summary>Gets the observable group-ban notice stream. / 获取可观察的群禁言通知流。</summary>
    public IObservable<GroupBanNoticeEvent> GroupBanNotices => _groupBanNotices;

    /// <summary>Gets the observable friend-add notice stream. / 获取可观察的好友新增通知流。</summary>
    public IObservable<FriendAddNoticeEvent> FriendAddNotices => _friendAddNotices;

    /// <summary>Gets the observable friend-request stream. / 获取可观察的加好友请求流。</summary>
    public IObservable<FriendRequestEvent> FriendRequests => _friendRequests;

    /// <summary>Gets the observable group-request stream. / 获取可观察的群请求流。</summary>
    public IObservable<GroupRequestEvent> GroupRequests => _groupRequests;

    /// <summary>Gets the observable lifecycle meta-event stream. / 获取可观察的生命周期元事件流。</summary>
    public IObservable<LifecycleMetaEvent> LifecycleEvents => _lifecycleEvents;

    /// <summary>Gets the observable heartbeat meta-event stream. / 获取可观察的心跳元事件流。</summary>
    public IObservable<HeartbeatMetaEvent> Heartbeats => _heartbeats;

    private void PublishNotice(OneBot10NoticeEvent notice)
    {
        InvokeEventHandlerSafely(NoticeDispatched, notice);
        _notices.Publish(notice);

        switch (notice)
        {
            case GroupUploadNoticeEvent value: PublishConcreteEvent(GroupUploadNoticeReceived, _groupUploadNotices, value); break;
            case GroupAdminNoticeEvent value: PublishConcreteEvent(GroupAdminNoticeReceived, _groupAdminNotices, value); break;
            case GroupDecreaseNoticeEvent value: PublishConcreteEvent(GroupDecreaseNoticeReceived, _groupDecreaseNotices, value); break;
            case GroupIncreaseNoticeEvent value: PublishConcreteEvent(GroupIncreaseNoticeReceived, _groupIncreaseNotices, value); break;
            case GroupBanNoticeEvent value: PublishConcreteEvent(GroupBanNoticeReceived, _groupBanNotices, value); break;
            case FriendAddNoticeEvent value: PublishConcreteEvent(FriendAddNoticeReceived, _friendAddNotices, value); break;
        }
    }

    private void PublishRequest(OneBot10RequestEvent request)
    {
        InvokeEventHandlerSafely(RequestDispatched, request);
        _requests.Publish(request);

        switch (request)
        {
            case FriendRequestEvent value: PublishConcreteEvent(FriendRequestReceived, _friendRequests, value); break;
            case GroupRequestEvent value: PublishConcreteEvent(GroupRequestReceived, _groupRequests, value); break;
        }
    }

    private void PublishMetaEvent(OneBot10MetaEvent metaEvent)
    {
        InvokeEventHandlerSafely(MetaEventDispatched, metaEvent);
        _metaEvents.Publish(metaEvent);

        switch (metaEvent)
        {
            case LifecycleMetaEvent value: PublishConcreteEvent(LifecycleMetaEventReceived, _lifecycleEvents, value); break;
            case HeartbeatMetaEvent value: PublishConcreteEvent(HeartbeatMetaEventReceived, _heartbeats, value); break;
        }
    }

    private void PublishConcreteEvent<TEvent>(
        EventHandler<OneBot10EventArgs<TEvent>>? handlers,
        OneBot10EventStream<TEvent> stream,
        TEvent value)
        where TEvent : OneBot10Event
    {
        // Keep EventHandler and IObservable delivery order identical for every official event.
        // 保持每个官方事件的 EventHandler 与 IObservable 分发顺序一致。
        InvokeEventHandlerSafely(handlers, value);
        stream.Publish(value);
    }
}
