using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Transports;
using OneBotSdk.Net.V11.Transports.WebSockets;
using Xunit;

namespace OneBotSdk.Net.Tests;

[Collection(JsonConfigurationCollection.Name)]
public sealed class WebSocketSessionTests
{
    [Fact]
    public async Task Session_ReassemblesFragmentedTextBeforeDispatchingEvent()
    {
        using var socket = new ScriptedWebSocket();
        var dispatcher = new OneBot11EventDispatcher();
        var received = new TaskCompletionSource<OneBot11Event>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = dispatcher.Events.Subscribe(value => received.TrySetResult(value));
        using var session = new OneBot11WebSocketSession(socket, dispatcher);
        await session.StartAsync(CancellationToken.None);

        socket.EnqueueText("{\"post_type\":\"meta_event\",\"meta_event_type\":\"lifecycle\",\"sub_type\":\"connect\"}", 17);

        var parsed = Assert.IsType<LifecycleMetaEvent>(await WithTimeout(received.Task));
        Assert.Equal("connect", parsed.SubType);
    }

    [Fact]
    public async Task Session_CorrelatesAnObjectEchoEvenWhenResponsePropertiesAreReordered()
    {
        using var socket = new ScriptedWebSocket();
        using var session = new OneBot11WebSocketSession(socket, new OneBot11EventDispatcher());
        string? rawRequestJson = null;
        string? rawResponseJson = null;
        socket.TextSent = requestText =>
        {
            rawRequestJson = requestText;
            var request = JsonNode.Parse(requestText)!.AsObject();
            var echo = request["echo"]!.AsObject();
            rawResponseJson = " \r\n" + OneBot11Json.Serialize(new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject { ["result"] = "matched" },
                ["echo"] = new JsonObject
                {
                    ["a"] = echo["a"]!.GetValue<int>(),
                    ["b"] = echo["b"]!.GetValue<int>()
                }
            }) + "\n";
            socket.EnqueueText(rawResponseJson, 23);
        };
        await session.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource();
        timeout.CancelAfter(TimeSpan.FromSeconds(3));

        var response = await session.SendAsync(
            "get_status",
            null,
            new JsonObject
            {
                ["b"] = 2,
                ["a"] = 1
            },
            timeout.Token);

        Assert.Equal("matched", response.Response["data"]!["result"]!.GetValue<string>());
        Assert.Equal("get_status", response.Action);
        Assert.Empty(response.RequestParameters);
        Assert.Equal(1, response.RequestEcho!["a"]!.GetValue<int>());
        Assert.Equal(rawRequestJson, response.RawRequestJson);
        Assert.Equal(rawResponseJson, response.RawResponseJson);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Session_CorrelatesUnicodeEchoAfterTheGlobalEncoderChanges(bool requestUsesUnsafeEncoder, bool responseUsesUnsafeEncoder)
    {
        const string echoText = "<中文&>";
        var previous = OneBot11Json.UseUnsafeRelaxedJsonEscaping;
        try
        {
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = requestUsesUnsafeEncoder;
            using var socket = new ScriptedWebSocket();
            using var session = new OneBot11WebSocketSession(socket, new OneBot11EventDispatcher());
            socket.TextSent = requestText =>
            {
                if (requestUsesUnsafeEncoder)
                {
                    Assert.Contains(echoText, requestText);
                }
                else
                {
                    Assert.DoesNotContain(echoText, requestText);
                }

                // Change the wire encoder before the response arrives; correlation must remain stable.
                // 在响应到达前切换线报文编码器；请求关联必须保持稳定。
                OneBot11Json.UseUnsafeRelaxedJsonEscaping = responseUsesUnsafeEncoder;
                socket.EnqueueText(OneBot11Json.Serialize(new JsonObject
                {
                    ["status"] = "ok",
                    ["retcode"] = 0,
                    ["data"] = new JsonObject { ["matched"] = true },
                    ["echo"] = echoText
                }));
            };
            await session.StartAsync(CancellationToken.None);
            using var timeout = new CancellationTokenSource();
            timeout.CancelAfter(TimeSpan.FromSeconds(3));

            var response = await session.SendAsync("get_status", null, JsonValue.Create(echoText), timeout.Token);

            Assert.True(response.Response["data"]!["matched"]!.GetValue<bool>());
        }
        finally
        {
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = previous;
        }
    }

