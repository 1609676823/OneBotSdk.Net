# OneBotSdk.Net

[English](README.md) | [简体中文](README.zh-CN.md)

一个面向 OneBot 10、OneBot 11 和 OneBot 12 候选规范的强类型、多目标框架 .NET SDK。

同一个程序集提供三套协议专用 API：

- `OneBotSdk.Net.V10.*`
- `OneBotSdk.Net.V11.*`
- `OneBotSdk.Net.V12.*`

线协议模型不会跨版本混用。每个版本分别拥有自己的 Action、事件层次、消息段、响应信封、传输和 JSON 解析器。

> OneBotSdk.Net 是独立的社区 SDK，并非 OneBot 官方项目。升级版本前请检查对应版本说明。

## 使用指南（建议先看）

> **完整入口：[打开文档目录](docs/README.md)**。每个版本都可以直接进入 Action 方法或接收事件，不需要先阅读控制台调试工程。

| 协议 | 使用指南 | Action 方法 | 接收事件 | 官方协议 |
| --- | --- | --- | --- | --- |
| OneBot 10 | [中文指南](docs/onebot-10.zh-CN.md) | [逐方法说明](docs/onebot-10.zh-CN.md#action-方法) | [逐事件说明](docs/onebot-10.zh-CN.md#接收事件) | [官方规范仓库](https://github.com/botuniverse/onebot-10) |
| OneBot 11 | [中文指南](docs/onebot-11.zh-CN.md) | [逐方法说明](docs/onebot-11.zh-CN.md#action-方法) | [逐事件说明](docs/onebot-11.zh-CN.md#接收事件) | [官方规范仓库](https://github.com/botuniverse/onebot-11) · [公开 API](https://github.com/botuniverse/onebot-11/blob/master/api/public.md) |
| OneBot 12 | [中文指南](docs/onebot-12.zh-CN.md) | [逐方法说明](docs/onebot-12.zh-CN.md#action-方法) | [逐事件说明](docs/onebot-12.zh-CN.md#接收事件) | [候选规范仓库](https://github.com/botuniverse/onebot) · [官方文档](https://12.onebot.dev/) |

英文指南：[OneBot 10](docs/onebot-10.md) · [OneBot 11](docs/onebot-11.md) · [OneBot 12](docs/onebot-12.md)。命名空间和线协议隔离规则见[协议版本架构](docs/PROTOCOL_VERSIONS.md)。

## 通过 NuGet 安装

NuGet 包地址：[OneBotSdk.Net](https://www.nuget.org/packages/OneBotSdk.Net/)

使用 .NET CLI：

```powershell
dotnet add package OneBotSdk.Net
```

或使用 Visual Studio 程序包管理器控制台：

```powershell
Install-Package OneBotSdk.Net
```

完整文档、示例和源码可从任一仓库镜像查看：

- GitHub：[项目主页](https://github.com/1609676823/OneBotSdk.Net) · [文档](docs/README.md) · [示例](samples/)
- Gitee：[项目主页](https://gitee.com/lnsyzjw/one-bot-sdk.-net) · [文档](docs/README.md) · [示例](samples/)

## 主要特性

- 强类型 Action、响应、事件和消息段。
- 发送与接收消息使用独立模型，明确区分不同方向允许的字段。
- 同时提供标准 `EventHandler<TEventArgs>` 和无第三方依赖的 `IObservable<T>`、`OfType<T>()`。
- 入站数据按字段容错解析：单个异常字段不会导致整个响应或事件丢失。
- 通过 `RawJson`、`RawData`、`RawRequestJson` 和 `RawResponseJson` 保留完整诊断数据。
- Action 和 Event 分别配置地址、端口、Token 与传输选项。
- 根据所选协议提供 HTTP、HTTP Webhook、正向 WebSocket 和反向 WebSocket。
- 提供 `Start()`、`StartAsync()` 组合启动入口，同时保留底层连接方法。
- 只使用 `System.Text.Json`，不使用 Newtonsoft.Json。
- 使用 xUnit 覆盖现代 .NET 和 .NET Framework 多个目标框架。
- 使用 MIT 许可证。

## 已实现协议范围

| 协议 | 命名空间根 | 已实现的标准范围 | 指南 |
| --- | --- | --- | --- |
| OneBot 10 | `OneBotSdk.Net.V10` | 37 个公开 Action、13 类具体事件、14 种标准消息段 wire type | [OneBot 10 中文指南](docs/onebot-10.zh-CN.md) |
| OneBot 11 | `OneBotSdk.Net.V11` | 38 个公开 Action、17 类具体事件、强类型 QQ 消息段 | [OneBot 11 中文指南](docs/onebot-11.zh-CN.md) |
| OneBot 12 候选规范 | `OneBotSdk.Net.V12` | 31 个 Action、19 类具体事件、10 种标准消息段 | [OneBot 12 中文指南](docs/onebot-12.zh-CN.md) |

完整使用说明和 API 直达入口位于本文最前面的[使用指南](#使用指南建议先看)。

## 支持的目标框架

类库目标框架：

```text
net10.0; net9.0; net8.0; net7.0; net6.0; net5.0;
net481; net48; net472; net471; net47; net462;
netstandard2.1; netstandard2.0
```

示例与测试面向除 .NET Standard 外相同的可执行框架。新项目建议优先选择仍在微软支持周期内的 .NET 版本；较旧目标主要用于兼容已有部署。

## 仅接收快速开始

下面是一个完整的 OneBot 11 程序，它会打印每个事件的保留报文并同时展示两种订阅模式，但不会发送聊天消息。

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Json;

namespace OneBotSdk.Net.QuickStart
{
    internal static class Program
    {
        private static async Task Main()
        {
            var options = new OneBot11BotOptions(
                new OneBot11ActionEndpointOptions(
                    "127.0.0.1",
                    3000,
                    "请替换为ActionToken"),
                new OneBot11EventEndpointOptions(
                    "127.0.0.1",
                    3001,
                    "请替换为EventToken"));

            using (var bot = new OneBot11Bot(options))
            using (var rawEvents = bot.EventReceived.Subscribe(oneBotEvent =>
                Console.WriteLine(OneBot11Json.Serialize(oneBotEvent.RawJson))))
            {
                bot.Events.GroupMessageReceived += (_, eventArgs) =>
                    Console.WriteLine(eventArgs.Event.MessageChain.PlainText);

                var login = await bot.StartAsync();
                Console.WriteLine(
                    "已连接：{0} ({1})",
                    login.Data?.Nickname,
                    login.Data?.UserId);

                Console.WriteLine("正在监听；按 Enter 键退出。");
                Console.ReadLine();
            }
        }
    }
}
```

请在调用 `StartAsync()` 前完成订阅，避免遗漏第一个生命周期事件。`StartAsync()` 会执行对应协议的只读启动检查，然后连接 Event 终结点。

这里的 `OneBot11BotOptions` 固定组合 HTTP（按方向可理解为正向 HTTP）Action 与正向 WebSocket Event。两个构造参数的地址格式和用途见 [OneBot 11 的两个端点参数](docs/onebot-11.zh-CN.md#两个端点参数)。

控制台示例不需要 `ManualResetEvent`。它持有等待句柄，使用时应释放，但可以用 `using var` 避免嵌套；这里直接等待 Enter，退出时会正常释放订阅和机器人客户端。服务程序应改用宿主的取消令牌。

Action 和 Event Token 有意归属于不同终结点对象。二者可以具有相同值，但 SDK 不会隐式共享，也不会在两个终结点之间回退。

## 事件订阅

可以任选一种模式，也可以在同一个应用中同时使用。

```csharp
// 标准 .NET 事件模式。
bot.Events.GroupMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawMessage);

// 无第三方依赖的 Observable 模式。
using var subscription = bot.MessageReceived
    .OfType<GroupMessageEvent>()
    .Subscribe(message => Console.WriteLine(message.RawMessage));
```

Observable 是不重放历史事件的热流。请保存并释放 `Subscribe` 返回的 `IDisposable`。
SDK 会报告 WebSocket 关闭与故障，但不会强制采用某一种自动重连策略；生产应用应自行负责重试、退避和停止流程。

## 消息模型

发送和接收分别使用不同的对象体系：

- `OneBot10SendMessage` / `OneBot10ReceivedMessage`
- `OneBot11SendMessage` / `OneBot11ReceivedMessage`
- `OneBot12SendMessage` / `OneBot12ReceivedMessage`

每一种具体消息段都拆分为独立源码文件。可以直接通过 LINQ 风格的 `OfType<TSegment>()` 从消息链筛选文本、图片、提及、回复、未知段等接收类型。

## 原始报文与容错解析

Action 响应同时保留便于业务使用的强类型值与完整诊断层：

```csharp
var response = await bot.Actions.GetStatusAsync();

Console.WriteLine(response.Data?.Online);
Console.WriteLine(response.RawData);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

- `Data` 是经过逐字段容错解析的强类型结果。
- `RawData` 是未进行强类型投影的 `data` 节点独立副本。
- `RawRequestJson` 和 `RawResponseJson` 是传输层报文。
- 事件和消息对象通过 `RawJson` 保留实现端扩展字段。

这些值可能不同。不要通过重新序列化 `Data` 替代 `RawData`，因为强类型投影可能规范化字段、跳过异常集合元素或省略实现端扩展字段。

## JSON 配置

每个协议版本都有独立的全局 JSON 设置：

```csharp
OneBot10Json.UseUnsafeRelaxedJsonEscaping = true;
OneBot11Json.UseUnsafeRelaxedJsonEscaping = true;
OneBot12Json.UseUnsafeRelaxedJsonEscaping = true;
```

默认值是 `false`，即使用 `JavaScriptEncoder.Default`。只有在接收方明确把输出作为 JSON 处理时才应启用 `UnsafeRelaxedJsonEscaping`，绝不能把其输出直接嵌入 HTML 或脚本上下文。

## 控制台调试工程

| 协议 | Observable 仅接收 | EventHandler 仅接收 | HTTP Action |
| --- | --- | --- | --- |
| OneBot 10 | [Observable](samples/OneBotSdk.Net.V10.ObservableExample) | [EventHandler](samples/OneBotSdk.Net.V10.EventHandlerExample) | [HTTP Action](samples/OneBotSdk.Net.V10.HttpActionExample) |
| OneBot 11 | [Observable](samples/OneBotSdk.Net.ObservableExample) | [EventHandler](samples/OneBotSdk.Net.EventHandlerExample) | [HTTP Action](samples/OneBotSdk.Net.HttpActionExample) |
| OneBot 12 | [Observable](samples/OneBotSdk.Net.V12.ObservableExample) | [EventHandler](samples/OneBotSdk.Net.V12.EventHandlerExample) | [HTTP Action](samples/OneBotSdk.Net.V12.HttpActionExample) |

这些工程用于连接实现端并进行交互调试，不是 API 的主要说明入口。每个 Action 的参数与独立调用片段、每种接收事件的处理片段都直接写在上面的协议文档中。仅接收调试工程会打印完整 `RawJson`，并且不会发送消息；HTTP Action 调试工程可能修改远端状态，运行前必须仔细检查源码。

## 安全与破坏性操作

- 不要提交仍然有效的 Access Token；公开分支前应替换或轮换本地测试凭据。
- 踢人、退群、解散群、删除、重启、清理缓存、上传、重命名和请求处理均属于会修改远端状态的操作。
- V10，尤其是 V11 的 HTTP 示例，默认会执行真实的状态修改调用，包括消息/点赞，以及 V11 中的删除、管理、请求处理、清缓存和实现端重启。部分最高风险的退群/踢人代码虽已注释，但完整程序仍不安全，绝不能不经修改就使用生产账号运行。
- OneBot 10、11、12 是不同的线协议。即使主机和端口形式相似，也不能直接把 V10 或 V12 客户端连接到 OneBot 11 服务。

## 构建与测试

```powershell
dotnet restore OneBotSdk.Net.sln
dotnet build OneBotSdk.Net.sln -c Release --no-restore
dotnet test OneBotSdk.Net.Tests\OneBotSdk.Net.Tests.csproj -c Release -f net10.0 --no-restore
dotnet pack OneBotSdk.Net\OneBotSdk.Net.csproj -c Release --no-restore
```

以上命令运行可移植的 net10.0 测试目标。完整的 12 个可执行目标测试矩阵包含 .NET Framework，因此需要 Windows 和兼容 CLR。测试覆盖 Action 目录、消息与事件解析、请求/响应追踪、终结点隔离、启动顺序、HTTP/Webhook、WebSocket 会话和跨版本 API 边界。

## 参与贡献

欢迎提交 Issue 和 Pull Request。请遵循以下约定：

1. 公开线协议类型必须位于正确的版本命名空间。
2. 保持逐字段容错解析和原始 JSON 快照。
3. 协议变更必须新增或更新 xUnit 测试。
4. 关键代码注释使用英文在前、中文在后。
5. 提交前运行格式化、编译和测试。

## 许可证

OneBotSdk.Net 使用 [MIT License](LICENSE) 开源。

OneBot 协议规范及文档中引用的实现端是独立项目，分别遵循其自身的许可证与政策。

## 协议参考

- [OneBot 10 规范](https://github.com/botuniverse/onebot-10)
- [OneBot 11 规范](https://github.com/botuniverse/onebot-11)
- [OneBot 12 候选规范](https://github.com/botuniverse/onebot)
- [OneBot 12 文档](https://12.onebot.dev/)
