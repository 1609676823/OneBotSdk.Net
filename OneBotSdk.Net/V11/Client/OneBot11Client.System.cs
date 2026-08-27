using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

public sealed partial class OneBot11Client
{
    /// <summary>Gets cookies for an optional domain. / 获取可选域名的 Cookies。</summary>
    public Task<OneBot11Response<OneBot11CookiesData>> GetCookiesAsync(
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
            OneBot11Actions.GetCookies,
            new JsonObject { ["domain"] = domain },
            OneBot11CookiesData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets the QQ CSRF token. / 获取 QQ CSRF Token。</summary>
    public Task<OneBot11Response<OneBot11CsrfTokenData>> GetCsrfTokenAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetCsrfToken,
            null,
            OneBot11CsrfTokenData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets cookies and the CSRF token in one response. / 在一个响应中获取 Cookies 与 CSRF Token。</summary>
    public Task<OneBot11Response<OneBot11CredentialsData>> GetCredentialsAsync(
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
            OneBot11Actions.GetCredentials,
            new JsonObject { ["domain"] = domain },
            OneBot11CredentialsData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets and converts a received record file. / 获取并转换收到的语音文件。</summary>
    public Task<OneBot11Response<OneBot11FileData>> GetRecordAsync(
        string file,
        OneBot11RecordFormat outputFormat,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        return SendTypedAsync(
            OneBot11Actions.GetRecord,
            new JsonObject
            {
                ["file"] = file,
                ["out_format"] = outputFormat.ToProtocolValue()
            },
            OneBot11FileData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets a received image file. / 获取收到的图片文件。</summary>
    public Task<OneBot11Response<OneBot11FileData>> GetImageAsync(
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
            OneBot11Actions.GetImage,
            new JsonObject { ["file"] = file },
            OneBot11FileData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Checks whether the implementation can send images. / 检查实现端是否可以发送图片。</summary>
    public Task<OneBot11Response<OneBot11CapabilityData>> CanSendImageAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.CanSendImage,
            null,
            OneBot11CapabilityData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Checks whether the implementation can send records. / 检查实现端是否可以发送语音。</summary>
    public Task<OneBot11Response<OneBot11CapabilityData>> CanSendRecordAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.CanSendRecord,
            null,
            OneBot11CapabilityData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets portable implementation health information. / 获取可移植的实现端健康信息。</summary>
    public Task<OneBot11Response<OneBot11StatusData>> GetStatusAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetStatus,
            null,
            OneBot11StatusData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Gets portable implementation and protocol version information. / 获取可移植的实现端与协议版本信息。</summary>
    public Task<OneBot11Response<OneBot11VersionInfoData>> GetVersionInfoAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetVersionInfo,
            null,
            OneBot11VersionInfoData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Requests an inherently asynchronous implementation restart after an optional delay. / 请求实现端在可选延迟后执行固有异步重启。</summary>
    public Task<OneBot11Response> SetRestartAsync(
        long delay = 0,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot11Actions.SetRestart,
            new JsonObject { ["delay"] = delay },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>Cleans implementation cache files. / 清理实现端缓存文件。</summary>
    public Task<OneBot11Response> CleanCacheAsync(
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot11Actions.CleanCache,
            null,
            invocationMode,
            echo,
            cancellationToken);
    }
}
