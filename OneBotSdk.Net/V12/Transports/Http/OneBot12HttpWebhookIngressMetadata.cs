namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>Contains HTTP metadata captured by the caller's web host. / 包含调用方 Web 宿主捕获的 HTTP 元数据。</summary>
public sealed class OneBot12HttpWebhookIngressMetadata
{
    /// <summary>Initializes inbound Webhook metadata. / 初始化入站 Webhook 元数据。</summary>
    public OneBot12HttpWebhookIngressMetadata(
        string? contentType,
        string? userAgent,
        string? oneBotVersion,
        string? implementation,
        string? authorization,
        string? accessTokenQuery = null)
    {
        ContentType = contentType;
        UserAgent = userAgent;
        OneBotVersion = oneBotVersion;
        Implementation = implementation;
        Authorization = authorization;
        AccessTokenQuery = accessTokenQuery;
    }

    /// <summary>Gets the Content-Type header. / 获取 Content-Type 请求头。</summary>
    public string? ContentType { get; }

    /// <summary>Gets the User-Agent header. / 获取 User-Agent 请求头。</summary>
    public string? UserAgent { get; }

    /// <summary>Gets the X-OneBot-Version header. / 获取 X-OneBot-Version 请求头。</summary>
    public string? OneBotVersion { get; }

    /// <summary>Gets the X-Impl header. / 获取 X-Impl 请求头。</summary>
    public string? Implementation { get; }

    /// <summary>Gets the Authorization header. / 获取 Authorization 请求头。</summary>
    public string? Authorization { get; }

    /// <summary>Gets the access_token query fallback. / 获取 access_token 查询参数回退值。</summary>
    public string? AccessTokenQuery { get; }
}
