using System;

namespace OneBotSdk.Net.V12.Transports.Http;

/// <summary>Configures a OneBot 12 HTTP action endpoint and its matching token. / 配置 OneBot 12 HTTP 动作终结点及其对应令牌。</summary>
public sealed class OneBot12HttpActionTransportOptions
{
    /// <summary>Initializes options for the specification-defined root action endpoint. / 初始化规范定义的根路径动作终结点选项。</summary>
    public OneBot12HttpActionTransportOptions(Uri endpoint)
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    /// <summary>Gets or sets the absolute HTTP(S) root endpoint receiving action envelopes. / 获取或设置接收动作信封的绝对 HTTP(S) 根终结点。</summary>
    public Uri Endpoint { get; set; }

    /// <summary>Gets or sets the exact non-empty access token belonging to this endpoint. / 获取或设置属于此终结点的精确非空访问令牌。</summary>
    /// <remarks>Header-safe values use Bearer authentication; other non-empty values use the standard query fallback without trimming. / 可安全写入请求头的值使用 Bearer 鉴权；其它非空值不经裁剪并使用标准查询参数回退。</remarks>
    public string? AccessToken { get; set; }

    /// <summary>Gets or sets the largest accepted response body in bytes. / 获取或设置允许接收的最大响应正文大小（字节）。</summary>
    public int MaxResponseBodyBytes { get; set; } = 4 * 1024 * 1024;

    internal OneBot12HttpActionTransportOptions Snapshot()
    {
        var endpoint = Endpoint ?? throw new ArgumentNullException(nameof(Endpoint));
        ValidateEndpoint(endpoint);
        if (MaxResponseBodyBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxResponseBodyBytes), "The value must be greater than zero.");
        }

        return new OneBot12HttpActionTransportOptions(endpoint)
        {
            AccessToken = NormalizeToken(AccessToken),
            MaxResponseBodyBytes = MaxResponseBodyBytes
        };
    }

    private static void ValidateEndpoint(Uri endpoint)
    {
        if (!endpoint.IsAbsoluteUri ||
            (!string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("The endpoint must be an absolute HTTP or HTTPS URI.", nameof(Endpoint));
        }

        if (!string.IsNullOrEmpty(endpoint.Query) || !string.IsNullOrEmpty(endpoint.Fragment))
        {
            throw new ArgumentException("The endpoint cannot contain a query or fragment.", nameof(Endpoint));
        }

        // OneBot 12 HTTP actions are POSTed to one root path, never to /{action}.
        // OneBot 12 HTTP 动作统一 POST 到根路径，绝不能发送到 /{action}。
        if (!string.IsNullOrEmpty(endpoint.AbsolutePath) && endpoint.AbsolutePath != "/")
        {
            throw new ArgumentException("The standard OneBot 12 HTTP action endpoint path must be '/'.", nameof(Endpoint));
        }
    }

    private static string? NormalizeToken(string? token)
    {
        // The protocol disables authentication only for a missing or empty token; whitespace remains significant.
        // 协议仅在令牌缺失或为空字符串时禁用鉴权；空白字符仍具有实际意义。
        return string.IsNullOrEmpty(token) ? null : token;
    }
}
