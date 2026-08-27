using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Messages;
using OneBotSdk.Net.V10.Responses;

namespace OneBotSdk.Net.V10.Client;

public sealed partial class OneBot10Client
{
    /// <summary>
    /// Sends a private message using the outgoing-only message model.
    /// 使用仅出站消息模型发送私聊消息。
    /// </summary>
    public Task<OneBot10Response<OneBot10SendMessageResult>> SendPrivateMessageAsync(
        long userId,
        OneBot10SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return SendTypedAsync(
            OneBot10Actions.SendPrivateMessage,
            new JsonObject
            {
                ["user_id"] = userId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot10SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a group message using the outgoing-only message model.
    /// 使用仅出站消息模型发送群消息。
    /// </summary>
    public Task<OneBot10Response<OneBot10SendMessageResult>> SendGroupMessageAsync(
        long groupId,
        OneBot10SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return SendTypedAsync(
            OneBot10Actions.SendGroupMessage,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot10SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a discussion-group message using the outgoing-only message model.
    /// 使用仅出站消息模型发送讨论组消息。
    /// </summary>
    public Task<OneBot10Response<OneBot10SendMessageResult>> SendDiscussMessageAsync(
        long discussId,
        OneBot10SendMessage message,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return SendTypedAsync(
            OneBot10Actions.SendDiscussMessage,
            new JsonObject
            {
                ["discuss_id"] = discussId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot10SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a conditionally targeted message using the outgoing-only message model.
    /// 使用仅出站消息模型发送条件目标消息。
    /// </summary>
    public Task<OneBot10Response<OneBot10SendMessageResult>> SendMessageAsync(
        OneBot10SendMessage message,
        OneBot10MessageType? messageType = null,
        long? userId = null,
        long? groupId = null,
        long? discussId = null,
        bool autoEscape = false,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var parameters = CreateSendMessageParameters(
            message.ToJsonNode(),
            messageType,
            userId,
            groupId,
            discussId,
            autoEscape);
        return SendTypedAsync(
            OneBot10Actions.SendMessage,
            parameters,
            OneBot10SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Deletes a message by its OneBot message identifier.
    /// 按 OneBot 消息标识撤回消息。
    /// </summary>
    public Task<OneBot10Response> DeleteMessageAsync(
        long messageId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.DeleteMessage,
            new JsonObject { ["message_id"] = messageId },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends one or more likes to a friend; OneBot limits each friend to ten likes per day.
    /// 向好友发送一次或多次赞；OneBot 将每位好友每日限制为十次。
    /// </summary>
    public Task<OneBot10Response> SendLikeAsync(
        long userId,
        long times = 1,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot10Actions.SendLike,
            new JsonObject
            {
                ["user_id"] = userId,
                ["times"] = times
            },
            invocationMode,
            echo,
            cancellationToken);
    }

    private static JsonObject CreateSendMessageParameters(
        JsonNode? message,
        OneBot10MessageType? messageType,
        long? userId,
        long? groupId,
        long? discussId,
        bool autoEscape)
    {
        var parameters = new JsonObject
        {
            ["message"] = message,
            ["auto_escape"] = autoEscape
        };

        if (messageType.HasValue)
        {
            parameters["message_type"] = messageType.Value.ToProtocolValue();
        }

        if (userId.HasValue)
        {
            parameters["user_id"] = userId.Value;
        }

        if (groupId.HasValue)
        {
            parameters["group_id"] = groupId.Value;
        }

        if (discussId.HasValue)
        {
            parameters["discuss_id"] = discussId.Value;
        }

        return parameters;
    }
}
