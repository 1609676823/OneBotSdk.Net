using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12.Messages;
using OneBotSdk.Net.V12.Responses;

namespace OneBotSdk.Net.V12.Client;

public sealed partial class OneBot12Client
{
    /// <summary>Sends a standard or implementation-defined destination type. / 向标准或实现扩展的目标类型发送消息。</summary>
    public Task<OneBot12Response<OneBot12SendMessageData>> SendMessageAsync(
        string detailType,
        OneBot12SendMessage message,
        string? userId = null,
        string? groupId = null,
        string? guildId = null,
        string? channelId = null,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var normalizedDetailType = Require(detailType, nameof(detailType));
        ValidateStandardDestination(normalizedDetailType, userId, groupId, guildId, channelId);

        var parameters = new JsonObject
        {
            ["detail_type"] = normalizedDetailType,
            ["message"] = message.ToJsonNode()
        };
        AddOptional(parameters, "user_id", userId);
        AddOptional(parameters, "group_id", groupId);
        AddOptional(parameters, "guild_id", guildId);
        AddOptional(parameters, "channel_id", channelId);

        return SendTypedAsync(
            OneBot12Actions.SendMessage,
            parameters,
            OneBot12SendMessageData.Parse,
            echo,
            self,
            cancellationToken);
    }

    /// <summary>Sends a private message to one user. / 向一个用户发送私聊消息。</summary>
    public Task<OneBot12Response<OneBot12SendMessageData>> SendPrivateMessageAsync(
        string userId,
        OneBot12SendMessage message,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync("private", message, userId: Require(userId, nameof(userId)), echo: echo, self: self, cancellationToken: cancellationToken);
    }

    /// <summary>Sends a message to a one-level group. / 向单级群发送消息。</summary>
    public Task<OneBot12Response<OneBot12SendMessageData>> SendGroupMessageAsync(
        string groupId,
        OneBot12SendMessage message,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync("group", message, groupId: Require(groupId, nameof(groupId)), echo: echo, self: self, cancellationToken: cancellationToken);
    }

    /// <summary>Sends a message to one channel in a guild. / 向群组中的一个频道发送消息。</summary>
    public Task<OneBot12Response<OneBot12SendMessageData>> SendChannelMessageAsync(
        string guildId,
        string channelId,
        OneBot12SendMessage message,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync(
            "channel",
            message,
            guildId: Require(guildId, nameof(guildId)),
            channelId: Require(channelId, nameof(channelId)),
            echo: echo,
            self: self,
            cancellationToken: cancellationToken);
    }

    /// <summary>Deletes or recalls one message. / 删除或撤回一条消息。</summary>
    public Task<OneBot12Response> DeleteMessageAsync(
        string messageId,
        string? echo = null,
        OneBot12Self? self = null,
        CancellationToken cancellationToken = default)
    {
        return SendWithoutDataAsync(
            OneBot12Actions.DeleteMessage,
            new JsonObject { ["message_id"] = Require(messageId, nameof(messageId)) },
            echo,
            self,
            cancellationToken);
    }

    private static void AddOptional(JsonObject parameters, string name, string? value)
    {
        if (value != null)
        {
            parameters[name] = value;
        }
    }

    private static void ValidateStandardDestination(
        string detailType,
        string? userId,
        string? groupId,
        string? guildId,
        string? channelId)
    {
        // Standard destinations have required identifiers; extension detail types remain intentionally open.
        // 标准目标具有必填标识；实现扩展 detail_type 则有意保持开放。
        switch (detailType)
        {
            case "private":
                Require(userId, nameof(userId));
                break;
            case "group":
                Require(groupId, nameof(groupId));
                break;
            case "channel":
                Require(guildId, nameof(guildId));
                Require(channelId, nameof(channelId));
                break;
        }
    }
}
