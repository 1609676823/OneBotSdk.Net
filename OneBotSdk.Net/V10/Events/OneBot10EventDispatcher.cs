using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Routes parsed events through standard event handlers and categorized hot observable streams.
/// 通过标准事件处理器和分类热 Observable 流路由已解析事件。
/// </summary>
/// <remarks>
/// Observable streams do not replay, retain delivered values, or complete when one transport disconnects because a dispatcher can be reused.
/// Observable 流不会重放、保留已分发值，也不会在单个传输断开时完成，因为分发器可以复用。
/// Dispatch callbacks are serialized, and reentrant events are queued until the current callback chain returns.
/// A callback must never synchronously wait for an action response
/// that depends on the same WebSocket receive loop; start an asynchronous task that handles its own exceptions instead.
/// 分发回调会串行执行，重入事件会排队至当前回调链返回后处理。
/// 回调绝不能同步等待依赖同一 WebSocket 接收循环的动作响应；
/// 应改为启动一个自行处理异常的异步任务。
/// </remarks>
public sealed partial class OneBot10EventDispatcher : IObservable<OneBot10Event>
{
    private readonly object _dispatchGate = new object();
    private readonly Queue<OneBot10Event> _pendingEvents = new Queue<OneBot10Event>();
    private bool _isDispatching;
    private readonly OneBot10EventStream<OneBot10Event> _events = new OneBot10EventStream<OneBot10Event>();
    private readonly OneBot10EventStream<OneBot10MessageEvent> _messages = new OneBot10EventStream<OneBot10MessageEvent>();
    private readonly OneBot10EventStream<PrivateMessageEvent> _privateMessages = new OneBot10EventStream<PrivateMessageEvent>();
    private readonly OneBot10EventStream<GroupMessageEvent> _groupMessages = new OneBot10EventStream<GroupMessageEvent>();
    private readonly OneBot10EventStream<DiscussMessageEvent> _discussMessages = new OneBot10EventStream<DiscussMessageEvent>();
    private readonly OneBot10EventStream<OneBot10NoticeEvent> _notices = new OneBot10EventStream<OneBot10NoticeEvent>();
    private readonly OneBot10EventStream<OneBot10RequestEvent> _requests = new OneBot10EventStream<OneBot10RequestEvent>();
    private readonly OneBot10EventStream<OneBot10MetaEvent> _metaEvents = new OneBot10EventStream<OneBot10MetaEvent>();
    private readonly OneBot10EventStream<OneBot10Event> _unknownEvents = new OneBot10EventStream<OneBot10Event>();

    /// <summary>Occurs for every event through the standard sender/EventArgs pattern. / 通过标准 sender/EventArgs 模式在每个事件到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<OneBot10Event>>? EventDispatched;

    /// <summary>Occurs for every message through the standard sender/EventArgs pattern. / 通过标准 sender/EventArgs 模式在每个消息到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<OneBot10MessageEvent>>? MessageDispatched;

    /// <summary>Occurs for private messages, including the standard friend subtype. / 在私聊消息（包括标准好友子类型）到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<PrivateMessageEvent>>? PrivateMessageReceived;

    /// <summary>Occurs for group messages. / 在群消息到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<GroupMessageEvent>>? GroupMessageReceived;

    /// <summary>Occurs for discussion-group messages. / 在讨论组消息到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<DiscussMessageEvent>>? DiscussMessageReceived;

    /// <summary>Occurs for notices through the standard sender/EventArgs pattern. / 通过标准 sender/EventArgs 模式在通知到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<OneBot10NoticeEvent>>? NoticeDispatched;

    /// <summary>Occurs for requests through the standard sender/EventArgs pattern. / 通过标准 sender/EventArgs 模式在请求到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<OneBot10RequestEvent>>? RequestDispatched;

    /// <summary>Occurs for meta events through the standard sender/EventArgs pattern. / 通过标准 sender/EventArgs 模式在元事件到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<OneBot10MetaEvent>>? MetaEventDispatched;

    /// <summary>Occurs for unknown fallbacks through the standard sender/EventArgs pattern. / 通过标准 sender/EventArgs 模式在未知回退事件到达时发生。</summary>
    public event EventHandler<OneBot10EventArgs<OneBot10Event>>? UnknownEventDispatched;

    /// <summary>Gets the hot stream of every parsed event. / 获取所有已解析事件的热流。</summary>
    public IObservable<OneBot10Event> Events => _events;

    /// <summary>Gets the hot stream of all message events. / 获取所有消息事件的热流。</summary>
    public IObservable<OneBot10MessageEvent> Messages => _messages;

    /// <summary>Gets the hot stream of private messages. / 获取私聊消息热流。</summary>
    public IObservable<PrivateMessageEvent> PrivateMessages => _privateMessages;

