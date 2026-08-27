using System;
using System.Collections.Generic;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Implements a dependency-free hot observable. / 实现无第三方依赖的热 Observable。</summary>
internal sealed class OneBot12EventStream<TEvent> : IObservable<TEvent>
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
                // A faulty observer must not stop event ingestion or starve sibling observers.
                // 异常观察者不得终止事件接入，也不得阻止其它观察者接收事件。
            }
        }
    }

    private void Unsubscribe(Subscription subscription)
    {
        lock (_gate)
        {
            _subscriptions.Remove(subscription);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly object _gate = new object();
        private OneBot12EventStream<TEvent>? _owner;
        private readonly IObserver<TEvent> _observer;

        internal Subscription(OneBot12EventStream<TEvent> owner, IObserver<TEvent> observer)
        {
            _owner = owner;
            _observer = observer;
        }

        internal void Publish(TEvent value)
        {
            lock (_gate)
            {
                if (_owner != null)
                {
                    _observer.OnNext(value);
                }
            }
        }

        public void Dispose()
        {
            OneBot12EventStream<TEvent>? owner;
            lock (_gate)
            {
                owner = _owner;
                _owner = null;
            }

            owner?.Unsubscribe(this);
        }
    }
}
