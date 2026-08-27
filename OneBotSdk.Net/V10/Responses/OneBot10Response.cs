using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Transports;

namespace OneBotSdk.Net.V10.Responses;

/// <summary>
/// Identifies the normalized status of a OneBot 10 action response.
/// 标识 OneBot 10 动作响应的规范化状态。
/// </summary>
public enum OneBot10ResponseStatus
{
    /// <summary>
    /// The endpoint returned an absent or implementation-specific status value.
    /// 实现端返回了缺失或实现特有的状态值。
    /// </summary>
    Unknown,

    /// <summary>
    /// The action completed successfully.
    /// 动作已成功完成。
    /// </summary>
    Ok,

    /// <summary>
    /// The action was accepted for asynchronous execution.
    /// 动作已被接受并异步执行。
    /// </summary>
    Async,

    /// <summary>
    /// The action failed.
    /// 动作执行失败。
    /// </summary>
    Failed
}

/// <summary>
/// Contains the common, field-tolerant portion of a OneBot 10 response envelope.
/// 包含 OneBot 10 响应信封中按字段容错解析的公共部分。
/// </summary>
public abstract class OneBot10ResponseBase : OneBot10JsonModel
{
    private protected OneBot10ResponseBase(
        JsonObject? source,
        OneBot10ActionTransportResult? transportResult)
    {
        // Every envelope field is read independently; one malformed field must not hide its siblings.
        // 每个信封字段均独立读取，单个异常字段不得遮蔽其它同级字段。
        RawJson = source == null ? new JsonObject() : TolerantJson.CloneObject(source);
        Status = TolerantJson.String(source, "status");
        RetCode = TolerantJson.Int64(source, "retcode");
        Echo = TolerantJson.Clone(TolerantJson.Node(source, "echo"));

        if (transportResult != null)
        {
            Action = transportResult.Action;
            RequestParameters = TolerantJson.CloneObject(transportResult.RequestParameters);
            RequestEcho = TolerantJson.Clone(transportResult.RequestEcho);
            RawRequestJson = transportResult.RawRequestJson;
            RawResponseJson = transportResult.RawResponseJson;
        }
    }

    /// <summary>
    /// Gets the actual action name, including an invocation suffix, when this response came from a client call.
    /// 获取实际动作名（包含调用后缀）；仅当此响应来自客户端调用时可用。
    /// </summary>
    [JsonIgnore]
    public string? Action { get; }

    /// <summary>
    /// Gets a detached snapshot of the parameters actually sent, when this response came from a client call.
    /// 获取实际发送参数的独立快照；仅当此响应来自客户端调用时可用。
    /// </summary>
    [JsonIgnore]
    public JsonObject? RequestParameters { get; }

    /// <summary>
    /// Gets a detached snapshot of the correlation value actually sent, when applicable.
    /// 获取实际发送的关联值独立快照（如适用）。
    /// </summary>
    [JsonIgnore]
    public JsonNode? RequestEcho { get; }

    /// <summary>
    /// Gets the exact JSON text sent by the transport, when this response came from a client call.
    /// 获取传输层实际发送的精确 JSON 文本；仅当此响应来自客户端调用时可用。
    /// </summary>
    [JsonIgnore]
    public string? RawRequestJson { get; }

    /// <summary>
    /// Gets the exact strict-UTF-8 JSON text received by the transport, when available.
    /// 获取传输层按严格 UTF-8 实际接收的精确 JSON 文本（如可用）。
    /// </summary>
    [JsonIgnore]
    public string? RawResponseJson { get; }

