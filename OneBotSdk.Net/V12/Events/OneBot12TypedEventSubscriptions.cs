using System;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Exposes EventHandler and IObservable endpoints for every non-message standard concrete event. / 为每个非消息标准具体事件公开 EventHandler 与 IObservable 入口。</summary>
public sealed partial class OneBot12EventDispatcher
{
    private readonly OneBot12EventStream<FriendIncreaseNoticeEvent> _friendIncreaseNotices = new OneBot12EventStream<FriendIncreaseNoticeEvent>();
    private readonly OneBot12EventStream<FriendDecreaseNoticeEvent> _friendDecreaseNotices = new OneBot12EventStream<FriendDecreaseNoticeEvent>();
    private readonly OneBot12EventStream<PrivateMessageDeleteNoticeEvent> _privateMessageDeleteNotices = new OneBot12EventStream<PrivateMessageDeleteNoticeEvent>();
    private readonly OneBot12EventStream<GroupMemberIncreaseNoticeEvent> _groupMemberIncreaseNotices = new OneBot12EventStream<GroupMemberIncreaseNoticeEvent>();
    private readonly OneBot12EventStream<GroupMemberDecreaseNoticeEvent> _groupMemberDecreaseNotices = new OneBot12EventStream<GroupMemberDecreaseNoticeEvent>();
    private readonly OneBot12EventStream<GroupMessageDeleteNoticeEvent> _groupMessageDeleteNotices = new OneBot12EventStream<GroupMessageDeleteNoticeEvent>();
    private readonly OneBot12EventStream<GuildMemberIncreaseNoticeEvent> _guildMemberIncreaseNotices = new OneBot12EventStream<GuildMemberIncreaseNoticeEvent>();
    private readonly OneBot12EventStream<GuildMemberDecreaseNoticeEvent> _guildMemberDecreaseNotices = new OneBot12EventStream<GuildMemberDecreaseNoticeEvent>();
    private readonly OneBot12EventStream<ChannelMemberIncreaseNoticeEvent> _channelMemberIncreaseNotices = new OneBot12EventStream<ChannelMemberIncreaseNoticeEvent>();
    private readonly OneBot12EventStream<ChannelMemberDecreaseNoticeEvent> _channelMemberDecreaseNotices = new OneBot12EventStream<ChannelMemberDecreaseNoticeEvent>();
    private readonly OneBot12EventStream<ChannelMessageDeleteNoticeEvent> _channelMessageDeleteNotices = new OneBot12EventStream<ChannelMessageDeleteNoticeEvent>();
    private readonly OneBot12EventStream<ChannelCreateNoticeEvent> _channelCreateNotices = new OneBot12EventStream<ChannelCreateNoticeEvent>();
    private readonly OneBot12EventStream<ChannelDeleteNoticeEvent> _channelDeleteNotices = new OneBot12EventStream<ChannelDeleteNoticeEvent>();
    private readonly OneBot12EventStream<ConnectMetaEvent> _connectEvents = new OneBot12EventStream<ConnectMetaEvent>();
    private readonly OneBot12EventStream<HeartbeatMetaEvent> _heartbeats = new OneBot12EventStream<HeartbeatMetaEvent>();
    private readonly OneBot12EventStream<StatusUpdateMetaEvent> _statusUpdates = new OneBot12EventStream<StatusUpdateMetaEvent>();

