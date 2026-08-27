using System.Text.Json.Serialization;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Represents the standard and implementation-specific fields of OneBot runtime status.
/// 表示 OneBot 运行状态的标准字段和实现端特有字段。
/// </summary>
public sealed class OneBot11Status : OneBot11JsonModel
{
    internal OneBot11Status()
    {
    }

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
