using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Messages;
using OneBotSdk.Net.V11.Responses;

namespace OneBotSdk.Net.V11.Client;

public sealed partial class OneBot11Client
{
    /// <summary>
    /// Sends a private message using the outgoing-only message model.
    /// 使用仅出站消息模型发送私聊消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11SendMessageResult>> SendPrivateMessageAsync(
        long userId,
        OneBot11SendMessage message,
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
            OneBot11Actions.SendPrivateMessage,
            new JsonObject
            {
                ["user_id"] = userId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot11SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a private message using the former shared compatibility model.
    /// 使用原有收发共享兼容模型发送私聊消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11SendMessageData>> SendPrivateMessageAsync(
        long userId,
        OneBot11Message message,
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
            OneBot11Actions.SendPrivateMessage,
            new JsonObject
            {
                ["user_id"] = userId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot11SendMessageData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a group message using the outgoing-only message model.
    /// 使用仅出站消息模型发送群消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11SendMessageResult>> SendGroupMessageAsync(
        long groupId,
        OneBot11SendMessage message,
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
            OneBot11Actions.SendGroupMessage,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot11SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a group message using the former shared compatibility model.
    /// 使用原有收发共享兼容模型发送群消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11SendMessageData>> SendGroupMessageAsync(
        long groupId,
        OneBot11Message message,
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
            OneBot11Actions.SendGroupMessage,
            new JsonObject
            {
                ["group_id"] = groupId,
                ["message"] = message.ToJsonNode(),
                ["auto_escape"] = autoEscape
            },
            OneBot11SendMessageData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a conditionally targeted message using the outgoing-only message model.
    /// 使用仅出站消息模型发送条件目标消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11SendMessageResult>> SendMessageAsync(
        OneBot11SendMessage message,
        OneBot11MessageType? messageType = null,
        long? userId = null,
        long? groupId = null,
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
            autoEscape);
        return SendTypedAsync(
            OneBot11Actions.SendMessage,
            parameters,
            OneBot11SendMessageResult.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends a conditionally targeted message using the former shared compatibility model.
    /// 使用原有收发共享兼容模型发送条件目标消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11SendMessageData>> SendMessageAsync(
        OneBot11Message message,
        OneBot11MessageType? messageType = null,
        long? userId = null,
        long? groupId = null,
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
            autoEscape);

        return SendTypedAsync(
            OneBot11Actions.SendMessage,
            parameters,
            OneBot11SendMessageData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Deletes a message by its OneBot message identifier.
    /// 按 OneBot 消息标识撤回消息。
    /// </summary>
    public Task<OneBot11Response> DeleteMessageAsync(
        long messageId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot11Actions.DeleteMessage,
            new JsonObject { ["message_id"] = messageId },
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Gets a message by its OneBot message identifier.
    /// 按 OneBot 消息标识获取消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11MessageData>> GetMessageAsync(
        long messageId,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendTypedAsync(
            OneBot11Actions.GetMessage,
            new JsonObject { ["message_id"] = messageId },
            OneBot11MessageData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Gets a merged-forward message by its forward identifier.
    /// 按合并转发标识获取合并转发消息。
    /// </summary>
    public Task<OneBot11Response<OneBot11ForwardMessageData>> GetForwardMessageAsync(
        string id,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return SendTypedAsync(
            OneBot11Actions.GetForwardMessage,
            new JsonObject { ["id"] = id },
            OneBot11ForwardMessageData.Parse,
            invocationMode,
            echo,
            cancellationToken);
    }

    /// <summary>
    /// Sends one or more likes to a friend; OneBot limits each friend to ten likes per day.
    /// 向好友发送一次或多次赞；OneBot 将每位好友每日限制为十次。
    /// </summary>
    public Task<OneBot11Response> SendLikeAsync(
        long userId,
        long times = 1,
        InvocationMode invocationMode = InvocationMode.Normal,
        JsonNode? echo = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot11Actions.SendLike,
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
        OneBot11MessageType? messageType,
        long? userId,
        long? groupId,
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

        return parameters;
    }
}
