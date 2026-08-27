using System.Text.Json.Serialization;
using OneBotSdk.Net.V10.Json;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Represents the standard and implementation-specific fields of OneBot runtime status.
/// 表示 OneBot 运行状态的标准字段和实现端特有字段。
/// </summary>
public sealed class OneBot10Status : OneBot10JsonModel
{
    internal OneBot10Status()
    {
    }

    /// <summary>
    /// Gets whether the application framework has been initialized.
    /// 获取应用框架是否已经初始化。
    /// </summary>
    [JsonPropertyName("app_initialized")]
    public bool? AppInitialized { get; internal set; }

    /// <summary>
    /// Gets whether the application framework is enabled.
    /// 获取应用框架是否已经启用。
    /// </summary>
    [JsonPropertyName("app_enabled")]
    public bool? AppEnabled { get; internal set; }

    /// <summary>
    /// Gets whether all loaded plugins are operating normally.
    /// 获取所有已加载插件是否运行正常。
    /// </summary>
    [JsonPropertyName("plugins_good")]
    public bool? PluginsGood { get; internal set; }

    /// <summary>
    /// Gets whether the application framework is operating normally.
    /// 获取应用框架是否运行正常。
    /// </summary>
    [JsonPropertyName("app_good")]
    public bool? AppGood { get; internal set; }

    /// <summary>
    /// Gets whether QQ is online; null means the implementation cannot determine it.
    /// 获取 QQ 是否在线；null 表示实现端无法确定。
    /// </summary>
    [JsonPropertyName("online")]
    public bool? Online { get; internal set; }

    /// <summary>
    /// Gets whether all modules operate as expected and QQ is online.
    /// 获取各模块是否正常工作且 QQ 在线。
    /// </summary>
    [JsonPropertyName("good")]
    public bool? Good { get; internal set; }
}
