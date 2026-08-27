using System;
using OneBotSdk.Net.V11.Transports;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Configures the HTTP action endpoint used by <see cref="OneBot11Bot"/>.
/// The SDK acts as the HTTP client and sends actions to the OneBot implementation's HTTP server.
/// 配置 <see cref="OneBot11Bot"/> 使用的 HTTP 动作终结点。
/// SDK 作为 HTTP 客户端，主动向 OneBot 实现端的 HTTP 服务器发送动作请求。
/// </summary>
/// <remarks>
/// The OneBot 11 specification calls this communication mode <c>HTTP</c>; in directional terms it is
/// often described as forward HTTP. This is the <c>actionEndpoint</c> argument of
/// <see cref="OneBot11BotOptions"/>, not a general transport-mode selector.
/// OneBot 11 规范将此通信方式称为 <c>HTTP</c>；按连接方向也常称为正向 HTTP。
/// 它表示 <see cref="OneBot11BotOptions"/> 的 <c>actionEndpoint</c> 参数，
/// 不是通用的传输模式选择器。
/// </remarks>
public sealed class OneBot11ActionEndpointOptions
{
    private Uri _address;
    private int _maxResponseBodyBytes = 4 * 1024 * 1024;

    /// <summary>
    /// Creates <c>http://{host}:{port}/</c> and associates the token only with this action endpoint.
    /// 创建 <c>http://{host}:{port}/</c>，并将令牌仅关联到当前动作终结点。
    /// </summary>
    /// <param name="host">The OneBot implementation's HTTP action server host name or IP address. / OneBot 实现端 HTTP 动作服务器的主机名或 IP 地址。</param>
    /// <param name="port">The action server TCP port. / 动作服务器 TCP 端口。</param>
    /// <param name="accessToken">The token used only by this action endpoint. / 仅供当前动作终结点使用的令牌。</param>
    public OneBot11ActionEndpointOptions(string host, int port, string? accessToken = null)
        : this(OneBot11EndpointAddress.CreateHttpAction(host, port), accessToken)
    {
    }

    /// <summary>
    /// Initializes an absolute HTTP or HTTPS action endpoint, including an optional deployment reverse-proxy path.
    /// The reverse proxy is unrelated to OneBot reverse HTTP or reverse WebSocket communication.
    /// 初始化绝对 HTTP 或 HTTPS 动作终结点，并支持可选的部署层反向代理路径。
    /// 这里的反向代理与 OneBot 的反向 HTTP 或反向 WebSocket 通信无关。
    /// </summary>
    /// <param name="address">The complete action base address, including any reverse-proxy path. / 完整的动作基础地址，包括反向代理路径。</param>
    /// <param name="accessToken">The token used only by this action endpoint. / 仅供当前动作终结点使用的令牌。</param>
    public OneBot11ActionEndpointOptions(Uri address, string? accessToken = null)
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
            OneBot11HttpActionTransportOptions.ValidatePositive(value, nameof(MaxResponseBodyBytes));
            _maxResponseBodyBytes = value;
        }
    }

    internal OneBot11HttpActionTransportOptions CreateTransportOptions()
    {
        var address = Address ?? throw new ArgumentNullException(nameof(Address));
        var accessToken = AccessToken;
        var maxResponseBodyBytes = MaxResponseBodyBytes;
        return new OneBot11HttpActionTransportOptions(address)
        {
            AccessToken = accessToken,
            MaxResponseBodyBytes = maxResponseBodyBytes
        }.Snapshot();
    }

    private static Uri ValidateAddress(Uri? address, string parameterName)
    {
        var value = address ?? throw new ArgumentNullException(parameterName);
        OneBot11HttpActionTransportOptions.ValidateAbsoluteHttpUri(value, parameterName);
        return value;
    }
}
