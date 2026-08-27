# OneBot 11 usage guide

[README](../README.md) | [Documentation index](README.md) | [简体中文](onebot-11.zh-CN.md)

> **Official OneBot 11 references:** [repository](https://github.com/botuniverse/onebot-11) · [public Action API](https://github.com/botuniverse/onebot-11/blob/master/api/public.md) · [event documentation](https://github.com/botuniverse/onebot-11/tree/master/event)

The OneBot 11 API lives under `OneBotSdk.Net.V11.*`. It currently covers 38 public actions, 17 standard concrete event classes plus five concrete unknown-fallback types, and 20 standard message-segment types. This is usually the preferred version for QQ implementations that explicitly support OneBot 11.

## Reference index

- [The two endpoint parameters](#the-two-endpoint-parameters)
- [Actions](#actions)
- [Receiving events](#receiving-events)

### Action methods at a glance

| Category | Methods |
| --- | --- |
| Messages | [`SendPrivateMessageAsync`](#sendprivatemessageasync), [`SendGroupMessageAsync`](#sendgroupmessageasync), [`SendMessageAsync`](#sendmessageasync), [`DeleteMessageAsync`](#deletemessageasync), [`GetMessageAsync`](#getmessageasync), [`GetForwardMessageAsync`](#getforwardmessageasync), [`SendLikeAsync`](#sendlikeasync) |
| Group management | [`SetGroupKickAsync`](#setgroupkickasync), [`SetGroupBanAsync`](#setgroupbanasync), [`SetGroupAnonymousBanAsync`](#setgroupanonymousbanasync), [`SetGroupWholeBanAsync`](#setgroupwholebanasync), [`SetGroupAdminAsync`](#setgroupadminasync), [`SetGroupAnonymousAsync`](#setgroupanonymousasync), [`SetGroupCardAsync`](#setgroupcardasync), [`SetGroupNameAsync`](#setgroupnameasync), [`SetGroupLeaveAsync`](#setgroupleaveasync), [`SetGroupSpecialTitleAsync`](#setgroupspecialtitleasync) |
| Requests | [`SetFriendAddRequestAsync`](#setfriendaddrequestasync), [`SetGroupAddRequestAsync`](#setgroupaddrequestasync) |
| Information | [`GetLoginInfoAsync`](#getlogininfoasync), [`GetStrangerInfoAsync`](#getstrangerinfoasync), [`GetFriendListAsync`](#getfriendlistasync), [`GetGroupInfoAsync`](#getgroupinfoasync), [`GetGroupListAsync`](#getgrouplistasync), [`GetGroupMemberInfoAsync`](#getgroupmemberinfoasync), [`GetGroupMemberListAsync`](#getgroupmemberlistasync), [`GetGroupHonorInfoAsync`](#getgrouphonorinfoasync) |
| Files, capabilities, runtime | [`GetCookiesAsync`](#getcookiesasync), [`GetCsrfTokenAsync`](#getcsrftokenasync), [`GetCredentialsAsync`](#getcredentialsasync), [`GetRecordAsync`](#getrecordasync), [`GetImageAsync`](#getimageasync), [`CanSendImageAsync`](#cansendimageasync), [`CanSendRecordAsync`](#cansendrecordasync), [`GetStatusAsync`](#getstatusasync), [`GetVersionInfoAsync`](#getversioninfoasync), [`SetRestartAsync`](#setrestartasync), [`CleanCacheAsync`](#cleancacheasync) |
| Advanced | [`CallActionAsync`](#callactionasync), [`HandleQuickOperationAsync`](#handlequickoperationasync) |

### Concrete received events at a glance

| Category | Event types |
| --- | --- |
| Messages | [`PrivateMessageEvent`](#privatemessageevent), [`GroupMessageEvent`](#groupmessageevent) |
| Notices | [`GroupUploadNoticeEvent`](#groupuploadnoticeevent), [`GroupAdminNoticeEvent`](#groupadminnoticeevent), [`GroupDecreaseNoticeEvent`](#groupdecreasenoticeevent), [`GroupIncreaseNoticeEvent`](#groupincreasenoticeevent), [`GroupBanNoticeEvent`](#groupbannoticeevent), [`FriendAddNoticeEvent`](#friendaddnoticeevent), [`GroupRecallNoticeEvent`](#grouprecallnoticeevent), [`FriendRecallNoticeEvent`](#friendrecallnoticeevent), [`GroupPokeNoticeEvent`](#grouppokenoticeevent), [`LuckyKingNoticeEvent`](#luckykingnoticeevent), [`GroupHonorNoticeEvent`](#grouphonornoticeevent) |
| Requests | [`FriendRequestEvent`](#friendrequestevent), [`GroupRequestEvent`](#grouprequestevent) |
| Meta events | [`LifecycleMetaEvent`](#lifecyclemetaevent), [`HeartbeatMetaEvent`](#heartbeatmetaevent) |
| Unknown fallbacks | [`UnknownOneBot11Event`](#unknownonebot11event), [`UnknownMessageEvent`](#unknownmessageevent), [`UnknownNoticeEvent`](#unknownnoticeevent), [`UnknownRequestEvent`](#unknownrequestevent), [`UnknownMetaEvent`](#unknownmetaevent) |

## Setup and startup

```csharp
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Messages;

var options = new OneBot11BotOptions(
    new OneBot11ActionEndpointOptions("127.0.0.1", 3000, "ActionToken"),
    new OneBot11EventEndpointOptions("127.0.0.1", 3001, "EventToken"));

using var bot = new OneBot11Bot(options);
```

### The two endpoint parameters

This `OneBot11BotOptions` constructor always combines HTTP Actions with forward WebSocket Events. Its two arguments mean:

| Constructor argument | Communication mode | Initiator | Default address and purpose |
| --- | --- | --- | --- |
| `OneBot11ActionEndpointOptions actionEndpoint` | [`HTTP`](https://github.com/botuniverse/onebot-11/blob/master/communication/http.md), directionally forward HTTP | SDK → OneBot implementation | The host-and-port constructor creates `http://host:port/`; it configures the Action base address, token, and response limit, and the SDK appends each Action name. |
| `OneBot11EventEndpointOptions eventEndpoint` | [Forward WebSocket](https://github.com/botuniverse/onebot-11/blob/master/communication/ws.md) | SDK → OneBot implementation | The host-and-port constructor creates `ws://host:port/event`; it configures the Event address, token, and WebSocket session settings. |

These arguments are not forward/reverse mode selectors: `actionEndpoint` is the HTTP Action endpoint, and `eventEndpoint` is the forward WebSocket Event endpoint.

The `Uri` constructors accept a “reverse-proxy path.” Here, reverse proxy means a deployment URL prefix provided by software such as Nginx or Caddy; it is unrelated to OneBot reverse HTTP or reverse WebSocket communication.

Action and Event addresses and tokens are configured separately. Subscribe before calling `StartAsync()`.

### EventHandler

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawMessage);

    foreach (var text in args.Event.MessageChain.OfType<TextReceivedSegment>())
        Console.WriteLine("Text: " + text.Text);

    foreach (var image in args.Event.MessageChain.OfType<ImageReceivedSegment>())
        Console.WriteLine("Image: " + image.Url);
};
```

### Observable

```csharp
using var subscription = bot.MessageReceived
    .OfType<PrivateMessageEvent>()
    .Subscribe(message => Console.WriteLine(message.MessageChain.PlainText));
```

### Start listening

```csharp
var login = await bot.StartAsync();
Console.WriteLine($"Connected: {login.Data?.Nickname}");
Console.WriteLine("Press Enter to exit.");
Console.ReadLine();
```

The console sample does not need `ManualResetEvent`. If you use one, dispose its operating-system wait handle; a `using var exit = new ManualResetEvent(false);` declaration avoids nesting. Hosted services should use their host cancellation token.

## Receiving events

Subscribe before `StartAsync()`. Every event inherits nullable `Time`, `SelfId`, and `PostType`, and retains the complete source object in `RawJson`. Type-specific fields are parsed independently and may be `null` when an implementation omits or malforms them. The snippets below use the concrete `EventHandler` exposed by `bot.Events`; retain the delegate if you need to unsubscribe later.

**Message events**

Both message types inherit `MessageType`, `SubType`, `MessageId`, `UserId`, `MessageChain`, `RawMessage`, and `Font`.

<a id="privatemessageevent"></a>

### `PrivateMessageEvent` — Private message (`message/private`)

**Subscription:** `bot.Events.PrivateMessageReceived`

Represents a private message. Key fields are `UserId`, `MessageId`, `SubType`, `MessageChain`, `RawMessage`, and best-effort `Sender` information.

```csharp
bot.Events.PrivateMessageReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.UserId}: {args.Event.MessageChain.PlainText}");
};
```

<a id="groupmessageevent"></a>

### `GroupMessageEvent` — Group message (`message/group`)

**Subscription:** `bot.Events.GroupMessageReceived`

Represents a group message. In addition to the common message fields, use `GroupId`, nullable `Anonymous`, and best-effort `Sender`; an anonymous sender's regular sender fields are not reliable.

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}/{args.Event.UserId}: {args.Event.MessageChain.PlainText}");
};
```

**Notice events**

Every notice inherits the nullable `NoticeType` discriminator in addition to the universal event fields.

<a id="groupuploadnoticeevent"></a>

### `GroupUploadNoticeEvent` — Group file upload (`notice/group_upload`)

**Subscription:** `bot.Events.GroupUploadNoticeReceived`

Reports a group file upload. Key fields are `GroupId`, uploader `UserId`, and `File`, whose `Id`, `Name`, `Size`, and `BusId` contain the file metadata.

```csharp
bot.Events.GroupUploadNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.File?.Name} ({args.Event.File?.Size} bytes)");
};
```

<a id="groupadminnoticeevent"></a>

### `GroupAdminNoticeEvent` — Group administrator change (`notice/group_admin`)

**Subscription:** `bot.Events.GroupAdminNoticeReceived`

Reports administrator status being set or removed. `SubType` is normally `set` or `unset`; `GroupId` and `UserId` identify the group and affected member.

```csharp
bot.Events.GroupAdminNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} {args.Event.SubType}");
};
```

<a id="groupdecreasenoticeevent"></a>

### `GroupDecreaseNoticeEvent` — Group member decrease (`notice/group_decrease`)

**Subscription:** `bot.Events.GroupDecreaseNoticeReceived`

Reports a member leaving or being removed. `SubType` is normally `leave`, `kick`, or `kick_me`; `GroupId`, `OperatorId`, and `UserId` identify the group, operator, and departing member.

```csharp
bot.Events.GroupDecreaseNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} left ({args.Event.SubType})");
};
```

<a id="groupincreasenoticeevent"></a>

### `GroupIncreaseNoticeEvent` — Group member increase (`notice/group_increase`)

**Subscription:** `bot.Events.GroupIncreaseNoticeReceived`

Reports a member joining. `SubType` is normally `approve` or `invite`; `GroupId`, `OperatorId`, and `UserId` identify the group, operator, and new member.

```csharp
bot.Events.GroupIncreaseNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} joined ({args.Event.SubType})");
};
```

<a id="groupbannoticeevent"></a>

### `GroupBanNoticeEvent` — Group mute change (`notice/group_ban`)

**Subscription:** `bot.Events.GroupBanNoticeReceived`

Reports mute state changes. `SubType` is normally `ban` or `lift_ban`; `GroupId`, `OperatorId`, `UserId`, and `Duration` describe the target and duration in seconds.

```csharp
bot.Events.GroupBanNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.UserId}: {args.Event.SubType}, {args.Event.Duration}s");
};
```

<a id="friendaddnoticeevent"></a>

### `FriendAddNoticeEvent` — Friend added (`notice/friend_add`)

**Subscription:** `bot.Events.FriendAddNoticeReceived`

Reports a newly added friend. `UserId` is the new friend's QQ ID.

```csharp
bot.Events.FriendAddNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"Friend added: {args.Event.UserId}");
};
```

<a id="grouprecallnoticeevent"></a>

### `GroupRecallNoticeEvent` — Group message recall (`notice/group_recall`)

**Subscription:** `bot.Events.GroupRecallNoticeReceived`

Reports a recalled group message. `GroupId`, original sender `UserId`, recalling `OperatorId`, and `MessageId` identify the operation.

```csharp
bot.Events.GroupRecallNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: recalled {args.Event.MessageId} by {args.Event.OperatorId}");
};
```

<a id="friendrecallnoticeevent"></a>

### `FriendRecallNoticeEvent` — Friend message recall (`notice/friend_recall`)

**Subscription:** `bot.Events.FriendRecallNoticeReceived`

Reports a recalled private message. `UserId` identifies the friend and `MessageId` identifies the recalled message.

```csharp
bot.Events.FriendRecallNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.UserId}: recalled {args.Event.MessageId}");
};
```

<a id="grouppokenoticeevent"></a>

### `GroupPokeNoticeEvent` — Group poke (`notice/notify/poke`)

**Subscription:** `bot.Events.GroupPokeNoticeReceived`

Reports a group poke. `SubType` is normally `poke`; `GroupId`, initiating `UserId`, and `TargetId` identify the participants.

```csharp
bot.Events.GroupPokeNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} poked {args.Event.TargetId}");
};
```

<a id="luckykingnoticeevent"></a>

### `LuckyKingNoticeEvent` — Lucky king (`notice/notify/lucky_king`)

**Subscription:** `bot.Events.LuckyKingNoticeReceived`

Reports the lucky king of a group red packet. `GroupId`, red-packet sender `UserId`, and lucky-king `TargetId` are the key fields.

```csharp
bot.Events.LuckyKingNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: lucky king {args.Event.TargetId}");
};
```

<a id="grouphonornoticeevent"></a>

### `GroupHonorNoticeEvent` — Group member honor change (`notice/notify/honor`)

**Subscription:** `bot.Events.GroupHonorNoticeReceived`

Reports a group honor change. `GroupId` and `UserId` identify the group and member; `HonorType` is normally `talkative`, `performer`, or `emotion`, and `SubType` is normally `honor`.

```csharp
bot.Events.GroupHonorNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} received {args.Event.HonorType}");
};
```

**Request events**

Every request inherits `RequestType`, `UserId`, `Comment`, and `Flag`. Request flags are opaque, potentially sensitive tokens; preserve the exact value and pass it only to the matching request-handling Action when authorized.

<a id="friendrequestevent"></a>

### `FriendRequestEvent` — Friend request (`request/friend`)

**Subscription:** `bot.Events.FriendRequestReceived`

Reports a friend-add request. Key fields inherited from `OneBot11RequestEvent` are requester `UserId`, verification `Comment`, and handling `Flag`.

```csharp
bot.Events.FriendRequestReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"Friend request from {args.Event.UserId}: {args.Event.Comment}");
};
```

<a id="grouprequestevent"></a>

### `GroupRequestEvent` — Group join request or invitation (`request/group`)

**Subscription:** `bot.Events.GroupRequestReceived`

Reports a group join request or invitation. `SubType` is normally `add` or `invite`; `GroupId`, requester `UserId`, `Comment`, and `Flag` describe and identify the request.

```csharp
bot.Events.GroupRequestReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.SubType} from {args.Event.UserId}");
};
```

**Meta events**

Both meta events inherit the nullable `MetaEventType` discriminator.

<a id="lifecyclemetaevent"></a>

### `LifecycleMetaEvent` — Lifecycle meta event (`meta_event/lifecycle`)

**Subscription:** `bot.Events.LifecycleMetaEventReceived`

Reports implementation lifecycle changes. `SubType` is normally `enable`, `disable`, or `connect`; `MetaEventType` is inherited from the meta-event base class.

```csharp
bot.Events.LifecycleMetaEventReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"Lifecycle: {args.Event.SubType}");
};
```

<a id="heartbeatmetaevent"></a>

### `HeartbeatMetaEvent` — Heartbeat meta event (`meta_event/heartbeat`)

**Subscription:** `bot.Events.HeartbeatMetaEventReceived`

Reports periodic runtime health. `Interval` is milliseconds until the next heartbeat; nullable `Status?.Online` and `Status?.Good` contain the portable status fields.

```csharp
bot.Events.HeartbeatMetaEventReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"Heartbeat: online={args.Event.Status?.Online}, good={args.Event.Status?.Good}");
};
```

**Unknown fallback events**

Unknown fallbacks retain the complete `RawJson`. They are delivered through `UnknownEventDispatched` and `bot.UnknownEventReceived`, allowing newer or implementation-specific discriminators to remain observable.

<a id="unknownonebot11event"></a>

### `UnknownOneBot11Event` — Unknown top-level event (unknown `post_type`)

**Subscription:** `bot.Events.UnknownEventDispatched` (pattern-match `UnknownOneBot11Event`)

Used when the top-level `post_type` is unknown. Inspect `PostType` and `RawJson`.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownOneBot11Event e)
        Console.WriteLine($"Unknown post_type: {e.PostType}");
};
```

<a id="unknownmessageevent"></a>

### `UnknownMessageEvent` — Unknown message type (`message/*`)

**Subscription:** `bot.Events.UnknownEventDispatched` (pattern-match `UnknownMessageEvent`)

Used when `post_type` is `message` but `message_type` is unknown. It retains the common message fields, including `MessageType`, `UserId`, `MessageChain`, and `RawMessage`.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownMessageEvent e)
        Console.WriteLine($"Unknown message_type: {e.MessageType}");
};
```

<a id="unknownnoticeevent"></a>

### `UnknownNoticeEvent` — Unknown notice combination (`notice/*` or `notice/notify/*`)

**Subscription:** `bot.Events.UnknownEventDispatched` (pattern-match `UnknownNoticeEvent`)

Used for an unknown notice discriminator combination. Inspect `NoticeType`, optional `SubType`, and `RawJson`.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownNoticeEvent e)
        Console.WriteLine($"Unknown notice: {e.NoticeType}/{e.SubType}");
};
```

<a id="unknownrequestevent"></a>

### `UnknownRequestEvent` — Unknown request type (`request/*`)

**Subscription:** `bot.Events.UnknownEventDispatched` (pattern-match `UnknownRequestEvent`)

Used when `request_type` is unknown. It retains `RequestType`, optional `SubType`, `UserId`, `Comment`, `Flag`, and `RawJson`.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownRequestEvent e)
        Console.WriteLine($"Unknown request: {e.RequestType}/{e.SubType}");
};
```

<a id="unknownmetaevent"></a>

### `UnknownMetaEvent` — Unknown meta-event type (`meta_event/*`)

**Subscription:** `bot.Events.UnknownEventDispatched` (pattern-match `UnknownMetaEvent`)

Used when `meta_event_type` is unknown. Inspect `MetaEventType`, optional `SubType`, and `RawJson`.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownMetaEvent e)
        Console.WriteLine($"Unknown meta event: {e.MetaEventType}/{e.SubType}");
};
```

## Actions

`OneBot11Client` exposes 40 action method names (45 overloads): strongly typed methods for all 38 public OneBot 11 actions, plus `CallActionAsync` and `HandleQuickOperationAsync` for advanced calls.

Every method accepts the same optional trailing parameters unless stated otherwise:

- `invocationMode` (`InvocationMode`, default `InvocationMode.Normal`): `Normal` calls the base action and waits for its result. `Async` adds `_async`; the protocol reports only acceptance and does not return the action's final result. `RateLimited` adds `_rate_limited` and queues the action at the rate configured by the implementation.
- `echo` (`JsonNode?`, default `null`): optional correlation data; WebSocket transports send it in the action envelope, while the HTTP action transport ignores it because HTTP responses are already paired with their requests.
- `cancellationToken` (`CancellationToken`, default `default`): cancels the transport operation.

Every response exposes `IsSuccess`, `Status`, `RetCode`, `RawRequestJson`, and `RawResponseJson`. Generic responses also expose typed `Data` and the unprojected `RawData`. The examples below deliberately print only the exact request and response JSON retained by the transport, without a shared output helper.

Raw packets can contain private messages, cookies, CSRF tokens, and other sensitive values. Print them only in a controlled diagnostic environment and redact them before sharing logs.

The snippets are executable, not side-effect-free probes. Sending or deleting messages, sending likes, handling friend/group requests, group-management calls, `CleanCacheAsync`, `SetRestartAsync`, and `HandleQuickOperationAsync` can all change external state; run them only against intended targets and with event data that you are authorized to process.

The snippets assume that `bot` was created as shown above and use these example values. Replace flags and file names with values received from your implementation.

```csharp
long userId = 123456789;
long groupId = 987654321;
long messageId = 123;
string requestFlag = "flag from an event";
string anonymousFlag = "flag from an anonymous event";
string forwardId = "merged-forward ID";
string imageFile = "received image file name";
string recordFile = "received audio file name";
var message = new OneBot11SendMessage().Text("Hello");
```

**Message actions**

<a id="sendprivatemessageasync"></a>

### `SendPrivateMessageAsync` — Send a private message (`send_private_msg`)

**Purpose.** Sends a message to one QQ user.

**Parameters.** `userId` (`long`, required) is the recipient QQ ID; `message` (`OneBot11SendMessage`, non-null) is the outgoing message; `autoEscape` (`bool`, default `false`) asks the implementation to treat a string message as plain text when true.

**Returns.** `OneBot11Response<OneBot11SendMessageResult>`; `Data?.MessageId` is the assigned message ID.

```csharp
var response = await bot.Actions.SendPrivateMessageAsync(userId, message);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The compatibility overload replaces `message` with a non-null `OneBot11Message`; `userId` remains a required `long`, and `autoEscape` remains a `bool` defaulting to `false`. It returns `OneBot11Response<OneBot11SendMessageData>`, whose `Data?.MessageId` has the same meaning. Prefer `OneBot11SendMessage` in new code.

```csharp
var compatibilityResponse = await bot.Actions.SendPrivateMessageAsync(
    userId,
    OneBot11Message.FromString("Hello"));
Console.WriteLine(compatibilityResponse.RawRequestJson);
Console.WriteLine(compatibilityResponse.RawResponseJson);
```

<a id="sendgroupmessageasync"></a>

### `SendGroupMessageAsync` — Send a group message (`send_group_msg`)

**Purpose.** Sends a message to a group.

**Parameters.** `groupId` (`long`, required) is the target group ID; `message` (`OneBot11SendMessage`, non-null) is the outgoing message; `autoEscape` (`bool`, default `false`) controls string-message escaping.

**Returns.** `OneBot11Response<OneBot11SendMessageResult>`; `Data?.MessageId` is the assigned message ID.

```csharp
var response = await bot.Actions.SendGroupMessageAsync(groupId, message);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The compatibility overload replaces `message` with a non-null `OneBot11Message`; `groupId` remains a required `long`, and `autoEscape` remains a `bool` defaulting to `false`. It returns `OneBot11Response<OneBot11SendMessageData>`. Prefer the outgoing-only model in new code.

```csharp
var compatibilityResponse = await bot.Actions.SendGroupMessageAsync(
    groupId,
    OneBot11Message.FromString("Hello"));
Console.WriteLine(compatibilityResponse.RawRequestJson);
Console.WriteLine(compatibilityResponse.RawResponseJson);
```

<a id="sendmessageasync"></a>

### `SendMessageAsync` — Send a message (`send_msg`)

**Purpose.** Sends a message through the generic `send_msg` action when the target kind is selected at runtime.

**Parameters.** `message` (`OneBot11SendMessage`, non-null) is the outgoing message; `messageType` (`OneBot11MessageType?`, default `null`) accepts `Private`, `Group`, or `null`; `userId` (`long?`, default `null`) and `groupId` (`long?`, default `null`) are nullable target IDs, so supply the one matching `messageType`; `autoEscape` (`bool`, default `false`) controls string-message escaping. The client serializes only target fields that have values, so provide a valid target combination for the implementation.

**Returns.** `OneBot11Response<OneBot11SendMessageResult>` with the assigned `Data?.MessageId`.

```csharp
var response = await bot.Actions.SendMessageAsync(
    message,
    OneBot11MessageType.Group,
    groupId: groupId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The compatibility overload replaces `message` with a non-null `OneBot11Message`; the nullable target parameters and `autoEscape` keep the types and defaults listed above. It returns `OneBot11Response<OneBot11SendMessageData>`.

```csharp
var compatibilityResponse = await bot.Actions.SendMessageAsync(
    OneBot11Message.FromString("Hello"),
    OneBot11MessageType.Group,
    groupId: groupId);
Console.WriteLine(compatibilityResponse.RawRequestJson);
Console.WriteLine(compatibilityResponse.RawResponseJson);
```

<a id="deletemessageasync"></a>

### `DeleteMessageAsync` — Recall a message (`delete_msg`)

**Purpose.** Recalls a previously sent or received message.

**Parameters.** `messageId` (`long`, required) is the OneBot message ID to recall.

**Returns.** `OneBot11Response`. The standard success response has no typed data payload; inspect `IsSuccess`/`RetCode` or the raw response.

```csharp
var response = await bot.Actions.DeleteMessageAsync(messageId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getmessageasync"></a>

### `GetMessageAsync` — Get a message (`get_msg`)

**Purpose.** Retrieves one message by its OneBot message ID.

**Parameters.** `messageId` (`long`, required) is the message to retrieve.

**Returns.** `OneBot11Response<OneBot11MessageData>`. `Data` contains `Time`, `MessageType`, `MessageId`, `RealId`, `Sender`, and the parsed received `MessageChain`.

```csharp
var response = await bot.Actions.GetMessageAsync(messageId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getforwardmessageasync"></a>

### `GetForwardMessageAsync` — Get a merged-forward message (`get_forward_msg`)

**Purpose.** Retrieves the contents of a merged-forward message.

**Parameters.** `id` (`string`, non-null) is the forward ID obtained from a received forward segment.

**Returns.** `OneBot11Response<OneBot11ForwardMessageData>`; `Data?.MessageChain` contains the received forward nodes.

```csharp
var response = await bot.Actions.GetForwardMessageAsync(forwardId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendlikeasync"></a>

### `SendLikeAsync` — Send friend likes (`send_like`)

**Purpose.** Sends one or more likes to a friend.

**Parameters.** `userId` (`long`, required) is the friend QQ ID; `times` (`long`, default `1`) is the number of likes. OneBot limits each friend to ten likes per day.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SendLikeAsync(userId, times: 1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Group-management actions**

These methods change external state. Test them only with groups and accounts where the operation is intended.

<a id="setgroupkickasync"></a>

### `SetGroupKickAsync` — Remove a group member (`set_group_kick`)

**Purpose.** Removes a member from a group.

**Parameters.** `groupId` (`long`, required) identifies the group; `userId` (`long`, required) identifies the member; `rejectAddRequest` (`bool`, default `false`) controls whether future join requests from that member are rejected.

**Returns.** `OneBot11Response` without a standard typed data payload. This membership change cannot be automatically undone. Never use the currently logged-in account or a group owner as an automatic test target.

```csharp
var response = await bot.Actions.SetGroupKickAsync(
    groupId,
    userId,
    rejectAddRequest: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupbanasync"></a>

### `SetGroupBanAsync` — Mute a group member (`set_group_ban`)

**Purpose.** Mutes or unmutes one group member.

**Parameters.** `groupId` (`long`, required) and `userId` (`long`, required) select the member; `duration` (`long`, default `1800`) is the mute duration in seconds, and `0` removes the mute.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupBanAsync(groupId, userId, duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousbanasync"></a>

### `SetGroupAnonymousBanAsync` — Mute an anonymous group member (`set_group_anonymous_ban`)

**Purpose.** Mutes an anonymous participant in a group.

**Parameters.** Both overloads take `groupId` (`long`, required) and `duration` (`long`, default `1800`) in seconds. One requires `anonymousFlag` (`string`, non-null) from the event and sends `anonymous_flag`; the other requires the complete `anonymous` (`JsonObject`, non-null) event object and sends it as `anonymous`.

**Returns.** Both overloads return `OneBot11Response` without a standard typed data payload.

```csharp
var flagResponse = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId,
    anonymousFlag,
    duration: 60);
Console.WriteLine(flagResponse.RawRequestJson);
Console.WriteLine(flagResponse.RawResponseJson);
```

The complete-object overload sends the supplied `JsonObject` as `anonymous`:

```csharp
var anonymous = new JsonObject { ["flag"] = anonymousFlag };
var objectResponse = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId,
    anonymous,
    duration: 60);
Console.WriteLine(objectResponse.RawRequestJson);
Console.WriteLine(objectResponse.RawResponseJson);
```

<a id="setgroupwholebanasync"></a>

### `SetGroupWholeBanAsync` — Toggle whole-group mute (`set_group_whole_ban`)

**Purpose.** Enables or disables whole-group mute.

**Parameters.** `groupId` (`long`, required) identifies the group; `enable` (`bool`, default `true`) enables whole-group mute when true.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupWholeBanAsync(groupId, enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupadminasync"></a>

### `SetGroupAdminAsync` — Set a group administrator (`set_group_admin`)

**Purpose.** Grants or removes group administrator status.

**Parameters.** `groupId` (`long`, required) and `userId` (`long`, required) select the member; `enable` (`bool`, default `true`) grants administrator status when true and removes it when false.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupAdminAsync(groupId, userId, enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousasync"></a>

### `SetGroupAnonymousAsync` — Toggle anonymous group chat (`set_group_anonymous`)

**Purpose.** Enables or disables anonymous chat in a group.

**Parameters.** `groupId` (`long`, required) identifies the group; `enable` (`bool`, default `true`) enables anonymous chat when true.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupAnonymousAsync(groupId, enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupcardasync"></a>

### `SetGroupCardAsync` — Set a group card (`set_group_card`)

**Purpose.** Sets or removes a member's group card.

**Parameters.** `groupId` (`long`, required) and `userId` (`long`, required) select the member; `card` (`string`, non-null, default `""`) is the new card, and an empty value removes the current card.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupCardAsync(groupId, userId, card: "New card");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupnameasync"></a>

### `SetGroupNameAsync` — Set the group name (`set_group_name`)

**Purpose.** Changes a group's name.

**Parameters.** `groupId` (`long`, required) identifies the group; `groupName` (`string`, non-null) is the new name.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupNameAsync(groupId, groupName: "New group name");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupleaveasync"></a>

### `SetGroupLeaveAsync` — Leave a group (`set_group_leave`)

**Purpose.** Leaves a group, or requests dismissal when the logged-in account owns the group.

**Parameters.** `groupId` (`long`, required) identifies the group; `isDismiss` (`bool`, default `false`) requests group dismissal when true.

**Returns.** `OneBot11Response` without a standard typed data payload. This action is destructive and cannot be automatically undone; some implementations may treat a group owner's leave request differently even when `isDismiss` is false. Do not run it from an automated example/test or against a group that must be preserved.

```csharp
var response = await bot.Actions.SetGroupLeaveAsync(groupId, isDismiss: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupspecialtitleasync"></a>

### `SetGroupSpecialTitleAsync` — Set a group special title (`set_group_special_title`)

**Purpose.** Sets or removes a member's special group title.

**Parameters.** `groupId` (`long`, required) and `userId` (`long`, required) select the member; `specialTitle` (`string`, non-null, default `""`) sets the title and an empty value removes it; `duration` (`long`, default `-1`) is in seconds and requests a permanent title where supported.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupSpecialTitleAsync(
    groupId,
    userId,
    specialTitle: "Title",
    duration: -1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Request-handling actions**

<a id="setfriendaddrequestasync"></a>

### `SetFriendAddRequestAsync` — Handle a friend request (`set_friend_add_request`)

**Purpose.** Approves or rejects a friend request.

**Parameters.** `flag` (`string`, non-null) is the exact value from `FriendRequestEvent`; `approve` (`bool`, default `true`) accepts or rejects it; `remark` (`string`, non-null, default `""`) becomes the friend remark when approved.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetFriendAddRequestAsync(
    requestFlag,
    approve: true,
    remark: "Remark");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupaddrequestasync"></a>

### `SetGroupAddRequestAsync` — Handle a group request or invitation (`set_group_add_request`)

**Purpose.** Approves or rejects a group join request or invitation.

**Parameters.** `flag` (`string`, non-null) is the exact value from `GroupRequestEvent`; `requestType` (`OneBot11GroupRequestType`, required) is `Add` for a join request or `Invite` for an invitation; `approve` (`bool`, default `true`) accepts or rejects it; `reason` (`string`, non-null, default `""`) is the rejection reason.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.SetGroupAddRequestAsync(
    requestFlag,
    OneBot11GroupRequestType.Add,
    approve: true,
    reason: "");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Information-query actions**

<a id="getlogininfoasync"></a>

### `GetLoginInfoAsync` — Get login information (`get_login_info`)

**Purpose.** Gets the QQ account currently logged in by the implementation.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<OneBot11LoginInfoData>`; `Data` contains `UserId` and `Nickname`.

```csharp
var response = await bot.Actions.GetLoginInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstrangerinfoasync"></a>

### `GetStrangerInfoAsync` — Get stranger information (`get_stranger_info`)

**Purpose.** Gets public information about a QQ user who need not be a friend.

**Parameters.** `userId` (`long`, required) is the QQ ID; `noCache` (`bool`, default `false`) requests fresh information when true.

**Returns.** `OneBot11Response<OneBot11StrangerInfoData>`; `Data` contains `UserId`, `Nickname`, `Sex`, and `Age` when supplied by the implementation.

```csharp
var response = await bot.Actions.GetStrangerInfoAsync(userId, noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfriendlistasync"></a>

### `GetFriendListAsync` — Get the friend list (`get_friend_list`)

**Purpose.** Gets all friends visible to the logged-in account.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<IReadOnlyList<OneBot11FriendInfo>>`; each item can contain `UserId`, `Nickname`, and `Remark`.

```csharp
var response = await bot.Actions.GetFriendListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupinfoasync"></a>

### `GetGroupInfoAsync` — Get group information (`get_group_info`)

**Purpose.** Gets information about one group.

**Parameters.** `groupId` (`long`, required) identifies the group; `noCache` (`bool`, default `false`) requests fresh information when true.

**Returns.** `OneBot11Response<OneBot11GroupInfo>`; `Data` can contain `GroupId`, `GroupName`, `MemberCount`, and `MaxMemberCount`.

```csharp
var response = await bot.Actions.GetGroupInfoAsync(groupId, noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouplistasync"></a>

### `GetGroupListAsync` — Get the group list (`get_group_list`)

**Purpose.** Gets all groups visible to the logged-in account.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<IReadOnlyList<OneBot11GroupInfo>>`; each item contains the same fields as `GetGroupInfoAsync` when available.

```csharp
var response = await bot.Actions.GetGroupListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberinfoasync"></a>

### `GetGroupMemberInfoAsync` — Get group member information (`get_group_member_info`)

**Purpose.** Gets detailed information about one group member.

**Parameters.** `groupId` (`long`, required) and `userId` (`long`, required) select the member; `noCache` (`bool`, default `false`) requests fresh information when true.

**Returns.** `OneBot11Response<OneBot11GroupMemberInfo>`. `Data` includes IDs, nickname/card, profile fields, join and last-sent times, level/role, `Unfriendly`, title information, and card-change permission when available.

```csharp
var response = await bot.Actions.GetGroupMemberInfoAsync(
    groupId,
    userId,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberlistasync"></a>

### `GetGroupMemberListAsync` — Get the group member list (`get_group_member_list`)

**Purpose.** Gets the members of one group.

**Parameters.** `groupId` (`long`, required) identifies the group.

**Returns.** `OneBot11Response<IReadOnlyList<OneBot11GroupMemberInfo>>`. Some per-member fields may be absent in list responses.

```csharp
var response = await bot.Actions.GetGroupMemberListAsync(groupId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouphonorinfoasync"></a>

### `GetGroupHonorInfoAsync` — Get group honor information (`get_group_honor_info`)

**Purpose.** Gets one or all standard group-honor categories.

**Parameters.** `groupId` (`long`, required) identifies the group; `honorType` (`OneBot11GroupHonorType`, required) is `Talkative`, `Performer`, `Legend`, `StrongNewbie`, `Emotion`, or `All`.

**Returns.** `OneBot11Response<OneBot11GroupHonorInfoData>`. `Data` contains `GroupId`; the other fields appear conditionally according to `honorType`: `CurrentTalkative`, `TalkativeList`, `PerformerList`, `LegendList`, `StrongNewbieList`, and `EmotionList`.

```csharp
var response = await bot.Actions.GetGroupHonorInfoAsync(
    groupId,
    OneBot11GroupHonorType.All);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**File, capability, and runtime actions**

<a id="getcookiesasync"></a>

### `GetCookiesAsync` — Get Cookies (`get_cookies`)

**Purpose.** Gets QQ cookies, optionally restricted to a domain.

**Parameters.** `domain` (`string`, non-null, default `""`) optionally restricts the cookies to a domain; an empty value applies no restriction.

**Returns.** `OneBot11Response<OneBot11CookiesData>`; the cookie header value is in `Data?.Cookies`.

```csharp
var response = await bot.Actions.GetCookiesAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcsrftokenasync"></a>

### `GetCsrfTokenAsync` — Get a CSRF token (`get_csrf_token`)

**Purpose.** Gets the current QQ CSRF token.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<OneBot11CsrfTokenData>`; the token is in `Data?.Token`.

```csharp
var response = await bot.Actions.GetCsrfTokenAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcredentialsasync"></a>

### `GetCredentialsAsync` — Get QQ interface credentials (`get_credentials`)

**Purpose.** Gets cookies and the CSRF token in one call.

**Parameters.** `domain` (`string`, non-null, default `""`) optionally restricts the credentials to a domain; an empty value applies no restriction.

**Returns.** `OneBot11Response<OneBot11CredentialsData>`; use `Data?.Cookies` and `Data?.CsrfToken`.

```csharp
var response = await bot.Actions.GetCredentialsAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getrecordasync"></a>

### `GetRecordAsync` — Get a voice record (`get_record`)

**Purpose.** Gets and converts a received voice-record file.

**Parameters.** `file` (`string`, non-null) is the file value from a received record segment; `outputFormat` (`OneBot11RecordFormat`, required) is `Mp3`, `Amr`, `Wma`, `M4a`, `Spx`, `Ogg`, `Wav`, or `Flac`.

**Returns.** `OneBot11Response<OneBot11FileData>`; `Data?.File` is the implementation-local converted file path.

```csharp
var response = await bot.Actions.GetRecordAsync(
    recordFile,
    OneBot11RecordFormat.Mp3);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getimageasync"></a>

### `GetImageAsync` — Get an image (`get_image`)

**Purpose.** Gets a received image file.

**Parameters.** `file` (`string`, non-null) is the file value from a received image segment.

**Returns.** `OneBot11Response<OneBot11FileData>`; `Data?.File` is the implementation-local file path.

```csharp
var response = await bot.Actions.GetImageAsync(imageFile);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendimageasync"></a>

### `CanSendImageAsync` — Check image-send support (`can_send_image`)

**Purpose.** Checks whether the implementation can send images.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<OneBot11CapabilityData>`; `Data?.Yes` is the capability result.

```csharp
var response = await bot.Actions.CanSendImageAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendrecordasync"></a>

### `CanSendRecordAsync` — Check voice-record-send support (`can_send_record`)

**Purpose.** Checks whether the implementation can send voice records.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<OneBot11CapabilityData>`; `Data?.Yes` is the capability result.

```csharp
var response = await bot.Actions.CanSendRecordAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstatusasync"></a>

### `GetStatusAsync` — Get runtime status (`get_status`)

**Purpose.** Gets portable health and login status from the implementation.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<OneBot11StatusData>`; `Data?.Online` reports QQ login state and `Data?.Good` reports overall health. Implementation-specific fields remain in `RawData`.

```csharp
var response = await bot.Actions.GetStatusAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getversioninfoasync"></a>

### `GetVersionInfoAsync` — Get version information (`get_version_info`)

**Purpose.** Gets implementation and OneBot protocol version information.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response<OneBot11VersionInfoData>`; `Data` contains `AppName`, `AppVersion`, and `ProtocolVersion`.

```csharp
var response = await bot.Actions.GetVersionInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setrestartasync"></a>

### `SetRestartAsync` — Restart the OneBot implementation (`set_restart`)

**Purpose.** Requests an inherently asynchronous implementation restart.

**Parameters.** `delay` (`long`, default `0`) is the restart delay in milliseconds.

**Returns.** `OneBot11Response` without a standard typed data payload. The endpoint may become temporarily unavailable after a successful request.

```csharp
var response = await bot.Actions.SetRestartAsync(delay: 0);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cleancacheasync"></a>

### `CleanCacheAsync` — Clean the cache (`clean_cache`)

**Purpose.** Asks the implementation to clean its cache files.

**Parameters.** No action-specific parameters.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var response = await bot.Actions.CleanCacheAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Advanced action calls**

<a id="callactionasync"></a>

### `CallActionAsync` — Call a dynamic Action (runtime `action`)

**Purpose.** Calls a standard or implementation-specific action by name. Use this when no strongly typed method matches the implementation extension.

**Parameters.** Both overloads require `action` (`string`, non-null/non-empty/non-whitespace); `parameters` (`JsonObject?`, default `null`) supplies Action parameters. They also accept the common trailing parameters. The generic overload additionally requires `dataParser` (`Func<JsonNode?, TData?>`, non-null) to project the response `data` node.

**Returns.** The non-generic overload returns `OneBot11Response` with raw `JsonNode? Data`. The generic overload returns `OneBot11Response<TData>` with parsed `Data` and unprojected `RawData`.

```csharp
var response = await bot.Actions.CallActionAsync(
    "implementation_extension",
    new JsonObject { ["key"] = "value" });
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The generic overload applies the supplied data parser:

```csharp
var parsedResponse = await bot.Actions.CallActionAsync<JsonNode>(
    OneBot11Actions.GetVersionInfo,
    node => node);
Console.WriteLine(parsedResponse.RawRequestJson);
Console.WriteLine(parsedResponse.RawResponseJson);
```

<a id="handlequickoperationasync"></a>

### `HandleQuickOperationAsync` — Perform a quick operation for an event (hidden Action `.handle_quick_operation`)

**Purpose.** Calls the official hidden `.handle_quick_operation` action for an event received through HTTP POST. Availability and supported operation fields depend on the implementation.

**Parameters.** `context` (`JsonObject`, non-null) is the complete event JSON received by HTTP POST; `operation` (`JsonObject`, non-null) describes a supported reply, delete, kick, ban, or other quick operation.

**Returns.** `OneBot11Response` without a standard typed data payload.

```csharp
var eventContext = new JsonObject
{
    ["post_type"] = "message",
    ["message_type"] = "group",
    ["group_id"] = groupId,
    ["user_id"] = userId,
    ["message_id"] = messageId
}; // Replace with the complete HTTP POST event object.
var operation = new JsonObject { ["reply"] = "Received" };

var response = await bot.Actions.HandleQuickOperationAsync(eventContext, operation);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

## Message chains

New code should build ordered outgoing messages with `OneBot11SendMessage`:

```csharp
var message = new OneBot11SendMessage()
    .Text("Image below:")
    .Image("https://example.com/cat.png");

var response = await bot.Actions.SendGroupMessageAsync(
    groupId: 123456789,
    message: message);
```

Incoming messages use `OneBot11ReceivedMessage`. Read concrete segments with `MessageChain.OfType<T>()`; unknown segments become `UnknownReceivedSegment` and retain their raw JSON.

The older `OneBot11MessageChain` and `OneBot11Message` remain for compatibility. Prefer the direction-specific send and receive models in new projects.

## Supported message segments

Each snippet creates a segment that can be added to `OneBot11SendMessage`, except where marked receive-only.

### `text`

```csharp
var segment = new TextSendSegment("Hello");
```

`text` may be empty but not `null`.

### `face`

```csharp
var segment = new FaceSendSegment(14L);
```

`id` is the protocol face ID and may be a `long` or string.

### `image`

```csharp
var segment = new ImageSendSegment(
    file: "https://example.com/cat.png",
    flash: false,
    cache: true,
    proxy: true,
    timeoutSeconds: 30);
```

`file` may be a received file name, file URI, network URL, or `base64://` URI. The other parameters control flash mode, caching, proxying, and download timeout.

### `record`

```csharp
var segment = new RecordSendSegment(
    file: "file:///D:/audio/test.amr",
    magic: false,
    cache: true,
    proxy: true,
    timeoutSeconds: 30);
```

`file` is the audio source; `magic` requests a voice effect. The remaining parameters match `image`.

### `video`

```csharp
var segment = new VideoSendSegment(
    file: "https://example.com/video.mp4",
    cache: true,
    proxy: true,
    timeoutSeconds: 30);
```

`file` is the video source; the remaining parameters control its download.

### `at`

```csharp
var user = new AtSendSegment(123456789L);
var everyone = new AtSendSegment("all");
```

Pass a QQ ID, or `all` to mention everyone.

### `rps`

```csharp
var segment = new RpsSendSegment();
```

No parameters.

### `dice`

```csharp
var segment = new DiceSendSegment();
```

No parameters.

### `shake`

```csharp
var segment = new ShakeSendSegment();
```

No parameters.

### `poke`

```csharp
var segment = new PokeSendSegment(pokeType: "1", id: "2");
```

`pokeType` and `id` are implementation-defined poke identifiers.

### `anonymous`

```csharp
var segment = new AnonymousSendSegment(ignoreFailure: true);
```

`ignoreFailure` controls whether sending continues when anonymous mode is unavailable. This type is send-only.

### `share`

```csharp
var segment = new ShareSendSegment(
    url: "https://example.com",
    title: "Example",
    content: "Optional summary",
    image: "https://example.com/cover.png");
```

`url` and `title` are required; `content` and `image` are optional.

### `contact`

```csharp
var friend = new ContactSendSegment(OneBot11ContactTarget.Friend, "123456789");
var group = new ContactSendSegment(OneBot11ContactTarget.Group, "987654321");
```

The first parameter selects a friend or group; `id` is the corresponding QQ or group ID.

### `location`

```csharp
var segment = new LocationSendSegment(
    latitude: "39.9042",
    longitude: "116.4074",
    title: "Beijing",
    content: "Optional description");
```

Coordinates are strings; `title` and `content` are optional.

### `music`

```csharp
var providerMusic = new MusicSendSegment(OneBot11MusicProvider.QQ, "song-id");
var customMusic = new CustomMusicSendSegment(
    url: "https://example.com/song",
    audio: "https://example.com/song.mp3",
    title: "Song title",
    content: "Optional description",
    image: "https://example.com/cover.png");
```

Provider music accepts `QQ`, `NetEase`, or `Xiami` plus a song ID. Custom music requires a page URL, audio URL, and title.

### `reply`

```csharp
var segment = new ReplySendSegment(messageId: 123L);
```

`messageId` is the referenced message and may be a `long` or string.

### `forward` (receive-only)

```csharp
foreach (var forward in messageEvent.MessageChain.OfType<ForwardReceivedSegment>())
    await bot.Actions.GetForwardMessageAsync(forward.ForwardId!);
```

Pass `ForwardId` to `GetForwardMessageAsync` to retrieve merged-forward content.

### `node`

```csharp
var existing = new ForwardNodeSendSegment(messageId: 123L);
var custom = new CustomForwardNodeSendSegment(
    userId: "123456789",
    nickname: "Sender",
    content: new OneBot11SendMessage().Text("Node content"));
```

An existing node needs a message ID. A custom node needs the displayed user ID, nickname, and nested message.

### `xml`

```csharp
var segment = new XmlSendSegment("<msg>...</msg>");
```

`xml` is the complete XML string.

### `json`

```csharp
var segment = new JsonSendSegment("{\"app\":\"example\"}");
```

`json` is the implementation-specific JSON string.

### Implementation extensions

```csharp
var segment = new CustomSendSegment(
    "markdown",
    new JsonObject { ["content"] = "**Hello**" });
```

`type` and `data` follow the implementation's contract.

## Console debugging project

These runnable projects are optional debugging tools. Use the Actions and Receiving events sections above as the API reference. Their configuration may contain live credentials and target IDs, so review it before running any state-changing operation.

- [Observable sample](../samples/OneBotSdk.Net.ObservableExample)
- [EventHandler sample](../samples/OneBotSdk.Net.EventHandlerExample)
- [HTTP Action sample](../samples/OneBotSdk.Net.HttpActionExample)

Do not place tokens in source code or logs. Prefer HTTPS/WSS in production.
