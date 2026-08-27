using System;
using OneBotSdk.Net.V12.Events;

namespace OneBotSdk.Net.V12.Client;

/// <summary>
/// Exposes the bot's categorized hot event streams for concise Observable subscriptions.
/// 直接公开机器人的分类事件热流，便于简洁使用 Observable 订阅。
/// </summary>
public sealed partial class OneBot12Bot
{
    /// <summary>Gets every parsed event, including unknown fallbacks. / 获取所有已解析事件，包括未知回退事件。</summary>
    public IObservable<OneBot12Event> EventReceived => Events.Events;

    /// <summary>Gets all message events. / 获取所有消息事件。</summary>
    public IObservable<OneBot12MessageEvent> MessageReceived => Events.Messages;

    /// <summary>Gets all notice events. / 获取所有通知事件。</summary>
    public IObservable<OneBot12NoticeEvent> NoticeReceived => Events.Notices;

    /// <summary>Gets all request-category events. / 获取所有请求类别事件。</summary>
    public IObservable<OneBot12RequestEvent> RequestReceived => Events.Requests;

    /// <summary>Gets all meta events. / 获取所有元事件。</summary>
    public IObservable<OneBot12MetaEvent> MetaEventReceived => Events.MetaEvents;

    /// <summary>Gets events represented by any unknown fallback type. / 获取由任何未知回退类型表示的事件。</summary>
    public IObservable<OneBot12Event> UnknownEventReceived => Events.UnknownEvents;
}
