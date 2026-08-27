using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Responses;

/// <summary>
/// Contains cookies returned by a OneBot implementation.
/// 包含 OneBot 实现端返回的 Cookies。
/// </summary>
public sealed class OneBot11CookiesData : OneBot11JsonModel
{
    internal static OneBot11CookiesData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11CookiesData
            {
                RawJson = TolerantJson.CloneObject(source),
                Cookies = TolerantJson.String(source, "cookies")
            };
    }

    /// <summary>
    /// Gets the cookie header value.
    /// 获取 Cookie 请求头值。
    /// </summary>
    [JsonPropertyName("cookies")]
    public string? Cookies { get; private set; }
}

/// <summary>
/// Contains the CSRF token returned by <c>get_csrf_token</c>.
/// 包含 <c>get_csrf_token</c> 返回的 CSRF Token。
/// </summary>
public sealed class OneBot11CsrfTokenData : OneBot11JsonModel
{
    internal static OneBot11CsrfTokenData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11CsrfTokenData
            {
                RawJson = TolerantJson.CloneObject(source),
                Token = TolerantJson.Int64(source, "token")
            };
    }

    /// <summary>
    /// Gets the CSRF token.
    /// 获取 CSRF Token。
    /// </summary>
    [JsonPropertyName("token")]
    public long? Token { get; private set; }
}

/// <summary>
/// Contains the combined cookies and CSRF token returned by <c>get_credentials</c>.
/// 包含 <c>get_credentials</c> 返回的 Cookies 与 CSRF Token。
/// </summary>
public sealed class OneBot11CredentialsData : OneBot11JsonModel
{
    internal static OneBot11CredentialsData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11CredentialsData
            {
                RawJson = TolerantJson.CloneObject(source),
                Cookies = TolerantJson.String(source, "cookies"),
                CsrfToken = TolerantJson.Int64(source, "csrf_token")
            };
    }

    /// <summary>
    /// Gets the cookie header value.
    /// 获取 Cookie 请求头值。
    /// </summary>
    [JsonPropertyName("cookies")]
    public string? Cookies { get; private set; }

    /// <summary>
    /// Gets the CSRF token; this combined action uses the protocol field <c>csrf_token</c>.
    /// 获取 CSRF Token；该组合动作使用协议字段 <c>csrf_token</c>。
    /// </summary>
    [JsonPropertyName("csrf_token")]
    public long? CsrfToken { get; private set; }
}

/// <summary>
/// Contains a local file path returned by a media retrieval action.
/// 包含媒体获取动作返回的本地文件路径。
/// </summary>
public sealed class OneBot11FileData : OneBot11JsonModel
{
    internal static OneBot11FileData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11FileData
            {
                RawJson = TolerantJson.CloneObject(source),
                File = TolerantJson.String(source, "file")
            };
    }

    /// <summary>
    /// Gets the implementation-local file path.
    /// 获取实现端本地文件路径。
    /// </summary>
    [JsonPropertyName("file")]
    public string? File { get; private set; }
}

/// <summary>
/// Contains a standard OneBot capability check result.
/// 包含标准 OneBot 能力检查结果。
/// </summary>
public sealed class OneBot11CapabilityData : OneBot11JsonModel
{
    internal static OneBot11CapabilityData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11CapabilityData
            {
                RawJson = TolerantJson.CloneObject(source),
                Yes = TolerantJson.Boolean(source, "yes")
            };
    }

    /// <summary>
    /// Gets whether the capability is available.
    /// 获取该能力是否可用。
    /// </summary>
    [JsonPropertyName("yes")]
    public bool? Yes { get; private set; }
}

/// <summary>
/// Contains the portable portion of <c>get_status</c> while retaining implementation extensions.
/// 包含 <c>get_status</c> 的可移植部分，同时保留实现扩展字段。
/// </summary>
public sealed class OneBot11StatusData : OneBot11JsonModel
{
    internal static OneBot11StatusData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11StatusData
            {
                RawJson = TolerantJson.CloneObject(source),
                Online = TolerantJson.Boolean(source, "online"),
                Good = TolerantJson.Boolean(source, "good")
            };
    }

    /// <summary>
    /// Gets whether QQ is online, or <see langword="null"/> when the implementation cannot determine it.
    /// 获取 QQ 是否在线；实现端无法确定时为 <see langword="null"/>。
    /// </summary>
    [JsonPropertyName("online")]
    public bool? Online { get; private set; }

    /// <summary>
    /// Gets whether the implementation is healthy and the account is online.
    /// 获取实现端是否健康且账号在线。
    /// </summary>
    [JsonPropertyName("good")]
    public bool? Good { get; private set; }
}

/// <summary>
/// Contains portable OneBot implementation version information.
/// 包含可移植的 OneBot 实现版本信息。
/// </summary>
public sealed class OneBot11VersionInfoData : OneBot11JsonModel
{
    internal static OneBot11VersionInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot11VersionInfoData
            {
                RawJson = TolerantJson.CloneObject(source),
                AppName = TolerantJson.String(source, "app_name"),
                AppVersion = TolerantJson.String(source, "app_version"),
                ProtocolVersion = TolerantJson.String(source, "protocol_version")
            };
    }

    /// <summary>
    /// Gets the implementation application identifier.
    /// 获取实现应用标识。
    /// </summary>
    [JsonPropertyName("app_name")]
    public string? AppName { get; private set; }

    /// <summary>
    /// Gets the implementation application version.
    /// 获取实现应用版本。
    /// </summary>
    [JsonPropertyName("app_version")]
    public string? AppVersion { get; private set; }

    /// <summary>
    /// Gets the OneBot protocol version, normally <c>v11</c>.
    /// 获取 OneBot 协议版本，通常为 <c>v11</c>。
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public string? ProtocolVersion { get; private set; }
}
