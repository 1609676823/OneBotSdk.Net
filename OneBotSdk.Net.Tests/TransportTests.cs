using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Transports;
using OneBotSdk.Net.V11.Transports.Http;
using OneBotSdk.Net.V11.Transports.WebSockets;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class TransportTests
{
    [Fact]
    public void HttpPostSignature_MatchesTheRfcHmacSha1VectorAndRejectsMutation()
    {
        var secret = new string('\x0b', 20);
        var body = Encoding.ASCII.GetBytes("Hi There");
        const string expected = "sha1=b617318655057264e28bc0b6fb378c8ef146be00";

        Assert.Equal(expected, OneBot11HttpPostSignature.Compute(body, secret));
        Assert.True(OneBot11HttpPostSignature.Verify(body, expected, secret));

        body[0] ^= 1;
        Assert.False(OneBot11HttpPostSignature.Verify(body, expected, secret));
    }

    [Fact]
    public async Task HttpActionTransport_PostsJsonParametersAndBearerToken()
    {
        const string rawResponseJson = "{ \"status\":\"ok\", \"retcode\":0, \"data\":{\"message_id\":7,\"text\":\"\\u4e2d\\u6587\"} }";
        Uri? requestUri = null;
        string? authorization = null;
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            requestUri = request.RequestUri;
            authorization = request.Headers.Authorization?.ToString();
            requestBody = await request.Content!.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(rawResponseJson)
            };
        });
        using var httpClient = new HttpClient(handler);
        using var transport = new OneBot11HttpActionTransport(
            new OneBot11HttpActionTransportOptions(new Uri("http://127.0.0.1:5700/api"))
            {
                AccessToken = "secret-token"
            },
            httpClient);

        var response = await transport.SendAsync(
            "send_group_msg",
            new JsonObject
            {
                ["group_id"] = 123456L,
                ["message"] = "hello"
            },
            JsonValue.Create("ignored-over-http"),
            CancellationToken.None);

        Assert.Equal(new Uri("http://127.0.0.1:5700/api/send_group_msg"), requestUri);
        Assert.Equal("Bearer secret-token", authorization);
        Assert.Equal(123456L, JsonNode.Parse(requestBody!)!["group_id"]!.GetValue<long>());
        Assert.Equal("send_group_msg", response.Action);
        Assert.Equal(123456L, response.RequestParameters["group_id"]!.GetValue<long>());
        Assert.Null(response.RequestEcho);
        Assert.Equal(requestBody, response.RawRequestJson);
        Assert.Equal(rawResponseJson, response.RawResponseJson);
        Assert.Equal(7, response.Response["data"]!["message_id"]!.GetValue<int>());
        Assert.Equal("中文", response.Response["data"]!["text"]!.GetValue<string>());
    }

    [Fact]
    public async Task HttpActionTransport_MapsNonSuccessStatusToTransportFailure()
    {
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent(string.Empty)
            }));
        using var httpClient = new HttpClient(handler);
        using var transport = new OneBot11HttpActionTransport(
            new OneBot11HttpActionTransportOptions(new Uri("http://127.0.0.1:5700/")),
            httpClient);

        var exception = await Assert.ThrowsAsync<OneBot11TransportException>(() =>
            transport.SendAsync("get_status", null, null, CancellationToken.None));

        Assert.Equal(OneBot11TransportError.HttpFailure, exception.Error);
        Assert.Equal(HttpStatusCode.Forbidden, exception.HttpStatusCode);
        Assert.Equal("get_status", exception.Action);
        Assert.NotNull(exception.RequestParameters);
        Assert.Equal("{}", exception.RawRequestJson);
        Assert.Equal(string.Empty, exception.RawResponseJson);
    }

    [Fact]
    public async Task HttpActionTransport_PreservesRequestAndMalformedResponseTextOnProtocolFailure()
    {
        const string malformedResponseJson = "{not-valid-json";
        var handler = new StubHttpMessageHandler(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(malformedResponseJson)
            }));
        using var httpClient = new HttpClient(handler);
        using var transport = new OneBot11HttpActionTransport(
            new OneBot11HttpActionTransportOptions(new Uri("http://127.0.0.1:5700/")),
            httpClient);

        var exception = await Assert.ThrowsAsync<OneBot11TransportException>(() =>
            transport.SendAsync(
                "get_group_info",
                new JsonObject { ["group_id"] = 123L },
                null,
                CancellationToken.None));

        Assert.Equal(OneBot11TransportError.ProtocolViolation, exception.Error);
        Assert.Equal("get_group_info", exception.Action);
        Assert.Equal(123L, exception.RequestParameters!["group_id"]!.GetValue<long>());
        Assert.Equal("{\"group_id\":123}", exception.RawRequestJson);
        Assert.Equal(malformedResponseJson, exception.RawResponseJson);
    }

    [Theory]
    [InlineData("http://127.0.0.1:0/")]
    [InlineData("http://127.0.0.1:3000/api?token=value")]
    [InlineData("http://127.0.0.1:3000/api#fragment")]
    public void HttpActionTransport_RejectsAmbiguousBaseAddresses(string address)
    {
        var options = new OneBot11HttpActionTransportOptions(new Uri(address));

        Assert.Throws<ArgumentException>(() => new OneBot11HttpActionTransport(options));
    }

    [Theory]
    [InlineData("ws://127.0.0.1:0/event")]
    [InlineData("ws://127.0.0.1:3001/event#fragment")]
    public void ForwardWebSocketClient_RejectsInvalidEndpoints(string address)
    {
        var options = new OneBot11ForwardWebSocketClientOptions(new Uri(address));

        Assert.Throws<ArgumentException>(() =>
            new OneBot11ForwardWebSocketClient(options, new OneBot11EventDispatcher()));
    }

    [Theory]
    [InlineData("safe\r\nX-Injected: http", true)]
    [InlineData("safe\nX-Injected: websocket", false)]
    public void Transports_RejectUnsafeAuthorizationTokens(string accessToken, bool useHttp)
    {
        if (useHttp)
        {
            var options = new OneBot11HttpActionTransportOptions(new Uri("http://127.0.0.1:3000/"))
            {
                AccessToken = accessToken
            };

            Assert.Throws<ArgumentException>(() => new OneBot11HttpActionTransport(options));
            return;
        }

        var webSocketOptions = new OneBot11ForwardWebSocketClientOptions(new Uri("ws://127.0.0.1:3001/event"))
        {
            AccessToken = accessToken
        };

        Assert.Throws<ArgumentException>(() =>
            new OneBot11ForwardWebSocketClient(webSocketOptions, new OneBot11EventDispatcher()));
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
