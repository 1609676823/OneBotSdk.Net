using System;
using OneBotSdk.Net.V10.Events;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Exposes the bot's categorized event streams directly for concise observable subscriptions.
/// 直接公开机器人的分类事件流，便于简洁地进行 Observable 订阅。
/// </summary>
public sealed partial class OneBot10Bot
{
    /// <summary>
    /// Gets the hot stream of every parsed event, including unknown fallbacks.
    /// 获取所有已解析事件（包括未知回退事件）的热流。
    /// </summary>
    public IObservable<OneBot10Event> EventReceived => Events.Events;

    /// <summary>
    /// Gets the hot stream of all message events.
    /// 获取所有消息事件的热流。
    /// </summary>
    public IObservable<OneBot10MessageEvent> MessageReceived => Events.Messages;

    /// <summary>
    /// Gets the hot stream of all notice events.
    /// 获取所有通知事件的热流。
    /// </summary>
    public IObservable<OneBot10NoticeEvent> NoticeReceived => Events.Notices;

    /// <summary>
    /// Gets the hot stream of all request events.
    /// 获取所有请求事件的热流。
    /// </summary>
    public IObservable<OneBot10RequestEvent> RequestReceived => Events.Requests;

    /// <summary>
    /// Gets the hot stream of all meta events.
    /// 获取所有元事件的热流。
    /// </summary>
    public IObservable<OneBot10MetaEvent> MetaEventReceived => Events.MetaEvents;

    /// <summary>
    /// Gets the hot stream of events that use an unknown fallback at any discriminator level.
    /// 获取在任意判别层级使用未知回退类型的事件热流。
    /// </summary>
    public IObservable<OneBot10Event> UnknownEventReceived => Events.UnknownEvents;
}
