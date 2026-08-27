using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;
using OneBotSdk.Net.V11.Transports;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class ClientTests
{
    [Fact]
    public void StandardActionCatalog_ContainsExactlyTheOfficial38UniqueBaseActions()
    {
        Assert.Equal(38, OneBot11Actions.All.Count);
        Assert.Equal(38, OneBot11Actions.All.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(OneBot11HiddenActions.HandleQuickOperation, OneBot11Actions.All);
        Assert.DoesNotContain(OneBot11Actions.All, action => action.EndsWith("_async", StringComparison.Ordinal));
        Assert.DoesNotContain(OneBot11Actions.All, action => action.EndsWith("_rate_limited", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TypedMessageCall_UsesCanonicalFieldsAndParsesDriftedResponseField()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = _ => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject
                {
                    ["message_id"] = "321",
                    ["implementation_field"] = true
                }
            }
        };
        var client = new OneBot11Client(transport);

        var response = await client.SendGroupMessageAsync(
            10001,
            new OneBot11SendMessage
            {
                new TextSendSegment("hello"),
                new AtSendSegment("all")
            });

        var request = Assert.Single(transport.Requests);
        Assert.Equal("send_group_msg", request.Action);
        Assert.Equal(10001L, request.Parameters!["group_id"]!.GetValue<long>());
        Assert.False(request.Parameters["auto_escape"]!.GetValue<bool>());
        Assert.Equal(2, request.Parameters["message"]!.AsArray().Count);
        Assert.Equal(321L, response.Data!.MessageId);
        Assert.True(response.RawData!["implementation_field"]!.GetValue<bool>());
        Assert.Equal("send_group_msg", response.Action);
        Assert.Equal(10001L, response.RequestParameters!["group_id"]!.GetValue<long>());
        Assert.Null(response.RequestEcho);
        Assert.Equal(OneBot11Json.Serialize(request.Parameters), response.RawRequestJson);
        Assert.Equal(OneBot11Json.Serialize(response.RawJson), response.RawResponseJson);
    }

    [Fact]
    public async Task GetMessageAndForwardMessage_ParseIndependentReceivedModels()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = request => request.Action == OneBot11Actions.GetMessage
                ? new JsonObject
                {
                    ["status"] = "ok",
                    ["retcode"] = 0,
                    ["data"] = new JsonObject
                    {
                        ["time"] = "1710000000",
                        ["message_type"] = "group",
                        ["message_id"] = 12,
                        ["real_id"] = "13",
                        ["sender"] = new JsonObject
                        {
                            ["user_id"] = "10001",
                            ["nickname"] = "sender",
                            ["card"] = "group card",
                            ["role"] = "member"
                        },
                        ["message"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["type"] = "image",
                                ["data"] = new JsonObject
                                {
                                    ["file"] = "received.jpg",
                                    ["url"] = "https://example.test/received.jpg"
                                }
                            }
                        }
                    }
                }
                : new JsonObject
                {
                    ["status"] = "ok",
                    ["retcode"] = 0,
                    ["data"] = new JsonObject
                    {
                        ["message"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["type"] = "node",
                                ["data"] = new JsonObject
                                {
                                    ["user_id"] = "42",
                                    ["nickname"] = "node",
                                    ["content"] = "nested text"
                                }
                            }
                        }
                    }
                }
        };
        var client = new OneBot11Client(transport);

        var messageResponse = await client.GetMessageAsync(12);
        var forwardResponse = await client.GetForwardMessageAsync("forward-id");

        var image = Assert.IsType<ImageReceivedSegment>(messageResponse.Data!.MessageChain[0]);
        Assert.Equal("https://example.test/received.jpg", image.Url);
        Assert.Equal(1710000000L, messageResponse.Data.Time);
        Assert.Equal(13L, messageResponse.Data.RealId);
        Assert.Equal(10001L, messageResponse.Data.Sender!.UserId);
        Assert.Equal("group card", messageResponse.Data.Sender.Card);
        Assert.Equal("member", messageResponse.Data.Sender.Role);
        var node = Assert.Single(forwardResponse.Data!.MessageChain);
        Assert.Equal("nested text", node.Content!.PlainText);
    }

    [Fact]
    public async Task SendMessage_NewModelUsesConditionalTargetAndIndependentResult()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = _ => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject { ["message_id"] = "44" }
            }
        };
        var client = new OneBot11Client(transport);

        var response = await client.SendMessageAsync(
            new OneBot11SendMessage { new TextSendSegment("hello") },
            messageType: OneBot11MessageType.Group,
            groupId: 20001);

        var request = Assert.Single(transport.Requests);
        Assert.Equal("send_msg", request.Action);
        Assert.Equal("group", request.Parameters!["message_type"]!.GetValue<string>());
        Assert.Equal(20001L, request.Parameters["group_id"]!.GetValue<long>());
        Assert.Equal(44L, response.Data!.MessageId);
    }

    [Fact]
    public async Task DirectStringSend_RemainsSourceCompatibleWithTheLegacyResultType()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = _ => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject { ["message_id"] = 45 }
            }
        };
        var client = new OneBot11Client(transport);

        OneBotSdk.Net.V11.Responses.OneBot11Response<OneBotSdk.Net.V11.Responses.OneBot11SendMessageData> response =
            await client.SendGroupMessageAsync(20001, "legacy string");

        Assert.Equal(45L, response.Data!.MessageId);
        Assert.Equal("legacy string", Assert.Single(transport.Requests).Parameters!["message"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(InvocationMode.Normal, "send_private_msg")]
    [InlineData(InvocationMode.Async, "send_private_msg_async")]
    [InlineData(InvocationMode.RateLimited, "send_private_msg_rate_limited")]
    public async Task InvocationMode_AppliesOnlyTheOfficialMutuallyExclusiveSuffix(
        InvocationMode invocationMode,
        string expectedAction)
    {
        var transport = new RecordingTransport();
        var client = new OneBot11Client(transport);

        await client.SendPrivateMessageAsync(
            42,
            OneBot11SendMessage.FromString("hello"),
            invocationMode: invocationMode);

        Assert.Equal(expectedAction, Assert.Single(transport.Requests).Action);
    }

    [Fact]
    public async Task CustomAction_PreservesArbitraryEchoAndExtensionData()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = request => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject { ["vendor"] = 7 },
                ["echo"] = Clone(request.Echo)
            }
        };
        var client = new OneBot11Client(transport);
        var echo = new JsonObject { ["nested"] = new JsonArray(1, true) };

        var response = await client.CallActionAsync(
            "vendor_action",
            new JsonObject { ["parameter"] = "value" },
            echo: echo);

        Assert.Equal(7, response.Data!["vendor"]!.GetValue<int>());
        Assert.True(response.Echo!["nested"]![1]!.GetValue<bool>());
        Assert.Equal("vendor_action", Assert.Single(transport.Requests).Action);
        Assert.Equal("vendor_action", response.Action);
        Assert.Equal("value", response.RequestParameters!["parameter"]!.GetValue<string>());
        Assert.True(response.RequestEcho!["nested"]![1]!.GetValue<bool>());
        Assert.NotNull(response.RawRequestJson);
        Assert.NotNull(response.RawResponseJson);
    }

    [Fact]
    public async Task ResponseList_SkipsOnlyMalformedItemsAndKeepsUsableFieldsInPartialItems()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = _ => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["group_id"] = new JsonObject { ["invalid"] = true },
                        ["user_id"] = "12345",
                        ["nickname"] = "usable",
                        ["vendor_level"] = 9
                    },
                    42
                }
            }
        };
        var client = new OneBot11Client(transport);

        var response = await client.GetGroupMemberListAsync(10000);

        var member = Assert.Single(response.Data!);
        Assert.Null(member.GroupId);
        Assert.Equal(12345L, member.UserId);
        Assert.Equal("usable", member.Nickname);
        Assert.Equal(9, member.RawJson["vendor_level"]!.GetValue<int>());
        Assert.Equal(2, response.RawData!.AsArray().Count);
    }

    [Fact]
    public async Task CustomResponseParser_ReceivesDetachedUsableDataWhenAnExtensionCannotBeCloned()
    {
        var sourceData = new JsonObject
        {
            ["usable"] = 42,
            ["unserializable_extension"] = JsonValue.Create<object>(new CyclicExtension())
        };
        var transport = new RecordingTransport
        {
            ResponseFactory = _ => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = sourceData
            }
        };
        var client = new OneBot11Client(transport);
        JsonNode? parserInput = null;

        var response = await client.CallActionAsync<long?>("vendor_action", data =>
        {
            parserInput = data;
            return data?["usable"]?.GetValue<long>();
        });

        Assert.Equal(42L, response.Data);
        Assert.NotSame(sourceData, parserInput);
        Assert.Equal(42, response.RawData!["usable"]!.GetValue<int>());
        Assert.False(response.RawData.AsObject().ContainsKey("unserializable_extension"));
    }

    private sealed class RecordingTransport : IOneBot11ActionTransport
    {
        internal List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

        internal Func<RecordedRequest, JsonObject>? ResponseFactory { get; set; }

        public Task<OneBot11ActionTransportResult> SendAsync(
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
            return Task.FromResult(new OneBot11ActionTransportResult(
                action,
                requestParameters,
                request.Echo,
                OneBot11Json.Serialize(requestParameters),
                response,
                SerializeOrEmptyObject(response)));
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
        return node == null ? null : OneBot11Json.Parse(OneBot11Json.Serialize(node));
    }

    private static string SerializeOrEmptyObject(JsonObject value)
    {
        try
        {
            return OneBot11Json.Serialize(value);
        }
        catch (Exception)
        {
            // The recording test transport can receive deliberately unserializable in-memory extensions.
            // 记录型测试传输可能收到故意构造的不可序列化内存扩展。
            return "{}";
        }
    }

    private sealed class CyclicExtension
    {
        public CyclicExtension Self => this;
    }
}
