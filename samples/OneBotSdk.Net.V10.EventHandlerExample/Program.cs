using System;
using System.Linq;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Messages;

namespace OneBotSdk.Net.V10.EventHandlerExample
{
    /// <summary>
    /// Demonstrates receive-only OneBot 10 subscriptions with the standard EventHandler pattern.
    /// 演示使用标准 EventHandler 模式进行仅接收的 OneBot 10 事件订阅。
    /// </summary>
    internal static class Program
    {
        private static async Task Main()
        {
            // The SDK default is false; this example explicitly enables less restrictive JSON escaping.
            // SDK 默认值为 false；本示例显式启用限制更少的 JSON 转义。
            OneBot10Json.UseUnsafeRelaxedJsonEscaping = true;

            // Each endpoint owns its address, port, and token.
            // 每个终结点独立拥有自己的地址、端口和令牌。
            var options = new OneBot10BotOptions(
                new OneBot10ActionEndpointOptions("127.0.0.1", 3000, "123456"),
                new OneBot10EventEndpointOptions("127.0.0.1", 3001, "123456"));

            using (var bot = new OneBot10Bot(options))
            {
                // This general handler prints the complete retained packet for every parsed event.
                // 该通用处理器会为每个已解析事件打印完整保留报文。
                bot.Events.EventDispatched += (_, eventArgs) =>
                {
                    Console.WriteLine("Raw event / 原始报文: " + OneBot10Json.Serialize(eventArgs.Event.RawJson));
                };

                // Category handlers receive every event in their corresponding category.
                // 分类处理器会接收对应分类中的每个事件。
                bot.Events.MessageDispatched += (_, eventArgs) =>
                    Console.WriteLine("Message category / 消息分类: type={0}", eventArgs.Event.MessageType);
                bot.Events.NoticeDispatched += (_, eventArgs) =>
                    Console.WriteLine("Notice category / 通知分类: type={0}", eventArgs.Event.NoticeType);
                bot.Events.RequestDispatched += (_, eventArgs) =>
                    Console.WriteLine("Request category / 请求分类: type={0}", eventArgs.Event.RequestType);
                bot.Events.MetaEventDispatched += (_, eventArgs) =>
                    Console.WriteLine("Meta category / 元事件分类: type={0}", eventArgs.Event.MetaEventType);

                // Concrete message handlers expose private, group, and discussion-group objects directly.
                // 具体消息处理器直接公开私聊、群和讨论组消息对象。
                bot.Events.PrivateMessageReceived += (_, eventArgs) =>
                    WriteMessage("Private message / 私聊消息", eventArgs.Event);
                bot.Events.GroupMessageReceived += (_, eventArgs) =>
                    WriteMessage("Group message / 群消息", eventArgs.Event);
                bot.Events.DiscussMessageReceived += (_, eventArgs) =>
                    WriteMessage("Discuss message / 讨论组消息", eventArgs.Event);

                // Every official notice has its own strongly typed EventHandler event.
                // 每个官方通知都有对应的强类型 EventHandler 事件。
                bot.Events.GroupUploadNoticeReceived += (_, args) => WriteConcrete("Group upload / 群文件上传", args.Event);
                bot.Events.GroupAdminNoticeReceived += (_, args) => WriteConcrete("Group admin / 群管理员变动", args.Event);
                bot.Events.GroupDecreaseNoticeReceived += (_, args) => WriteConcrete("Group decrease / 群成员减少", args.Event);
                bot.Events.GroupIncreaseNoticeReceived += (_, args) => WriteConcrete("Group increase / 群成员增加", args.Event);
                bot.Events.GroupBanNoticeReceived += (_, args) => WriteConcrete("Group ban / 群禁言", args.Event);
                bot.Events.FriendAddNoticeReceived += (_, args) => WriteConcrete("Friend add / 好友添加", args.Event);

                // Requests and meta events also expose concrete EventHandler events.
                // 请求和元事件同样公开具体 EventHandler 事件。
                bot.Events.FriendRequestReceived += (_, args) => WriteConcrete("Friend request / 加好友请求", args.Event);
                bot.Events.GroupRequestReceived += (_, args) => WriteConcrete("Group request / 群请求", args.Event);
                bot.Events.LifecycleMetaEventReceived += (_, args) => WriteConcrete("Lifecycle / 生命周期", args.Event);
                bot.Events.HeartbeatMetaEventReceived += (_, args) => WriteConcrete("Heartbeat / 心跳", args.Event);

                Console.WriteLine("Warning: the supplied NapCat configuration is OneBot 11 and cannot be used as a conforming OneBot 10 test server.");
                Console.WriteLine("警告：当前提供的 NapCat 配置是 OneBot 11，不能作为符合规范的 OneBot 10 测试服务器。");

                // StartAsync performs a read-only login check and then starts event listening.
                // StartAsync 先执行只读登录检查，然后启动事件监听。
                var login = await bot.StartAsync();
                Console.WriteLine("Connected account / 已连接账号: user={0}, nickname={1}", login.Data?.UserId, login.Data?.Nickname);
                Console.WriteLine("Listening with EventHandler; no message is ever sent / 正在使用 EventHandler 监听；程序不会发送任何消息。");
                Console.WriteLine("Press Enter to exit / 按 Enter 键退出。");
                Console.ReadLine();
            }
        }

        /// <summary>Writes common message fields and demonstrates message-chain OfType. / 输出公共消息字段并演示消息链 OfType。</summary>
        private static void WriteMessage(string title, OneBot10MessageEvent message)
        {
            Console.WriteLine("{0}: user={1}, message={2}", title, message.UserId, message.RawMessage);

            // LINQ OfType selects concrete objects from the message chain while event delivery remains EventHandler-based.
            // LINQ OfType 从消息链中筛选具体对象，事件分发仍然使用 EventHandler。
            foreach (var text in message.MessageChain.OfType<TextReceivedSegment>())
            {
                Console.WriteLine("Text segment / 文本段: " + text.Text);
            }

            foreach (var image in message.MessageChain.OfType<ImageReceivedSegment>())
            {
                Console.WriteLine("Image segment / 图片段: file={0}, url={1}", image.File, image.Url);
            }

            foreach (var unknown in message.MessageChain.OfType<UnknownReceivedSegment>())
            {
                Console.WriteLine("Unknown segment / 未知段: " + OneBot10Json.Serialize(unknown.RawJson));
            }
        }

        /// <summary>Writes the common identity of a concrete event. / 输出具体事件的公共标识。</summary>
        private static void WriteConcrete(string title, OneBot10Event oneBotEvent)
        {
            Console.WriteLine("{0}: time={1}, self={2}", title, oneBotEvent.Time, oneBotEvent.SelfId);
        }
    }
}
