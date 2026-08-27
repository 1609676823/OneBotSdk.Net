# OneBot 12 candidate usage guide

[README](../README.md) | [Documentation index](README.md) | [简体中文](onebot-12.zh-CN.md)

> **Official OneBot 12 candidate specification:** [source repository](https://github.com/botuniverse/onebot) · [published documentation](https://12.onebot.dev/)

The OneBot 12 API lives under `OneBotSdk.Net.V12.*`. It currently covers 31 standard protocol actions, 40 public Action method names through 45 overloads, 19 standard concrete events plus 5 unknown fallbacks, and 10 standard message-segment types. OneBot 12 remains a candidate specification and is not compatible with OneBot 10 or 11 implementations.

## API directory

- **[Actions](#actions) — low-level and meta:** [`CallActionAsync`](#callactionasync), [`GetLatestEventsAsync`](#getlatesteventsasync), [`GetSupportedActionsAsync`](#getsupportedactionsasync), [`GetStatusAsync`](#getstatusasync), [`GetVersionAsync`](#getversionasync)
- **Actions — messages and users:** [`SendMessageAsync`](#sendmessageasync), [`SendPrivateMessageAsync`](#sendprivatemessageasync), [`SendGroupMessageAsync`](#sendgroupmessageasync), [`SendChannelMessageAsync`](#sendchannelmessageasync), [`DeleteMessageAsync`](#deletemessageasync), [`GetSelfInfoAsync`](#getselfinfoasync), [`GetUserInfoAsync`](#getuserinfoasync), [`GetFriendListAsync`](#getfriendlistasync)
- **Actions — groups:** [`GetGroupInfoAsync`](#getgroupinfoasync), [`GetGroupListAsync`](#getgrouplistasync), [`GetGroupMemberInfoAsync`](#getgroupmemberinfoasync), [`GetGroupMemberListAsync`](#getgroupmemberlistasync), [`SetGroupNameAsync`](#setgroupnameasync), [`LeaveGroupAsync`](#leavegroupasync)
- **Actions — guilds and channels:** [`GetGuildInfoAsync`](#getguildinfoasync), [`GetGuildListAsync`](#getguildlistasync), [`SetGuildNameAsync`](#setguildnameasync), [`GetGuildMemberInfoAsync`](#getguildmemberinfoasync), [`GetGuildMemberListAsync`](#getguildmemberlistasync), [`LeaveGuildAsync`](#leaveguildasync), [`GetChannelInfoAsync`](#getchannelinfoasync), [`GetChannelListAsync`](#getchannellistasync), [`SetChannelNameAsync`](#setchannelnameasync), [`GetChannelMemberInfoAsync`](#getchannelmemberinfoasync), [`GetChannelMemberListAsync`](#getchannelmemberlistasync), [`LeaveChannelAsync`](#leavechannelasync)
- **Actions — files:** [`UploadFileAsync`](#uploadfileasync), [`UploadFileFragmentedAsync`](#uploadfilefragmentedasync), [`PrepareUploadFileFragmentedAsync`](#prepareuploadfilefragmentedasync), [`TransferUploadFileFragmentAsync`](#transferuploadfilefragmentasync), [`FinishUploadFileFragmentedAsync`](#finishuploadfilefragmentedasync), [`GetFileAsync`](#getfileasync), [`GetFileFragmentedAsync`](#getfilefragmentedasync), [`PrepareGetFileFragmentedAsync`](#preparegetfilefragmentedasync), [`GetFileFragmentAsync`](#getfilefragmentasync)
- **[Receiving events](#receiving-events) — messages:** [`PrivateMessageEvent`](#privatemessageevent), [`GroupMessageEvent`](#groupmessageevent), [`ChannelMessageEvent`](#channelmessageevent)
- **Receiving events — notices:** [`FriendIncreaseNoticeEvent`](#friendincreasenoticeevent), [`FriendDecreaseNoticeEvent`](#frienddecreasenoticeevent), [`PrivateMessageDeleteNoticeEvent`](#privatemessagedeletenoticeevent), [`GroupMemberIncreaseNoticeEvent`](#groupmemberincreasenoticeevent), [`GroupMemberDecreaseNoticeEvent`](#groupmemberdecreasenoticeevent), [`GroupMessageDeleteNoticeEvent`](#groupmessagedeletenoticeevent), [`GuildMemberIncreaseNoticeEvent`](#guildmemberincreasenoticeevent), [`GuildMemberDecreaseNoticeEvent`](#guildmemberdecreasenoticeevent), [`ChannelMemberIncreaseNoticeEvent`](#channelmemberincreasenoticeevent), [`ChannelMemberDecreaseNoticeEvent`](#channelmemberdecreasenoticeevent), [`ChannelMessageDeleteNoticeEvent`](#channelmessagedeletenoticeevent), [`ChannelCreateNoticeEvent`](#channelcreatenoticeevent), [`ChannelDeleteNoticeEvent`](#channeldeletenoticeevent)
- **Receiving events — meta and fallbacks:** [`ConnectMetaEvent`](#connectmetaevent), [`HeartbeatMetaEvent`](#heartbeatmetaevent), [`StatusUpdateMetaEvent`](#statusupdatemetaevent), [`UnknownOneBot12Event`](#unknownonebot12event), [`UnknownMessageEvent`](#unknownmessageevent), [`UnknownNoticeEvent`](#unknownnoticeevent), [`UnknownRequestEvent`](#unknownrequestevent), [`UnknownMetaEvent`](#unknownmetaevent)

## Setup and startup

```csharp
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OneBotSdk.Net.V12;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Messages;

var options = new OneBot12BotOptions(
    new OneBot12ActionEndpointOptions("127.0.0.1", 3000, "ActionToken"),
    new OneBot12EventEndpointOptions("127.0.0.1", 3001, "EventToken"),
    new OneBot12Self("platform-name", "bot-user-id"));

using var bot = new OneBot12Bot(options);
```

OneBot 12 IDs are strings. Action and Event addresses and tokens are configured separately. Subscribe before calling `StartAsync()`.

### EventHandler

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    var message = args.Event.Message;
    if (message == null)
        return;

    foreach (var text in message.OfType<OneBot12TextReceivedSegment>())
        Console.WriteLine("Text: " + text.Text);

    foreach (var image in message.OfType<OneBot12ImageReceivedSegment>())
        Console.WriteLine("Image file_id: " + image.FileId);
};
```

### Observable

```csharp
using var subscription = bot.MessageReceived
    .OfType<PrivateMessageEvent>()
    .Subscribe(message => Console.WriteLine(message.Message?.PlainText));
```

### Start listening

```csharp
var start = await bot.StartAsync();
Console.WriteLine($"Implementation: {start.VersionResponse.Data?.Impl}");
Console.WriteLine("Press Enter to exit.");
Console.ReadLine();
```

The console sample does not need `ManualResetEvent`. If you use one, dispose its operating-system wait handle; a `using var exit = new ManualResetEvent(false);` declaration avoids nesting. Hosted services should use their host cancellation token.

## Receiving events

Subscribe before `StartAsync()`. The snippets below use the typed `EventHandler` endpoints; the same dispatcher also exposes matching hot `IObservable<T>` streams such as `PrivateMessages`, `FriendIncreaseNotices`, and `Heartbeats`. Keep the returned `IDisposable` when using an Observable and dispose it when the subscription is no longer needed.

Every event inherits these field-tolerant, nullable properties from `OneBot12Event`: `Id` (`string?`, globally unique event ID), `Time` (`double?`, Unix seconds), `Type` (`string?`), `DetailType` (`string?`), `SubType` (`string?`), and `Self` (`OneBot12Self?`, omitted on meta events). `RawJson` is a detached `JsonObject` containing the complete received event, including implementation extensions. Each minimal handler below prints that original event object directly.

The first 19 sections are the standard concrete types. The final 5 are concrete fallback types, so an unknown extension is still observable instead of being discarded.

<a id="privatemessageevent"></a>

### `PrivateMessageEvent` — Private message (`message/private`)

Subscription entry: `bot.Events.PrivateMessageReceived` (hot stream: `bot.Events.PrivateMessages`).

Receives a `message/private` event. Key fields are `MessageId`, `UserId`, parsed `Message`, and textual `AltMessage`.

```csharp
bot.Events.PrivateMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmessageevent"></a>

### `GroupMessageEvent` — Group message (`message/group`)

Subscription entry: `bot.Events.GroupMessageReceived` (hot stream: `bot.Events.GroupMessages`).

Receives a `message/group` event. It adds `GroupId` to the common message fields `MessageId`, `UserId`, `Message`, and `AltMessage`.

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmessageevent"></a>

### `ChannelMessageEvent` — Channel message (`message/channel`)

Subscription entry: `bot.Events.ChannelMessageReceived` (hot stream: `bot.Events.ChannelMessages`).

Receives a `message/channel` event. Key fields are `GuildId`, `ChannelId`, `MessageId`, `UserId`, `Message`, and `AltMessage`.

```csharp
bot.Events.ChannelMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="friendincreasenoticeevent"></a>

### `FriendIncreaseNoticeEvent` — Friend-added notice (`notice/friend_increase`)

Subscription entry: `bot.Events.FriendIncreaseNoticeReceived` (hot stream: `bot.Events.FriendIncreaseNotices`).

Reports a newly added friend. `UserId` identifies that friend.

```csharp
bot.Events.FriendIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="frienddecreasenoticeevent"></a>

### `FriendDecreaseNoticeEvent` — Friend-removed notice (`notice/friend_decrease`)

Subscription entry: `bot.Events.FriendDecreaseNoticeReceived` (hot stream: `bot.Events.FriendDecreaseNotices`).

Reports a removed friend. `UserId` identifies the removed friend.

```csharp
bot.Events.FriendDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="privatemessagedeletenoticeevent"></a>

### `PrivateMessageDeleteNoticeEvent` — Private-message deletion notice (`notice/private_message_delete`)

Subscription entry: `bot.Events.PrivateMessageDeleteNoticeReceived` (hot stream: `bot.Events.PrivateMessageDeleteNotices`).

Reports a deleted or recalled private message. `MessageId` identifies the message and `UserId` identifies the peer.

```csharp
bot.Events.PrivateMessageDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmemberincreasenoticeevent"></a>

### `GroupMemberIncreaseNoticeEvent` — Group-member increase notice (`notice/group_member_increase`)

Subscription entry: `bot.Events.GroupMemberIncreaseNoticeReceived` (hot stream: `bot.Events.GroupMemberIncreaseNotices`).

Reports a member joining a single-level group. Key fields are `GroupId`, joining `UserId`, and `OperatorId`.

```csharp
bot.Events.GroupMemberIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmemberdecreasenoticeevent"></a>

### `GroupMemberDecreaseNoticeEvent` — Group-member decrease notice (`notice/group_member_decrease`)

Subscription entry: `bot.Events.GroupMemberDecreaseNoticeReceived` (hot stream: `bot.Events.GroupMemberDecreaseNotices`).

Reports a member leaving a single-level group. Key fields are `GroupId`, departing `UserId`, and `OperatorId`.

```csharp
bot.Events.GroupMemberDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmessagedeletenoticeevent"></a>

### `GroupMessageDeleteNoticeEvent` — Group-message deletion notice (`notice/group_message_delete`)

Subscription entry: `bot.Events.GroupMessageDeleteNoticeReceived` (hot stream: `bot.Events.GroupMessageDeleteNotices`).

Reports a deleted or recalled group message. Key fields are `GroupId`, `MessageId`, author `UserId`, and `OperatorId`.

```csharp
bot.Events.GroupMessageDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="guildmemberincreasenoticeevent"></a>

### `GuildMemberIncreaseNoticeEvent` — Guild-member increase notice (`notice/guild_member_increase`)

Subscription entry: `bot.Events.GuildMemberIncreaseNoticeReceived` (hot stream: `bot.Events.GuildMemberIncreaseNotices`).

Reports a member joining a two-level guild. Key fields are `GuildId`, joining `UserId`, and `OperatorId`.

```csharp
bot.Events.GuildMemberIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="guildmemberdecreasenoticeevent"></a>

### `GuildMemberDecreaseNoticeEvent` — Guild-member decrease notice (`notice/guild_member_decrease`)

Subscription entry: `bot.Events.GuildMemberDecreaseNoticeReceived` (hot stream: `bot.Events.GuildMemberDecreaseNotices`).

Reports a member leaving a two-level guild. Key fields are `GuildId`, departing `UserId`, and `OperatorId`.

```csharp
bot.Events.GuildMemberDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmemberincreasenoticeevent"></a>

### `ChannelMemberIncreaseNoticeEvent` — Channel-member increase notice (`notice/channel_member_increase`)

Subscription entry: `bot.Events.ChannelMemberIncreaseNoticeReceived` (hot stream: `bot.Events.ChannelMemberIncreaseNotices`).

Reports a member joining a channel. Key fields are `GuildId`, `ChannelId`, joining `UserId`, and `OperatorId`.

```csharp
bot.Events.ChannelMemberIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmemberdecreasenoticeevent"></a>

### `ChannelMemberDecreaseNoticeEvent` — Channel-member decrease notice (`notice/channel_member_decrease`)

Subscription entry: `bot.Events.ChannelMemberDecreaseNoticeReceived` (hot stream: `bot.Events.ChannelMemberDecreaseNotices`).

Reports a member leaving a channel. Key fields are `GuildId`, `ChannelId`, departing `UserId`, and `OperatorId`.

```csharp
bot.Events.ChannelMemberDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmessagedeletenoticeevent"></a>

### `ChannelMessageDeleteNoticeEvent` — Channel-message deletion notice (`notice/channel_message_delete`)

Subscription entry: `bot.Events.ChannelMessageDeleteNoticeReceived` (hot stream: `bot.Events.ChannelMessageDeleteNotices`).

Reports a deleted or recalled channel message. Key fields are `GuildId`, `ChannelId`, `MessageId`, author `UserId`, and `OperatorId`.

```csharp
bot.Events.ChannelMessageDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelcreatenoticeevent"></a>

### `ChannelCreateNoticeEvent` — Channel-created notice (`notice/channel_create`)

Subscription entry: `bot.Events.ChannelCreateNoticeReceived` (hot stream: `bot.Events.ChannelCreateNotices`).

Reports channel creation. `GuildId` identifies the containing guild, `ChannelId` the new channel, and `OperatorId` the operator.

```csharp
bot.Events.ChannelCreateNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channeldeletenoticeevent"></a>

### `ChannelDeleteNoticeEvent` — Channel-deleted notice (`notice/channel_delete`)

Subscription entry: `bot.Events.ChannelDeleteNoticeReceived` (hot stream: `bot.Events.ChannelDeleteNotices`).

Reports channel deletion. `GuildId` identifies the former containing guild, `ChannelId` the deleted channel, and `OperatorId` the operator.

```csharp
bot.Events.ChannelDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="connectmetaevent"></a>

### `ConnectMetaEvent` — Connection-established meta event (`meta/connect`)

Subscription entry: `bot.Events.ConnectMetaEventReceived` (hot stream: `bot.Events.ConnectEvents`).

Receives the first event on a successful WebSocket connection. `Version` is a nullable `OneBot12VersionData` containing `Impl`, `Version`, and `OneBotVersion`.

```csharp
bot.Events.ConnectMetaEventReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="heartbeatmetaevent"></a>

### `HeartbeatMetaEvent` — Heartbeat meta event (`meta/heartbeat`)

Subscription entry: `bot.Events.HeartbeatMetaEventReceived` (hot stream: `bot.Events.Heartbeats`).

Receives a periodic heartbeat. `Interval` is the nullable `long` interval until the next heartbeat, in milliseconds.

```csharp
bot.Events.HeartbeatMetaEventReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="statusupdatemetaevent"></a>

### `StatusUpdateMetaEvent` — Status-update meta event (`meta/status_update`)

Subscription entry: `bot.Events.StatusUpdateMetaEventReceived` (hot stream: `bot.Events.StatusUpdates`).

Receives an implementation or bot status change. `Status` is a nullable `OneBot12StatusData` with overall `Good` state and per-account `Bots` entries.

```csharp
bot.Events.StatusUpdateMetaEventReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="unknownonebot12event"></a>

### `UnknownOneBot12Event` — Unknown top-level event (`<unknown>/*` fallback)

Subscription entry: `bot.UnknownEventReceived.OfType<UnknownOneBot12Event>()` (all-unknown EventHandler: `bot.Events.UnknownEventDispatched`).

Preserves an unrecognized top-level `type`. Only the common fields are projected; inspect `RawJson` for extension data.

```csharp
using var unknownEventSubscription = bot.UnknownEventReceived
    .OfType<UnknownOneBot12Event>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownmessageevent"></a>

### `UnknownMessageEvent` — Unknown message detail (`message/<unknown>` fallback)

Subscription entry: `bot.UnknownEventReceived.OfType<UnknownMessageEvent>()` (all-unknown EventHandler: `bot.Events.UnknownEventDispatched`).

Preserves a `message` event whose `detail_type` is unknown. It still projects `MessageId`, `UserId`, `Message`, and `AltMessage`; extension fields remain in `RawJson`.

```csharp
using var unknownMessageSubscription = bot.UnknownEventReceived
    .OfType<UnknownMessageEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownnoticeevent"></a>

### `UnknownNoticeEvent` — Unknown notice detail (`notice/<unknown>` fallback)

Subscription entry: `bot.UnknownEventReceived.OfType<UnknownNoticeEvent>()` (all-unknown EventHandler: `bot.Events.UnknownEventDispatched`).

Preserves a `notice` event whose `detail_type` is unknown. Use the common discriminators and `RawJson` to handle implementation-defined fields.

```csharp
using var unknownNoticeSubscription = bot.UnknownEventReceived
    .OfType<UnknownNoticeEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownrequestevent"></a>

### `UnknownRequestEvent` — Reserved request event (`request/*` fallback)

Subscription entry: `bot.UnknownEventReceived.OfType<UnknownRequestEvent>()` (all-unknown EventHandler: `bot.Events.UnknownEventDispatched`).

Preserves every `request` event. OneBot 12 currently reserves this category without standard detail types, so inspect `DetailType`, `SubType`, and `RawJson`.

```csharp
using var unknownRequestSubscription = bot.UnknownEventReceived
    .OfType<UnknownRequestEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownmetaevent"></a>

### `UnknownMetaEvent` — Unknown meta detail (`meta/<unknown>` fallback)

Subscription entry: `bot.UnknownEventReceived.OfType<UnknownMetaEvent>()` (all-unknown EventHandler: `bot.Events.UnknownEventDispatched`).

Preserves a `meta` event whose `detail_type` is unknown. Use the common discriminators and `RawJson` for extension fields.

```csharp
using var unknownMetaSubscription = bot.UnknownEventReceived
    .OfType<UnknownMetaEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

## Actions

`OneBot12Client` exposes 40 public action method names through 45 overloads. The reference below covers every one. Each snippet assumes that `bot` was created as shown above and prints the exact JSON request and response texts without an output helper.

Every response has `Status`, `RetCode`, `Message`, `Echo`, and `IsSuccess`, plus `RawRequestJson` and `RawResponseJson`. A typed `OneBot12Response<TData>` exposes parsed `Data` and unprojected `RawData`; an untyped `OneBot12Response` exposes its raw JSON `Data` directly.

Raw packets can contain private messages, file data, local paths, URLs, headers, and other sensitive values. Print them only in a controlled diagnostic environment and redact them before sharing logs.

Unless stated otherwise, methods end with these optional parameters:

- `echo` (`string?`, default `null`): an optional correlation string copied into the request and normally echoed by the implementation.
- `self` (`OneBot12Self?`, default `null`): the optional bot identity used for the action. Omitting it uses the identity from `OneBot12BotOptions`.
- `cancellationToken` (`CancellationToken`, default value): cancels the transport operation.

The four meta methods do not accept or send `self`. All OneBot 12 IDs are non-empty strings. OneBot 12 does not use the OneBot 10/11 `_async` or `_rate_limited` invocation modes.

**Low-level and extension actions**

<a id="callactionasync"></a>

### `CallActionAsync` — Call a dynamic Action (`action` supplied at runtime)

Calls any standard or implementation-defined action. The untyped overload takes a non-empty `action`, `JsonObject? parameters = null`, and the common `echo`, `self`, and `cancellationToken` arguments. It returns `OneBot12Response`, whose `Data` is the action's raw JSON data.

```csharp
var response = await bot.Actions.CallActionAsync(
    "implementation_extension",
    new JsonObject { ["key"] = "value" },
    echo: "extension-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The generic overload additionally requires a non-null `Func<JsonNode?, TData?> dataParser` immediately after `action`. It returns `OneBot12Response<TData>` with parsed `Data` and raw `RawData`.

```csharp
var response = await bot.Actions.CallActionAsync<JsonNode>(
    "implementation_extension",
    node => node,
    new JsonObject { ["key"] = "value" },
    echo: "extension-typed-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Meta actions**

<a id="getlatesteventsasync"></a>

### `GetLatestEventsAsync` — Poll the latest events (`get_latest_events`)

Polls buffered non-meta events over HTTP. `limit` is a non-negative `long` that defaults to `0`, meaning no event-count limit. `timeoutSeconds` is a non-negative `long` that also defaults to `0`, meaning short polling with no wait. The remaining optional parameters are `echo` and `cancellationToken`. `Data` is an `IReadOnlyList<OneBot12Event>`; every event retains extension fields.

```csharp
var response = await bot.Actions.GetLatestEventsAsync(
    limit: 100,
    timeoutSeconds: 30,
    echo: "latest-events-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getsupportedactionsasync"></a>

### `GetSupportedActionsAsync` — List supported Actions (`get_supported_actions`)

Gets the action names advertised by the implementation. It accepts optional `echo` and `cancellationToken`. `Data` is an `IReadOnlyList<string>` of action names.

```csharp
var response = await bot.Actions.GetSupportedActionsAsync(echo: "supported-actions-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstatusasync"></a>

### `GetStatusAsync` — Get runtime status (`get_status`)

Gets implementation-wide and per-bot runtime status. It accepts optional `echo` and `cancellationToken`. `Data` is `OneBot12StatusData`, including `Good` and the `Bots` collection; each bot entry contains `Self` and `Online`.

```csharp
var response = await bot.Actions.GetStatusAsync(echo: "status-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getversionasync"></a>

### `GetVersionAsync` — Get version information (`get_version`)

Gets implementation and protocol version information. It accepts optional `echo` and `cancellationToken`. `Data` is `OneBot12VersionData`, including `Impl`, `Version`, and `OneBotVersion`.

```csharp
var response = await bot.Actions.GetVersionAsync(echo: "version-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Message actions**

<a id="sendmessageasync"></a>

### `SendMessageAsync` — Send a message (`send_message`)

Sends a message to a standard or implementation-defined destination. Required parameters are non-empty `string detailType` and non-null `OneBot12SendMessage message`. `userId`, `groupId`, `guildId`, and `channelId` are `string?` values defaulting to `null`: `private` requires `userId`, `group` requires `groupId`, and `channel` requires both `guildId` and `channelId`. Extension detail types may use whichever optional IDs their implementation defines. The common optional arguments follow them. `Data` is `OneBot12SendMessageData`, containing `MessageId` and `Time` when supplied by the implementation.

```csharp
var message = new OneBot12MessageChain().Text("Hello");
var response = await bot.Actions.SendMessageAsync(
    detailType: "group",
    message: message,
    groupId: "group-id",
    echo: "send-message-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendprivatemessageasync"></a>

### `SendPrivateMessageAsync` — Send a private message (`send_message`, `detail_type=private`)

Convenience wrapper for `send_message` with `detail_type` set to `private`. Pass a non-empty `userId`, a non-null `OneBot12SendMessage message`, and optionally `echo`, `self`, and `cancellationToken`. `Data` is `OneBot12SendMessageData` with the returned message ID and time.

```csharp
var message = new OneBot12MessageChain().Text("Hello");
var response = await bot.Actions.SendPrivateMessageAsync(
    "user-id", message, echo: "send-private-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendgroupmessageasync"></a>

### `SendGroupMessageAsync` — Send a group message (`send_message`, `detail_type=group`)

Convenience wrapper for `send_message` with `detail_type` set to `group`. Pass a non-empty `groupId`, a non-null `OneBot12SendMessage message`, and the common optional arguments. `Data` is `OneBot12SendMessageData` with the returned message ID and time.

```csharp
var message = new OneBot12MessageChain().Text("Hello group");
var response = await bot.Actions.SendGroupMessageAsync(
    "group-id", message, echo: "send-group-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendchannelmessageasync"></a>

### `SendChannelMessageAsync` — Send a channel message (`send_message`, `detail_type=channel`)

Convenience wrapper for `send_message` with `detail_type` set to `channel`. Pass non-empty `guildId` and `channelId`, a non-null `OneBot12SendMessage message`, and the common optional arguments. `Data` is `OneBot12SendMessageData` with the returned message ID and time.

```csharp
var message = new OneBot12MessageChain().Text("Hello channel");
var response = await bot.Actions.SendChannelMessageAsync(
    "guild-id", "channel-id", message, echo: "send-channel-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="deletemessageasync"></a>

### `DeleteMessageAsync` — Delete or recall a message (`delete_message`)

Deletes or recalls one message. Pass a non-empty `messageId` and the common optional arguments. It returns an untyped `OneBot12Response`; use the envelope fields to determine success. This operation changes external state.

```csharp
var response = await bot.Actions.DeleteMessageAsync(
    "message-id", echo: "delete-message-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**User actions**

<a id="getselfinfoasync"></a>

### `GetSelfInfoAsync` — Get bot account information (`get_self_info`)

Gets information about the selected bot account. It only takes the common optional arguments. `Data` is `OneBot12SelfInfoData`, including `UserId`, `UserName`, and `UserDisplayName`.

```csharp
var response = await bot.Actions.GetSelfInfoAsync(echo: "self-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getuserinfoasync"></a>

### `GetUserInfoAsync` — Get user information (`get_user_info`)

Gets information about a friend or stranger. Pass a non-empty `userId` and the common optional arguments. `Data` is `OneBot12UserInfoData`, including the user ID, name, display name, and remark.

```csharp
var response = await bot.Actions.GetUserInfoAsync(
    "user-id", echo: "user-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfriendlistasync"></a>

### `GetFriendListAsync` — List friends or followers (`get_friend_list`)

Gets the selected bot's friends or followers. It only takes the common optional arguments. `Data` is an `IReadOnlyList<OneBot12UserInfoData>`.

```csharp
var response = await bot.Actions.GetFriendListAsync(echo: "friend-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Group actions**

<a id="getgroupinfoasync"></a>

### `GetGroupInfoAsync` — Get group information (`get_group_info`)

Gets one single-level group. Pass a non-empty `groupId` and the common optional arguments. `Data` is `OneBot12GroupInfoData`, including `GroupId` and `GroupName`.

```csharp
var response = await bot.Actions.GetGroupInfoAsync(
    "group-id", echo: "group-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouplistasync"></a>

### `GetGroupListAsync` — List joined groups (`get_group_list`)

Gets all single-level groups joined by the selected bot. It only takes the common optional arguments. `Data` is an `IReadOnlyList<OneBot12GroupInfoData>`.

```csharp
var response = await bot.Actions.GetGroupListAsync(echo: "group-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberinfoasync"></a>

### `GetGroupMemberInfoAsync` — Get group-member information (`get_group_member_info`)

Gets one member of a single-level group. Pass non-empty `groupId` and `userId`, followed by the common optional arguments. `Data` is `OneBot12GroupMemberInfoData`, including the user ID, name, and display name.

```csharp
var response = await bot.Actions.GetGroupMemberInfoAsync(
    "group-id", "user-id", echo: "group-member-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberlistasync"></a>

### `GetGroupMemberListAsync` — List group members (`get_group_member_list`)

Gets every member of a single-level group. Pass a non-empty `groupId` and the common optional arguments. `Data` is an `IReadOnlyList<OneBot12GroupMemberInfoData>`.

```csharp
var response = await bot.Actions.GetGroupMemberListAsync(
    "group-id", echo: "group-member-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupnameasync"></a>

### `SetGroupNameAsync` — Rename a group (`set_group_name`)

Changes a single-level group's name. Pass a non-empty `groupId`, a non-null `groupName`, and the common optional arguments. It returns an untyped `OneBot12Response`. This operation changes visible external state.

```csharp
var response = await bot.Actions.SetGroupNameAsync(
    "group-id", "New group name", echo: "set-group-name-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="leavegroupasync"></a>

### `LeaveGroupAsync` — Leave a group (`leave_group`)

Leaves a single-level group. Pass a non-empty `groupId` and the common optional arguments. It returns an untyped `OneBot12Response`. Membership changes may be irreversible, and some platforms may dismiss a group owned by the bot.

```csharp
var response = await bot.Actions.LeaveGroupAsync(
    "group-id", echo: "leave-group-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**Guild and channel actions**

<a id="getguildinfoasync"></a>

### `GetGuildInfoAsync` — Get guild information (`get_guild_info`)

Gets one two-level guild. Pass a non-empty `guildId` and the common optional arguments. `Data` is `OneBot12GuildInfoData`, including `GuildId` and `GuildName`.

```csharp
var response = await bot.Actions.GetGuildInfoAsync(
    "guild-id", echo: "guild-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getguildlistasync"></a>

### `GetGuildListAsync` — List joined guilds (`get_guild_list`)

Gets all guilds joined by the selected bot. It only takes the common optional arguments. `Data` is an `IReadOnlyList<OneBot12GuildInfoData>`.

```csharp
var response = await bot.Actions.GetGuildListAsync(echo: "guild-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setguildnameasync"></a>

### `SetGuildNameAsync` — Rename a guild (`set_guild_name`)

Changes a guild's name. Pass a non-empty `guildId`, a non-null `guildName`, and the common optional arguments. It returns an untyped `OneBot12Response` and changes visible external state.

```csharp
var response = await bot.Actions.SetGuildNameAsync(
    "guild-id", "New guild name", echo: "set-guild-name-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getguildmemberinfoasync"></a>

### `GetGuildMemberInfoAsync` — Get guild-member information (`get_guild_member_info`)

Gets one guild member. Pass non-empty `guildId` and `userId`, followed by the common optional arguments. `Data` is `OneBot12GuildMemberInfoData`, including the user ID, name, and display name.

```csharp
var response = await bot.Actions.GetGuildMemberInfoAsync(
    "guild-id", "user-id", echo: "guild-member-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getguildmemberlistasync"></a>

### `GetGuildMemberListAsync` — List guild members (`get_guild_member_list`)

Gets every member of a guild. Pass a non-empty `guildId` and the common optional arguments. `Data` is an `IReadOnlyList<OneBot12GuildMemberInfoData>`.

```csharp
var response = await bot.Actions.GetGuildMemberListAsync(
    "guild-id", echo: "guild-member-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="leaveguildasync"></a>

### `LeaveGuildAsync` — Leave a guild (`leave_guild`)

Leaves a guild. Pass a non-empty `guildId` and the common optional arguments. It returns an untyped `OneBot12Response`. The membership change may be irreversible.

```csharp
var response = await bot.Actions.LeaveGuildAsync(
    "guild-id", echo: "leave-guild-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannelinfoasync"></a>

### `GetChannelInfoAsync` — Get channel information (`get_channel_info`)

Gets one channel in a guild. Pass non-empty `guildId` and `channelId`, followed by the common optional arguments. `Data` is `OneBot12ChannelInfoData`, including `ChannelId` and `ChannelName`.

```csharp
var response = await bot.Actions.GetChannelInfoAsync(
    "guild-id", "channel-id", echo: "channel-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannellistasync"></a>

### `GetChannelListAsync` — List visible or joined channels (`get_channel_list`)

Gets visible channels in a guild. Pass a non-empty `guildId`; set `joinedOnly` to `true` to keep only channels the bot has joined, or leave its default `false` to include all visible channels. The common optional arguments follow. `Data` is an `IReadOnlyList<OneBot12ChannelInfoData>`.

```csharp
var response = await bot.Actions.GetChannelListAsync(
    "guild-id", joinedOnly: true, echo: "channel-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setchannelnameasync"></a>

### `SetChannelNameAsync` — Rename a channel (`set_channel_name`)

Changes a channel's name. Pass non-empty `guildId` and `channelId`, a non-null `channelName`, and the common optional arguments. It returns an untyped `OneBot12Response` and changes visible external state.

```csharp
var response = await bot.Actions.SetChannelNameAsync(
    "guild-id", "channel-id", "New channel name",
    echo: "set-channel-name-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannelmemberinfoasync"></a>

### `GetChannelMemberInfoAsync` — Get channel-member information (`get_channel_member_info`)

Gets one channel member. Pass non-empty `guildId`, `channelId`, and `userId`, followed by the common optional arguments. `Data` is `OneBot12ChannelMemberInfoData`, including the user ID, name, and display name.

```csharp
var response = await bot.Actions.GetChannelMemberInfoAsync(
    "guild-id", "channel-id", "user-id",
    echo: "channel-member-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannelmemberlistasync"></a>

### `GetChannelMemberListAsync` — List channel members (`get_channel_member_list`)

Gets every member of a channel. Pass non-empty `guildId` and `channelId`, followed by the common optional arguments. `Data` is an `IReadOnlyList<OneBot12ChannelMemberInfoData>`.

```csharp
var response = await bot.Actions.GetChannelMemberListAsync(
    "guild-id", "channel-id", echo: "channel-member-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="leavechannelasync"></a>

### `LeaveChannelAsync` — Leave a channel (`leave_channel`)

Leaves a channel. Pass non-empty `guildId` and `channelId`, followed by the common optional arguments. It returns an untyped `OneBot12Response`. The membership change may be irreversible.

```csharp
var response = await bot.Actions.LeaveChannelAsync(
    "guild-id", "channel-id", echo: "leave-channel-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**File actions**

<a id="uploadfileasync"></a>

### `UploadFileAsync` — Upload a complete file (`upload_file`)

Uploads one complete file. Pass a non-null `OneBot12UploadFileRequest` plus the common optional arguments. Build the request with `FromUrl(name, url, headers?, sha256?)`, `FromPath(name, path, sha256?)`, or `FromData(name, byte[], sha256?)`; names and source strings must be non-empty. `Data` is `OneBot12FileIdData` containing the returned `FileId`. Uploading creates external state.

```csharp
var request = OneBot12UploadFileRequest.FromUrl(
    "cat.png", "https://example.com/cat.png");
var response = await bot.Actions.UploadFileAsync(request, echo: "upload-file-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="uploadfilefragmentedasync"></a>

### `UploadFileFragmentedAsync` — Upload a file in stages (`upload_file_fragmented`; `prepare` / `transfer` / `finish`)

Provides three overloads for the three stages of `upload_file_fragmented`; every stage may modify stored external data.

The prepare overload takes non-empty `name`, non-negative `totalSize`, and the common optional arguments. `Data` is `OneBot12FileIdData` containing the temporary file ID.

```csharp
var response = await bot.Actions.UploadFileFragmentedAsync(
    name: "large.bin",
    totalSize: 4,
    echo: "upload-prepare-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The transfer overload takes non-empty `fileId`, non-negative `offset`, non-null raw `byte[] data`, and the common optional arguments. The SDK Base64-encodes `data`. It returns an untyped `OneBot12Response`.

```csharp
var response = await bot.Actions.UploadFileFragmentedAsync(
    fileId: "temporary-file-id",
    offset: 0,
    data: new byte[] { 0x4f, 0x42, 0x31, 0x32 },
    echo: "upload-transfer-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The finish overload takes non-empty `fileId`, non-empty lowercase `sha256` for the complete file, and the common optional arguments. `Data` is `OneBot12FileIdData` containing the final file ID.

```csharp
var response = await bot.Actions.UploadFileFragmentedAsync(
    fileId: "temporary-file-id",
    sha256: "9417d9a3474a248147afdb1dd56c2e920754f84fc596622dcfa7b3a4f5f16ae4",
    echo: "upload-finish-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="prepareuploadfilefragmentedasync"></a>

### `PrepareUploadFileFragmentedAsync` — Prepare a fragmented upload (`upload_file_fragmented`, `stage=prepare`)

Named equivalent of the prepare `UploadFileFragmentedAsync` overload. Pass non-empty `name`, non-negative `totalSize`, and the common optional arguments. `Data` is `OneBot12FileIdData` containing the temporary file ID.

```csharp
var response = await bot.Actions.PrepareUploadFileFragmentedAsync(
    "large.bin", 4, echo: "prepare-upload-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="transferuploadfilefragmentasync"></a>

### `TransferUploadFileFragmentAsync` — Transfer an upload fragment (`upload_file_fragmented`, `stage=transfer`)

Named equivalent of the transfer `UploadFileFragmentedAsync` overload. Pass non-empty `fileId`, non-negative `offset`, non-null raw `byte[] data`, and the common optional arguments. The SDK Base64-encodes the bytes and returns an untyped `OneBot12Response`.

```csharp
var response = await bot.Actions.TransferUploadFileFragmentAsync(
    "temporary-file-id",
    0,
    new byte[] { 0x4f, 0x42, 0x31, 0x32 },
    echo: "transfer-upload-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="finishuploadfilefragmentedasync"></a>

### `FinishUploadFileFragmentedAsync` — Finish a fragmented upload (`upload_file_fragmented`, `stage=finish`)

Named equivalent of the finish `UploadFileFragmentedAsync` overload. Pass non-empty `fileId`, non-empty lowercase `sha256` for the complete file, and the common optional arguments. `Data` is `OneBot12FileIdData` containing the final file ID.

```csharp
var response = await bot.Actions.FinishUploadFileFragmentedAsync(
    "temporary-file-id",
    "9417d9a3474a248147afdb1dd56c2e920754f84fc596622dcfa7b3a4f5f16ae4",
    echo: "finish-upload-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfileasync"></a>

### `GetFileAsync` — Get a complete file (`get_file`)

Gets a complete file. The standard overload takes non-empty `fileId`, a `OneBot12FileAccessType` value (`Url`, `Path`, or `Data`), and the common optional arguments. `Data` is `OneBot12FileData`; depending on the requested representation it may contain `Name`, `Url`, `Headers`, `Path`, inline decoded `Data`, and `Sha256`.

```csharp
var response = await bot.Actions.GetFileAsync(
    "file-id", OneBot12FileAccessType.Url, echo: "get-file-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The string overload accepts a non-empty standard or implementation-defined protocol `type` instead of the enum and returns the same response type.

```csharp
var response = await bot.Actions.GetFileAsync(
    "file-id", "url", echo: "get-file-string-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfilefragmentedasync"></a>

### `GetFileFragmentedAsync` — Get a file in stages (`get_file_fragmented`; `prepare` / `transfer`)

Provides two overloads for the two stages of `get_file_fragmented`.

The prepare overload takes non-empty `fileId` and the common optional arguments. `Data` is `OneBot12FileDownloadPreparationData`, including `Name`, `TotalSize`, and `Sha256`.

```csharp
var response = await bot.Actions.GetFileFragmentedAsync(
    "file-id", echo: "download-prepare-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

The transfer overload additionally takes non-negative `offset` and `size`. `Data` is `OneBot12FileFragmentData`, whose `Data` property contains the decoded bytes.

```csharp
var response = await bot.Actions.GetFileFragmentedAsync(
    "file-id", offset: 0, size: 1024, echo: "download-transfer-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="preparegetfilefragmentedasync"></a>

### `PrepareGetFileFragmentedAsync` — Prepare a fragmented download (`get_file_fragmented`, `stage=prepare`)

Named equivalent of the prepare `GetFileFragmentedAsync` overload. Pass non-empty `fileId` and the common optional arguments. `Data` is `OneBot12FileDownloadPreparationData`, including the name, total size, and checksum.

```csharp
var response = await bot.Actions.PrepareGetFileFragmentedAsync(
    "file-id", echo: "prepare-download-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfilefragmentasync"></a>

### `GetFileFragmentAsync` — Get a download fragment (`get_file_fragmented`, `stage=transfer`)

Named equivalent of the transfer `GetFileFragmentedAsync` overload. Pass non-empty `fileId`, non-negative `offset` and `size`, followed by the common optional arguments. `Data` is `OneBot12FileFragmentData` containing the decoded bytes.

```csharp
var response = await bot.Actions.GetFileFragmentAsync(
    "file-id", offset: 0, size: 1024, echo: "get-fragment-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

## Message chains

Use `OneBot12MessageChain` to build an ordered outgoing message. An image uses a `file_id` returned by `upload_file`:

```csharp
string imageFileId = "file_id returned by upload_file";

OneBot12SendMessage message = new OneBot12MessageChain()
    .Text("Image below:")
    .Image(imageFileId);

var response = await bot.Actions.SendGroupMessageAsync(
    groupId: "group-id",
    message: message);
```

Incoming messages use `OneBot12ReceivedMessage` and support `OfType<T>()` directly. Unknown segments become `OneBot12UnknownReceivedSegment` and retain their raw JSON.

## Supported message segments

Each snippet creates a segment that can be added to `OneBot12MessageChain` or `OneBot12SendMessage`.

### `text`

```csharp
var segment = new OneBot12TextSendSegment("Hello");
```

`text` may be empty but not `null`.

### `mention`

```csharp
var segment = new OneBot12MentionSendSegment("user-id");
```

`userId` is the platform user ID.

### `mention_all`

```csharp
var segment = new OneBot12MentionAllSendSegment();
```

No parameters.

### `image`

```csharp
var segment = new OneBot12ImageSendSegment("image-file-id");
```

`fileId` comes from `UploadFileAsync` or an existing implementation file.

### `voice`

```csharp
var segment = new OneBot12VoiceSendSegment("voice-file-id");
```

`fileId` refers to recorded voice.

### `audio`

```csharp
var segment = new OneBot12AudioSendSegment("audio-file-id");
```

`fileId` refers to ordinary audio.

### `video`

```csharp
var segment = new OneBot12VideoSendSegment("video-file-id");
```

`fileId` refers to a video file.

### `file`

```csharp
var segment = new OneBot12FileSendSegment("file-id");
```

`fileId` refers to a generic file.

### `location`

```csharp
var segment = new OneBot12LocationSendSegment(
    latitude: 39.9042,
    longitude: 116.4074,
    title: "Beijing",
    content: "Location description");
```

Coordinates are `double`; `title` and `content` are required.

### `reply`

```csharp
var segment = new OneBot12ReplySendSegment(
    messageId: "message-id",
    userId: "optional-user-id");
```

`messageId` is required and `userId` is optional.

### Implementation extensions

```csharp
var segment = new OneBot12CustomSendSegment(
    "markdown",
    new JsonObject { ["content"] = "**Hello**" });
```

`type` and `data` follow the implementation's contract. Unknown incoming types are retained as `OneBot12UnknownReceivedSegment`.

## Console debugging projects

These runnable projects are optional debugging tools. Use the Actions and Receiving events sections above as the API reference.

- [Observable sample](../samples/OneBotSdk.Net.V12.ObservableExample)
- [EventHandler sample](../samples/OneBotSdk.Net.V12.EventHandlerExample)
- [HTTP Action sample](../samples/OneBotSdk.Net.V12.HttpActionExample)

Do not place tokens in source code or logs. Prefer HTTPS/WSS in production. Upload, delete, rename, and leave operations change external state.
