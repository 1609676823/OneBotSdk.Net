using System;
using OneBotSdk.Net.V12.Transports;

namespace OneBotSdk.Net.V12.Client;

/// <summary>
/// Configures one forward WebSocket event endpoint together with its own token and session settings.
/// 将一个正向 WebSocket 事件终结点与它自己的令牌及会话设置绑定配置。
/// </summary>
public sealed class OneBot12EventEndpointOptions
{
    private Uri _address;
    private string? _accessToken;

    /// <summary>
    /// Creates a <c>ws://host:port/</c> endpoint and associates the token only with this event connection.
    /// 创建 <c>ws://host:port/</c> 终结点，并将令牌仅与当前事件连接关联。
    /// </summary>
    /// <param name="host">The event-server host name or IP address. / 事件服务器主机名或 IP 地址。</param>
    /// <param name="port">The event-server TCP port. / 事件服务器 TCP 端口。</param>
    /// <param name="accessToken">The token used only by this event endpoint. / 仅供当前事件终结点使用的令牌。</param>
    public OneBot12EventEndpointOptions(string host, int port, string? accessToken = null)
        : this(CreateAddress(host, port), accessToken)
    {
    }

    /// <summary>
    /// Initializes a complete absolute WS or WSS event endpoint, including an optional reverse-proxy path.
    /// 初始化完整的绝对 WS 或 WSS 事件终结点，包括可选的反向代理路径。
    /// </summary>
    /// <param name="address">The complete event endpoint. / 完整的事件终结点。</param>
    /// <param name="accessToken">The token used only by this event endpoint. / 仅供当前事件终结点使用的令牌。</param>
    public OneBot12EventEndpointOptions(Uri address, string? accessToken = null)
    {
        _address = OneBot12ForwardWebSocketClientOptions.ValidateEndpoint(address, nameof(address));
        _accessToken = OneBot12ForwardWebSocketClientOptions.ValidateAccessToken(accessToken, nameof(accessToken));
    }

    /// <summary>
    /// Gets or sets the absolute WS/WSS address paired with <see cref="AccessToken"/>.
    /// 获取或设置与 <see cref="AccessToken"/> 配对的绝对 WS/WSS 地址。
    /// </summary>
    public Uri Address
    {
        get => _address;
        set => _address = OneBot12ForwardWebSocketClientOptions.ValidateEndpoint(value, nameof(Address));
    }

    /// <summary>
    /// Gets or sets the exact access token sent only while connecting to <see cref="Address"/>.
    /// The transport uses a Bearer header when possible and otherwise uses the standard query fallback.
    /// This value is never reused for HTTP action requests.
    /// 获取或设置仅在连接 <see cref="Address"/> 时发送的精确访问令牌。
    /// 传输层在可行时使用 Bearer 请求头，否则使用标准查询参数回退。
    /// 此值绝不会自动复用于 HTTP 动作请求。
    /// </summary>
    public string? AccessToken
    {
        get => _accessToken;
        set => _accessToken = OneBot12ForwardWebSocketClientOptions.ValidateAccessToken(value, nameof(AccessToken));
    }

    /// <summary>
    /// Gets the framing, message-size, and keep-alive settings copied when the endpoint connects.
    /// 获取在终结点连接时复制的分帧、消息大小与保活设置。
    /// </summary>
    public OneBot12WebSocketTransportOptions Session { get; } = new OneBot12WebSocketTransportOptions();

    internal OneBot12ForwardWebSocketClientOptions CreateTransportOptions()
    {
        var session = Session.Snapshot();
        var result = new OneBot12ForwardWebSocketClientOptions(Address)
        {
            AccessToken = AccessToken
        };
        result.Session.ReceiveBufferSize = session.ReceiveBufferSize;
        result.Session.MaxMessageBytes = session.MaxMessageBytes;
        result.Session.KeepAliveInterval = session.KeepAliveInterval;
        return result.Snapshot();
    }

    private static Uri CreateAddress(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("An event-server host is required.", nameof(host));
        }

        if (port < 1 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "A TCP port must be between 1 and 65535.");
        }

        try
        {
            return new UriBuilder("ws", host, port, "/").Uri;
        }
        catch (UriFormatException exception)
        {
            throw new ArgumentException("The event-server host is not valid.", nameof(host), exception);
        }
    }
}
