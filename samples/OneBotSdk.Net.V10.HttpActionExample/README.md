# OneBotSdk.Net.V10.HttpActionExample

> **警告 / Warning**：当前 `http://127.0.0.1:3000/` 服务器实现的是 OneBot 11。V10 与 V11 虽有相似接口，但不应把它们视为同一协议；在地址改为符合规范的 OneBot 10 服务器前请勿运行本示例。

该示例只创建 HTTP Action 客户端，不建立事件连接。`Program.Main` 中直接列出 OneBot 10 全部 37 个公开动作，并通过公共 `WriteResponse` 输出：

- `Action`
- `RequestParameters`
- `RequestEcho`
- `RawRequestJson`
- `Data`
- `RawData`（强类型响应；非泛型响应的 `Data` 本身就是原始节点）
- `RawResponseJson`

配置使用 `http://127.0.0.1:3000/` 和 Token `123456`。所有群消息只发送到用户指定的群 `782351597`。

`OneBot10Json.UseUnsafeRelaxedJsonEscaping` 默认为 `false`；示例显式设为 `true`。

> **危险 / DANGER**：撤回、踢人、群管理、退群/解散群、退出讨论组、处理请求、重启、清理数据目录和日志等高风险代码块均已整段注释，默认绝不执行。普通发消息与点赞调用仍是活跃示例，运行时会产生可见的远端状态；群消息只指向用户授权的 `782351597`。

在没有符合规范的 OneBot 10 服务器时只编译，不运行：

```powershell
dotnet build samples/OneBotSdk.Net.V10.HttpActionExample/OneBotSdk.Net.V10.HttpActionExample.csproj -c Release -f net10.0
```
