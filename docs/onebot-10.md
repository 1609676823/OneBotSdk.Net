# OneBot 10 usage guide

[README](../README.md) | [Documentation index](README.md) | [简体中文](onebot-10.zh-CN.md)

> **Official specification:** [OneBot 10](https://github.com/botuniverse/onebot-10)

The OneBot 10 API lives under `OneBotSdk.Net.V10.*`. It currently covers 37 public actions, 13 standard concrete event classes, 5 unknown-event fallback classes, and 14 standard message-segment wire types. Use it only with an implementation that actually supports OneBot 10.

## Quick directory

- [Actions](#actions)
  - General: [`CallActionAsync`](#callactionasync), [`HandleQuickOperationAsync`](#handlequickoperationasync)
  - Messages: [`SendPrivateMessageAsync`](#sendprivatemessageasync), [`SendGroupMessageAsync`](#sendgroupmessageasync), [`SendDiscussMessageAsync`](#senddiscussmessageasync), [`SendMessageAsync`](#sendmessageasync), [`DeleteMessageAsync`](#deletemessageasync), [`SendLikeAsync`](#sendlikeasync)
  - Groups and discussions: [`SetGroupKickAsync`](#setgroupkickasync), [`SetGroupBanAsync`](#setgroupbanasync), [`SetGroupAnonymousBanAsync`](#setgroupanonymousbanasync), [`SetGroupWholeBanAsync`](#setgroupwholebanasync), [`SetGroupAdminAsync`](#setgroupadminasync), [`SetGroupAnonymousAsync`](#setgroupanonymousasync), [`SetGroupCardAsync`](#setgroupcardasync), [`SetGroupLeaveAsync`](#setgroupleaveasync), [`SetGroupSpecialTitleAsync`](#setgroupspecialtitleasync), [`SetDiscussLeaveAsync`](#setdiscussleaveasync)
  - Requests: [`SetFriendAddRequestAsync`](#setfriendaddrequestasync), [`SetGroupAddRequestAsync`](#setgroupaddrequestasync)
  - Information: [`GetLoginInfoAsync`](#getlogininfoasync), [`GetStrangerInfoAsync`](#getstrangerinfoasync), [`GetFriendListAsync`](#getfriendlistasync), [`GetGroupListAsync`](#getgrouplistasync), [`GetGroupInfoAsync`](#getgroupinfoasync), [`GetGroupMemberInfoAsync`](#getgroupmemberinfoasync), [`GetGroupMemberListAsync`](#getgroupmemberlistasync)
  - Files, credentials, and system: [`GetCookiesAsync`](#getcookiesasync), [`GetCsrfTokenAsync`](#getcsrftokenasync), [`GetCredentialsAsync`](#getcredentialsasync), [`GetRecordAsync`](#getrecordasync), [`GetImageAsync`](#getimageasync), [`CanSendImageAsync`](#cansendimageasync), [`CanSendRecordAsync`](#cansendrecordasync), [`GetStatusAsync`](#getstatusasync), [`GetVersionInfoAsync`](#getversioninfoasync), [`SetRestartPluginAsync`](#setrestartpluginasync), [`CleanDataDirectoryAsync`](#cleandatadirectoryasync), [`CleanPluginLogAsync`](#cleanpluginlogasync)
- [Receiving events](#receiving-events)
  - Messages: [`PrivateMessageEvent`](#privatemessageevent), [`GroupMessageEvent`](#groupmessageevent), [`DiscussMessageEvent`](#discussmessageevent)
  - Notices: [`GroupUploadNoticeEvent`](#groupuploadnoticeevent), [`GroupAdminNoticeEvent`](#groupadminnoticeevent), [`GroupDecreaseNoticeEvent`](#groupdecreasenoticeevent), [`GroupIncreaseNoticeEvent`](#groupincreasenoticeevent), [`GroupBanNoticeEvent`](#groupbannoticeevent), [`FriendAddNoticeEvent`](#friendaddnoticeevent)
  - Requests and meta events: [`FriendRequestEvent`](#friendrequestevent), [`GroupRequestEvent`](#grouprequestevent), [`LifecycleMetaEvent`](#lifecyclemetaevent), [`HeartbeatMetaEvent`](#heartbeatmetaevent)
  - Unknown fallbacks: [`UnknownOneBot10Event`](#unknownonebot10event), [`UnknownMessageEvent`](#unknownmessageevent), [`UnknownNoticeEvent`](#unknownnoticeevent), [`UnknownRequestEvent`](#unknownrequestevent), [`UnknownMetaEvent`](#unknownmetaevent)

## Setup and startup

```csharp
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Messages;

var options = new OneBot10BotOptions(
    new OneBot10ActionEndpointOptions("127.0.0.1", 3000, "ActionToken"),
    new OneBot10EventEndpointOptions("127.0.0.1", 3001, "EventToken"));

using var bot = new OneBot10Bot(options);
```

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

The console sample does not need `ManualResetEvent`. If you use one, dispose its operating-system wait handle; a `using var exit = new ManualResetEvent(false);` declaration avoids another nested block. Hosted services should use their host cancellation token.

## Receiving events

Subscribe before `StartAsync()`. Every event inherits nullable `Time`, `SelfId`, and `PostType`, and its `RawJson` retains the complete inbound object. The examples below are independent handlers that assume only the `bot` created in [Setup and startup](#setup-and-startup); each prints the original event payload directly. Standard events also have a same-type hot observable on `bot.Events`, while all five fallback types arrive through `UnknownEventDispatched` and `UnknownEvents`.

<a id="privatemessageevent"></a>

### `PrivateMessageEvent` — Private message (`message/private`)

**Subscription entry:** `bot.Events.PrivateMessageReceived` (EventHandler) or `bot.Events.PrivateMessages` (Observable).

Receives a private message. Key fields are `UserId`, `SubType`, `MessageId`, `MessageChain`, `RawMessage`, and nullable `Sender` details.

```csharp
bot.Events.PrivateMessageReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.UserId}: {e.MessageChain.PlainText}");
};
```

<a id="groupmessageevent"></a>

### `GroupMessageEvent` — Group message (`message/group`)

**Subscription entry:** `bot.Events.GroupMessageReceived` (EventHandler) or `bot.Events.GroupMessages` (Observable).

Receives a group message. `GroupId` and `UserId` identify the conversation and sender; `Anonymous` is non-null for anonymous messages, and `MessageChain`, `MessageId`, and `Sender` carry the content and sender details.

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.GroupId}/{e.UserId}: {e.MessageChain.PlainText}");
};
```

<a id="discussmessageevent"></a>

### `DiscussMessageEvent` — Discussion-group message (`message/discuss`)

**Subscription entry:** `bot.Events.DiscussMessageReceived` (EventHandler) or `bot.Events.DiscussMessages` (Observable).

Receives a discussion-group message. Key fields are `DiscussId`, `UserId`, `MessageId`, `MessageChain`, and nullable `Sender`.

```csharp
bot.Events.DiscussMessageReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.DiscussId}/{e.UserId}: {e.MessageChain.PlainText}");
};
```

<a id="groupuploadnoticeevent"></a>

### `GroupUploadNoticeEvent` — Group file-upload notice (`notice/group_upload`)

**Subscription entry:** `bot.Events.GroupUploadNoticeReceived` (EventHandler) or `bot.Events.GroupUploadNotices` (Observable).

Reports a group file upload. `GroupId` identifies the group, `UserId` the uploader, and nullable `File` contains `Id`, `Name`, `Size` in bytes, and `BusId`.

```csharp
bot.Events.GroupUploadNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.GroupId}/{e.UserId}: {e.File?.Name} ({e.File?.Size})");
};
```

<a id="groupadminnoticeevent"></a>

### `GroupAdminNoticeEvent` — Group administrator-change notice (`notice/group_admin`)

**Subscription entry:** `bot.Events.GroupAdminNoticeReceived` (EventHandler) or `bot.Events.GroupAdminNotices` (Observable).

Reports administrator assignment or removal. `SubType` is `set` or `unset`; `GroupId` identifies the group and `UserId` the affected administrator.

```csharp
bot.Events.GroupAdminNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.UserId}");
};
```

<a id="groupdecreasenoticeevent"></a>

### `GroupDecreaseNoticeEvent` — Group member-decrease notice (`notice/group_decrease`)

**Subscription entry:** `bot.Events.GroupDecreaseNoticeReceived` (EventHandler) or `bot.Events.GroupDecreaseNotices` (Observable).

Reports a member leaving or being removed. `SubType` is `leave`, `kick`, or `kick_me`; `GroupId`, `OperatorId`, and `UserId` identify the group, operator, and departing member.

```csharp
bot.Events.GroupDecreaseNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.OperatorId}/{e.UserId}");
};
```

<a id="groupincreasenoticeevent"></a>

### `GroupIncreaseNoticeEvent` — Group member-increase notice (`notice/group_increase`)

**Subscription entry:** `bot.Events.GroupIncreaseNoticeReceived` (EventHandler) or `bot.Events.GroupIncreaseNotices` (Observable).

Reports a member joining. `SubType` is `approve` or `invite`; `GroupId`, `OperatorId`, and `UserId` identify the group, operator, and joining member.

```csharp
bot.Events.GroupIncreaseNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.OperatorId}/{e.UserId}");
};
```

<a id="groupbannoticeevent"></a>

### `GroupBanNoticeEvent` — Group mute notice (`notice/group_ban`)

**Subscription entry:** `bot.Events.GroupBanNoticeReceived` (EventHandler) or `bot.Events.GroupBanNotices` (Observable).

Reports a mute or unmute. `SubType` is `ban` or `lift_ban`; `GroupId`, `OperatorId`, and `UserId` identify the participants, and `Duration` is the mute duration in seconds.

```csharp
bot.Events.GroupBanNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.UserId}, {e.Duration}s");
};
```

<a id="friendaddnoticeevent"></a>

### `FriendAddNoticeEvent` — Friend-add notice (`notice/friend_add`)

**Subscription entry:** `bot.Events.FriendAddNoticeReceived` (EventHandler) or `bot.Events.FriendAddNotices` (Observable).

Reports a new friend. `UserId` is the newly added friend's QQ ID.

```csharp
bot.Events.FriendAddNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine(e.UserId);
};
```

<a id="friendrequestevent"></a>

### `FriendRequestEvent` — Friend request (`request/friend`)

**Subscription entry:** `bot.Events.FriendRequestReceived` (EventHandler) or `bot.Events.FriendRequests` (Observable).

Receives a friend request. `UserId` identifies the requester, `Comment` is the verification text, and non-null `Flag` should be passed unchanged to `SetFriendAddRequestAsync` when processing it.

```csharp
bot.Events.FriendRequestReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.UserId}: {e.Comment}; flag={e.Flag}");
};
```

<a id="grouprequestevent"></a>

### `GroupRequestEvent` — Group request or invitation (`request/group`)

**Subscription entry:** `bot.Events.GroupRequestReceived` (EventHandler) or `bot.Events.GroupRequests` (Observable).

Receives a join request or invitation. `SubType` is `add` or `invite`; `GroupId`, `UserId`, `Comment`, and `Flag` identify and describe the request. Preserve `Flag` for `SetGroupAddRequestAsync`.

```csharp
bot.Events.GroupRequestReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.UserId}; flag={e.Flag}");
};
```

<a id="lifecyclemetaevent"></a>

### `LifecycleMetaEvent` — Lifecycle meta event (`meta_event/lifecycle`)

**Subscription entry:** `bot.Events.LifecycleMetaEventReceived` (EventHandler) or `bot.Events.LifecycleEvents` (Observable).

Reports implementation lifecycle changes. `SubType` is normally `enable`, `disable`, or `connect`; `SelfId` identifies the bot account.

```csharp
bot.Events.LifecycleMetaEventReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SelfId}: {e.SubType}");
};
```

<a id="heartbeatmetaevent"></a>

### `HeartbeatMetaEvent` — Heartbeat meta event (`meta_event/heartbeat`)

**Subscription entry:** `bot.Events.HeartbeatMetaEventReceived` (EventHandler) or `bot.Events.Heartbeats` (Observable).

Reports periodic runtime health. `Interval` is milliseconds until the next heartbeat; nullable `Status` exposes `Online`, `Good`, and implementation-specific health fields.

```csharp
bot.Events.HeartbeatMetaEventReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"online={e.Status?.Online}, good={e.Status?.Good}, next={e.Interval}ms");
};
```

<a id="unknownonebot10event"></a>

### `UnknownOneBot10Event` — Unknown top-level event (unknown `post_type`)

**Subscription entry:** `bot.Events.UnknownEventDispatched`; use `bot.Events.UnknownEvents` for the Observable stream.

Retains an event whose top-level `PostType` is unknown. Inspect `PostType` and `RawJson`; no category-specific fields can be assumed.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    if (args.Event is UnknownOneBot10Event e)
    {
        Console.WriteLine(e.RawJson.ToJsonString());
        Console.WriteLine(e.PostType);
    }
};
```

