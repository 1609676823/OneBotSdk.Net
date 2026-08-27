# OneBotSdk.Net.V10.ObservableExample

> **警告 / Warning**：当前附件中的 NapCat 服务器配置是 OneBot 11，不是 OneBot 10。请不要用它直接运行本示例；只有准备好符合规范的 OneBot 10 实现端后才能连接。

该示例使用 `IObservable` 和 `OfType` 监听 OneBot 10 私聊、群、讨论组、通知、请求及元事件，并为每个事件输出 `RawJson`。示例只接收事件，不发送消息。

默认配置：

- Action：`http://127.0.0.1:3000/`
- Event：`ws://127.0.0.1:3001/event`
- Token：`123456`

程序会持续监听，按 Enter 键即可退出并释放机器人客户端。

`OneBot10Json.UseUnsafeRelaxedJsonEscaping` 默认为 `false`；示例显式设为 `true`，用于展示如何全局启用限制更少的 JSON 转义。只要尚未替换成真实 OneBot 10 实现端，就应只编译而不运行：

```powershell
dotnet build samples/OneBotSdk.Net.V10.ObservableExample/OneBotSdk.Net.V10.ObservableExample.csproj -c Release -f net10.0
```