    /// <summary>Gets the hot stream of group messages. / 获取群消息热流。</summary>
    public IObservable<GroupMessageEvent> GroupMessages => _groupMessages;

    /// <summary>Gets the hot stream of discussion-group messages. / 获取讨论组消息热流。</summary>
    public IObservable<DiscussMessageEvent> DiscussMessages => _discussMessages;

    /// <summary>Gets the hot stream of notice events. / 获取通知事件热流。</summary>
    public IObservable<OneBot10NoticeEvent> Notices => _notices;

    /// <summary>Gets the hot stream of request events. / 获取请求事件热流。</summary>
    public IObservable<OneBot10RequestEvent> Requests => _requests;

    /// <summary>Gets the hot stream of meta events. / 获取元事件热流。</summary>
    public IObservable<OneBot10MetaEvent> MetaEvents => _metaEvents;

    /// <summary>Gets the hot stream of unknown fallback events. / 获取未知回退事件热流。</summary>
    public IObservable<OneBot10Event> UnknownEvents => _unknownEvents;

    /// <summary>
    /// Subscribes to every parsed event; this is equivalent to subscribing to <see cref="Events"/>.
    /// 订阅所有已解析事件；等价于订阅 <see cref="Events"/>。
    /// </summary>
    public IDisposable Subscribe(IObserver<OneBot10Event> observer)
    {
        return _events.Subscribe(observer);
    }

    /// <summary>
    /// Queues one parsed event for serialized delivery from general streams to category and concrete message streams.
    /// 将一个已解析事件排入队列，并从通用流到分类和具体消息流串行分发。
    /// </summary>
    public void Dispatch(OneBot10Event value)
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
        }

        // The queue serializes concurrent ingress without holding a monitor while user callbacks execute.
        // 队列在执行用户回调时不持有监视器锁，同时可串行化并发入口。
        DrainQueue();
    }

    private void DrainQueue()
    {
        try
        {
            while (true)
            {
                OneBot10Event value;
                lock (_dispatchGate)
                {
                    if (_pendingEvents.Count == 0)
                    {
                        _isDispatching = false;
                        return;
                    }

                    value = _pendingEvents.Dequeue();
                }

                DispatchCore(value);
            }
        }
        catch
        {
            // Normal subscriber exceptions are isolated below; reset ownership for unexpected internal failures.
            // 普通订阅者异常会在下方隔离；遇到未预期内部失败时重置队列所有权。
            lock (_dispatchGate)
            {
                _isDispatching = false;
            }

            throw;
        }
    }

    private void DispatchCore(OneBot10Event value)
    {
        InvokeEventHandlerSafely(EventDispatched, value);
        _events.Publish(value);

        switch (value)
        {
            case OneBot10MessageEvent message:
                PublishMessage(message);
                break;
            case OneBot10NoticeEvent notice:
                PublishNotice(notice);
                break;
            case OneBot10RequestEvent request:
                PublishRequest(request);
                break;
            case OneBot10MetaEvent metaEvent:
                PublishMetaEvent(metaEvent);
                break;
        }

        if (IsUnknown(value))
        {
            InvokeEventHandlerSafely(UnknownEventDispatched, value);
            _unknownEvents.Publish(value);
        }
    }

    private void PublishMessage(OneBot10MessageEvent message)
    {
        InvokeEventHandlerSafely(MessageDispatched, message);
        _messages.Publish(message);

        switch (message)
        {
            case PrivateMessageEvent privateMessage:
                InvokeEventHandlerSafely(PrivateMessageReceived, privateMessage);
                _privateMessages.Publish(privateMessage);
                break;
            case GroupMessageEvent groupMessage:
                InvokeEventHandlerSafely(GroupMessageReceived, groupMessage);
                _groupMessages.Publish(groupMessage);
                break;
            case DiscussMessageEvent discussMessage:
                InvokeEventHandlerSafely(DiscussMessageReceived, discussMessage);
                _discussMessages.Publish(discussMessage);
                break;
        }
    }

    private static bool IsUnknown(OneBot10Event value)
    {
        return value is UnknownOneBot10Event ||
               value is UnknownMessageEvent ||
               value is UnknownNoticeEvent ||
               value is UnknownRequestEvent ||
               value is UnknownMetaEvent;
    }

    private void InvokeEventHandlerSafely<TEvent>(
        EventHandler<OneBot10EventArgs<TEvent>>? handlers,
        TEvent value)
        where TEvent : OneBot10Event
    {
        if (handlers == null)
        {
            return;
        }

        var eventArgs = new OneBot10EventArgs<TEvent>(value);
        foreach (EventHandler<OneBot10EventArgs<TEvent>> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception)
            {
                // EventHandler subscribers have the same transport-isolation guarantee as observers.
                // EventHandler 订阅者与观察者具有相同的传输隔离保证。
            }
        }
    }
}