<a id="unknownmessageevent"></a>

### `UnknownMessageEvent` — Unknown message event (`message/<unknown>`)

**Subscription entry:** `bot.Events.UnknownEventDispatched`; use `bot.Events.UnknownEvents` for the Observable stream.

Retains an unknown `MessageType` while still parsing the common message fields: `SubType`, `MessageId`, `UserId`, `MessageChain`, `RawMessage`, and `Font`.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    if (args.Event is UnknownMessageEvent e)
    {
        Console.WriteLine(e.RawJson.ToJsonString());
        Console.WriteLine($"{e.MessageType}: {e.MessageChain.PlainText}");
    }
};
```

<a id="unknownnoticeevent"></a>

### `UnknownNoticeEvent` — Unknown notice event (`notice/<unknown>`)

**Subscription entry:** `bot.Events.UnknownEventDispatched`; use `bot.Events.UnknownEvents` for the Observable stream.

Retains an unknown notice. `NoticeType` and `SubType` preserve its discriminators, and `RawJson` preserves every extension field.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    if (args.Event is UnknownNoticeEvent e)
    {
        Console.WriteLine(e.RawJson.ToJsonString());
        Console.WriteLine($"{e.NoticeType}/{e.SubType}");
    }
};
```

<a id="unknownrequestevent"></a>

