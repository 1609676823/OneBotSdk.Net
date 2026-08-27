using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Transports.Internal;

namespace OneBotSdk.Net.V11.Transports.WebSockets;

/// <summary>
/// Runs OneBot action correlation and event dispatch over an already connected WebSocket.
/// 在已经连接的 WebSocket 上运行 OneBot 动作关联与事件分发。
/// </summary>
public sealed class OneBot11WebSocketSession : IOneBot11ActionTransport, IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly OneBot11EventDispatcher _dispatcher;
    private readonly OneBot11WebSocketTransportOptions _options;
    private readonly SemaphoreSlim _sendGate = new SemaphoreSlim(1, 1);
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ReceivedActionResponse>> _pending =
        new ConcurrentDictionary<string, TaskCompletionSource<ReceivedActionResponse>>(StringComparer.Ordinal);
    private readonly object _stateGate = new object();

    private CancellationTokenSource? _lifetime;
    private Task? _completion;
    private int _disposed;
    private int _closedRaised;

    /// <summary>
    /// Initializes a session over an open WebSocket supplied by a forward client or server host.
    /// 在正向客户端或服务端宿主提供的已打开 WebSocket 上初始化会话。
    /// </summary>
    public OneBot11WebSocketSession(
        WebSocket webSocket,
        OneBot11EventDispatcher dispatcher,
        OneBot11WebSocketTransportOptions? options = null)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _options = (options ?? new OneBot11WebSocketTransportOptions()).Snapshot();
    }

    /// <summary>
    /// Raised when the receive loop detects a malformed message or transport failure.
    /// 当接收循环检测到异常消息或传输失败时触发。
    /// </summary>
    public event Action<OneBot11TransportException>? Faulted;

    /// <summary>
    /// Raised once when the WebSocket session closes.
    /// WebSocket 会话关闭时触发一次。
    /// </summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed;

    /// <summary>
    /// Raised for valid JSON objects that are neither events nor correlated action responses.
    /// 当有效 JSON 对象既不是事件也不是已关联动作响应时触发。
    /// </summary>
    public event Action<JsonObject>? UnmatchedMessageReceived;

    /// <summary>
    /// Gets the underlying WebSocket state.
    /// 获取底层 WebSocket 状态。
    /// </summary>
    public WebSocketState State => _webSocket.State;

    /// <summary>
    /// Gets the receive-loop task. It is completed before the session is started.
    /// 获取接收循环任务；会话启动前该属性返回已完成任务。
    /// </summary>
    public Task Completion
    {
        get
        {
            lock (_stateGate)
            {
                return _completion ?? Task.CompletedTask;
            }
        }
    }

    /// <summary>
    /// Starts the single receive loop used by this session.
    /// 启动此会话唯一的接收循环。
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        lock (_stateGate)
        {
            if (_completion != null)
            {
                throw new InvalidOperationException("The WebSocket session has already been started.");
            }

            if (_webSocket.State != WebSocketState.Open)
            {
                throw new OneBot11TransportException(
                    OneBot11TransportError.NotConnected,
                    "The WebSocket must be open before the OneBot session is started.");
            }

            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationToken.None);
            _completion = ReceiveLoopAsync(_lifetime.Token);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<OneBot11ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("A OneBot action name is required.", nameof(action));
        }

        EnsureRunningAndOpen();
        cancellationToken.ThrowIfCancellationRequested();

        var requestParameters = OneBot11TransportPayload.Clone(parameters) as JsonObject ?? new JsonObject();
        var effectiveEcho = OneBot11TransportPayload.Clone(echo) ??
                            JsonValue.Create("onebotsdk.net:" + Guid.NewGuid().ToString("N"));
        if (effectiveEcho == null)
        {
            throw new OneBot11TransportException(
                OneBot11TransportError.ProtocolViolation,
                "A WebSocket action requires a non-null echo value.");
        }

        var envelope = new JsonObject
        {
            ["action"] = action,
            ["params"] = OneBot11TransportPayload.Clone(requestParameters) as JsonObject ?? new JsonObject(),
            ["echo"] = OneBot11TransportPayload.Clone(effectiveEcho)
        };
        var rawRequestJson = OneBot11Json.Serialize(envelope);
        var echoKey = OneBot11EchoKey.Create(effectiveEcho);
        var completion = new TaskCompletionSource<ReceivedActionResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(echoKey, completion))
        {
            throw new InvalidOperationException("An action with the same echo value is already pending.");
        }

        using (cancellationToken.Register(() => CancelPending(echoKey)))
        {
            try
            {
                await SendTextAsync(rawRequestJson, cancellationToken).ConfigureAwait(false);
                var received = await completion.Task.ConfigureAwait(false);
                return new OneBot11ActionTransportResult(
                    action,
                    requestParameters,
                    effectiveEcho,
                    rawRequestJson,
                    received.Response,
                    received.RawResponseJson);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (OneBot11TransportException exception)
            {
                exception.Action = action;
                AttachTrace(exception, requestParameters, effectiveEcho, rawRequestJson);
                throw;
            }
            catch (Exception exception)
            {
                var failure = new OneBot11TransportException(
                    OneBot11TransportError.ConnectionFailed,
                    "The WebSocket OneBot action could not be completed.",
                    exception)
                {
                    Action = action
                };
                AttachTrace(failure, requestParameters, effectiveEcho, rawRequestJson);
                throw failure;
            }
            finally
            {
                TaskCompletionSource<ReceivedActionResponse>? ignored;
                _pending.TryRemove(echoKey, out ignored);
            }
        }
    }

    /// <summary>
    /// Sends a close frame without starting a competing receive operation.
    /// 发送关闭帧，同时避免启动与现有接收循环冲突的第二个接收操作。
    /// </summary>
    public async Task CloseAsync(
        WebSocketCloseStatus closeStatus,
        string? statusDescription,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket
                    .CloseOutputAsync(closeStatus, statusDescription, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new OneBot11TransportException(
                    OneBot11TransportError.ConnectionFailed,
                    "The WebSocket close frame could not be sent.",
                    exception);
            }
        }
    }

    /// <summary>
    /// Cancels the receive loop and releases the WebSocket.
    /// 取消接收循环并释放 WebSocket。
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        CancelLifetime();
        try
        {
            _webSocket.Abort();
        }
        catch (Exception)
        {
            // Disposal is best-effort after cancellation.
            // 取消后的释放过程按尽力而为处理。
        }

        _webSocket.Dispose();
        CancelAllPending();
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var receiveBuffer = new byte[_options.ReceiveBufferSize];
        using (var message = new MemoryStream())
        {
            try
            {
                while (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseSent)
                {
                    var result = await _webSocket
                        .ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken)
                        .ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await AcknowledgeRemoteCloseAsync(result, cancellationToken).ConfigureAwait(false);
                        FailAllPending(new OneBot11TransportException(
                            OneBot11TransportError.RemoteClosed,
                            "The remote OneBot WebSocket endpoint closed the connection."));
                        RaiseClosed(result.CloseStatus, result.CloseStatusDescription);
                        return;
                    }

                    if (message.Length + result.Count > _options.MaxMessageBytes)
                    {
                        await TryCloseOutputAsync(
                            WebSocketCloseStatus.MessageTooBig,
                            "OneBot message exceeds the configured limit.",
                            cancellationToken).ConfigureAwait(false);
                        throw OneBot11TransportPayload.TooLarge(_options.MaxMessageBytes);
                    }

                    message.Write(receiveBuffer, 0, result.Count);
                    if (!result.EndOfMessage)
                    {
                        continue;
                    }

                    if (result.MessageType != WebSocketMessageType.Text)
                    {
                        message.SetLength(0);
                        await TryCloseOutputAsync(
                            WebSocketCloseStatus.InvalidMessageType,
                            "OneBot transports accept JSON text messages only.",
                            cancellationToken).ConfigureAwait(false);
                        throw new OneBot11TransportException(
                            OneBot11TransportError.ProtocolViolation,
                            "A OneBot WebSocket peer sent a non-text message.");
                    }

                    var payload = message.ToArray();
                    message.SetLength(0);
                    ProcessMessage(payload);
                }

                FailAllPending(new OneBot11TransportException(
                    OneBot11TransportError.RemoteClosed,
                    "The OneBot WebSocket session stopped before pending actions completed."));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                CancelAllPending();
                TryAbort();
            }
            catch (OneBot11TransportException exception)
            {
                FailAllPending(exception);
                RaiseFaulted(exception);
                throw;
            }
            catch (Exception exception)
            {
                var failure = new OneBot11TransportException(
                    OneBot11TransportError.ConnectionFailed,
                    "The OneBot WebSocket receive loop failed.",
                    exception);
                FailAllPending(failure);
                RaiseFaulted(failure);
                throw failure;
            }
            finally
            {
                RaiseClosed(_webSocket.CloseStatus, _webSocket.CloseStatusDescription);
            }
        }
    }

    private void ProcessMessage(byte[] payload)
    {
        JsonObject source;
        string rawResponseJson;
        try
        {
            rawResponseJson = OneBot11TransportPayload.DecodeUtf8(payload);
            source = OneBot11TransportPayload.ParseObject(rawResponseJson);
        }
        catch (OneBot11TransportException exception)
        {
            // A malformed frame is isolated so later valid events remain usable.
            // 隔离异常消息帧，使后续有效事件仍可继续处理。
            RaiseFaulted(exception);
            return;
        }

        JsonNode? postType;
        if (source.TryGetPropertyValue("post_type", out postType) && postType != null)
        {
            try
            {
                _dispatcher.Dispatch(OneBot11EventParser.Parse(source));
            }
            catch (Exception exception)
            {
                RaiseFaulted(new OneBot11TransportException(
                    OneBot11TransportError.ProtocolViolation,
                    "A OneBot event could not be parsed or dispatched.",
                    exception));
            }

            return;
        }

        JsonNode? echo;
        if (source.TryGetPropertyValue("echo", out echo) && echo != null)
        {
            TaskCompletionSource<ReceivedActionResponse>? pending;
            if (_pending.TryRemove(OneBot11EchoKey.Create(echo), out pending) && pending != null)
            {
                pending.TrySetResult(new ReceivedActionResponse(source, rawResponseJson));
                return;
            }
        }

        RaiseUnmatched(source);
    }

    private async Task SendTextAsync(string text, CancellationToken cancellationToken)
    {
        var payload = Encoding.UTF8.GetBytes(text);
        if (payload.Length > _options.MaxMessageBytes)
        {
            throw OneBot11TransportPayload.TooLarge(_options.MaxMessageBytes);
        }

        await _sendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureRunningAndOpen();
            // ClientWebSocket supports only one send and one receive in parallel; all sends share this gate.
            // ClientWebSocket 仅支持各一个并行发送与接收；所有发送都通过此信号量串行化。
            await _webSocket
                .SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (OneBot11TransportException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OneBot11TransportException(
                OneBot11TransportError.ConnectionFailed,
                "The OneBot WebSocket message could not be sent.",
                exception);
        }
        finally
        {
            _sendGate.Release();
        }
    }

    private async Task AcknowledgeRemoteCloseAsync(WebSocketReceiveResult result, CancellationToken cancellationToken)
    {
        if (_webSocket.State != WebSocketState.CloseReceived)
        {
            return;
        }

        await TryCloseOutputAsync(
            result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
            result.CloseStatusDescription,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task TryCloseOutputAsync(
        WebSocketCloseStatus status,
        string? description,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
            {
                await _webSocket.CloseOutputAsync(status, description, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            TryAbort();
        }
    }

    private void EnsureRunningAndOpen()
    {
        lock (_stateGate)
        {
            if (_completion == null || _webSocket.State != WebSocketState.Open)
            {
                throw new OneBot11TransportException(
                    OneBot11TransportError.NotConnected,
                    "The OneBot WebSocket session is not running and open.");
            }
        }
    }

    private void CancelPending(string echoKey)
    {
        TaskCompletionSource<ReceivedActionResponse>? pending;
        if (_pending.TryRemove(echoKey, out pending) && pending != null)
        {
            pending.TrySetCanceled();
        }
    }

    private void CancelAllPending()
    {
        foreach (var pair in _pending)
        {
            TaskCompletionSource<ReceivedActionResponse>? pending;
            if (_pending.TryRemove(pair.Key, out pending) && pending != null)
            {
                pending.TrySetCanceled();
            }
        }
    }

    private void FailAllPending(Exception exception)
    {
        foreach (var pair in _pending)
        {
            TaskCompletionSource<ReceivedActionResponse>? pending;
            if (_pending.TryRemove(pair.Key, out pending) && pending != null)
            {
                pending.TrySetException(exception);
            }
        }
    }

    private void RaiseFaulted(OneBot11TransportException exception)
    {
        InvokeSafely(Faulted, exception);
    }

    private void RaiseClosed(WebSocketCloseStatus? status, string? description)
    {
        if (Interlocked.Exchange(ref _closedRaised, 1) != 0)
        {
            return;
        }

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
                // A lifecycle observer must not corrupt transport state.
                // 生命周期观察者不得破坏传输状态。
            }
        }
    }

    private void RaiseUnmatched(JsonObject source)
    {
        InvokeSafely(UnmatchedMessageReceived, source);
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
                // Transport observation callbacks are isolated from the receive loop.
                // 将传输观察回调与接收循环相互隔离。
            }
        }
    }

    private static void AttachTrace(
        OneBot11TransportException exception,
        JsonObject requestParameters,
        JsonNode requestEcho,
        string rawRequestJson)
    {
        exception.RequestParameters = OneBot11TransportPayload.Clone(requestParameters) as JsonObject ?? new JsonObject();
        exception.RequestEcho = OneBot11TransportPayload.Clone(requestEcho);
        exception.RawRequestJson = rawRequestJson;
    }

    private void CancelLifetime()
    {
        lock (_stateGate)
        {
            if (_lifetime == null)
            {
                return;
            }

            try
            {
                _lifetime.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private void TryAbort()
    {
        try
        {
            _webSocket.Abort();
        }
        catch (Exception)
        {
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(OneBot11WebSocketSession));
        }
    }

    private sealed class ReceivedActionResponse
    {
        internal ReceivedActionResponse(JsonObject response, string rawResponseJson)
        {
            Response = response;
            RawResponseJson = rawResponseJson;
        }

        internal JsonObject Response { get; }

        internal string RawResponseJson { get; }
    }
}
