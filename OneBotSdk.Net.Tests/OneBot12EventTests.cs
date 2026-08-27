using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Messages;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class OneBot12EventTests
{
    public static IEnumerable<object[]> StandardEventCases()
    {
        yield return Case("message", "private", typeof(PrivateMessageEvent));
        yield return Case("message", "group", typeof(GroupMessageEvent));
        yield return Case("message", "channel", typeof(ChannelMessageEvent));
        yield return Case("notice", "friend_increase", typeof(FriendIncreaseNoticeEvent));
        yield return Case("notice", "friend_decrease", typeof(FriendDecreaseNoticeEvent));
        yield return Case("notice", "private_message_delete", typeof(PrivateMessageDeleteNoticeEvent));
        yield return Case("notice", "group_member_increase", typeof(GroupMemberIncreaseNoticeEvent));
        yield return Case("notice", "group_member_decrease", typeof(GroupMemberDecreaseNoticeEvent));
        yield return Case("notice", "group_message_delete", typeof(GroupMessageDeleteNoticeEvent));
        yield return Case("notice", "guild_member_increase", typeof(GuildMemberIncreaseNoticeEvent));
        yield return Case("notice", "guild_member_decrease", typeof(GuildMemberDecreaseNoticeEvent));
        yield return Case("notice", "channel_member_increase", typeof(ChannelMemberIncreaseNoticeEvent));
        yield return Case("notice", "channel_member_decrease", typeof(ChannelMemberDecreaseNoticeEvent));
        yield return Case("notice", "channel_message_delete", typeof(ChannelMessageDeleteNoticeEvent));
        yield return Case("notice", "channel_create", typeof(ChannelCreateNoticeEvent));
        yield return Case("notice", "channel_delete", typeof(ChannelDeleteNoticeEvent));
        yield return Case("meta", "connect", typeof(ConnectMetaEvent));
        yield return Case("meta", "heartbeat", typeof(HeartbeatMetaEvent));
        yield return Case("meta", "status_update", typeof(StatusUpdateMetaEvent));
    }

    [Theory]
    [MemberData(nameof(StandardEventCases))]
    public void Parser_MapsEveryStandardDetailType(
        string type,
        string detailType,
        Type expectedType)
    {
        var source = CreateEvent(type, detailType);

        var parsed = OneBot12EventParser.Parse(source);

        Assert.IsType(expectedType, parsed);
        Assert.Equal("event-1", parsed.Id);
        Assert.Equal(1700000000.125, parsed.Time);
        Assert.Equal(type, parsed.Type);
        Assert.Equal(detailType, parsed.DetailType);
        Assert.Equal("vendor-value", parsed.RawJson["vendor.extension"]!.GetValue<string>());
        if (type == "meta")
        {
            Assert.Null(parsed.Self);
        }
        else
        {
            Assert.Equal("qq", parsed.Self!.Platform);
            Assert.Equal("bot-1", parsed.Self.UserId);
        }
    }

    [Fact]
    public void Parser_PreservesRawEventAndToleratesIndependentMalformedFields()
    {
        var source = CreateEvent("message", "group");
        source["id"] = 123;
        source["time"] = "bad";
        source["self"] = new JsonObject
        {
            ["platform"] = 42,
            ["user_id"] = "bot-1",
            ["extension"] = true
        };
        source["group_id"] = "group-1";
        source["message"] = new JsonArray
        {
            new JsonObject
            {
                ["type"] = "text",
                ["data"] = new JsonObject { ["text"] = "hello" }
            },
            JsonValue.Create(42)
        };

        var parsed = Assert.IsType<GroupMessageEvent>(OneBot12EventParser.Parse(source));
        source["group_id"] = "mutated";

        Assert.Equal("123", parsed.Id);
        Assert.Null(parsed.Time);
        Assert.Equal("42", parsed.Self!.Platform);
        Assert.Equal("bot-1", parsed.Self.UserId);
        Assert.Equal("group-1", parsed.GroupId);
        Assert.Equal("hello", parsed.Message!.PlainText);
        Assert.Single(parsed.Message);
        Assert.Equal("group-1", parsed.RawJson["group_id"]!.GetValue<string>());
    }

    [Fact]
    public void Parser_UsesUnknownFallbacksWithoutDiscardingExtensions()
    {
        var unknownRequest = OneBot12EventParser.Parse(CreateEvent("request", "vendor.request"));
        var unknownNotice = OneBot12EventParser.Parse(CreateEvent("notice", "vendor.notice"));
        var unknownTopLevel = OneBot12EventParser.Parse(CreateEvent("vendor", "anything"));

        Assert.IsType<UnknownRequestEvent>(unknownRequest);
        Assert.IsType<UnknownNoticeEvent>(unknownNotice);
        Assert.IsType<UnknownOneBot12Event>(unknownTopLevel);
        Assert.Equal("vendor-value", unknownRequest.RawJson["vendor.extension"]!.GetValue<string>());
        Assert.Equal("vendor.notice", unknownNotice.DetailType);
    }

    [Fact]
    public void Dispatcher_DeliversSameTypedEventToEventHandlerAndObservable()
    {
        var dispatcher = new OneBot12EventDispatcher();
        GroupMessageEvent? handlerValue = null;
        GroupMessageEvent? observableValue = null;
        var allEventCount = 0;

        dispatcher.GroupMessageReceived += (_, args) => handlerValue = args.Event;
        using (dispatcher.Events.Subscribe(_ => allEventCount++))
        using (dispatcher.Events.OfType<GroupMessageEvent>().Subscribe(value => observableValue = value))
        {
            var parsed = Assert.IsType<GroupMessageEvent>(
                OneBot12EventParser.Parse(CreateEvent("message", "group")));
            dispatcher.Dispatch(parsed);

            Assert.Same(parsed, handlerValue);
            Assert.Same(parsed, observableValue);
            Assert.Equal(1, allEventCount);
        }
    }

    private static object[] Case(string type, string detailType, Type expectedType)
    {
        return new object[] { type, detailType, expectedType };
    }

    private static JsonObject CreateEvent(string type, string detailType)
    {
        return new JsonObject
        {
            ["id"] = "event-1",
            ["self"] = type == "meta"
                ? null
                : new JsonObject
                {
                    ["platform"] = "qq",
                    ["user_id"] = "bot-1"
                },
            ["time"] = 1700000000.125,
            ["type"] = type,
            ["detail_type"] = detailType,
            ["sub_type"] = "test",
            ["message_id"] = "message-1",
            ["message"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["data"] = new JsonObject { ["text"] = "hello" }
                }
            },
            ["alt_message"] = "hello",
            ["user_id"] = "user-1",
            ["operator_id"] = "operator-1",
            ["group_id"] = "group-1",
            ["guild_id"] = "guild-1",
            ["channel_id"] = "channel-1",
            ["interval"] = 5000,
            ["version"] = new JsonObject
            {
                ["impl"] = "test",
                ["version"] = "1.0.0",
                ["onebot_version"] = "12"
            },
            ["status"] = new JsonObject
            {
                ["good"] = true,
                ["bots"] = new JsonArray()
            },
            ["vendor.extension"] = "vendor-value"
        };
    }
}
