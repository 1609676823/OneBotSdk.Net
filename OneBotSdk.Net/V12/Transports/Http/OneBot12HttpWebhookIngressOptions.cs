using System;

namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>Configures framework-independent OneBot 12 HTTP Webhook ingestion. / 配置与 Web 框架无关的 OneBot 12 HTTP Webhook 接入。</summary>
public sealed class OneBot12HttpWebhookIngressOptions
{
    private string? _accessToken;

    /// <summary>Gets or sets the exact expected token for Bearer or query authentication; null or empty disables verification. / 获取或设置 Bearer 或查询参数身份验证所需的精确令牌；null 或空值表示禁用验证。</summary>
    public string? AccessToken
    {
        get => _accessToken;
        set => _accessToken = ValidateAccessToken(value, nameof(AccessToken));
    }

    /// <summary>Gets or sets whether mandatory OneBot version, implementation, User-Agent, and JSON content headers are checked. / 获取或设置是否校验必填的 OneBot 版本、实现端、User-Agent 与 JSON 内容请求头。</summary>
    public bool RequireStandardHeaders { get; set; } = true;

    /// <summary>Gets or sets the largest accepted request body. / 获取或设置允许接收的最大请求正文。</summary>
    public int MaxRequestBodyBytes { get; set; } = 4 * 1024 * 1024;

    internal OneBot12HttpWebhookIngressOptions Snapshot()
    {
        if (MaxRequestBodyBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxRequestBodyBytes));
        }

        return new OneBot12HttpWebhookIngressOptions
        {
            AccessToken = AccessToken,
            RequireStandardHeaders = RequireStandardHeaders,
            MaxRequestBodyBytes = MaxRequestBodyBytes
        };
    }

    private static string? ValidateAccessToken(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Incoming authentication retains every character because the implementation may use query fallback.
        // 入站身份验证保留每个字符，因为实现端可使用查询参数回退。
        _ = parameterName;
        return value;
    }
}
