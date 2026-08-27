using System;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Selects the standard OneBot 11 action invocation suffix.
/// 选择标准 OneBot 11 动作调用后缀。
/// </summary>
public enum InvocationMode
{
    /// <summary>
    /// Invokes the base action and waits for its result.
    /// 调用基础动作并等待其结果。
    /// </summary>
    Normal,

    /// <summary>
    /// Appends <c>_async</c>; the final result is not reported by the protocol.
    /// 追加 <c>_async</c>；协议不会报告最终结果。
    /// </summary>
    Async,

    /// <summary>
    /// Appends <c>_rate_limited</c> and queues the action at the implementation's configured rate.
    /// 追加 <c>_rate_limited</c>，按实现端配置的速率排队执行。
    /// </summary>
    RateLimited
}

internal static class OneBot11ActionName
{
    internal static string ApplyInvocationMode(string action, InvocationMode mode)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("An action name is required.", nameof(action));
        }

        // Invocation modes are mutually exclusive because OneBot 11 defines no combined suffix form.
        // 调用模式互斥，因为 OneBot 11 未定义组合后缀形式。
        switch (mode)
        {
            case InvocationMode.Normal:
                return action;
            case InvocationMode.Async:
                return action + "_async";
            case InvocationMode.RateLimited:
                return action + "_rate_limited";
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown OneBot 11 invocation mode.");
        }
    }
}
