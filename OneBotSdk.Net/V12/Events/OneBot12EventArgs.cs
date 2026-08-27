using System;

namespace OneBotSdk.Net.V12.Events;

/// <summary>Wraps a parsed event for the standard EventHandler pattern. / 为标准 EventHandler 模式包装已解析事件。</summary>
public sealed class OneBot12EventArgs<TEvent> : EventArgs
    where TEvent : OneBot12Event
{
    /// <summary>Initializes event arguments. / 初始化事件参数。</summary>
    public OneBot12EventArgs(TEvent @event)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
    }

    /// <summary>Gets the parsed event. / 获取已解析事件。</summary>
    public TEvent Event { get; }
}
