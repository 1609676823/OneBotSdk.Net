using System;
using System.Threading.Tasks;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Provides dependency-free lambda subscriptions and concrete event filtering. / 提供无第三方依赖的 Lambda 订阅和具体事件筛选。</summary>
public static class OneBot12ObservableExtensions
{
    /// <summary>Subscribes with an on-next callback. / 使用 on-next 回调订阅。</summary>
    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
    {
        return Subscribe(source, onNext, null, null);
    }

    /// <summary>Subscribes with all standard callbacks. / 使用全部标准回调订阅。</summary>
    public static IDisposable Subscribe<T>(
        this IObservable<T> source,
        Action<T> onNext,
        Action<Exception>? onError,
        Action? onCompleted)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (onNext == null)
        {
            throw new ArgumentNullException(nameof(onNext));
        }

        return source.Subscribe(new DelegateObserver<T>(onNext, onError, onCompleted));
    }

    /// <summary>Subscribes with an asynchronous callback and observes post-await failures. / 使用异步回调订阅并观察 await 后的失败。</summary>
    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<T, Task> onNextAsync,
        Action<Exception> onError,
        Action? onCompleted = null)
    {
        if (onNextAsync == null)
        {
            throw new ArgumentNullException(nameof(onNextAsync));
        }

        if (onError == null)
        {
            throw new ArgumentNullException(nameof(onError));
        }

        return Subscribe(source, value => _ = InvokeAsync(value, onNextAsync, onError), onError, onCompleted);
    }

    /// <summary>Filters a base event stream to one concrete event type. / 将基础事件流筛选为一种具体事件类型。</summary>
    public static IObservable<TEvent> OfType<TEvent>(this IObservable<OneBot12Event> source)
        where TEvent : OneBot12Event
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return new TypeFilterObservable<TEvent>(source);
    }

    private static async Task InvokeAsync<T>(T value, Func<T, Task> callback, Action<Exception> onError)
    {
        try
        {
            await callback(value).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            try
            {
                onError(exception);
            }
            catch (Exception)
            {
                // Error callbacks are isolated from the already observed asynchronous failure.
                // 错误回调与已经观察到的异步失败相互隔离。
            }
        }
    }

    private sealed class DelegateObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception>? _onError;
        private readonly Action? _onCompleted;

        internal DelegateObserver(Action<T> onNext, Action<Exception>? onError, Action? onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnNext(T value) => _onNext(value);

        public void OnError(Exception error)
        {
            if (_onError == null)
            {
                throw error;
            }

            _onError(error);
        }

        public void OnCompleted() => _onCompleted?.Invoke();
    }

    private sealed class TypeFilterObservable<TEvent> : IObservable<TEvent>
        where TEvent : OneBot12Event
    {
        private readonly IObservable<OneBot12Event> _source;

        internal TypeFilterObservable(IObservable<OneBot12Event> source)
        {
            _source = source;
        }

        public IDisposable Subscribe(IObserver<TEvent> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            return _source.Subscribe(new TypeFilterObserver(observer));
        }

        private sealed class TypeFilterObserver : IObserver<OneBot12Event>
        {
            private readonly IObserver<TEvent> _target;

            internal TypeFilterObserver(IObserver<TEvent> target)
            {
                _target = target;
            }

            public void OnNext(OneBot12Event value)
            {
                if (value is TEvent concrete)
                {
                    _target.OnNext(concrete);
                }
            }

            public void OnError(Exception error) => _target.OnError(error);

            public void OnCompleted() => _target.OnCompleted();
        }
    }
}