### `UnknownRequestEvent` — Unknown request event (`request/<unknown>`)

**Subscription entry:** `bot.Events.UnknownEventDispatched`; use `bot.Events.UnknownEvents` for the Observable stream.

Retains an unknown request. Common fields remain available as `RequestType`, `SubType`, `UserId`, `Comment`, and `Flag`; do not process an unfamiliar request automatically.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    if (args.Event is UnknownRequestEvent e)
    {
        Console.WriteLine(e.RawJson.ToJsonString());
        Console.WriteLine($"{e.RequestType}/{e.SubType}: {e.UserId}; flag={e.Flag}");
    }
};
```

<a id="unknownmetaevent"></a>

### `UnknownMetaEvent` — Unknown meta event (`meta_event/<unknown>`)

**Subscription entry:** `bot.Events.UnknownEventDispatched`; use `bot.Events.UnknownEvents` for the Observable stream.

Retains an unknown meta event. `MetaEventType` and `SubType` preserve its discriminators, while `RawJson` contains all implementation-specific status data.

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    if (args.Event is UnknownMetaEvent e)
    {
        Console.WriteLine(e.RawJson.ToJsonString());
        Console.WriteLine($"{e.MetaEventType}/{e.SubType}");
    }
};
```

## Actions

`OneBot10Client` exposes 39 unique public method names (46 overloads): wrappers for all 37 official public base actions, plus custom-action and quick-operation entry points. Every method also accepts these trailing optional parameters; their types, meanings, nullability, and defaults are the same in every signature:

- `invocationMode` is `InvocationMode` and defaults to `InvocationMode.Normal`. `Async` appends `_async`, requests asynchronous acceptance, and does not report the eventual Action result; `RateLimited` appends `_rate_limited` and asks the implementation to queue the Action at its configured rate.
- `echo` is `JsonNode?`, defaults to `null`, and is an optional correlation value used by transports that support it. The built-in HTTP Action transport ignores it because HTTP requests do not carry a WebSocket envelope.
- `cancellationToken` is `CancellationToken`, defaults to `default` (`CancellationToken.None`), and cancels the pending transport operation.

