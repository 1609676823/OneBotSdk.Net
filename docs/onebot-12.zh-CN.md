# OneBot 12 候选规范使用指南

[返回中文 README](../README.zh-CN.md) | [文档目录](README.md) | [English](onebot-12.md)

> **OneBot 12 候选规范官方资料：** [规范源码仓库](https://github.com/botuniverse/onebot) · [发布版文档](https://12.onebot.dev/)

OneBot 12 API 位于 `OneBotSdk.Net.V12.*`。当前实现包含 31 个标准协议 Action、40 个公开 Action 方法名（共 45 个重载）、19 类标准具体事件及 5 类未知回退事件，以及 10 种标准消息段。OneBot 12 仍是候选规范，且不能连接 OneBot 10/11 实现端。

## API 目录

- **[Action 方法](#action-方法) — 底层与元 Action：** [`CallActionAsync`](#callactionasync)、[`GetLatestEventsAsync`](#getlatesteventsasync)、[`GetSupportedActionsAsync`](#getsupportedactionsasync)、[`GetStatusAsync`](#getstatusasync)、[`GetVersionAsync`](#getversionasync)
- **Action 方法 — 消息与用户：** [`SendMessageAsync`](#sendmessageasync)、[`SendPrivateMessageAsync`](#sendprivatemessageasync)、[`SendGroupMessageAsync`](#sendgroupmessageasync)、[`SendChannelMessageAsync`](#sendchannelmessageasync)、[`DeleteMessageAsync`](#deletemessageasync)、[`GetSelfInfoAsync`](#getselfinfoasync)、[`GetUserInfoAsync`](#getuserinfoasync)、[`GetFriendListAsync`](#getfriendlistasync)
- **Action 方法 — 群：** [`GetGroupInfoAsync`](#getgroupinfoasync)、[`GetGroupListAsync`](#getgrouplistasync)、[`GetGroupMemberInfoAsync`](#getgroupmemberinfoasync)、[`GetGroupMemberListAsync`](#getgroupmemberlistasync)、[`SetGroupNameAsync`](#setgroupnameasync)、[`LeaveGroupAsync`](#leavegroupasync)
- **Action 方法 — 群组与频道：** [`GetGuildInfoAsync`](#getguildinfoasync)、[`GetGuildListAsync`](#getguildlistasync)、[`SetGuildNameAsync`](#setguildnameasync)、[`GetGuildMemberInfoAsync`](#getguildmemberinfoasync)、[`GetGuildMemberListAsync`](#getguildmemberlistasync)、[`LeaveGuildAsync`](#leaveguildasync)、[`GetChannelInfoAsync`](#getchannelinfoasync)、[`GetChannelListAsync`](#getchannellistasync)、[`SetChannelNameAsync`](#setchannelnameasync)、[`GetChannelMemberInfoAsync`](#getchannelmemberinfoasync)、[`GetChannelMemberListAsync`](#getchannelmemberlistasync)、[`LeaveChannelAsync`](#leavechannelasync)
- **Action 方法 — 文件：** [`UploadFileAsync`](#uploadfileasync)、[`UploadFileFragmentedAsync`](#uploadfilefragmentedasync)、[`PrepareUploadFileFragmentedAsync`](#prepareuploadfilefragmentedasync)、[`TransferUploadFileFragmentAsync`](#transferuploadfilefragmentasync)、[`FinishUploadFileFragmentedAsync`](#finishuploadfilefragmentedasync)、[`GetFileAsync`](#getfileasync)、[`GetFileFragmentedAsync`](#getfilefragmentedasync)、[`PrepareGetFileFragmentedAsync`](#preparegetfilefragmentedasync)、[`GetFileFragmentAsync`](#getfilefragmentasync)
- **[接收事件](#接收事件) — 消息：** [`PrivateMessageEvent`](#privatemessageevent)、[`GroupMessageEvent`](#groupmessageevent)、[`ChannelMessageEvent`](#channelmessageevent)
- **接收事件 — 通知：** [`FriendIncreaseNoticeEvent`](#friendincreasenoticeevent)、[`FriendDecreaseNoticeEvent`](#frienddecreasenoticeevent)、[`PrivateMessageDeleteNoticeEvent`](#privatemessagedeletenoticeevent)、[`GroupMemberIncreaseNoticeEvent`](#groupmemberincreasenoticeevent)、[`GroupMemberDecreaseNoticeEvent`](#groupmemberdecreasenoticeevent)、[`GroupMessageDeleteNoticeEvent`](#groupmessagedeletenoticeevent)、[`GuildMemberIncreaseNoticeEvent`](#guildmemberincreasenoticeevent)、[`GuildMemberDecreaseNoticeEvent`](#guildmemberdecreasenoticeevent)、[`ChannelMemberIncreaseNoticeEvent`](#channelmemberincreasenoticeevent)、[`ChannelMemberDecreaseNoticeEvent`](#channelmemberdecreasenoticeevent)、[`ChannelMessageDeleteNoticeEvent`](#channelmessagedeletenoticeevent)、[`ChannelCreateNoticeEvent`](#channelcreatenoticeevent)、[`ChannelDeleteNoticeEvent`](#channeldeletenoticeevent)
- **接收事件 — 元事件与回退：** [`ConnectMetaEvent`](#connectmetaevent)、[`HeartbeatMetaEvent`](#heartbeatmetaevent)、[`StatusUpdateMetaEvent`](#statusupdatemetaevent)、[`UnknownOneBot12Event`](#unknownonebot12event)、[`UnknownMessageEvent`](#unknownmessageevent)、[`UnknownNoticeEvent`](#unknownnoticeevent)、[`UnknownRequestEvent`](#unknownrequestevent)、[`UnknownMetaEvent`](#unknownmetaevent)

## 基本引用与启动

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
    new OneBot12Self("平台名称", "机器人用户ID"));

using var bot = new OneBot12Bot(options);
```

OneBot 12 的 ID 都是字符串。Action 与 Event 地址和 Token 分开配置；请在 `StartAsync()` 前订阅事件。

### EventHandler

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    var message = args.Event.Message;
    if (message == null)
        return;

    foreach (var text in message.OfType<OneBot12TextReceivedSegment>())
        Console.WriteLine("文本：" + text.Text);

    foreach (var image in message.OfType<OneBot12ImageReceivedSegment>())
        Console.WriteLine("图片 file_id：" + image.FileId);
};
```

### Observable

```csharp
using var subscription = bot.MessageReceived
    .OfType<PrivateMessageEvent>()
    .Subscribe(message => Console.WriteLine(message.Message?.PlainText));
```

### 开始监听

```csharp
var start = await bot.StartAsync();
Console.WriteLine($"实现端：{start.VersionResponse.Data?.Impl}");
Console.WriteLine("按 Enter 键退出。");
Console.ReadLine();
```

控制台示例不需要 `ManualResetEvent`。若确实使用它，应释放等待句柄，但可以写成 `using var exit = new ManualResetEvent(false);`，无需嵌套。服务程序建议使用宿主取消令牌。

## 接收事件

请在 `StartAsync()` 前完成订阅。下面使用强类型 `EventHandler` 入口；同一个分发器也提供对应的热 `IObservable<T>`，例如 `PrivateMessages`、`FriendIncreaseNotices` 和 `Heartbeats`。使用 Observable 时请保存返回的 `IDisposable`，并在不再需要订阅时释放。

所有事件都继承 `OneBot12Event` 的按字段容错可空属性：`Id`（`string?`，全局唯一事件 ID）、`Time`（`double?`，Unix 秒）、`Type`（`string?`）、`DetailType`（`string?`）、`SubType`（`string?`）以及 `Self`（`OneBot12Self?`，元事件不提供）。`RawJson` 是包含完整接收事件及实现端扩展字段的独立 `JsonObject`。下面每个最小处理器都直接输出该原始事件对象。

前 19 节是标准具体事件；最后 5 节是具体回退类型，因此未知扩展事件也可以被观察，而不会被丢弃。

<a id="privatemessageevent"></a>

### `PrivateMessageEvent` — 私聊消息（`message/private`）

订阅入口：`bot.Events.PrivateMessageReceived`（热流：`bot.Events.PrivateMessages`）。

接收 `message/private` 私聊消息。关键字段是 `MessageId`、`UserId`、解析后的 `Message` 和文本替代表示 `AltMessage`。

```csharp
bot.Events.PrivateMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmessageevent"></a>

### `GroupMessageEvent` — 群消息（`message/group`）

订阅入口：`bot.Events.GroupMessageReceived`（热流：`bot.Events.GroupMessages`）。

接收 `message/group` 群消息。除 `MessageId`、`UserId`、`Message` 和 `AltMessage` 外，还提供 `GroupId`。

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmessageevent"></a>

### `ChannelMessageEvent` — 频道消息（`message/channel`）

订阅入口：`bot.Events.ChannelMessageReceived`（热流：`bot.Events.ChannelMessages`）。

接收 `message/channel` 频道消息。关键字段是 `GuildId`、`ChannelId`、`MessageId`、`UserId`、`Message` 和 `AltMessage`。

```csharp
bot.Events.ChannelMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="friendincreasenoticeevent"></a>

### `FriendIncreaseNoticeEvent` — 好友增加通知（`notice/friend_increase`）

订阅入口：`bot.Events.FriendIncreaseNoticeReceived`（热流：`bot.Events.FriendIncreaseNotices`）。

报告新增好友，`UserId` 标识该好友。

```csharp
bot.Events.FriendIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="frienddecreasenoticeevent"></a>

### `FriendDecreaseNoticeEvent` — 好友减少通知（`notice/friend_decrease`）

订阅入口：`bot.Events.FriendDecreaseNoticeReceived`（热流：`bot.Events.FriendDecreaseNotices`）。

报告好友被移除，`UserId` 标识被移除的好友。

```csharp
bot.Events.FriendDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="privatemessagedeletenoticeevent"></a>

### `PrivateMessageDeleteNoticeEvent` — 私聊消息删除通知（`notice/private_message_delete`）

订阅入口：`bot.Events.PrivateMessageDeleteNoticeReceived`（热流：`bot.Events.PrivateMessageDeleteNotices`）。

报告私聊消息被删除或撤回。`MessageId` 标识消息，`UserId` 标识对端用户。

```csharp
bot.Events.PrivateMessageDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmemberincreasenoticeevent"></a>

### `GroupMemberIncreaseNoticeEvent` — 群成员增加通知（`notice/group_member_increase`）

订阅入口：`bot.Events.GroupMemberIncreaseNoticeReceived`（热流：`bot.Events.GroupMemberIncreaseNotices`）。

报告成员加入单级群。关键字段是 `GroupId`、加入者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.GroupMemberIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmemberdecreasenoticeevent"></a>

### `GroupMemberDecreaseNoticeEvent` — 群成员减少通知（`notice/group_member_decrease`）

订阅入口：`bot.Events.GroupMemberDecreaseNoticeReceived`（热流：`bot.Events.GroupMemberDecreaseNotices`）。

报告成员离开单级群。关键字段是 `GroupId`、离开者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.GroupMemberDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="groupmessagedeletenoticeevent"></a>

### `GroupMessageDeleteNoticeEvent` — 群消息删除通知（`notice/group_message_delete`）

订阅入口：`bot.Events.GroupMessageDeleteNoticeReceived`（热流：`bot.Events.GroupMessageDeleteNotices`）。

报告群消息被删除或撤回。关键字段是 `GroupId`、`MessageId`、消息作者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.GroupMessageDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="guildmemberincreasenoticeevent"></a>

### `GuildMemberIncreaseNoticeEvent` — 群组成员增加通知（`notice/guild_member_increase`）

订阅入口：`bot.Events.GuildMemberIncreaseNoticeReceived`（热流：`bot.Events.GuildMemberIncreaseNotices`）。

报告成员加入两级群组。关键字段是 `GuildId`、加入者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.GuildMemberIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="guildmemberdecreasenoticeevent"></a>

### `GuildMemberDecreaseNoticeEvent` — 群组成员减少通知（`notice/guild_member_decrease`）

订阅入口：`bot.Events.GuildMemberDecreaseNoticeReceived`（热流：`bot.Events.GuildMemberDecreaseNotices`）。

报告成员离开两级群组。关键字段是 `GuildId`、离开者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.GuildMemberDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmemberincreasenoticeevent"></a>

### `ChannelMemberIncreaseNoticeEvent` — 频道成员增加通知（`notice/channel_member_increase`）

订阅入口：`bot.Events.ChannelMemberIncreaseNoticeReceived`（热流：`bot.Events.ChannelMemberIncreaseNotices`）。

报告成员加入频道。关键字段是 `GuildId`、`ChannelId`、加入者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.ChannelMemberIncreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmemberdecreasenoticeevent"></a>

### `ChannelMemberDecreaseNoticeEvent` — 频道成员减少通知（`notice/channel_member_decrease`）

订阅入口：`bot.Events.ChannelMemberDecreaseNoticeReceived`（热流：`bot.Events.ChannelMemberDecreaseNotices`）。

报告成员离开频道。关键字段是 `GuildId`、`ChannelId`、离开者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.ChannelMemberDecreaseNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelmessagedeletenoticeevent"></a>

### `ChannelMessageDeleteNoticeEvent` — 频道消息删除通知（`notice/channel_message_delete`）

订阅入口：`bot.Events.ChannelMessageDeleteNoticeReceived`（热流：`bot.Events.ChannelMessageDeleteNotices`）。

报告频道消息被删除或撤回。关键字段是 `GuildId`、`ChannelId`、`MessageId`、消息作者 `UserId` 和 `OperatorId`。

```csharp
bot.Events.ChannelMessageDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channelcreatenoticeevent"></a>

### `ChannelCreateNoticeEvent` — 频道创建通知（`notice/channel_create`）

订阅入口：`bot.Events.ChannelCreateNoticeReceived`（热流：`bot.Events.ChannelCreateNotices`）。

报告频道创建。`GuildId` 标识所属群组，`ChannelId` 标识新频道，`OperatorId` 标识操作者。

```csharp
bot.Events.ChannelCreateNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="channeldeletenoticeevent"></a>

### `ChannelDeleteNoticeEvent` — 频道删除通知（`notice/channel_delete`）

订阅入口：`bot.Events.ChannelDeleteNoticeReceived`（热流：`bot.Events.ChannelDeleteNotices`）。

报告频道删除。`GuildId` 标识原所属群组，`ChannelId` 标识已删除频道，`OperatorId` 标识操作者。

```csharp
bot.Events.ChannelDeleteNoticeReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="connectmetaevent"></a>

### `ConnectMetaEvent` — 连接建立元事件（`meta/connect`）

订阅入口：`bot.Events.ConnectMetaEventReceived`（热流：`bot.Events.ConnectEvents`）。

接收 WebSocket 成功连接后的首个事件。`Version` 是可空 `OneBot12VersionData`，包含 `Impl`、`Version` 和 `OneBotVersion`。

```csharp
bot.Events.ConnectMetaEventReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="heartbeatmetaevent"></a>

### `HeartbeatMetaEvent` — 心跳元事件（`meta/heartbeat`）

订阅入口：`bot.Events.HeartbeatMetaEventReceived`（热流：`bot.Events.Heartbeats`）。

接收周期性心跳。`Interval` 是可空 `long`，表示距下一次心跳的毫秒数。

```csharp
bot.Events.HeartbeatMetaEventReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="statusupdatemetaevent"></a>

### `StatusUpdateMetaEvent` — 状态更新元事件（`meta/status_update`）

订阅入口：`bot.Events.StatusUpdateMetaEventReceived`（热流：`bot.Events.StatusUpdates`）。

接收实现端或机器人状态变化。`Status` 是可空 `OneBot12StatusData`，包含整体 `Good` 状态和各账号 `Bots` 条目。

```csharp
bot.Events.StatusUpdateMetaEventReceived += (_, args) =>
    Console.WriteLine(args.Event.RawJson.ToJsonString());
```

<a id="unknownonebot12event"></a>

### `UnknownOneBot12Event` — 未知顶层事件（`<unknown>/*` 回退）

订阅入口：`bot.UnknownEventReceived.OfType<UnknownOneBot12Event>()`（全部未知事件的 EventHandler：`bot.Events.UnknownEventDispatched`）。

保留无法识别的顶层 `type`。仅投影公共字段；实现端扩展数据请读取 `RawJson`。

```csharp
using var unknownEventSubscription = bot.UnknownEventReceived
    .OfType<UnknownOneBot12Event>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownmessageevent"></a>

### `UnknownMessageEvent` — 未知消息详细类型（`message/<unknown>` 回退）

订阅入口：`bot.UnknownEventReceived.OfType<UnknownMessageEvent>()`（全部未知事件的 EventHandler：`bot.Events.UnknownEventDispatched`）。

保留 `detail_type` 未知的 `message` 事件。仍会投影 `MessageId`、`UserId`、`Message` 和 `AltMessage`，扩展字段保留在 `RawJson`。

```csharp
using var unknownMessageSubscription = bot.UnknownEventReceived
    .OfType<UnknownMessageEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownnoticeevent"></a>

### `UnknownNoticeEvent` — 未知通知详细类型（`notice/<unknown>` 回退）

订阅入口：`bot.UnknownEventReceived.OfType<UnknownNoticeEvent>()`（全部未知事件的 EventHandler：`bot.Events.UnknownEventDispatched`）。

保留 `detail_type` 未知的 `notice` 事件。可结合公共判别字段与 `RawJson` 处理实现端自定义字段。

```csharp
using var unknownNoticeSubscription = bot.UnknownEventReceived
    .OfType<UnknownNoticeEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownrequestevent"></a>

### `UnknownRequestEvent` — 保留的请求事件（`request/*` 回退）

订阅入口：`bot.UnknownEventReceived.OfType<UnknownRequestEvent>()`（全部未知事件的 EventHandler：`bot.Events.UnknownEventDispatched`）。

保留所有 `request` 事件。OneBot 12 当前只保留该类别而未定义标准详细类型，因此应读取 `DetailType`、`SubType` 和 `RawJson`。

```csharp
using var unknownRequestSubscription = bot.UnknownEventReceived
    .OfType<UnknownRequestEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

<a id="unknownmetaevent"></a>

### `UnknownMetaEvent` — 未知元事件详细类型（`meta/<unknown>` 回退）

订阅入口：`bot.UnknownEventReceived.OfType<UnknownMetaEvent>()`（全部未知事件的 EventHandler：`bot.Events.UnknownEventDispatched`）。

保留 `detail_type` 未知的 `meta` 事件。可结合公共判别字段与 `RawJson` 处理扩展字段。

```csharp
using var unknownMetaSubscription = bot.UnknownEventReceived
    .OfType<UnknownMetaEvent>()
    .Subscribe(e => Console.WriteLine(e.RawJson.ToJsonString()));
```

## Action 方法

`OneBot12Client` 通过 45 个重载公开了 40 个 Action 方法名，下面逐一说明。每段代码均假定已按前文创建 `bot`，并直接输出实际发送的 JSON 请求报文和接收的响应报文，不依赖公共输出函数。

每个响应都包含 `Status`、`RetCode`、`Message`、`Echo`、`IsSuccess`、`RawRequestJson` 和 `RawResponseJson`。强类型 `OneBot12Response<TData>` 的解析结果位于 `Data`，未投影的原始 data 位于 `RawData`；非泛型 `OneBot12Response` 的 `Data` 本身就是原始 JSON。

原始报文可能包含私聊内容、文件数据、本机路径、URL、请求头等敏感值。请只在受控诊断环境中输出，并在分享日志前进行脱敏。

除特别说明外，方法末尾都有以下可选参数：

- `echo`（`string?`，默认 `null`）：可选的关联字符串，随请求发送，通常由实现端原样返回。
- `self`（`OneBot12Self?`，默认 `null`）：执行 Action 的可选机器人身份；省略时使用 `OneBot12BotOptions` 中的默认身份。
- `cancellationToken`（`CancellationToken`，默认值）：用于取消传输操作。

四个元 Action 不接受也不会发送 `self`。所有 OneBot 12 ID 都是非空字符串。OneBot 12 不使用 OneBot 10/11 的 `_async` 或 `_rate_limited` 调用模式。

**底层调用与扩展 Action**

<a id="callactionasync"></a>

### `CallActionAsync` — 动态调用 Action（运行时传入 `action`）

调用任意标准或实现端扩展 Action。非泛型重载需要非空 `action`，还可传 `JsonObject? parameters = null` 以及通用的 `echo`、`self`、`cancellationToken`。返回 `OneBot12Response`，其中 `Data` 是 Action 的原始 JSON data。

```csharp
var response = await bot.Actions.CallActionAsync(
    "implementation_extension",
    new JsonObject { ["key"] = "value" },
    echo: "extension-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

泛型重载还要求在 `action` 后传入非 `null` 的 `Func<JsonNode?, TData?> dataParser`，返回 `OneBot12Response<TData>`；解析结果在 `Data`，原始值在 `RawData`。

```csharp
var response = await bot.Actions.CallActionAsync<JsonNode>(
    "implementation_extension",
    node => node,
    new JsonObject { ["key"] = "value" },
    echo: "extension-typed-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**元 Action**

<a id="getlatesteventsasync"></a>

### `GetLatestEventsAsync` — 轮询最新事件（`get_latest_events`）

通过 HTTP 轮询实现端缓冲的非元事件。`limit` 是非负 `long`，默认 `0`，表示不限制事件数量；`timeoutSeconds` 也是非负 `long`，默认 `0`，表示使用短轮询且不等待。其余可选参数为 `echo` 和 `cancellationToken`。`Data` 是 `IReadOnlyList<OneBot12Event>`，每个事件均保留扩展字段。

```csharp
var response = await bot.Actions.GetLatestEventsAsync(
    limit: 100,
    timeoutSeconds: 30,
    echo: "latest-events-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getsupportedactionsasync"></a>

### `GetSupportedActionsAsync` — 获取支持的 Action（`get_supported_actions`）

获取实现端声明支持的 Action 名称。可选参数为 `echo` 和 `cancellationToken`。`Data` 是包含 Action 名称的 `IReadOnlyList<string>`。

```csharp
var response = await bot.Actions.GetSupportedActionsAsync(echo: "supported-actions-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstatusasync"></a>

### `GetStatusAsync` — 获取运行状态（`get_status`）

获取实现端整体及各机器人运行状态。可选参数为 `echo` 和 `cancellationToken`。`Data` 是 `OneBot12StatusData`，包含 `Good` 和 `Bots` 集合；每个机器人状态包含 `Self` 和 `Online`。

```csharp
var response = await bot.Actions.GetStatusAsync(echo: "status-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getversionasync"></a>

### `GetVersionAsync` — 获取版本信息（`get_version`）

获取实现端与协议版本信息。可选参数为 `echo` 和 `cancellationToken`。`Data` 是 `OneBot12VersionData`，包含 `Impl`、`Version` 和 `OneBotVersion`。

```csharp
var response = await bot.Actions.GetVersionAsync(echo: "version-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**消息 Action**

<a id="sendmessageasync"></a>

### `SendMessageAsync` — 发送消息（`send_message`）

向标准或实现端扩展目标发送消息。必填参数是非空 `string detailType` 和非 `null` 的 `OneBot12SendMessage message`。`userId`、`groupId`、`guildId`、`channelId` 均为默认 `null` 的 `string?`：`private` 必须传 `userId`，`group` 必须传 `groupId`，`channel` 必须同时传 `guildId` 与 `channelId`；扩展目标类型可按实现端约定使用这些可选 ID。随后是通用可选参数。`Data` 是 `OneBot12SendMessageData`，实现端提供时包含 `MessageId` 和 `Time`。

```csharp
var message = new OneBot12MessageChain().Text("你好");
var response = await bot.Actions.SendMessageAsync(
    detailType: "group",
    message: message,
    groupId: "群ID",
    echo: "send-message-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendprivatemessageasync"></a>

### `SendPrivateMessageAsync` — 发送私聊消息（`send_message`，`detail_type=private`）

`send_message` 的便利方法，会把 `detail_type` 设为 `private`。传入非空 `userId`、非 null 的 `OneBot12SendMessage message`，以及可选的 `echo`、`self`、`cancellationToken`。`Data` 是包含返回消息 ID 和时间的 `OneBot12SendMessageData`。

```csharp
var message = new OneBot12MessageChain().Text("你好");
var response = await bot.Actions.SendPrivateMessageAsync(
    "用户ID", message, echo: "send-private-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendgroupmessageasync"></a>

### `SendGroupMessageAsync` — 发送群消息（`send_message`，`detail_type=group`）

`send_message` 的便利方法，会把 `detail_type` 设为 `group`。传入非空 `groupId`、非 null 的 `OneBot12SendMessage message` 和通用可选参数。`Data` 是包含返回消息 ID 和时间的 `OneBot12SendMessageData`。

```csharp
var message = new OneBot12MessageChain().Text("大家好");
var response = await bot.Actions.SendGroupMessageAsync(
    "群ID", message, echo: "send-group-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendchannelmessageasync"></a>

### `SendChannelMessageAsync` — 发送频道消息（`send_message`，`detail_type=channel`）

`send_message` 的便利方法，会把 `detail_type` 设为 `channel`。传入非空 `guildId` 和 `channelId`、非 null 的 `OneBot12SendMessage message` 和通用可选参数。`Data` 是包含返回消息 ID 和时间的 `OneBot12SendMessageData`。

```csharp
var message = new OneBot12MessageChain().Text("频道你好");
var response = await bot.Actions.SendChannelMessageAsync(
    "群组ID", "频道ID", message, echo: "send-channel-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="deletemessageasync"></a>

### `DeleteMessageAsync` — 删除或撤回消息（`delete_message`）

删除或撤回一条消息。传入非空 `messageId` 和通用可选参数。返回非泛型 `OneBot12Response`，通过响应信封字段判断执行结果。此操作会修改外部状态。

```csharp
var response = await bot.Actions.DeleteMessageAsync(
    "消息ID", echo: "delete-message-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**用户 Action**

<a id="getselfinfoasync"></a>

### `GetSelfInfoAsync` — 获取机器人账号信息（`get_self_info`）

获取所选机器人账号信息。仅需通用可选参数。`Data` 是 `OneBot12SelfInfoData`，包含 `UserId`、`UserName` 和 `UserDisplayName`。

```csharp
var response = await bot.Actions.GetSelfInfoAsync(echo: "self-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getuserinfoasync"></a>

### `GetUserInfoAsync` — 获取用户信息（`get_user_info`）

获取好友或陌生用户信息。传入非空 `userId` 和通用可选参数。`Data` 是 `OneBot12UserInfoData`，包含用户 ID、名称、显示名称和备注。

```csharp
var response = await bot.Actions.GetUserInfoAsync(
    "用户ID", echo: "user-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfriendlistasync"></a>

### `GetFriendListAsync` — 获取好友或关注者列表（`get_friend_list`）

获取所选机器人的好友或关注者。仅需通用可选参数。`Data` 是 `IReadOnlyList<OneBot12UserInfoData>`。

```csharp
var response = await bot.Actions.GetFriendListAsync(echo: "friend-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**群 Action**

<a id="getgroupinfoasync"></a>

### `GetGroupInfoAsync` — 获取群信息（`get_group_info`）

获取一个单级群。传入非空 `groupId` 和通用可选参数。`Data` 是 `OneBot12GroupInfoData`，包含 `GroupId` 和 `GroupName`。

```csharp
var response = await bot.Actions.GetGroupInfoAsync(
    "群ID", echo: "group-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouplistasync"></a>

### `GetGroupListAsync` — 获取已加入群列表（`get_group_list`）

获取所选机器人加入的全部单级群。仅需通用可选参数。`Data` 是 `IReadOnlyList<OneBot12GroupInfoData>`。

```csharp
var response = await bot.Actions.GetGroupListAsync(echo: "group-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberinfoasync"></a>

### `GetGroupMemberInfoAsync` — 获取群成员信息（`get_group_member_info`）

获取单级群中的一个成员。传入非空 `groupId` 和 `userId`，之后可传通用可选参数。`Data` 是 `OneBot12GroupMemberInfoData`，包含用户 ID、名称和显示名称。

```csharp
var response = await bot.Actions.GetGroupMemberInfoAsync(
    "群ID", "用户ID", echo: "group-member-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberlistasync"></a>

### `GetGroupMemberListAsync` — 获取群成员列表（`get_group_member_list`）

获取单级群的全部成员。传入非空 `groupId` 和通用可选参数。`Data` 是 `IReadOnlyList<OneBot12GroupMemberInfoData>`。

```csharp
var response = await bot.Actions.GetGroupMemberListAsync(
    "群ID", echo: "group-member-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupnameasync"></a>

### `SetGroupNameAsync` — 修改群名称（`set_group_name`）

修改单级群名称。传入非空 `groupId`、非 null 的 `groupName` 和通用可选参数。返回非泛型 `OneBot12Response`。此操作会改变对外可见状态。

```csharp
var response = await bot.Actions.SetGroupNameAsync(
    "群ID", "新群名", echo: "set-group-name-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="leavegroupasync"></a>

### `LeaveGroupAsync` — 退出群（`leave_group`）

退出一个单级群。传入非空 `groupId` 和通用可选参数。返回非泛型 `OneBot12Response`。成员关系变更可能无法撤销，部分平台还可能解散机器人拥有的群。

```csharp
var response = await bot.Actions.LeaveGroupAsync(
    "群ID", echo: "leave-group-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**群组与频道 Action**

<a id="getguildinfoasync"></a>

### `GetGuildInfoAsync` — 获取群组信息（`get_guild_info`）

获取一个两级群组。传入非空 `guildId` 和通用可选参数。`Data` 是 `OneBot12GuildInfoData`，包含 `GuildId` 和 `GuildName`。

```csharp
var response = await bot.Actions.GetGuildInfoAsync(
    "群组ID", echo: "guild-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getguildlistasync"></a>

### `GetGuildListAsync` — 获取已加入群组列表（`get_guild_list`）

获取所选机器人加入的全部群组。仅需通用可选参数。`Data` 是 `IReadOnlyList<OneBot12GuildInfoData>`。

```csharp
var response = await bot.Actions.GetGuildListAsync(echo: "guild-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setguildnameasync"></a>

### `SetGuildNameAsync` — 修改群组名称（`set_guild_name`）

修改群组名称。传入非空 `guildId`、非 null 的 `guildName` 和通用可选参数。返回非泛型 `OneBot12Response`，并会改变对外可见状态。

```csharp
var response = await bot.Actions.SetGuildNameAsync(
    "群组ID", "新群组名", echo: "set-guild-name-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getguildmemberinfoasync"></a>

### `GetGuildMemberInfoAsync` — 获取群组成员信息（`get_guild_member_info`）

获取一个群组成员。传入非空 `guildId` 和 `userId`，之后可传通用可选参数。`Data` 是 `OneBot12GuildMemberInfoData`，包含用户 ID、名称和显示名称。

```csharp
var response = await bot.Actions.GetGuildMemberInfoAsync(
    "群组ID", "用户ID", echo: "guild-member-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getguildmemberlistasync"></a>

### `GetGuildMemberListAsync` — 获取群组成员列表（`get_guild_member_list`）

获取群组中的全部成员。传入非空 `guildId` 和通用可选参数。`Data` 是 `IReadOnlyList<OneBot12GuildMemberInfoData>`。

```csharp
var response = await bot.Actions.GetGuildMemberListAsync(
    "群组ID", echo: "guild-member-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="leaveguildasync"></a>

### `LeaveGuildAsync` — 退出群组（`leave_guild`）

退出一个群组。传入非空 `guildId` 和通用可选参数。返回非泛型 `OneBot12Response`。成员关系变更可能无法撤销。

```csharp
var response = await bot.Actions.LeaveGuildAsync(
    "群组ID", echo: "leave-guild-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannelinfoasync"></a>

### `GetChannelInfoAsync` — 获取频道信息（`get_channel_info`）

获取群组中的一个频道。传入非空 `guildId` 和 `channelId`，之后可传通用可选参数。`Data` 是 `OneBot12ChannelInfoData`，包含 `ChannelId` 和 `ChannelName`。

```csharp
var response = await bot.Actions.GetChannelInfoAsync(
    "群组ID", "频道ID", echo: "channel-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannellistasync"></a>

### `GetChannelListAsync` — 获取可见或已加入频道列表（`get_channel_list`）

获取群组中可见的频道。传入非空 `guildId`；`joinedOnly` 设为 `true` 时仅保留机器人已加入的频道，默认 `false` 表示全部可见频道，之后可传通用可选参数。`Data` 是 `IReadOnlyList<OneBot12ChannelInfoData>`。

```csharp
var response = await bot.Actions.GetChannelListAsync(
    "群组ID", joinedOnly: true, echo: "channel-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setchannelnameasync"></a>

### `SetChannelNameAsync` — 修改频道名称（`set_channel_name`）

修改频道名称。传入非空 `guildId`、`channelId`、非 null 的 `channelName` 和通用可选参数。返回非泛型 `OneBot12Response`，并会改变对外可见状态。

```csharp
var response = await bot.Actions.SetChannelNameAsync(
    "群组ID", "频道ID", "新频道名",
    echo: "set-channel-name-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannelmemberinfoasync"></a>

### `GetChannelMemberInfoAsync` — 获取频道成员信息（`get_channel_member_info`）

获取一个频道成员。传入非空 `guildId`、`channelId`、`userId`，之后可传通用可选参数。`Data` 是 `OneBot12ChannelMemberInfoData`，包含用户 ID、名称和显示名称。

```csharp
var response = await bot.Actions.GetChannelMemberInfoAsync(
    "群组ID", "频道ID", "用户ID",
    echo: "channel-member-info-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getchannelmemberlistasync"></a>

### `GetChannelMemberListAsync` — 获取频道成员列表（`get_channel_member_list`）

获取频道中的全部成员。传入非空 `guildId` 和 `channelId`，之后可传通用可选参数。`Data` 是 `IReadOnlyList<OneBot12ChannelMemberInfoData>`。

```csharp
var response = await bot.Actions.GetChannelMemberListAsync(
    "群组ID", "频道ID", echo: "channel-member-list-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="leavechannelasync"></a>

### `LeaveChannelAsync` — 退出频道（`leave_channel`）

退出一个频道。传入非空 `guildId` 和 `channelId`，之后可传通用可选参数。返回非泛型 `OneBot12Response`。成员关系变更可能无法撤销。

```csharp
var response = await bot.Actions.LeaveChannelAsync(
    "群组ID", "频道ID", echo: "leave-channel-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**文件 Action**

<a id="uploadfileasync"></a>

### `UploadFileAsync` — 上传完整文件（`upload_file`）

上传一个完整文件。传入非 null 的 `OneBot12UploadFileRequest` 和通用可选参数。请求可通过 `FromUrl(name, url, headers?, sha256?)`、`FromPath(name, path, sha256?)` 或 `FromData(name, byte[], sha256?)` 创建；文件名和来源字符串必须非空。`Data` 是包含返回 `FileId` 的 `OneBot12FileIdData`。上传会创建外部状态。

```csharp
var request = OneBot12UploadFileRequest.FromUrl(
    "cat.png", "https://example.com/cat.png");
var response = await bot.Actions.UploadFileAsync(request, echo: "upload-file-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="uploadfilefragmentedasync"></a>

### `UploadFileFragmentedAsync` — 分阶段上传文件（`upload_file_fragmented`；`prepare` / `transfer` / `finish`）

通过三个重载分别执行 `upload_file_fragmented` 的三个阶段；每个阶段都可能修改实现端存储的数据。

准备重载需要非空 `name`、非负 `totalSize` 和通用可选参数。`Data` 是包含临时文件 ID 的 `OneBot12FileIdData`。

```csharp
var response = await bot.Actions.UploadFileFragmentedAsync(
    name: "large.bin",
    totalSize: 4,
    echo: "upload-prepare-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

传输重载需要非空 `fileId`、非负 `offset`、非 null 的原始 `byte[] data` 和通用可选参数。SDK 会把 `data` 转为 Base64，返回非泛型 `OneBot12Response`。

```csharp
var response = await bot.Actions.UploadFileFragmentedAsync(
    fileId: "临时文件ID",
    offset: 0,
    data: new byte[] { 0x4f, 0x42, 0x31, 0x32 },
    echo: "upload-transfer-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

结束重载需要非空 `fileId`、完整文件的非空小写 `sha256` 和通用可选参数。`Data` 是包含最终文件 ID 的 `OneBot12FileIdData`。

```csharp
var response = await bot.Actions.UploadFileFragmentedAsync(
    fileId: "临时文件ID",
    sha256: "9417d9a3474a248147afdb1dd56c2e920754f84fc596622dcfa7b3a4f5f16ae4",
    echo: "upload-finish-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="prepareuploadfilefragmentedasync"></a>

### `PrepareUploadFileFragmentedAsync` — 准备分片上传（`upload_file_fragmented`，`stage=prepare`）

准备阶段 `UploadFileFragmentedAsync` 重载的具名等价方法。传入非空 `name`、非负 `totalSize` 和通用可选参数。`Data` 是包含临时文件 ID 的 `OneBot12FileIdData`。

```csharp
var response = await bot.Actions.PrepareUploadFileFragmentedAsync(
    "large.bin", 4, echo: "prepare-upload-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="transferuploadfilefragmentasync"></a>

### `TransferUploadFileFragmentAsync` — 传输上传分片（`upload_file_fragmented`，`stage=transfer`）

传输阶段 `UploadFileFragmentedAsync` 重载的具名等价方法。传入非空 `fileId`、非负 `offset`、非 null 的原始 `byte[] data` 和通用可选参数。SDK 会把字节转为 Base64，并返回非泛型 `OneBot12Response`。

```csharp
var response = await bot.Actions.TransferUploadFileFragmentAsync(
    "临时文件ID",
    0,
    new byte[] { 0x4f, 0x42, 0x31, 0x32 },
    echo: "transfer-upload-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="finishuploadfilefragmentedasync"></a>

### `FinishUploadFileFragmentedAsync` — 完成分片上传（`upload_file_fragmented`，`stage=finish`）

结束阶段 `UploadFileFragmentedAsync` 重载的具名等价方法。传入非空 `fileId`、完整文件的非空小写 `sha256` 和通用可选参数。`Data` 是包含最终文件 ID 的 `OneBot12FileIdData`。

```csharp
var response = await bot.Actions.FinishUploadFileFragmentedAsync(
    "临时文件ID",
    "9417d9a3474a248147afdb1dd56c2e920754f84fc596622dcfa7b3a4f5f16ae4",
    echo: "finish-upload-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfileasync"></a>

### `GetFileAsync` — 获取完整文件（`get_file`）

获取一个完整文件。标准重载需要非空 `fileId`、`OneBot12FileAccessType` 值（`Url`、`Path` 或 `Data`）和通用可选参数。`Data` 是 `OneBot12FileData`；根据请求表示形式，可能包含 `Name`、`Url`、`Headers`、`Path`、解码后的内联 `Data` 和 `Sha256`。

```csharp
var response = await bot.Actions.GetFileAsync(
    "文件ID", OneBot12FileAccessType.Url, echo: "get-file-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

字符串重载用非空的标准或实现端扩展协议 `type` 代替枚举，返回类型相同。

```csharp
var response = await bot.Actions.GetFileAsync(
    "文件ID", "url", echo: "get-file-string-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfilefragmentedasync"></a>

### `GetFileFragmentedAsync` — 分阶段获取文件（`get_file_fragmented`；`prepare` / `transfer`）

通过两个重载分别执行 `get_file_fragmented` 的两个阶段。

准备重载需要非空 `fileId` 和通用可选参数。`Data` 是 `OneBot12FileDownloadPreparationData`，包含 `Name`、`TotalSize` 和 `Sha256`。

```csharp
var response = await bot.Actions.GetFileFragmentedAsync(
    "文件ID", echo: "download-prepare-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

传输重载还需传入非负 `offset` 和 `size`。`Data` 是 `OneBot12FileFragmentData`，其 `Data` 属性包含解码后的字节。

```csharp
var response = await bot.Actions.GetFileFragmentedAsync(
    "文件ID", offset: 0, size: 1024, echo: "download-transfer-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="preparegetfilefragmentedasync"></a>

### `PrepareGetFileFragmentedAsync` — 准备分片下载（`get_file_fragmented`，`stage=prepare`）

准备阶段 `GetFileFragmentedAsync` 重载的具名等价方法。传入非空 `fileId` 和通用可选参数。`Data` 是 `OneBot12FileDownloadPreparationData`，包含文件名、总大小和校验和。

```csharp
var response = await bot.Actions.PrepareGetFileFragmentedAsync(
    "文件ID", echo: "prepare-download-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfilefragmentasync"></a>

### `GetFileFragmentAsync` — 获取下载分片（`get_file_fragmented`，`stage=transfer`）

传输阶段 `GetFileFragmentedAsync` 重载的具名等价方法。传入非空 `fileId`、非负 `offset` 和 `size`，之后可传通用可选参数。`Data` 是包含解码字节的 `OneBot12FileFragmentData`。

```csharp
var response = await bot.Actions.GetFileFragmentAsync(
    "文件ID", offset: 0, size: 1024, echo: "get-fragment-1");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

## 消息链

`OneBot12MessageChain` 用于构建有序发送消息。图片参数是实现端通过 `upload_file` 返回的 `file_id`：

```csharp
string imageFileId = "upload_file 返回的 file_id";

OneBot12SendMessage message = new OneBot12MessageChain()
    .Text("图片如下：")
    .Image(imageFileId);

var response = await bot.Actions.SendGroupMessageAsync(
    groupId: "群ID",
    message: message);
```

接收消息使用 `OneBot12ReceivedMessage`，可直接调用 `OfType<T>()`。未知段解析为 `OneBot12UnknownReceivedSegment` 并保留原始 JSON。

## 支持的消息段

以下代码均创建一个可加入 `OneBot12MessageChain` 或 `OneBot12SendMessage` 的发送段。

### `text` 文本

```csharp
var segment = new OneBot12TextSendSegment("你好");
```

`text` 是文本内容，可以为空字符串，不能为 `null`。

### `mention` 提及用户

```csharp
var segment = new OneBot12MentionSendSegment("用户ID");
```

`userId` 是平台用户 ID。

### `mention_all` 提及全体

```csharp
var segment = new OneBot12MentionAllSendSegment();
```

无需参数。

### `image` 图片

```csharp
var segment = new OneBot12ImageSendSegment("图片 file_id");
```

`fileId` 来自 `UploadFileAsync` 或实现端已有文件。

### `voice` 录制语音

```csharp
var segment = new OneBot12VoiceSendSegment("语音 file_id");
```

`fileId` 指向录制语音文件。

### `audio` 音频

```csharp
var segment = new OneBot12AudioSendSegment("音频 file_id");
```

`fileId` 指向普通音频文件。

### `video` 视频

```csharp
var segment = new OneBot12VideoSendSegment("视频 file_id");
```

`fileId` 指向视频文件。

### `file` 通用文件

```csharp
var segment = new OneBot12FileSendSegment("文件 file_id");
```

`fileId` 指向任意通用文件。

### `location` 位置

```csharp
var segment = new OneBot12LocationSendSegment(
    latitude: 39.9042,
    longitude: 116.4074,
    title: "北京",
    content: "位置说明");
```

经纬度是 `double`；`title` 和 `content` 必填。

### `reply` 回复

```csharp
var segment = new OneBot12ReplySendSegment(
    messageId: "消息ID",
    userId: "可选用户ID");
```

`messageId` 必填；`userId` 可省略。

### 实现端扩展

```csharp
var segment = new OneBot12CustomSendSegment(
    "markdown",
    new JsonObject { ["content"] = "**你好**" });
```

`type` 是扩展段名称，`data` 是实现端约定的参数。未知入站类型通过 `OneBot12UnknownReceivedSegment` 保留。

## 控制台调试工程

这些可运行工程只用于调试。API 使用方式以本文上面的 Action 方法和接收事件章节为准，不需要先阅读控制台工程。

- [Observable 示例](../samples/OneBotSdk.Net.V12.ObservableExample)
- [EventHandler 示例](../samples/OneBotSdk.Net.V12.EventHandlerExample)
- [HTTP Action 示例](../samples/OneBotSdk.Net.V12.HttpActionExample)

不要把 Token 写入源码或日志；生产环境优先使用 HTTPS/WSS。上传、删除、改名和退出操作会修改外部状态。
