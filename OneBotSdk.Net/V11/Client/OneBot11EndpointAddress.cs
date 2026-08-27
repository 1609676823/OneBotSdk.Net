using System;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Creates conventional endpoint addresses while keeping action and event configuration independent.
/// 创建常规终结点地址，同时保持动作与事件配置相互独立。
/// </summary>
internal static class OneBot11EndpointAddress
{
    internal static Uri CreateHttpAction(string host, int port)
    {
        return Create(Uri.UriSchemeHttp, host, port, "/");
    }

    internal static Uri CreateWebSocketEvent(string host, int port)
    {
        return Create("ws", host, port, "/event");
    }

    private static Uri Create(string scheme, string host, int port, string path)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("A host name or IP address is required.", nameof(host));
        }

        if (port < 1 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "The port must be between 1 and 65535.");
        }

        return new UriBuilder(scheme, host, port, path).Uri;
    }
}
