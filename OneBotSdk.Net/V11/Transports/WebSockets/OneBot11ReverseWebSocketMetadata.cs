using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using OneBotSdk.Net.V11.Transports.Http;

namespace OneBotSdk.Net.V11.Transports.WebSockets;

/// <summary>
/// Describes the role declared by a OneBot 11 reverse WebSocket connection.
/// 描述 OneBot 11 反向 WebSocket 连接声明的角色。
/// </summary>
public enum OneBot11ReverseWebSocketRole
{
    /// <summary>
    /// The peer omitted or supplied an unrecognized role.
    /// 对端未提供角色或提供了无法识别的角色。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The connection carries both actions and events.
    /// 连接同时承载动作和事件。
    /// </summary>
    Universal,

    /// <summary>
    /// The connection is intended for action requests and responses.
    /// 连接用于动作请求和响应。
    /// </summary>
    Api,

    /// <summary>
    /// The connection is intended for events only.
    /// 连接仅用于事件。
    /// </summary>
    Event
}

/// <summary>
/// Captures standard reverse WebSocket handshake headers without depending on an ASP.NET host.
/// 在不依赖 ASP.NET 主机的情况下保存标准反向 WebSocket 握手请求头。
/// </summary>
public sealed class OneBot11ReverseWebSocketMetadata
{
    /// <summary>
    /// Standard header containing the OneBot implementation account ID.
    /// 包含 OneBot 实现端账号 ID 的标准请求头。
    /// </summary>
    public const string SelfIdHeader = "X-Self-ID";

    /// <summary>
    /// Standard header declaring Universal, API, or Event connection role.
    /// 声明 Universal、API 或 Event 连接角色的标准请求头。
    /// </summary>
    public const string ClientRoleHeader = "X-Client-Role";

    private readonly IReadOnlyDictionary<string, string> _headers;

    private OneBot11ReverseWebSocketMetadata(Dictionary<string, string> headers)
    {
        _headers = new ReadOnlyDictionary<string, string>(headers);

        SelfId = GetValueOrNull(headers, SelfIdHeader);
        ClientRole = GetValueOrNull(headers, ClientRoleHeader);
        UserAgent = GetValueOrNull(headers, "User-Agent");
        Authorization = GetValueOrNull(headers, "Authorization");
        Role = ParseRole(ClientRole);

        long parsedSelfId;
        SelfIdNumber = long.TryParse(SelfId, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedSelfId)
            ? parsedSelfId
            : (long?)null;
    }

    /// <summary>
    /// Gets all captured headers using case-insensitive key lookup.
    /// 获取全部已保存请求头，键查找不区分大小写。
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers => _headers;

    /// <summary>
    /// Gets the raw <c>X-Self-ID</c> value.
    /// 获取原始 <c>X-Self-ID</c> 值。
    /// </summary>
    public string? SelfId { get; }

    /// <summary>
    /// Gets the numeric self ID when the header contains an Int64 value.
    /// 当请求头包含 Int64 值时获取数字形式的自身 ID。
    /// </summary>
    public long? SelfIdNumber { get; }

    /// <summary>
    /// Gets the raw <c>X-Client-Role</c> value.
    /// 获取原始 <c>X-Client-Role</c> 值。
    /// </summary>
    public string? ClientRole { get; }

    /// <summary>
    /// Gets the normalized connection role.
    /// 获取规范化后的连接角色。
    /// </summary>
    public OneBot11ReverseWebSocketRole Role { get; }

    /// <summary>
    /// Gets the peer user-agent header.
    /// 获取对端 User-Agent 请求头。
    /// </summary>
    public string? UserAgent { get; }

    /// <summary>
    /// Gets the raw authorization header.
    /// 获取原始 Authorization 请求头。
    /// </summary>
    public string? Authorization { get; }

    /// <summary>
    /// Creates metadata from host-neutral header name/value pairs.
    /// 从与宿主无关的请求头名称/值对创建元数据。
    /// </summary>
    public static OneBot11ReverseWebSocketMetadata FromHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
        if (headers == null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key))
            {
                continue;
            }

            string? existing;
            copy[header.Key] = copy.TryGetValue(header.Key, out existing) && existing != null
                ? existing + "," + (header.Value ?? string.Empty)
                : header.Value ?? string.Empty;
        }

        return new OneBot11ReverseWebSocketMetadata(copy);
    }

    /// <summary>
    /// Verifies the bearer token supplied during the reverse WebSocket handshake.
    /// 验证反向 WebSocket 握手期间提供的 Bearer 令牌。
    /// </summary>
    public bool HasBearerToken(string expectedAccessToken)
    {
        if (expectedAccessToken == null)
        {
            throw new ArgumentNullException(nameof(expectedAccessToken));
        }

        const string bearer = "Bearer ";
        if (Authorization == null || !Authorization.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes(expectedAccessToken);
        // Compare the exact token bytes after the fixed scheme separator; token whitespace is significant.
        // 在固定认证方案分隔符后按原始字节比较令牌；令牌中的空白具有实际意义。
        var supplied = Encoding.UTF8.GetBytes(Authorization.Substring(bearer.Length));
        return OneBot11HttpPostSignature.FixedTimeEquals(expected, supplied);
    }

    private static OneBot11ReverseWebSocketRole ParseRole(string? value)
    {
        if (string.Equals(value, "Universal", StringComparison.OrdinalIgnoreCase))
        {
            return OneBot11ReverseWebSocketRole.Universal;
        }

        if (string.Equals(value, "API", StringComparison.OrdinalIgnoreCase))
        {
            return OneBot11ReverseWebSocketRole.Api;
        }

        if (string.Equals(value, "Event", StringComparison.OrdinalIgnoreCase))
        {
            return OneBot11ReverseWebSocketRole.Event;
        }

        return OneBot11ReverseWebSocketRole.Unknown;
    }

    private static string? GetValueOrNull(Dictionary<string, string> source, string name)
    {
        string? value;
        return source.TryGetValue(name, out value) ? value : null;
    }
}
