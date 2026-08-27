using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Polls buffered non-meta events from an HTTP implementation. / 从 HTTP 实现端轮询已缓冲的非元事件。</summary>
    /// <remarks>The returned objects retain every extension field and can be passed to the event parser. / 返回对象保留全部扩展字段，可继续传给事件解析器。</remarks>
    public Task<OneBot12Response<IReadOnlyList<OneBot12Event>>> GetLatestEventsAsync(
        long limit = 0,
        long timeoutSeconds = 0,
        string? echo = null,
        CancellationToken cancellationToken = default)
    {
        ValidateNonNegative(limit, nameof(limit));
        ValidateNonNegative(timeoutSeconds, nameof(timeoutSeconds));
        return SendMetaTypedAsync<IReadOnlyList<OneBot12Event>>(
            OneBot12Actions.GetLatestEvents,
            new JsonObject
            {
                ["limit"] = limit,
                ["timeout"] = timeoutSeconds
            },
            node => OneBot12ResponseDataParsers.ParseList(
                node,
                item =>
                {
                    var eventObject = TolerantJson.Object(item);
                    return eventObject == null ? null : OneBot12EventParser.Parse(eventObject);
                }),
            echo,
            cancellationToken);
    }

    /// <summary>Gets every action advertised by the implementation. / 获取实现端声明支持的全部动作。</summary>
    public Task<OneBot12Response<IReadOnlyList<string>>> GetSupportedActionsAsync(
        string? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendMetaTypedAsync<IReadOnlyList<string>>(
            OneBot12Actions.GetSupportedActions,
            null,
            OneBot12ResponseDataParsers.ParseStrings,
            echo,
            cancellationToken);
    }

    /// <summary>Gets implementation-wide and per-bot runtime status. / 获取实现端整体及各机器人运行状态。</summary>
    public Task<OneBot12Response<OneBot12StatusData>> GetStatusAsync(
        string? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendMetaTypedAsync(
            OneBot12Actions.GetStatus,
            null,
            OneBot12StatusData.Parse,
            echo,
            cancellationToken);
    }

    /// <summary>Gets implementation and OneBot protocol version information. / 获取实现端及 OneBot 协议版本信息。</summary>
    public Task<OneBot12Response<OneBot12VersionData>> GetVersionAsync(
        string? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendMetaTypedAsync(
            OneBot12Actions.GetVersion,
            null,
            OneBot12VersionData.Parse,
            echo,
            cancellationToken);
    }
}
