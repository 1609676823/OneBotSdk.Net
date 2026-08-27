using System;
using OneBotSdk.Net.V12.Transports.Http;

namespace OneBotSdk.Net.V12.Client;

/// <summary>
/// Configures one OneBot 12 HTTP action endpoint together with its own token and response limit.
/// 将一个 OneBot 12 HTTP 动作终结点与它自己的令牌及响应限制绑定配置。
/// </summary>
public sealed class OneBot12ActionEndpointOptions
{
    private Uri _address;
    private int _maxResponseBodyBytes = 4 * 1024 * 1024;

    /// <summary>
    /// Creates <c>http://host:port/</c> and associates the token only with this action endpoint.
    /// 创建 <c>http://host:port/</c> 并将令牌仅与当前动作终结点关联。
    /// </summary>
    /// <param name="host">The action-server host name or IP address. / 动作服务器主机名或 IP 地址。</param>
    /// <param name="port">The action-server TCP port. / 动作服务器 TCP 端口。</param>
    /// <param name="accessToken">The token used only by this action endpoint. / 仅供当前动作终结点使用的令牌。</param>
    public OneBot12ActionEndpointOptions(string host, int port, string? accessToken = null)
        : this(CreateAddress(host, port), accessToken)
    {
    }

    /// <summary>
    /// Initializes the absolute root HTTP or HTTPS endpoint required by OneBot 12.
    /// 初始化 OneBot 12 要求的绝对 HTTP 或 HTTPS 根终结点。
    /// </summary>
    /// <param name="address">The complete root action address. / 完整的根动作地址。</param>
    /// <param name="accessToken">The token used only by this action endpoint. / 仅供当前动作终结点使用的令牌。</param>
    public OneBot12ActionEndpointOptions(Uri address, string? accessToken = null)
    {
        _address = ValidateAddress(address, nameof(address));
        AccessToken = accessToken;
    }

    /// <summary>
    /// Gets or sets the absolute root HTTP/HTTPS address paired with <see cref="AccessToken"/>.
    /// Every action envelope is posted directly to this root address.
    /// 获取或设置与 <see cref="AccessToken"/> 配对的绝对 HTTP/HTTPS 根地址。
    /// 每个动作信封都会直接 POST 到此根地址。
    /// </summary>
    public Uri Address
    {
        get => _address;
        set => _address = ValidateAddress(value, nameof(Address));
    }

    /// <summary>
    /// Gets or sets the exact access token sent only to <see cref="Address"/>.
    /// The transport uses a Bearer header when possible and otherwise uses the standard query fallback.
    /// This value is never reused for the event endpoint.
    /// 获取或设置仅发送到 <see cref="Address"/> 的精确访问令牌。
    /// 传输层在可行时使用 Bearer 请求头，否则使用标准查询参数回退。
    /// 此值绝不会自动复用于事件终结点。
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>Gets or sets the largest accepted response body in bytes. / 获取或设置允许接收的最大响应正文字节数。</summary>
    public int MaxResponseBodyBytes
    {
        get => _maxResponseBodyBytes;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxResponseBodyBytes), "The value must be greater than zero.");
            }

            _maxResponseBodyBytes = value;
        }
    }

    internal OneBot12HttpActionTransportOptions CreateTransportOptions()
    {
        return new OneBot12HttpActionTransportOptions(Address)
        {
            AccessToken = AccessToken,
            MaxResponseBodyBytes = MaxResponseBodyBytes
        }.Snapshot();
    }

    internal static OneBot12ActionEndpointOptions FromTransportOptions(
        OneBot12HttpActionTransportOptions transportOptions)
    {
        if (transportOptions == null)
        {
            throw new ArgumentNullException(nameof(transportOptions));
        }

        var snapshot = transportOptions.Snapshot();
        return new OneBot12ActionEndpointOptions(snapshot.Endpoint, snapshot.AccessToken)
        {
            MaxResponseBodyBytes = snapshot.MaxResponseBodyBytes
        };
    }

    private static Uri ValidateAddress(Uri? address, string parameterName)
    {
        if (address == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        // Snapshot validation is the single wire-level rule source, including the mandatory root path.
        // 快照校验是唯一的线协议规则来源，其中包括必须使用根路径的限制。
        return new OneBot12HttpActionTransportOptions(address).Snapshot().Endpoint;
    }

    private static Uri CreateAddress(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("An action-server host is required.", nameof(host));
        }

        if (port < 1 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "A TCP port must be between 1 and 65535.");
        }

        try
        {
            return new UriBuilder(Uri.UriSchemeHttp, host, port, "/").Uri;
        }
        catch (UriFormatException exception)
        {
            throw new ArgumentException("The action-server host is not valid.", nameof(host), exception);
        }
    }
}
