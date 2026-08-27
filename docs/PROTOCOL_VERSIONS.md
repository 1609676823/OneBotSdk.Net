# OneBot protocol version architecture / OneBot 协议版本架构

OneBotSdk.Net is one multi-target SDK package and assembly. Protocol wire contracts are isolated by
versioned public namespaces instead of a runtime version switch:

OneBotSdk.Net 是一个多目标框架 SDK 包和程序集。各协议的线协议契约通过版本化公开命名空间隔离，
而不是在运行时通过版本开关分支：

| Protocol / 协议 | Public namespace root / 公开命名空间根 | Main client / 主客户端 |
| --- | --- | --- |
| OneBot 10 | OneBotSdk.Net.V10 | OneBot10Client, OneBot10Bot |
| OneBot 11 | OneBotSdk.Net.V11 | OneBot11Client, OneBot11Bot |
| OneBot 12 candidate / 候选规范 | OneBotSdk.Net.V12 | OneBot12Client, OneBot12Bot |

Each version owns its action names, request and response envelopes, strongly typed response models,
event hierarchy, message segments, parser, dispatcher, endpoint options, and transports. Public DTOs
never inherit from or expose another protocol version's DTOs.

每个版本独立拥有 Action 名称、请求与响应信封、强类型响应模型、事件层次、消息段、解析器、
分发器、终结点选项和传输层。公开 DTO 不会继承或暴露其它协议版本的 DTO。

## Namespace layout / 命名空间布局

~~~text
OneBotSdk.Net.V10|V11|V12
├── Client
├── Events
├── Json
├── Messages
├── Responses
└── Transports
    ├── Http
    └── WebSockets
~~~

Protocol roots, clients, envelopes, identities, and other cross-cutting entry types carry explicit
version prefixes such as OneBot10Response<TData>, OneBot11ReceivedMessage, or OneBot12Self.
Concrete event and message-segment names stay concise inside their version-specific namespaces;
for example, V10 and V12 may both contain PrivateMessageEvent without becoming assignable types.

协议根、客户端、信封、身份及其它跨领域入口类型带有明确版本前缀，例如
OneBot10Response<TData>、OneBot11ReceivedMessage 或 OneBot12Self。具体事件与消息段在各自的
版本命名空间内保持简洁命名；例如 V10 与 V12 都可以拥有 PrivateMessageEvent，但它们不是可相互赋值的类型。

## Deliberately separate wire contracts / 有意隔离的线协议契约

- OneBot 10 and 11 use numeric QQ identifiers, CQ-compatible messages, and post_type events.
  OneBot 10 additionally contains discussion-group APIs and uses Token authentication for reverse
  WebSocket connections.
- OneBot 11 keeps its own expanded QQ action, event, and message-segment set.
- OneBot 12 uses string identifiers, self { platform, user_id }, floating-point event timestamps,
  type/detail_type/sub_type events, and a complete { action, params, echo, self } HTTP request
  envelope posted to /. CQ codes are not part of its receive contract.

- OneBot 10 和 11 使用数值型 QQ 标识、兼容 CQ 码的消息以及 post_type 事件。
  OneBot 10 还包含讨论组接口，并在反向 WebSocket 中使用 Token 鉴权。
- OneBot 11 独立保留其扩展后的 QQ Action、事件和消息段集合。
- OneBot 12 使用字符串标识、self { platform, user_id }、浮点事件时间戳、
  type/detail_type/sub_type 事件，并把完整 { action, params, echo, self } HTTP 请求信封
  POST 到 /。其接收契约不包含 CQ 码。

## Compatibility targets / 兼容目标

The library targets net10.0, net9.0, net8.0, net7.0, net6.0, net5.0, net481,
net48, net472, net471, net47, net462, netstandard2.1, and netstandard2.0.
Executable examples and xUnit tests target the same runnable frameworks, excluding .NET Standard.

类库面向 net10.0、net9.0、net8.0、net7.0、net6.0、net5.0、net481、
net48、net472、net471、net47、net462、netstandard2.1 和 netstandard2.0。
控制台示例与 xUnit 测试面向除 .NET Standard 外相同的可执行目标。
