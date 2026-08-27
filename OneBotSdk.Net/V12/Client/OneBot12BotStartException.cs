using System;
using System.Globalization;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

/// <summary>Identifies the startup meta action that returned a failed protocol response. / 标识返回失败协议响应的启动元动作。</summary>
public enum OneBot12BotStartStage
{
    /// <summary>The <c>get_version</c> verification failed. / <c>get_version</c> 校验失败。</summary>
    GetVersion,

    /// <summary>The <c>get_status</c> verification failed. / <c>get_status</c> 校验失败。</summary>
    GetStatus
}

/// <summary>
/// Represents a protocol-level startup failure while preserving its complete typed and raw response.
/// 表示协议级启动失败，并保留完整的强类型及原始响应。
/// </summary>
public sealed class OneBot12BotStartException : Exception
{
    /// <summary>Initializes a startup failure for one verification stage. / 为一个校验阶段初始化启动失败。</summary>
    /// <param name="stage">The failed startup stage. / 失败的启动阶段。</param>
    /// <param name="response">The complete failed response. / 完整的失败响应。</param>
    public OneBot12BotStartException(OneBot12BotStartStage stage, OneBot12ResponseBase response)
        : base(CreateMessage(stage, response))
    {
        Stage = stage;
        Response = response;
    }

    /// <summary>Gets the failed verification stage. / 获取失败的校验阶段。</summary>
    public OneBot12BotStartStage Stage { get; }

    /// <summary>Gets the complete failed response, including raw request and response JSON. / 获取包含原始请求与响应 JSON 的完整失败响应。</summary>
    public OneBot12ResponseBase Response { get; }

    private static string CreateMessage(OneBot12BotStartStage stage, OneBot12ResponseBase? response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var action = stage == OneBot12BotStartStage.GetVersion ? "get_version" : "get_status";
        var status = response.Status ?? "<missing>";
        var retCode = response.RetCode.HasValue
            ? response.RetCode.Value.ToString(CultureInfo.InvariantCulture)
            : "<missing>";
        return "The OneBot 12 bot could not start because " + action + " returned status '" +
               status + "' and retcode '" + retCode + "'.";
    }
}
