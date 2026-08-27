using System;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Transports;

/// <summary>
/// Captures one completed action exchange exactly as it crossed the transport boundary.
/// 捕获一次已完成的动作交互，内容与通过传输边界时完全一致。
/// </summary>
public sealed class OneBot10ActionTransportResult
{
    /// <summary>
    /// Initializes an immutable-text, detached-node snapshot of an action exchange.
    /// 初始化一个包含不可变文本和独立节点快照的动作交互。
    /// </summary>
    public OneBot10ActionTransportResult(
        string action,
        JsonObject requestParameters,
        JsonNode? requestEcho,
        string rawRequestJson,
        JsonObject response,
        string rawResponseJson)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("A OneBot action name is required.", nameof(action));
        }

        Action = action;
        RequestParameters = TolerantJson.CloneObject(
            requestParameters ?? throw new ArgumentNullException(nameof(requestParameters)));
        RequestEcho = TolerantJson.Clone(requestEcho);
        RawRequestJson = rawRequestJson ?? throw new ArgumentNullException(nameof(rawRequestJson));
        Response = TolerantJson.CloneObject(response ?? throw new ArgumentNullException(nameof(response)));
        RawResponseJson = rawResponseJson ?? throw new ArgumentNullException(nameof(rawResponseJson));
    }

    /// <summary>Gets the actual action name, including an invocation suffix when used. / 获取实际动作名，使用调用后缀时包含该后缀。</summary>
    public string Action { get; }

    /// <summary>Gets a detached snapshot of the parameters actually sent. / 获取实际发送参数的独立快照。</summary>
    public JsonObject RequestParameters { get; }

    /// <summary>Gets a detached snapshot of the correlation value actually sent, when applicable. / 获取实际发送的关联值独立快照（如适用）。</summary>
    public JsonNode? RequestEcho { get; }

    /// <summary>Gets the exact JSON text sent by the transport. / 获取传输层实际发送的精确 JSON 文本。</summary>
    public string RawRequestJson { get; }

    /// <summary>Gets a detached parsed snapshot of the response object. / 获取响应对象解析后的独立快照。</summary>
    public JsonObject Response { get; }

    /// <summary>Gets the exact strict-UTF-8 JSON text received by the transport. / 获取传输层按严格 UTF-8 实际接收的精确 JSON 文本。</summary>
    public string RawResponseJson { get; }
}
