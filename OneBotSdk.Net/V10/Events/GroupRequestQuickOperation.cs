using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V10.Events;

/// <summary>
/// Defines optional quick operations returned for a group request or invitation.
/// 定义加群请求或邀请响应中可选的快速操作。
/// </summary>
public sealed class GroupRequestQuickOperation
{
    /// <summary>
    /// Gets or sets whether to approve; null means do not process the request.
    /// 获取或设置是否同意；null 表示不处理请求。
    /// </summary>
    [JsonPropertyName("approve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Approve { get; set; }

    /// <summary>Gets or sets the rejection reason used only when rejecting. / 获取或设置仅在拒绝时使用的理由。</summary>
    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }
}
