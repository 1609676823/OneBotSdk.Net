using System;
using System.Threading.Tasks;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Provides dependency-free lambda subscriptions and concrete event filtering for OneBot observables.
/// 为 OneBot Observable 提供无第三方依赖的 Lambda 订阅和具体事件过滤。
/// </summary>
public static class OneBot10ObservableExtensions
{
    /// <summary>
    /// Subscribes with an on-next callback without requiring System.Reactive.
    /// 使用 on-next 回调订阅，无需引入 System.Reactive。
    /// </summary>
    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
    {
        return Subscribe(source, onNext, null, null);
    }

    /// <summary>
    /// Subscribes with callbacks without requiring System.Reactive.
    /// 使用回调订阅，无需引入 System.Reactive。
    /// </summary>
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

    /// <summary>
    /// Subscribes with an asynchronous callback and routes post-await failures to a required error callback.
    /// 使用异步回调订阅，并将 await 后产生的异常传递给必填的错误回调。
    /// </summary>
    /// <remarks>
    /// Notifications are started without backpressure and may overlap; use application-level serialization when ordering is required.
    /// 通知启动时不施加背压且可能并发执行；需要严格顺序时应在应用层串行化。
    /// </remarks>
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

        return Subscribe(
            source,
            value => _ = InvokeAsync(value, onNextAsync, onError),
            onError,
            onCompleted);
    }

    /// <summary>
    /// Filters an event stream to a concrete OneBot event object while preserving the source lifecycle.
    /// 将事件流过滤为具体 OneBot 事件对象，同时保留源流生命周期。
    /// </summary>
    public static IObservable<TEvent> OfType<TEvent>(this IObservable<OneBot10Event> source)
        where TEvent : OneBot10Event
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return new TypeFilterObservable<TEvent>(source);
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

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
            if (_onError == null)
            {
                // Match the usual observable contract by making an unhandled source failure visible.
                // 遵循常见 Observable 契约，使未处理的源流故障保持可见。
                throw error;
            }

            _onError(error);
        }

        public void OnCompleted()
        {
            _onCompleted?.Invoke();
        }
    }

    private static async Task InvokeAsync<T>(
        T value,
        Func<T, Task> onNextAsync,
        Action<Exception> onError)
    {
        try
        {
            await onNextAsync(value).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // The callback is intentionally observed here so no async-void exception escapes the dispatcher.
            // 此处有意观察异步回调，防止 async-void 异常逃逸出分发器。
            try
            {
                onError(exception);
            }
            catch
            {
                // Error handlers must not turn an already observed callback failure into an unobserved task.
                // 错误处理器不得把已观察的回调故障再次变成未观察任务。
            }
        }
    }

    private sealed class TypeFilterObservable<TEvent> : IObservable<TEvent>
        where TEvent : OneBot10Event
    {
        private readonly IObservable<OneBot10Event> _source;

        internal TypeFilterObservable(IObservable<OneBot10Event> source)
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

        private sealed class TypeFilterObserver : IObserver<OneBot10Event>
        {
            private readonly IObserver<TEvent> _target;

            internal TypeFilterObserver(IObserver<TEvent> target)
            {
                _target = target;
            }

            public void OnNext(OneBot10Event value)
            {
                if (value is TEvent concrete)
                {
                    _target.OnNext(concrete);
                }
            }

            public void OnError(Exception error)
            {
                _target.OnError(error);
            }

            public void OnCompleted()
            {
                _target.OnCompleted();
            }
        }
    }
}
