using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Transports;

namespace OneBotSdk.Net.V12.Responses;

/// <summary>Identifies the normalized OneBot 12 response status. / 标识规范化的 OneBot 12 响应状态。</summary>
public enum OneBot12ResponseStatus
{
    /// <summary>The status is absent or implementation-defined. / 状态缺失或为实现扩展值。</summary>
    Unknown,
    /// <summary>The action succeeded. / 动作执行成功。</summary>
    Ok,
    /// <summary>The action failed. / 动作执行失败。</summary>
    Failed
}

/// <summary>Contains the common field-tolerant OneBot 12 response envelope. / 包含按字段容错的 OneBot 12 公共响应信封。</summary>
public abstract class OneBot12ResponseBase : OneBot12JsonModel
{
    private protected OneBot12ResponseBase(JsonObject? source, OneBot12ActionTransportResult? exchange)
        : base(source == null ? new JsonObject() : TolerantJson.CloneObject(source))
    {
        // Parse every envelope field independently so one implementation defect never hides its siblings.
        // 独立解析每个信封字段，避免单个实现缺陷遮蔽其它同级字段。
        Status = TolerantJson.String(source, "status");
        RetCode = TolerantJson.Int64(source, "retcode");
        Message = TolerantJson.String(source, "message");
        Echo = TolerantJson.String(source, "echo");

        if (exchange != null)
        {
            Action = exchange.Action;
            RequestParameters = TolerantJson.CloneObject(exchange.RequestParameters);
            RequestEcho = exchange.RequestEcho;
            RequestSelf = exchange.RequestSelf?.Clone();
            RawRequestJson = exchange.RawRequestJson;
            RawResponseJson = exchange.RawResponseJson;
        }
    }

    /// <summary>Gets the action actually sent by the transport. / 获取传输层实际发送的动作。</summary>
    [JsonIgnore]
    public string? Action { get; }

    /// <summary>Gets a detached snapshot of the parameters actually sent. / 获取实际发送参数的独立快照。</summary>
    [JsonIgnore]
    public JsonObject? RequestParameters { get; }

    /// <summary>Gets the correlation string actually sent. / 获取实际发送的关联字符串。</summary>
    [JsonIgnore]
    public string? RequestEcho { get; }

    /// <summary>Gets the bot identity actually sent. / 获取实际发送的机器人身份。</summary>
    [JsonIgnore]
    public OneBot12Self? RequestSelf { get; }

    /// <summary>Gets the exact JSON request text sent by the transport. / 获取传输层实际发送的精确 JSON 请求文本。</summary>
    [JsonIgnore]
    public string? RawRequestJson { get; }

    /// <summary>Gets the exact strict-UTF-8 response text. / 获取按严格 UTF-8 接收的精确响应文本。</summary>
    [JsonIgnore]
    public string? RawResponseJson { get; }

    /// <summary>Gets the raw status string. / 获取原始状态字符串。</summary>
    [JsonPropertyName("status")]
    public string? Status { get; }

    /// <summary>Gets the normalized status without discarding unknown raw values. / 获取规范化状态，同时不丢弃未知原始值。</summary>
    [JsonIgnore]
    public OneBot12ResponseStatus StatusKind
    {
        get
        {
            if (string.Equals(Status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return OneBot12ResponseStatus.Ok;
            }

            if (string.Equals(Status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                return OneBot12ResponseStatus.Failed;
            }

            return OneBot12ResponseStatus.Unknown;
        }
    }

    /// <summary>Gets the protocol return code, or null when malformed. / 获取协议返回码；格式异常时为 null。</summary>
    [JsonPropertyName("retcode")]
    public long? RetCode { get; }

    /// <summary>Gets the human-readable response message. / 获取便于阅读的响应消息。</summary>
    [JsonPropertyName("message")]
    public string? Message { get; }

    /// <summary>Gets the response correlation string. / 获取响应关联字符串。</summary>
    [JsonPropertyName("echo")]
    public string? Echo { get; }

    /// <summary>Gets whether this is a successful standard response. / 获取是否为成功的标准响应。</summary>
    [JsonIgnore]
    public bool IsSuccess => StatusKind == OneBot12ResponseStatus.Ok && RetCode == 0;
}

/// <summary>Represents a response whose data remains raw JSON. / 表示 data 保持原始 JSON 的响应。</summary>
public sealed class OneBot12Response : OneBot12ResponseBase
{
    private OneBot12Response(JsonObject? source, OneBot12ActionTransportResult? exchange)
        : base(source, exchange)
    {
        Data = TolerantJson.Clone(TolerantJson.Node(source, "data"));
    }

    /// <summary>Gets a detached raw response data node. / 获取独立的原始响应 data 节点。</summary>
    [JsonPropertyName("data")]
    public JsonNode? Data { get; }

    /// <summary>Parses a response without transport trace metadata. / 解析不包含传输追踪元数据的响应。</summary>
    public static OneBot12Response Parse(JsonObject? source) => new OneBot12Response(source, null);

    internal static OneBot12Response Parse(OneBot12ActionTransportResult exchange) =>
        new OneBot12Response(exchange.Response, exchange);
}

/// <summary>Represents a response with field-tolerant strongly typed data. / 表示包含按字段容错强类型 data 的响应。</summary>
public sealed class OneBot12Response<TData> : OneBot12ResponseBase
{
    private OneBot12Response(
        JsonObject? source,
        Func<JsonNode?, TData?> parser,
        OneBot12ActionTransportResult? exchange)
        : base(source, exchange)
    {
        var sourceData = TolerantJson.Node(source, "data");
        RawData = TolerantJson.Clone(sourceData);
        try
        {
            // A data parser failure is isolated from status, retcode, message, echo, and raw diagnostics.
            // data 解析器失败时与 status、retcode、message、echo 和原始诊断信息隔离。
            Data = parser(RawData ?? sourceData);
        }
        catch (Exception)
        {
            Data = default;
        }
    }

    /// <summary>Gets the parsed action-specific data. / 获取解析后的动作专属 data。</summary>
    [JsonPropertyName("data")]
    public TData? Data { get; }

    /// <summary>Gets the detached unparsed data for diagnostics and extensions. / 获取用于诊断和扩展的独立未解析 data。</summary>
    [JsonIgnore]
    public JsonNode? RawData { get; }

    /// <summary>Parses typed data without transport trace metadata. / 解析不包含传输追踪元数据的强类型 data。</summary>
    public static OneBot12Response<TData> Parse(JsonObject? source, Func<JsonNode?, TData?> parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        return new OneBot12Response<TData>(source, parser, null);
    }

    internal static OneBot12Response<TData> Parse(
        OneBot12ActionTransportResult exchange,
        Func<JsonNode?, TData?> parser)
    {
        if (exchange == null)
        {
            throw new ArgumentNullException(nameof(exchange));
        }

        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        return new OneBot12Response<TData>(exchange.Response, parser, exchange);
    }
}
