using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Implements a dependency-free hot observable used by the event dispatcher.
/// 实现事件分发器使用的无第三方依赖热 Observable。
/// </summary>
internal sealed class OneBot10EventStream<TEvent> : IObservable<TEvent>
{
    private readonly object _gate = new object();
    private readonly List<Subscription> _subscriptions = new List<Subscription>();

    public IDisposable Subscribe(IObserver<TEvent> observer)
    {
        if (observer == null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var subscription = new Subscription(this, observer);
        lock (_gate)
        {
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    internal void Publish(TEvent value)
    {
        Subscription[] snapshot;
        lock (_gate)
        {
            snapshot = _subscriptions.ToArray();
        }

        foreach (var subscription in snapshot)
        {
            try
            {
                subscription.Publish(value);
            }
            catch (Exception)
            {
                // A faulty observer must not terminate transport event ingestion or starve sibling observers.
                // 异常观察者不得终止传输事件接入，也不得阻止其它观察者接收事件。
            }
        }
    }

    private void Unsubscribe(Subscription subscription)
    {
        lock (_gate)
        {
            // Subscriptions are removed by registration identity, never by an observer's custom Equals implementation.
            // 按注册身份移除订阅，绝不依赖观察者自定义的 Equals 实现。
            _subscriptions.Remove(subscription);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly object _gate = new object();
        private OneBot10EventStream<TEvent>? _owner;
        private readonly IObserver<TEvent> _observer;

        internal Subscription(OneBot10EventStream<TEvent> owner, IObserver<TEvent> observer)
        {
            _owner = owner;
            _observer = observer;
        }

        internal void Publish(TEvent value)
        {
            lock (_gate)
            {
                if (_owner == null)
                {
                    return;
                }

                // The per-registration lock serializes OnNext and ensures Dispose waits for an in-flight callback.
                // 每个注册项的锁会串行化 OnNext，并确保 Dispose 等待正在执行的回调结束。
                _observer.OnNext(value);
            }
        }

        public void Dispose()
        {
            OneBot10EventStream<TEvent>? owner;
            lock (_gate)
            {
                owner = _owner;
                _owner = null;
            }

            owner?.Unsubscribe(this);
        }
    }
}
