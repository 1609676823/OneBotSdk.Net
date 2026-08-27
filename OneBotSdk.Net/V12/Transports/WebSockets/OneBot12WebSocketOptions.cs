using System;
using System.Net.Http.Headers;

namespace OneBotSdk.Net.V12.Transports;

/// <summary>Configures framing and safety limits shared by OneBot 12 WebSocket sessions. / 配置 OneBot 12 WebSocket 会话共享的分帧参数和安全限制。</summary>
public sealed class OneBot12WebSocketTransportOptions
{
    /// <summary>Gets or sets the receive-buffer size for each frame fragment. / 获取或设置每个消息帧分片的接收缓冲区大小。</summary>
    public int ReceiveBufferSize { get; set; } = 16 * 1024;

    /// <summary>Gets or sets the largest complete message accepted by the SDK. / 获取或设置 SDK 接受的最大完整消息大小。</summary>
    public int MaxMessageBytes { get; set; } = 4 * 1024 * 1024;

    /// <summary>Gets or sets the forward-client keep-alive interval. / 获取或设置正向客户端保活间隔。</summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(30);

    internal OneBot12WebSocketTransportOptions Snapshot()
    {
        if (ReceiveBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ReceiveBufferSize));
        }

        if (MaxMessageBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxMessageBytes));
        }

        if (ReceiveBufferSize > MaxMessageBytes)
        {
            throw new ArgumentException("ReceiveBufferSize cannot be larger than MaxMessageBytes.");
        }

        if (KeepAliveInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(KeepAliveInterval));
        }

        return new OneBot12WebSocketTransportOptions
        {
            ReceiveBufferSize = ReceiveBufferSize,
            MaxMessageBytes = MaxMessageBytes,
            KeepAliveInterval = KeepAliveInterval
        };
    }
}

/// <summary>Configures a forward WebSocket connection to a OneBot 12 implementation. / 配置到 OneBot 12 实现端的正向 WebSocket 连接。</summary>
public sealed class OneBot12ForwardWebSocketClientOptions
{
    private Uri _endpoint;
    private string? _accessToken;

    /// <summary>Initializes options for an absolute WS or WSS endpoint. / 使用绝对 WS 或 WSS 终结点初始化选项。</summary>
    public OneBot12ForwardWebSocketClientOptions(Uri endpoint)
    {
        _endpoint = ValidateEndpoint(endpoint, nameof(endpoint));
    }

    /// <summary>Gets or sets the complete forward WebSocket endpoint. / 获取或设置完整正向 WebSocket 终结点。</summary>
    public Uri Endpoint
    {
        get => _endpoint;
        set => _endpoint = ValidateEndpoint(value, nameof(Endpoint));
    }

    /// <summary>Gets or sets the exact token owned by this endpoint; the transport selects Bearer or query fallback. / 获取或设置当前终结点独立拥有的精确令牌；传输层会选择 Bearer 或查询参数回退。</summary>
    public string? AccessToken
    {
        get => _accessToken;
        set => _accessToken = ValidateAccessToken(value, nameof(AccessToken));
    }

    /// <summary>Gets the mutable session settings copied when a connection opens. / 获取建立连接时复制的可变会话设置。</summary>
    public OneBot12WebSocketTransportOptions Session { get; } = new OneBot12WebSocketTransportOptions();

    internal OneBot12ForwardWebSocketClientOptions Snapshot()
    {
        var session = Session.Snapshot();
        var result = new OneBot12ForwardWebSocketClientOptions(Endpoint)
        {
            AccessToken = AccessToken
        };
        result.Session.ReceiveBufferSize = session.ReceiveBufferSize;
        result.Session.MaxMessageBytes = session.MaxMessageBytes;
        result.Session.KeepAliveInterval = session.KeepAliveInterval;
        return result;
    }

    internal static Uri ValidateEndpoint(Uri? endpoint, string parameterName)
    {
        if (endpoint == null || !endpoint.IsAbsoluteUri ||
            (!string.Equals(endpoint.Scheme, "ws", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(endpoint.Scheme, "wss", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The endpoint must be an absolute WS or WSS URI.", parameterName);
        }

        if (string.IsNullOrEmpty(endpoint.Host) || endpoint.Port < 1 || endpoint.Port > 65535 ||
            !string.IsNullOrEmpty(endpoint.Fragment))
        {
            throw new ArgumentException("The endpoint must contain a valid host and TCP port without a fragment.", parameterName);
        }

        return endpoint;
    }

    internal static string? ValidateAccessToken(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Preserve non-empty tokens exactly; the client uses the official query fallback when needed.
        // 原样保留非空令牌；客户端会在需要时使用官方查询参数回退。
        _ = parameterName;
        return value;
    }

    internal static bool CanUseAuthorizationHeader(string value)
    {
        try
        {
            _ = new AuthenticationHeaderValue("Bearer", value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
