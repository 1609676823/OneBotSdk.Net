using System;
using System.Net;
using System.Text.Json.Nodes;

namespace OneBotSdk.Net.V10.Transports;

/// <summary>
/// Identifies the transport boundary at which an operation failed.
/// 标识操作失败时所在的传输边界。
/// </summary>
public enum OneBot10TransportError
{
    /// <summary>
    /// The failure does not fit a more specific category.
    /// 失败不属于其它更具体的类别。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A transport option or endpoint is invalid.
    /// 传输选项或终结点无效。
    /// </summary>
    InvalidConfiguration,

    /// <summary>
    /// The requested transport is not connected or has already stopped.
    /// 请求的传输尚未连接或已经停止。
    /// </summary>
    NotConnected,

    /// <summary>
    /// A network connection could not be established or was interrupted.
    /// 网络连接无法建立或被中断。
    /// </summary>
    ConnectionFailed,

    /// <summary>
    /// An HTTP endpoint returned a non-success status code.
    /// HTTP 终结点返回了非成功状态码。
    /// </summary>
    HttpFailure,

    /// <summary>
    /// The peer returned data that is not a valid OneBot transport payload.
    /// 对端返回的数据不是有效的 OneBot 传输载荷。
    /// </summary>
    ProtocolViolation,

    /// <summary>
    /// Transport authentication or signature verification failed.
    /// 传输认证或签名验证失败。
    /// </summary>
    AuthenticationFailed,

    /// <summary>
    /// An inbound or outbound payload exceeded the configured safety limit.
    /// 入站或出站载荷超过了配置的安全限制。
    /// </summary>
    MessageTooLarge,

    /// <summary>
    /// The remote endpoint closed the transport before the operation completed.
    /// 远端在操作完成前关闭了传输连接。
    /// </summary>
    RemoteClosed
}

/// <summary>
/// Represents a failure in an HTTP, WebSocket, or reverse-post transport.
/// 表示 HTTP、WebSocket 或反向上报传输中的失败。
/// </summary>
public sealed class OneBot10TransportException : Exception
{
    /// <summary>
    /// Initializes a transport exception.
    /// 初始化传输异常。
    /// </summary>
    public OneBot10TransportException(OneBot10TransportError error, string message)
        : base(message)
    {
        Error = error;
    }

    /// <summary>
    /// Initializes a transport exception with its underlying failure.
    /// 使用底层失败信息初始化传输异常。
    /// </summary>
    public OneBot10TransportException(OneBot10TransportError error, string message, Exception innerException)
        : base(message, innerException)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the stable transport error category.
    /// 获取稳定的传输错误类别。
    /// </summary>
    public OneBot10TransportError Error { get; }

    /// <summary>
    /// Gets the OneBot action associated with the failure, when applicable.
    /// 获取与失败关联的 OneBot 动作（如适用）。
    /// </summary>
    public string? Action { get; internal set; }

    /// <summary>
    /// Gets the HTTP status associated with the failure, when applicable.
    /// 获取与失败关联的 HTTP 状态码（如适用）。
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; internal set; }

    /// <summary>
    /// Gets a detached snapshot of the action parameters when the request was formed before the failure.
    /// 获取失败前已构造请求时的动作参数独立快照。
    /// </summary>
    public JsonObject? RequestParameters { get; internal set; }

    /// <summary>
    /// Gets a detached snapshot of the correlation value actually sent, when available.
    /// 获取实际发送的关联值独立快照（如可用）。
    /// </summary>
    public JsonNode? RequestEcho { get; internal set; }

    /// <summary>
    /// Gets the exact JSON text sent before the failure, when available.
    /// 获取失败前已发送的精确 JSON 文本（如可用）。
    /// </summary>
    public string? RawRequestJson { get; internal set; }

    /// <summary>
    /// Gets the exact strict-UTF-8 response text received before the failure, when available.
    /// 获取失败前已按严格 UTF-8 接收的精确响应文本（如可用）。
    /// </summary>
    public string? RawResponseJson { get; internal set; }
}
