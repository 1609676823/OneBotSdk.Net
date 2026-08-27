using System;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

/// <summary>
/// Preserves both successful meta-action exchanges performed before an event connection starts.
/// 保留事件连接启动前执行的两次成功元动作交互。
/// </summary>
public sealed class OneBot12BotStartResult
{
    /// <summary>Initializes a complete startup result. / 初始化完整启动结果。</summary>
    /// <param name="versionResponse">The successful <c>get_version</c> response. / 成功的 <c>get_version</c> 响应。</param>
    /// <param name="statusResponse">The successful <c>get_status</c> response. / 成功的 <c>get_status</c> 响应。</param>
    public OneBot12BotStartResult(
        OneBot12Response<OneBot12VersionData> versionResponse,
        OneBot12Response<OneBot12StatusData> statusResponse)
    {
        VersionResponse = versionResponse ?? throw new ArgumentNullException(nameof(versionResponse));
        StatusResponse = statusResponse ?? throw new ArgumentNullException(nameof(statusResponse));
    }

    /// <summary>Gets the complete typed and raw <c>get_version</c> response. / 获取完整的强类型及原始 <c>get_version</c> 响应。</summary>
    public OneBot12Response<OneBot12VersionData> VersionResponse { get; }

    /// <summary>Gets the complete typed and raw <c>get_status</c> response. / 获取完整的强类型及原始 <c>get_status</c> 响应。</summary>
    public OneBot12Response<OneBot12StatusData> StatusResponse { get; }
}
