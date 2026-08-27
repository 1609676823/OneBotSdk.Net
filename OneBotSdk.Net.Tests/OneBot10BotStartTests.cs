using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class OneBot10BotStartTests
{
    private const string LoginResponse =
        "{\"status\":\"ok\",\"retcode\":0,\"data\":{\"user_id\":1230000000,\"nickname\":\"OneBot 10\"}}";

    [Fact]
    public async Task StartAsync_GetsLoginInformationBeforeConnectingEvents()
    {
        var steps = new List<string>();
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal("/get_login_info", request.RequestUri!.AbsolutePath);
            steps.Add("get_login_info");
            return Task.FromResult(CreateResponse(LoginResponse));
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            steps.Add("connect_event");
            return Task.CompletedTask;
        });

        var response = await bot.StartAsync();

        Assert.Equal(new[] { "get_login_info", "connect_event" }, steps);
        Assert.Equal(1230000000L, response.Data?.UserId);
        Assert.Equal("OneBot 10", response.Data?.Nickname);
        Assert.Equal("get_login_info", response.Action);
        Assert.NotNull(response.RawRequestJson);
        Assert.NotNull(response.RawResponseJson);
    }

    [Fact]
    public async Task StartAsync_WhenLoginFails_DoesNotConnectEvents()
    {
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(CreateResponse(
            "{\"status\":\"failed\",\"retcode\":100,\"wording\":\"denied\",\"data\":null}")));
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return Task.CompletedTask;
        });

        var exception = await Assert.ThrowsAsync<OneBot10BotStartException>(() => bot.StartAsync());

        Assert.Equal("get_login_info", exception.LoginInfoResponse.Action);
        Assert.Equal(100L, exception.LoginInfoResponse.RetCode);
        Assert.Equal(0, eventCalls);
    }

    [Fact]
    public void BotOptions_KeepActionAndEventAddressesAndTokensIndependent()
    {
        var options = new OneBot10BotOptions(
            new OneBot10ActionEndpointOptions("127.0.0.1", 3000, "action-token"),
            new OneBot10EventEndpointOptions("127.0.0.1", 3001, "event-token"));

        Assert.Equal(new Uri("http://127.0.0.1:3000/"), options.ActionEndpoint.Address);
        Assert.Equal("action-token", options.ActionEndpoint.AccessToken);
        Assert.Equal(new Uri("ws://127.0.0.1:3001/event"), options.EventEndpoint.Address);
        Assert.Equal("event-token", options.EventEndpoint.AccessToken);
    }

    private static OneBot10Bot CreateBot(
        HttpClient httpClient,
        Func<CancellationToken, Task> connectEventAsync)
    {
        return new OneBot10Bot(
            new OneBot10BotOptions(
                new OneBot10ActionEndpointOptions("127.0.0.1", 3000, "action-token"),
                new OneBot10EventEndpointOptions("127.0.0.1", 3001, "event-token")),
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
