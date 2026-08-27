using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Transports.WebSockets;

/// <summary>
/// Runs OneBot 12 event dispatch and action-response correlation over one connected WebSocket.
/// 在一个已连接 WebSocket 上运行 OneBot 12 事件分发与动作响应关联。
/// </summary>
public sealed class OneBot12WebSocketSession : IOneBot12ActionTransport, IDisposable
{
    private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

    private readonly WebSocket _webSocket;
    private readonly OneBot12EventDispatcher _dispatcher;
    private readonly OneBot12WebSocketTransportOptions _options;
    private readonly SemaphoreSlim _sendGate = new SemaphoreSlim(1, 1);
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ReceivedActionResponse>> _pending =
        new ConcurrentDictionary<string, TaskCompletionSource<ReceivedActionResponse>>(StringComparer.Ordinal);
    private readonly object _stateGate = new object();

    private CancellationTokenSource? _lifetime;
    private Task? _completion;
    private int _disposed;
    private int _closedRaised;

    /// <summary>Initializes a session over an already open WebSocket. / 在已打开的 WebSocket 上初始化会话。</summary>
    public OneBot12WebSocketSession(
        WebSocket webSocket,
        OneBot12EventDispatcher dispatcher,
        OneBot12WebSocketTransportOptions? options = null)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _options = (options ?? new OneBot12WebSocketTransportOptions()).Snapshot();
    }

    /// <summary>Occurs when malformed input or transport I/O fails. / 在输入异常或传输 I/O 失败时发生。</summary>
    public event Action<OneBot12TransportException>? Faulted;

    /// <summary>Occurs once when the session closes. / 在会话关闭时触发一次。</summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed;

    /// <summary>Occurs for valid objects that are neither events nor correlated responses. / 在有效对象既非事件也非已关联响应时发生。</summary>
    public event Action<JsonObject>? UnmatchedMessageReceived;

    /// <summary>Gets the underlying WebSocket state. / 获取底层 WebSocket 状态。</summary>
    public WebSocketState State => _webSocket.State;

    /// <summary>Gets the receive-loop task, or a completed task before startup. / 获取接收循环任务；启动前返回已完成任务。</summary>
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

    /// <summary>Starts the session's single receive loop. / 启动会话唯一的接收循环。</summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
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
                throw new OneBot12TransportException(
                    OneBot12TransportError.NotConnected,
                    "The WebSocket must be open before the OneBot 12 session starts.");
            }

            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _completion = ReceiveLoopAsync(_lifetime.Token);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<OneBot12ActionTransportResult> SendAsync(
        string action,
        JsonObject? parameters,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("A OneBot 12 action name is required.", nameof(action));
        }

        EnsureRunningAndOpen();
        cancellationToken.ThrowIfCancellationRequested();

        var requestParameters = TolerantJson.Clone(parameters) as JsonObject ?? new JsonObject();
        var requestSelf = self?.Clone();
        var effectiveEcho = string.IsNullOrEmpty(echo)
            ? "onebotsdk.net:" + Guid.NewGuid().ToString("N")
            : echo!;
        var envelope = new JsonObject
        {
            ["action"] = action,
            ["params"] = TolerantJson.Clone(requestParameters),
            ["echo"] = effectiveEcho
        };
        if (requestSelf != null)
        {
            envelope["self"] = requestSelf.ToJsonObject();
        }

        var rawRequestJson = OneBot12Json.Serialize(envelope);
        var completion = new TaskCompletionSource<ReceivedActionResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(effectiveEcho, completion))
        {
            throw new InvalidOperationException("An action with the same echo value is already pending.");
        }

        using (cancellationToken.Register(() => CancelPending(effectiveEcho)))
        {
            try
            {
                await SendTextAsync(rawRequestJson, cancellationToken).ConfigureAwait(false);
                var received = await completion.Task.ConfigureAwait(false);
                return new OneBot12ActionTransportResult(
                    action,
                    requestParameters,
                    effectiveEcho,
                    requestSelf,
                    rawRequestJson,
                    received.Response,
                    received.RawResponseJson);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (OneBot12TransportException exception)
            {
                exception.Action = action;
                AttachTrace(exception, requestParameters, effectiveEcho, requestSelf, rawRequestJson);
                throw;
            }
            catch (Exception exception)
            {
                var failure = new OneBot12TransportException(
                    OneBot12TransportError.ConnectionFailed,
                    "The OneBot 12 WebSocket action could not be completed.",
                    exception)
                {
                    Action = action
                };
                AttachTrace(failure, requestParameters, effectiveEcho, requestSelf, rawRequestJson);
                throw failure;
            }
            finally
            {
                TaskCompletionSource<ReceivedActionResponse>? ignored;
                _pending.TryRemove(effectiveEcho, out ignored);
            }
        }
    }

    /// <summary>Sends a close frame without starting a competing receive operation. / 发送关闭帧，同时不启动相互竞争的接收操作。</summary>
    public async Task CloseAsync(
        WebSocketCloseStatus closeStatus,
        string? statusDescription,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_webSocket.State != WebSocketState.Open)
        {
            return;
        }

        try
        {
            await _webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.ConnectionFailed,
                "The OneBot 12 WebSocket close frame could not be sent.",
                exception);
        }
    }

    /// <summary>Cancels the receive loop and releases the WebSocket. / 取消接收循环并释放 WebSocket。</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        CancelLifetime();
        TryAbort();
        _webSocket.Dispose();
        CancelAllPending();
        // Do not dispose the send gate: an in-flight send may still release it while disposal cancels the socket.
        // 不释放发送信号量：释放连接并取消套接字时，进行中的发送仍可能归还该信号量。
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
                        FailAllPending(new OneBot12TransportException(
                            OneBot12TransportError.RemoteClosed,
                            "The remote OneBot 12 endpoint closed the WebSocket."));
                        RaiseClosed(result.CloseStatus, result.CloseStatusDescription);
                        return;
                    }

                    if (message.Length + result.Count > _options.MaxMessageBytes)
                    {
                        await TryCloseOutputAsync(
                            WebSocketCloseStatus.MessageTooBig,
                            "OneBot 12 message exceeds the configured limit.",
                            cancellationToken).ConfigureAwait(false);
                        throw new OneBot12TransportException(
                            OneBot12TransportError.MessageTooLarge,
                            "A OneBot 12 WebSocket message exceeded the configured byte limit.");
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
                            "This SDK accepts the mandatory JSON text encoding only.",
                            cancellationToken).ConfigureAwait(false);
                        throw new OneBot12TransportException(
                            OneBot12TransportError.ProtocolViolation,
                            "A OneBot 12 peer sent an unsupported binary WebSocket message.");
                    }

                    var payload = message.ToArray();
                    message.SetLength(0);
                    ProcessMessage(payload);
                }

                FailAllPending(new OneBot12TransportException(
                    OneBot12TransportError.RemoteClosed,
                    "The OneBot 12 WebSocket session stopped before pending actions completed."));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                CancelAllPending();
                TryAbort();
            }
            catch (OneBot12TransportException exception)
            {
                FailAllPending(exception);
                RaiseFaulted(exception);
                throw;
            }
            catch (Exception exception)
            {
                var failure = new OneBot12TransportException(
                    OneBot12TransportError.ConnectionFailed,
                    "The OneBot 12 WebSocket receive loop failed.",
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
        string rawJson;
        JsonObject source;
        try
        {
            rawJson = StrictUtf8.GetString(payload);
            source = JsonNode.Parse(rawJson) as JsonObject ?? throw new InvalidOperationException("The payload is not a JSON object.");
        }
        catch (Exception exception)
        {
            // A malformed frame is isolated so later valid events remain usable.
            // 隔离异常消息帧，使后续有效事件仍可继续处理。
            RaiseFaulted(new OneBot12TransportException(
                OneBot12TransportError.ProtocolViolation,
                "A WebSocket message was not a strict UTF-8 OneBot 12 JSON object.",
                exception));
            return;
        }

        var type = TolerantJson.String(source, "type");
        if (type == OneBot12EventTypes.Meta || type == OneBot12EventTypes.Message ||
            type == OneBot12EventTypes.Notice || type == OneBot12EventTypes.Request)
        {
            try
            {
                _dispatcher.Dispatch(OneBot12EventParser.Parse(source));
            }
            catch (Exception exception)
            {
                RaiseFaulted(new OneBot12TransportException(
                    OneBot12TransportError.ProtocolViolation,
                    "A OneBot 12 event could not be parsed or dispatched.",
                    exception));
            }

            return;
        }

        var echo = ReadStringEcho(source);
        if (echo != null)
        {
            TaskCompletionSource<ReceivedActionResponse>? pending;
            if (_pending.TryRemove(echo, out pending) && pending != null)
            {
                pending.TrySetResult(new ReceivedActionResponse(source, rawJson));
                return;
            }
        }

        RaiseUnmatched(source);
    }

    private async Task SendTextAsync(string text, CancellationToken cancellationToken)
    {
        var payload = StrictUtf8.GetBytes(text);
        if (payload.Length > _options.MaxMessageBytes)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.MessageTooLarge,
                "The OneBot 12 action request exceeded the configured byte limit.");
        }

        await _sendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureRunningAndOpen();
            // WebSocket implementations generally allow only one concurrent send, so all writes share this gate.
            // WebSocket 实现通常只允许一个并发发送，因此所有写入共用此信号量。
            await _webSocket
                .SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (OneBot12TransportException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OneBot12TransportException(
                OneBot12TransportError.ConnectionFailed,
                "The OneBot 12 WebSocket message could not be sent.",
                exception);
        }
        finally
        {
            _sendGate.Release();
        }
    }

    private async Task AcknowledgeRemoteCloseAsync(WebSocketReceiveResult result, CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.CloseReceived)
        {
            await TryCloseOutputAsync(
                result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                result.CloseStatusDescription,
                cancellationToken).ConfigureAwait(false);
        }
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
                throw new OneBot12TransportException(
                    OneBot12TransportError.NotConnected,
                    "The OneBot 12 WebSocket session is not running and open.");
            }
        }
    }

    private static string? ReadStringEcho(JsonObject source)
    {
        try
        {
            return source["echo"]?.GetValue<string>();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void CancelPending(string echo)
    {
        TaskCompletionSource<ReceivedActionResponse>? pending;
        if (_pending.TryRemove(echo, out pending) && pending != null)
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

    private void RaiseFaulted(OneBot12TransportException exception) => InvokeSafely(Faulted, exception);

    private void RaiseUnmatched(JsonObject source) => InvokeSafely(UnmatchedMessageReceived, source);

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
                // Lifecycle observers cannot corrupt transport state.
                // 生命周期观察者不得破坏传输状态。
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
                // Observation callbacks are isolated from the receive loop.
                // 观察回调与接收循环相互隔离。
            }
        }
    }

    private static void AttachTrace(
        OneBot12TransportException exception,
        JsonObject parameters,
        string echo,
        OneBot12Self? self,
        string rawRequestJson)
    {
        exception.RequestParameters = TolerantJson.Clone(parameters) as JsonObject ?? new JsonObject();
        exception.RequestEcho = echo;
        exception.RequestSelf = self?.Clone();
        exception.RawRequestJson = rawRequestJson;
    }

    private void CancelLifetime()
    {
        lock (_stateGate)
        {
            try
            {
                _lifetime?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _lifetime?.Dispose();
            _lifetime = null;
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
            throw new ObjectDisposedException(nameof(OneBot12WebSocketSession));
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
