using System;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Transports;

/// <summary>Captures one complete OneBot 12 action exchange. / 捕获一次完整的 OneBot 12 动作交互。</summary>
public sealed class OneBot12ActionTransportResult
{
    /// <summary>Initializes a detached action exchange. / 初始化独立的动作交互。</summary>
    public OneBot12ActionTransportResult(
        string action,
        JsonObject requestParameters,
        string? requestEcho,
        OneBot12Self? requestSelf,
        string rawRequestJson,
        JsonObject response,
        string rawResponseJson)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        RequestParameters = TolerantJson.CloneObject(requestParameters ?? throw new ArgumentNullException(nameof(requestParameters)));
        RequestEcho = requestEcho;
        RequestSelf = requestSelf?.Clone();
        RawRequestJson = rawRequestJson ?? throw new ArgumentNullException(nameof(rawRequestJson));
        Response = TolerantJson.CloneObject(response ?? throw new ArgumentNullException(nameof(response)));
        RawResponseJson = rawResponseJson ?? throw new ArgumentNullException(nameof(rawResponseJson));
    }

    /// <summary>Gets the action name actually sent. / 获取实际发送的动作名称。</summary>
    public string Action { get; }
    /// <summary>Gets detached parameters actually sent. / 获取实际发送参数的独立副本。</summary>
    public JsonObject RequestParameters { get; }
    /// <summary>Gets the correlation string actually sent. / 获取实际发送的关联字符串。</summary>
    public string? RequestEcho { get; }
    /// <summary>Gets the bot identity actually sent. / 获取实际发送的机器人身份。</summary>
    public OneBot12Self? RequestSelf { get; }
    /// <summary>Gets the exact JSON request text. / 获取精确的 JSON 请求文本。</summary>
    public string RawRequestJson { get; }
    /// <summary>Gets a detached parsed response envelope. / 获取解析后响应信封的独立副本。</summary>
    public JsonObject Response { get; }
    /// <summary>Gets the exact strict-UTF-8 response text. / 获取按严格 UTF-8 接收的精确响应文本。</summary>
    public string RawResponseJson { get; }
}
