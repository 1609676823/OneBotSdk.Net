using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Events;

namespace OneBotSdk.Net.V12.Transports.WebSockets;

/// <summary>Opens and owns a forward WebSocket connection to a OneBot 12 implementation. / 建立并拥有到 OneBot 12 实现端的正向 WebSocket 连接。</summary>
public sealed class OneBot12ForwardWebSocketClient : IOneBot12ActionTransport, IDisposable
{
    private readonly OneBot12ForwardWebSocketClientOptions _options;
    private readonly OneBot12EventDispatcher _dispatcher;
    private readonly object _stateGate = new object();
    private ClientWebSocket? _client;
    private OneBot12WebSocketSession? _session;
    private int _disposed;

    /// <summary>Initializes a forward client with an event dispatcher. / 使用事件分发器初始化正向客户端。</summary>
    public OneBot12ForwardWebSocketClient(
        OneBot12ForwardWebSocketClientOptions options,
        OneBot12EventDispatcher dispatcher)
    {
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Snapshot();
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>Occurs when the active session reports a transport failure. / 在当前会话报告传输失败时发生。</summary>
    public event Action<OneBot12TransportException>? Faulted;

    /// <summary>Occurs when the active session closes. / 在当前会话关闭时发生。</summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed;

    /// <summary>Gets the active session, or null before connection. / 获取当前会话；连接前为 null。</summary>
    public OneBot12WebSocketSession? Session
    {
        get
        {
            lock (_stateGate)
            {
                return _session;
            }
        }
    }

    /// <summary>Connects to the configured endpoint and starts receiving. / 连接配置的终结点并开始接收。</summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        ClientWebSocket client;
        lock (_stateGate)
        {
            if (_session != null &&
                (_session.State == WebSocketState.Open || _session.State == WebSocketState.Connecting))
            {
                throw new InvalidOperationException("The OneBot 12 forward WebSocket client is already connected.");
            }

            if (_session == null && _client != null)
            {
                // Reserving _client before asynchronous I/O prevents overlapping connection attempts.
                // 在异步 I/O 前预留 _client，可防止连接尝试重叠。
                throw new InvalidOperationException("The OneBot 12 forward WebSocket client is already connecting.");
            }

            DisposeConnectionLocked();
            client = new ClientWebSocket();
            client.Options.KeepAliveInterval = _options.Session.KeepAliveInterval;
            if (_options.AccessToken != null &&
                OneBot12ForwardWebSocketClientOptions.CanUseAuthorizationHeader(_options.AccessToken))
            {
                client.Options.SetRequestHeader("Authorization", "Bearer " + _options.AccessToken);
            }

            _client = client;
        }

        try
        {
            var endpoint = _options.AccessToken != null &&
                           !OneBot12ForwardWebSocketClientOptions.CanUseAuthorizationHeader(_options.AccessToken)
                ? AddAccessTokenQuery(_options.Endpoint, _options.AccessToken)
                : _options.Endpoint;
            await client.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            CleanupFailedConnection(client);
            throw;
        }
        catch (Exception exception)
        {
            CleanupFailedConnection(client);
            throw new OneBot12TransportException(
                OneBot12TransportError.ConnectionFailed,
                "The forward OneBot 12 WebSocket connection could not be established.",
                exception);
        }

        var session = new OneBot12WebSocketSession(client, _dispatcher, _options.Session);
        session.Faulted += HandleFaulted;
        session.Closed += HandleClosed;
        await session.StartAsync(CancellationToken.None).ConfigureAwait(false);

        lock (_stateGate)
        {
            if (!ReferenceEquals(_client, client) || Volatile.Read(ref _disposed) != 0)
            {
                session.Dispose();
                throw new ObjectDisposedException(nameof(OneBot12ForwardWebSocketClient));
            }

            _session = session;
        }
    }

    /// <inheritdoc />
    public Task<OneBot12ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        OneBot12WebSocketSession? session;
        lock (_stateGate)
        {
            session = _session;
        }

        if (session == null)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.NotConnected,
                "The forward OneBot 12 WebSocket client is not connected.");
        }

        return session.SendAsync(action, parameters, echo, self, cancellationToken);
    }

    /// <summary>Sends a normal close frame. / 发送正常关闭帧。</summary>
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        OneBot12WebSocketSession? session;
        lock (_stateGate)
        {
            session = _session;
        }

        return session == null
            ? Task.CompletedTask
            : session.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting.", cancellationToken);
    }

    /// <summary>Aborts and releases the active connection. / 中止并释放当前连接。</summary>
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
        }
        else if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }
    }

    private void HandleFaulted(OneBot12TransportException exception) => InvokeSafely(Faulted, exception);

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
                // Application lifecycle callbacks are isolated from connection state.
                // 应用生命周期回调与连接状态相互隔离。
            }
        }
    }

    private static void InvokeSafely<T>(Action<T>? handlers, T value)
    {
        if (handlers == null)
        {
            return;
        }

        foreach (Action<T> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(value);
            }
            catch (Exception)
            {
                // A diagnostic callback cannot corrupt the transport.
                // 诊断回调不得破坏传输状态。
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(OneBot12ForwardWebSocketClient));
        }
    }

    private static Uri AddAccessTokenQuery(Uri endpoint, string accessToken)
    {
        var builder = new UriBuilder(endpoint);
        var values = new List<string>();
        var query = builder.Query;
        if (!string.IsNullOrEmpty(query))
        {
            var pairs = query.TrimStart('?').Split('&');
            foreach (var pair in pairs)
            {
                if (pair.Length == 0)
                {
                    continue;
                }

                var separator = pair.IndexOf('=');
                var encodedName = separator < 0 ? pair : pair.Substring(0, separator);
                string name;
                try
                {
                    name = Uri.UnescapeDataString(encodedName);
                }
                catch (UriFormatException)
                {
                    name = encodedName;
                }

                if (!string.Equals(name, "access_token", StringComparison.Ordinal))
                {
                    values.Add(pair);
                }
            }
        }

        values.Add("access_token=" + Uri.EscapeDataString(accessToken));
        builder.Query = string.Join("&", values);
        return builder.Uri;
    }
}
