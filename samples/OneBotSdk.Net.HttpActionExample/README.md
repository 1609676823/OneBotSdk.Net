# OneBotSdk.Net.HttpActionExample

该工程是一个纯 HTTP 控制台示例。它只创建 `OneBot11HttpActionTransport` 和 `OneBot11Client`，不创建 `OneBot11Bot`、不连接事件终结点，也不接收任何事件。

示例代码全部集中在 [`Program.cs`](Program.cs) 中，不再使用测试运行器、结果模型或报告生成器。`OneBot11Client` 的 45 个公开方法签名均出现在 `Main` 方法内，并按调用顺序阅读。

## 运行配置

- HTTP 地址：`http://127.0.0.1:3000/`
- Access Token：`123456`
- 测试群：`782351597`

```powershell
dotnet run --project samples/OneBotSdk.Net.HttpActionExample/OneBotSdk.Net.HttpActionExample.csproj -f net8.0
```

SDK 的 `OneBot11Json.UseUnsafeRelaxedJsonEscaping` 默认值为 `false`；本示例显式设置为 `true`。因此 `RawRequestJson` 会保留该全局设置下传输层实际发送的 JSON 文本。

## 输出内容

每个调用都会先通过 `Console.WriteLine` 输出方法名和输入变量，再输出返回对象中的：

- `Action`
- `RequestParameters`
- `RequestEcho`
- `RawRequestJson`
- `Status`
- `RetCode`
- `IsSuccess`
- 强类型 `Data` 与 `RawData`
- `RawResponseJson`

请求参数不再手工复制。控制台打印的是返回对象记录的实际请求参数和实际 HTTP 请求、响应报文。

示例会直接调用消息发送、点赞、可恢复的群管理、请求处理、缓存清理和实现端重启等功能。群相关调用统一使用 `782351597`；群名称、群名片、专属头衔、全员禁言等可恢复设置会在调用后继续演示恢复请求。媒体文件、合并转发 ID、匿名 flag 和请求 flag 需要由真实事件提供，当前示例使用清晰可见的测试变量，并照常打印实现端返回的成功或失败报文。

`SetGroupKickAsync` 与 `SetGroupLeaveAsync` 只保留带有双语警告的注释代码，不会自动执行。尤其是群主调用 `SetGroupLeaveAsync` 时，部分实现端即使收到 `isDismiss=false` 也可能退出或解散群，这类成员关系操作无法由示例自动恢复。

最近一次本地实机调用记录位于 [`HTTP_ACTION_TEST_REPORT.md`](HTTP_ACTION_TEST_REPORT.md)。该历史记录是在上述两个调用被注释前生成的，并且用户确认测试群随后被解散。该文件按调试要求不脱敏，可能包含 Token、Cookie、账号、群和成员信息，请勿公开发布。

该工程支持 `net10.0` 到 `net5.0`，以及 `net481`、`net48`、`net472`、`net471`、`net47` 和 `net462`。部分目标已结束官方支持，仅用于验证兼容范围。
