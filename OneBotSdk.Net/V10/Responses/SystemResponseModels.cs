using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Responses;

/// <summary>
/// Contains cookies returned by a OneBot implementation.
/// 包含 OneBot 实现端返回的 Cookies。
/// </summary>
public sealed class OneBot10CookiesData : OneBot10JsonModel
{
    internal static OneBot10CookiesData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10CookiesData
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
public sealed class OneBot10CsrfTokenData : OneBot10JsonModel
{
    internal static OneBot10CsrfTokenData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10CsrfTokenData
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
public sealed class OneBot10CredentialsData : OneBot10JsonModel
{
    internal static OneBot10CredentialsData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10CredentialsData
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
public sealed class OneBot10FileData : OneBot10JsonModel
{
    internal static OneBot10FileData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10FileData
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
public sealed class OneBot10CapabilityData : OneBot10JsonModel
{
    internal static OneBot10CapabilityData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10CapabilityData
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
/// Contains the complete standard <c>get_status</c> result while retaining implementation extensions.
/// 包含完整的标准 <c>get_status</c> 结果，同时保留实现扩展字段。
/// </summary>
public sealed class OneBot10StatusData : OneBot10JsonModel
{
    internal static OneBot10StatusData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10StatusData
            {
                RawJson = TolerantJson.CloneObject(source),
                AppInitialized = TolerantJson.Boolean(source, "app_initialized"),
                AppEnabled = TolerantJson.Boolean(source, "app_enabled"),
                PluginsGood = TolerantJson.Boolean(source, "plugins_good"),
                AppGood = TolerantJson.Boolean(source, "app_good"),
                Online = TolerantJson.Boolean(source, "online"),
                Good = TolerantJson.Boolean(source, "good")
            };
    }

    /// <summary>
    /// Gets whether the application framework has been initialized.
    /// 获取应用框架是否已经初始化。
    /// </summary>
    [JsonPropertyName("app_initialized")]
    public bool? AppInitialized { get; private set; }

    /// <summary>
    /// Gets whether the application framework is enabled.
    /// 获取应用框架是否已经启用。
    /// </summary>
    [JsonPropertyName("app_enabled")]
    public bool? AppEnabled { get; private set; }

    /// <summary>
    /// Gets whether all loaded plugins are operating normally.
    /// 获取所有已加载插件是否运行正常。
    /// </summary>
    [JsonPropertyName("plugins_good")]
    public bool? PluginsGood { get; private set; }

    /// <summary>
    /// Gets whether the application framework is operating normally.
    /// 获取应用框架是否运行正常。
    /// </summary>
    [JsonPropertyName("app_good")]
    public bool? AppGood { get; private set; }

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
/// Contains the CQHTTP and CKYU version fields defined by OneBot 10.
/// 包含 OneBot 10 定义的 CQHTTP 与 CKYU 版本字段。
/// </summary>
public sealed class OneBot10VersionInfoData : OneBot10JsonModel
{
    internal static OneBot10VersionInfoData? Parse(JsonNode? node)
    {
        var source = TolerantJson.Object(node);
        return source == null
            ? null
            : new OneBot10VersionInfoData
            {
                RawJson = TolerantJson.CloneObject(source),
                CoolqDirectory = TolerantJson.String(source, "coolq_directory"),
                CoolqEdition = TolerantJson.String(source, "coolq_edition"),
                PluginVersion = TolerantJson.String(source, "plugin_version"),
                PluginBuildNumber = TolerantJson.Int64(source, "plugin_build_number"),
                PluginBuildConfiguration = TolerantJson.String(source, "plugin_build_configuration")
            };
    }

    /// <summary>
    /// Gets the compatibility CKYU root directory.
    /// 获取兼容性的 CKYU 根目录。
    /// </summary>
    [JsonPropertyName("coolq_directory")]
    public string? CoolqDirectory { get; private set; }

    /// <summary>
    /// Gets the CKYU edition, normally <c>air</c> or <c>pro</c>.
    /// 获取 CKYU 版本，通常为 <c>air</c> 或 <c>pro</c>。
    /// </summary>
    [JsonPropertyName("coolq_edition")]
    public string? CoolqEdition { get; private set; }

    /// <summary>
    /// Gets the CQHTTP plug-in version.
    /// 获取 CQHTTP 插件版本。
    /// </summary>
    [JsonPropertyName("plugin_version")]
    public string? PluginVersion { get; private set; }

    /// <summary>Gets the CQHTTP build number. / 获取 CQHTTP 构建号。</summary>
    [JsonPropertyName("plugin_build_number")]
    public long? PluginBuildNumber { get; private set; }

    /// <summary>Gets the CQHTTP build configuration. / 获取 CQHTTP 构建配置。</summary>
    [JsonPropertyName("plugin_build_configuration")]
    public string? PluginBuildConfiguration { get; private set; }
}