Each completed call retains the exact transport JSON in `RawRequestJson` and `RawResponseJson`. All responses also expose `Status`, `StatusKind`, `RetCode`, `IsSuccess`, `IsAsync`, and `Echo`. The snippets below print the two raw strings directly and assume the `bot` from [Setup and startup](#setup-and-startup) is available. Replace every sample ID, flag, file name, domain, and target with a value from your own implementation. A typed response exposes parsed `Data` and the original response `data` node as `RawData`; an untyped `OneBot10Response` exposes that node as `Data`.

Raw payloads may contain message text, QQ and group IDs, event context, cookies, CSRF tokens, implementation credentials, or other sensitive extension data. Do not log them indiscriminately in production; redact sensitive fields and restrict access and retention before persisting or sharing logs.

<a id="callactionasync"></a>

### `CallActionAsync` — Call a dynamic Action (caller-supplied `action`)

**Action-specific parameters:** `action` (`string`, required and non-blank); `parameters` (`JsonObject?`, default `null`). The generic overload also requires non-null `dataParser` (`Func<JsonNode?, TData?>`).

Calls a standard or implementation-specific Action by name. The non-generic overload takes `action`, an optional `JsonObject parameters`, and the common trailing options, and returns `OneBot10Response`. The generic overload additionally requires `Func<JsonNode?, TData?> dataParser` and returns `OneBot10Response<TData>` while preserving `RawData`. `action` must not be blank, and the parser must not be `null`.

```csharp
var response = await bot.Actions.CallActionAsync(
    "implementation_extension",
    new JsonObject { ["key"] = "value" });
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The generic overload uses the supplied tolerant parser for `Data` and still preserves the unparsed node as `RawData`:

```csharp
var response = await bot.Actions.CallActionAsync<JsonNode>(
    OneBot10Actions.GetStatus,
    dataParser: node => node);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="handlequickoperationasync"></a>

### `HandleQuickOperationAsync` — Handle an event quick operation (hidden `.handle_quick_operation`)

**Action-specific parameters:** one required, non-null `context`/`operation` pair per overload. The exact `JsonObject` or strongly typed pairs are listed below; neither value has a default.

Executes the hidden `.handle_quick_operation` Action for an event delivered by HTTP POST. Its overloads are:

- `JsonObject context, JsonObject operation`: sends the complete event and operation objects as supplied.
- `PrivateMessageEvent context, PrivateMessageQuickOperation operation`: supports `Reply` and `AutoEscape`.
- `GroupMessageEvent context, GroupMessageQuickOperation operation`: supports `Reply`, `AutoEscape`, `AtSender`, `Delete`, `Kick`, `Ban`, and `BanDuration`.
- `DiscussMessageEvent context, DiscussMessageQuickOperation operation`: supports `Reply`, `AutoEscape`, and `AtSender`.
- `FriendRequestEvent context, FriendRequestQuickOperation operation`: supports `Approve` and `Remark`.
- `GroupRequestEvent context, GroupRequestQuickOperation operation`: supports `Approve` and `Reason`.

For every overload, both `context` and `operation` are required and must be non-null. In the five typed operation models, every property is nullable and decorated to be omitted from JSON when it is `null`; null therefore means “send no instruction for this field,” rather than serializing JSON `null` (the raw `JsonObject` overload sends the object you construct):

- `Reply` is `OneBot10SendMessage?`; it is the outgoing reply and null means no reply. The `bool?` properties omit their choice at null and explicitly send both `true` and `false`: `AutoEscape` controls whether a string-format reply bypasses CQ-code parsing, `AtSender` controls whether the sender is mentioned, `Delete` controls source-message recall, `Kick` controls removing the sender, and `Ban` controls muting the sender.
- `BanDuration` is `long?` seconds, matters when `Ban` is true, and is omitted at null so the implementation can use its default.
- `Approve` is `bool?`: true approves, false rejects, and null omits it and does not process the friend/group request. `Remark` and `Reason` are `string?`, are omitted at null, and apply only when approving a friend request or rejecting a group request respectively.

Quick operations can change external state: a reply sends a message, `Delete = true` recalls a message, `Kick = true` changes membership, `Ban = true` changes live mute state, and a non-null `Approve` processes a request. Review the event context before enabling any of them; recall, kick, and request-processing operations cannot be reliably undone or replayed.

Every overload returns `OneBot10Response` with no standard response data.

```csharp
async Task HandleHttpPostEventAsync(JsonObject eventContext)
{
    var response = await bot.Actions.HandleQuickOperationAsync(
        eventContext,
        new JsonObject { ["reply"] = "Received" });
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

```csharp
async Task HandlePrivateMessageAsync(PrivateMessageEvent context)
{
    var operation = new PrivateMessageQuickOperation
    {
        Reply = new OneBot10SendMessage().Text("Received"),
        AutoEscape = false
    };
    var response = await bot.Actions.HandleQuickOperationAsync(context, operation);
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

```csharp
async Task HandleGroupMessageAsync(GroupMessageEvent context)
{
    var operation = new GroupMessageQuickOperation
    {
        Reply = new OneBot10SendMessage().Text("Received"),
        AtSender = true
    };
    var response = await bot.Actions.HandleQuickOperationAsync(context, operation);
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

```csharp
async Task HandleDiscussMessageAsync(DiscussMessageEvent context)
{
    var operation = new DiscussMessageQuickOperation
    {
        Reply = new OneBot10SendMessage().Text("Received"),
        AtSender = true
    };
    var response = await bot.Actions.HandleQuickOperationAsync(context, operation);
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

```csharp
async Task HandleFriendRequestAsync(FriendRequestEvent context)
{
    var operation = new FriendRequestQuickOperation
    {
        Approve = true,
        Remark = "Friend remark"
    };
    var response = await bot.Actions.HandleQuickOperationAsync(context, operation);
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

```csharp
async Task HandleGroupRequestAsync(GroupRequestEvent context)
{
    var operation = new GroupRequestQuickOperation
    {
        Approve = false,
        Reason = "Reason for rejection"
    };
    var response = await bot.Actions.HandleQuickOperationAsync(context, operation);
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

<a id="sendprivatemessageasync"></a>

### `SendPrivateMessageAsync` — Send a private message (`send_private_msg`)

**Action-specific parameters:** `userId` (`long`); `message` (`OneBot10SendMessage`, required and non-null); `autoEscape` (`bool`, default `false`).

Sends a private message. `userId` is the recipient QQ ID, `message` is a non-null `OneBot10SendMessage`, and `autoEscape` defaults to `false`. It returns `OneBot10Response<OneBot10SendMessageResult>`; `Data?.MessageId` is the message ID assigned by the implementation.

```csharp
var response = await bot.Actions.SendPrivateMessageAsync(
    123456789,
    new OneBot10SendMessage().Text("Hello"));
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendgroupmessageasync"></a>

### `SendGroupMessageAsync` — Send a group message (`send_group_msg`)

**Action-specific parameters:** `groupId` (`long`); `message` (`OneBot10SendMessage`, required and non-null); `autoEscape` (`bool`, default `false`).

Sends a group message. `groupId` is the target group ID, `message` is a non-null outgoing message chain, and `autoEscape` defaults to `false`. It returns `OneBot10Response<OneBot10SendMessageResult>` with the sent `MessageId` in `Data`.

```csharp
var response = await bot.Actions.SendGroupMessageAsync(
    987654321,
    new OneBot10SendMessage().Text("Hello, group"));
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="senddiscussmessageasync"></a>

### `SendDiscussMessageAsync` — Send a discussion-group message (`send_discuss_msg`)

**Action-specific parameters:** `discussId` (`long`); `message` (`OneBot10SendMessage`, required and non-null); `autoEscape` (`bool`, default `false`).

Sends a discussion-group message. `discussId` identifies the discussion group, `message` is a non-null outgoing message chain, and `autoEscape` defaults to `false`. It returns `OneBot10Response<OneBot10SendMessageResult>` with the sent `MessageId` in `Data`.

```csharp
var response = await bot.Actions.SendDiscussMessageAsync(
    111222333,
    new OneBot10SendMessage().Text("Hello, discussion"));
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendmessageasync"></a>

### `SendMessageAsync` — Send a message (`send_msg`)

**Action-specific parameters:** `message` (`OneBot10SendMessage`, required and non-null); `messageType` (`OneBot10MessageType?`, default `null`); `userId`, `groupId`, and `discussId` (`long?`, each default `null`); `autoEscape` (`bool`, default `false`).

Sends a message to a conditionally selected target. `message` is required; `messageType` may be `Private`, `Group`, or `Discuss`, and the matching nullable `userId`, `groupId`, or `discussId` identifies the target. `autoEscape` defaults to `false`. Supply a compatible type and target ID. It returns `OneBot10Response<OneBot10SendMessageResult>` with `Data?.MessageId`.

```csharp
var response = await bot.Actions.SendMessageAsync(
    new OneBot10SendMessage().Text("Hello"),
    messageType: OneBot10MessageType.Group,
    groupId: 987654321);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="deletemessageasync"></a>

### `DeleteMessageAsync` — Recall a message (`delete_msg`)

**Action-specific parameter:** `messageId` (`long`).

Recalls a message. `messageId` is the OneBot message ID returned by a send Action or event. It returns `OneBot10Response`; the standard response has no data. Recalling cannot be automatically undone, so verify the ID first.

```csharp
var response = await bot.Actions.DeleteMessageAsync(messageId: 123);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendlikeasync"></a>

### `SendLikeAsync` — Send likes (`send_like`)

**Action-specific parameters:** `userId` (`long`); `times` (`long`, default `1`).

Sends likes to a friend. `userId` is the friend's QQ ID and `times` is the number of likes (default `1`; OneBot documents a maximum of ten per friend per day). It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SendLikeAsync(userId: 123456789, times: 1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The following group and discussion methods change live external state. Review the account permissions and every target before running their snippets.

<a id="setgroupkickasync"></a>

### `SetGroupKickAsync` — Remove a group member (`set_group_kick`)

**Action-specific parameters:** `groupId` and `userId` (`long`); `rejectAddRequest` (`bool`, default `false`).

Removes a member from a group. `groupId` and `userId` identify the group and member; `rejectAddRequest` defaults to `false` and controls whether future add requests from that user are rejected. It returns `OneBot10Response` with no standard response data. The membership change cannot be automatically undone.

```csharp
var response = await bot.Actions.SetGroupKickAsync(
    groupId: 987654321,
    userId: 123456789,
    rejectAddRequest: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupbanasync"></a>

### `SetGroupBanAsync` — Mute or unmute a group member (`set_group_ban`)

**Action-specific parameters:** `groupId` and `userId` (`long`); `duration` (`long` seconds, default `1800`; `0` removes the mute).

Mutes or unmutes one group member. `groupId` and `userId` identify the target; `duration` is seconds, defaults to `1800`, and `0` removes the mute. It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SetGroupBanAsync(
    groupId: 987654321,
    userId: 123456789,
    duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousbanasync"></a>

### `SetGroupAnonymousBanAsync` — Mute an anonymous group participant (`set_group_anonymous_ban`)

**Action-specific parameters:** `groupId` (`long`); either `anonymousFlag` (`string`, required and non-null) or `anonymous` (`JsonObject`, required and non-null); `duration` (`long` seconds, default `1800`).

Mutes an anonymous participant. Both overloads take `groupId` and a `duration` in seconds (default `1800`). One takes the non-null `anonymousFlag` copied from the event; the other takes the complete non-null anonymous `JsonObject`. It returns `OneBot10Response` with no standard response data. Use only data from the intended event.

```csharp
var response = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId: 987654321,
    anonymousFlag: "flag copied from the anonymous event",
    duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The complete-object overload sends the anonymous `id`, `name`, `flag`, and any implementation extension fields copied from the intended event:

```csharp
var anonymous = new JsonObject
{
    ["id"] = 10001,
    ["name"] = "Anonymous user",
    ["flag"] = "flag copied from the anonymous event"
};
var response = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId: 987654321,
    anonymous: anonymous,
    duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupwholebanasync"></a>

### `SetGroupWholeBanAsync` — Enable or disable whole-group muting (`set_group_whole_ban`)

**Action-specific parameters:** `groupId` (`long`); `enable` (`bool`, default `true`).

Enables or disables whole-group muting. `groupId` identifies the group and `enable` defaults to `true`; pass `false` to disable it. It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SetGroupWholeBanAsync(
    groupId: 987654321,
    enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupadminasync"></a>

### `SetGroupAdminAsync` — Set or remove a group administrator (`set_group_admin`)

**Action-specific parameters:** `groupId` and `userId` (`long`); `enable` (`bool`, default `true`).

Grants or revokes group administrator status. `groupId` and `userId` identify the target; `enable` defaults to `true`, while `false` revokes the role. It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SetGroupAdminAsync(
    groupId: 987654321,
    userId: 123456789,
    enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousasync"></a>

### `SetGroupAnonymousAsync` — Enable or disable anonymous group chat (`set_group_anonymous`)

**Action-specific parameters:** `groupId` (`long`); `enable` (`bool`, default `true`).

Enables or disables anonymous chat in a group. `groupId` identifies the group and `enable` defaults to `true`; pass `false` to disable it. It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SetGroupAnonymousAsync(
    groupId: 987654321,
    enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupcardasync"></a>

### `SetGroupCardAsync` — Set a group member card (`set_group_card`)

**Action-specific parameters:** `groupId` and `userId` (`long`); `card` (`string`, non-null, default empty).

Sets or removes a member's group card. `groupId` and `userId` identify the member; `card` is non-null and defaults to an empty string, which removes the card. It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SetGroupCardAsync(
    groupId: 987654321,
    userId: 123456789,
    card: "New card");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupleaveasync"></a>

### `SetGroupLeaveAsync` — Leave or dismiss a group (`set_group_leave`)

**Action-specific parameters:** `groupId` (`long`); `isDismiss` (`bool`, default `false`).

Leaves or dismisses a group. `groupId` identifies the group and `isDismiss` defaults to `false`; an owner passing `true` may irreversibly dissolve it. It returns `OneBot10Response` with no standard response data. Do not use this as a connectivity test.

```csharp
var response = await bot.Actions.SetGroupLeaveAsync(
    groupId: 987654321,
    isDismiss: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupspecialtitleasync"></a>

### `SetGroupSpecialTitleAsync` — Set a group member special title (`set_group_special_title`)

**Action-specific parameters:** `groupId` and `userId` (`long`); `specialTitle` (`string`, non-null, default empty); `duration` (`long` seconds, default `-1`).

Sets or removes a member's special title. `groupId` and `userId` identify the member; `specialTitle` is non-null and defaults to empty (remove), while `duration` is seconds and defaults to `-1` (permanent where supported). It returns `OneBot10Response` with no standard response data.

```csharp
var response = await bot.Actions.SetGroupSpecialTitleAsync(
    groupId: 987654321,
    userId: 123456789,
    specialTitle: "Title",
    duration: -1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setdiscussleaveasync"></a>

### `SetDiscussLeaveAsync` — Leave a discussion group (`set_discuss_leave`)

**Action-specific parameter:** `discussId` (`long`).

Leaves a discussion group. `discussId` identifies that discussion group. It returns `OneBot10Response` with no standard response data. This membership change cannot be automatically undone.

```csharp
var response = await bot.Actions.SetDiscussLeaveAsync(discussId: 111222333);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setfriendaddrequestasync"></a>

### `SetFriendAddRequestAsync` — Process a friend request (`set_friend_add_request`)

**Action-specific parameters:** `flag` (`string`, required and non-null); `approve` (`bool`, default `true`); `remark` (`string`, non-null, default empty).

Approves or rejects a friend request. `flag` is the non-null flag from the request event, `approve` defaults to `true`, and non-null `remark` defaults to empty and applies when approving. It returns `OneBot10Response` with no standard response data. Processing a request is externally visible and generally cannot be replayed.

```csharp
var response = await bot.Actions.SetFriendAddRequestAsync(
    flag: "flag copied from the friend-request event",
    approve: true,
    remark: "Friend remark");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupaddrequestasync"></a>

### `SetGroupAddRequestAsync` — Process a group request (`set_group_add_request`)

**Action-specific parameters:** `flag` (`string`, required and non-null); `requestType` (`OneBot10GroupRequestType`, required); `approve` (`bool`, default `true`); `reason` (`string`, non-null, default empty).

Approves or rejects a group join request or invitation. Non-null `flag` comes from the request event; `requestType` is `Add` or `Invite`; `approve` defaults to `true`; and non-null `reason` defaults to empty and is used when rejecting. It returns `OneBot10Response` with no standard response data. Processing is externally visible and cannot be reliably replayed, so verify the event flag and subtype first.

```csharp
var response = await bot.Actions.SetGroupAddRequestAsync(
    flag: "flag copied from the group-request event",
    requestType: OneBot10GroupRequestType.Add,
    approve: true,
    reason: "");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getlogininfoasync"></a>

### `GetLoginInfoAsync` — Get login account information (`get_login_info`)

**Action-specific parameters:** none; only the three common optional parameters above.

Gets the currently logged-in QQ account. It has no Action-specific parameters. It returns `OneBot10Response<OneBot10LoginInfoData>`; `Data` contains `UserId` and `Nickname`.

```csharp
var response = await bot.Actions.GetLoginInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstrangerinfoasync"></a>

### `GetStrangerInfoAsync` — Get stranger information (`get_stranger_info`)

**Action-specific parameters:** `userId` (`long`); `noCache` (`bool`, default `false`).

Gets QQ user information. `userId` is the QQ ID and `noCache` defaults to `false`; pass `true` to request fresh data. It returns `OneBot10Response<OneBot10StrangerInfoData>` with `UserId`, `Nickname`, `Sex`, and `Age` in `Data` when supplied.

```csharp
var response = await bot.Actions.GetStrangerInfoAsync(
    userId: 123456789,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfriendlistasync"></a>

### `GetFriendListAsync` — Get the friend list (`get_friend_list`)

**Action-specific parameters:** none; only the three common optional parameters above.

Gets the complete friend list and has no Action-specific parameters. It returns `OneBot10Response<IReadOnlyList<OneBot10FriendInfo>>`; each `Data` item can contain `UserId`, `Nickname`, and `Remark`.

```csharp
var response = await bot.Actions.GetFriendListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouplistasync"></a>

### `GetGroupListAsync` — Get the group list (`get_group_list`)

**Action-specific parameters:** none; only the three common optional parameters above.

Gets the complete group list and has no Action-specific parameters. It returns `OneBot10Response<IReadOnlyList<OneBot10GroupListItem>>`; each `Data` item contains the parsed `GroupId` and `GroupName` when present.

```csharp
var response = await bot.Actions.GetGroupListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupinfoasync"></a>

### `GetGroupInfoAsync` — Get group information (`get_group_info`)

**Action-specific parameters:** `groupId` (`long`); `noCache` (`bool`, default `false`).

Gets information about one group. `groupId` identifies it and `noCache` defaults to `false`; pass `true` to request fresh data. It returns `OneBot10Response<OneBot10GroupInfo>` with `GroupId`, `GroupName`, `MemberCount`, and `MaxMemberCount` in `Data` when present.

```csharp
var response = await bot.Actions.GetGroupInfoAsync(
    groupId: 987654321,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberinfoasync"></a>

### `GetGroupMemberInfoAsync` — Get group member information (`get_group_member_info`)

**Action-specific parameters:** `groupId` and `userId` (`long`); `noCache` (`bool`, default `false`).

Gets detailed information about one member. `groupId` and `userId` identify the member, and `noCache` defaults to `false`. It returns `OneBot10Response<OneBot10GroupMemberInfo>`; `Data` includes IDs, nickname, card, role, title, timestamps, and other member fields when supplied.

```csharp
var response = await bot.Actions.GetGroupMemberInfoAsync(
    groupId: 987654321,
    userId: 123456789,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberlistasync"></a>

### `GetGroupMemberListAsync` — Get the group member list (`get_group_member_list`)

**Action-specific parameter:** `groupId` (`long`).

Gets a group's member list. `groupId` identifies the group. It returns `OneBot10Response<IReadOnlyList<OneBot10GroupMemberInfo>>`; some fields on each `Data` item may be absent depending on the implementation.

```csharp
var response = await bot.Actions.GetGroupMemberListAsync(groupId: 987654321);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcookiesasync"></a>

### `GetCookiesAsync` — Get cookies (`get_cookies`)

**Action-specific parameter:** `domain` (`string`, non-null, default empty).

Gets QQ cookies. Non-null `domain` optionally restricts them to a domain and defaults to an empty string. It returns `OneBot10Response<OneBot10CookiesData>` with the cookie string in `Data?.Cookies`.

```csharp
var response = await bot.Actions.GetCookiesAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcsrftokenasync"></a>

### `GetCsrfTokenAsync` — Get the CSRF token (`get_csrf_token`)

**Action-specific parameters:** none; only the three common optional parameters above.

Gets the QQ CSRF token and has no Action-specific parameters. It returns `OneBot10Response<OneBot10CsrfTokenData>` with the numeric token in `Data?.Token`.

```csharp
var response = await bot.Actions.GetCsrfTokenAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcredentialsasync"></a>

### `GetCredentialsAsync` — Get cookies and CSRF credentials (`get_credentials`)

**Action-specific parameter:** `domain` (`string`, non-null, default empty).

Gets cookies and the CSRF token together. Non-null `domain` optionally restricts the cookies and defaults to empty. It returns `OneBot10Response<OneBot10CredentialsData>` with `Cookies` and `CsrfToken` in `Data`.

```csharp
var response = await bot.Actions.GetCredentialsAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getrecordasync"></a>

### `GetRecordAsync` — Get or convert a received audio file (`get_record`)

**Action-specific parameters:** `file` (`string`, required and non-null); `outputFormat` (`OneBot10RecordFormat`, required); `fullPath` (`bool`, default `false`).

Gets and converts a received audio file. `file` is the non-null file name from a received segment; `outputFormat` is `Mp3`, `Amr`, `Wma`, `M4a`, `Spx`, `Ogg`, `Wav`, or `Flac`; `fullPath` defaults to `false`. It returns `OneBot10Response<OneBot10FileData>` with the resulting path in `Data?.File`.

```csharp
var response = await bot.Actions.GetRecordAsync(
    file: "received-audio-file-name",
    outputFormat: OneBot10RecordFormat.Mp3,
    fullPath: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getimageasync"></a>

### `GetImageAsync` — Get a received image file (`get_image`)

**Action-specific parameter:** `file` (`string`, required and non-null).

Gets a received image file. `file` is the non-null file name from the received image segment. It returns `OneBot10Response<OneBot10FileData>` with the resulting path in `Data?.File`.

```csharp
var response = await bot.Actions.GetImageAsync(file: "received-image-file-name");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendimageasync"></a>

### `CanSendImageAsync` — Check image sending capability (`can_send_image`)

**Action-specific parameters:** none; only the three common optional parameters above.

Checks whether the implementation can send images and has no Action-specific parameters. It returns `OneBot10Response<OneBot10CapabilityData>`; `Data?.Yes` is the reported capability.

```csharp
var response = await bot.Actions.CanSendImageAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendrecordasync"></a>

### `CanSendRecordAsync` — Check audio sending capability (`can_send_record`)

**Action-specific parameters:** none; only the three common optional parameters above.

Checks whether the implementation can send audio/record messages and has no Action-specific parameters. It returns `OneBot10Response<OneBot10CapabilityData>`; `Data?.Yes` is the reported capability.

```csharp
var response = await bot.Actions.CanSendRecordAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstatusasync"></a>

### `GetStatusAsync` — Get implementation status (`get_status`)

**Action-specific parameters:** none; only the three common optional parameters above.

Gets implementation health information and has no Action-specific parameters. It returns `OneBot10Response<OneBot10StatusData>`; portable fields include `Data?.Online` and `Data?.Good`, while CQHTTP-specific status fields are also retained when supplied.

```csharp
var response = await bot.Actions.GetStatusAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getversioninfoasync"></a>

### `GetVersionInfoAsync` — Get implementation version information (`get_version_info`)

**Action-specific parameters:** none; only the three common optional parameters above.

Gets CQHTTP plug-in and CKYU host version information and has no Action-specific parameters. It returns `OneBot10Response<OneBot10VersionInfoData>` with directory, edition, plug-in version, build number, and build configuration fields when supplied.

```csharp
var response = await bot.Actions.GetVersionInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setrestartpluginasync"></a>

### `SetRestartPluginAsync` — Restart the CQHTTP plug-in (`set_restart_plugin`)

**Action-specific parameter:** `delay` (`long` milliseconds, default `0`).

Restarts the CQHTTP plug-in. `delay` is the implementation's restart delay in milliseconds and defaults to `0`. It returns `OneBot10Response` with no standard response data if a response arrives; restarting may interrupt the connection before that happens. This is not a connectivity test.

```csharp
var response = await bot.Actions.SetRestartPluginAsync(delay: 2000);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cleandatadirectoryasync"></a>

### `CleanDataDirectoryAsync` — Clean a CQHTTP data directory (`clean_data_dir`)

**Action-specific parameter:** `dataDirectory` (`OneBot10DataDirectory`, required).

Permanently deletes files from one CQHTTP data directory. `dataDirectory` must be `Image`, `Record`, `Show`, or `Bface`. It returns `OneBot10Response` with no standard response data. Review the selected directory before calling it; the deletion cannot be automatically undone.

```csharp
var response = await bot.Actions.CleanDataDirectoryAsync(
    OneBot10DataDirectory.Image);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cleanpluginlogasync"></a>

### `CleanPluginLogAsync` — Clean the CQHTTP plug-in log (`clean_plugin_log`)

**Action-specific parameters:** none; only the three common optional parameters above.

Permanently clears the CQHTTP plug-in log. It has no Action-specific parameters and returns `OneBot10Response` with no standard response data. This removes diagnostic history and cannot be automatically undone.

```csharp
var response = await bot.Actions.CleanPluginLogAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

## Message chains

`OneBot10SendMessage` is an ordered outgoing message-chain builder. This example combines text and an image:

```csharp
var message = new OneBot10SendMessage()
    .Text("Image below:")
    .Image("https://example.com/cat.png");

var response = await bot.Actions.SendGroupMessageAsync(
    groupId: 123456789,
    message: message);
```

Incoming messages use `OneBot10ReceivedMessage`. Read concrete segments with `MessageChain.OfType<T>()`; unknown segments become `UnknownReceivedSegment` and retain their raw JSON.

## Supported message segments

Each snippet below creates a segment that can be added to `OneBot10SendMessage`.

### `text`

```csharp
var segment = new TextSendSegment("Hello");
```

`text` is the content. It may be empty but not `null`.

### `face`

```csharp
var segment = new FaceSendSegment(14L);
```

`id` is the protocol face ID and may be a `long` or string.

### `image`

```csharp
var segment = new ImageSendSegment(
    file: "https://example.com/cat.png",
    cache: true,
    timeoutSeconds: 30);
```

`file` may be a received file name, file URI, network URL, or `base64://` URI. `cache` controls implementation caching, and `timeoutSeconds` is the download timeout.

### `record`

```csharp
var segment = new RecordSendSegment(
    file: "file:///D:/audio/test.amr",
    magic: false,
    cache: true,
    timeoutSeconds: 30);
```

`file` is the audio source; `magic` requests a voice effect. The remaining parameters match `image`.

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
var friend = new ContactSendSegment(OneBot10ContactTarget.Friend, "123456789");
var group = new ContactSendSegment(OneBot10ContactTarget.Group, "987654321");
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
var providerMusic = new MusicSendSegment(OneBot10MusicProvider.QQ, "song-id");
var customMusic = new CustomMusicSendSegment(
    url: "https://example.com/song",
    audio: "https://example.com/song.mp3",
    title: "Song title",
    content: "Optional description",
    image: "https://example.com/cover.png");
```

Provider music accepts `QQ`, `NetEase`, or `Xiami` plus a song ID. Custom music requires a page URL, audio URL, and title.

### `rich` (receive-only)

```csharp
foreach (var rich in messageEvent.MessageChain.OfType<RichReceivedSegment>())
    Console.WriteLine(rich.Data);
```

The standard defines no fixed parameters; inspect `Data` or `RawJson`.

### Implementation extensions

```csharp
var segment = new CustomSendSegment(
    "markdown",
    new JsonObject { ["content"] = "**Hello**" });
```

`type` and `data` follow the implementation's contract. Unknown incoming types are retained as `UnknownReceivedSegment`.


## Console debugging projects

These runnable projects are optional debugging tools. Use the Actions and Receiving events sections above as the API reference.

- [Observable sample](../samples/OneBotSdk.Net.V10.ObservableExample)
- [EventHandler sample](../samples/OneBotSdk.Net.V10.EventHandlerExample)
- [HTTP Action sample](../samples/OneBotSdk.Net.V10.HttpActionExample)

Do not place tokens in source code or logs. Prefer HTTPS/WSS in production.
