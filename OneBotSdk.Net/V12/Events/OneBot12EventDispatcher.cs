using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V12.Events;

/// <summary>
/// Routes parsed events through both EventHandler and hot IObservable APIs.
/// 通过 EventHandler 与热 IObservable 两套 API 路由已解析事件。
/// </summary>
/// <remarks>
/// Dispatch callbacks are serialized and reentrant events are queued until the current callback chain returns.
/// 分发回调会串行执行，重入事件会排队至当前回调链返回后再处理。
/// </remarks>
public sealed partial class OneBot12EventDispatcher : IObservable<OneBot12Event>
{
    private readonly object _dispatchGate = new object();
    private readonly Queue<OneBot12Event> _pendingEvents = new Queue<OneBot12Event>();
    private bool _isDispatching;

    private readonly OneBot12EventStream<OneBot12Event> _events = new OneBot12EventStream<OneBot12Event>();
    private readonly OneBot12EventStream<OneBot12MessageEvent> _messages = new OneBot12EventStream<OneBot12MessageEvent>();
    private readonly OneBot12EventStream<PrivateMessageEvent> _privateMessages = new OneBot12EventStream<PrivateMessageEvent>();
    private readonly OneBot12EventStream<GroupMessageEvent> _groupMessages = new OneBot12EventStream<GroupMessageEvent>();
    private readonly OneBot12EventStream<ChannelMessageEvent> _channelMessages = new OneBot12EventStream<ChannelMessageEvent>();
    private readonly OneBot12EventStream<OneBot12NoticeEvent> _notices = new OneBot12EventStream<OneBot12NoticeEvent>();
    private readonly OneBot12EventStream<OneBot12RequestEvent> _requests = new OneBot12EventStream<OneBot12RequestEvent>();
    private readonly OneBot12EventStream<OneBot12MetaEvent> _metaEvents = new OneBot12EventStream<OneBot12MetaEvent>();
    private readonly OneBot12EventStream<OneBot12Event> _unknownEvents = new OneBot12EventStream<OneBot12Event>();

    /// <summary>Occurs for every parsed event. / 在每个已解析事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<OneBot12Event>>? EventDispatched;

    /// <summary>Occurs for every message event. / 在每个消息事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<OneBot12MessageEvent>>? MessageDispatched;

    /// <summary>Occurs for private messages. / 在私聊消息到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<PrivateMessageEvent>>? PrivateMessageReceived;

    /// <summary>Occurs for group messages. / 在群消息到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<GroupMessageEvent>>? GroupMessageReceived;

    /// <summary>Occurs for channel messages. / 在频道消息到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<ChannelMessageEvent>>? ChannelMessageReceived;

    /// <summary>Occurs for every notice event. / 在每个通知事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<OneBot12NoticeEvent>>? NoticeDispatched;

    /// <summary>Occurs for every request-category event. / 在每个请求类别事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<OneBot12RequestEvent>>? RequestDispatched;

    /// <summary>Occurs for every meta event. / 在每个元事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<OneBot12MetaEvent>>? MetaEventDispatched;

    /// <summary>Occurs for every unknown fallback. / 在每个未知回退事件到达时发生。</summary>
    public event EventHandler<OneBot12EventArgs<OneBot12Event>>? UnknownEventDispatched;

    /// <summary>Gets the hot stream of all events. / 获取全部事件热流。</summary>
    public IObservable<OneBot12Event> Events => _events;

    /// <summary>Gets the hot stream of message events. / 获取消息事件热流。</summary>
    public IObservable<OneBot12MessageEvent> Messages => _messages;

    /// <summary>Gets the hot stream of private messages. / 获取私聊消息热流。</summary>
    public IObservable<PrivateMessageEvent> PrivateMessages => _privateMessages;

    /// <summary>Gets the hot stream of group messages. / 获取群消息热流。</summary>
    public IObservable<GroupMessageEvent> GroupMessages => _groupMessages;

    /// <summary>Gets the hot stream of channel messages. / 获取频道消息热流。</summary>
    public IObservable<ChannelMessageEvent> ChannelMessages => _channelMessages;

    /// <summary>Gets the hot stream of notice events. / 获取通知事件热流。</summary>
    public IObservable<OneBot12NoticeEvent> Notices => _notices;

