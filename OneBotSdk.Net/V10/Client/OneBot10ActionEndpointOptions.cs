using System;
using OneBotSdk.Net.V10.Transports;

namespace OneBotSdk.Net.V10.Client;

/// <summary>
/// Configures one HTTP action endpoint together with its own access token and response limit.
/// 将一个 HTTP 动作终结点与它自己的访问令牌和响应限制配置在一起。
/// </summary>
public sealed class OneBot10ActionEndpointOptions
{
    private Uri _address;
    private int _maxResponseBodyBytes = 4 * 1024 * 1024;

    /// <summary>
    /// Creates <c>http://{host}:{port}/</c> and associates the token only with this action endpoint.
    /// 创建 <c>http://{host}:{port}/</c>，并将令牌仅关联到当前动作终结点。
    /// </summary>
    /// <param name="host">The action server host name or IP address. / 动作服务器主机名或 IP 地址。</param>
    /// <param name="port">The action server TCP port. / 动作服务器 TCP 端口。</param>
    /// <param name="accessToken">The token used only by this action endpoint. / 仅供当前动作终结点使用的令牌。</param>
    public OneBot10ActionEndpointOptions(string host, int port, string? accessToken = null)
        : this(OneBot10EndpointAddress.CreateHttpAction(host, port), accessToken)
    {
    }

    /// <summary>
    /// Initializes an absolute HTTP or HTTPS action endpoint, including an optional reverse-proxy path.
    /// 初始化绝对 HTTP 或 HTTPS 动作终结点，并支持可选的反向代理路径。
    /// </summary>
    /// <param name="address">The complete action base address, including any reverse-proxy path. / 完整的动作基础地址，包括反向代理路径。</param>
    /// <param name="accessToken">The token used only by this action endpoint. / 仅供当前动作终结点使用的令牌。</param>
    public OneBot10ActionEndpointOptions(Uri address, string? accessToken = null)
    {
        _address = ValidateAddress(address, nameof(address));
        AccessToken = accessToken;
    }

    /// <summary>
    /// Gets or sets the absolute HTTP/HTTPS base address paired with <see cref="AccessToken"/>.
    /// The SDK appends the escaped OneBot action name to this address.
    /// 获取或设置与 <see cref="AccessToken"/> 配对的绝对 HTTP/HTTPS 基础地址。
    /// SDK 会在此地址后追加经过转义的 OneBot action 名称。
    /// </summary>
    public Uri Address
    {
        get => _address;
        set => _address = ValidateAddress(value, nameof(Address));
    }

    /// <summary>
    /// Gets or sets the token sent only to <see cref="Address"/> as an <c>Authorization: Bearer</c> header.
    /// Null, empty, or whitespace means that no Authorization header is sent.
    /// A non-empty value must be valid for an HTTP Authorization header.
    /// This token is never reused for the event endpoint.
    /// 获取或设置仅向 <see cref="Address"/> 发送的 <c>Authorization: Bearer</c> 令牌。
    /// 值为 null、空字符串或纯空白时不发送 Authorization 请求头。
    /// 非空值必须符合 HTTP Authorization 请求头格式。
    /// 此令牌绝不会自动复用于事件终结点。
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the largest accepted response body from this action endpoint in bytes.
    /// 获取或设置从当前动作终结点接收的最大响应正文大小（字节）。
    /// </summary>
    public int MaxResponseBodyBytes
    {
        get => _maxResponseBodyBytes;
        set
        {
            OneBot10HttpActionTransportOptions.ValidatePositive(value, nameof(MaxResponseBodyBytes));
            _maxResponseBodyBytes = value;
        }
    }

    internal OneBot10HttpActionTransportOptions CreateTransportOptions()
    {
        var address = Address ?? throw new ArgumentNullException(nameof(Address));
        var accessToken = AccessToken;
        var maxResponseBodyBytes = MaxResponseBodyBytes;
        return new OneBot10HttpActionTransportOptions(address)
        {
            AccessToken = accessToken,
            MaxResponseBodyBytes = maxResponseBodyBytes
        }.Snapshot();
    }

    private static Uri ValidateAddress(Uri? address, string parameterName)
    {
        var value = address ?? throw new ArgumentNullException(parameterName);
        OneBot10HttpActionTransportOptions.ValidateAbsoluteHttpUri(value, parameterName);
        return value;
    }
}
