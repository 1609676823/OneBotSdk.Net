using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Responses;
using OneBotSdk.Net.V11.Transports;
using OneBotSdk.Net.V11.Transports.Http;
using OneBotSdk.Net.V11.Transports.WebSockets;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Configures a OneBot 11 bot through a fixed pair of transports:
/// an HTTP (directionally, forward HTTP) action endpoint and a forward WebSocket event endpoint.
/// Each endpoint owns its address, access token, and transport-specific settings.
/// No shared token or cross-endpoint token fallback exists.
/// 通过一组固定的正向传输配置 OneBot 11 机器人：HTTP 动作终结点与正向 WebSocket 事件终结点。
/// 每个终结点都拥有自己的地址、访问令牌和传输专用设置。
/// 不存在共享令牌或跨终结点令牌回退。
/// </summary>
/// <remarks>
/// This high-level composition does not select or host reverse HTTP or reverse WebSocket transports.
/// 此高层组合不选择也不托管反向 HTTP 或反向 WebSocket 传输。
/// </remarks>
public sealed class OneBot11BotOptions
{
    /// <summary>
    /// Initializes independently configured action and event endpoints.
    /// The bot snapshots both endpoint configurations when it is constructed.
    /// 初始化相互独立配置的动作与事件终结点。
    /// 机器人在构造时会为两个终结点配置创建快照。
    /// </summary>
    /// <param name="actionEndpoint">The HTTP (directionally, forward HTTP) action address, token, and response settings. / HTTP（按方向即正向 HTTP）动作地址、令牌和响应设置。</param>
    /// <param name="eventEndpoint">The forward WebSocket event address, token, and session settings. / 正向 WebSocket 事件地址、令牌和会话设置。</param>
    /// <remarks>
    /// Changes to these option objects after constructing a bot affect only bots constructed later.
    /// 机器人构造完成后再修改这些选项对象，只会影响之后新建的机器人。
    /// </remarks>
    public OneBot11BotOptions(
        OneBot11ActionEndpointOptions actionEndpoint,
        OneBot11EventEndpointOptions eventEndpoint)
    {
        ActionEndpoint = actionEndpoint ?? throw new ArgumentNullException(nameof(actionEndpoint));
        EventEndpoint = eventEndpoint ?? throw new ArgumentNullException(nameof(eventEndpoint));
    }

    /// <summary>
    /// Gets the HTTP (directionally, forward HTTP) action endpoint that owns its address, token, and response settings.
    /// 获取拥有自身地址、令牌和响应设置的 HTTP（按方向即正向 HTTP）动作终结点。
    /// </summary>
    public OneBot11ActionEndpointOptions ActionEndpoint { get; }

    /// <summary>
    /// Gets the forward WebSocket event endpoint that owns its address, token, and session settings.
    /// 获取拥有自身地址、令牌和会话设置的正向 WebSocket 事件终结点。
    /// </summary>
    public OneBot11EventEndpointOptions EventEndpoint { get; }

    internal OneBot11HttpActionTransportOptions CreateActionOptions()
    {
        return ActionEndpoint.CreateTransportOptions();
    }

    internal OneBot11ForwardWebSocketClientOptions CreateEventOptions()
    {
        return EventEndpoint.CreateTransportOptions();
    }
}

/// <summary>
/// Composes an HTTP (directionally, forward HTTP) action client and a forward WebSocket event listener without global state.
/// 在不使用全局状态的前提下组合 HTTP（按方向即正向 HTTP）动作客户端与正向 WebSocket 事件监听器。
/// </summary>
public sealed partial class OneBot11Bot : IDisposable
{
    private readonly Func<CancellationToken, Task>? _startEventConnector;
    private readonly SemaphoreSlim _startGate = new SemaphoreSlim(1, 1);
    private int _disposed;

    /// <summary>Initializes a bot with internally owned transports. / 使用内部拥有的传输实例初始化机器人。</summary>
    public OneBot11Bot(OneBot11BotOptions options)
        : this(options, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a bot with an optional dispatcher and caller-owned HTTP client.
    /// 使用可选分发器和调用方拥有的 HTTP 客户端初始化机器人。
    /// </summary>
    /// <remarks>The injected HTTP client is never disposed by this object. / 注入的 HTTP 客户端永远不会由此对象释放。</remarks>
    public OneBot11Bot(
        OneBot11BotOptions options,
        OneBot11EventDispatcher? dispatcher,
        HttpClient? httpClient)
        : this(options, dispatcher, httpClient, null)
    {
    }

    internal OneBot11Bot(
        OneBot11BotOptions options,
        OneBot11EventDispatcher? dispatcher,
        HttpClient? httpClient,
        Func<CancellationToken, Task>? startEventConnector)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var actionOptions = options.CreateActionOptions();
        var eventOptions = options.CreateEventOptions();
        ActionAddress = actionOptions.BaseUri;
        EventAddress = eventOptions.Endpoint;
        Events = dispatcher ?? new OneBot11EventDispatcher();
        ActionTransport = new OneBot11HttpActionTransport(actionOptions, httpClient);
        EventTransport = new OneBot11ForwardWebSocketClient(eventOptions, Events);
        Actions = new OneBot11Client(ActionTransport);
        _startEventConnector = startEventConnector;
    }

    /// <summary>Gets the configured HTTP action address snapshot. / 获取已配置的 HTTP 动作地址快照。</summary>
    public Uri ActionAddress { get; }

    /// <summary>Gets the configured WebSocket event address snapshot. / 获取已配置的 WebSocket 事件地址快照。</summary>
    public Uri EventAddress { get; }

