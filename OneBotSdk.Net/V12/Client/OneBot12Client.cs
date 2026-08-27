using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

/// <summary>Provides a transport-independent strongly typed facade for all standard OneBot 12 actions. / 为全部 OneBot 12 标准动作提供与传输无关的强类型门面。</summary>
public sealed partial class OneBot12Client
{
    private readonly IOneBot12ActionTransport _transport;
    private readonly OneBot12Self? _defaultSelf;

    /// <summary>Initializes a client with an optional default bot identity for non-meta actions. / 使用非元动作可选的默认机器人身份初始化客户端。</summary>
    public OneBot12Client(IOneBot12ActionTransport transport, OneBot12Self? defaultSelf = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _defaultSelf = defaultSelf?.Clone();
    }

    /// <summary>Gets the underlying action transport. / 获取底层动作传输。</summary>
    public IOneBot12ActionTransport Transport => _transport;

    /// <summary>Gets the detached default identity applied to non-meta actions. / 获取用于非元动作的独立默认身份。</summary>
    public OneBot12Self? DefaultSelf => _defaultSelf?.Clone();

    /// <summary>Calls a standard or extension action while preserving raw response data. / 调用标准或扩展动作，同时保留原始响应 data。</summary>
    public async Task<OneBot12Response> CallActionAsync(
        string action,
        JsonObject? parameters = null,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        ValidateAction(action);
        var result = await _transport
            .SendAsync(action, parameters, echo, ResolveSelf(self), cancellationToken)
            .ConfigureAwait(false);
        return OneBot12Response.Parse(result);
    }

    /// <summary>Calls an action with a caller-provided field-tolerant data parser. / 使用调用方提供的按字段容错 data 解析器调用动作。</summary>
    public async Task<OneBot12Response<TData>> CallActionAsync<TData>(
        string action,
        Func<JsonNode?, TData?> dataParser,
        JsonObject? parameters = null,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        if (dataParser == null)
        {
            throw new ArgumentNullException(nameof(dataParser));
        }

        ValidateAction(action);
        var result = await _transport
            .SendAsync(action, parameters, echo, ResolveSelf(self), cancellationToken)
            .ConfigureAwait(false);
        return OneBot12Response<TData>.Parse(result, dataParser);
    }

    private async Task<OneBot12Response<TData>> SendTypedAsync<TData>(
        string action,
        JsonObject? parameters,
        Func<JsonNode?, TData?> parser,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken)
    {
        var result = await _transport
            .SendAsync(action, parameters, echo, ResolveSelf(self), cancellationToken)
            .ConfigureAwait(false);
        return OneBot12Response<TData>.Parse(result, parser);
    }

    private async Task<OneBot12Response<TData>> SendMetaTypedAsync<TData>(
        string action,
        JsonObject? parameters,
        Func<JsonNode?, TData?> parser,
        string? echo,
        CancellationToken cancellationToken)
    {
        var result = await _transport.SendAsync(action, parameters, echo, null, cancellationToken).ConfigureAwait(false);
        return OneBot12Response<TData>.Parse(result, parser);
    }

    private async Task<OneBot12Response> SendWithoutDataAsync(
        string action,
        JsonObject? parameters,
        string? echo,
        OneBot12Self? self,
        CancellationToken cancellationToken)
    {
        var result = await _transport
            .SendAsync(action, parameters, echo, ResolveSelf(self), cancellationToken)
            .ConfigureAwait(false);
        return OneBot12Response.Parse(result);
    }

    private OneBot12Self? ResolveSelf(OneBot12Self? value) => (value ?? _defaultSelf)?.Clone();

    private static JsonNode? Clone(JsonNode? node) => TolerantJson.Clone(node);

    private static string Require(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty OneBot protocol value is required.", parameterName);
        }

        return value!;
    }

    private static void ValidateAction(string action) => Require(action, nameof(action));
}
