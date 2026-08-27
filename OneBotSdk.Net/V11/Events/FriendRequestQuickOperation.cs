using System.Text.Json.Serialization;

namespace OneBotSdk.Net.V11.Events;

/// <summary>
/// Defines optional quick operations returned for a friend request.
/// 定义加好友请求响应中可选的快速操作。
/// </summary>
public sealed class FriendRequestQuickOperation
{
    /// <summary>
    /// Gets or sets whether to approve; null means do not process the request.
    /// 获取或设置是否同意；null 表示不处理请求。
    /// </summary>
    [JsonPropertyName("approve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Approve { get; set; }

    /// <summary>Gets or sets the friend remark used only when approving. / 获取或设置仅在同意时使用的好友备注。</summary>
    [JsonPropertyName("remark")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Remark { get; set; }
}
