using System;
using System.Net;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V12.Transports;

/// <summary>Identifies a stable OneBot 12 transport failure category. / 标识稳定的 OneBot 12 传输失败类别。</summary>
public enum OneBot12TransportError
{
    /// <summary>An uncategorized failure. / 未分类失败。</summary>
    Unknown,
    /// <summary>Invalid transport configuration. / 传输配置无效。</summary>
    InvalidConfiguration,
    /// <summary>The requested session is not connected. / 请求的会话尚未连接。</summary>
    NotConnected,
    /// <summary>A network connection failed. / 网络连接失败。</summary>
    ConnectionFailed,
    /// <summary>An HTTP status indicates failure. / HTTP 状态表示失败。</summary>
    HttpFailure,
    /// <summary>The peer violated the wire protocol. / 对端违反线协议。</summary>
    ProtocolViolation,
    /// <summary>Authentication failed. / 身份验证失败。</summary>
    AuthenticationFailed,
    /// <summary>A payload exceeded its safety limit. / 载荷超过安全限制。</summary>
    MessageTooLarge,
    /// <summary>The remote peer closed prematurely. / 远端过早关闭连接。</summary>
    RemoteClosed
}

/// <summary>Represents a OneBot 12 transport failure with available raw trace data. / 表示包含可用原始追踪数据的 OneBot 12 传输失败。</summary>
public sealed class OneBot12TransportException : Exception
{
    /// <summary>Initializes a transport exception. / 初始化传输异常。</summary>
    public OneBot12TransportException(OneBot12TransportError error, string message) : base(message) => Error = error;
    /// <summary>Initializes a transport exception with its cause. / 使用底层原因初始化传输异常。</summary>
    public OneBot12TransportException(OneBot12TransportError error, string message, Exception innerException) : base(message, innerException) => Error = error;

    /// <summary>Gets the stable failure category. / 获取稳定的失败类别。</summary>
    public OneBot12TransportError Error { get; }
    /// <summary>Gets the associated action when available. / 获取关联动作（如可用）。</summary>
    public string? Action { get; internal set; }
    /// <summary>Gets the associated HTTP status when available. / 获取关联 HTTP 状态（如可用）。</summary>
    public HttpStatusCode? HttpStatusCode { get; internal set; }
    /// <summary>Gets detached request parameters formed before failure. / 获取失败前已构造请求参数的独立副本。</summary>
    public JsonObject? RequestParameters { get; internal set; }
    /// <summary>Gets the request correlation string when available. / 获取请求关联字符串（如可用）。</summary>
    public string? RequestEcho { get; internal set; }
    /// <summary>Gets the request bot identity when available. / 获取请求机器人身份（如可用）。</summary>
    public OneBot12Self? RequestSelf { get; internal set; }
    /// <summary>Gets the exact request JSON when available. / 获取精确请求 JSON（如可用）。</summary>
    public string? RawRequestJson { get; internal set; }
    /// <summary>Gets the exact response JSON when available. / 获取精确响应 JSON（如可用）。</summary>
    public string? RawResponseJson { get; internal set; }
}