    /// <summary>
    /// Gets the raw protocol status string, including unknown implementation-specific values.
    /// 获取原始协议状态字符串，包括未知的实现特有值。
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; }

    /// <summary>
    /// Gets the normalized response status without discarding the raw <see cref="Status"/> value.
    /// 获取规范化响应状态，同时保留原始 <see cref="Status"/> 值。
    /// </summary>
    [JsonIgnore]
    public OneBot10ResponseStatus StatusKind
    {
        get
        {
            if (string.Equals(Status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return OneBot10ResponseStatus.Ok;
            }

            if (string.Equals(Status, "async", StringComparison.OrdinalIgnoreCase))
            {
                return OneBot10ResponseStatus.Async;
            }

            if (string.Equals(Status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                return OneBot10ResponseStatus.Failed;
            }

            return OneBot10ResponseStatus.Unknown;
        }
    }

    /// <summary>
    /// Gets the return code, or <see langword="null"/> when the field is absent or malformed.
    /// 获取返回码；字段缺失或格式异常时为 <see langword="null"/>。
    /// </summary>
    [JsonPropertyName("retcode")]
    public long? RetCode { get; }

    /// <summary>
    /// Gets a detached copy of the optional request correlation value.
    /// 获取可选请求关联值的独立副本。
    /// </summary>
    [JsonPropertyName("echo")]
    public JsonNode? Echo { get; }

    /// <summary>
    /// Gets whether the response represents a completed successful action.
    /// 获取响应是否表示动作已成功完成。
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => StatusKind == OneBot10ResponseStatus.Ok && RetCode == 0;

    /// <summary>
    /// Gets whether the response represents an asynchronously accepted action.
    /// 获取响应是否表示动作已被异步接受。
    /// </summary>
    [JsonIgnore]
    public bool IsAsync => StatusKind == OneBot10ResponseStatus.Async && RetCode == 1;
}

/// <summary>
/// Represents a OneBot 10 response whose data is intentionally kept as JSON.
/// 表示将 data 原样保留为 JSON 的 OneBot 10 响应。
/// </summary>
public sealed class OneBot10Response : OneBot10ResponseBase
{
    private OneBot10Response(
        JsonObject? source,
        OneBot10ActionTransportResult? transportResult)
        : base(source, transportResult)
    {
        Data = TolerantJson.Clone(TolerantJson.Node(source, "data"));
    }

    /// <summary>
    /// Gets a detached copy of the raw response data.
    /// 获取原始响应 data 的独立副本。
    /// </summary>
    [JsonPropertyName("data")]
    public JsonNode? Data { get; }

    /// <summary>
    /// Parses an action response without allowing malformed fields to invalidate the envelope.
    /// 解析动作响应，并避免异常字段使整个信封失效。
    /// </summary>
    public static OneBot10Response Parse(JsonObject? source)
    {
        return new OneBot10Response(source, null);
    }

    internal static OneBot10Response Parse(OneBot10ActionTransportResult transportResult)
    {
        if (transportResult == null)
        {
            throw new ArgumentNullException(nameof(transportResult));
        }

        return new OneBot10Response(transportResult.Response, transportResult);
    }
}

/// <summary>
/// Represents a OneBot 10 response with field-tolerant strongly typed data.
/// 表示包含按字段容错强类型 data 的 OneBot 10 响应。
/// </summary>
/// <typeparam name="TData">The action-specific response data type. / 动作专属响应数据类型。</typeparam>
public sealed class OneBot10Response<TData> : OneBot10ResponseBase
{
    private OneBot10Response(
        JsonObject? source,
        Func<JsonNode?, TData?> dataParser,
        OneBot10ActionTransportResult? transportResult)
        : base(source, transportResult)
    {
        var sourceData = TolerantJson.Node(source, "data");
        RawData = TolerantJson.Clone(sourceData);

        try
        {
            // Prefer the detached snapshot for compatibility, but fall back to the source if cloning failed.
            // 为保持兼容优先解析独立快照；克隆失败时回退到源节点。
            Data = dataParser(RawData ?? sourceData);
        }
        catch (Exception)
        {
            // A custom parser failure is contained within data and never invalidates status, retcode, or echo.
            // 自定义解析器失败仅影响 data，不得使 status、retcode 或 echo 失效。
            Data = default;
        }
    }

    /// <summary>
    /// Gets the parsed action-specific data, or its default value when data is absent or malformed.
    /// 获取已解析的动作专属数据；data 缺失或异常时为其默认值。
    /// </summary>
    [JsonPropertyName("data")]
    public TData? Data { get; }

    /// <summary>
    /// Gets the detached unparsed data for diagnostics and implementation extensions.
    /// 获取用于诊断和实现扩展的独立未解析 data。
    /// </summary>
    [JsonIgnore]
    public JsonNode? RawData { get; }

    internal static OneBot10Response<TData> Parse(JsonObject? source, Func<JsonNode?, TData?> dataParser)
    {
        if (dataParser == null)
        {
            throw new ArgumentNullException(nameof(dataParser));
        }

        return new OneBot10Response<TData>(source, dataParser, null);
    }

    internal static OneBot10Response<TData> Parse(
        OneBot10ActionTransportResult transportResult,
        Func<JsonNode?, TData?> dataParser)
    {
        if (transportResult == null)
        {
            throw new ArgumentNullException(nameof(transportResult));
        }

        if (dataParser == null)
        {
            throw new ArgumentNullException(nameof(dataParser));
        }

        return new OneBot10Response<TData>(transportResult.Response, dataParser, transportResult);
    }
}
