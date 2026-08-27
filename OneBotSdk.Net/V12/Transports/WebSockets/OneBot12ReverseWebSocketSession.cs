using System;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Events;

namespace OneBotSdk.Net.V12.Transports.WebSockets;

/// <summary>Adapts a host-accepted reverse WebSocket into a bidirectional OneBot 12 session. / 将宿主已接受的反向 WebSocket 适配为双向 OneBot 12 会话。</summary>
public sealed class OneBot12ReverseWebSocketSession : IOneBot12ActionTransport, IDisposable
{
    private readonly OneBot12WebSocketSession _session;

    /// <summary>Initializes and validates a reverse WebSocket session. / 初始化并校验反向 WebSocket 会话。</summary>
    public OneBot12ReverseWebSocketSession(
        WebSocket acceptedWebSocket,
        OneBot12ReverseWebSocketMetadata metadata,
        OneBot12EventDispatcher dispatcher,
        OneBot12WebSocketTransportOptions? options = null,
        string? expectedAccessToken = null,
        bool requireProtocolMetadata = true)
    {
        if (acceptedWebSocket == null)
        {
            throw new ArgumentNullException(nameof(acceptedWebSocket));
        }

        var validatedMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Metadata = validatedMetadata;
        if (requireProtocolMetadata &&
            (string.IsNullOrWhiteSpace(validatedMetadata.UserAgent) ||
             !string.Equals(validatedMetadata.OneBotVersion, "12", StringComparison.Ordinal) ||
             !validatedMetadata.HasValidImplementationName()))
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.ProtocolViolation,
                "The reverse WebSocket must declare User-Agent and Sec-WebSocket-Protocol: 12.<impl>.");
        }

        if (expectedAccessToken != null && expectedAccessToken.Length != 0 &&
            !validatedMetadata.HasAccessToken(expectedAccessToken))
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.AuthenticationFailed,
                "The reverse WebSocket access token is missing or invalid.");
        }

        _session = new OneBot12WebSocketSession(acceptedWebSocket, dispatcher, options);
    }

    /// <summary>Gets the captured handshake metadata. / 获取已捕获的握手元数据。</summary>
    public OneBot12ReverseWebSocketMetadata Metadata { get; }

    /// <summary>Gets the underlying receive-loop task. / 获取底层接收循环任务。</summary>
    public Task Completion => _session.Completion;

    /// <summary>Occurs when the underlying session faults. / 在底层会话发生故障时发生。</summary>
    public event Action<OneBot12TransportException>? Faulted
    {
        add { _session.Faulted += value; }
        remove { _session.Faulted -= value; }
    }

    /// <summary>Occurs when the underlying session closes. / 在底层会话关闭时发生。</summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed
    {
        add { _session.Closed += value; }
        remove { _session.Closed -= value; }
    }

    /// <summary>Starts receiving events and action responses. / 开始接收事件与动作响应。</summary>
    public Task StartAsync(CancellationToken cancellationToken = default) => _session.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task<OneBot12ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken)
    {
        return _session.SendAsync(action, parameters, echo, self, cancellationToken);
    }

    /// <summary>Sends a close frame to the implementation peer. / 向实现端对端发送关闭帧。</summary>
    public Task CloseAsync(
        WebSocketCloseStatus status,
        string? description,
        CancellationToken cancellationToken = default)
    {
        return _session.CloseAsync(status, description, cancellationToken);
    }

    /// <summary>Releases the accepted socket and receive loop. / 释放已接受的套接字与接收循环。</summary>
    public void Dispose() => _session.Dispose();
}