    /// <summary>Gets the hot stream of request-category events. / 获取请求类别事件热流。</summary>
    public IObservable<OneBot12RequestEvent> Requests => _requests;

    /// <summary>Gets the hot stream of meta events. / 获取元事件热流。</summary>
    public IObservable<OneBot12MetaEvent> MetaEvents => _metaEvents;

    /// <summary>Gets the hot stream of unknown fallbacks. / 获取未知回退事件热流。</summary>
    public IObservable<OneBot12Event> UnknownEvents => _unknownEvents;

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<OneBot12Event> observer) => _events.Subscribe(observer);

    /// <summary>Dispatches one event while preserving callback order under concurrency and reentrancy. / 分发一个事件，并在并发与重入场景下保持回调顺序。</summary>
    public void Dispatch(OneBot12Event value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        lock (_dispatchGate)
        {
            _pendingEvents.Enqueue(value);
            if (_isDispatching)
            {
                return;
            }

            _isDispatching = true;
            try
            {
                while (_pendingEvents.Count != 0)
                {
                    DispatchCore(_pendingEvents.Dequeue());
                }
            }
            finally
            {
                _isDispatching = false;
            }
        }
    }

    private void DispatchCore(OneBot12Event value)
    {
        InvokeEventHandlerSafely(EventDispatched, value);
        _events.Publish(value);

        if (value is OneBot12MessageEvent message)
        {
            InvokeEventHandlerSafely(MessageDispatched, message);
            _messages.Publish(message);
            DispatchMessage(message);
        }
        else if (value is OneBot12NoticeEvent notice)
        {
            InvokeEventHandlerSafely(NoticeDispatched, notice);
            _notices.Publish(notice);
            DispatchNotice(notice);
        }
        else if (value is OneBot12RequestEvent request)
        {
            InvokeEventHandlerSafely(RequestDispatched, request);
            _requests.Publish(request);
        }
        else if (value is OneBot12MetaEvent metaEvent)
        {
            InvokeEventHandlerSafely(MetaEventDispatched, metaEvent);
            _metaEvents.Publish(metaEvent);
            DispatchMeta(metaEvent);
        }

        if (IsUnknown(value))
        {
            InvokeEventHandlerSafely(UnknownEventDispatched, value);
            _unknownEvents.Publish(value);
        }
    }

    private void DispatchMessage(OneBot12MessageEvent value)
    {
        if (value is PrivateMessageEvent privateMessage)
        {
            PublishTyped(PrivateMessageReceived, _privateMessages, privateMessage);
        }
        else if (value is GroupMessageEvent groupMessage)
        {
            PublishTyped(GroupMessageReceived, _groupMessages, groupMessage);
        }
        else if (value is ChannelMessageEvent channelMessage)
        {
            PublishTyped(ChannelMessageReceived, _channelMessages, channelMessage);
        }
    }

    private static bool IsUnknown(OneBot12Event value)
    {
        return value is UnknownOneBot12Event ||
               value is UnknownMessageEvent ||
               value is UnknownNoticeEvent ||
               value is UnknownRequestEvent ||
               value is UnknownMetaEvent;
    }

    private void PublishTyped<TEvent>(
        EventHandler<OneBot12EventArgs<TEvent>>? handlers,
        OneBot12EventStream<TEvent> stream,
        TEvent value)
        where TEvent : OneBot12Event
    {
        // EventHandler and Observable subscribers see each concrete event in the same deterministic order.
        // EventHandler 与 Observable 订阅者以相同的确定顺序接收每个具体事件。
        InvokeEventHandlerSafely(handlers, value);
        stream.Publish(value);
    }

    private void InvokeEventHandlerSafely<TEvent>(
        EventHandler<OneBot12EventArgs<TEvent>>? handlers,
        TEvent value)
        where TEvent : OneBot12Event
    {
        if (handlers == null)
        {
            return;
        }

        var eventArgs = new OneBot12EventArgs<TEvent>(value);
        foreach (EventHandler<OneBot12EventArgs<TEvent>> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception)
            {
                // A faulty application callback is isolated from transport ingestion and sibling subscribers.
                // 异常应用回调与传输接入及其它订阅者相互隔离。
            }
        }
    }
}