    /// <summary>Gets the strongly typed action facade. / 获取强类型动作门面。</summary>
    public OneBot11Client Actions { get; }

    /// <summary>Gets the categorized event dispatcher. / 获取分类事件分发器。</summary>
    public OneBot11EventDispatcher Events { get; }

    /// <summary>Gets the owned HTTP action transport. / 获取内部拥有的 HTTP 动作传输。</summary>
    public OneBot11HttpActionTransport ActionTransport { get; }

    /// <summary>Gets the owned WebSocket event transport. / 获取内部拥有的 WebSocket 事件传输。</summary>
    public OneBot11ForwardWebSocketClient EventTransport { get; }

    /// <summary>Forwards event-transport failures. / 转发事件传输故障。</summary>
    public event Action<OneBot11TransportException>? Faulted
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
    /// Verifies the HTTP action endpoint, starts the WebSocket event listener, and returns the login response.
    /// 验证 HTTP 动作终结点、启动 WebSocket 事件监听，并返回登录响应。
    /// </summary>
    /// <remarks>
    /// This method blocks the calling thread. Prefer <see cref="StartAsync"/> in UI, ASP.NET, and other asynchronous applications.
    /// 此方法会阻塞调用线程；在 UI、ASP.NET 和其它异步应用中应优先使用 <see cref="StartAsync"/>。
    /// Register event subscriptions before calling this method so the first lifecycle event cannot be missed.
    /// 请在调用此方法前注册事件订阅，以免遗漏首个生命周期事件。
    /// Concurrent Start calls are serialized. Do not overlap Start with ConnectAsync or direct EventTransport.ConnectAsync calls.
    /// 多个 Start 调用会自动串行执行；不要让 Start 与 ConnectAsync 或直接的 EventTransport.ConnectAsync 调用并发重叠。
    /// </remarks>
    /// <returns>The successful typed <c>get_login_info</c> response. / 成功的强类型 <c>get_login_info</c> 响应。</returns>
    /// <exception cref="OneBot11BotStartException">The <c>get_login_info</c> action returned a failed response. / <c>get_login_info</c> 动作返回失败响应。</exception>
    /// <exception cref="OneBot11TransportException">An HTTP or WebSocket transport failed. / HTTP 或 WebSocket 传输失败。</exception>
    /// <exception cref="OperationCanceledException">Startup was canceled. / 启动操作被取消。</exception>
    /// <exception cref="ObjectDisposedException">The bot has been disposed. / 机器人已经释放。</exception>
    /// <exception cref="InvalidOperationException">The event endpoint is already connected. / 事件终结点已经连接。</exception>
    public OneBot11Response<OneBot11LoginInfoData> Start(CancellationToken cancellationToken = default)
    {
        return StartAsync(cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously verifies the HTTP action endpoint, starts the WebSocket event listener, and returns the login response.
    /// 异步验证 HTTP 动作终结点、启动 WebSocket 事件监听，并返回登录响应。
    /// </summary>
    /// <remarks>
    /// The method calls <c>get_login_info</c> first and connects the event endpoint only after a successful response.
    /// 此方法先调用 <c>get_login_info</c>，仅在响应成功后连接事件终结点。
    /// Register event subscriptions before calling this method so the first lifecycle event cannot be missed.
    /// 请在调用此方法前注册事件订阅，以免遗漏首个生命周期事件。
    /// Concurrent Start calls are serialized. Do not overlap Start with ConnectAsync or direct EventTransport.ConnectAsync calls.
    /// 多个 Start 调用会自动串行执行；不要让 Start 与 ConnectAsync 或直接的 EventTransport.ConnectAsync 调用并发重叠。
    /// </remarks>
    /// <returns>A task containing the successful typed <c>get_login_info</c> response. / 包含成功强类型 <c>get_login_info</c> 响应的任务。</returns>
    /// <exception cref="OneBot11BotStartException">The <c>get_login_info</c> action returned a failed response. / <c>get_login_info</c> 动作返回失败响应。</exception>
    /// <exception cref="OneBot11TransportException">An HTTP or WebSocket transport failed. / HTTP 或 WebSocket 传输失败。</exception>
    /// <exception cref="OperationCanceledException">Startup was canceled. / 启动操作被取消。</exception>
    /// <exception cref="ObjectDisposedException">The bot has been disposed. / 机器人已经释放。</exception>
    /// <exception cref="InvalidOperationException">The event endpoint is already connected. / 事件终结点已经连接。</exception>
    public async Task<OneBot11Response<OneBot11LoginInfoData>> StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        await _startGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            // Verify the independent Action endpoint before any Event connection can publish callbacks.
            // 在 Event 连接发布任何回调之前，先验证独立的 Action 终结点。
            var loginInfoResponse = await Actions.GetLoginInfoAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!loginInfoResponse.IsSuccess)
            {
                throw new OneBot11BotStartException(loginInfoResponse);
            }

            if (_startEventConnector == null)
            {
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _startEventConnector(cancellationToken).ConfigureAwait(false);
            }

            return loginInfoResponse;
        }
        finally
        {
            _startGate.Release();
        }
    }

    /// <summary>
    /// Connects only the independently configured event endpoint without calling an action.
    /// 仅连接独立配置的事件终结点，不调用任何动作。
    /// </summary>
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

    /// <summary>Releases both owned transports. / 释放两个内部拥有的传输实例。</summary>
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
            throw new ObjectDisposedException(nameof(OneBot11Bot));
        }
    }
}
