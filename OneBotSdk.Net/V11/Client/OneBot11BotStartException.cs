using System;
using System.Globalization;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

/// <summary>
/// Represents a protocol-level startup failure returned by the <c>get_login_info</c> action.
/// 表示 <c>get_login_info</c> 动作返回的协议级启动失败。
/// </summary>
public sealed class OneBot11BotStartException : Exception
{
    /// <summary>
    /// Initializes a startup exception while preserving the complete typed and raw login response.
    /// 初始化启动异常，并保留完整的强类型及原始登录响应。
    /// </summary>
    /// <param name="loginInfoResponse">The failed login-information response. / 失败的登录信息响应。</param>
    public OneBot11BotStartException(OneBot11Response<OneBot11LoginInfoData> loginInfoResponse)
        : base(CreateMessage(loginInfoResponse))
    {
        LoginInfoResponse = loginInfoResponse;
    }

    /// <summary>
    /// Gets the complete failed response for status, return-code, raw JSON, and implementation-specific diagnostics.
    /// 获取完整失败响应，用于读取状态、返回码、原始 JSON 和实现端诊断信息。
    /// </summary>
    public OneBot11Response<OneBot11LoginInfoData> LoginInfoResponse { get; }

    private static string CreateMessage(OneBot11Response<OneBot11LoginInfoData>? response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var status = response.Status ?? "<missing>";
        var retCode = response.RetCode.HasValue
            ? response.RetCode.Value.ToString(CultureInfo.InvariantCulture)
            : "<missing>";
        return "The OneBot bot could not start because get_login_info returned status '" +
               status + "' and retcode '" + retCode + "'.";
    }
}
