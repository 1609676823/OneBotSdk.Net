# OneBotSdk.Net.V12.HttpActionExample

> **Important:** The server currently configured at `http://127.0.0.1:3000/` implements OneBot 11. OneBot 12 uses a different HTTP request envelope, so this example cannot run directly against the current server.
>
> **重要：** 当前 `http://127.0.0.1:3000/` 服务器实现的是 OneBot 11。OneBot 12 使用不同的 HTTP 请求信封，因此本示例不能直接在当前服务器上兼容运行。

本示例依据官方仍标记为候选规范的 OneBot 12 文档实现。

该工程是纯 HTTP 动作控制台示例，只创建 `OneBot12HttpActionTransport` 和 `OneBot12Client`，不会创建事件连接。全部代码集中在 [`Program.cs`](Program.cs)，并按官方文档顺序展示 31 个标准 Action；分片上传和分片下载动作还分别展示了每个阶段。

## 配置

- HTTP 地址：`http://127.0.0.1:3000/`
- Access Token：`123456`
- Self：`platform=qq`、`user_id=123xxxxxxx`
- 测试群：`782351597`

这些值按照当前本地配置原样保留，没有脱敏。`guild_id`、`channel_id`、`message_id` 和 `file_id` 使用清晰可见的占位值，需要在连接真正的 OneBot 12 实现后自行替换。

## 输出内容

每个直接调用都会先输出方法名和输入变量，然后统一输出返回对象记录的：

- `Action`
- `RequestParameters`
- `RequestEcho`
- `RequestSelf`
- `RawRequestJson`
- `Data`
- `RawData`（仅强类型响应具有独立属性）
- `RawResponseJson`
- `Status`、`RetCode` 和 `IsSuccess`

强类型响应中的 `Data` 是容错解析后的对象，`RawData` 是服务器原始 `data` 节点。非泛型响应的 `Data` 本身就是原始 JSON，所以没有重复的 `RawData` 属性；示例会在该输出行明确标注“不适用”，而不会把同一数值伪装成两份数据。

`OneBot12Json.UseUnsafeRelaxedJsonEscaping` 默认是 `false`；本示例显式设置为 `true`，用于展示如何全局启用 `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`。

## 安全说明

以下会产生副作用的完整调用代码仅作为注释保留，默认不会执行：

- `send_message`、`delete_message`
- `set_group_name`、`set_guild_name`、`set_channel_name`
- `upload_file` 及 `upload_file_fragmented` 的全部阶段
- `leave_group`、`leave_guild`、`leave_channel`

其中三个 `leave_*` 动作会改变成员关系并且可能无法撤销。机器人是所有者时，平台或实现端还可能产生解散群等更严重结果，绝不能在自动化示例中执行。

只有在地址已经替换为真正的 OneBot 12 HTTP 根终结点、身份和占位标识均确认无误后，才应手动取消所需调用的注释。

## 编译

```powershell
dotnet build samples/OneBotSdk.Net.V12.HttpActionExample/OneBotSdk.Net.V12.HttpActionExample.csproj -c Release -f net10.0
```

项目还声明了 `net9.0` 至 `net5.0`，以及 `net481`、`net48`、`net472`、`net471`、`net47` 和 `net462`，用于与 SDK 的控制台兼容范围保持一致。
