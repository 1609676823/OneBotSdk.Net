using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V12;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Transports.WebSockets;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class OneBot12WebSocketSessionTests
{
    [Fact]
    public async Task Session_DispatchesEventsAndCorrelatesActionResponsesOnTheSameConnection()
    {
        using var socket = new ScriptedWebSocket();
        var dispatcher = new OneBot12EventDispatcher();
        var receivedEvent = new TaskCompletionSource<OneBot12Event>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = dispatcher.Events.Subscribe(value => receivedEvent.TrySetResult(value));
        using var session = new OneBot12WebSocketSession(socket, dispatcher);
        JsonObject? sentEnvelope = null;

        socket.TextSent = requestText =>
        {
            sentEnvelope = JsonNode.Parse(requestText)!.AsObject();

            // One forward connection can interleave an event before the matching action response.
            // 同一正向连接可在匹配动作响应前穿插推送事件。
            socket.EnqueueText(
                "{\"id\":\"event-1\",\"time\":1.5,\"type\":\"message\",\"detail_type\":\"group\",\"sub_type\":\"normal\"," +
                "\"self\":{\"platform\":\"qq\",\"user_id\":\"123xxxxxxx\"},\"message_id\":\"message-1\"," +
                "\"group_id\":\"782351597\",\"user_id\":\"10001\",\"message\":[{\"type\":\"text\",\"data\":{\"text\":\"hello\"}}]," +
                "\"alt_message\":\"hello\"}",
                31);
            socket.EnqueueText(OneBot12Json.Serialize(new JsonObject
            {
                ["status"] = "ok",
                ["retcode"] = 0,
                ["data"] = new JsonObject { ["nickname"] = "tester" },
                ["message"] = "",
                ["echo"] = sentEnvelope["echo"]!.GetValue<string>()
            }));
        };

        await session.StartAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var response = await session.SendAsync(
            "get_user_info",
            new JsonObject { ["user_id"] = "10001" },
            "echo-1",
            new OneBot12Self("qq", "123xxxxxxx"),
            timeout.Token);
        var parsedEvent = Assert.IsType<GroupMessageEvent>(await WithTimeout(receivedEvent.Task));

        Assert.Equal("782351597", parsedEvent.GroupId);
        Assert.Equal("hello", parsedEvent.Message?.PlainText);
        Assert.Equal("tester", response.Response["data"]!["nickname"]!.GetValue<string>());
        Assert.Equal("echo-1", response.RequestEcho);
        Assert.Equal("qq", response.RequestSelf?.Platform);
        Assert.Equal("get_user_info", sentEnvelope!["action"]!.GetValue<string>());
        Assert.Equal("123xxxxxxx", sentEnvelope["self"]!["user_id"]!.GetValue<string>());
        Assert.NotNull(response.RawRequestJson);
        Assert.NotNull(response.RawResponseJson);
    }

    [Fact]
    public async Task Session_RejectsStartingASecondReceiveLoop()
    {
        using var socket = new ScriptedWebSocket();
        using var session = new OneBot12WebSocketSession(socket, new OneBot12EventDispatcher());

        await session.StartAsync();

        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = session.StartAsync();
        });
    }

    private static async Task<T> WithTimeout<T>(Task<T> task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(3))).ConfigureAwait(false);
        Assert.Same(task, completed);
        return await task.ConfigureAwait(false);
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
                Enqueue(new Frame(Slice(payload, 0, splitAt), false));
                Enqueue(new Frame(Slice(payload, splitAt, payload.Length - splitAt), true));
                return;
            }

            Enqueue(new Frame(payload, true));
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

            Buffer.BlockCopy(frame.Payload, 0, buffer.Array!, buffer.Offset, frame.Payload.Length);
            return new WebSocketReceiveResult(
                frame.Payload.Length,
                WebSocketMessageType.Text,
                frame.EndOfMessage);
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
            internal Frame(byte[] payload, bool endOfMessage)
            {
                Payload = payload;
                EndOfMessage = endOfMessage;
            }

            internal byte[] Payload { get; }

            internal bool EndOfMessage { get; }
        }
    }
}
