using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Json;

namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>Describes the framework-neutral HTTP response a host should return for an ingested event. / 描述宿主应为已接入事件返回的框架无关 HTTP 响应。</summary>
public sealed class OneBot12HttpWebhookIngressResponse
{
    private static readonly byte[] EmptyBody = Array.Empty<byte>();

    private OneBot12HttpWebhookIngressResponse(int statusCode, string? contentType, byte[] body)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Body = body;
    }

    /// <summary>Gets the HTTP status code. / 获取 HTTP 状态码。</summary>
    public int StatusCode { get; }

    /// <summary>Gets the response content type, or null for 204. / 获取响应内容类型；204 响应时为 null。</summary>
    public string? ContentType { get; }

    /// <summary>Gets the exact response body bytes. / 获取精确响应正文字节。</summary>
    public byte[] Body { get; }

    /// <summary>Creates the standard 204 response when no actions are returned. / 在不返回动作时创建标准 204 响应。</summary>
    public static OneBot12HttpWebhookIngressResponse NoContent()
    {
        return new OneBot12HttpWebhookIngressResponse(204, null, EmptyBody);
    }

    /// <summary>Creates a 200 JSON response containing an Action request array, or 204 for an empty sequence. / 创建包含动作请求数组的 200 JSON 响应；序列为空时创建 204 响应。</summary>
    public static OneBot12HttpWebhookIngressResponse FromActions(
        IEnumerable<OneBot12HttpWebhookIngressActionRequest> actions)
    {
        if (actions == null)
        {
            throw new ArgumentNullException(nameof(actions));
        }

        var array = new JsonArray();
        foreach (var action in actions)
        {
            if (action == null)
            {
                throw new ArgumentException("An Action request sequence cannot contain null.", nameof(actions));
            }

            array.Add(action.ToJsonObject());
        }

        if (array.Count == 0)
        {
            return NoContent();
        }

        var body = Encoding.UTF8.GetBytes(OneBot12Json.Serialize(array));
        return new OneBot12HttpWebhookIngressResponse(200, "application/json", body);
    }
}
