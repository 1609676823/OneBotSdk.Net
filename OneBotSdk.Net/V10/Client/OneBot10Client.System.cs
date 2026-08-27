using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

public sealed partial class OneBot10Client
{
    /// <summary>Gets cookies for an optional domain. / 获取可选域名的 Cookies。</summary>
    public Task<OneBot10Response<OneBot10CookiesData>> GetCookiesAsync(
        string domain = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return SendTypedAsync(
            OneBot10Actions.GetCookies,
            new JsonObject { ["domain"] = domain },
            OneBot10CookiesData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets the QQ CSRF token. / 获取 QQ CSRF Token。</summary>
    public Task<OneBot10Response<OneBot10CsrfTokenData>> GetCsrfTokenAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetCsrfToken,
            null,
            OneBot10CsrfTokenData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets cookies and the CSRF token in one response. / 在一个响应中获取 Cookies 与 CSRF Token。</summary>
    public Task<OneBot10Response<OneBot10CredentialsData>> GetCredentialsAsync(
        string domain = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return SendTypedAsync(
            OneBot10Actions.GetCredentials,
            new JsonObject { ["domain"] = domain },
            OneBot10CredentialsData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets and converts a received record file. / 获取并转换收到的语音文件。</summary>
    public Task<OneBot10Response<OneBot10FileData>> GetRecordAsync(
        string file,
        OneBot10RecordFormat outputFormat,
        bool fullPath = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        return SendTypedAsync(
            OneBot10Actions.GetRecord,
            new JsonObject
            {
                ["file"] = file,
                ["out_format"] = outputFormat.ToProtocolValue(),
                ["full_path"] = fullPath
            },
            OneBot10FileData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets a received image file. / 获取收到的图片文件。</summary>
    public Task<OneBot10Response<OneBot10FileData>> GetImageAsync(
        string file,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        return SendTypedAsync(
            OneBot10Actions.GetImage,
            new JsonObject { ["file"] = file },
            OneBot10FileData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Checks whether the implementation can send images. / 检查实现端是否可以发送图片。</summary>
    public Task<OneBot10Response<OneBot10CapabilityData>> CanSendImageAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.CanSendImage,
            null,
            OneBot10CapabilityData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Checks whether the implementation can send records. / 检查实现端是否可以发送语音。</summary>
    public Task<OneBot10Response<OneBot10CapabilityData>> CanSendRecordAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.CanSendRecord,
            null,
            OneBot10CapabilityData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets portable implementation health information. / 获取可移植的实现端健康信息。</summary>
    public Task<OneBot10Response<OneBot10StatusData>> GetStatusAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetStatus,
            null,
            OneBot10StatusData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets CQHTTP plug-in and CKYU host version information. / 获取 CQHTTP 插件与 CKYU 宿主版本信息。</summary>
    public Task<OneBot10Response<OneBot10VersionInfoData>> GetVersionInfoAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot10Actions.GetVersionInfo,
            null,
            OneBot10VersionInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Restarts the CQHTTP plug-in. The request is inherently asynchronous and may interrupt current connections.
    /// 重启 CQHTTP 插件。该请求固有异步，并可能中断当前连接。
    /// </summary>
    public Task<OneBot10Response> SetRestartPluginAsync(
        long delay = 0,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SetRestartPlugin,
            new JsonObject { ["delay"] = delay },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Deletes files from a selected CQHTTP data directory. This operation cannot be automatically undone.
    /// 删除指定 CQHTTP 数据目录中的文件。此操作无法自动撤销。
    /// </summary>
    public Task<OneBot10Response> CleanDataDirectoryAsync(
        OneBot10DataDirectory dataDirectory,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.CleanDataDirectory,
            new JsonObject { ["data_dir"] = dataDirectory.ToProtocolValue() },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Clears the CQHTTP plug-in log. This operation cannot be automatically undone.
    /// 清空 CQHTTP 插件日志。此操作无法自动撤销。
    /// </summary>
    public Task<OneBot10Response> CleanPluginLogAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.CleanPluginLog,
            null,
            invocationMode,
            echo,
            cancellationToken);
    }
}
