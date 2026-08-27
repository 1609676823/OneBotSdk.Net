using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Transports;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Sends already-formed OneBot 10 action requests without prescribing HTTP or WebSocket behavior.
/// 发送已构造的 OneBot 10 动作请求，同时不限定 HTTP 或 WebSocket 行为。
/// </summary>
public interface IOneBot10ActionTransport
{
    /// <summary>
    /// Sends an action and returns the exact request and response exchange.
    /// 发送动作并返回精确的请求与响应交互。
    /// </summary>
    /// <param name="action">The final action name, including any invocation suffix. / 最终动作名，包含调用后缀。</param>
    /// <param name="parameters">The action parameters, or null when absent. / 动作参数；无参数时为 null。</param>
    /// <param name="echo">The optional correlation value. / 可选关联值。</param>
    /// <param name="cancellationToken">A token that cancels transport I/O. / 用于取消传输 I/O 的令牌。</param>
    Task<OneBot10ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        JsonNode? echo,
        CancellationToken cancellationToken);
}
