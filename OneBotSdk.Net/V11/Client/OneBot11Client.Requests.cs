using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

public sealed partial class OneBot11Client
{
    /// <summary>
    /// Approves or rejects a friend request identified by its event flag.
    /// 根据事件 flag 同意或拒绝好友请求。
    /// </summary>
    public Task<OneBot11Response> SetFriendAddRequestAsync(
        string flag,
        bool approve = true,
        string remark = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (flag == null)
        {
            throw new ArgumentNullException(nameof(flag));
        }

        if (remark == null)
        {
            throw new ArgumentNullException(nameof(remark));
        }

        return SendWithoutDataAsync(
            OneBot11Actions.SetFriendAddRequest,
            new JsonObject
            {
                ["flag"] = flag,
                ["approve"] = approve,
                ["remark"] = remark
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Approves or rejects a group join request or invitation identified by its event flag.
    /// 根据事件 flag 同意或拒绝加群请求或邀请。
    /// </summary>
    public Task<OneBot11Response> SetGroupAddRequestAsync(
        string flag,
        OneBot11GroupRequestType requestType,
        bool approve = true,
        string reason = "",
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (flag == null)
        {
            throw new ArgumentNullException(nameof(flag));
        }

        if (reason == null)
        {
            throw new ArgumentNullException(nameof(reason));
        }

        return SendWithoutDataAsync(
            OneBot11Actions.SetGroupAddRequest,
            new JsonObject
            {
                ["flag"] = flag,
                ["sub_type"] = requestType.ToProtocolValue(),
                ["approve"] = approve,
                ["reason"] = reason
            },
            invocationMode,
            echo,
            cancellationToken);
    }
}
