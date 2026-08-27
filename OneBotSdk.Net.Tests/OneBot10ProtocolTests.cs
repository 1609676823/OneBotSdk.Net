using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Messages;
using OneBotSdk.Net.V10.Transports;
using OneBotSdk.Net.V10.Transports.WebSockets;
using Xunit;

namespace OneBotSdk.Net.Tests;

[Collection(JsonConfigurationCollection.Name)]
public sealed class OneBot10ProtocolTests
{
    private static readonly string[] OfficialActions =
    {
        "send_private_msg", "send_group_msg", "send_discuss_msg", "send_msg", "delete_msg", "send_like",
        "set_group_kick", "set_group_ban", "set_group_anonymous_ban", "set_group_whole_ban",
        "set_group_admin", "set_group_anonymous", "set_group_card", "set_group_leave",
        "set_group_special_title", "set_discuss_leave", "set_friend_add_request", "set_group_add_request",
        "get_login_info", "get_stranger_info", "get_friend_list", "get_group_list", "get_group_info",
        "get_group_member_info", "get_group_member_list", "get_cookies", "get_csrf_token",
        "get_credentials", "get_record", "get_image", "can_send_image", "can_send_record", "get_status",
        "get_version_info", "set_restart_plugin", "clean_data_dir", "clean_plugin_log"
    };

    public static IEnumerable<object[]> StandardEventCases()
    {
        yield return Case("message", "message_type", "private", typeof(PrivateMessageEvent));
        yield return Case("message", "message_type", "group", typeof(GroupMessageEvent));
        yield return Case("message", "message_type", "discuss", typeof(DiscussMessageEvent));
        yield return Case("notice", "notice_type", "group_upload", typeof(GroupUploadNoticeEvent));
        yield return Case("notice", "notice_type", "group_admin", typeof(GroupAdminNoticeEvent));
        yield return Case("notice", "notice_type", "group_decrease", typeof(GroupDecreaseNoticeEvent));
        yield return Case("notice", "notice_type", "group_increase", typeof(GroupIncreaseNoticeEvent));
        yield return Case("notice", "notice_type", "group_ban", typeof(GroupBanNoticeEvent));
        yield return Case("notice", "notice_type", "friend_add", typeof(FriendAddNoticeEvent));
        yield return Case("request", "request_type", "friend", typeof(FriendRequestEvent));
        yield return Case("request", "request_type", "group", typeof(GroupRequestEvent));
        yield return Case("meta_event", "meta_event_type", "lifecycle", typeof(LifecycleMetaEvent));
        yield return Case("meta_event", "meta_event_type", "heartbeat", typeof(HeartbeatMetaEvent));
    }

