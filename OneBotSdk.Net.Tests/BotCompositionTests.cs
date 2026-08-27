using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Events;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class BotCompositionTests
{
    [Fact]
    public void Bot_EventStreamPropertiesForwardTheDispatcherInstances()
    {
        using var bot = CreateBot();

        Assert.Same(bot.Events.Events, bot.EventReceived);
        Assert.Same(bot.Events.Messages, bot.MessageReceived);
        Assert.Same(bot.Events.Notices, bot.NoticeReceived);
        Assert.Same(bot.Events.Requests, bot.RequestReceived);
        Assert.Same(bot.Events.MetaEvents, bot.MetaEventReceived);
        Assert.Same(bot.Events.UnknownEvents, bot.UnknownEventReceived);
    }

    [Fact]
    public void Bot_MessageReceivedSupportsOfTypeSubscriptions()
    {
        using var bot = CreateBot();
        GroupMessageEvent? received = null;
        using var subscription = bot.MessageReceived
            .OfType<GroupMessageEvent>()
            .Subscribe(value => received = value);
        var groupMessage = new GroupMessageEvent();

        bot.Events.Dispatch(groupMessage);

        Assert.Same(groupMessage, received);
    }

    [Fact]
    public void BotOptions_CreateIndependentDefaultUrisFromHostsAndPorts()
    {
        var options = new OneBot11BotOptions(
            new OneBot11ActionEndpointOptions("api-host", 5701),
            new OneBot11EventEndpointOptions("event-host", 6701));

        Assert.Equal(new Uri("http://api-host:5701/"), options.ActionEndpoint.Address);
        Assert.Equal(new Uri("ws://event-host:6701/event"), options.EventEndpoint.Address);
    }

    [Fact]
    public void BotOptions_MatchDocumentedNapCatServerConfiguration()
    {
        var options = new OneBot11BotOptions(
            new OneBot11ActionEndpointOptions("127.0.0.1", 3000, "123456"),
            new OneBot11EventEndpointOptions("127.0.0.1", 3001, "123456"));

        Assert.Equal(new Uri("http://127.0.0.1:3000/"), options.ActionEndpoint.Address);
        Assert.Equal("123456", options.ActionEndpoint.AccessToken);
        Assert.Equal(new Uri("ws://127.0.0.1:3001/event"), options.EventEndpoint.Address);
        Assert.Equal("123456", options.EventEndpoint.AccessToken);
    }

    [Fact]
    public void BotOptions_ReportTheSpecificInvalidPortParameter()
    {
        var actionFailure = Assert.Throws<ArgumentOutOfRangeException>(
            () => new OneBot11ActionEndpointOptions("api-host", 0));
        var eventFailure = Assert.Throws<ArgumentOutOfRangeException>(
            () => new OneBot11EventEndpointOptions("event-host", 65536));

        Assert.Equal("port", actionFailure.ParamName);
        Assert.Equal("port", eventFailure.ParamName);
    }

    [Fact]
    public void EndpointOptions_RejectInvalidMutationsWithoutReplacingValidValues()
    {
        var actionEndpoint = new OneBot11ActionEndpointOptions("api-host", 5701);
        var eventEndpoint = new OneBot11EventEndpointOptions("event-host", 6701);

        Assert.Throws<ArgumentException>(() =>
            actionEndpoint.Address = new Uri("ftp://api-host:5701/"));
        Assert.Throws<ArgumentException>(() =>
            eventEndpoint.Address = new Uri("http://event-host:6701/event"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            actionEndpoint.MaxResponseBodyBytes = 0);

        Assert.Equal(new Uri("http://api-host:5701/"), actionEndpoint.Address);
        Assert.Equal(new Uri("ws://event-host:6701/event"), eventEndpoint.Address);
        Assert.Equal(4 * 1024 * 1024, actionEndpoint.MaxResponseBodyBytes);
    }

    [Fact]
    public async Task Bot_UsesIndependentActionAndEventEndpoints()
    {
        Uri? actionRequestUri = null;
        string? actionAuthorization = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            actionRequestUri = request.RequestUri;
            actionAuthorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\",\"retcode\":0,\"data\":null}")
            });
        });
        using var httpClient = new HttpClient(handler);
        var actionEndpoint = new OneBot11ActionEndpointOptions(
            new Uri("http://api.example.test:5701/onebot/api/"),
            "action-secret");
        var eventEndpoint = new OneBot11EventEndpointOptions(
            new Uri("ws://events.example.test:6701/onebot/event"),
            "event-secret");
        var options = new OneBot11BotOptions(actionEndpoint, eventEndpoint);

        Assert.Same(actionEndpoint, options.ActionEndpoint);
        Assert.Same(eventEndpoint, options.EventEndpoint);

        using (var bot = new OneBot11Bot(options, null, httpClient))
        {
            // A bot owns snapshots; later caller mutations cannot redirect it or replace its credentials.
            // Bot 使用独立快照；调用方随后修改配置不能重定向连接或替换凭据。
            actionEndpoint.Address = new Uri("http://changed.example.test/");
            actionEndpoint.AccessToken = "changed-action-secret";
            eventEndpoint.Address = new Uri("ws://changed.example.test/event");
            eventEndpoint.AccessToken = "changed-event-secret";

            var response = await bot.Actions.CallActionAsync("get_status");

            Assert.True(response.IsSuccess);
            Assert.Equal(new Uri("http://api.example.test:5701/onebot/api/"), bot.ActionAddress);
            Assert.Equal(new Uri("ws://events.example.test:6701/onebot/event"), bot.EventAddress);
            Assert.Equal(new Uri("http://api.example.test:5701/onebot/api/get_status"), actionRequestUri);
            Assert.Equal("Bearer action-secret", actionAuthorization);
            Assert.NotNull(bot.Events);
        }
    }

    [Fact]
    public async Task Bot_DoesNotReuseTheEventTokenForActionRequests()
    {
        string? actionAuthorization = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            actionAuthorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\",\"retcode\":0,\"data\":null}")
            });
        });
        using var httpClient = new HttpClient(handler);
        var options = new OneBot11BotOptions(
            new OneBot11ActionEndpointOptions("127.0.0.1", 3000, "   "),
            new OneBot11EventEndpointOptions("127.0.0.1", 3001, "event-only-secret"));

        using (var bot = new OneBot11Bot(options, null, httpClient))
        {
            await bot.Actions.CallActionAsync("get_status");
        }

        Assert.Null(actionAuthorization);
    }

    [Theory]
    [InlineData("safe\r\nX-Injected: action", true)]
    [InlineData("safe\nX-Injected: event", false)]
    public void Bot_RejectsUnsafeTokensBeforeOpeningEitherEndpoint(string accessToken, bool useActionEndpoint)
    {
        var actionToken = useActionEndpoint ? accessToken : "safe-action-token";
        var eventToken = useActionEndpoint ? "safe-event-token" : accessToken;
        var options = new OneBot11BotOptions(
            new OneBot11ActionEndpointOptions("127.0.0.1", 3000, actionToken),
            new OneBot11EventEndpointOptions("127.0.0.1", 3001, eventToken));

        var exception = Assert.Throws<ArgumentException>(() => new OneBot11Bot(options));

        Assert.Equal("AccessToken", exception.ParamName);
    }

    [Theory]
    [InlineData("ftp://127.0.0.1:5700/")]
    [InlineData("http://127.0.0.1:0/")]
    [InlineData("http://127.0.0.1:5700/api?token=value")]
    [InlineData("http://127.0.0.1:5700/api#fragment")]
    public void ActionEndpoint_RejectsInvalidCompleteAddressesImmediately(string address)
    {
        Assert.Throws<ArgumentException>(() =>
            new OneBot11ActionEndpointOptions(new Uri(address), "action-secret"));
    }

    [Theory]
    [InlineData("http://127.0.0.1:6700/event")]
    [InlineData("ws://127.0.0.1:0/event")]
    [InlineData("ws://127.0.0.1:6700/event#fragment")]
    public void EventEndpoint_RejectsInvalidCompleteAddressesImmediately(string address)
    {
        Assert.Throws<ArgumentException>(() =>
            new OneBot11EventEndpointOptions(new Uri(address), "event-secret"));
    }

    private static OneBot11Bot CreateBot()
    {
        return new OneBot11Bot(
            new OneBot11BotOptions(
                new OneBot11ActionEndpointOptions("127.0.0.1", 3000),
                new OneBot11EventEndpointOptions("127.0.0.1", 3001)));
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
