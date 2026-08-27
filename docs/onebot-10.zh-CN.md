# OneBot 10 使用指南

[返回中文 README](../README.zh-CN.md) | [文档目录](README.md) | [English](onebot-10.md)

> **官方规范：** [OneBot 10](https://github.com/botuniverse/onebot-10)

OneBot 10 API 位于 `OneBotSdk.Net.V10.*`。当前实现包含 37 个公开 Action、13 类标准具体事件、5 类未知事件回退类型和 14 种标准消息段 wire type。它只能连接真正支持 OneBot 10 的实现端。

## 快速目录

- [Action 方法](#action-方法)
  - 通用：[`CallActionAsync`](#callactionasync)、[`HandleQuickOperationAsync`](#handlequickoperationasync)
  - 消息：[`SendPrivateMessageAsync`](#sendprivatemessageasync)、[`SendGroupMessageAsync`](#sendgroupmessageasync)、[`SendDiscussMessageAsync`](#senddiscussmessageasync)、[`SendMessageAsync`](#sendmessageasync)、[`DeleteMessageAsync`](#deletemessageasync)、[`SendLikeAsync`](#sendlikeasync)
  - 群与讨论组：[`SetGroupKickAsync`](#setgroupkickasync)、[`SetGroupBanAsync`](#setgroupbanasync)、[`SetGroupAnonymousBanAsync`](#setgroupanonymousbanasync)、[`SetGroupWholeBanAsync`](#setgroupwholebanasync)、[`SetGroupAdminAsync`](#setgroupadminasync)、[`SetGroupAnonymousAsync`](#setgroupanonymousasync)、[`SetGroupCardAsync`](#setgroupcardasync)、[`SetGroupLeaveAsync`](#setgroupleaveasync)、[`SetGroupSpecialTitleAsync`](#setgroupspecialtitleasync)、[`SetDiscussLeaveAsync`](#setdiscussleaveasync)
  - 请求：[`SetFriendAddRequestAsync`](#setfriendaddrequestasync)、[`SetGroupAddRequestAsync`](#setgroupaddrequestasync)
  - 信息：[`GetLoginInfoAsync`](#getlogininfoasync)、[`GetStrangerInfoAsync`](#getstrangerinfoasync)、[`GetFriendListAsync`](#getfriendlistasync)、[`GetGroupListAsync`](#getgrouplistasync)、[`GetGroupInfoAsync`](#getgroupinfoasync)、[`GetGroupMemberInfoAsync`](#getgroupmemberinfoasync)、[`GetGroupMemberListAsync`](#getgroupmemberlistasync)
  - 文件、凭据与系统：[`GetCookiesAsync`](#getcookiesasync)、[`GetCsrfTokenAsync`](#getcsrftokenasync)、[`GetCredentialsAsync`](#getcredentialsasync)、[`GetRecordAsync`](#getrecordasync)、[`GetImageAsync`](#getimageasync)、[`CanSendImageAsync`](#cansendimageasync)、[`CanSendRecordAsync`](#cansendrecordasync)、[`GetStatusAsync`](#getstatusasync)、[`GetVersionInfoAsync`](#getversioninfoasync)、[`SetRestartPluginAsync`](#setrestartpluginasync)、[`CleanDataDirectoryAsync`](#cleandatadirectoryasync)、[`CleanPluginLogAsync`](#cleanpluginlogasync)
- [接收事件](#接收事件)
  - 消息：[`PrivateMessageEvent`](#privatemessageevent)、[`GroupMessageEvent`](#groupmessageevent)、[`DiscussMessageEvent`](#discussmessageevent)
  - 通知：[`GroupUploadNoticeEvent`](#groupuploadnoticeevent)、[`GroupAdminNoticeEvent`](#groupadminnoticeevent)、[`GroupDecreaseNoticeEvent`](#groupdecreasenoticeevent)、[`GroupIncreaseNoticeEvent`](#groupincreasenoticeevent)、[`GroupBanNoticeEvent`](#groupbannoticeevent)、[`FriendAddNoticeEvent`](#friendaddnoticeevent)
  - 请求与元事件：[`FriendRequestEvent`](#friendrequestevent)、[`GroupRequestEvent`](#grouprequestevent)、[`LifecycleMetaEvent`](#lifecyclemetaevent)、[`HeartbeatMetaEvent`](#heartbeatmetaevent)
  - 未知事件回退：[`UnknownOneBot10Event`](#unknownonebot10event)、[`UnknownMessageEvent`](#unknownmessageevent)、[`UnknownNoticeEvent`](#unknownnoticeevent)、[`UnknownRequestEvent`](#unknownrequestevent)、[`UnknownMetaEvent`](#unknownmetaevent)

## 基本引用与启动

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

Action 与 Event 地址和 Token 分开配置。请在 `StartAsync()` 前订阅事件。

### EventHandler

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawMessage);

    foreach (var text in args.Event.MessageChain.OfType<TextReceivedSegment>())
        Console.WriteLine("文本：" + text.Text);

    foreach (var image in args.Event.MessageChain.OfType<ImageReceivedSegment>())
        Console.WriteLine("图片：" + image.Url);
};
```

### Observable

```csharp
using var subscription = bot.MessageReceived
    .OfType<PrivateMessageEvent>()
    .Subscribe(message => Console.WriteLine(message.MessageChain.PlainText));
```

### 开始监听

```csharp
var login = await bot.StartAsync();
Console.WriteLine($"已连接：{login.Data?.Nickname}");
Console.WriteLine("按 Enter 键退出。");
Console.ReadLine();
```

这里不需要 `ManualResetEvent`。它并非语法上必须写进 `using`，但持有系统等待句柄，使用时应释放；若确实需要，可以写成 `using var exit = new ManualResetEvent(false);`，无需增加嵌套。控制台示例直接等待 Enter 更清楚，服务程序则应使用宿主的取消令牌。

## 接收事件

请在 `StartAsync()` 前订阅。每个事件都继承可空的 `Time`、`SelfId`、`PostType`，并通过 `RawJson` 保留完整入站对象。下面是彼此独立的最小处理器，只假设已经创建[基本引用与启动](#基本引用与启动)中的 `bot`；每段代码都会直接输出原始事件报文。13 种标准事件在 `bot.Events` 上还各有同类型热 Observable，5 种回退类型则统一通过 `UnknownEventDispatched` 和 `UnknownEvents` 接收。

<a id="privatemessageevent"></a>

### `PrivateMessageEvent` — 私聊消息（`message/private`）

**订阅入口：** `bot.Events.PrivateMessageReceived`（EventHandler）或 `bot.Events.PrivateMessages`（Observable）。

接收私聊消息。关键字段包括 `UserId`、`SubType`、`MessageId`、`MessageChain`、`RawMessage` 和可空的 `Sender` 详情。

```csharp
bot.Events.PrivateMessageReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.UserId}: {e.MessageChain.PlainText}");
};
```

<a id="groupmessageevent"></a>

### `GroupMessageEvent` — 群消息（`message/group`）

**订阅入口：** `bot.Events.GroupMessageReceived`（EventHandler）或 `bot.Events.GroupMessages`（Observable）。

接收群消息。`GroupId`、`UserId` 分别标识群和发送者；匿名消息的 `Anonymous` 非空；`MessageChain`、`MessageId`、`Sender` 提供消息与发送者详情。

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.GroupId}/{e.UserId}: {e.MessageChain.PlainText}");
};
```

<a id="discussmessageevent"></a>

### `DiscussMessageEvent` — 讨论组消息（`message/discuss`）

**订阅入口：** `bot.Events.DiscussMessageReceived`（EventHandler）或 `bot.Events.DiscussMessages`（Observable）。

接收讨论组消息。关键字段包括 `DiscussId`、`UserId`、`MessageId`、`MessageChain` 和可空的 `Sender`。

```csharp
bot.Events.DiscussMessageReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.DiscussId}/{e.UserId}: {e.MessageChain.PlainText}");
};
```

<a id="groupuploadnoticeevent"></a>

### `GroupUploadNoticeEvent` — 群文件上传通知（`notice/group_upload`）

**订阅入口：** `bot.Events.GroupUploadNoticeReceived`（EventHandler）或 `bot.Events.GroupUploadNotices`（Observable）。

报告群文件上传。`GroupId` 标识群，`UserId` 标识上传者；可空的 `File` 包含 `Id`、`Name`、字节数 `Size` 和 `BusId`。

```csharp
bot.Events.GroupUploadNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.GroupId}/{e.UserId}: {e.File?.Name} ({e.File?.Size})");
};
```

<a id="groupadminnoticeevent"></a>

### `GroupAdminNoticeEvent` — 群管理员变动通知（`notice/group_admin`）

**订阅入口：** `bot.Events.GroupAdminNoticeReceived`（EventHandler）或 `bot.Events.GroupAdminNotices`（Observable）。

报告群管理员设置或取消。`SubType` 为 `set` 或 `unset`；`GroupId` 标识群，`UserId` 标识受影响管理员。

```csharp
bot.Events.GroupAdminNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.UserId}");
};
```

<a id="groupdecreasenoticeevent"></a>

### `GroupDecreaseNoticeEvent` — 群成员减少通知（`notice/group_decrease`）

**订阅入口：** `bot.Events.GroupDecreaseNoticeReceived`（EventHandler）或 `bot.Events.GroupDecreaseNotices`（Observable）。

报告成员退群或被移出。`SubType` 为 `leave`、`kick` 或 `kick_me`；`GroupId`、`OperatorId`、`UserId` 分别标识群、操作者和离开成员。

```csharp
bot.Events.GroupDecreaseNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.OperatorId}/{e.UserId}");
};
```

<a id="groupincreasenoticeevent"></a>

### `GroupIncreaseNoticeEvent` — 群成员增加通知（`notice/group_increase`）

**订阅入口：** `bot.Events.GroupIncreaseNoticeReceived`（EventHandler）或 `bot.Events.GroupIncreaseNotices`（Observable）。

报告成员入群。`SubType` 为 `approve` 或 `invite`；`GroupId`、`OperatorId`、`UserId` 分别标识群、操作者和新成员。

```csharp
bot.Events.GroupIncreaseNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.OperatorId}/{e.UserId}");
};
```

<a id="groupbannoticeevent"></a>

### `GroupBanNoticeEvent` — 群禁言通知（`notice/group_ban`）

**订阅入口：** `bot.Events.GroupBanNoticeReceived`（EventHandler）或 `bot.Events.GroupBanNotices`（Observable）。

报告禁言或解除禁言。`SubType` 为 `ban` 或 `lift_ban`；`GroupId`、`OperatorId`、`UserId` 标识相关对象，`Duration` 是禁言秒数。

```csharp
bot.Events.GroupBanNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.UserId}, {e.Duration}s");
};
```

<a id="friendaddnoticeevent"></a>

### `FriendAddNoticeEvent` — 好友新增通知（`notice/friend_add`）

**订阅入口：** `bot.Events.FriendAddNoticeReceived`（EventHandler）或 `bot.Events.FriendAddNotices`（Observable）。

报告新增好友。`UserId` 是新好友的 QQ 号。

```csharp
bot.Events.FriendAddNoticeReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine(e.UserId);
};
```

<a id="friendrequestevent"></a>

### `FriendRequestEvent` — 加好友请求（`request/friend`）

**订阅入口：** `bot.Events.FriendRequestReceived`（EventHandler）或 `bot.Events.FriendRequests`（Observable）。

接收加好友请求。`UserId` 标识请求者，`Comment` 是验证信息；处理请求时应把非空 `Flag` 原样传给 `SetFriendAddRequestAsync`。

```csharp
bot.Events.FriendRequestReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.UserId}: {e.Comment}; flag={e.Flag}");
};
```

<a id="grouprequestevent"></a>

### `GroupRequestEvent` — 加群请求或邀请（`request/group`）

**订阅入口：** `bot.Events.GroupRequestReceived`（EventHandler）或 `bot.Events.GroupRequests`（Observable）。

接收加群请求或邀请。`SubType` 为 `add` 或 `invite`；`GroupId`、`UserId`、`Comment`、`Flag` 标识并描述请求。处理时保留 `Flag` 并传给 `SetGroupAddRequestAsync`。

```csharp
bot.Events.GroupRequestReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SubType}: {e.GroupId}/{e.UserId}; flag={e.Flag}");
};
```

<a id="lifecyclemetaevent"></a>

### `LifecycleMetaEvent` — 生命周期元事件（`meta_event/lifecycle`）

**订阅入口：** `bot.Events.LifecycleMetaEventReceived`（EventHandler）或 `bot.Events.LifecycleEvents`（Observable）。

报告实现端生命周期变化。`SubType` 通常为 `enable`、`disable` 或 `connect`；`SelfId` 标识机器人账号。

```csharp
bot.Events.LifecycleMetaEventReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"{e.SelfId}: {e.SubType}");
};
```

<a id="heartbeatmetaevent"></a>

### `HeartbeatMetaEvent` — 心跳元事件（`meta_event/heartbeat`）

**订阅入口：** `bot.Events.HeartbeatMetaEventReceived`（EventHandler）或 `bot.Events.Heartbeats`（Observable）。

定期报告运行状态。`Interval` 是距下一次心跳的毫秒数；可空的 `Status` 提供 `Online`、`Good` 及实现端特有健康字段。

```csharp
bot.Events.HeartbeatMetaEventReceived += (_, args) =>
{
    var e = args.Event;
    Console.WriteLine(e.RawJson.ToJsonString());
    Console.WriteLine($"online={e.Status?.Online}, good={e.Status?.Good}, next={e.Interval}ms");
};
```

<a id="unknownonebot10event"></a>

### `UnknownOneBot10Event` — 未知顶层事件（未知 `post_type`）

**订阅入口：** `bot.Events.UnknownEventDispatched`；Observable 使用 `bot.Events.UnknownEvents`。

保留顶层 `PostType` 未知的事件。应检查 `PostType` 和 `RawJson`，不能假定存在任何分类专有字段。

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

### `UnknownMessageEvent` — 未知消息事件（`message/<unknown>`）

**订阅入口：** `bot.Events.UnknownEventDispatched`；Observable 使用 `bot.Events.UnknownEvents`。

保留未知 `MessageType`，同时仍会解析消息公共字段 `SubType`、`MessageId`、`UserId`、`MessageChain`、`RawMessage`、`Font`。

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

### `UnknownNoticeEvent` — 未知通知事件（`notice/<unknown>`）

**订阅入口：** `bot.Events.UnknownEventDispatched`；Observable 使用 `bot.Events.UnknownEvents`。

保留未知通知。`NoticeType`、`SubType` 保留判别值，`RawJson` 保留全部扩展字段。

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

### `UnknownRequestEvent` — 未知请求事件（`request/<unknown>`）

**订阅入口：** `bot.Events.UnknownEventDispatched`；Observable 使用 `bot.Events.UnknownEvents`。

保留未知请求。仍可读取公共字段 `RequestType`、`SubType`、`UserId`、`Comment`、`Flag`；不要自动处理不熟悉的请求。

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

### `UnknownMetaEvent` — 未知元事件（`meta_event/<unknown>`）

**订阅入口：** `bot.Events.UnknownEventDispatched`；Observable 使用 `bot.Events.UnknownEvents`。

保留未知元事件。`MetaEventType`、`SubType` 保留判别值，`RawJson` 包含全部实现端特有状态数据。

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

## Action 方法

`OneBot10Client` 共公开 39 个唯一方法名（46 个重载）：它们覆盖全部 37 个官方公开基础 Action，另含扩展 Action 与快速操作入口。每个方法末尾都接受以下可选公共参数；它们在每个签名中的类型、含义、可空性和默认值相同：

- `invocationMode` 类型为 `InvocationMode`，默认为 `InvocationMode.Normal`。`Async` 追加 `_async`，请求实现端异步受理，并且协议不会报告 Action 的最终执行结果；`RateLimited` 追加 `_rate_limited`，要求实现端按其配置速率排队执行。
- `echo` 类型为 `JsonNode?`，默认为 `null`，是供支持它的传输使用的可选关联值。内置 HTTP Action 传输没有 WebSocket 信封，因此会忽略它。
- `cancellationToken` 类型为 `CancellationToken`，默认为 `default`（即 `CancellationToken.None`），用于取消尚未完成的传输操作。

每次完成调用后，`RawRequestJson` 和 `RawResponseJson` 分别保留传输层实际发送、接收的原始 JSON。所有响应还提供 `Status`、`StatusKind`、`RetCode`、`IsSuccess`、`IsAsync` 和 `Echo`。下面每段代码都直接输出两个原始字符串，并假设已经有[基本引用与启动](#基本引用与启动)中的 `bot`。请把示例 ID、flag、文件名、域名和目标全部替换为自己实现端的真实值。泛型响应通过 `Data` 提供解析后的数据，通过 `RawData` 保留原始响应 `data` 节点；非泛型 `OneBot10Response` 则通过 `Data` 提供该节点。

原始报文可能包含消息正文、QQ 号、群号、完整事件上下文、Cookies、CSRF Token、实现端凭据或其它敏感扩展数据。生产环境不要无差别记录；持久化或共享日志前必须脱敏，并限制日志的访问权限和保留时间。

<a id="callactionasync"></a>

### `CallActionAsync` — 动态调用 Action（调用方传入 `action`）

**Action 专用参数：** `action`（`string`，必填且不能空白）；`parameters`（`JsonObject?`，默认 `null`）。泛型重载还要求非空的 `dataParser`（`Func<JsonNode?, TData?>`）。

按名称调用标准或实现端扩展 Action。非泛型重载接收 `action`、可选 `JsonObject parameters` 和公共尾参数，返回 `OneBot10Response`。泛型重载还要求传入 `Func<JsonNode?, TData?> dataParser`，返回 `OneBot10Response<TData>` 并保留 `RawData`。`action` 不能为空白，解析器不能为 `null`。

```csharp
var response = await bot.Actions.CallActionAsync(
    "implementation_extension",
    new JsonObject { ["key"] = "value" });
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

泛型重载使用调用方提供的容错解析器生成 `Data`，同时仍通过 `RawData` 保留未解析节点：

```csharp
var response = await bot.Actions.CallActionAsync<JsonNode>(
    OneBot10Actions.GetStatus,
    dataParser: node => node);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="handlequickoperationasync"></a>

### `HandleQuickOperationAsync` — 处理事件快速操作（隐藏 Action `.handle_quick_operation`）

**Action 专用参数：** 每个重载都要求一组非空的 `context`/`operation`。下文列出具体的 `JsonObject` 或强类型组合；二者都没有默认值。

针对 HTTP POST 上报的事件执行隐藏 Action `.handle_quick_operation`。它包含以下重载：

- `JsonObject context, JsonObject operation`：按原样发送完整事件对象和操作对象。
- `PrivateMessageEvent context, PrivateMessageQuickOperation operation`：支持 `Reply`、`AutoEscape`。
- `GroupMessageEvent context, GroupMessageQuickOperation operation`：支持 `Reply`、`AutoEscape`、`AtSender`、`Delete`、`Kick`、`Ban`、`BanDuration`。
- `DiscussMessageEvent context, DiscussMessageQuickOperation operation`：支持 `Reply`、`AutoEscape`、`AtSender`。
- `FriendRequestEvent context, FriendRequestQuickOperation operation`：支持 `Approve`、`Remark`。
- `GroupRequestEvent context, GroupRequestQuickOperation operation`：支持 `Approve`、`Reason`。

每个重载中的 `context` 和 `operation` 都是必填参数，不能为 `null`。五种强类型操作模型的每个属性均为可空类型，并设置为在值为 `null` 时不写入 JSON；因此，`null` 表示“不发送该字段的操作指令”，并不是发送 JSON `null`（原始 `JsonObject` 重载会发送调用方构造的对象）：

- `Reply` 类型为 `OneBot10SendMessage?`，表示出站回复，`null` 表示不回复。以下属性均为 `bool?`，值为 `null` 时省略，`true` 和 `false` 都会被明确发送：`AutoEscape` 控制字符串格式回复是否跳过 CQ 码解析，`AtSender` 控制是否提及发送者，`Delete` 控制是否撤回原消息，`Kick` 控制是否移出发送者，`Ban` 控制是否禁言发送者。
- `BanDuration` 类型为 `long?`，单位为秒，仅在 `Ban` 为 `true` 时有意义；值为 `null` 时省略，由实现端采用默认时长。
- `Approve` 类型为 `bool?`：`true` 表示同意，`false` 表示拒绝，`null` 表示省略且不处理好友或群请求。`Remark`、`Reason` 类型为 `string?`，值为 `null` 时省略，分别仅在同意好友请求、拒绝群请求时使用。

快速操作会修改外部状态：回复会发送消息，`Delete = true` 会撤回消息，`Kick = true` 会改变成员关系，`Ban = true` 会改变实时禁言状态，非 `null` 的 `Approve` 会处理请求。启用前必须核对事件上下文；撤回、踢人和请求处理无法可靠撤销或重放。

每个重载都返回 `OneBot10Response`，规范没有响应数据。

```csharp
async Task HandleHttpPostEventAsync(JsonObject eventContext)
{
    var response = await bot.Actions.HandleQuickOperationAsync(
        eventContext,
        new JsonObject { ["reply"] = "收到" });
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

```csharp
async Task HandlePrivateMessageAsync(PrivateMessageEvent context)
{
    var operation = new PrivateMessageQuickOperation
    {
        Reply = new OneBot10SendMessage().Text("收到"),
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
        Reply = new OneBot10SendMessage().Text("收到"),
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
        Reply = new OneBot10SendMessage().Text("收到"),
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
        Remark = "好友备注"
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
        Reason = "拒绝理由"
    };
    var response = await bot.Actions.HandleQuickOperationAsync(context, operation);
    Console.WriteLine(response.RawRequestJson);
    Console.WriteLine(response.RawResponseJson);
}
```

<a id="sendprivatemessageasync"></a>

### `SendPrivateMessageAsync` — 发送私聊消息（`send_private_msg`）

**Action 专用参数：** `userId`（`long`）；`message`（`OneBot10SendMessage`，必填且非空）；`autoEscape`（`bool`，默认 `false`）。

发送私聊消息。`userId` 是接收者 QQ 号；`message` 是非 `null` 的 `OneBot10SendMessage`；`autoEscape` 默认为 `false`。返回 `OneBot10Response<OneBot10SendMessageResult>`，实现端分配的消息 ID 位于 `Data?.MessageId`。

```csharp
var response = await bot.Actions.SendPrivateMessageAsync(
    123456789,
    new OneBot10SendMessage().Text("你好"));
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendgroupmessageasync"></a>

### `SendGroupMessageAsync` — 发送群消息（`send_group_msg`）

**Action 专用参数：** `groupId`（`long`）；`message`（`OneBot10SendMessage`，必填且非空）；`autoEscape`（`bool`，默认 `false`）。

发送群消息。`groupId` 是目标群号；`message` 是非 `null` 的出站消息链；`autoEscape` 默认为 `false`。返回 `OneBot10Response<OneBot10SendMessageResult>`，发送后的 `MessageId` 位于 `Data`。

```csharp
var response = await bot.Actions.SendGroupMessageAsync(
    987654321,
    new OneBot10SendMessage().Text("大家好"));
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="senddiscussmessageasync"></a>

### `SendDiscussMessageAsync` — 发送讨论组消息（`send_discuss_msg`）

**Action 专用参数：** `discussId`（`long`）；`message`（`OneBot10SendMessage`，必填且非空）；`autoEscape`（`bool`，默认 `false`）。

发送讨论组消息。`discussId` 标识目标讨论组；`message` 是非 `null` 的出站消息链；`autoEscape` 默认为 `false`。返回 `OneBot10Response<OneBot10SendMessageResult>`，发送后的 `MessageId` 位于 `Data`。

```csharp
var response = await bot.Actions.SendDiscussMessageAsync(
    111222333,
    new OneBot10SendMessage().Text("讨论组好"));
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendmessageasync"></a>

### `SendMessageAsync` — 发送消息（`send_msg`）

**Action 专用参数：** `message`（`OneBot10SendMessage`，必填且非空）；`messageType`（`OneBot10MessageType?`，默认 `null`）；`userId`、`groupId`、`discussId`（均为 `long?`，默认 `null`）；`autoEscape`（`bool`，默认 `false`）。

按条件选择目标并发送消息。`message` 必填；`messageType` 可为 `Private`、`Group` 或 `Discuss`；对应的可空 `userId`、`groupId` 或 `discussId` 指定目标；`autoEscape` 默认为 `false`。请传入相匹配的类型和目标 ID。返回 `OneBot10Response<OneBot10SendMessageResult>`，消息 ID 位于 `Data?.MessageId`。

```csharp
var response = await bot.Actions.SendMessageAsync(
    new OneBot10SendMessage().Text("你好"),
    messageType: OneBot10MessageType.Group,
    groupId: 987654321);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="deletemessageasync"></a>

### `DeleteMessageAsync` — 撤回消息（`delete_msg`）

**Action 专用参数：** `messageId`（`long`）。

撤回消息。`messageId` 是发送 Action 或事件给出的 OneBot 消息 ID。返回 `OneBot10Response`，规范没有响应数据。撤回无法自动恢复，请先核对 ID。

```csharp
var response = await bot.Actions.DeleteMessageAsync(messageId: 123);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendlikeasync"></a>

### `SendLikeAsync` — 发送好友赞（`send_like`）

**Action 专用参数：** `userId`（`long`）；`times`（`long`，默认 `1`）。

向好友发送赞。`userId` 是好友 QQ 号；`times` 是次数，默认 `1`；OneBot 规定每位好友每天最多十次。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SendLikeAsync(userId: 123456789, times: 1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

下面的群和讨论组方法会修改实时外部状态。运行代码前请检查账号权限和每个目标。

<a id="setgroupkickasync"></a>

### `SetGroupKickAsync` — 移出群成员（`set_group_kick`）

**Action 专用参数：** `groupId`、`userId`（`long`）；`rejectAddRequest`（`bool`，默认 `false`）。

将成员移出群。`groupId`、`userId` 分别标识群和成员；`rejectAddRequest` 默认为 `false`，用于控制是否拒绝该用户之后的加群请求。返回 `OneBot10Response`，规范没有响应数据。成员关系变化无法自动撤销。

```csharp
var response = await bot.Actions.SetGroupKickAsync(
    groupId: 987654321,
    userId: 123456789,
    rejectAddRequest: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupbanasync"></a>

### `SetGroupBanAsync` — 禁言或解除禁言群成员（`set_group_ban`）

**Action 专用参数：** `groupId`、`userId`（`long`）；`duration`（`long`，单位秒，默认 `1800`；`0` 表示解除禁言）。

禁言或解除单个群成员。`groupId`、`userId` 指定目标；`duration` 单位为秒，默认 `1800`，传 `0` 表示解除。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SetGroupBanAsync(
    groupId: 987654321,
    userId: 123456789,
    duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousbanasync"></a>

### `SetGroupAnonymousBanAsync` — 禁言匿名群成员（`set_group_anonymous_ban`）

**Action 专用参数：** `groupId`（`long`）；二选一传入 `anonymousFlag`（`string`，必填且非空）或 `anonymous`（`JsonObject`，必填且非空）；`duration`（`long`，单位秒，默认 `1800`）。

禁言匿名参与者。两个重载都接收 `groupId` 和以秒为单位的 `duration`（默认 `1800`）：一个接收从事件复制的非 `null` `anonymousFlag`，另一个接收完整、非 `null` 的匿名 `JsonObject`。返回 `OneBot10Response`，规范没有响应数据。只能使用目标事件的真实数据。

```csharp
var response = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId: 987654321,
    anonymousFlag: "从匿名事件复制的 flag",
    duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

完整对象重载会发送从目标事件复制的匿名 `id`、`name`、`flag` 以及其它实现端扩展字段：

```csharp
var anonymous = new JsonObject
{
    ["id"] = 10001,
    ["name"] = "匿名用户",
    ["flag"] = "从匿名事件复制的 flag"
};
var response = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId: 987654321,
    anonymous: anonymous,
    duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupwholebanasync"></a>

### `SetGroupWholeBanAsync` — 开启或关闭全群禁言（`set_group_whole_ban`）

**Action 专用参数：** `groupId`（`long`）；`enable`（`bool`，默认 `true`）。

开启或关闭全员禁言。`groupId` 标识群；`enable` 默认为 `true`，传 `false` 表示关闭。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SetGroupWholeBanAsync(
    groupId: 987654321,
    enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupadminasync"></a>

### `SetGroupAdminAsync` — 设置或取消群管理员（`set_group_admin`）

**Action 专用参数：** `groupId`、`userId`（`long`）；`enable`（`bool`，默认 `true`）。

设置或取消群管理员。`groupId`、`userId` 指定目标；`enable` 默认为 `true`，传 `false` 表示取消。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SetGroupAdminAsync(
    groupId: 987654321,
    userId: 123456789,
    enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousasync"></a>

### `SetGroupAnonymousAsync` — 开启或关闭群匿名聊天（`set_group_anonymous`）

**Action 专用参数：** `groupId`（`long`）；`enable`（`bool`，默认 `true`）。

开启或关闭群匿名聊天。`groupId` 标识群；`enable` 默认为 `true`，传 `false` 表示关闭。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SetGroupAnonymousAsync(
    groupId: 987654321,
    enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupcardasync"></a>

### `SetGroupCardAsync` — 设置群成员名片（`set_group_card`）

**Action 专用参数：** `groupId`、`userId`（`long`）；`card`（`string`，非空，默认为空字符串）。

设置或删除成员群名片。`groupId`、`userId` 指定成员；非 `null` 的 `card` 默认为空字符串，空值表示删除名片。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SetGroupCardAsync(
    groupId: 987654321,
    userId: 123456789,
    card: "新名片");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupleaveasync"></a>

### `SetGroupLeaveAsync` — 退出或解散群（`set_group_leave`）

**Action 专用参数：** `groupId`（`long`）；`isDismiss`（`bool`，默认 `false`）。

退出或解散群。`groupId` 标识群；`isDismiss` 默认为 `false`，群主传 `true` 可能不可逆地解散群。返回 `OneBot10Response`，规范没有响应数据。不要将它作为连通性测试。

```csharp
var response = await bot.Actions.SetGroupLeaveAsync(
    groupId: 987654321,
    isDismiss: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupspecialtitleasync"></a>

### `SetGroupSpecialTitleAsync` — 设置群成员专属头衔（`set_group_special_title`）

**Action 专用参数：** `groupId`、`userId`（`long`）；`specialTitle`（`string`，非空，默认为空字符串）；`duration`（`long`，单位秒，默认 `-1`）。

设置或删除成员专属头衔。`groupId`、`userId` 指定成员；非 `null` 的 `specialTitle` 默认空值（删除）；`duration` 单位为秒，默认 `-1`（实现端支持时表示永久）。返回 `OneBot10Response`，规范没有响应数据。

```csharp
var response = await bot.Actions.SetGroupSpecialTitleAsync(
    groupId: 987654321,
    userId: 123456789,
    specialTitle: "头衔",
    duration: -1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setdiscussleaveasync"></a>

### `SetDiscussLeaveAsync` — 退出讨论组（`set_discuss_leave`）

**Action 专用参数：** `discussId`（`long`）。

退出讨论组。`discussId` 标识目标讨论组。返回 `OneBot10Response`，规范没有响应数据。成员关系变化无法自动撤销。

```csharp
var response = await bot.Actions.SetDiscussLeaveAsync(discussId: 111222333);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setfriendaddrequestasync"></a>

### `SetFriendAddRequestAsync` — 处理加好友请求（`set_friend_add_request`）

**Action 专用参数：** `flag`（`string`，必填且非空）；`approve`（`bool`，默认 `true`）；`remark`（`string`，非空，默认为空字符串）。

同意或拒绝好友请求。`flag` 是请求事件给出的非 `null` flag；`approve` 默认为 `true`；非 `null` 的 `remark` 默认为空，并在同意时作为好友备注。返回 `OneBot10Response`，规范没有响应数据。处理结果对外可见，而且通常不能重放。

```csharp
var response = await bot.Actions.SetFriendAddRequestAsync(
    flag: "从好友请求事件复制的 flag",
    approve: true,
    remark: "好友备注");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupaddrequestasync"></a>

### `SetGroupAddRequestAsync` — 处理加群请求（`set_group_add_request`）

**Action 专用参数：** `flag`（`string`，必填且非空）；`requestType`（`OneBot10GroupRequestType`，必填）；`approve`（`bool`，默认 `true`）；`reason`（`string`，非空，默认为空字符串）。

同意或拒绝加群请求或邀请。非 `null` 的 `flag` 来自请求事件；`requestType` 为 `Add` 或 `Invite`；`approve` 默认为 `true`；非 `null` 的 `reason` 默认为空，并在拒绝时使用。返回 `OneBot10Response`，规范没有响应数据。处理结果对外可见且无法可靠重放，因此必须先核对事件 flag 和子类型。

```csharp
var response = await bot.Actions.SetGroupAddRequestAsync(
    flag: "从群请求事件复制的 flag",
    requestType: OneBot10GroupRequestType.Add,
    approve: true,
    reason: "");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getlogininfoasync"></a>

### `GetLoginInfoAsync` — 获取登录账号信息（`get_login_info`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

获取当前登录 QQ 账号，没有 Action 专用参数。返回 `OneBot10Response<OneBot10LoginInfoData>`，`Data` 中包含 `UserId` 和 `Nickname`。

```csharp
var response = await bot.Actions.GetLoginInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstrangerinfoasync"></a>

### `GetStrangerInfoAsync` — 获取陌生人信息（`get_stranger_info`）

**Action 专用参数：** `userId`（`long`）；`noCache`（`bool`，默认 `false`）。

获取 QQ 用户信息。`userId` 是 QQ 号；`noCache` 默认为 `false`，传 `true` 表示请求最新数据。返回 `OneBot10Response<OneBot10StrangerInfoData>`；实现端提供时，`Data` 包含 `UserId`、`Nickname`、`Sex` 和 `Age`。

```csharp
var response = await bot.Actions.GetStrangerInfoAsync(
    userId: 123456789,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfriendlistasync"></a>

### `GetFriendListAsync` — 获取好友列表（`get_friend_list`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

获取完整好友列表，没有 Action 专用参数。返回 `OneBot10Response<IReadOnlyList<OneBot10FriendInfo>>`；`Data` 中每项可包含 `UserId`、`Nickname` 和 `Remark`。

```csharp
var response = await bot.Actions.GetFriendListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouplistasync"></a>

### `GetGroupListAsync` — 获取群列表（`get_group_list`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

获取完整群列表，没有 Action 专用参数。返回 `OneBot10Response<IReadOnlyList<OneBot10GroupListItem>>`；存在对应字段时，`Data` 中每项包含解析后的 `GroupId` 和 `GroupName`。

```csharp
var response = await bot.Actions.GetGroupListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupinfoasync"></a>

### `GetGroupInfoAsync` — 获取群信息（`get_group_info`）

**Action 专用参数：** `groupId`（`long`）；`noCache`（`bool`，默认 `false`）。

获取单个群的信息。`groupId` 标识群；`noCache` 默认为 `false`，传 `true` 表示请求最新数据。返回 `OneBot10Response<OneBot10GroupInfo>`；存在对应字段时，`Data` 包含 `GroupId`、`GroupName`、`MemberCount` 和 `MaxMemberCount`。

```csharp
var response = await bot.Actions.GetGroupInfoAsync(
    groupId: 987654321,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberinfoasync"></a>

### `GetGroupMemberInfoAsync` — 获取群成员信息（`get_group_member_info`）

**Action 专用参数：** `groupId`、`userId`（`long`）；`noCache`（`bool`，默认 `false`）。

获取单个群成员的详细信息。`groupId`、`userId` 指定成员；`noCache` 默认为 `false`。返回 `OneBot10Response<OneBot10GroupMemberInfo>`；实现端提供时，`Data` 包含 ID、昵称、名片、角色、头衔、时间戳等成员字段。

```csharp
var response = await bot.Actions.GetGroupMemberInfoAsync(
    groupId: 987654321,
    userId: 123456789,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberlistasync"></a>

### `GetGroupMemberListAsync` — 获取群成员列表（`get_group_member_list`）

**Action 专用参数：** `groupId`（`long`）。

获取群成员列表。`groupId` 标识群。返回 `OneBot10Response<IReadOnlyList<OneBot10GroupMemberInfo>>`；实现端不同，`Data` 中每个成员的部分字段可能缺失。

```csharp
var response = await bot.Actions.GetGroupMemberListAsync(groupId: 987654321);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcookiesasync"></a>

### `GetCookiesAsync` — 获取 Cookies（`get_cookies`）

**Action 专用参数：** `domain`（`string`，非空，默认为空字符串）。

获取 QQ Cookies。非 `null` 的 `domain` 可限制目标域名，默认空字符串。返回 `OneBot10Response<OneBot10CookiesData>`，Cookie 字符串位于 `Data?.Cookies`。

```csharp
var response = await bot.Actions.GetCookiesAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcsrftokenasync"></a>

### `GetCsrfTokenAsync` — 获取 CSRF Token（`get_csrf_token`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

获取 QQ CSRF Token，没有 Action 专用参数。返回 `OneBot10Response<OneBot10CsrfTokenData>`，数值 Token 位于 `Data?.Token`。

```csharp
var response = await bot.Actions.GetCsrfTokenAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcredentialsasync"></a>

### `GetCredentialsAsync` — 获取 Cookies 与 CSRF 凭据（`get_credentials`）

**Action 专用参数：** `domain`（`string`，非空，默认为空字符串）。

同时获取 Cookies 和 CSRF Token。非 `null` 的 `domain` 可限制 Cookie 域名，默认空字符串。返回 `OneBot10Response<OneBot10CredentialsData>`，`Data` 中包含 `Cookies` 和 `CsrfToken`。

```csharp
var response = await bot.Actions.GetCredentialsAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getrecordasync"></a>

### `GetRecordAsync` — 获取或转换已接收语音（`get_record`）

**Action 专用参数：** `file`（`string`，必填且非空）；`outputFormat`（`OneBot10RecordFormat`，必填）；`fullPath`（`bool`，默认 `false`）。

获取并转换已接收语音。`file` 是接收消息段给出的非 `null` 文件名；`outputFormat` 可为 `Mp3`、`Amr`、`Wma`、`M4a`、`Spx`、`Ogg`、`Wav` 或 `Flac`；`fullPath` 默认为 `false`。返回 `OneBot10Response<OneBot10FileData>`，结果路径位于 `Data?.File`。

```csharp
var response = await bot.Actions.GetRecordAsync(
    file: "收到的语音文件名",
    outputFormat: OneBot10RecordFormat.Mp3,
    fullPath: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getimageasync"></a>

### `GetImageAsync` — 获取已接收图片（`get_image`）

**Action 专用参数：** `file`（`string`，必填且非空）。

获取已接收图片。`file` 是接收图片段给出的非 `null` 文件名。返回 `OneBot10Response<OneBot10FileData>`，结果路径位于 `Data?.File`。

```csharp
var response = await bot.Actions.GetImageAsync(file: "收到的图片文件名");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendimageasync"></a>

### `CanSendImageAsync` — 检查图片发送能力（`can_send_image`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

检查实现端是否可以发送图片，没有 Action 专用参数。返回 `OneBot10Response<OneBot10CapabilityData>`，能力结果位于 `Data?.Yes`。

```csharp
var response = await bot.Actions.CanSendImageAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendrecordasync"></a>

### `CanSendRecordAsync` — 检查语音发送能力（`can_send_record`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

检查实现端是否可以发送语音，没有 Action 专用参数。返回 `OneBot10Response<OneBot10CapabilityData>`，能力结果位于 `Data?.Yes`。

```csharp
var response = await bot.Actions.CanSendRecordAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstatusasync"></a>

### `GetStatusAsync` — 获取实现端状态（`get_status`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

获取实现端健康状态，没有 Action 专用参数。返回 `OneBot10Response<OneBot10StatusData>`；可移植字段包括 `Data?.Online` 和 `Data?.Good`，实现端给出的 CQHTTP 特定状态字段也会保留。

```csharp
var response = await bot.Actions.GetStatusAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getversioninfoasync"></a>

### `GetVersionInfoAsync` — 获取实现端版本信息（`get_version_info`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

获取 CQHTTP 插件和 CKYU 宿主版本信息，没有 Action 专用参数。返回 `OneBot10Response<OneBot10VersionInfoData>`；实现端提供时，其中包含目录、版本、插件版本、构建号和构建配置等字段。

```csharp
var response = await bot.Actions.GetVersionInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setrestartpluginasync"></a>

### `SetRestartPluginAsync` — 重启 CQHTTP 插件（`set_restart_plugin`）

**Action 专用参数：** `delay`（`long`，单位毫秒，默认 `0`）。

重启 CQHTTP 插件。`delay` 是实现端重启延迟毫秒数，默认 `0`。如果能够收到响应，则返回 `OneBot10Response` 且规范没有响应数据；重启可能在响应到达前中断连接。不要将它作为连通性测试。

```csharp
var response = await bot.Actions.SetRestartPluginAsync(delay: 2000);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cleandatadirectoryasync"></a>

### `CleanDataDirectoryAsync` — 清理 CQHTTP 数据目录（`clean_data_dir`）

**Action 专用参数：** `dataDirectory`（`OneBot10DataDirectory`，必填）。

永久删除一个 CQHTTP 数据目录中的文件。`dataDirectory` 必须为 `Image`、`Record`、`Show` 或 `Bface`。返回 `OneBot10Response`，规范没有响应数据。调用前请核对目录；删除无法自动撤销。

```csharp
var response = await bot.Actions.CleanDataDirectoryAsync(
    OneBot10DataDirectory.Image);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cleanpluginlogasync"></a>

### `CleanPluginLogAsync` — 清理 CQHTTP 插件日志（`clean_plugin_log`）

**Action 专用参数：** 无；仅接受上文三个公共可选参数。

永久清空 CQHTTP 插件日志。没有 Action 专用参数，返回 `OneBot10Response`，规范没有响应数据。它会删除诊断历史，且无法自动撤销。

```csharp
var response = await bot.Actions.CleanPluginLogAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

## 消息链

发送消息使用 `OneBot10SendMessage`，它本身就是有序消息链构建器。下面把文本和图片组合后发送到群：

```csharp
var message = new OneBot10SendMessage()
    .Text("图片如下：")
    .Image("https://example.com/cat.png");

var response = await bot.Actions.SendGroupMessageAsync(
    groupId: 123456789,
    message: message);
```

接收消息使用 `OneBot10ReceivedMessage`。通过 `MessageChain.OfType<T>()` 读取指定段；未知段会成为 `UnknownReceivedSegment`，不会丢失原始 JSON。

## 支持的消息段

以下代码均创建一个可加入 `OneBot10SendMessage` 的发送段。

### `text` 文本

```csharp
var segment = new TextSendSegment("你好");
```

参数 `text` 是要发送的文本，可以为空字符串，不能为 `null`。

### `face` QQ 表情

```csharp
var segment = new FaceSendSegment(14L);
```

参数 `id` 是协议表情 ID，可传 `long` 或字符串。

### `image` 图片

```csharp
var segment = new ImageSendSegment(
    file: "https://example.com/cat.png",
    cache: true,
    timeoutSeconds: 30);
```

`file` 可为已接收文件名、文件 URI、网络 URL 或 `base64://` URI；`cache` 控制实现端缓存；`timeoutSeconds` 是下载超时秒数。

### `record` 语音

```csharp
var segment = new RecordSendSegment(
    file: "file:///D:/audio/test.amr",
    magic: false,
    cache: true,
    timeoutSeconds: 30);
```

`file` 是语音来源；`magic` 是否使用变声；其余参数含义与图片相同。

### `at` 提及

```csharp
var user = new AtSendSegment(123456789L);
var everyone = new AtSendSegment("all");
```

参数是 QQ 号；字符串 `all` 表示全体成员。

### `rps` 猜拳

```csharp
var segment = new RpsSendSegment();
```

无需参数。

### `dice` 骰子

```csharp
var segment = new DiceSendSegment();
```

无需参数。

### `shake` 窗口抖动

```csharp
var segment = new ShakeSendSegment();
```

无需参数。

### `anonymous` 匿名标记

```csharp
var segment = new AnonymousSendSegment(ignoreFailure: true);
```

`ignoreFailure` 表示匿名发送失败时是否继续发送。这是仅发送类型。

### `share` 链接分享

```csharp
var segment = new ShareSendSegment(
    url: "https://example.com",
    title: "示例网站",
    content: "可选摘要",
    image: "https://example.com/cover.png");
```

`url`、`title` 必填；`content`、`image` 可省略。

### `contact` 推荐联系人或群

```csharp
var friend = new ContactSendSegment(OneBot10ContactTarget.Friend, "123456789");
var group = new ContactSendSegment(OneBot10ContactTarget.Group, "987654321");
```

第一个参数选择好友或群，`id` 是对应 QQ 号或群号。

### `location` 位置

```csharp
var segment = new LocationSendSegment(
    latitude: "39.9042",
    longitude: "116.4074",
    title: "北京",
    content: "可选说明");
```

经纬度使用字符串；`title`、`content` 可省略。

### `music` 音乐分享

```csharp
var providerMusic = new MusicSendSegment(OneBot10MusicProvider.QQ, "歌曲ID");
var customMusic = new CustomMusicSendSegment(
    url: "https://example.com/song",
    audio: "https://example.com/song.mp3",
    title: "歌曲名",
    content: "可选简介",
    image: "https://example.com/cover.png");
```

平台音乐的 `provider` 可选 `QQ`、`NetEase`、`Xiami`，`id` 是平台歌曲 ID；自定义音乐需要页面 URL、音频 URL 和标题。

### `rich` 富内容（仅接收）

```csharp
foreach (var rich in messageEvent.MessageChain.OfType<RichReceivedSegment>())
    Console.WriteLine(rich.Data);
```

规范没有固定参数，请从 `Data` 或 `RawJson` 读取实现端字段。

### 实现端扩展

```csharp
var segment = new CustomSendSegment(
    "markdown",
    new JsonObject { ["content"] = "**你好**" });
```

`type` 是扩展段名称，`data` 是实现端约定的参数。未知入站类型通过 `UnknownReceivedSegment` 保留。


## 控制台调试工程

这些可运行工程只用于调试。API 使用方式以本文上面的 Action 方法和接收事件章节为准，不需要先阅读控制台工程。

- [Observable 示例](../samples/OneBotSdk.Net.V10.ObservableExample)
- [EventHandler 示例](../samples/OneBotSdk.Net.V10.EventHandlerExample)
- [HTTP Action 示例](../samples/OneBotSdk.Net.V10.HttpActionExample)

不要把 Token 写入源码或日志；生产环境优先使用 HTTPS/WSS。
