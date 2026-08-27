using System;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Provides a transport-independent strongly typed facade for all official OneBot 11 actions.
/// 为全部官方 OneBot 11 动作提供与传输方式无关的强类型门面。
/// </summary>
public sealed partial class OneBot11Client
{
    private readonly IOneBot11ActionTransport _transport;

    /// <summary>
    /// Initializes a OneBot 11 client over the supplied action transport.
    /// 使用给定动作传输初始化 OneBot 11 客户端。
    /// </summary>
    /// <param name="transport">The transport responsible only for raw action request and response I/O. / 仅负责原始动作请求与响应 I/O 的传输。</param>
    public OneBot11Client(IOneBot11ActionTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <summary>
    /// Gets the underlying action transport.
    /// 获取底层动作传输。
    /// </summary>
    public IOneBot11ActionTransport Transport => _transport;

    /// <summary>
    /// Calls a standard or implementation-specific action and preserves raw response data.
    /// 调用标准或实现特有动作，并保留原始响应 data。
    /// </summary>
    public async Task<OneBot11Response> CallActionAsync(
        string action,
        JsonObject? parameters = null,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        var finalAction = OneBot11ActionName.ApplyInvocationMode(action, invocationMode);
        var transportResult = await _transport
            .SendAsync(finalAction, parameters, echo, cancellationToken)
            .ConfigureAwait(false);
        return OneBot11Response.Parse(transportResult);
    }

    /// <summary>
    /// Calls a standard or implementation-specific action with a caller-supplied tolerant data parser.
    /// 使用调用方提供的容错 data 解析器调用标准或实现特有动作。
    /// </summary>
    public async Task<OneBot11Response<TData>> CallActionAsync<TData>(
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

        var finalAction = OneBot11ActionName.ApplyInvocationMode(action, invocationMode);
        var transportResult = await _transport
            .SendAsync(finalAction, parameters, echo, cancellationToken)
            .ConfigureAwait(false);
        return OneBot11Response<TData>.Parse(transportResult, dataParser);
    }

    /// <summary>
    /// Executes the sole official hidden action for an event quick operation.
    /// 执行唯一官方隐藏动作，以对事件执行快速操作。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public Task<OneBot11Response> HandleQuickOperationAsync(
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
            OneBot11HiddenActions.HandleQuickOperation,
            new JsonObject
            {
                ["context"] = Clone(context),
                ["operation"] = Clone(operation)
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    private async Task<OneBot11Response<TData>> SendTypedAsync<TData>(
        string action,
        JsonObject? parameters,
        Func<JsonNode?, TData?> dataParser,
        InvocationMode invocationMode,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        var finalAction = OneBot11ActionName.ApplyInvocationMode(action, invocationMode);
        var transportResult = await _transport
            .SendAsync(finalAction, parameters, echo, cancellationToken)
            .ConfigureAwait(false);
        return OneBot11Response<TData>.Parse(transportResult, dataParser);
    }

    private Task<OneBot11Response> SendWithoutDataAsync(
        string action,
        JsonObject? parameters,
        InvocationMode invocationMode,
        JsonNode? echo,
        CancellationToken cancellationToken)
    {
        return CallActionAsync(action, parameters, invocationMode, echo, cancellationToken);
    }

    private static JsonNode? Clone(JsonNode? node)
    {
        return TolerantJson.Clone(node);
    }
}
