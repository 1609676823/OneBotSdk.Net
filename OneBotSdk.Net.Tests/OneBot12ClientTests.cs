using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Messages;
using OneBotSdk.Net.V12.Transports;
using OneBotSdk.Net.V12.Transports.Http;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class OneBot12ClientTests
{
    private static readonly string[] OfficialActions =
    {
        "get_latest_events", "get_supported_actions", "get_status", "get_version",
        "send_message", "delete_message",
        "get_self_info", "get_user_info", "get_friend_list",
        "get_group_info", "get_group_list", "get_group_member_info", "get_group_member_list",
        "set_group_name", "leave_group",
        "get_guild_info", "get_guild_list", "set_guild_name", "get_guild_member_info",
        "get_guild_member_list", "leave_guild",
        "get_channel_info", "get_channel_list", "set_channel_name", "get_channel_member_info",
        "get_channel_member_list", "leave_channel",
        "upload_file", "upload_file_fragmented", "get_file", "get_file_fragmented"
    };

    [Fact]
    public void ActionCatalog_MatchesExactlyThirtyOneOfficialActions()
    {
        Assert.Equal(31, OneBot12Actions.All.Count);
        Assert.Equal(OfficialActions, OneBot12Actions.All);
        Assert.Equal(31, OneBot12Actions.All.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task TypedMessageAction_UsesStringIdsSelfAndCompleteTrace()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = request => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject
                {
                    ["message_id"] = "message-1",
                    ["time"] = "1700000000.25",
                    ["vendor_field"] = true
                },
                ["message"] = "",
                ["echo"] = request.Echo
            }
        };
        var defaultSelf = new OneBot12Self("qq", "bot-1");
        var client = new OneBot12Client(transport, defaultSelf);

        var response = await client.SendGroupMessageAsync(
            "group-1",
            new OneBot12SendMessage { new OneBot12TextSendSegment("hello") },
            echo: "echo-1");

        var request = Assert.Single(transport.Requests);
        Assert.Equal("send_message", request.Action);
        Assert.Equal("group", request.Parameters!["detail_type"]!.GetValue<string>());
        Assert.Equal("group-1", request.Parameters["group_id"]!.GetValue<string>());
        Assert.Equal("qq", request.Self!.Platform);
        Assert.Equal("bot-1", request.Self.UserId);
        Assert.Equal("message-1", response.Data!.MessageId);
        Assert.Equal(1700000000.25, response.Data.Time);
        Assert.True(response.RawData!["vendor_field"]!.GetValue<bool>());
        Assert.Equal("send_message", response.Action);
        Assert.Equal("echo-1", response.RequestEcho);
        Assert.Equal("bot-1", response.RequestSelf!.UserId);
        Assert.Contains("\"action\":\"send_message\"", response.RawRequestJson);
        Assert.Contains("\"self\":", response.RawRequestJson);
        Assert.Contains("\"vendor_field\":true", response.RawResponseJson);
    }

    [Fact]
    public async Task StronglyTypedMethods_CoverEveryOfficialActionWithoutNetworkIo()
    {
        var transport = new RecordingTransport();
        var client = new OneBot12Client(transport, new OneBot12Self("qq", "bot-1"));
        var message = OneBot12SendMessage.FromString("test");

        await client.GetLatestEventsAsync();
        await client.GetSupportedActionsAsync();
        await client.GetStatusAsync();
        await client.GetVersionAsync();
        await client.SendMessageAsync("private", message, userId: "user-1");
        await client.DeleteMessageAsync("message-1");
        await client.GetSelfInfoAsync();
        await client.GetUserInfoAsync("user-1");
        await client.GetFriendListAsync();
        await client.GetGroupInfoAsync("group-1");
        await client.GetGroupListAsync();
        await client.GetGroupMemberInfoAsync("group-1", "user-1");
        await client.GetGroupMemberListAsync("group-1");
        await client.SetGroupNameAsync("group-1", "name");

        // These calls use an in-memory recording transport; no membership-changing request reaches a bot.
        // 这些调用使用内存记录传输；不会有任何改变成员关系的请求到达机器人。
        await client.LeaveGroupAsync("group-1");
        await client.GetGuildInfoAsync("guild-1");
        await client.GetGuildListAsync();
        await client.SetGuildNameAsync("guild-1", "name");
        await client.GetGuildMemberInfoAsync("guild-1", "user-1");
        await client.GetGuildMemberListAsync("guild-1");
        await client.LeaveGuildAsync("guild-1");
        await client.GetChannelInfoAsync("guild-1", "channel-1");
        await client.GetChannelListAsync("guild-1");
        await client.SetChannelNameAsync("guild-1", "channel-1", "name");
        await client.GetChannelMemberInfoAsync("guild-1", "channel-1", "user-1");
        await client.GetChannelMemberListAsync("guild-1", "channel-1");
        await client.LeaveChannelAsync("guild-1", "channel-1");
        await client.UploadFileAsync(OneBot12UploadFileRequest.FromData("a.bin", new byte[] { 1 }));
        await client.PrepareUploadFileFragmentedAsync("a.bin", 1);
        await client.GetFileAsync("file-1", OneBot12FileAccessType.Data);
        await client.PrepareGetFileFragmentedAsync("file-1");

        Assert.Equal(31, transport.Requests.Count);
        Assert.Equal(
            OneBot12Actions.All.OrderBy(value => value, StringComparer.Ordinal),
            transport.Requests.Select(request => request.Action).OrderBy(value => value, StringComparer.Ordinal));
    }

    [Fact]
    public async Task MetaActions_DoNotSendDefaultSelf()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = _ => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject
                {
                    ["impl"] = "test",
                    ["version"] = "1.0.0",
                    ["onebot_version"] = "12"
                },
                ["message"] = ""
            }
        };
        var client = new OneBot12Client(transport, new OneBot12Self("qq", "bot-1"));

        var response = await client.GetVersionAsync();

        var request = Assert.Single(transport.Requests);
        Assert.Null(request.Self);
        Assert.Null(response.RequestSelf);
        Assert.Equal("test", response.Data!.Impl);
        Assert.Equal("12", response.Data.OneBotVersion);
    }

    [Fact]
    public async Task FragmentedFileMethods_WriteAllFiveOfficialStages()
    {
        var transport = new RecordingTransport
        {
            ResponseFactory = request => new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = request.Parameters?["stage"]?.GetValue<string>() == "transfer"
                    ? new JsonObject { ["data"] = "AQID" }
                    : new JsonObject
                    {
                        ["file_id"] = "file-1",
                        ["name"] = "a.bin",
                        ["total_size"] = 3,
                        ["sha256"] = "hash"
                    },
                ["message"] = ""
            }
        };
        var client = new OneBot12Client(transport, new OneBot12Self("qq", "bot-1"));

        await client.PrepareUploadFileFragmentedAsync("a.bin", 3);
        await client.TransferUploadFileFragmentAsync("file-1", 0, new byte[] { 1, 2, 3 });
        await client.FinishUploadFileFragmentedAsync("file-1", "hash");
        await client.PrepareGetFileFragmentedAsync("file-1");
        var fragment = await client.GetFileFragmentAsync("file-1", 0, 3);

        Assert.Equal(5, transport.Requests.Count);
        Assert.Equal(
            new[] { "prepare", "transfer", "finish", "prepare", "transfer" },
            transport.Requests.Select(request => request.Parameters!["stage"]!.GetValue<string>()));
        Assert.Equal("AQID", transport.Requests[1].Parameters!["data"]!.GetValue<string>());
        Assert.Equal(3L, transport.Requests[4].Parameters!["size"]!.GetValue<long>());
        Assert.Equal(new byte[] { 1, 2, 3 }, fragment.Data!.Data);
    }

    [Fact]
    public async Task HttpTransport_PostsCompleteEnvelopeOnlyToRootEndpoint()
    {
        var handler = new RecordingHttpHandler();
        using (var httpClient = new HttpClient(handler))
        using (var transport = new OneBot12HttpActionTransport(
            new OneBot12HttpActionTransportOptions(new Uri("http://127.0.0.1:5700/"))
            {
                AccessToken = "secret"
            },
            httpClient))
        {
            var result = await transport.SendAsync(
                "get_user_info",
                new JsonObject { ["user_id"] = "user-1" },
                "echo-1",
                new OneBot12Self("qq", "bot-1"),
                CancellationToken.None);

            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal(new Uri("http://127.0.0.1:5700/"), handler.RequestUri);
            Assert.Equal("Bearer", handler.AuthorizationScheme);
            Assert.Equal("secret", handler.AuthorizationParameter);
            var envelope = Assert.IsType<JsonObject>(OneBot12Json.Parse(handler.Body!));
            Assert.Equal("get_user_info", envelope["action"]!.GetValue<string>());
            Assert.Equal("user-1", envelope["params"]!["user_id"]!.GetValue<string>());
            Assert.Equal("echo-1", envelope["echo"]!.GetValue<string>());
            Assert.Equal("qq", envelope["self"]!["platform"]!.GetValue<string>());
            Assert.Equal("bot-1", envelope["self"]!["user_id"]!.GetValue<string>());
            Assert.Equal(handler.Body, result.RawRequestJson);
            Assert.Equal("ok", result.Response["status"]!.GetValue<string>());
        }
    }

    [Fact]
    public void HttpOptions_RejectVersionElevenStyleActionPath()
    {
        var options = new OneBot12HttpActionTransportOptions(
            new Uri("http://127.0.0.1:5700/get_version"));

        Assert.Throws<ArgumentException>(() => new OneBot12HttpActionTransport(options));
    }

    private sealed class RecordingTransport : IOneBot12ActionTransport
    {
        internal List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

        internal Func<RecordedRequest, JsonObject>? ResponseFactory { get; set; }

        public Task<OneBot12ActionTransportResult> SendAsync(
            string action,
            JsonObject? parameters,
            string? echo,
            OneBot12Self? self,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new RecordedRequest(
                action,
                Clone(parameters) as JsonObject,
                echo,
                self == null ? null : OneBot12Self.Parse(self.ToJsonObject()));
            Requests.Add(request);
            var response = ResponseFactory == null
                ? new JsonObject
                {
                    ["status"] = "ok",
                    ["retcode"] = 0,
                    ["data"] = null,
                    ["message"] = "",
                    ["echo"] = echo
                }
                : ResponseFactory(request);
            var requestParameters = request.Parameters ?? new JsonObject();
            var envelope = new JsonObject
            {
                ["action"] = action,
                ["params"] = Clone(requestParameters)
            };
            if (echo != null) envelope["echo"] = echo;
            if (request.Self != null) envelope["self"] = request.Self.ToJsonObject();
            return Task.FromResult(new OneBot12ActionTransportResult(
                action,
                requestParameters,
                echo,
                request.Self,
                OneBot12Json.Serialize(envelope),
                response,
                OneBot12Json.Serialize(response)));
        }
    }

    private sealed class RecordedRequest
    {
        internal RecordedRequest(
            string action,
            JsonObject? parameters,
            string? echo,
            OneBot12Self? self)
        {
            Action = action;
            Parameters = parameters;
            Echo = echo;
            Self = self;
        }

        internal string Action { get; }
        internal JsonObject? Parameters { get; }
        internal string? Echo { get; }
        internal OneBot12Self? Self { get; }
    }

    private sealed class RecordingHttpHandler : HttpMessageHandler
    {
        internal HttpMethod? Method { get; private set; }
        internal Uri? RequestUri { get; private set; }
        internal string? AuthorizationScheme { get; private set; }
        internal string? AuthorizationParameter { get; private set; }
        internal string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Body = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"status\":\"ok\",\"retcode\":0,\"data\":{},\"message\":\"\",\"echo\":\"echo-1\"}",
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }

    private static JsonNode? Clone(JsonNode? node)
    {
        return node == null ? null : OneBot12Json.Parse(OneBot12Json.Serialize(node));
    }
}
