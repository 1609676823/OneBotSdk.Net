using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Transports;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class BotStartTests
{
    private const string SuccessfulLoginResponse =
        "{\"status\":\"ok\",\"retcode\":0,\"data\":{\"user_id\":1230000000,\"nickname\":\"OneBot\"}}";

    [Fact]
    public async Task StartAsync_GetsLoginInformationBeforeConnectingEvents()
    {
        var steps = new List<string>();
        var handler = new StubHttpMessageHandler(request =>
        {
            steps.Add("get_login_info");
            Assert.Equal("/get_login_info", request.RequestUri!.AbsolutePath);
            return CreateResponseAsync(SuccessfulLoginResponse);
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
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(1230000000L, response.Data!.UserId);
        Assert.Equal("OneBot", response.Data.Nickname);
    }

    [Fact]
    public void Start_SynchronouslyReturnsTheSameTypedLoginResponse()
    {
        var handler = new StubHttpMessageHandler(_ => CreateResponseAsync(SuccessfulLoginResponse));
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ => Task.CompletedTask);

        var response = bot.Start();

        Assert.True(response.IsSuccess);
        Assert.Equal(1230000000L, response.Data?.UserId);
    }

    [Fact]
    public async Task StartAsync_ThrowsTypedFailureAndSkipsEventsWhenLoginActionFails()
    {
        var eventConnectCalls = 0;
        var handler = new StubHttpMessageHandler(_ => CreateResponseAsync(
            "{\"status\":\"failed\",\"retcode\":100,\"data\":null,\"wording\":\"denied\"}"));
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventConnectCalls++;
            return Task.CompletedTask;
        });

        var exception = await Assert.ThrowsAsync<OneBot11BotStartException>(() => bot.StartAsync());

        Assert.Equal(0, eventConnectCalls);
        Assert.Equal("failed", exception.LoginInfoResponse.Status);
        Assert.Equal(100L, exception.LoginInfoResponse.RetCode);
        Assert.Equal("denied", exception.LoginInfoResponse.RawJson["wording"]?.GetValue<string>());
    }

    [Fact]
    public async Task StartAsync_AcceptsSuccessfulEnvelopeWhenOptionalLoginDataIsMissing()
    {
        var eventConnectCalls = 0;
        var handler = new StubHttpMessageHandler(_ => CreateResponseAsync(
            "{\"status\":\"ok\",\"retcode\":0,\"data\":null}"));
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventConnectCalls++;
            return Task.CompletedTask;
        });

        var response = await bot.StartAsync();

        Assert.True(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Equal(1, eventConnectCalls);
    }

    [Fact]
    public async Task StartAsync_PropagatesHttpTransportFailureAndSkipsEvents()
    {
        var eventConnectCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
            throw new HttpRequestException("Expected HTTP connection failure."));
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventConnectCalls++;
            return Task.CompletedTask;
        });

        var exception = await Assert.ThrowsAsync<OneBot11TransportException>(() => bot.StartAsync());

        Assert.Equal(OneBot11TransportError.ConnectionFailed, exception.Error);
        Assert.Equal("get_login_info", exception.Action);
        Assert.Equal(0, eventConnectCalls);
    }

    [Fact]
    public void Start_DoesNotWrapTheOriginalConnectionException()
    {
        var expected = new OneBot11TransportException(
            OneBot11TransportError.ConnectionFailed,
            "Expected event connection failure.");
        var handler = new StubHttpMessageHandler(_ => CreateResponseAsync(SuccessfulLoginResponse));
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ => throw expected);

        var actual = Assert.Throws<OneBot11TransportException>(() => bot.Start());

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task StartAsync_DoesNotCallEitherEndpointWhenAlreadyCanceled()
    {
        var actionCalls = 0;
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            actionCalls++;
            return CreateResponseAsync(SuccessfulLoginResponse);
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return Task.CompletedTask;
        });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            bot.StartAsync(cancellation.Token));

        Assert.Equal(0, actionCalls);
        Assert.Equal(0, eventCalls);
    }

    [Fact]
    public async Task StartAsync_ReleasesItsGateAfterAProtocolFailureSoItCanBeRetried()
    {
        var actionCalls = 0;
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            actionCalls++;
            return actionCalls == 1
                ? CreateResponseAsync("{\"status\":\"failed\",\"retcode\":1,\"data\":null}")
                : CreateResponseAsync(SuccessfulLoginResponse);
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<OneBot11BotStartException>(() => bot.StartAsync());
        var response = await bot.StartAsync();

        Assert.True(response.IsSuccess);
        Assert.Equal(2, actionCalls);
        Assert.Equal(1, eventCalls);
    }

    [Fact]
    public async Task ConcurrentStartAsyncCalls_DoNotOverlapTheirStartupSequences()
    {
        var releaseFirstAction = new TaskCompletionSource<bool>();
        var firstActionEntered = new TaskCompletionSource<bool>();
        var actionCalls = 0;
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(async _ =>
        {
            actionCalls++;
            if (actionCalls == 1)
            {
                firstActionEntered.SetResult(true);
                await releaseFirstAction.Task.ConfigureAwait(false);
            }

            return await CreateResponseAsync(SuccessfulLoginResponse).ConfigureAwait(false);
        });
        using var httpClient = new HttpClient(handler);
        using var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return eventCalls == 1
                ? Task.CompletedTask
                : Task.FromException(new InvalidOperationException("The event endpoint is already connected."));
        });

        var firstStart = bot.StartAsync();
        await firstActionEntered.Task;
        var secondStart = bot.StartAsync();

        Assert.Equal(1, actionCalls);
        releaseFirstAction.SetResult(true);
        await firstStart;
        await Assert.ThrowsAsync<InvalidOperationException>(() => secondStart);
        Assert.Equal(2, actionCalls);
        Assert.Equal(2, eventCalls);
    }

    [Fact]
    public async Task StartAndStartAsync_RejectDisposedBotsBeforePerformingIo()
    {
        var actionCalls = 0;
        var eventCalls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            actionCalls++;
            return CreateResponseAsync(SuccessfulLoginResponse);
        });
        using var httpClient = new HttpClient(handler);
        var bot = CreateBot(httpClient, _ =>
        {
            eventCalls++;
            return Task.CompletedTask;
        });
        bot.Dispose();

        Assert.Throws<ObjectDisposedException>(() => bot.Start());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => bot.StartAsync());
        Assert.Equal(0, actionCalls);
        Assert.Equal(0, eventCalls);
    }

    private static OneBot11Bot CreateBot(
        HttpClient httpClient,
        Func<CancellationToken, Task> connectEventAsync)
    {
        var options = new OneBot11BotOptions(
            new OneBot11ActionEndpointOptions("127.0.0.1", 3000, "action-token"),
            new OneBot11EventEndpointOptions("127.0.0.1", 3001, "event-token"));
        return new OneBot11Bot(options, null, httpClient, connectEventAsync);
    }

    private static Task<HttpResponseMessage> CreateResponseAsync(string json)
    {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
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
