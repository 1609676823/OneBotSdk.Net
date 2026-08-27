# OneBotSdk.Net.V12.EventHandlerExample

> 警告：当前附件中的 NapCat 服务器配置是 OneBot 11，不是 OneBot 12。请不要用它直接运行本示例；只有准备好真正的 OneBot 12 实现端后才能连接。

本示例依据官方仍标记为候选规范的 OneBot 12 文档实现。

这个控制台项目只展示标准 .NET `EventHandler<TEventArgs>` 事件订阅方式。所有入站事件都通过 `bot.Events` 订阅，不使用 Observable 事件流，也不包含任何发送聊天消息的代码。`Start()` 只执行只读的 `get_version` 和 `get_status` 检查，然后连接事件 WebSocket。

完整代码位于 [Program.cs](Program.cs)，使用传统 `Program` 类和同步 `Main` 方法，没有使用顶级语句。

## 示例内容

- `bot.Events.EventDispatched` 为每个已知或未知事件输出完整 `RawJson`。
- 3 个强类型消息处理器、13 个强类型通知处理器和 3 个强类型元事件处理器逐一展示，共覆盖 19 种 OneBot 12 标准具体事件。
- `messageEvent.Message.OfType<OneBot12TextReceivedSegment>()` 仅对强类型消息链执行 LINQ 筛选；事件交付本身仍完全使用 `EventHandler`。
- 程序持续监听，按 Enter 键即可退出并释放机器人客户端。

## 配置

代码按需求保留了未脱敏的地址、端口、Token 和机器人身份，但这些数值来自 OneBot 11 环境，仅用于展示 OneBot 12 的独立终结点配置结构：

```csharp
var options = new OneBot12BotOptions(
    new OneBot12ActionEndpointOptions("127.0.0.1", 3000, "123456"),
    new OneBot12EventEndpointOptions("127.0.0.1", 3001, "123456"),
    new OneBot12Self("qq", "123xxxxxxx"));
```

Action 和 Event 的地址、端口和 Token 分别属于各自的终结点对象，不共享也不交叉回退。示例显式设置 `OneBot12Json.UseUnsafeRelaxedJsonEscaping = true`；SDK 默认值是 `false`。

## 构建与运行

可以在不连接服务器的情况下只构建本项目：

```powershell
dotnet build samples\OneBotSdk.Net.V12.EventHandlerExample\OneBotSdk.Net.V12.EventHandlerExample.csproj -f net10.0
```

只有在地址、端口、Token 和 `self` 都替换为真实 OneBot 12 实现端的配置后，才可以运行。项目支持 `net5.0` 至 `net10.0`，以及 `net462`、`net47`、`net471`、`net472`、`net48`、`net481`。
