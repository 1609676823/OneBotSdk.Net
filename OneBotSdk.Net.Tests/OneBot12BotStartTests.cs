using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12;
using OneBotSdk.Net.V12.Client;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class OneBot12BotStartTests
{
    private const string VersionResponse =
        "{\"status\":\"ok\",\"retcode\":0,\"data\":{\"impl\":\"test\",\"version\":\"1.0\",\"onebot_version\":\"12\"}}";

    private const string StatusResponse =
        "{\"status\":\"ok\",\"retcode\":0,\"data\":{\"good\":true,\"bots\":[{\"self\":{\"platform\":\"qq\",\"user_id\":\"123xxxxxxx\"},\"online\":true}]}}";

    [Fact]
    public async Task StartAsync_VerifiesVersionThenStatusBeforeConnectingEvents()
    {
        var steps = new List<string>();
        var requests = new List<JsonObject>();
        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal("/", request.RequestUri!.AbsolutePath);
            var body = await request.Content!.ReadAsStringAsync().ConfigureAwait(false);
            var envelope = JsonNode.Parse(body)!.AsObject();
            requests.Add(envelope);
            var action = envelope["action"]!.GetValue<string>();
            steps.Add(action);
            return CreateResponse(action == "get_version" ? VersionResponse : StatusResponse);
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            steps.Add("connect_event");
            return Task.CompletedTask;
        });

        var result = await bot.StartAsync();

        Assert.Equal(new[] { "get_version", "get_status", "connect_event" }, steps);
        Assert.Equal("12", result.VersionResponse.Data?.OneBotVersion);
        Assert.True(result.StatusResponse.Data?.Good);
        Assert.All(requests, envelope => Assert.Null(envelope["self"]));
        Assert.Equal("get_version", result.VersionResponse.Action);
        Assert.Equal("get_status", result.StatusResponse.Action);
        Assert.NotNull(result.VersionResponse.RawRequestJson);
        Assert.NotNull(result.StatusResponse.RawResponseJson);
    }

    [Fact]
    public void Start_SynchronouslyReturnsBothTypedMetaResponses()
    {
        var actionCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            actionCalls++;
            return Task.FromResult(CreateResponse(actionCalls == 1 ? VersionResponse : StatusResponse));
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ => Task.CompletedTask);

        var result = bot.Start();

        Assert.Equal("test", result.VersionResponse.Data?.Impl);
        Assert.True(result.StatusResponse.Data?.Bots[0].Online);
    }

    [Fact]
    public async Task StartAsync_WhenVersionFails_DoesNotRequestStatusOrConnectEvents()
    {
        var actionCalls = 0;
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            actionCalls++;
            return Task.FromResult(CreateResponse(
                "{\"status\":\"failed\",\"retcode\":10002,\"message\":\"unsupported\",\"data\":null}"));
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return Task.CompletedTask;
        });

        var exception = await Assert.ThrowsAsync<OneBot12BotStartException>(() => bot.StartAsync());

        Assert.Equal(OneBot12BotStartStage.GetVersion, exception.Stage);
        Assert.Equal(10002L, exception.Response.RetCode);
        Assert.Equal("get_version", exception.Response.Action);
        Assert.Equal(1, actionCalls);
        Assert.Equal(0, eventCalls);
    }

    [Fact]
    public async Task StartAsync_WhenStatusFails_DoesNotConnectEvents()
    {
        var actionCalls = 0;
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            actionCalls++;
            var json = actionCalls == 1
                ? VersionResponse
                : "{\"status\":\"failed\",\"retcode\":20001,\"message\":\"bad status\",\"data\":null}";
            return Task.FromResult(CreateResponse(json));
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return Task.CompletedTask;
        });

        var exception = await Assert.ThrowsAsync<OneBot12BotStartException>(() => bot.StartAsync());

        Assert.Equal(OneBot12BotStartStage.GetStatus, exception.Stage);
        Assert.Equal("get_status", exception.Response.Action);
        Assert.Equal(2, actionCalls);
        Assert.Equal(0, eventCalls);
    }

    [Fact]
    public void BotOptions_KeepEndpointsTokensAndDefaultSelfIndependent()
    {
        var defaultSelf = new OneBot12Self("qq", "123xxxxxxx");
        var options = new OneBot12BotOptions(
            new OneBot12ActionEndpointOptions("127.0.0.1", 3000, "action-token"),
            new OneBot12EventEndpointOptions("127.0.0.1", 3001, "event-token"),
            defaultSelf);

        var firstSnapshot = options.DefaultSelf;
        firstSnapshot!.RawJson["extension"] = "changed";
        var secondSnapshot = options.DefaultSelf;

        Assert.Equal(new Uri("http://127.0.0.1:3000/"), options.ActionEndpoint.Address);
        Assert.Equal("action-token", options.ActionEndpoint.AccessToken);
        Assert.Equal(new Uri("ws://127.0.0.1:3001/"), options.EventEndpoint.Address);
        Assert.Equal("event-token", options.EventEndpoint.AccessToken);
        Assert.NotSame(firstSnapshot, secondSnapshot);
        Assert.Equal("qq", secondSnapshot?.Platform);
        Assert.Equal("123xxxxxxx", secondSnapshot?.UserId);
        Assert.Null(secondSnapshot?.RawJson["extension"]);
    }

    private static OneBot12Bot CreateBot(
        HttpClient httpClient,
        Func<CancellationToken, Task> connectEventAsync)
    {
        return new OneBot12Bot(
            new OneBot12BotOptions(
                new OneBot12ActionEndpointOptions("127.0.0.1", 3000, "action-token"),
                new OneBot12EventEndpointOptions("127.0.0.1", 3001, "event-token"),
                new OneBot12Self("qq", "123xxxxxxx")),
            null,
            httpClient,
            connectEventAsync);
    }

    private static HttpResponseMessage CreateResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        internal StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _handler(request);
        }
    }
}
