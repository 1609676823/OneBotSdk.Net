using System;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>Represents one Action request returned in a successful HTTP Webhook response. / 表示 HTTP Webhook 成功响应中返回的一项动作请求。</summary>
public sealed class OneBot12HttpWebhookIngressActionRequest
{
    /// <summary>Initializes a detached Action request. / 初始化独立的动作请求。</summary>
    public OneBot12HttpWebhookIngressActionRequest(
        string action,
        JsonObject? parameters = null,
        string? echo = null,
        OneBot12Self? self = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("A OneBot 12 action name is required.", nameof(action));
        }

        Action = action;
        Parameters = TolerantJson.Clone(parameters) as JsonObject ?? new JsonObject();
        Echo = echo;
        Self = self?.Clone();
    }

    /// <summary>Gets the Action name. / 获取动作名称。</summary>
    public string Action { get; }

    /// <summary>Gets the detached Action parameters. / 获取独立的动作参数。</summary>
    public JsonObject Parameters { get; }

    /// <summary>Gets the optional correlation string. / 获取可选关联字符串。</summary>
    public string? Echo { get; }

    /// <summary>Gets the optional bot identity. / 获取可选机器人身份。</summary>
    public OneBot12Self? Self { get; }

    internal JsonObject ToJsonObject()
    {
        var result = new JsonObject
        {
            ["action"] = Action,
            ["params"] = TolerantJson.Clone(Parameters)
        };
        if (Echo != null)
        {
            result["echo"] = Echo;
        }

        if (Self != null)
        {
            result["self"] = Self.ToJsonObject();
        }

        return result;
    }
}
