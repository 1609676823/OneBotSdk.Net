# OneBot 11 使用指南

[返回中文 README](../README.zh-CN.md) | [文档目录](README.md) | [English](onebot-11.md)

> **OneBot 11 官方资料：** [官方仓库](https://github.com/botuniverse/onebot-11) · [公开 Action API](https://github.com/botuniverse/onebot-11/blob/master/api/public.md) · [事件文档](https://github.com/botuniverse/onebot-11/tree/master/event)

OneBot 11 API 位于 `OneBotSdk.Net.V11.*`。当前实现包含 38 个公开 Action、17 类标准具体事件、5 类具体未知回退事件和 20 种标准消息段。对于明确支持 OneBot 11 的 QQ 实现端，通常优先使用此版本。

## 参考目录

- [两个端点参数](#两个端点参数)
- [Action 方法](#action-方法)
- [接收事件](#接收事件)

### Action 方法目录

| 分类 | 方法 |
| --- | --- |
| 消息 | [`SendPrivateMessageAsync`](#sendprivatemessageasync)、[`SendGroupMessageAsync`](#sendgroupmessageasync)、[`SendMessageAsync`](#sendmessageasync)、[`DeleteMessageAsync`](#deletemessageasync)、[`GetMessageAsync`](#getmessageasync)、[`GetForwardMessageAsync`](#getforwardmessageasync)、[`SendLikeAsync`](#sendlikeasync) |
| 群管理 | [`SetGroupKickAsync`](#setgroupkickasync)、[`SetGroupBanAsync`](#setgroupbanasync)、[`SetGroupAnonymousBanAsync`](#setgroupanonymousbanasync)、[`SetGroupWholeBanAsync`](#setgroupwholebanasync)、[`SetGroupAdminAsync`](#setgroupadminasync)、[`SetGroupAnonymousAsync`](#setgroupanonymousasync)、[`SetGroupCardAsync`](#setgroupcardasync)、[`SetGroupNameAsync`](#setgroupnameasync)、[`SetGroupLeaveAsync`](#setgroupleaveasync)、[`SetGroupSpecialTitleAsync`](#setgroupspecialtitleasync) |
| 请求 | [`SetFriendAddRequestAsync`](#setfriendaddrequestasync)、[`SetGroupAddRequestAsync`](#setgroupaddrequestasync) |
| 信息 | [`GetLoginInfoAsync`](#getlogininfoasync)、[`GetStrangerInfoAsync`](#getstrangerinfoasync)、[`GetFriendListAsync`](#getfriendlistasync)、[`GetGroupInfoAsync`](#getgroupinfoasync)、[`GetGroupListAsync`](#getgrouplistasync)、[`GetGroupMemberInfoAsync`](#getgroupmemberinfoasync)、[`GetGroupMemberListAsync`](#getgroupmemberlistasync)、[`GetGroupHonorInfoAsync`](#getgrouphonorinfoasync) |
| 文件、能力与运行状态 | [`GetCookiesAsync`](#getcookiesasync)、[`GetCsrfTokenAsync`](#getcsrftokenasync)、[`GetCredentialsAsync`](#getcredentialsasync)、[`GetRecordAsync`](#getrecordasync)、[`GetImageAsync`](#getimageasync)、[`CanSendImageAsync`](#cansendimageasync)、[`CanSendRecordAsync`](#cansendrecordasync)、[`GetStatusAsync`](#getstatusasync)、[`GetVersionInfoAsync`](#getversioninfoasync)、[`SetRestartAsync`](#setrestartasync)、[`CleanCacheAsync`](#cleancacheasync) |
| 高级调用 | [`CallActionAsync`](#callactionasync)、[`HandleQuickOperationAsync`](#handlequickoperationasync) |

### 具体接收事件目录

| 分类 | 事件类型 |
| --- | --- |
| 消息 | [`PrivateMessageEvent`](#privatemessageevent)、[`GroupMessageEvent`](#groupmessageevent) |
| 通知 | [`GroupUploadNoticeEvent`](#groupuploadnoticeevent)、[`GroupAdminNoticeEvent`](#groupadminnoticeevent)、[`GroupDecreaseNoticeEvent`](#groupdecreasenoticeevent)、[`GroupIncreaseNoticeEvent`](#groupincreasenoticeevent)、[`GroupBanNoticeEvent`](#groupbannoticeevent)、[`FriendAddNoticeEvent`](#friendaddnoticeevent)、[`GroupRecallNoticeEvent`](#grouprecallnoticeevent)、[`FriendRecallNoticeEvent`](#friendrecallnoticeevent)、[`GroupPokeNoticeEvent`](#grouppokenoticeevent)、[`LuckyKingNoticeEvent`](#luckykingnoticeevent)、[`GroupHonorNoticeEvent`](#grouphonornoticeevent) |
| 请求 | [`FriendRequestEvent`](#friendrequestevent)、[`GroupRequestEvent`](#grouprequestevent) |
| 元事件 | [`LifecycleMetaEvent`](#lifecyclemetaevent)、[`HeartbeatMetaEvent`](#heartbeatmetaevent) |
| 未知回退 | [`UnknownOneBot11Event`](#unknownonebot11event)、[`UnknownMessageEvent`](#unknownmessageevent)、[`UnknownNoticeEvent`](#unknownnoticeevent)、[`UnknownRequestEvent`](#unknownrequestevent)、[`UnknownMetaEvent`](#unknownmetaevent) |

## 基本引用与启动

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

### 两个端点参数

`OneBot11BotOptions` 的这个构造函数固定组合 HTTP Action 与正向 WebSocket Event。两个参数分别表示：

| 构造参数 | 代表的通信方式 | 主动发起方 | 默认地址与用途 |
| --- | --- | --- | --- |
| `OneBot11ActionEndpointOptions actionEndpoint` | [`HTTP`](https://github.com/botuniverse/onebot-11/blob/master/communication/http.md)，按方向可理解为正向 HTTP | SDK → OneBot 实现端 | 主机/端口构造函数生成 `http://host:port/`；配置 Action 基础地址、Token 和响应限制，SDK 会在地址后追加 Action 名称。 |
| `OneBot11EventEndpointOptions eventEndpoint` | [`正向 WebSocket`](https://github.com/botuniverse/onebot-11/blob/master/communication/ws.md) | SDK → OneBot 实现端 | 主机/端口构造函数生成 `ws://host:port/event`；配置 Event 连接地址、Token 和 WebSocket 会话参数。 |

这两个参数不是正向/反向模式选择器：`actionEndpoint` 就是 HTTP Action，`eventEndpoint` 就是正向 WebSocket Event。

两个接受 `Uri` 的构造函数支持“反向代理路径”，这里的反向代理仅指 Nginx、Caddy 等部署层 URL 前缀，与 OneBot 的反向 HTTP 或反向 WebSocket 通信无关。

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

控制台示例不需要 `ManualResetEvent`。若业务确实使用它，应释放等待句柄，但可写成 `using var exit = new ManualResetEvent(false);`，无需嵌套 `using` 块。服务程序建议直接使用宿主的取消令牌。

## 接收事件

请在 `StartAsync()` 前订阅。所有事件都继承可空的 `Time`、`SelfId`、`PostType`，并在 `RawJson` 中保留完整来源对象。各强类型字段独立解析；实现端省略字段或字段格式异常时，相应属性可能为 `null`。以下代码使用 `bot.Events` 提供的具体 `EventHandler`；如果后续需要取消订阅，请保留委托实例。

**消息事件**

两类消息都继承 `MessageType`、`SubType`、`MessageId`、`UserId`、`MessageChain`、`RawMessage` 和 `Font`。

<a id="privatemessageevent"></a>

### `PrivateMessageEvent` — 私聊消息（`message/private`）

**订阅入口：** `bot.Events.PrivateMessageReceived`

表示私聊消息。关键字段包括 `UserId`、`MessageId`、`SubType`、`MessageChain`、`RawMessage` 和尽力解析的 `Sender` 信息。

```csharp
bot.Events.PrivateMessageReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.UserId}: {args.Event.MessageChain.PlainText}");
};
```

<a id="groupmessageevent"></a>

### `GroupMessageEvent` — 群消息（`message/group`）

**订阅入口：** `bot.Events.GroupMessageReceived`

表示群消息。除消息公共字段外，可读取 `GroupId`、可空的 `Anonymous` 和尽力解析的 `Sender`；匿名消息中的常规发送者字段不可靠。

```csharp
bot.Events.GroupMessageReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}/{args.Event.UserId}: {args.Event.MessageChain.PlainText}");
};
```

**通知事件**

每个通知除通用事件字段外，还继承可空的 `NoticeType` 判别值。

<a id="groupuploadnoticeevent"></a>

### `GroupUploadNoticeEvent` — 群文件上传（`notice/group_upload`）

**订阅入口：** `bot.Events.GroupUploadNoticeReceived`

表示群文件上传。关键字段为 `GroupId`、上传者 `UserId` 和 `File`；文件元数据包含 `Id`、`Name`、`Size`、`BusId`。

```csharp
bot.Events.GroupUploadNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.File?.Name} ({args.Event.File?.Size} bytes)");
};
```

<a id="groupadminnoticeevent"></a>

### `GroupAdminNoticeEvent` — 群管理员变动（`notice/group_admin`）

**订阅入口：** `bot.Events.GroupAdminNoticeReceived`

表示管理员身份被设置或取消。`SubType` 通常为 `set` 或 `unset`；`GroupId` 和 `UserId` 指定群与受影响成员。

```csharp
bot.Events.GroupAdminNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} {args.Event.SubType}");
};
```

<a id="groupdecreasenoticeevent"></a>

### `GroupDecreaseNoticeEvent` — 群成员减少（`notice/group_decrease`）

**订阅入口：** `bot.Events.GroupDecreaseNoticeReceived`

表示成员退出或被移出。`SubType` 通常为 `leave`、`kick` 或 `kick_me`；`GroupId`、`OperatorId`、`UserId` 分别指定群、操作者和离开成员。

```csharp
bot.Events.GroupDecreaseNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} 离开（{args.Event.SubType}）");
};
```

<a id="groupincreasenoticeevent"></a>

### `GroupIncreaseNoticeEvent` — 群成员增加（`notice/group_increase`）

**订阅入口：** `bot.Events.GroupIncreaseNoticeReceived`

表示成员加入群。`SubType` 通常为 `approve` 或 `invite`；`GroupId`、`OperatorId`、`UserId` 分别指定群、操作者和新成员。

```csharp
bot.Events.GroupIncreaseNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} 加入（{args.Event.SubType}）");
};
```

<a id="groupbannoticeevent"></a>

### `GroupBanNoticeEvent` — 群禁言（`notice/group_ban`）

**订阅入口：** `bot.Events.GroupBanNoticeReceived`

表示禁言状态变化。`SubType` 通常为 `ban` 或 `lift_ban`；`GroupId`、`OperatorId`、`UserId` 和 `Duration` 描述目标及禁言秒数。

```csharp
bot.Events.GroupBanNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.UserId}: {args.Event.SubType}，{args.Event.Duration} 秒");
};
```

<a id="friendaddnoticeevent"></a>

### `FriendAddNoticeEvent` — 好友添加（`notice/friend_add`）

**订阅入口：** `bot.Events.FriendAddNoticeReceived`

表示新增好友。`UserId` 是新好友 QQ 号。

```csharp
bot.Events.FriendAddNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"新增好友：{args.Event.UserId}");
};
```

<a id="grouprecallnoticeevent"></a>

### `GroupRecallNoticeEvent` — 群消息撤回（`notice/group_recall`）

**订阅入口：** `bot.Events.GroupRecallNoticeReceived`

表示群消息撤回。`GroupId`、原发送者 `UserId`、撤回操作者 `OperatorId`、`MessageId` 用于标识此次操作。

```csharp
bot.Events.GroupRecallNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.OperatorId} 撤回 {args.Event.MessageId}");
};
```

<a id="friendrecallnoticeevent"></a>

### `FriendRecallNoticeEvent` — 好友消息撤回（`notice/friend_recall`）

**订阅入口：** `bot.Events.FriendRecallNoticeReceived`

表示私聊消息撤回。`UserId` 指定好友，`MessageId` 指定被撤回的消息。

```csharp
bot.Events.FriendRecallNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.UserId}: 撤回 {args.Event.MessageId}");
};
```

<a id="grouppokenoticeevent"></a>

### `GroupPokeNoticeEvent` — 群内戳一戳（`notice/notify/poke`）

**订阅入口：** `bot.Events.GroupPokeNoticeReceived`

表示群内戳一戳。`SubType` 通常为 `poke`；`GroupId`、发起者 `UserId`、`TargetId` 指定参与者。

```csharp
bot.Events.GroupPokeNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} 戳了 {args.Event.TargetId}");
};
```

<a id="luckykingnoticeevent"></a>

### `LuckyKingNoticeEvent` — 群红包运气王提示（`notice/notify/lucky_king`）

**订阅入口：** `bot.Events.LuckyKingNoticeReceived`

表示群红包运气王。关键字段为 `GroupId`、红包发送者 `UserId` 和运气王 `TargetId`。

```csharp
bot.Events.LuckyKingNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: 运气王 {args.Event.TargetId}");
};
```

<a id="grouphonornoticeevent"></a>

### `GroupHonorNoticeEvent` — 群成员荣誉变更提示（`notice/notify/honor`）

**订阅入口：** `bot.Events.GroupHonorNoticeReceived`

表示群荣誉变化。`GroupId`、`UserId` 指定群和成员；`HonorType` 通常为 `talkative`、`performer` 或 `emotion`，`SubType` 通常为 `honor`。

```csharp
bot.Events.GroupHonorNoticeReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} 获得 {args.Event.HonorType}");
};
```

**请求事件**

每个请求都继承 `RequestType`、`UserId`、`Comment` 和 `Flag`。请求 flag 是不透明且可能敏感的令牌；请原样保留，并且只在获得授权时将其传给匹配的请求处理 Action。

<a id="friendrequestevent"></a>

### `FriendRequestEvent` — 加好友请求（`request/friend`）

**订阅入口：** `bot.Events.FriendRequestReceived`

表示好友添加请求。从 `OneBot11RequestEvent` 继承的关键字段为请求者 `UserId`、验证信息 `Comment` 和处理用 `Flag`。

```csharp
bot.Events.FriendRequestReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"好友请求 {args.Event.UserId}: {args.Event.Comment}");
};
```

<a id="grouprequestevent"></a>

### `GroupRequestEvent` — 加群请求／邀请（`request/group`）

**订阅入口：** `bot.Events.GroupRequestReceived`

表示加群请求或邀请。`SubType` 通常为 `add` 或 `invite`；`GroupId`、请求者 `UserId`、`Comment`、`Flag` 描述并标识该请求。

```csharp
bot.Events.GroupRequestReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"{args.Event.GroupId}: {args.Event.UserId} 发起 {args.Event.SubType}");
};
```

**元事件**

两类元事件都继承可空的 `MetaEventType` 判别值。

<a id="lifecyclemetaevent"></a>

### `LifecycleMetaEvent` — 生命周期（`meta_event/lifecycle`）

**订阅入口：** `bot.Events.LifecycleMetaEventReceived`

表示实现端生命周期变化。`SubType` 通常为 `enable`、`disable` 或 `connect`；`MetaEventType` 从元事件基类继承。

```csharp
bot.Events.LifecycleMetaEventReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"生命周期：{args.Event.SubType}");
};
```

<a id="heartbeatmetaevent"></a>

### `HeartbeatMetaEvent` — 心跳（`meta_event/heartbeat`）

**订阅入口：** `bot.Events.HeartbeatMetaEventReceived`

表示周期性运行状态。`Interval` 是距离下一次心跳的毫秒数；可空的 `Status?.Online` 和 `Status?.Good` 是可移植状态字段。

```csharp
bot.Events.HeartbeatMetaEventReceived += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    Console.WriteLine($"心跳：online={args.Event.Status?.Online}, good={args.Event.Status?.Good}");
};
```

**未知回退事件**

未知回退类型会保留完整 `RawJson`，并通过 `UnknownEventDispatched` 和 `bot.UnknownEventReceived` 分发，因此新版或实现端特有判别值不会丢失。

<a id="unknownonebot11event"></a>

### `UnknownOneBot11Event` — 未知顶层事件（未知 `post_type`）

**订阅入口：** `bot.Events.UnknownEventDispatched`（再匹配 `UnknownOneBot11Event`）

顶层 `post_type` 未知时使用。请检查 `PostType` 和 `RawJson`。

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownOneBot11Event e)
        Console.WriteLine($"未知 post_type：{e.PostType}");
};
```

<a id="unknownmessageevent"></a>

### `UnknownMessageEvent` — 未知消息类型（`message/*`）

**订阅入口：** `bot.Events.UnknownEventDispatched`（再匹配 `UnknownMessageEvent`）

`post_type` 为 `message`、但 `message_type` 未知时使用。它保留 `MessageType`、`UserId`、`MessageChain`、`RawMessage` 等消息公共字段。

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownMessageEvent e)
        Console.WriteLine($"未知 message_type：{e.MessageType}");
};
```

<a id="unknownnoticeevent"></a>

### `UnknownNoticeEvent` — 未知通知组合（`notice/*` 或 `notice/notify/*`）

**订阅入口：** `bot.Events.UnknownEventDispatched`（再匹配 `UnknownNoticeEvent`）

通知判别值组合未知时使用。请检查 `NoticeType`、可选 `SubType` 和 `RawJson`。

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownNoticeEvent e)
        Console.WriteLine($"未知通知：{e.NoticeType}/{e.SubType}");
};
```

<a id="unknownrequestevent"></a>

### `UnknownRequestEvent` — 未知请求类型（`request/*`）

**订阅入口：** `bot.Events.UnknownEventDispatched`（再匹配 `UnknownRequestEvent`）

`request_type` 未知时使用。它保留 `RequestType`、可选 `SubType`、`UserId`、`Comment`、`Flag` 和 `RawJson`。

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownRequestEvent e)
        Console.WriteLine($"未知请求：{e.RequestType}/{e.SubType}");
};
```

<a id="unknownmetaevent"></a>

### `UnknownMetaEvent` — 未知元事件类型（`meta_event/*`）

**订阅入口：** `bot.Events.UnknownEventDispatched`（再匹配 `UnknownMetaEvent`）

`meta_event_type` 未知时使用。请检查 `MetaEventType`、可选 `SubType` 和 `RawJson`。

```csharp
bot.Events.UnknownEventDispatched += (_, args) =>
{
    Console.WriteLine(args.Event.RawJson.ToJsonString());
    if (args.Event is UnknownMetaEvent e)
        Console.WriteLine($"未知元事件：{e.MetaEventType}/{e.SubType}");
};
```

## Action 方法

`OneBot11Client` 提供 40 个 Action 方法名（共 45 个重载）：38 个 OneBot 11 公开 Action 均有强类型方法，此外还提供高级调用所需的 `CallActionAsync` 和 `HandleQuickOperationAsync`。

除非另有说明，每个方法末尾都有以下可选参数：

- `invocationMode`（`InvocationMode`，默认 `InvocationMode.Normal`）：`Normal` 调用基础 Action 并等待结果；`Async` 添加 `_async` 后缀，协议只报告已受理，不会返回该 Action 的最终结果；`RateLimited` 添加 `_rate_limited` 后缀，并按实现端配置的速率排队执行。
- `echo`（`JsonNode?`，默认 `null`）：可选关联数据；WebSocket 传输会把它放入 Action 信封，HTTP Action 传输则会忽略它，因为 HTTP 请求与响应已自然配对。
- `cancellationToken`（`CancellationToken`，默认 `default`）：取消传输操作。

所有响应都提供 `IsSuccess`、`Status`、`RetCode`、`RawRequestJson` 和 `RawResponseJson`；泛型响应还提供强类型 `Data` 和未投影的 `RawData`。以下示例不使用公共输出函数，只直接输出传输层保留的原始请求与响应 JSON。

原始报文可能包含私聊内容、Cookies、CSRF Token 等敏感值。请只在受控诊断环境中输出，并在分享日志前进行脱敏。

这些代码片段可以直接执行，并非无副作用的探测。发送或撤回消息、点赞、处理好友/加群请求、群管理调用、`CleanCacheAsync`、`SetRestartAsync` 和 `HandleQuickOperationAsync` 都可能改变外部状态；请只对预期目标执行，并确保有权处理所使用的事件数据。

代码片段假设已按前文创建 `bot`，并使用以下示例变量。请将 flag 和文件名替换为实现端事件或响应中的真实值。

```csharp
long userId = 123456789;
long groupId = 987654321;
long messageId = 123;
string requestFlag = "事件中的 flag";
string anonymousFlag = "匿名事件中的 flag";
string forwardId = "合并转发 ID";
string imageFile = "收到的图片文件名";
string recordFile = "收到的语音文件名";
var message = new OneBot11SendMessage().Text("你好");
```

**消息 Action**

<a id="sendprivatemessageasync"></a>

### `SendPrivateMessageAsync` — 发送私聊消息（`send_private_msg`）

**作用。** 向一个 QQ 用户发送私聊消息。

**参数。** `userId`（`long`，必填）是接收者 QQ 号；`message`（`OneBot11SendMessage`，不能为 `null`）是待发送消息；`autoEscape`（`bool`，默认 `false`）为 true 时要求实现端把字符串消息当作纯文本处理。

**返回。** `OneBot11Response<OneBot11SendMessageResult>`；`Data?.MessageId` 是实现端分配的消息 ID。

```csharp
var response = await bot.Actions.SendPrivateMessageAsync(userId, message);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

兼容重载把 `message` 替换为不能为 `null` 的 `OneBot11Message`；`userId` 仍是必填 `long`，`autoEscape` 仍是默认 `false` 的 `bool`。它返回 `OneBot11Response<OneBot11SendMessageData>`，其中 `Data?.MessageId` 含义相同。新代码优先使用 `OneBot11SendMessage`。

```csharp
var compatibilityResponse = await bot.Actions.SendPrivateMessageAsync(
    userId,
    OneBot11Message.FromString("你好"));
Console.WriteLine(compatibilityResponse.RawRequestJson);
Console.WriteLine(compatibilityResponse.RawResponseJson);
```

<a id="sendgroupmessageasync"></a>

### `SendGroupMessageAsync` — 发送群消息（`send_group_msg`）

**作用。** 向群发送消息。

**参数。** `groupId`（`long`，必填）是目标群号；`message`（`OneBot11SendMessage`，不能为 `null`）是待发送消息；`autoEscape`（`bool`，默认 `false`）控制字符串消息转义。

**返回。** `OneBot11Response<OneBot11SendMessageResult>`；`Data?.MessageId` 是实现端分配的消息 ID。

```csharp
var response = await bot.Actions.SendGroupMessageAsync(groupId, message);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

兼容重载把 `message` 替换为不能为 `null` 的 `OneBot11Message`；`groupId` 仍是必填 `long`，`autoEscape` 仍是默认 `false` 的 `bool`。它返回 `OneBot11Response<OneBot11SendMessageData>`。新代码优先使用仅发送模型。

```csharp
var compatibilityResponse = await bot.Actions.SendGroupMessageAsync(
    groupId,
    OneBot11Message.FromString("你好"));
Console.WriteLine(compatibilityResponse.RawRequestJson);
Console.WriteLine(compatibilityResponse.RawResponseJson);
```

<a id="sendmessageasync"></a>

### `SendMessageAsync` — 发送消息（`send_msg`）

**作用。** 目标类型在运行时确定时，通过通用 `send_msg` Action 发送消息。

**参数。** `message`（`OneBot11SendMessage`，不能为 `null`）是待发送消息；`messageType`（`OneBot11MessageType?`，默认 `null`）可为 `Private`、`Group` 或 `null`；`userId`（`long?`，默认 `null`）和 `groupId`（`long?`，默认 `null`）是可空目标 ID，应传入与 `messageType` 匹配的一项；`autoEscape`（`bool`，默认 `false`）控制字符串消息转义。客户端只序列化有值的目标字段，因此调用方应提供实现端可接受的目标组合。

**返回。** `OneBot11Response<OneBot11SendMessageResult>`；发送后的消息 ID 位于 `Data?.MessageId`。

```csharp
var response = await bot.Actions.SendMessageAsync(
    message,
    OneBot11MessageType.Group,
    groupId: groupId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

兼容重载把 `message` 替换为不能为 `null` 的 `OneBot11Message`；可空目标参数和 `autoEscape` 沿用上面列出的类型与默认值。它返回 `OneBot11Response<OneBot11SendMessageData>`。

```csharp
var compatibilityResponse = await bot.Actions.SendMessageAsync(
    OneBot11Message.FromString("你好"),
    OneBot11MessageType.Group,
    groupId: groupId);
Console.WriteLine(compatibilityResponse.RawRequestJson);
Console.WriteLine(compatibilityResponse.RawResponseJson);
```

<a id="deletemessageasync"></a>

### `DeleteMessageAsync` — 撤回消息（`delete_msg`）

**作用。** 撤回一条已发送或已接收的消息。

**参数。** `messageId`（`long`，必填）是要撤回的 OneBot 消息 ID。

**返回。** `OneBot11Response`。标准成功响应没有强类型数据载荷，可检查 `IsSuccess`/`RetCode` 或原始响应。

```csharp
var response = await bot.Actions.DeleteMessageAsync(messageId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getmessageasync"></a>

### `GetMessageAsync` — 获取消息（`get_msg`）

**作用。** 按 OneBot 消息 ID 获取一条消息。

**参数。** `messageId`（`long`，必填）是要获取的消息 ID。

**返回。** `OneBot11Response<OneBot11MessageData>`。`Data` 包含 `Time`、`MessageType`、`MessageId`、`RealId`、`Sender` 和解析后的接收 `MessageChain`。

```csharp
var response = await bot.Actions.GetMessageAsync(messageId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getforwardmessageasync"></a>

### `GetForwardMessageAsync` — 获取合并转发消息（`get_forward_msg`）

**作用。** 获取一条合并转发消息的内容。

**参数。** `id`（`string`，不能为 `null`）是从已接收转发消息段中取得的合并转发 ID。

**返回。** `OneBot11Response<OneBot11ForwardMessageData>`；接收到的转发节点位于 `Data?.MessageChain`。

```csharp
var response = await bot.Actions.GetForwardMessageAsync(forwardId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="sendlikeasync"></a>

### `SendLikeAsync` — 发送好友赞（`send_like`）

**作用。** 向好友发送一次或多次赞。

**参数。** `userId`（`long`，必填）是好友 QQ 号；`times`（`long`，默认 `1`）是点赞次数。OneBot 规定每位好友每天最多十次。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SendLikeAsync(userId, times: 1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**群管理 Action**

以下方法会修改外部状态，请只对确实需要执行这些操作的账号和群进行测试。

<a id="setgroupkickasync"></a>

### `SetGroupKickAsync` — 群组踢人（`set_group_kick`）

**作用。** 将一个成员移出群。

**参数。** `groupId`（`long`，必填）指定群；`userId`（`long`，必填）指定成员；`rejectAddRequest`（`bool`，默认 `false`）表示是否拒绝该成员后续的加群请求。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。成员关系变化无法自动撤销；绝不能把当前登录账号或群主作为自动测试目标。

```csharp
var response = await bot.Actions.SetGroupKickAsync(
    groupId,
    userId,
    rejectAddRequest: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupbanasync"></a>

### `SetGroupBanAsync` — 群组单人禁言（`set_group_ban`）

**作用。** 禁言或解除禁言一个群成员。

**参数。** `groupId`（`long`，必填）和 `userId`（`long`，必填）指定成员；`duration`（`long`，默认 `1800`）是禁言秒数，传 `0` 表示解除禁言。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupBanAsync(groupId, userId, duration: 60);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousbanasync"></a>

### `SetGroupAnonymousBanAsync` — 群组匿名用户禁言（`set_group_anonymous_ban`）

**作用。** 禁言群内匿名成员。

**参数。** 两个重载都接收 `groupId`（`long`，必填）和禁言秒数 `duration`（`long`，默认 `1800`）。一个重载要求事件中的 `anonymousFlag`（`string`，不能为 `null`）并发送 `anonymous_flag`；另一个要求完整事件 `anonymous`（`JsonObject`，不能为 `null`）并以 `anonymous` 发送。

**返回。** 两个重载都返回 `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var flagResponse = await bot.Actions.SetGroupAnonymousBanAsync(
    groupId,
    anonymousFlag,
    duration: 60);
Console.WriteLine(flagResponse.RawRequestJson);
Console.WriteLine(flagResponse.RawResponseJson);
```

完整对象重载会把传入的 `JsonObject` 作为 `anonymous` 发送：

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

### `SetGroupWholeBanAsync` — 群组全员禁言（`set_group_whole_ban`）

**作用。** 开启或关闭全员禁言。

**参数。** `groupId`（`long`，必填）指定群；`enable`（`bool`，默认 `true`）为 true 时开启全员禁言。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupWholeBanAsync(groupId, enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupadminasync"></a>

### `SetGroupAdminAsync` — 群组设置管理员（`set_group_admin`）

**作用。** 设置或取消群管理员。

**参数。** `groupId`（`long`，必填）和 `userId`（`long`，必填）指定成员；`enable`（`bool`，默认 `true`）为 true 时设置管理员，为 false 时取消管理员。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupAdminAsync(groupId, userId, enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupanonymousasync"></a>

### `SetGroupAnonymousAsync` — 群组匿名（`set_group_anonymous`）

**作用。** 开启或关闭群匿名聊天。

**参数。** `groupId`（`long`，必填）指定群；`enable`（`bool`，默认 `true`）为 true 时开启匿名聊天。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupAnonymousAsync(groupId, enable: true);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupcardasync"></a>

### `SetGroupCardAsync` — 设置群名片（群备注）（`set_group_card`）

**作用。** 设置或删除成员群名片。

**参数。** `groupId`（`long`，必填）和 `userId`（`long`，必填）指定成员；`card`（`string`，不能为 `null`，默认 `""`）是新群名片，空字符串表示删除当前群名片。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupCardAsync(groupId, userId, card: "新名片");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupnameasync"></a>

### `SetGroupNameAsync` — 设置群名（`set_group_name`）

**作用。** 修改群名称。

**参数。** `groupId`（`long`，必填）指定群；`groupName`（`string`，不能为 `null`）是新群名。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupNameAsync(groupId, groupName: "新群名");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupleaveasync"></a>

### `SetGroupLeaveAsync` — 退出群组（`set_group_leave`）

**作用。** 退出群；当前登录账号为群主时可请求解散群。

**参数。** `groupId`（`long`，必填）指定群；`isDismiss`（`bool`，默认 `false`）为 true 时请求解散群。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。此操作具有破坏性且无法自动撤销；部分实现端即使在 `isDismiss` 为 false 时也可能以不同方式处理群主退出请求。不要在自动示例/测试中运行，也不要对必须保留的群运行。

```csharp
var response = await bot.Actions.SetGroupLeaveAsync(groupId, isDismiss: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupspecialtitleasync"></a>

### `SetGroupSpecialTitleAsync` — 设置群组专属头衔（`set_group_special_title`）

**作用。** 设置或删除成员群专属头衔。

**参数。** `groupId`（`long`，必填）和 `userId`（`long`，必填）指定成员；`specialTitle`（`string`，不能为 `null`，默认 `""`）设置头衔，空字符串表示删除；`duration`（`long`，默认 `-1`）单位为秒，实现端支持时表示永久。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupSpecialTitleAsync(
    groupId,
    userId,
    specialTitle: "头衔",
    duration: -1);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**请求处理 Action**

<a id="setfriendaddrequestasync"></a>

### `SetFriendAddRequestAsync` — 处理加好友请求（`set_friend_add_request`）

**作用。** 同意或拒绝好友请求。

**参数。** `flag`（`string`，不能为 `null`）必须使用 `FriendRequestEvent` 中的原始值；`approve`（`bool`，默认 `true`）表示同意或拒绝；`remark`（`string`，不能为 `null`，默认 `""`）在同意时作为好友备注。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetFriendAddRequestAsync(
    requestFlag,
    approve: true,
    remark: "备注");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setgroupaddrequestasync"></a>

### `SetGroupAddRequestAsync` — 处理加群请求／邀请（`set_group_add_request`）

**作用。** 同意或拒绝加群请求或邀请。

**参数。** `flag`（`string`，不能为 `null`）必须使用 `GroupRequestEvent` 中的原始值；`requestType`（`OneBot11GroupRequestType`，必填）在用户申请加群时传 `Add`，机器人被邀请时传 `Invite`；`approve`（`bool`，默认 `true`）表示同意或拒绝；`reason`（`string`，不能为 `null`，默认 `""`）是拒绝理由。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.SetGroupAddRequestAsync(
    requestFlag,
    OneBot11GroupRequestType.Add,
    approve: true,
    reason: "");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**信息查询 Action**

<a id="getlogininfoasync"></a>

### `GetLoginInfoAsync` — 获取登录号信息（`get_login_info`）

**作用。** 获取实现端当前登录的 QQ 账号。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<OneBot11LoginInfoData>`；`Data` 包含 `UserId` 和 `Nickname`。

```csharp
var response = await bot.Actions.GetLoginInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstrangerinfoasync"></a>

### `GetStrangerInfoAsync` — 获取陌生人信息（`get_stranger_info`）

**作用。** 获取一个 QQ 用户的公开信息，该用户不必是好友。

**参数。** `userId`（`long`，必填）是 QQ 号；`noCache`（`bool`，默认 `false`）为 true 时要求获取最新信息。

**返回。** `OneBot11Response<OneBot11StrangerInfoData>`；实现端提供时，`Data` 包含 `UserId`、`Nickname`、`Sex` 和 `Age`。

```csharp
var response = await bot.Actions.GetStrangerInfoAsync(userId, noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getfriendlistasync"></a>

### `GetFriendListAsync` — 获取好友列表（`get_friend_list`）

**作用。** 获取当前登录账号可见的全部好友。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<IReadOnlyList<OneBot11FriendInfo>>`；每项可包含 `UserId`、`Nickname` 和 `Remark`。

```csharp
var response = await bot.Actions.GetFriendListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupinfoasync"></a>

### `GetGroupInfoAsync` — 获取群信息（`get_group_info`）

**作用。** 获取一个群的信息。

**参数。** `groupId`（`long`，必填）指定群；`noCache`（`bool`，默认 `false`）为 true 时要求获取最新信息。

**返回。** `OneBot11Response<OneBot11GroupInfo>`；`Data` 可包含 `GroupId`、`GroupName`、`MemberCount` 和 `MaxMemberCount`。

```csharp
var response = await bot.Actions.GetGroupInfoAsync(groupId, noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouplistasync"></a>

### `GetGroupListAsync` — 获取群列表（`get_group_list`）

**作用。** 获取当前登录账号可见的全部群。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<IReadOnlyList<OneBot11GroupInfo>>`；实现端提供时，每项包含与 `GetGroupInfoAsync` 相同的字段。

```csharp
var response = await bot.Actions.GetGroupListAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberinfoasync"></a>

### `GetGroupMemberInfoAsync` — 获取群成员信息（`get_group_member_info`）

**作用。** 获取一个群成员的详细信息。

**参数。** `groupId`（`long`，必填）和 `userId`（`long`，必填）指定成员；`noCache`（`bool`，默认 `false`）为 true 时要求获取最新信息。

**返回。** `OneBot11Response<OneBot11GroupMemberInfo>`。`Data` 可包含群与用户 ID、昵称/群名片、资料字段、入群与最后发言时间、等级/角色、`Unfriendly`、头衔信息和群名片修改权限。

```csharp
var response = await bot.Actions.GetGroupMemberInfoAsync(
    groupId,
    userId,
    noCache: false);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgroupmemberlistasync"></a>

### `GetGroupMemberListAsync` — 获取群成员列表（`get_group_member_list`）

**作用。** 获取一个群的成员列表。

**参数。** `groupId`（`long`，必填）指定群。

**返回。** `OneBot11Response<IReadOnlyList<OneBot11GroupMemberInfo>>`。列表响应中的部分成员字段可能缺失。

```csharp
var response = await bot.Actions.GetGroupMemberListAsync(groupId);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getgrouphonorinfoasync"></a>

### `GetGroupHonorInfoAsync` — 获取群荣誉信息（`get_group_honor_info`）

**作用。** 获取一种或全部标准群荣誉类别。

**参数。** `groupId`（`long`，必填）指定群；`honorType`（`OneBot11GroupHonorType`，必填）可为 `Talkative`、`Performer`、`Legend`、`StrongNewbie`、`Emotion` 或 `All`。

**返回。** `OneBot11Response<OneBot11GroupHonorInfoData>`。`Data` 包含 `GroupId`；其余字段会按 `honorType` 条件出现，包括 `CurrentTalkative`、`TalkativeList`、`PerformerList`、`LegendList`、`StrongNewbieList` 和 `EmotionList`。

```csharp
var response = await bot.Actions.GetGroupHonorInfoAsync(
    groupId,
    OneBot11GroupHonorType.All);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**文件、能力与运行状态 Action**

<a id="getcookiesasync"></a>

### `GetCookiesAsync` — 获取 Cookies（`get_cookies`）

**作用。** 获取 QQ Cookies，可选择限定域名。

**参数。** `domain`（`string`，不能为 `null`，默认 `""`）可限定域名；空字符串表示不限定。

**返回。** `OneBot11Response<OneBot11CookiesData>`；Cookie 请求头值位于 `Data?.Cookies`。

```csharp
var response = await bot.Actions.GetCookiesAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcsrftokenasync"></a>

### `GetCsrfTokenAsync` — 获取 CSRF Token（`get_csrf_token`）

**作用。** 获取当前 QQ CSRF Token。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<OneBot11CsrfTokenData>`；Token 位于 `Data?.Token`。

```csharp
var response = await bot.Actions.GetCsrfTokenAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getcredentialsasync"></a>

### `GetCredentialsAsync` — 获取 QQ 相关接口凭证（`get_credentials`）

**作用。** 一次获取 Cookies 和 CSRF Token。

**参数。** `domain`（`string`，不能为 `null`，默认 `""`）可限定凭据所属域名；空字符串表示不限定。

**返回。** `OneBot11Response<OneBot11CredentialsData>`；使用 `Data?.Cookies` 和 `Data?.CsrfToken` 读取结果。

```csharp
var response = await bot.Actions.GetCredentialsAsync(domain: "example.com");
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getrecordasync"></a>

### `GetRecordAsync` — 获取语音（`get_record`）

**作用。** 获取并转换一份已接收的语音文件。

**参数。** `file`（`string`，不能为 `null`）是已接收语音段中的 file 值；`outputFormat`（`OneBot11RecordFormat`，必填）可为 `Mp3`、`Amr`、`Wma`、`M4a`、`Spx`、`Ogg`、`Wav` 或 `Flac`。

**返回。** `OneBot11Response<OneBot11FileData>`；`Data?.File` 是转换后文件在实现端本机上的路径。

```csharp
var response = await bot.Actions.GetRecordAsync(
    recordFile,
    OneBot11RecordFormat.Mp3);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getimageasync"></a>

### `GetImageAsync` — 获取图片（`get_image`）

**作用。** 获取一份已接收的图片文件。

**参数。** `file`（`string`，不能为 `null`）是已接收图片段中的 file 值。

**返回。** `OneBot11Response<OneBot11FileData>`；`Data?.File` 是文件在实现端本机上的路径。

```csharp
var response = await bot.Actions.GetImageAsync(imageFile);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendimageasync"></a>

### `CanSendImageAsync` — 检查是否可以发送图片（`can_send_image`）

**作用。** 检查实现端是否可以发送图片。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<OneBot11CapabilityData>`；能力检查结果位于 `Data?.Yes`。

```csharp
var response = await bot.Actions.CanSendImageAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cansendrecordasync"></a>

### `CanSendRecordAsync` — 检查是否可以发送语音（`can_send_record`）

**作用。** 检查实现端是否可以发送语音。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<OneBot11CapabilityData>`；能力检查结果位于 `Data?.Yes`。

```csharp
var response = await bot.Actions.CanSendRecordAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getstatusasync"></a>

### `GetStatusAsync` — 获取运行状态（`get_status`）

**作用。** 获取实现端的可移植健康状态和登录状态。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<OneBot11StatusData>`；`Data?.Online` 表示 QQ 登录状态，`Data?.Good` 表示整体健康状态；实现端扩展字段保留在 `RawData`。

```csharp
var response = await bot.Actions.GetStatusAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="getversioninfoasync"></a>

### `GetVersionInfoAsync` — 获取版本信息（`get_version_info`）

**作用。** 获取实现端和 OneBot 协议版本信息。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response<OneBot11VersionInfoData>`；`Data` 包含 `AppName`、`AppVersion` 和 `ProtocolVersion`。

```csharp
var response = await bot.Actions.GetVersionInfoAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="setrestartasync"></a>

### `SetRestartAsync` — 重启 OneBot 实现（`set_restart`）

**作用。** 请求实现端执行其自身具有异步性质的重启。

**参数。** `delay`（`long`，默认 `0`）是重启延迟毫秒数。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。请求成功后，终结点可能暂时不可用。

```csharp
var response = await bot.Actions.SetRestartAsync(delay: 0);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

<a id="cleancacheasync"></a>

### `CleanCacheAsync` — 清理缓存（`clean_cache`）

**作用。** 请求实现端清理缓存文件。

**参数。** 没有 Action 专用参数。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var response = await bot.Actions.CleanCacheAsync();
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

**高级 Action 调用**

<a id="callactionasync"></a>

### `CallActionAsync` — 动态调用 Action（运行时由 `action` 指定）

**作用。** 按名称调用标准或实现端扩展 Action；没有匹配的强类型方法时使用此入口。

**参数。** 两个重载都要求 `action`（`string`）不能为 `null`、空字符串或纯空白；`parameters`（`JsonObject?`，默认 `null`）提供 Action 参数。它们也接收公共尾参。泛型重载还要求 `dataParser`（`Func<JsonNode?, TData?>`，不能为 `null`），用于投影响应中的 `data` 节点。

**返回。** 非泛型重载返回 `OneBot11Response`，原始 `JsonNode? Data` 不做投影；泛型重载返回 `OneBot11Response<TData>`，其中 `Data` 为解析结果，`RawData` 为未投影数据。

```csharp
var response = await bot.Actions.CallActionAsync(
    "implementation_extension",
    new JsonObject { ["key"] = "value" });
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

泛型重载会应用调用方提供的数据解析器：

```csharp
var parsedResponse = await bot.Actions.CallActionAsync<JsonNode>(
    OneBot11Actions.GetVersionInfo,
    node => node);
Console.WriteLine(parsedResponse.RawRequestJson);
Console.WriteLine(parsedResponse.RawResponseJson);
```

<a id="handlequickoperationasync"></a>

### `HandleQuickOperationAsync` — 对事件执行快速操作（隐藏 Action `.handle_quick_operation`）

**作用。** 对通过 HTTP POST 收到的事件调用官方隐藏 Action `.handle_quick_operation`。可用性和支持的操作字段取决于实现端。

**参数。** `context`（`JsonObject`，不能为 `null`）是通过 HTTP POST 收到的完整事件 JSON；`operation`（`JsonObject`，不能为 `null`）描述该事件支持的回复、撤回、踢人、禁言或其他快速操作。

**返回。** `OneBot11Response`，没有标准强类型数据载荷。

```csharp
var eventContext = new JsonObject
{
    ["post_type"] = "message",
    ["message_type"] = "group",
    ["group_id"] = groupId,
    ["user_id"] = userId,
    ["message_id"] = messageId
}; // 请替换为完整的 HTTP POST 事件对象。
var operation = new JsonObject { ["reply"] = "收到" };

var response = await bot.Actions.HandleQuickOperationAsync(eventContext, operation);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

## 消息链

新代码使用 `OneBot11SendMessage` 构建有序消息链。下面组合文本和图片并发送到群：

```csharp
var message = new OneBot11SendMessage()
    .Text("图片如下：")
    .Image("https://example.com/cat.png");

var response = await bot.Actions.SendGroupMessageAsync(
    groupId: 123456789,
    message: message);
```

接收消息使用 `OneBot11ReceivedMessage`，通过 `MessageChain.OfType<T>()` 读取具体段。未知段解析为 `UnknownReceivedSegment` 并保留原始 JSON。

旧的 `OneBot11MessageChain`/`OneBot11Message` 仍为兼容代码保留；新项目优先使用上面的发送与接收模型。

## 支持的消息段

以下代码均创建一个可加入 `OneBot11SendMessage` 的发送段；标为“仅接收”的类型除外。

### `text` 文本

```csharp
var segment = new TextSendSegment("你好");
```

`text` 是文本内容，可以为空字符串，不能为 `null`。

### `face` QQ 表情

```csharp
var segment = new FaceSendSegment(14L);
```

`id` 是协议表情 ID，可传 `long` 或字符串。

### `image` 图片

```csharp
var segment = new ImageSendSegment(
    file: "https://example.com/cat.png",
    flash: false,
    cache: true,
    proxy: true,
    timeoutSeconds: 30);
```

`file` 可为已接收文件名、文件 URI、网络 URL 或 `base64://` URI；`flash` 表示闪照；其余参数控制缓存、代理和下载超时。

### `record` 语音

```csharp
var segment = new RecordSendSegment(
    file: "file:///D:/audio/test.amr",
    magic: false,
    cache: true,
    proxy: true,
    timeoutSeconds: 30);
```

`file` 是语音来源；`magic` 是否变声；其余参数与图片相同。

### `video` 短视频

```csharp
var segment = new VideoSendSegment(
    file: "https://example.com/video.mp4",
    cache: true,
    proxy: true,
    timeoutSeconds: 30);
```

`file` 是视频来源，其他参数控制下载。

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

### `poke` 戳一戳

```csharp
var segment = new PokeSendSegment(pokeType: "1", id: "2");
```

`pokeType` 和 `id` 是实现端定义的戳一戳类型与 ID。

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
var friend = new ContactSendSegment(OneBot11ContactTarget.Friend, "123456789");
var group = new ContactSendSegment(OneBot11ContactTarget.Group, "987654321");
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
var providerMusic = new MusicSendSegment(OneBot11MusicProvider.QQ, "歌曲ID");
var customMusic = new CustomMusicSendSegment(
    url: "https://example.com/song",
    audio: "https://example.com/song.mp3",
    title: "歌曲名",
    content: "可选简介",
    image: "https://example.com/cover.png");
```

平台音乐的 `provider` 可选 `QQ`、`NetEase`、`Xiami`，`id` 是歌曲 ID；自定义音乐需要页面 URL、音频 URL 和标题。

### `reply` 回复

```csharp
var segment = new ReplySendSegment(messageId: 123L);
```

`messageId` 是要引用的消息 ID，可传 `long` 或字符串。

### `forward` 合并转发引用（仅接收）

```csharp
foreach (var forward in messageEvent.MessageChain.OfType<ForwardReceivedSegment>())
    await bot.Actions.GetForwardMessageAsync(forward.ForwardId!);
```

`ForwardId` 可传给 `GetForwardMessageAsync` 获取合并转发内容。

### `node` 合并转发节点

```csharp
var existing = new ForwardNodeSendSegment(messageId: 123L);
var custom = new CustomForwardNodeSendSegment(
    userId: "123456789",
    nickname: "发送者",
    content: new OneBot11SendMessage().Text("节点内容"));
```

已有节点只需消息 ID；自定义节点需要显示用户 ID、昵称和嵌套消息。

### `xml` XML 富消息

```csharp
var segment = new XmlSendSegment("<msg>...</msg>");
```

参数 `xml` 是完整 XML 字符串。

### `json` JSON 富消息

```csharp
var segment = new JsonSendSegment("{\"app\":\"example\"}");
```

参数 `json` 是实现端要求的 JSON 字符串。

### 实现端扩展

```csharp
var segment = new CustomSendSegment(
    "markdown",
    new JsonObject { ["content"] = "**你好**" });
```

`type` 是扩展段名称，`data` 是实现端约定的参数。

## 控制台调试工程

这些可运行工程只用于调试。API 使用方式以本文上面的 Action 方法和接收事件章节为准，不需要先阅读控制台工程。其配置可能包含真实凭据和目标 ID，运行任何会改变状态的操作前请先逐项检查。

- [Observable 示例](../samples/OneBotSdk.Net.ObservableExample)
- [EventHandler 示例](../samples/OneBotSdk.Net.EventHandlerExample)
- [HTTP Action 示例](../samples/OneBotSdk.Net.HttpActionExample)

不要把 Token 写入源码或日志；生产环境优先使用 HTTPS/WSS。
