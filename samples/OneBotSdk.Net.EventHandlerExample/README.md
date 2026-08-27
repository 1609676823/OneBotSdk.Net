# OneBotSdk.Net.EventHandlerExample

这个控制台项目只展示标准 .NET `EventHandler<TEventArgs>` 事件订阅方式。所有 OneBot 入站事件都通过 `bot.Events` 订阅；不会使用 Observable 事件流，也不会发送任何消息。

完整代码位于 [Program.cs](Program.cs)，使用传统 `Program` 类和 `Main` 方法，没有使用顶级语句。

## 示例内容

- `bot.Events.EventDispatched`：为每个已知或未知事件输出一次完整 `RawJson`。
- `bot.Events.PrivateMessageReceived`：接收私聊消息。
- `bot.Events.GroupMessageReceived`：接收群消息并解析文本、图片消息段。
- `bot.Events.FriendRequestReceived`：接收好友请求。
- `bot.Events.GroupRequestReceived`：接收群请求。
- `bot.Events.NoticeDispatched`：接收全部通知事件。
- `bot.Events.MetaEventDispatched`：接收全部元事件。

如果只关心一种通知，可以订阅对应的强类型处理器，而不再同时订阅分类处理器：

```csharp
bot.Events.GroupBanNoticeReceived += (_, eventArgs) =>
{
    Console.WriteLine(eventArgs.Event.Duration);
};
```

`MessageChain.OfType<TextReceivedSegment>()` 是对消息链执行的 LINQ 类型筛选，不属于 Observable 事件订阅。事件本身仍完全通过 `EventHandler` 分发。

## 配置

示例使用以 `D:\NapCat.Framework\config\onebot11_123xxxxxxx.json` 为参考的脱敏配置值：

```csharp
var options = new OneBot11BotOptions(
    new OneBot11ActionEndpointOptions("127.0.0.1", 3000, "123456"),
    new OneBot11EventEndpointOptions("127.0.0.1", 3001, "123456"));
```

Action 和 Event 的地址、端口、Token 分别属于各自的终结点对象。示例显式设置 `OneBot11Json.UseUnsafeRelaxedJsonEscaping = true`；SDK 默认值为 `false`。

## 运行

```powershell
dotnet run --project samples\OneBotSdk.Net.EventHandlerExample\OneBotSdk.Net.EventHandlerExample.csproj -f net8.0
```

项目支持 `net5.0` 至 `net10.0`，以及 `net462`、`net47`、`net471`、`net472`、`net48`、`net481`。
