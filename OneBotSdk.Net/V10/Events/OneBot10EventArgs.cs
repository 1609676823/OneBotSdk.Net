using System;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Carries a strongly typed OneBot event through the standard <see cref="EventHandler{TEventArgs}"/> pattern.
/// 通过标准 <see cref="EventHandler{TEventArgs}"/> 模式携带强类型 OneBot 事件。
/// </summary>
/// <typeparam name="TEvent">The concrete or category-level OneBot event type. / 具体或分类级 OneBot 事件类型。</typeparam>
public sealed class OneBot10EventArgs<TEvent> : EventArgs
    where TEvent : OneBot10Event
{
    /// <summary>
    /// Initializes event arguments for one parsed protocol event.
    /// 为一个已解析的协议事件初始化事件参数。
    /// </summary>
    public OneBot10EventArgs(TEvent value)
    {
        Event = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets the parsed OneBot event instance.
    /// 获取已解析的 OneBot 事件实例。
    /// </summary>
    public TEvent Event { get; }
}
