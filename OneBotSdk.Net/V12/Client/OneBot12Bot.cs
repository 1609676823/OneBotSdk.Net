using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Responses;
using OneBotSdk.Net.V12.Transports;
using OneBotSdk.Net.V12.Transports.Http;
using OneBotSdk.Net.V12.Transports.WebSockets;

namespace OneBotSdk.Net.V12.Client;

/// <summary>
/// Configures a OneBot 12 bot through independent action and event endpoints and an optional default identity.
/// 通过彼此独立的动作与事件终结点及可选默认身份配置 OneBot 12 机器人。
/// </summary>
public sealed class OneBot12BotOptions
{
    private readonly OneBot12Self? _defaultSelf;

    /// <summary>
    /// Initializes independently configured endpoints; each endpoint retains its own access token.
    /// 初始化彼此独立的终结点；每个终结点均保留它自己的访问令牌。
    /// </summary>
    /// <param name="actionEndpoint">The HTTP action address, token, and response settings. / HTTP 动作地址、令牌与响应设置。</param>
    /// <param name="eventEndpoint">The WebSocket event address, token, and session settings. / WebSocket 事件地址、令牌与会话设置。</param>
    /// <param name="defaultSelf">The default bot identity for non-meta actions. / 非元动作使用的默认机器人身份。</param>
    public OneBot12BotOptions(
        OneBot12ActionEndpointOptions actionEndpoint,
        OneBot12EventEndpointOptions eventEndpoint,
        OneBot12Self? defaultSelf = null)
    {
        ActionEndpoint = actionEndpoint ?? throw new ArgumentNullException(nameof(actionEndpoint));
        EventEndpoint = eventEndpoint ?? throw new ArgumentNullException(nameof(eventEndpoint));
        _defaultSelf = defaultSelf?.Clone();
    }

    /// <summary>
    /// Initializes endpoints from the lower-level HTTP transport options when direct transport configuration is needed.
    /// 在需要直接配置传输时，使用底层 HTTP 传输选项初始化终结点。
    /// </summary>
    /// <param name="actionEndpoint">The lower-level HTTP action transport options. / 底层 HTTP 动作传输选项。</param>
    /// <param name="eventEndpoint">The WebSocket event endpoint options. / WebSocket 事件终结点选项。</param>
    /// <param name="defaultSelf">The default bot identity for non-meta actions. / 非元动作使用的默认机器人身份。</param>
    public OneBot12BotOptions(
        OneBot12HttpActionTransportOptions actionEndpoint,
        OneBot12EventEndpointOptions eventEndpoint,
        OneBot12Self? defaultSelf = null)
        : this(OneBot12ActionEndpointOptions.FromTransportOptions(actionEndpoint), eventEndpoint, defaultSelf)
    {
    }

    /// <summary>Gets the HTTP action endpoint and the token owned by it. / 获取 HTTP 动作终结点及它拥有的令牌。</summary>
    public OneBot12ActionEndpointOptions ActionEndpoint { get; }

    /// <summary>Gets the WebSocket event endpoint and the token owned by it. / 获取 WebSocket 事件终结点及它拥有的令牌。</summary>
    public OneBot12EventEndpointOptions EventEndpoint { get; }

    /// <summary>Gets a detached default identity for non-meta actions. / 获取用于非元动作的独立默认身份。</summary>
    public OneBot12Self? DefaultSelf => _defaultSelf?.Clone();

    internal OneBot12HttpActionTransportOptions CreateActionOptions() => ActionEndpoint.CreateTransportOptions();

    internal OneBot12ForwardWebSocketClientOptions CreateEventOptions() => EventEndpoint.CreateTransportOptions();
}

/// <summary>
/// Composes an HTTP action client and a forward WebSocket event listener without shared endpoint state.
/// 在不共享终结点状态的前提下组合 HTTP 动作客户端与正向 WebSocket 事件监听器。
/// </summary>
public sealed partial class OneBot12Bot : IDisposable
{
    private readonly Func<CancellationToken, Task>? _startEventConnector;
    private readonly SemaphoreSlim _startGate = new SemaphoreSlim(1, 1);
    private readonly OneBot12Self? _defaultSelf;
    private int _disposed;

    /// <summary>Initializes a bot with internally owned transports. / 使用内部拥有的传输实例初始化机器人。</summary>
    public OneBot12Bot(OneBot12BotOptions options)
        : this(options, null, null, null)
    {
    }

    /// <summary>Initializes a bot with an optional dispatcher and caller-owned HTTP client. / 使用可选分发器及调用方拥有的 HTTP 客户端初始化机器人。</summary>
    /// <remarks>The injected HTTP client is never disposed by this object. / 此对象绝不会释放注入的 HTTP 客户端。</remarks>
    public OneBot12Bot(
        OneBot12BotOptions options,
        OneBot12EventDispatcher? dispatcher,
        HttpClient? httpClient)
        : this(options, dispatcher, httpClient, null)
    {
    }