    /// <summary>Occurs for friend-increase notices. / 在好友增加通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<FriendIncreaseNoticeEvent>>? FriendIncreaseNoticeReceived;
    /// <summary>Occurs for friend-decrease notices. / 在好友减少通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<FriendDecreaseNoticeEvent>>? FriendDecreaseNoticeReceived;
    /// <summary>Occurs for private-message deletion notices. / 在私聊消息删除通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<PrivateMessageDeleteNoticeEvent>>? PrivateMessageDeleteNoticeReceived;
    /// <summary>Occurs for group-member increase notices. / 在群成员增加通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<GroupMemberIncreaseNoticeEvent>>? GroupMemberIncreaseNoticeReceived;
    /// <summary>Occurs for group-member decrease notices. / 在群成员减少通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<GroupMemberDecreaseNoticeEvent>>? GroupMemberDecreaseNoticeReceived;
    /// <summary>Occurs for group-message deletion notices. / 在群消息删除通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<GroupMessageDeleteNoticeEvent>>? GroupMessageDeleteNoticeReceived;
    /// <summary>Occurs for guild-member increase notices. / 在群组成员增加通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<GuildMemberIncreaseNoticeEvent>>? GuildMemberIncreaseNoticeReceived;
    /// <summary>Occurs for guild-member decrease notices. / 在群组成员减少通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<GuildMemberDecreaseNoticeEvent>>? GuildMemberDecreaseNoticeReceived;
    /// <summary>Occurs for channel-member increase notices. / 在频道成员增加通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ChannelMemberIncreaseNoticeEvent>>? ChannelMemberIncreaseNoticeReceived;
    /// <summary>Occurs for channel-member decrease notices. / 在频道成员减少通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ChannelMemberDecreaseNoticeEvent>>? ChannelMemberDecreaseNoticeReceived;
    /// <summary>Occurs for channel-message deletion notices. / 在频道消息删除通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ChannelMessageDeleteNoticeEvent>>? ChannelMessageDeleteNoticeReceived;
    /// <summary>Occurs for channel-create notices. / 在频道创建通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ChannelCreateNoticeEvent>>? ChannelCreateNoticeReceived;
    /// <summary>Occurs for channel-delete notices. / 在频道删除通知到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ChannelDeleteNoticeEvent>>? ChannelDeleteNoticeReceived;
    /// <summary>Occurs for connect meta events. / 在连接元事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ConnectMetaEvent>>? ConnectMetaEventReceived;
    /// <summary>Occurs for heartbeat meta events. / 在心跳元事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<HeartbeatMetaEvent>>? HeartbeatMetaEventReceived;
    /// <summary>Occurs for status-update meta events. / 在状态更新元事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<StatusUpdateMetaEvent>>? StatusUpdateMetaEventReceived;

    /// <summary>Gets friend-increase notices. / 获取好友增加通知热流。</summary>
    public IObservable<FriendIncreaseNoticeEvent> FriendIncreaseNotices => _friendIncreaseNotices;
    /// <summary>Gets friend-decrease notices. / 获取好友减少通知热流。</summary>
    public IObservable<FriendDecreaseNoticeEvent> FriendDecreaseNotices => _friendDecreaseNotices;
    /// <summary>Gets private-message deletion notices. / 获取私聊消息删除通知热流。</summary>
    public IObservable<PrivateMessageDeleteNoticeEvent> PrivateMessageDeleteNotices => _privateMessageDeleteNotices;
    /// <summary>Gets group-member increase notices. / 获取群成员增加通知热流。</summary>
    public IObservable<GroupMemberIncreaseNoticeEvent> GroupMemberIncreaseNotices => _groupMemberIncreaseNotices;
    /// <summary>Gets group-member decrease notices. / 获取群成员减少通知热流。</summary>
    public IObservable<GroupMemberDecreaseNoticeEvent> GroupMemberDecreaseNotices => _groupMemberDecreaseNotices;
    /// <summary>Gets group-message deletion notices. / 获取群消息删除通知热流。</summary>
    public IObservable<GroupMessageDeleteNoticeEvent> GroupMessageDeleteNotices => _groupMessageDeleteNotices;
    /// <summary>Gets guild-member increase notices. / 获取群组成员增加通知热流。</summary>
    public IObservable<GuildMemberIncreaseNoticeEvent> GuildMemberIncreaseNotices => _guildMemberIncreaseNotices;
    /// <summary>Gets guild-member decrease notices. / 获取群组成员减少通知热流。</summary>
    public IObservable<GuildMemberDecreaseNoticeEvent> GuildMemberDecreaseNotices => _guildMemberDecreaseNotices;
    /// <summary>Gets channel-member increase notices. / 获取频道成员增加通知热流。</summary>
    public IObservable<ChannelMemberIncreaseNoticeEvent> ChannelMemberIncreaseNotices => _channelMemberIncreaseNotices;
    /// <summary>Gets channel-member decrease notices. / 获取频道成员减少通知热流。</summary>
    public IObservable<ChannelMemberDecreaseNoticeEvent> ChannelMemberDecreaseNotices => _channelMemberDecreaseNotices;
    /// <summary>Gets channel-message deletion notices. / 获取频道消息删除通知热流。</summary>
    public IObservable<ChannelMessageDeleteNoticeEvent> ChannelMessageDeleteNotices => _channelMessageDeleteNotices;
    /// <summary>Gets channel-create notices. / 获取频道创建通知热流。</summary>
    public IObservable<ChannelCreateNoticeEvent> ChannelCreateNotices => _channelCreateNotices;
    /// <summary>Gets channel-delete notices. / 获取频道删除通知热流。</summary>
    public IObservable<ChannelDeleteNoticeEvent> ChannelDeleteNotices => _channelDeleteNotices;
    /// <summary>Gets connect meta events. / 获取连接元事件热流。</summary>
    public IObservable<ConnectMetaEvent> ConnectEvents => _connectEvents;
    /// <summary>Gets heartbeat meta events. / 获取心跳元事件热流。</summary>
    public IObservable<HeartbeatMetaEvent> Heartbeats => _heartbeats;
    /// <summary>Gets status-update meta events. / 获取状态更新元事件热流。</summary>
    public IObservable<StatusUpdateMetaEvent> StatusUpdates => _statusUpdates;

