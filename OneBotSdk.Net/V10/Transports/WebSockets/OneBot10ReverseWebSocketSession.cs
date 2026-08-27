using System;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Events;

namespace OneBotSdk.Net.V10.Transports.WebSockets;

/// <summary>
/// Adapts a host-accepted reverse WebSocket and its handshake metadata into a OneBot session.
/// 将宿主已接受的反向 WebSocket 及其握手元数据适配为 OneBot 会话。
/// </summary>
public sealed class OneBot10ReverseWebSocketSession : IOneBot10ActionTransport, IDisposable
{
    private readonly OneBot10WebSocketSession _session;

    /// <summary>
    /// Initializes a reverse session from a WebSocket accepted by the caller's HTTP server.
    /// 使用调用方 HTTP 服务器接受的 WebSocket 初始化反向会话。
    /// </summary>
    /// <param name="acceptedWebSocket">The already accepted open WebSocket. / 已接受并打开的 WebSocket。</param>
    /// <param name="metadata">Handshake header metadata. / 握手请求头元数据。</param>
    /// <param name="dispatcher">The event dispatcher receiving parsed events. / 接收已解析事件的分发器。</param>
    /// <param name="options">Optional framing and safety options. / 可选的分帧和安全选项。</param>
    /// <param name="expectedAccessToken">
    /// Optional legacy Token-scheme value that must match the handshake Authorization header.
    /// 必须与握手 Authorization 请求头匹配的可选旧版 Token 方案令牌。
    /// </param>
    public OneBot10ReverseWebSocketSession(
        WebSocket acceptedWebSocket,
        OneBot10ReverseWebSocketMetadata metadata,
        OneBot10EventDispatcher dispatcher,
        OneBot10WebSocketTransportOptions? options = null,
        string? expectedAccessToken = null)
    {
        if (acceptedWebSocket == null)
        {
            throw new ArgumentNullException(nameof(acceptedWebSocket));
        }

        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        if (expectedAccessToken != null && expectedAccessToken.Length != 0 && !metadata.HasToken(expectedAccessToken))
        {
            throw new OneBot10TransportException(
                OneBot10TransportError.AuthenticationFailed,
                "The reverse WebSocket Authorization Token value is missing or invalid.");
        }

        _session = new OneBot10WebSocketSession(acceptedWebSocket, dispatcher, options);
    }

    /// <summary>
    /// Gets metadata captured from the accepted handshake.
    /// 获取从已接受握手中保存的元数据。
    /// </summary>
    public OneBot10ReverseWebSocketMetadata Metadata { get; }

    /// <summary>
    /// Gets whether the declared connection role permits action calls.
    /// 获取已声明连接角色是否允许动作调用。
    /// </summary>
    public bool CanSendActions =>
        Metadata.Role == OneBot10ReverseWebSocketRole.Universal ||
        Metadata.Role == OneBot10ReverseWebSocketRole.Api;

    /// <summary>
    /// Gets the underlying session receive-loop task.
    /// 获取底层会话的接收循环任务。
    /// </summary>
    public Task Completion => _session.Completion;

    /// <summary>
    /// Raised when the underlying session reports a transport failure.
    /// 底层会话报告传输失败时触发。
    /// </summary>
    public event Action<OneBot10TransportException>? Faulted
    {
        add { _session.Faulted += value; }
        remove { _session.Faulted -= value; }
    }

    /// <summary>
    /// Raised when the underlying session closes.
    /// 底层会话关闭时触发。
    /// </summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed
    {
        add { _session.Closed += value; }
        remove { _session.Closed -= value; }
    }

    /// <summary>
    /// Starts receiving reverse WebSocket events and responses.
    /// 开始接收反向 WebSocket 事件和响应。
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _session.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<OneBot10ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        if (!CanSendActions)
        {
            throw new OneBot10TransportException(
                OneBot10TransportError.InvalidConfiguration,
                "Only Universal-role and API-role reverse WebSockets can carry OneBot action calls.");
        }

        return _session.SendAsync(action, parameters, echo, cancellationToken);
    }

    /// <summary>
    /// Sends a close frame to the reverse WebSocket peer.
    /// 向反向 WebSocket 对端发送关闭帧。
    /// </summary>
    public Task CloseAsync(
        WebSocketCloseStatus status,
        string? description,
        CancellationToken cancellationToken)
    {
        return _session.CloseAsync(status, description, cancellationToken);
    }

    /// <summary>
    /// Releases the accepted WebSocket and its receive loop.
    /// 释放已接受的 WebSocket 及其接收循环。
    /// </summary>
    public void Dispose()
    {
        _session.Dispose();
    }
}