    [Fact]
    public async Task Session_CorrelatesConcurrentResponsesThatArriveOutOfOrder()
    {
        using var socket = new ScriptedWebSocket();
        using var session = new OneBot11WebSocketSession(socket, new OneBot11EventDispatcher());
        var requests = new List<JsonObject>();
        var requestGate = new object();
        socket.TextSent = requestText =>
        {
            lock (requestGate)
            {
                requests.Add(JsonNode.Parse(requestText)!.AsObject());
                if (requests.Count != 2)
                {
                    return;
                }

                EnqueueResponse(socket, requests[1]);
                EnqueueResponse(socket, requests[0]);
            }
        };
        await session.StartAsync(CancellationToken.None);
        using var timeout = new CancellationTokenSource();
        timeout.CancelAfter(TimeSpan.FromSeconds(3));

        var first = session.SendAsync("first_action", null, null, timeout.Token);
        var second = session.SendAsync("second_action", null, null, timeout.Token);
        var responses = await Task.WhenAll(first, second);

        Assert.Equal("first_action", responses[0].Response["data"]!["action"]!.GetValue<string>());
        Assert.Equal("second_action", responses[1].Response["data"]!["action"]!.GetValue<string>());
    }

    private static void EnqueueResponse(ScriptedWebSocket socket, JsonObject request)
    {
        socket.EnqueueText(OneBot11Json.Serialize(new JsonObject
        {
            ["status"] = "ok",
            ["retcode"] = 0,
            ["data"] = new JsonObject { ["action"] = request["action"]!.GetValue<string>() },
            ["echo"] = Clone(request["echo"])
        }));
    }

    private static async Task<T> WithTimeout<T>(Task<T> task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(3))).ConfigureAwait(false);
        Assert.Same(task, completed);
        return await task.ConfigureAwait(false);
    }

    private static JsonNode? Clone(JsonNode? node)
    {
        return node == null ? null : OneBot11Json.Parse(OneBot11Json.Serialize(node));
    }

    private sealed class ScriptedWebSocket : WebSocket
    {
        private readonly ConcurrentQueue<Frame> _frames = new ConcurrentQueue<Frame>();
        private readonly SemaphoreSlim _available = new SemaphoreSlim(0);
        private WebSocketState _state = WebSocketState.Open;
        private WebSocketCloseStatus? _closeStatus;
        private string? _closeStatusDescription;

        internal Action<string>? TextSent { get; set; }

        public override WebSocketCloseStatus? CloseStatus => _closeStatus;

        public override string? CloseStatusDescription => _closeStatusDescription;

        public override WebSocketState State => _state;

        public override string? SubProtocol => null;

        internal void EnqueueText(string text, int splitAt = -1)
        {
            var payload = Encoding.UTF8.GetBytes(text);
            if (splitAt > 0 && splitAt < payload.Length)
            {
                Enqueue(new Frame(Slice(payload, 0, splitAt), WebSocketMessageType.Text, false));
                Enqueue(new Frame(Slice(payload, splitAt, payload.Length - splitAt), WebSocketMessageType.Text, true));
                return;
            }

            Enqueue(new Frame(payload, WebSocketMessageType.Text, true));
        }

        public override void Abort()
        {
            _state = WebSocketState.Aborted;
        }

        public override Task CloseAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _closeStatus = closeStatus;
            _closeStatusDescription = statusDescription;
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _closeStatus = closeStatus;
            _closeStatusDescription = statusDescription;
            _state = WebSocketState.CloseSent;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _state = WebSocketState.Closed;
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken)
        {
            await _available.WaitAsync(cancellationToken).ConfigureAwait(false);
            Frame? frame;
            if (!_frames.TryDequeue(out frame) || frame == null)
            {
                throw new InvalidOperationException("No scripted WebSocket frame was available.");
            }

            if (frame.Payload.Length > buffer.Count)
            {
                throw new InvalidOperationException("The scripted frame is larger than the receive buffer.");
            }

            Buffer.BlockCopy(frame.Payload, 0, buffer.Array!, buffer.Offset, frame.Payload.Length);
            return new WebSocketReceiveResult(frame.Payload.Length, frame.MessageType, frame.EndOfMessage);
        }

        public override Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.Equal(WebSocketMessageType.Text, messageType);
            Assert.True(endOfMessage);
            TextSent?.Invoke(Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));
            return Task.CompletedTask;
        }

        private void Enqueue(Frame frame)
        {
            _frames.Enqueue(frame);
            _available.Release();
        }

        private static byte[] Slice(byte[] source, int offset, int count)
        {
            var result = new byte[count];
            Buffer.BlockCopy(source, offset, result, 0, count);
            return result;
        }

        private sealed class Frame
        {
            internal Frame(byte[] payload, WebSocketMessageType messageType, bool endOfMessage)
            {
                Payload = payload;
                MessageType = messageType;
                EndOfMessage = endOfMessage;
            }

            internal byte[] Payload { get; }
            internal WebSocketMessageType MessageType { get; }
            internal bool EndOfMessage { get; }
        }
    }
}
