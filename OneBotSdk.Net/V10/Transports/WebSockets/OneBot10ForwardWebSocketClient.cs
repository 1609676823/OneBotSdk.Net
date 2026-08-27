using System;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Events;

namespace OneBotSdk.Net.V10.Transports.WebSockets;

/// <summary>
/// Opens and owns a forward WebSocket connection to a OneBot 10 implementation.
/// 建立并拥有到 OneBot 10 实现端的正向 WebSocket 连接。
/// </summary>
public sealed class OneBot10ForwardWebSocketClient : IOneBot10ActionTransport, IDisposable
{
    private readonly OneBot10ForwardWebSocketClientOptions _options;
    private readonly OneBot10EventDispatcher _dispatcher;
    private readonly object _stateGate = new object();

    private ClientWebSocket? _client;
    private OneBot10WebSocketSession? _session;
    private int _disposed;

    /// <summary>
    /// Initializes a forward WebSocket client.
    /// 初始化正向 WebSocket 客户端。
    /// </summary>
    public OneBot10ForwardWebSocketClient(
        OneBot10ForwardWebSocketClientOptions options,
        OneBot10EventDispatcher dispatcher)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Snapshot();
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// Raised when the active session reports a transport failure.
    /// 当前会话报告传输失败时触发。
    /// </summary>
    public event Action<OneBot10TransportException>? Faulted;

    /// <summary>
    /// Raised when the active session closes.
    /// 当前会话关闭时触发。
    /// </summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed;

    /// <summary>
    /// Gets the active connected session, or <see langword="null"/> before connection.
    /// 获取当前已连接会话；连接前为 <see langword="null"/>。
    /// </summary>
    public OneBot10WebSocketSession? Session
    {
        get
        {
            lock (_stateGate)
            {
                return _session;
            }
        }
    }

    /// <summary>
    /// Opens the configured endpoint and starts its receive loop.
    /// 打开配置的终结点并启动其接收循环。
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        ClientWebSocket client;
        lock (_stateGate)
        {
            if (_session != null &&
                (_session.State == WebSocketState.Open || _session.State == WebSocketState.Connecting))
            {
                throw new InvalidOperationException("The forward WebSocket client is already connected.");
            }

            DisposeConnectionLocked();
            client = new ClientWebSocket();
            client.Options.KeepAliveInterval = _options.Session.KeepAliveInterval;
            if (!string.IsNullOrWhiteSpace(_options.AccessToken))
            {
                client.Options.SetRequestHeader("Authorization", "Bearer " + _options.AccessToken);
            }

            _client = client;
        }

        try
        {
            await client.ConnectAsync(_options.Endpoint, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            CleanupFailedConnection(client);
            throw;
        }
        catch (Exception exception)
        {
            CleanupFailedConnection(client);
            throw new OneBot10TransportException(
                OneBot10TransportError.ConnectionFailed,
                "The forward OneBot WebSocket connection could not be established.",
                exception);
        }

        var session = new OneBot10WebSocketSession(client, _dispatcher, _options.Session);
        session.Faulted += HandleFaulted;
        session.Closed += HandleClosed;
        await session.StartAsync(CancellationToken.None).ConfigureAwait(false);

        lock (_stateGate)
        {
            if (!ReferenceEquals(_client, client) || Volatile.Read(ref _disposed) != 0)
            {
                session.Dispose();
                throw new ObjectDisposedException(nameof(OneBot10ForwardWebSocketClient));
            }

            _session = session;
        }
    }

    /// <inheritdoc />
    public Task<OneBot10ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        OneBot10WebSocketSession? session;
        lock (_stateGate)
        {
            session = _session;
        }

        if (session == null)
        {
            throw new OneBot10TransportException(
                OneBot10TransportError.NotConnected,
                "The forward OneBot WebSocket client is not connected.");
        }

        return session.SendAsync(action, parameters, echo, cancellationToken);
    }

    /// <summary>
    /// Sends a normal close frame. Await <see cref="OneBot10WebSocketSession.Completion"/> when the peer acknowledgement is required.
    /// 发送正常关闭帧；如需等待对端确认，请等待 <see cref="OneBot10WebSocketSession.Completion"/>。
    /// </summary>
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        OneBot10WebSocketSession? session;
        lock (_stateGate)
        {
            session = _session;
        }

        return session == null
            ? Task.CompletedTask
            : session.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting.", cancellationToken);
    }

    /// <summary>
    /// Aborts and releases the active connection.
    /// 中止并释放当前连接。
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        lock (_stateGate)
        {
            DisposeConnectionLocked();
        }
    }

    private void CleanupFailedConnection(ClientWebSocket client)
    {
        lock (_stateGate)
        {
            if (ReferenceEquals(_client, client))
            {
                _client = null;
            }
        }

        client.Dispose();
    }

    private void DisposeConnectionLocked()
    {
        if (_session != null)
        {
            _session.Faulted -= HandleFaulted;
            _session.Closed -= HandleClosed;
            _session.Dispose();
            _session = null;
            _client = null;
            return;
        }

        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }
    }

    private void HandleFaulted(OneBot10TransportException exception)
    {
        var handlers = Faulted;
        if (handlers == null)
        {
            return;
        }

        foreach (Action<OneBot10TransportException> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(exception);
            }
            catch (Exception)
            {
            }
        }
    }

    private void HandleClosed(WebSocketCloseStatus? status, string? description)
    {
        var handlers = Closed;
        if (handlers == null)
        {
            return;
        }

        foreach (Action<WebSocketCloseStatus?, string?> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(status, description);
            }
            catch (Exception)
            {
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(OneBot10ForwardWebSocketClient));
        }
    }
}
