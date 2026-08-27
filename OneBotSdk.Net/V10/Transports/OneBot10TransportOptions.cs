using System;
using System.Net.Http.Headers;

namespace OneBotSdk.Net.V10.Transports;

/// <summary>
/// Configures OneBot action calls made over HTTP.
/// 配置通过 HTTP 发起的 OneBot 动作调用。
/// </summary>
public sealed class OneBot10HttpActionTransportOptions
{
    /// <summary>
    /// Initializes HTTP action transport options.
    /// 初始化 HTTP 动作传输选项。
    /// </summary>
    public OneBot10HttpActionTransportOptions(Uri baseUri)
    {
        BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
    }

    /// <summary>
    /// Gets or sets the HTTP API base URI, for example <c>http://127.0.0.1:3000/</c>.
    /// 获取或设置 HTTP API 基础 URI，例如 <c>http://127.0.0.1:3000/</c>。
    /// </summary>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the token sent only to <see cref="BaseUri"/> as an Authorization Bearer header.
    /// Null, empty, or whitespace means that no Authorization header is sent.
    /// A non-empty value must be valid for an HTTP Authorization header.
    /// 获取或设置仅向 <see cref="BaseUri"/> 发送的 Authorization Bearer 令牌。
    /// 值为 null、空字符串或纯空白时不发送 Authorization 请求头。
    /// 非空值必须符合 HTTP Authorization 请求头格式。
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the largest accepted HTTP response body in bytes.
    /// 获取或设置允许接收的最大 HTTP 响应正文大小（字节）。
    /// </summary>
    public int MaxResponseBodyBytes { get; set; } = 4 * 1024 * 1024;

    internal OneBot10HttpActionTransportOptions Snapshot()
    {
        var baseUri = BaseUri ?? throw new ArgumentNullException(nameof(BaseUri));
        var accessToken = NormalizeAccessToken(AccessToken, nameof(AccessToken));
        var maxResponseBodyBytes = MaxResponseBodyBytes;
        ValidateAbsoluteHttpUri(baseUri, nameof(BaseUri));
        ValidatePositive(maxResponseBodyBytes, nameof(MaxResponseBodyBytes));

        return new OneBot10HttpActionTransportOptions(baseUri)
        {
            AccessToken = accessToken,
            MaxResponseBodyBytes = maxResponseBodyBytes
        };
    }

    internal static void ValidateAbsoluteHttpUri(Uri value, string parameterName)
    {
        if (!value.IsAbsoluteUri ||
            (!string.Equals(value.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(value.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The endpoint must be an absolute HTTP or HTTPS URI.", parameterName);
        }

        if (string.IsNullOrEmpty(value.Host) || value.Port < 1 || value.Port > 65535)
        {
            throw new ArgumentException("The endpoint must contain a host and a port between 1 and 65535.", parameterName);
        }

        if (!string.IsNullOrEmpty(value.Query) || !string.IsNullOrEmpty(value.Fragment))
        {
            throw new ArgumentException("An HTTP action base URI cannot contain a query string or fragment.", parameterName);
        }
    }

    internal static void ValidatePositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "The value must be greater than zero.");
        }
    }

    internal static string? NormalizeAccessToken(string? value, string parameterName)
    {
        if (value == null || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Reject line breaks before any transport can turn the token into an HTTP handshake header.
        // 在任何传输把令牌写入 HTTP 握手请求头之前拒绝换行符，防止请求头注入。
        if (value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0)
        {
            throw new ArgumentException("The access token cannot contain carriage-return or line-feed characters.", parameterName);
        }

        try
        {
            _ = new AuthenticationHeaderValue("Bearer", value);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("The access token must be a valid Authorization Bearer value.", parameterName, exception);
        }

        return value;
    }
}

/// <summary>
/// Configures framing and safety limits shared by WebSocket sessions.
/// 配置 WebSocket 会话共享的分帧参数和安全限制。
/// </summary>
public sealed class OneBot10WebSocketTransportOptions
{
    /// <summary>
    /// Gets or sets the receive buffer size used for each WebSocket fragment.
    /// 获取或设置每个 WebSocket 分片使用的接收缓冲区大小。
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 16 * 1024;

    /// <summary>
    /// Gets or sets the largest complete WebSocket message accepted by the SDK.
    /// 获取或设置 SDK 允许接收的最大完整 WebSocket 消息大小。
    /// </summary>
    public int MaxMessageBytes { get; set; } = 4 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the keep-alive interval used by a forward <see cref="System.Net.WebSockets.ClientWebSocket"/>.
    /// 获取或设置正向 <see cref="System.Net.WebSockets.ClientWebSocket"/> 使用的保活间隔。
    /// </summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(30);

