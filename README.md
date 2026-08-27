# OneBotSdk.Net

[English](README.md) | [简体中文](README.zh-CN.md)

A strongly typed, multi-target .NET SDK for OneBot 10, OneBot 11, and the OneBot 12 candidate specification.

One assembly exposes three protocol-specific API surfaces:

- `OneBotSdk.Net.V10.*`
- `OneBotSdk.Net.V11.*`
- `OneBotSdk.Net.V12.*`

Wire models never cross protocol boundaries. Each version owns its actions, event hierarchy, message segments, response envelopes, transports, and JSON parser.

> OneBotSdk.Net is an independent community SDK and is not an official OneBot project. Review the release notes before upgrading.

## Usage guides (start here)

> **Complete entry point: [open the documentation index](docs/README.md).** Each version links directly to its Actions and received events; the console debugging projects are optional.

| Protocol | Usage guide | Actions | Receiving events | Official specification |
| --- | --- | --- | --- | --- |
| OneBot 10 | [English guide](docs/onebot-10.md) | [Per-method reference](docs/onebot-10.md#actions) | [Per-event reference](docs/onebot-10.md#receiving-events) | [Official specification repository](https://github.com/botuniverse/onebot-10) |
| OneBot 11 | [English guide](docs/onebot-11.md) | [Per-method reference](docs/onebot-11.md#actions) | [Per-event reference](docs/onebot-11.md#receiving-events) | [Official specification repository](https://github.com/botuniverse/onebot-11) · [Public API](https://github.com/botuniverse/onebot-11/blob/master/api/public.md) |
| OneBot 12 | [English guide](docs/onebot-12.md) | [Per-method reference](docs/onebot-12.md#actions) | [Per-event reference](docs/onebot-12.md#receiving-events) | [Candidate repository](https://github.com/botuniverse/onebot) · [Official documentation](https://12.onebot.dev/) |

Chinese guides: [OneBot 10](docs/onebot-10.zh-CN.md) · [OneBot 11](docs/onebot-11.zh-CN.md) · [OneBot 12](docs/onebot-12.zh-CN.md). See [protocol version architecture](docs/PROTOCOL_VERSIONS.md) for namespace and wire-contract isolation rules.

## Install from NuGet

NuGet package: [OneBotSdk.Net](https://www.nuget.org/packages/OneBotSdk.Net/)

Using the .NET CLI:

```powershell
dotnet add package OneBotSdk.Net
```

Or using the Visual Studio Package Manager Console:

```powershell
Install-Package OneBotSdk.Net
```

The complete documentation, examples, and source are available from either repository mirror:

- GitHub: [project home](https://github.com/1609676823/OneBotSdk.Net) · [documentation](docs/README.md) · [examples](samples/)
- Gitee: [project home](https://gitee.com/lnsyzjw/one-bot-sdk.-net) · [documentation](docs/README.md) · [examples](samples/)

## Highlights

- Strongly typed actions, responses, events, and message segments.
- Separate send and receive message models so direction-specific fields remain explicit.
- Both standard `EventHandler<TEventArgs>` events and dependency-free `IObservable<T>` streams with `OfType<T>()`.
- Field-tolerant inbound parsing: one malformed field does not discard the rest of a response or event.
- Complete diagnostic data through `RawJson`, `RawData`, `RawRequestJson`, and `RawResponseJson`.
- Independent Action and Event endpoints, each with its own address, port, token, and transport settings.
- HTTP, HTTP webhook, forward WebSocket, and reverse WebSocket support where defined by the selected protocol.
- `Start()` and `StartAsync()` composition helpers, while lower-level connect methods remain available.
- `System.Text.Json` only; Newtonsoft.Json is not used.
- Extensive xUnit coverage across modern .NET and .NET Framework targets.
- MIT licensed.

## Implemented protocol surface

| Protocol | Namespace root | Implemented standard surface | Guide |
| --- | --- | --- | --- |
| OneBot 10 | `OneBotSdk.Net.V10` | 37 public actions, 13 concrete events, 14 standard message-segment wire types | [OneBot 10 guide](docs/onebot-10.md) |
| OneBot 11 | `OneBotSdk.Net.V11` | 38 public actions, 17 concrete events, typed QQ message segments | [OneBot 11 guide](docs/onebot-11.md) |
| OneBot 12 candidate | `OneBotSdk.Net.V12` | 31 actions, 19 concrete events, 10 standard message segments | [OneBot 12 guide](docs/onebot-12.md) |

The complete usage and direct API links are in [Usage guides](#usage-guides-start-here) at the top of this document.

## Supported target frameworks

The library targets:

```text
net10.0; net9.0; net8.0; net7.0; net6.0; net5.0;
net481; net48; net472; net471; net47; net462;
netstandard2.1; netstandard2.0
```

The examples and tests target the same runnable frameworks, excluding .NET Standard. For new applications, prefer a .NET version that is still supported by Microsoft; the older targets exist for compatibility with existing deployments.

## Receive-only quick start

The following complete program uses OneBot 11, prints every retained event packet, and demonstrates both subscription models. It does not send chat messages.

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
                    "replace-with-action-token"),
                new OneBot11EventEndpointOptions(
                    "127.0.0.1",
                    3001,
                    "replace-with-event-token"));

            using (var bot = new OneBot11Bot(options))
            using (var rawEvents = bot.EventReceived.Subscribe(oneBotEvent =>
                Console.WriteLine(OneBot11Json.Serialize(oneBotEvent.RawJson))))
            {
                bot.Events.GroupMessageReceived += (_, eventArgs) =>
                    Console.WriteLine(eventArgs.Event.MessageChain.PlainText);

                var login = await bot.StartAsync();
                Console.WriteLine(
                    "Connected: {0} ({1})",
                    login.Data?.Nickname,
                    login.Data?.UserId);

                Console.WriteLine("Listening. Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
```

Register subscriptions before calling `StartAsync()` so the first lifecycle event cannot be missed. `StartAsync()` performs the protocol-specific read-only startup checks and then connects the Event endpoint.

Here, `OneBot11BotOptions` always combines HTTP (directionally, forward HTTP) Actions with forward WebSocket Events. See [the two OneBot 11 endpoint parameters](docs/onebot-11.md#the-two-endpoint-parameters) for each constructor argument's address format and purpose.

The console sample does not need `ManualResetEvent`. It owns a wait handle and should be disposed when used, though a `using var` declaration avoids nesting. Waiting for Enter is clearer here and lets the subscriptions and bot dispose normally; hosted services should use their host cancellation token.

The Action and Event tokens intentionally belong to different endpoint objects. They may have the same value, but the SDK never shares or falls back between them implicitly.

## Event subscriptions

Choose either pattern, or use both in the same application.

```csharp
// Standard .NET event pattern.
bot.Events.GroupMessageReceived += (_, args) =>
    Console.WriteLine(args.Event.RawMessage);

// Dependency-free observable pattern.
using var subscription = bot.MessageReceived
    .OfType<GroupMessageEvent>()
    .Subscribe(message => Console.WriteLine(message.RawMessage));
```

Observable streams are hot and do not replay past events. Keep and dispose the `IDisposable` returned by `Subscribe`.
The SDK reports WebSocket closure and faults but does not impose an automatic reconnect policy; production applications should own retry, backoff, and shutdown behavior.

## Message models

Sending and receiving use different object families:

- `OneBot10SendMessage` / `OneBot10ReceivedMessage`
- `OneBot11SendMessage` / `OneBot11ReceivedMessage`
- `OneBot12SendMessage` / `OneBot12ReceivedMessage`

Concrete segment classes are split into individual source files. LINQ-style `OfType<TSegment>()` can select text, image, mention, reply, unknown, and other received segment types directly from a message chain.

## Raw packets and tolerant parsing

Action responses retain both convenient typed values and the exact diagnostic layers:

```csharp
var response = await bot.Actions.GetStatusAsync();

Console.WriteLine(response.Data?.Online);
Console.WriteLine(response.RawData);
Console.WriteLine(response.RawRequestJson);
Console.WriteLine(response.RawResponseJson);
```

- `Data` is the field-tolerantly parsed, strongly typed result.
- `RawData` is an independent copy of the unprojected `data` node.
- `RawRequestJson` and `RawResponseJson` are the transport payloads.
- Event and message objects expose `RawJson`, including unknown implementation extensions.

These values can legitimately differ. Do not replace `RawData` by serializing `Data`, because typed projection may normalize values, skip malformed collection elements, or omit implementation-specific fields.

## JSON configuration

Each protocol version has an independent global JSON setting:

```csharp
OneBot10Json.UseUnsafeRelaxedJsonEscaping = true;
OneBot11Json.UseUnsafeRelaxedJsonEscaping = true;
OneBot12Json.UseUnsafeRelaxedJsonEscaping = true;
```

The default is `false`, which uses `JavaScriptEncoder.Default`. Enable `UnsafeRelaxedJsonEscaping` only when the receiver treats the result as JSON; never embed its output directly into HTML or a script context.

## Console debugging projects

| Protocol | Observable receive-only | EventHandler receive-only | HTTP actions |
| --- | --- | --- | --- |
| OneBot 10 | [Observable](samples/OneBotSdk.Net.V10.ObservableExample) | [EventHandler](samples/OneBotSdk.Net.V10.EventHandlerExample) | [HTTP actions](samples/OneBotSdk.Net.V10.HttpActionExample) |
| OneBot 11 | [Observable](samples/OneBotSdk.Net.ObservableExample) | [EventHandler](samples/OneBotSdk.Net.EventHandlerExample) | [HTTP actions](samples/OneBotSdk.Net.HttpActionExample) |
| OneBot 12 | [Observable](samples/OneBotSdk.Net.V12.ObservableExample) | [EventHandler](samples/OneBotSdk.Net.V12.EventHandlerExample) | [HTTP actions](samples/OneBotSdk.Net.V12.HttpActionExample) |

These projects are tools for connecting to an implementation and debugging interactions; they are not the primary API reference. The protocol documents above contain every Action's parameters and standalone call snippet, plus a handler snippet for every received event. Receive-only debugging projects print complete `RawJson` packets and never send messages. HTTP Action debugging projects may change remote state, so review their source carefully before running them.

## Security and destructive actions

- Never commit a live access token. Replace or rotate any credentials used during local testing before publishing a fork.
- Treat kick, leave, dismiss, delete, restart, cache-cleaning, upload, rename, and request-handling actions as state-changing operations.
- The V10 and especially V11 HTTP examples execute live state-changing calls by default. These include message/like operations and, in V11, deletion, moderation, request handling, cache cleaning, and implementation restart. Some highest-risk leave/kick calls are commented, but that does not make the complete program safe. Never run it unchanged with a production account.
- OneBot 10, 11, and 12 are different wire protocols. Do not point a V10 or V12 client at a OneBot 11 endpoint merely because the host and port look compatible.

## Building and testing

```powershell
dotnet restore OneBotSdk.Net.sln
dotnet build OneBotSdk.Net.sln -c Release --no-restore
dotnet test OneBotSdk.Net.Tests\OneBotSdk.Net.Tests.csproj -c Release -f net10.0 --no-restore
dotnet pack OneBotSdk.Net\OneBotSdk.Net.csproj -c Release --no-restore
```

The command above runs the portable net10.0 test target. The full 12-target executable test matrix includes .NET Framework and therefore requires Windows with a compatible CLR. The test suite validates action catalogs, message and event parsing, request/response traces, endpoint isolation, startup ordering, HTTP/webhook behavior, WebSocket sessions, and cross-version API boundaries.

## Contributing

Issues and pull requests are welcome. Please:

1. Keep public wire types inside the correct versioned namespace.
2. Preserve field-level tolerant parsing and raw JSON snapshots.
3. Add or update xUnit coverage for protocol changes.
4. Keep critical code comments in English first and Chinese second.
5. Run formatting, build, and tests before submitting a pull request.

## License

OneBotSdk.Net is released under the [MIT License](LICENSE).

The OneBot specifications and referenced implementations are separate projects governed by their own licenses and policies.

## Protocol references

- [OneBot 10 specification](https://github.com/botuniverse/onebot-10)
- [OneBot 11 specification](https://github.com/botuniverse/onebot-11)
- [OneBot 12 candidate specification](https://github.com/botuniverse/onebot)
- [OneBot 12 documentation](https://12.onebot.dev/)
