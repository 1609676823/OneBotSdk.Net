using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Transports;

namespace OneBotSdk.Net.V12.Client;

/// <summary>Transports already-formed OneBot 12 action envelopes. / 传输已经构造完成的 OneBot 12 动作信封。</summary>
public interface IOneBot12ActionTransport
{
    /// <summary>Sends an action and returns its exact request/response exchange. / 发送动作并返回精确的请求/响应交互。</summary>
    Task<OneBot12ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken);
}
