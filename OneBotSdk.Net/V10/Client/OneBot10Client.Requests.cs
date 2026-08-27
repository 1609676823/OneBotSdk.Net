using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

public sealed partial class OneBot10Client
{
    /// <summary>
    /// Approves or rejects a friend request identified by its event flag.
    /// 根据事件 flag 同意或拒绝好友请求。
    /// </summary>
    public Task<OneBot10Response> SetFriendAddRequestAsync(
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
            OneBot10Actions.SetFriendAddRequest,
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
    public Task<OneBot10Response> SetGroupAddRequestAsync(
        string flag,
        OneBot10GroupRequestType requestType,
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
            OneBot10Actions.SetGroupAddRequest,
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