    [Fact]
    public void ActionCatalog_MatchesExactlyThirtySevenOfficialActions()
    {
        Assert.Equal(37, OneBot10Actions.All.Count);
        Assert.Equal(OfficialActions, OneBot10Actions.All);
        Assert.Equal(37, OneBot10Actions.All.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(".handle_quick_operation", OneBot10HiddenActions.HandleQuickOperation);
    }

    [Fact]
    public async Task TypedDiscussAction_UsesV10FieldsAndPreservesCompleteTrace()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = request => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject
                {
                    ["message_id"] = "321",
                    ["vendor_field"] = true
                },
                ["echo"] = Clone(request.Echo)
            }
        };
        var client = new OneBot10Client(transport);
        var echo = new JsonObject { ["request"] = 1 };

        var response = await client.SendDiscussMessageAsync(
            30001,
            new OneBot10SendMessage { new TextSendSegment("hello") },
            echo: echo);

        var request = Assert.Single(transport.Requests);
        Assert.Equal("send_discuss_msg", request.Action);
        Assert.Equal(30001L, request.Parameters!["discuss_id"]!.GetValue<long>());
        Assert.Equal(321L, response.Data!.MessageId);
        Assert.True(response.RawData!["vendor_field"]!.GetValue<bool>());
        Assert.Equal("send_discuss_msg", response.Action);
        Assert.Equal(30001L, response.RequestParameters!["discuss_id"]!.GetValue<long>());
        Assert.Equal(1, response.RequestEcho!["request"]!.GetValue<int>());
        Assert.NotNull(response.RawRequestJson);
        Assert.NotNull(response.RawResponseJson);
    }

    [Fact]
    public async Task GetStatus_ParsesAllSixOfficialV10FieldsTolerantly()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = request => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject
                {
                    ["app_initialized"] = "1",
                    ["app_enabled"] = true,
                    ["plugins_good"] = 0,
                    ["app_good"] = "false",
                    ["online"] = 1,
                    ["good"] = "true",
                    ["vendor_field"] = "retained"
                },
                ["echo"] = Clone(request.Echo)
            }
        };
        var client = new OneBot10Client(transport);

        var response = await client.GetStatusAsync();

        Assert.True(response.Data!.AppInitialized);
        Assert.True(response.Data.AppEnabled);
        Assert.False(response.Data.PluginsGood);
        Assert.False(response.Data.AppGood);
        Assert.True(response.Data.Online);
        Assert.True(response.Data.Good);
        Assert.Equal("retained", response.RawData!["vendor_field"]!.GetValue<string>());
    }

    [Fact]
    public async Task StronglyTypedMethods_CoverEveryOfficialActionWithoutNetworkIo()
    {
        var transport = new RecordingTransport();
        var client = new OneBot10Client(transport);
        var message = OneBot10SendMessage.FromString("test");

        await client.SendPrivateMessageAsync(1, message);
        await client.SendGroupMessageAsync(2, message);
        await client.SendDiscussMessageAsync(3, message);
        await client.SendMessageAsync(message, OneBot10MessageType.Private, userId: 1);
        await client.DeleteMessageAsync(4);
        await client.SendLikeAsync(1);

        // These calls use an in-memory recording transport; no destructive operation reaches a bot implementation.
        // 这些调用使用内存记录传输；不会有任何破坏性操作到达机器人实现端。
        await client.SetGroupKickAsync(2, 1);
        await client.SetGroupBanAsync(2, 1);
        await client.SetGroupAnonymousBanAsync(2, "anonymous-flag");
        await client.SetGroupWholeBanAsync(2);
        await client.SetGroupAdminAsync(2, 1);
        await client.SetGroupAnonymousAsync(2);
        await client.SetGroupCardAsync(2, 1);
        await client.SetGroupLeaveAsync(2);
        await client.SetGroupSpecialTitleAsync(2, 1);
        await client.SetDiscussLeaveAsync(3);
        await client.SetFriendAddRequestAsync("friend-flag");
        await client.SetGroupAddRequestAsync("group-flag", OneBot10GroupRequestType.Add);

        await client.GetLoginInfoAsync();
        await client.GetStrangerInfoAsync(1);
        await client.GetFriendListAsync();
        await client.GetGroupListAsync();
        await client.GetGroupInfoAsync(2);
        await client.GetGroupMemberInfoAsync(2, 1);
        await client.GetGroupMemberListAsync(2);
        await client.GetCookiesAsync();
        await client.GetCsrfTokenAsync();
        await client.GetCredentialsAsync();
        await client.GetRecordAsync("record.file", OneBot10RecordFormat.Mp3);
        await client.GetImageAsync("image.file");
        await client.CanSendImageAsync();
        await client.CanSendRecordAsync();
        await client.GetStatusAsync();
        await client.GetVersionInfoAsync();
        await client.SetRestartPluginAsync();
        await client.CleanDataDirectoryAsync(OneBot10DataDirectory.Image);
        await client.CleanPluginLogAsync();

        Assert.Equal(37, transport.Requests.Count);
        Assert.Equal(
            OneBot10Actions.All.OrderBy(value => value, StringComparer.Ordinal),
            transport.Requests.Select(request => request.Action).OrderBy(value => value, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(InvocationMode.Normal, "send_private_msg")]
    [InlineData(InvocationMode.Async, "send_private_msg_async")]
    [InlineData(InvocationMode.RateLimited, "send_private_msg_rate_limited")]
    public async Task InvocationMode_UsesOfficialMutuallyExclusiveSuffix(
        InvocationMode mode,
        string expectedAction)
    {
        var transport = new RecordingTransport();
        var client = new OneBot10Client(transport);

        await client.SendPrivateMessageAsync(
            10001,
            OneBot10SendMessage.FromString("hello"),
            invocationMode: mode);

        Assert.Equal(expectedAction, Assert.Single(transport.Requests).Action);
    }

    [Fact]
    public void MessageSegmentCatalog_MatchesExactlyFourteenWireTypes()
    {
        var expected = new[]
        {
            "text", "face", "image", "record", "at", "rps", "dice", "shake",
            "anonymous", "share", "contact", "location", "music", "rich"
        };

        Assert.Equal(expected, MessageSegmentTypes.Standard);
        Assert.Equal(expected.Length, MessageSegmentTypes.Standard.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void PublicMessages_ExposeOnlyDirectionSpecificModels()
    {
        var assembly = typeof(OneBot10SendMessage).Assembly;

        Assert.Null(assembly.GetType("OneBotSdk.Net.V10.Messages.OneBot10Message"));
        Assert.Null(assembly.GetType("OneBotSdk.Net.V10.Messages.MessageSegment"));
        Assert.Null(assembly.GetType("OneBotSdk.Net.V10.Messages.OneBot10MessageChain"));
        Assert.Null(assembly.GetType("OneBotSdk.Net.V10.Responses.OneBot10SendMessageData"));

        var messageMethods = typeof(OneBot10Client)
            .GetMethods()
            .Where(method => method.Name.StartsWith("Send", StringComparison.Ordinal));
        Assert.DoesNotContain(
            messageMethods.SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.FullName == "OneBotSdk.Net.V10.Messages.OneBot10Message");
    }

    [Fact]
    public void CqCodeCodec_EncodesOutgoingAndDecodesIntoReceivedOnlyObjects()
    {
        var outgoing = new OneBot10SendMessage()
            .Text("A&[B]")
            .AtAll();

        var encoded = CqCodeCodec.Encode(outgoing);
        var received = CqCodeCodec.Decode(encoded);

        Assert.Equal("A&amp;&#91;B&#93;[CQ:at,qq=all]", encoded);
        Assert.IsType<TextReceivedSegment>(received[0]);
        Assert.IsType<AtReceivedSegment>(received[1]);
        Assert.Equal("A&[B]", received.PlainText);
        Assert.Equal(encoded, received.ToString());
    }

    [Fact]
    public void ReverseWebSocketMetadata_UsesLegacyTokenSchemeInsteadOfBearer()
    {
        var tokenMetadata = OneBot10ReverseWebSocketMetadata.FromHeaders(new[]
        {
            new KeyValuePair<string, string>("X-Self-ID", "10000"),
            new KeyValuePair<string, string>("X-Client-Role", "Universal"),
            new KeyValuePair<string, string>("Authorization", "Token secret")
        });
        var bearerMetadata = OneBot10ReverseWebSocketMetadata.FromHeaders(new[]
        {
            new KeyValuePair<string, string>("Authorization", "Bearer secret")
        });

        Assert.Equal(10000L, tokenMetadata.SelfIdNumber);
        Assert.Equal(OneBot10ReverseWebSocketRole.Universal, tokenMetadata.Role);
        Assert.True(tokenMetadata.HasToken("secret"));
        Assert.False(bearerMetadata.HasToken("secret"));
    }

    [Fact]
    public void ReceivedMessage_ParsesStandardReceiveSegmentsAndUnknownFallback()
    {
        var source = JsonNode.Parse(
            "[" +
            "{\"type\":\"text\",\"data\":{\"text\":\"hello\"}}," +
            "{\"type\":\"face\",\"data\":{\"id\":\"14\"}}," +
            "{\"type\":\"image\",\"data\":{\"file\":\"a.jpg\",\"url\":\"https://example.test/a.jpg\"}}," +
            "{\"type\":\"record\",\"data\":{\"file\":\"a.silk\",\"magic\":\"1\"}}," +
            "{\"type\":\"at\",\"data\":{\"qq\":\"all\"}}," +
            "{\"type\":\"rps\",\"data\":{}}," +
            "{\"type\":\"dice\",\"data\":{}}," +
            "{\"type\":\"shake\",\"data\":{}}," +
            "{\"type\":\"share\",\"data\":{\"url\":\"https://example.test\",\"title\":\"title\"}}," +
            "{\"type\":\"contact\",\"data\":{\"type\":\"qq\",\"id\":\"10001\"}}," +
            "{\"type\":\"location\",\"data\":{\"lat\":\"31.2\",\"lon\":\"121.5\",\"title\":\"Shanghai\",\"content\":\"Bund\"}}," +
            "{\"type\":\"rich\",\"data\":{\"vendor\":\"value\"}}," +
            "{\"type\":\"vendor_extension\",\"data\":{\"answer\":42},\"root_extension\":true}" +
            "]");

        var message = OneBot10ReceivedMessage.Parse(source);

        Assert.NotNull(message);
        Assert.Equal(13, message!.Count);
        Assert.IsType<TextReceivedSegment>(message[0]);
        Assert.IsType<FaceReceivedSegment>(message[1]);
        Assert.IsType<ImageReceivedSegment>(message[2]);
        Assert.IsType<RecordReceivedSegment>(message[3]);
        Assert.IsType<AtReceivedSegment>(message[4]);
        Assert.IsType<RpsReceivedSegment>(message[5]);
        Assert.IsType<DiceReceivedSegment>(message[6]);
        Assert.IsType<ShakeReceivedSegment>(message[7]);
        Assert.IsType<ShareReceivedSegment>(message[8]);
        Assert.IsType<ContactReceivedSegment>(message[9]);
        Assert.IsType<LocationReceivedSegment>(message[10]);
        Assert.IsType<RichReceivedSegment>(message[11]);
        var unknown = Assert.IsType<UnknownReceivedSegment>(message[12]);
        Assert.Equal(42, unknown.Data!["answer"]!.GetValue<int>());
        Assert.True(unknown.RawJson["root_extension"]!.GetValue<bool>());
        Assert.Equal("hello", message.PlainText);
    }

    [Fact]
    public void SendMessage_WritesStringSingleArrayAndCqCodeShapes()
    {
        var stringMessage = OneBot10SendMessage.FromString("[CQ:at,qq=all]");
        var singleMessage = OneBot10SendMessage.FromSegment(new AtSendSegment("all"));
        var arrayMessage = new OneBot10SendMessage()
            .Text("a&[b]")
            .At(10001)
            .Image("a.jpg", cache: false, timeoutSeconds: 10);

        Assert.Equal("[CQ:at,qq=all]", stringMessage.ToJsonNode()!.GetValue<string>());
        var single = Assert.IsType<JsonObject>(singleMessage.ToJsonNode());
        Assert.Equal("at", single["type"]!.GetValue<string>());
        Assert.Equal("all", single["data"]!["qq"]!.GetValue<string>());

        var array = Assert.IsType<JsonArray>(arrayMessage.ToJsonNode());
        Assert.Equal(3, array.Count);
        Assert.Equal("0", array[2]!["data"]!["cache"]!.GetValue<string>());
        Assert.Equal("10", array[2]!["data"]!["timeout"]!.GetValue<string>());

        var cqCode = arrayMessage.ToCqCode();
        Assert.Contains("a&amp;&#91;b&#93;", cqCode);
        Assert.Contains("[CQ:at,qq=10001]", cqCode);
    }

    [Theory]
    [MemberData(nameof(StandardEventCases))]
    public void EventParser_MapsEveryStandardEvent(
        string postType,
        string discriminatorName,
        string discriminatorValue,
        Type expectedType)
    {
        var source = CreateEvent(postType);
        source[discriminatorName] = discriminatorValue;

        var parsed = OneBot10EventParser.Parse(source);

        Assert.IsType(expectedType, parsed);
        Assert.Equal(1700000000L, parsed.Time);
        Assert.Equal(10000L, parsed.SelfId);
        Assert.Equal("vendor", parsed.RawJson["extension"]!.GetValue<string>());
    }

    [Fact]
    public void Heartbeat_ParsesAllSixOfficialV10StatusFields()
    {
        var source = CreateEvent("meta_event");
        source["meta_event_type"] = "heartbeat";

        var heartbeat = Assert.IsType<HeartbeatMetaEvent>(OneBot10EventParser.Parse(source));

        Assert.True(heartbeat.Status!.AppInitialized);
        Assert.True(heartbeat.Status.AppEnabled);
        Assert.False(heartbeat.Status.PluginsGood);
        Assert.False(heartbeat.Status.AppGood);
        Assert.True(heartbeat.Status.Online);
        Assert.True(heartbeat.Status.Good);
        Assert.Equal("retained", heartbeat.Status.RawJson["vendor_field"]!.GetValue<string>());
    }

    [Fact]
    public void Dispatcher_DeliversDiscussMessageToBothSubscriptionPatterns()
    {
        var source = CreateEvent("message");
        source["message_type"] = "discuss";
        var parsed = Assert.IsType<DiscussMessageEvent>(OneBot10EventParser.Parse(source));
        var dispatcher = new OneBot10EventDispatcher();
        DiscussMessageEvent? handlerValue = null;
        DiscussMessageEvent? observableValue = null;

        dispatcher.DiscussMessageReceived += (_, args) => handlerValue = args.Event;
        using (dispatcher.Events.OfType<DiscussMessageEvent>().Subscribe(value => observableValue = value))
        {
            dispatcher.Dispatch(parsed);
            Assert.Same(parsed, handlerValue);
            Assert.Same(parsed, observableValue);
        }
    }

    [Fact]
    public void JsonConfiguration_UsesSafeDefaultAndExplicitUnsafeOptIn()
    {
        var previous = OneBot10Json.UseUnsafeRelaxedJsonEscaping;
        try
        {
            var value = new JsonObject { ["text"] = "<中文&>" };
            OneBot10Json.UseUnsafeRelaxedJsonEscaping = false;
            var safeOptions = OneBot10Json.CreateSerializerOptions();
            var safeJson = OneBot10Json.Serialize(value);
            OneBot10Json.UseUnsafeRelaxedJsonEscaping = true;
            var unsafeOptions = OneBot10Json.CreateSerializerOptions();
            var unsafeJson = OneBot10Json.Serialize(value);

            Assert.Same(JavaScriptEncoder.Default, safeOptions.Encoder);
            Assert.Same(JavaScriptEncoder.UnsafeRelaxedJsonEscaping, unsafeOptions.Encoder);
            Assert.DoesNotContain("<中文&>", safeJson);
            Assert.Contains("<中文&>", unsafeJson);
        }
        finally
        {
            OneBot10Json.UseUnsafeRelaxedJsonEscaping = previous;
        }
    }

    private static object[] Case(
        string postType,
        string discriminatorName,
        string discriminatorValue,
        Type expectedType)
    {
        return new object[] { postType, discriminatorName, discriminatorValue, expectedType };
    }

    private static JsonObject CreateEvent(string postType)
    {
        return new JsonObject
        {
            ["time"] = 1700000000,
            ["self_id"] = 10000,
            ["post_type"] = postType,
            ["sub_type"] = "normal",
            ["message_id"] = 1,
            ["user_id"] = 10001,
            ["group_id"] = 20001,
            ["discuss_id"] = 30001,
            ["operator_id"] = 10002,
            ["duration"] = 60,
            ["message"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["data"] = new JsonObject { ["text"] = "hello" }
                }
            },
            ["raw_message"] = "hello",
            ["font"] = 0,
            ["sender"] = new JsonObject { ["user_id"] = 10001, ["nickname"] = "tester" },
            ["anonymous"] = null,
            ["file"] = new JsonObject
            {
                ["id"] = "file-id",
                ["name"] = "file.txt",
                ["size"] = 10,
                ["busid"] = 1
            },
            ["comment"] = "request",
            ["flag"] = "flag",
            ["status"] = new JsonObject
            {
                ["app_initialized"] = "1",
                ["app_enabled"] = true,
                ["plugins_good"] = 0,
                ["app_good"] = "false",
                ["online"] = 1,
                ["good"] = "true",
                ["vendor_field"] = "retained"
            },
            ["interval"] = 5000,
            ["extension"] = "vendor"
        };
    }

    private sealed class RecordingTransport : IOneBot10ActionTransport
    {
        internal List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

        internal Func<RecordedRequest, JsonObject>? ResponseFactory { get; set; }

        public Task<OneBot10ActionTransportResult> SendAsync(
            string action,
            JsonObject? parameters,
            JsonNode? echo,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new RecordedRequest(action, Clone(parameters) as JsonObject, Clone(echo));
            Requests.Add(request);
            var response = ResponseFactory == null
                ? new JsonObject
                {
                    ["status"] = "async",
                    ["retcode"] = 1,
                    ["data"] = null,
                    ["echo"] = Clone(echo)
                }
                : ResponseFactory(request);
            var requestParameters = request.Parameters ?? new JsonObject();
            return Task.FromResult(new OneBot10ActionTransportResult(
                action,
                requestParameters,
                request.Echo,
                OneBot10Json.Serialize(requestParameters),
                response,
                OneBot10Json.Serialize(response)));
        }
    }

    private sealed class RecordedRequest
    {
        internal RecordedRequest(string action, JsonObject? parameters, JsonNode? echo)
        {
            Action = action;
            Parameters = parameters;
            Echo = echo;
        }

        internal string Action { get; }
        internal JsonObject? Parameters { get; }
        internal JsonNode? Echo { get; }
    }

    private static JsonNode? Clone(JsonNode? node)
    {
        return node == null ? null : OneBot10Json.Parse(OneBot10Json.Serialize(node));
    }
}
