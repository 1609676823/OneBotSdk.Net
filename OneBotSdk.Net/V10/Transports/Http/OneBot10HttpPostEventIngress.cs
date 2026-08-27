using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Transports.Internal;

namespace OneBotSdk.Net.V10.Transports.Http;

/// <summary>
/// Validates, parses, and dispatches OneBot 10 reverse HTTP POST events without depending on a web framework.
/// 在不依赖 Web 框架的情况下验证、解析并分发 OneBot 10 反向 HTTP POST 事件。
/// </summary>
public sealed class OneBot10HttpPostEventIngress
{
    private readonly OneBot10EventDispatcher _dispatcher;
    private readonly string? _secret;
    private readonly int _maxRequestBodyBytes;

    /// <summary>
    /// Initializes reverse HTTP event ingestion.
    /// 初始化反向 HTTP 事件接入。
    /// </summary>
    public OneBot10HttpPostEventIngress(
        OneBot10EventDispatcher dispatcher,
        OneBot10HttpPostEventIngressOptions? options = null)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        var snapshot = (options ?? new OneBot10HttpPostEventIngressOptions()).Snapshot();
        _secret = string.IsNullOrEmpty(snapshot.Secret) ? null : snapshot.Secret;
        _maxRequestBodyBytes = snapshot.MaxRequestBodyBytes;
    }

    /// <summary>
    /// Reads an exact raw HTTP request body, verifies its signature, and dispatches its event.
    /// 读取精确的原始 HTTP 请求正文、验证签名并分发其中的事件。
    /// </summary>
    public async Task<OneBot10Event> ReadAndDispatchAsync(
        Stream requestBody,
        string? signatureHeader,
        CancellationToken cancellationToken)
    {
        var bytes = await OneBot10TransportPayload
            .ReadBoundedAsync(requestBody, _maxRequestBodyBytes, cancellationToken)
            .ConfigureAwait(false);
        return ParseAndDispatch(bytes, signatureHeader);
    }

    /// <summary>
    /// Verifies and dispatches an event from raw request bytes.
    /// 从原始请求字节验证并分发事件。
    /// </summary>
    public OneBot10Event ParseAndDispatch(byte[] requestBody, string? signatureHeader)
    {
        if (requestBody == null)
        {
            throw new ArgumentNullException(nameof(requestBody));
        }

        if (requestBody.Length > _maxRequestBodyBytes)
        {
            throw OneBot10TransportPayload.TooLarge(_maxRequestBodyBytes);
        }

        VerifyRequiredSignature(requestBody, signatureHeader);
        var source = OneBot10TransportPayload.ParseObject(requestBody);
        var value = OneBot10EventParser.Parse(source);
        _dispatcher.Dispatch(value);
        return value;
    }

    /// <summary>
    /// Dispatches a JSON event string. This overload should be used only when the host does not require signatures.
    /// 分发 JSON 事件字符串；仅应在主机不要求签名时使用此重载。
    /// </summary>
    public OneBot10Event ParseAndDispatch(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        if (_secret != null)
        {
            throw new OneBot10TransportException(
                OneBot10TransportError.AuthenticationFailed,
                "Signed HTTP event ingestion requires the original request bytes and X-Signature header.");
        }

        var bytes = Encoding.UTF8.GetBytes(json);
        return ParseAndDispatch(bytes, null);
    }

    private void VerifyRequiredSignature(byte[] requestBody, string? signatureHeader)
    {
        if (_secret == null)
        {
            return;
        }

        if (!OneBot10HttpPostSignature.Verify(requestBody, signatureHeader, _secret))
        {
            throw new OneBot10TransportException(
                OneBot10TransportError.AuthenticationFailed,
                "The reverse HTTP X-Signature header is missing or invalid.");
        }
    }
}