    internal OneBot10WebSocketTransportOptions Snapshot()
    {
        var receiveBufferSize = ReceiveBufferSize;
        var maxMessageBytes = MaxMessageBytes;
        var keepAliveInterval = KeepAliveInterval;
        OneBot10HttpActionTransportOptions.ValidatePositive(receiveBufferSize, nameof(ReceiveBufferSize));
        OneBot10HttpActionTransportOptions.ValidatePositive(maxMessageBytes, nameof(MaxMessageBytes));

        if (receiveBufferSize > maxMessageBytes)
        {
            throw new ArgumentException("ReceiveBufferSize cannot be larger than MaxMessageBytes.");
        }

        if (keepAliveInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(KeepAliveInterval), "The interval cannot be negative.");
        }

        return new OneBot10WebSocketTransportOptions
        {
            ReceiveBufferSize = receiveBufferSize,
            MaxMessageBytes = maxMessageBytes,
            KeepAliveInterval = keepAliveInterval
        };
    }
}

/// <summary>
/// Configures a forward WebSocket connection to a OneBot implementation.
/// 配置到 OneBot 实现端的正向 WebSocket 连接。
/// </summary>
public sealed class OneBot10ForwardWebSocketClientOptions
{
    /// <summary>
    /// Initializes forward WebSocket client options.
    /// 初始化正向 WebSocket 客户端选项。
    /// </summary>
    public OneBot10ForwardWebSocketClientOptions(Uri endpoint)
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    /// <summary>
    /// Gets or sets the forward WebSocket endpoint.
    /// 获取或设置正向 WebSocket 终结点。
    /// </summary>
    public Uri Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the token sent only while connecting to <see cref="Endpoint"/> as an Authorization Bearer header.
    /// Null, empty, or whitespace means that no Authorization header is sent.
    /// A non-empty value must be valid for an HTTP Authorization header.
    /// 获取或设置仅在连接 <see cref="Endpoint"/> 时发送的 Authorization Bearer 令牌。
    /// 值为 null、空字符串或纯空白时不发送 Authorization 请求头。
    /// 非空值必须符合 HTTP Authorization 请求头格式。
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets the mutable session options copied when a connection is opened.
    /// 获取在建立连接时复制的可变会话选项。
    /// </summary>
    public OneBot10WebSocketTransportOptions Session { get; } = new OneBot10WebSocketTransportOptions();

    internal OneBot10ForwardWebSocketClientOptions Snapshot()
    {
        var endpoint = Endpoint ?? throw new ArgumentNullException(nameof(Endpoint));
        var accessToken = OneBot10HttpActionTransportOptions.NormalizeAccessToken(AccessToken, nameof(AccessToken));
        ValidateAbsoluteWebSocketUri(endpoint, nameof(Endpoint));
        var session = Session.Snapshot();
        var copy = new OneBot10ForwardWebSocketClientOptions(endpoint)
        {
            AccessToken = accessToken
        };
        copy.Session.ReceiveBufferSize = session.ReceiveBufferSize;
        copy.Session.MaxMessageBytes = session.MaxMessageBytes;
        copy.Session.KeepAliveInterval = session.KeepAliveInterval;
        return copy;
    }

    internal static void ValidateAbsoluteWebSocketUri(Uri endpoint, string parameterName)
    {
        if (!endpoint.IsAbsoluteUri ||
            (!string.Equals(endpoint.Scheme, "ws", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(endpoint.Scheme, "wss", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The endpoint must be an absolute WS or WSS URI.", parameterName);
        }

        if (string.IsNullOrEmpty(endpoint.Host) || endpoint.Port < 1 || endpoint.Port > 65535)
        {
            throw new ArgumentException("The endpoint must contain a host and a port between 1 and 65535.", parameterName);
        }

        if (!string.IsNullOrEmpty(endpoint.Fragment))
        {
            throw new ArgumentException("A WebSocket endpoint cannot contain a fragment.", parameterName);
        }
    }
}

/// <summary>
/// Configures reverse HTTP POST event ingestion.
/// 配置反向 HTTP POST 事件接入。
/// </summary>
public sealed class OneBot10HttpPostEventIngressOptions
{
    /// <summary>
    /// Gets or sets the secret used to verify <c>X-Signature</c>.
    /// When set, every inbound request must contain a valid signature.
    /// 获取或设置用于验证 <c>X-Signature</c> 的密钥；设置后每个入站请求都必须包含有效签名。
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Gets or sets the largest accepted request body in bytes.
    /// 获取或设置允许接收的最大请求正文大小（字节）。
    /// </summary>
    public int MaxRequestBodyBytes { get; set; } = 4 * 1024 * 1024;

    internal OneBot10HttpPostEventIngressOptions Snapshot()
    {
        OneBot10HttpActionTransportOptions.ValidatePositive(MaxRequestBodyBytes, nameof(MaxRequestBodyBytes));
        return new OneBot10HttpPostEventIngressOptions
        {
            Secret = Secret,
            MaxRequestBodyBytes = MaxRequestBodyBytes
        };
    }
}