    private void DispatchNotice(OneBot12NoticeEvent value)
    {
        if (value is FriendIncreaseNoticeEvent a) PublishTyped(FriendIncreaseNoticeReceived, _friendIncreaseNotices, a);
        else if (value is FriendDecreaseNoticeEvent b) PublishTyped(FriendDecreaseNoticeReceived, _friendDecreaseNotices, b);
        else if (value is PrivateMessageDeleteNoticeEvent c) PublishTyped(PrivateMessageDeleteNoticeReceived, _privateMessageDeleteNotices, c);
        else if (value is GroupMemberIncreaseNoticeEvent d) PublishTyped(GroupMemberIncreaseNoticeReceived, _groupMemberIncreaseNotices, d);
        else if (value is GroupMemberDecreaseNoticeEvent e) PublishTyped(GroupMemberDecreaseNoticeReceived, _groupMemberDecreaseNotices, e);
        else if (value is GroupMessageDeleteNoticeEvent f) PublishTyped(GroupMessageDeleteNoticeReceived, _groupMessageDeleteNotices, f);
        else if (value is GuildMemberIncreaseNoticeEvent g) PublishTyped(GuildMemberIncreaseNoticeReceived, _guildMemberIncreaseNotices, g);
        else if (value is GuildMemberDecreaseNoticeEvent h) PublishTyped(GuildMemberDecreaseNoticeReceived, _guildMemberDecreaseNotices, h);
        else if (value is ChannelMemberIncreaseNoticeEvent i) PublishTyped(ChannelMemberIncreaseNoticeReceived, _channelMemberIncreaseNotices, i);
        else if (value is ChannelMemberDecreaseNoticeEvent j) PublishTyped(ChannelMemberDecreaseNoticeReceived, _channelMemberDecreaseNotices, j);
        else if (value is ChannelMessageDeleteNoticeEvent k) PublishTyped(ChannelMessageDeleteNoticeReceived, _channelMessageDeleteNotices, k);
        else if (value is ChannelCreateNoticeEvent l) PublishTyped(ChannelCreateNoticeReceived, _channelCreateNotices, l);
        else if (value is ChannelDeleteNoticeEvent m) PublishTyped(ChannelDeleteNoticeReceived, _channelDeleteNotices, m);
    }

    private void DispatchMeta(OneBot12MetaEvent value)
    {
        if (value is ConnectMetaEvent connect) PublishTyped(ConnectMetaEventReceived, _connectEvents, connect);
        else if (value is HeartbeatMetaEvent heartbeat) PublishTyped(HeartbeatMetaEventReceived, _heartbeats, heartbeat);
        else if (value is StatusUpdateMetaEvent status) PublishTyped(StatusUpdateMetaEventReceived, _statusUpdates, status);
    }
}