    internal OneBot12Bot(
        OneBot12BotOptions options,
        OneBot12EventDispatcher? dispatcher,
        HttpClient? httpClient,
        Func<CancellationToken, Task>? startEventConnector)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var actionOptions = options.CreateActionOptions();
        var eventOptions = options.CreateEventOptions();
        var defaultSelf = options.DefaultSelf;
        ActionAddress = actionOptions.Endpoint;
        EventAddress = eventOptions.Endpoint;
        _defaultSelf = defaultSelf?.Clone();
        Events = dispatcher ?? new OneBot12EventDispatcher();
        ActionTransport = new OneBot12HttpActionTransport(actionOptions, httpClient);
        EventTransport = new OneBot12ForwardWebSocketClient(eventOptions, Events);
        Actions = new OneBot12Client(ActionTransport, defaultSelf);
        _startEventConnector = startEventConnector;
    }

    /// <summary>Gets the configured HTTP action address snapshot. / 获取已配置的 HTTP 动作地址快照。</summary>
    public Uri ActionAddress { get; }

    /// <summary>Gets the configured WebSocket event address snapshot. / 获取已配置的 WebSocket 事件地址快照。</summary>
    public Uri EventAddress { get; }

    /// <summary>Gets the detached default bot identity, or null when none was configured. / 获取独立的默认机器人身份；未配置时为 null。</summary>
    public OneBot12Self? DefaultSelf => _defaultSelf?.Clone();

    /// <summary>Gets the strongly typed action facade. / 获取强类型动作门面。</summary>
    public OneBot12Client Actions { get; }

    /// <summary>Gets the categorized EventHandler and Observable dispatcher. / 获取已分类的 EventHandler 与 Observable 分发器。</summary>
    public OneBot12EventDispatcher Events { get; }

    /// <summary>Gets the owned HTTP action transport. / 获取内部拥有的 HTTP 动作传输。</summary>
    public OneBot12HttpActionTransport ActionTransport { get; }

    /// <summary>Gets the owned WebSocket event transport. / 获取内部拥有的 WebSocket 事件传输。</summary>
    public OneBot12ForwardWebSocketClient EventTransport { get; }

    /// <summary>Forwards event-transport failures. / 转发事件传输故障。</summary>
    public event Action<OneBot12TransportException>? Faulted
    {
        add { EventTransport.Faulted += value; }
        remove { EventTransport.Faulted -= value; }
    }

    /// <summary>Forwards event-transport close notifications. / 转发事件传输关闭通知。</summary>
    public event Action<WebSocketCloseStatus?, string?>? Closed
    {
        add { EventTransport.Closed += value; }
        remove { EventTransport.Closed -= value; }
    }

    /// <summary>
    /// Verifies version and status, connects the event endpoint, and returns both meta responses.
    /// 校验版本与状态，连接事件终结点，并返回两个元动作响应。
    /// </summary>
    /// <remarks>
    /// This method blocks the calling thread. Register event subscriptions before startup.
    /// 此方法会阻塞调用线程；请在启动前注册事件订阅。
    /// </remarks>
    /// <param name="cancellationToken">Cancels verification or connection. / 取消校验或连接。</param>
    /// <returns>Both successful, trace-preserving meta responses. / 两个成功且保留追踪信息的元动作响应。</returns>
    public OneBot12BotStartResult Start(CancellationToken cancellationToken = default)
    {
        return StartAsync(cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously verifies <c>get_version</c> and <c>get_status</c> before connecting the event endpoint.
    /// 在连接事件终结点之前，异步校验 <c>get_version</c> 与 <c>get_status</c>。
    /// </summary>
    /// <param name="cancellationToken">Cancels verification or connection. / 取消校验或连接。</param>
    /// <returns>A task containing both successful, trace-preserving meta responses. / 包含两个成功且保留追踪信息的元动作响应任务。</returns>
    public async Task<OneBot12BotStartResult> StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        await _startGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            // Verify both meta endpoints before the event connection can publish its first callback.
            // 在事件连接可以发布首个回调之前，先校验两个元动作。
            var version = await Actions.GetVersionAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!version.IsSuccess)
            {
                throw new OneBot12BotStartException(OneBot12BotStartStage.GetVersion, version);
            }

            var status = await Actions.GetStatusAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!status.IsSuccess)
            {
                throw new OneBot12BotStartException(OneBot12BotStartStage.GetStatus, status);
            }

            if (_startEventConnector == null)
            {
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _startEventConnector(cancellationToken).ConfigureAwait(false);
            }

            return new OneBot12BotStartResult(version, status);
        }
        finally
        {
            _startGate.Release();
        }
    }

    /// <summary>Connects only the independently configured event endpoint without calling an action. / 仅连接独立配置的事件终结点，不调用任何动作。</summary>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return EventTransport.ConnectAsync(cancellationToken);
    }

    /// <summary>Disconnects the event endpoint. / 断开事件终结点连接。</summary>
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return EventTransport.DisconnectAsync(cancellationToken);
    }

    /// <summary>Releases both internally owned transports. / 释放两个内部拥有的传输实例。</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            EventTransport.Dispose();
        }
        finally
        {
            ActionTransport.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(OneBot12Bot));
        }
    }
}
