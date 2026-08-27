using System;
using System.Text;

namespace OneBotSdk.Net.V12.Transports.WebSockets;

/// <summary>Captures the standard headers and token fallback from a reverse WebSocket handshake. / 保存反向 WebSocket 握手中的标准请求头与令牌回退值。</summary>
public sealed class OneBot12ReverseWebSocketMetadata
{
    /// <summary>Initializes captured reverse-handshake metadata. / 初始化已捕获的反向握手元数据。</summary>
    public OneBot12ReverseWebSocketMetadata(
        string? userAgent,
        string? subprotocol,
        string? authorization,
        string? accessTokenQuery = null)
    {
        UserAgent = userAgent;
        Subprotocol = subprotocol;
        Authorization = authorization;
        AccessTokenQuery = accessTokenQuery;

        var protocol = subprotocol;
        if (protocol != null && protocol.Length != 0)
        {
            var separator = protocol.IndexOf('.');
            if (separator > 0 && separator < protocol.Length - 1)
            {
                OneBotVersion = protocol.Substring(0, separator);
                Implementation = protocol.Substring(separator + 1);
            }
        }
    }

    /// <summary>Gets the User-Agent header. / 获取 User-Agent 请求头。</summary>
    public string? UserAgent { get; }

    /// <summary>Gets the Sec-WebSocket-Protocol header value. / 获取 Sec-WebSocket-Protocol 请求头值。</summary>
    public string? Subprotocol { get; }

    /// <summary>Gets the Authorization header value. / 获取 Authorization 请求头值。</summary>
    public string? Authorization { get; }

    /// <summary>Gets the access_token query fallback. / 获取 access_token 查询参数回退值。</summary>
    public string? AccessTokenQuery { get; }

    /// <summary>Gets the protocol-version portion of the subprotocol. / 获取子协议中的协议版本部分。</summary>
    public string? OneBotVersion { get; }

    /// <summary>Gets the implementation-name portion of the subprotocol. / 获取子协议中的实现端名称部分。</summary>
    public string? Implementation { get; }

    internal bool HasValidImplementationName()
    {
        var value = Implementation;
        if (string.IsNullOrEmpty(value) || value![0] < 'a' || value[0] > 'z')
        {
            return false;
        }

        var previousWasDot = false;
        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '.')
            {
                if (previousWasDot || index == value.Length - 1)
                {
                    return false;
                }

                previousWasDot = true;
                continue;
            }

            if (!((character >= 'a' && character <= 'z') ||
                  (character >= '0' && character <= '9') ||
                  character == '-'))
            {
                return false;
            }

            previousWasDot = false;
        }

        return true;
    }

    /// <summary>Checks the Bearer header when present; only an absent header permits the query fallback. / 存在 Bearer 请求头时仅校验该请求头；只有请求头缺失时才允许使用查询参数回退。</summary>
    public bool HasAccessToken(string expectedAccessToken)
    {
        if (expectedAccessToken == null)
        {
            throw new ArgumentNullException(nameof(expectedAccessToken));
        }

        // A supplied Authorization header takes precedence, including when it carries a wrong value.
        // 已提供的 Authorization 请求头优先级最高，即使它携带了错误值也不例外。
        return Authorization != null
            ? FixedTimeEquals(Authorization, "Bearer " + expectedAccessToken)
            : FixedTimeEquals(AccessTokenQuery, expectedAccessToken);
    }

    private static bool FixedTimeEquals(string? left, string right)
    {
        if (left == null)
        {
            return false;
        }

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        var difference = leftBytes.Length ^ rightBytes.Length;
        var length = Math.Max(leftBytes.Length, rightBytes.Length);
        for (var index = 0; index < length; index++)
        {
            var leftByte = index < leftBytes.Length ? leftBytes[index] : (byte)0;
            var rightByte = index < rightBytes.Length ? rightBytes[index] : (byte)0;
            difference |= leftByte ^ rightByte;
        }

        return difference == 0;
    }
}
