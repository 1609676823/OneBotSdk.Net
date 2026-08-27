# OneBotSdk.Net.ObservableExample

这个控制台项目只展示 `IObservable<T>` 事件订阅方式。它使用 `OfType<TEvent>()` 获取具体事件对象，并使用消息链的 `OfType<TSegment>()` 读取强类型消息段；不会混用 `EventHandler`，也不会发送任何消息。

完整代码位于 [Program.cs](Program.cs)，使用传统 `Program` 类和 `Main` 方法，没有使用顶级语句。

## 示例内容

- `bot.EventReceived.Subscribe(...)`：为每个已知或未知事件输出一次完整 `RawJson`。
- `bot.MessageReceived.OfType<PrivateMessageEvent>()`：接收私聊消息。
- `bot.MessageReceived.OfType<GroupMessageEvent>()`：接收群消息并解析文本、图片消息段。
- `bot.RequestReceived.OfType<TEvent>()`：分别接收好友请求和群请求。
- `bot.NoticeReceived.Subscribe(...)`：接收全部通知事件。
- `bot.MetaEventReceived.Subscribe(...)`：接收全部元事件。

如果只关心一种通知或元事件，可以继续使用相同的 `OfType<TEvent>()` 风格：

```csharp
bot.NoticeReceived
    .OfType<GroupBanNoticeEvent>()
    .Subscribe(notice => Console.WriteLine(notice.Duration));
```

事件订阅位于 `StartAsync()` 之前，避免遗漏连接后立即到达的生命周期事件。所有订阅都是不重放的热流，生产代码在监听生命周期短于客户端时应保存并释放 `Subscribe` 返回的 `IDisposable`。

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
dotnet run --project samples\OneBotSdk.Net.ObservableExample\OneBotSdk.Net.ObservableExample.csproj -f net8.0
```

项目支持 `net5.0` 至 `net10.0`，以及 `net462`、`net47`、`net471`、`net472`、`net48`、`net481`。
