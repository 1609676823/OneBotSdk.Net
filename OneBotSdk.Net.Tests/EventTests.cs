using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class EventTests
{
    [Fact]
    public void MessageEvent_ExposesOneStronglyTypedMessageChainEntryPoint()
    {
        var source = JsonNode.Parse("""
            {
              "post_type": "message",
              "message_type": "group",
              "message": [
                { "type": "text", "data": { "text": "first" } },
                { "type": "image", "data": { "file": "received.jpg", "url": "https://example.test/received.jpg" } },
                { "type": "text", "data": { "text": "second" } }
              ]
            }
            """)!.AsObject();

        var parsed = Assert.IsType<GroupMessageEvent>(OneBot11EventParser.Parse(source));
        var texts = parsed.MessageChain.OfType<TextReceivedSegment>().ToArray();
        var image = Assert.Single(parsed.MessageChain.OfType<ImageReceivedSegment>());

        Assert.Equal(new[] { "first", "second" }, texts.Select(text => text.Text));
        Assert.Equal("https://example.test/received.jpg", image.Url);
        Assert.Equal(typeof(OneBot11ReceivedMessage), typeof(OneBot11MessageEvent).GetProperty(nameof(parsed.MessageChain))!.PropertyType);
        Assert.Null(typeof(OneBot11MessageEvent).GetProperty("Message"));
    }

    [Fact]
    public void MessageEvent_UsesAnEmptyChainWhenTheWireMessageIsMissing()
    {
        var source = JsonNode.Parse("""
            {
              "post_type": "message",
              "message_type": "private"
            }
            """)!.AsObject();

        var parsed = Assert.IsType<PrivateMessageEvent>(OneBot11EventParser.Parse(source));

        Assert.Empty(parsed.MessageChain);
        Assert.Equal(OneBot11ReceivedMessageKind.SegmentArray, parsed.MessageChain.Kind);
    }

    [Fact]
    public void GroupMessageParser_IsolatesNestedDriftAndPreservesRawJson()
    {
        var source = JsonNode.Parse("""
            {
              "time": 1710000000,
              "self_id": 10000,
              "post_type": "message",
              "message_type": "group",
              "sub_type": "normal",
              "message_id": 99,
              "group_id": "20000",
              "user_id": "30000",
              "anonymous": null,
              "message": [
                { "type": "text", "data": { "text": "hello" } },
                { "type": "vendor_extension", "data": null }
              ],
              "raw_message": "hello",
              "font": { "malformed": true },
              "sender": {
                "user_id": 30000,
                "nickname": "tester",
                "age": { "malformed": true },
                "role": "member",
                "vendor_badge": "kept"
              },
              "vendor_event_field": 42
            }
            """)!.AsObject();

        var parsed = Assert.IsType<GroupMessageEvent>(OneBot11EventParser.Parse(source));

        Assert.Equal(20000L, parsed.GroupId);
        Assert.Equal(30000L, parsed.UserId);
        Assert.Null(parsed.Font);
        Assert.Null(parsed.Anonymous);
        Assert.Equal(2, parsed.MessageChain.Segments.Count);
        Assert.Equal("hello", Assert.IsType<OneBotSdk.Net.V11.Messages.TextReceivedSegment>(parsed.MessageChain[0]).Text);
        Assert.Equal("vendor_extension", parsed.MessageChain.Segments[1].Type);
        Assert.IsType<OneBotSdk.Net.V11.Messages.UnknownReceivedSegment>(parsed.MessageChain[1]);
        Assert.Null(parsed.Sender!.Age);
        Assert.Equal("kept", parsed.Sender.RawJson["vendor_badge"]!.GetValue<string>());
        Assert.Equal(42, parsed.RawJson["vendor_event_field"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("message", "message_type", "private", typeof(PrivateMessageEvent))]
    [InlineData("message", "message_type", "group", typeof(GroupMessageEvent))]
    [InlineData("request", "request_type", "group", typeof(GroupRequestEvent))]
    [InlineData("request", "request_type", "friend", typeof(FriendRequestEvent))]
    [InlineData("notice", "notice_type", "group_increase", typeof(GroupIncreaseNoticeEvent))]
    [InlineData("meta_event", "meta_event_type", "lifecycle", typeof(LifecycleMetaEvent))]
    [InlineData("meta_event", "meta_event_type", "heartbeat", typeof(HeartbeatMetaEvent))]
    [InlineData("message", "message_type", "vendor_message", typeof(UnknownMessageEvent))]
    [InlineData("request", "request_type", "vendor_request", typeof(UnknownRequestEvent))]
    [InlineData("notice", "notice_type", "vendor_notice", typeof(UnknownNoticeEvent))]
    [InlineData("meta_event", "meta_event_type", "vendor_meta", typeof(UnknownMetaEvent))]
    [InlineData("vendor_post", "vendor_type", "vendor_event", typeof(UnknownOneBot11Event))]
    public void EventParser_PreservesRawJsonForAllEventCategories(string postType, string discriminatorName, string discriminatorValue, Type expectedType)
    {
        var source = new JsonObject
        {
            ["time"] = 1710000000,
            ["self_id"] = 10000,
            ["post_type"] = postType,
            [discriminatorName] = discriminatorValue,
            ["vendor_payload"] = new JsonObject
            {
                ["trace_id"] = "raw-event-trace",
                ["sequence"] = 7
            }
        };
        string originalJson = OneBot11Json.Serialize(source);

        OneBot11Event parsed = OneBot11EventParser.Parse(source);

        Assert.Equal(expectedType, parsed.GetType());
        Assert.NotSame(source, parsed.RawJson);
        Assert.Equal(originalJson, OneBot11Json.Serialize(parsed.RawJson));
        Assert.Equal("raw-event-trace", parsed.RawJson["vendor_payload"]!["trace_id"]!.GetValue<string>());
    }

    [Fact]
    public void HeartbeatParser_PreservesUnknownOnlineAndStatusExtensions()
    {
        var source = JsonNode.Parse("""
            {
              "time": 1710000000,
              "self_id": 10000,
              "post_type": "meta_event",
              "meta_event_type": "heartbeat",
              "status": {
                "online": null,
                "good": "yes",
                "vendor_latency": 12
              },
              "interval": "15000"
            }
            """)!.AsObject();

        var parsed = Assert.IsType<HeartbeatMetaEvent>(OneBot11EventParser.Parse(source));

        Assert.Null(parsed.Status!.Online);
        Assert.True(parsed.Status.Good);
        Assert.Equal(12, parsed.Status.RawJson["vendor_latency"]!.GetValue<int>());
        Assert.Equal(15000L, parsed.Interval);
    }

    [Fact]
    public void UnknownNotifySubtype_FallsBackWithoutDroppingTheEvent()
    {
        var source = JsonNode.Parse("""
            {
              "post_type": "notice",
              "notice_type": "notify",
              "sub_type": "vendor_notify",
              "payload": { "usable": true }
            }
            """)!.AsObject();

        var parsed = Assert.IsType<UnknownNoticeEvent>(OneBot11EventParser.Parse(source));

        Assert.Equal("notify", parsed.NoticeType);
        Assert.Equal("vendor_notify", parsed.SubType);
        Assert.True(parsed.RawJson["payload"]!["usable"]!.GetValue<bool>());
    }

    [Fact]
    public void Dispatcher_RaisesGeneralCategoryAndUnknownNotifications()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var generalCount = 0;
        var noticeCount = 0;
        var unknownCount = 0;
        using var generalSubscription = dispatcher.Events.Subscribe(_ => generalCount++);
        using var noticeSubscription = dispatcher.Notices.Subscribe(_ => noticeCount++);
        using var unknownSubscription = dispatcher.UnknownEvents.Subscribe(_ => unknownCount++);
        var parsed = OneBot11EventParser.Parse(new JsonObject
        {
            ["post_type"] = "notice",
            ["notice_type"] = "vendor_notice"
        });

        dispatcher.Dispatch(parsed);

        Assert.Equal(1, generalCount);
        Assert.Equal(1, noticeCount);
        Assert.Equal(1, unknownCount);
    }

    [Fact]
    public void Dispatcher_EventHandlerPatternProvidesSenderAndConcreteMessageType()
    {
        var dispatcher = new OneBot11EventDispatcher();
        object? sender = null;
        OneBot11Event? general = null;
        OneBot11MessageEvent? category = null;
        GroupMessageEvent? group = null;
        var privateCount = 0;

        dispatcher.EventDispatched += (value, args) =>
        {
            sender = value;
            general = args.Event;
        };
        dispatcher.MessageDispatched += (_, args) => category = args.Event;
        dispatcher.GroupMessageReceived += (_, args) => group = args.Event;
        dispatcher.PrivateMessageReceived += (_, _) => privateCount++;

        var parsed = ParseGroupMessage(20000, 30000, "/ping");
        dispatcher.Dispatch(parsed);

        Assert.Same(dispatcher, sender);
        Assert.Same(parsed, general);
        Assert.Same(parsed, category);
        Assert.Same(parsed, group);
        Assert.Equal(0, privateCount);
    }

    [Fact]
    public void Dispatcher_PrivateMessageReachesEventHandlerAndObservableStream()
    {
        var dispatcher = new OneBot11EventDispatcher();
        PrivateMessageEvent? eventHandlerValue = null;
        PrivateMessageEvent? observableValue = null;
        dispatcher.PrivateMessageReceived += (_, args) => eventHandlerValue = args.Event;
        using var subscription = dispatcher.PrivateMessages.Subscribe(value => observableValue = value);
        var parsed = ParsePrivateMessage(30000, "/ping");

        dispatcher.Dispatch(parsed);

        Assert.Same(parsed, eventHandlerValue);
        Assert.Same(parsed, observableValue);
    }

    [Fact]
    public void Dispatcher_PublicEventsUseOnlyTheStandardEventHandlerPattern()
    {
        var publicEvents = typeof(OneBot11EventDispatcher).GetEvents();

        Assert.NotEmpty(publicEvents);
        Assert.All(publicEvents, eventInfo =>
        {
            Assert.NotNull(eventInfo.EventHandlerType);
            Assert.True(eventInfo.EventHandlerType!.IsGenericType);
            Assert.Equal(typeof(EventHandler<>), eventInfo.EventHandlerType.GetGenericTypeDefinition());
        });
    }

    [Fact]
    public void Dispatcher_ObservableStreamsFilterTypesAndUnsubscribeIdempotently()
    {
        var dispatcher = new OneBot11EventDispatcher();
        IObservable<GroupMessageEvent> groupMessages = dispatcher.GroupMessages;
        var observer = new RecordingObserver<GroupMessageEvent>();
        var subscription = groupMessages.Subscribe(observer);

        dispatcher.Dispatch(ParsePrivateMessage(30000, "/ping"));
        dispatcher.Dispatch(ParseGroupMessage(20000, 30000, "/ping"));
        subscription.Dispose();
        subscription.Dispose();
        dispatcher.Dispatch(ParseGroupMessage(20000, 30001, "/ping"));

        Assert.Single(observer.Values);
        Assert.Equal(30000L, observer.Values[0].UserId);
    }

    [Fact]
    public void Dispatcher_ObservableBaseAndUnknownStreamsReceiveUnknownEvent()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var allObserver = new RecordingObserver<OneBot11Event>();
        var unknownObserver = new RecordingObserver<OneBot11Event>();
        using (dispatcher.Subscribe(allObserver))
        using (dispatcher.UnknownEvents.Subscribe(unknownObserver))
        {
            var parsed = OneBot11EventParser.Parse(new JsonObject
            {
                ["post_type"] = "vendor_event"
            });

            dispatcher.Dispatch(parsed);

            Assert.Same(parsed, Assert.Single(allObserver.Values));
            Assert.Same(parsed, Assert.Single(unknownObserver.Values));
        }
    }

    [Fact]
    public void Dispatcher_IsolatesFailingEventHandlerAndObserverSubscribers()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var categoryHandlerCount = 0;
        var eventHandlerCount = 0;
        var observer = new RecordingObserver<GroupMessageEvent>();

        dispatcher.MessageDispatched += (_, _) => throw new InvalidOperationException("broken category event handler");
        dispatcher.MessageDispatched += (_, _) => categoryHandlerCount++;
        dispatcher.GroupMessageReceived += (_, _) => throw new InvalidOperationException("broken event handler");
        dispatcher.GroupMessageReceived += (_, _) => eventHandlerCount++;
        using var failingSubscription = dispatcher.GroupMessages.Subscribe(new ThrowingObserver<GroupMessageEvent>());
        using var recordingSubscription = dispatcher.GroupMessages.Subscribe(observer);

        dispatcher.Dispatch(ParseGroupMessage(20000, 30000, "/ping"));

        Assert.Equal(1, categoryHandlerCount);
        Assert.Equal(1, eventHandlerCount);
        Assert.Single(observer.Values);
    }

    [Fact]
    public void Dispatcher_ObservableRejectsNullObserver()
    {
        var dispatcher = new OneBot11EventDispatcher();

        Assert.Throws<ArgumentNullException>(() => dispatcher.Subscribe(null!));
        Assert.Throws<ArgumentNullException>(() => dispatcher.GroupMessages.Subscribe(null!));
    }

    [Fact]
    public void Dispatcher_SubscriptionsUseRegistrationIdentityInsteadOfObserverEquality()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var first = new EqualRecordingObserver<GroupMessageEvent>();
        var second = new EqualRecordingObserver<GroupMessageEvent>();
        using (dispatcher.GroupMessages.Subscribe(first))
        {
            var secondSubscription = dispatcher.GroupMessages.Subscribe(second);
            secondSubscription.Dispose();

            dispatcher.Dispatch(ParseGroupMessage(20000, 30000, "/ping"));

            Assert.Single(first.Values);
            Assert.Empty(second.Values);
        }
    }

    [Fact]
    public async Task Dispatcher_SerializesConcurrentDispatchForEachObserver()
    {
        const int dispatchCount = 8;
        var dispatcher = new OneBot11EventDispatcher();
        var observer = new ConcurrencyObserver<GroupMessageEvent>();
        var parsed = ParseGroupMessage(20000, 30000, "/ping");
        using (dispatcher.GroupMessages.Subscribe(observer))
        using (var start = new ManualResetEventSlim(false))
        {
            var tasks = new Task[dispatchCount];
            for (var index = 0; index < tasks.Length; index++)
            {
                tasks[index] = Task.Run(() =>
                {
                    start.Wait();
                    dispatcher.Dispatch(parsed);
                });
            }

            start.Set();
            await Task.WhenAll(tasks);
        }

        Assert.Equal(dispatchCount, observer.Count);
        Assert.Equal(1, observer.MaxConcurrentCalls);
    }

    [Fact]
    public void Dispatcher_QueuesReentrantDispatchUntilCurrentNotificationReturns()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var notifications = new List<string>();
        var firstMessage = ParseGroupMessage(20000, 30000, "first");
        var secondMessage = ParseGroupMessage(20000, 30001, "second");

        using (dispatcher.GroupMessages.Subscribe(
                   new CallbackObserver<GroupMessageEvent>(value =>
                   {
                       notifications.Add("observer-1:" + value.RawMessage);
                       if (ReferenceEquals(value, firstMessage))
                       {
                           dispatcher.Dispatch(secondMessage);
                           notifications.Add("observer-1:first-returning");
                       }
                   })))
        using (dispatcher.GroupMessages.Subscribe(
                   new CallbackObserver<GroupMessageEvent>(value =>
                       notifications.Add("observer-2:" + value.RawMessage))))
        {
            dispatcher.Dispatch(firstMessage);
        }

        Assert.Equal(
            new[]
            {
                "observer-1:first",
                "observer-1:first-returning",
                "observer-2:first",
                "observer-1:second",
                "observer-2:second"
            },
            notifications);
    }

    [Fact]
    public void Dispatcher_RoutesEveryConcreteNoticeRequestAndMetaSubscription()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var handledTypes = new List<Type>();
        dispatcher.GroupUploadNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupUploadNoticeEvent));
        dispatcher.GroupAdminNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupAdminNoticeEvent));
        dispatcher.GroupDecreaseNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupDecreaseNoticeEvent));
        dispatcher.GroupIncreaseNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupIncreaseNoticeEvent));
        dispatcher.GroupBanNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupBanNoticeEvent));
        dispatcher.FriendAddNoticeReceived += (_, _) => handledTypes.Add(typeof(FriendAddNoticeEvent));
        dispatcher.GroupRecallNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupRecallNoticeEvent));
        dispatcher.FriendRecallNoticeReceived += (_, _) => handledTypes.Add(typeof(FriendRecallNoticeEvent));
        dispatcher.GroupPokeNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupPokeNoticeEvent));
        dispatcher.LuckyKingNoticeReceived += (_, _) => handledTypes.Add(typeof(LuckyKingNoticeEvent));
        dispatcher.GroupHonorNoticeReceived += (_, _) => handledTypes.Add(typeof(GroupHonorNoticeEvent));
        dispatcher.FriendRequestReceived += (_, _) => handledTypes.Add(typeof(FriendRequestEvent));
        dispatcher.GroupRequestReceived += (_, _) => handledTypes.Add(typeof(GroupRequestEvent));
        dispatcher.LifecycleMetaEventReceived += (_, _) => handledTypes.Add(typeof(LifecycleMetaEvent));
        dispatcher.HeartbeatMetaEventReceived += (_, _) => handledTypes.Add(typeof(HeartbeatMetaEvent));

        var groupUpload = new RecordingObserver<GroupUploadNoticeEvent>();
        var groupAdmin = new RecordingObserver<GroupAdminNoticeEvent>();
        var groupDecrease = new RecordingObserver<GroupDecreaseNoticeEvent>();
        var groupIncrease = new RecordingObserver<GroupIncreaseNoticeEvent>();
        var groupBan = new RecordingObserver<GroupBanNoticeEvent>();
        var friendAdd = new RecordingObserver<FriendAddNoticeEvent>();
        var groupRecall = new RecordingObserver<GroupRecallNoticeEvent>();
        var friendRecall = new RecordingObserver<FriendRecallNoticeEvent>();
        var groupPoke = new RecordingObserver<GroupPokeNoticeEvent>();
        var luckyKing = new RecordingObserver<LuckyKingNoticeEvent>();
        var groupHonor = new RecordingObserver<GroupHonorNoticeEvent>();
        var friendRequest = new RecordingObserver<FriendRequestEvent>();
        var groupRequest = new RecordingObserver<GroupRequestEvent>();
        var lifecycle = new RecordingObserver<LifecycleMetaEvent>();
        var heartbeat = new RecordingObserver<HeartbeatMetaEvent>();

        var subscriptions = new List<IDisposable>
        {
            dispatcher.GroupUploadNotices.Subscribe(groupUpload),
            dispatcher.GroupAdminNotices.Subscribe(groupAdmin),
            dispatcher.GroupDecreaseNotices.Subscribe(groupDecrease),
            dispatcher.GroupIncreaseNotices.Subscribe(groupIncrease),
            dispatcher.GroupBanNotices.Subscribe(groupBan),
            dispatcher.FriendAddNotices.Subscribe(friendAdd),
            dispatcher.GroupRecallNotices.Subscribe(groupRecall),
            dispatcher.FriendRecallNotices.Subscribe(friendRecall),
            dispatcher.GroupPokeNotices.Subscribe(groupPoke),
            dispatcher.LuckyKingNotices.Subscribe(luckyKing),
            dispatcher.GroupHonorNotices.Subscribe(groupHonor),
            dispatcher.FriendRequests.Subscribe(friendRequest),
            dispatcher.GroupRequests.Subscribe(groupRequest),
            dispatcher.LifecycleEvents.Subscribe(lifecycle),
            dispatcher.Heartbeats.Subscribe(heartbeat)
        };

        try
        {
            DispatchEvent(dispatcher, "notice", "notice_type", "group_upload");
            DispatchEvent(dispatcher, "notice", "notice_type", "group_admin");
            DispatchEvent(dispatcher, "notice", "notice_type", "group_decrease");
            DispatchEvent(dispatcher, "notice", "notice_type", "group_increase");
            DispatchEvent(dispatcher, "notice", "notice_type", "group_ban");
            DispatchEvent(dispatcher, "notice", "notice_type", "friend_add");
            DispatchEvent(dispatcher, "notice", "notice_type", "group_recall");
            DispatchEvent(dispatcher, "notice", "notice_type", "friend_recall");
            DispatchNotifyEvent(dispatcher, "poke");
            DispatchNotifyEvent(dispatcher, "lucky_king");
            DispatchNotifyEvent(dispatcher, "honor");
            DispatchEvent(dispatcher, "request", "request_type", "friend");
            DispatchEvent(dispatcher, "request", "request_type", "group");
            DispatchEvent(dispatcher, "meta_event", "meta_event_type", "lifecycle");
            DispatchEvent(dispatcher, "meta_event", "meta_event_type", "heartbeat");
        }
        finally
        {
            foreach (var subscription in subscriptions)
            {
                subscription.Dispose();
            }
        }

        Assert.Single(groupUpload.Values);
        Assert.Single(groupAdmin.Values);
        Assert.Single(groupDecrease.Values);
        Assert.Single(groupIncrease.Values);
        Assert.Single(groupBan.Values);
        Assert.Single(friendAdd.Values);
        Assert.Single(groupRecall.Values);
        Assert.Single(friendRecall.Values);
        Assert.Single(groupPoke.Values);
        Assert.Single(luckyKing.Values);
        Assert.Single(groupHonor.Values);
        Assert.Single(friendRequest.Values);
        Assert.Single(groupRequest.Values);
        Assert.Single(lifecycle.Values);
        Assert.Single(heartbeat.Values);
        Assert.Equal(15, handledTypes.Count);
        Assert.Equal(15, new HashSet<Type>(handledTypes).Count);
    }

    [Fact]
    public void ObservableExtensions_FilterConcreteEventsAndSupportLambdaSubscriptions()
    {
        var dispatcher = new OneBot11EventDispatcher();
        GroupBanNoticeEvent? received = null;
        using (dispatcher.Notices
                   .OfType<GroupBanNoticeEvent>()
                   .Subscribe(value => received = value))
        {
            DispatchEvent(dispatcher, "notice", "notice_type", "friend_add");
            DispatchEvent(dispatcher, "notice", "notice_type", "group_ban");
        }

        Assert.NotNull(received);
        Assert.Equal("group_ban", received!.NoticeType);
    }

    [Fact]
    public async Task ObservableExtensions_SubscribeAsyncObservesPostAwaitFailures()
    {
        var dispatcher = new OneBot11EventDispatcher();
        var failure = new TaskCompletionSource<Exception>();
        using (dispatcher.GroupMessages.SubscribeAsync(
                   async _ =>
                   {
                       await Task.Yield();
                       throw new InvalidOperationException("async observer failed");
                   },
                   exception => failure.TrySetResult(exception)))
        {
            dispatcher.Dispatch(ParseGroupMessage(20000, 30000, "/ping"));

            var completed = await Task.WhenAny(failure.Task, Task.Delay(TimeSpan.FromSeconds(5)));
            Assert.Same(failure.Task, completed);
            Assert.IsType<InvalidOperationException>(await failure.Task);
        }
    }

    [Fact]
    public void RequestQuickOperation_DistinguishesMissingApprovalFromExplicitRejection()
    {
        var unhandled = JsonSerializer.Serialize(new FriendRequestQuickOperation());
        var rejected = JsonSerializer.Serialize(new FriendRequestQuickOperation { Approve = false });

        Assert.DoesNotContain("approve", unhandled);
        Assert.False(JsonNode.Parse(rejected)!["approve"]!.GetValue<bool>());
    }

    [Fact]
    public void MessageQuickOperation_SerializesOutgoingOnlyReplyModel()
    {
        var operation = new GroupMessageQuickOperation
        {
            Reply = new OneBotSdk.Net.V11.Messages.OneBot11SendMessage
            {
                new OneBotSdk.Net.V11.Messages.TextSendSegment("reply")
            },
            AtSender = true
        };

        var json = JsonNode.Parse(JsonSerializer.Serialize(operation))!.AsObject();

        Assert.Equal("text", json["reply"]![0]!["type"]!.GetValue<string>());
        Assert.True(json["at_sender"]!.GetValue<bool>());
    }

    private static GroupMessageEvent ParseGroupMessage(long groupId, long userId, string message)
    {
        return Assert.IsType<GroupMessageEvent>(OneBot11EventParser.Parse(new JsonObject
        {
            ["post_type"] = "message",
            ["message_type"] = "group",
            ["sub_type"] = "normal",
            ["self_id"] = 10000,
            ["group_id"] = groupId,
            ["user_id"] = userId,
            ["message"] = message,
            ["raw_message"] = message
        }));
    }

    private static PrivateMessageEvent ParsePrivateMessage(long userId, string message)
    {
        return Assert.IsType<PrivateMessageEvent>(OneBot11EventParser.Parse(new JsonObject
        {
            ["post_type"] = "message",
            ["message_type"] = "private",
            ["sub_type"] = "friend",
            ["self_id"] = 10000,
            ["user_id"] = userId,
            ["message"] = message,
            ["raw_message"] = message
        }));
    }

    private static void DispatchEvent(
        OneBot11EventDispatcher dispatcher,
        string postType,
        string discriminatorName,
        string discriminatorValue)
    {
        dispatcher.Dispatch(OneBot11EventParser.Parse(new JsonObject
        {
            ["post_type"] = postType,
            [discriminatorName] = discriminatorValue
        }));
    }

    private static void DispatchNotifyEvent(OneBot11EventDispatcher dispatcher, string subType)
    {
        dispatcher.Dispatch(OneBot11EventParser.Parse(new JsonObject
        {
            ["post_type"] = "notice",
            ["notice_type"] = "notify",
            ["sub_type"] = subType
        }));
    }

    private sealed class RecordingObserver<T> : IObserver<T>
    {
        internal List<T> Values { get; } = new List<T>();

        public void OnNext(T value)
        {
            Values.Add(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }

    private sealed class ThrowingObserver<T> : IObserver<T>
    {
        public void OnNext(T value)
        {
            throw new InvalidOperationException("broken observer");
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }

    private sealed class EqualRecordingObserver<T> : IObserver<T>
    {
        internal List<T> Values { get; } = new List<T>();

        public void OnNext(T value)
        {
            Values.Add(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public override bool Equals(object? obj)
        {
            return obj is EqualRecordingObserver<T>;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    private sealed class ConcurrencyObserver<T> : IObserver<T>
    {
        private int _activeCalls;
        private int _count;
        private int _maxConcurrentCalls;

        internal int Count => Volatile.Read(ref _count);

        internal int MaxConcurrentCalls => Volatile.Read(ref _maxConcurrentCalls);

        public void OnNext(T value)
        {
            var activeCalls = Interlocked.Increment(ref _activeCalls);
            UpdateMaximum(activeCalls);
            Thread.Sleep(4);
            Interlocked.Increment(ref _count);
            Interlocked.Decrement(ref _activeCalls);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void UpdateMaximum(int candidate)
        {
            while (true)
            {
                var current = Volatile.Read(ref _maxConcurrentCalls);
                if (candidate <= current ||
                    Interlocked.CompareExchange(ref _maxConcurrentCalls, candidate, current) == current)
                {
                    return;
                }
            }
        }
    }

    private sealed class CallbackObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;

        internal CallbackObserver(Action<T> onNext)
        {
            _onNext = onNext;
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}
