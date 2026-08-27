using System;
using OneBotSdk.Net.V10.Transports;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Configures one forward WebSocket event endpoint together with its own access token and session settings.
/// 将一个正向 WebSocket 事件终结点与它自己的访问令牌和会话设置配置在一起。
/// </summary>
public sealed class OneBot10EventEndpointOptions
{
    private Uri _address;

    /// <summary>
    /// Creates <c>ws://{host}:{port}/event</c> and associates the token only with this event endpoint.
    /// 创建 <c>ws://{host}:{port}/event</c>，并将令牌仅关联到当前事件终结点。
    /// </summary>
    /// <param name="host">The event server host name or IP address. / 事件服务器主机名或 IP 地址。</param>
    /// <param name="port">The event server TCP port. / 事件服务器 TCP 端口。</param>
    /// <param name="accessToken">The token used only by this event endpoint. / 仅供当前事件终结点使用的令牌。</param>
    public OneBot10EventEndpointOptions(string host, int port, string? accessToken = null)
        : this(OneBot10EndpointAddress.CreateWebSocketEvent(host, port), accessToken)
    {
    }

    /// <summary>
    /// Initializes an absolute WS or WSS event endpoint, including an optional reverse-proxy path.
    /// 初始化绝对 WS 或 WSS 事件终结点，并支持可选的反向代理路径。
    /// </summary>
    /// <param name="address">The complete event address, including any reverse-proxy path. / 完整的事件地址，包括反向代理路径。</param>
    /// <param name="accessToken">The token used only by this event endpoint. / 仅供当前事件终结点使用的令牌。</param>
    public OneBot10EventEndpointOptions(Uri address, string? accessToken = null)
    {
        _address = ValidateAddress(address, nameof(address));
        AccessToken = accessToken;
    }

    /// <summary>
    /// Gets or sets the absolute WS/WSS address used for forward event delivery and paired with <see cref="AccessToken"/>.
    /// 获取或设置用于正向事件推送并与 <see cref="AccessToken"/> 配对的绝对 WS/WSS 地址。
    /// </summary>
    public Uri Address
    {
        get => _address;
        set => _address = ValidateAddress(value, nameof(Address));
    }

    /// <summary>
    /// Gets or sets the token sent only while connecting to <see cref="Address"/> as an <c>Authorization: Bearer</c> header.
    /// Null, empty, or whitespace means that no Authorization header is sent.
    /// A non-empty value must be valid for an HTTP Authorization header.
    /// This token is never reused for HTTP action requests.
    /// 获取或设置仅在连接 <see cref="Address"/> 时发送的 <c>Authorization: Bearer</c> 令牌。
    /// 值为 null、空字符串或纯空白时不发送 Authorization 请求头。
    /// 非空值必须符合 HTTP Authorization 请求头格式。
    /// 此令牌绝不会自动复用于 HTTP 动作请求。
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets the framing, message-size, and keep-alive settings used by this event endpoint.
    /// 获取当前事件终结点使用的分帧、消息大小和保活设置。
    /// </summary>
    public OneBot10WebSocketTransportOptions Session { get; } = new OneBot10WebSocketTransportOptions();

    internal OneBot10ForwardWebSocketClientOptions CreateTransportOptions()
    {
        var address = Address ?? throw new ArgumentNullException(nameof(Address));
        var accessToken = AccessToken;
        var session = Session.Snapshot();
        var options = new OneBot10ForwardWebSocketClientOptions(address)
        {
            AccessToken = accessToken
        };
        options.Session.ReceiveBufferSize = session.ReceiveBufferSize;
        options.Session.MaxMessageBytes = session.MaxMessageBytes;
        options.Session.KeepAliveInterval = session.KeepAliveInterval;
        return options.Snapshot();
    }

    private static Uri ValidateAddress(Uri? address, string parameterName)
    {
        var value = address ?? throw new ArgumentNullException(parameterName);
        OneBot10ForwardWebSocketClientOptions.ValidateAbsoluteWebSocketUri(value, parameterName);
        return value;
    }
}
