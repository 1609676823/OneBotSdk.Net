using System;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Provides a transport-independent strongly typed facade for all official OneBot 10 actions.
/// 为全部官方 OneBot 10 动作提供与传输方式无关的强类型门面。
/// </summary>
public sealed partial class OneBot10Client
{
    private readonly IOneBot10ActionTransport _transport;

    /// <summary>
    /// Initializes a OneBot 10 client over the supplied action transport.
    /// 使用给定动作传输初始化 OneBot 10 客户端。
    /// </summary>
    /// <param name="transport">The transport responsible only for raw action request and response I/O. / 仅负责原始动作请求与响应 I/O 的传输。</param>
    public OneBot10Client(IOneBot10ActionTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <summary>
    /// Gets the underlying action transport.
    /// 获取底层动作传输。
    /// </summary>
    public IOneBot10ActionTransport Transport => _transport;

    /// <summary>
    /// Calls a standard or implementation-specific action and preserves raw response data.
    /// 调用标准或实现特有动作，并保留原始响应 data。
    /// </summary>
    public async Task<OneBot10Response> CallActionAsync(
        string action,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        var finalAction = OneBot10ActionName.ApplyInvocationMode(action, invocationMode);
        var transportResult = await _transport
            .SendAsync(finalAction, parameters, echo, cancellationToken)
            .ConfigureAwait(false);
        return OneBot10Response.Parse(transportResult);
    }

    /// <summary>
    /// Calls a standard or implementation-specific action with a caller-supplied tolerant data parser.
    /// 使用调用方提供的容错 data 解析器调用标准或实现特有动作。
    /// </summary>
    public async Task<OneBot10Response<TData>> CallActionAsync<TData>(
        string action,
        Func<JsonNode?, TData?> dataParser,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (dataParser == null)
        {
            throw new ArgumentNullException(nameof(dataParser));
        }

        var finalAction = OneBot10ActionName.ApplyInvocationMode(action, invocationMode);
        var transportResult = await _transport
            .SendAsync(finalAction, parameters, echo, cancellationToken)
            .ConfigureAwait(false);
        return OneBot10Response<TData>.Parse(transportResult, dataParser);
    }

    /// <summary>
    /// Executes the sole official hidden action for an event quick operation.
    /// 执行唯一官方隐藏动作，以对事件执行快速操作。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot10Response> HandleQuickOperationAsync(
        JsonObject context,
        JsonObject operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        return CallActionAsync(
            OneBot10HiddenActions.HandleQuickOperation,
            new JsonObject
            {
                ["context"] = Clone(context),
                ["operation"] = Clone(operation)
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Executes a strongly typed private-message quick operation. / 执行强类型私聊消息快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot10Response> HandleQuickOperationAsync(
        PrivateMessageEvent context,
        PrivateMessageQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleTypedQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken);
    }

    /// <summary>Executes a strongly typed group-message quick operation. / 执行强类型群消息快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot10Response> HandleQuickOperationAsync(
        GroupMessageEvent context,
        GroupMessageQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleTypedQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken);
    }

    /// <summary>Executes a strongly typed discussion-message quick operation. / 执行强类型讨论组消息快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot10Response> HandleQuickOperationAsync(
        DiscussMessageEvent context,
        DiscussMessageQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleTypedQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken);
    }

    /// <summary>Executes a strongly typed friend-request quick operation. / 执行强类型加好友请求快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot10Response> HandleQuickOperationAsync(
        FriendRequestEvent context,
        FriendRequestQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleTypedQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken);
    }

    /// <summary>Executes a strongly typed group-request quick operation. / 执行强类型群请求快速操作。</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot10Response> HandleQuickOperationAsync(
        GroupRequestEvent context,
        GroupRequestQuickOperation operation,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return HandleTypedQuickOperationAsync(context, operation, invocationMode, echo, cancellationToken);
    }

    private async Task<OneBot10Response<TData>> SendTypedAsync<TData>(
        string action,
        JsonObject? parameters,
        Func<JsonNode?, TData?> dataParser,
        InvocationMode invocationMode,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        var finalAction = OneBot10ActionName.ApplyInvocationMode(action, invocationMode);
        var transportResult = await _transport
            .SendAsync(finalAction, parameters, echo, cancellationToken)
            .ConfigureAwait(false);
        return OneBot10Response<TData>.Parse(transportResult, dataParser);
    }

    private Task<OneBot10Response> SendWithoutDataAsync(
        string action,
        JsonObject? parameters,
        InvocationMode invocationMode,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        return CallActionAsync(action, parameters, invocationMode, echo, cancellationToken);
    }

    private Task<OneBot10Response> HandleTypedQuickOperationAsync<TOperation>(
        OneBot10Event context,
        TOperation operation,
        InvocationMode invocationMode,
        JsonNode? echo,
        CancellationToken cancellationToken)
        where TOperation : class
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        // Use the retained event object as context and serialize only explicitly selected quick-operation fields.
        // 使用事件保留的原始对象作为上下文，并且只序列化显式选择的快速操作字段。
        var operationObject = OneBot10Json.Parse(OneBot10Json.Serialize(operation)) as JsonObject ?? new JsonObject();
        return HandleQuickOperationAsync(
            TolerantJson.CloneObject(context.RawJson),
            operationObject,
            invocationMode,
            echo,
            cancellationToken);
    }

    private static JsonNode? Clone(JsonNode? node)
    {
        return TolerantJson.Clone(node);
    }
}
