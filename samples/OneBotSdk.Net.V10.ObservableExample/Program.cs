using System;
using System.Linq;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Events;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Messages;

namespace OneBotSdk.Net.V10.ObservableExample
{
    /// <summary>
    /// Demonstrates receive-only OneBot 10 subscriptions with IObservable and OfType.
    /// 演示使用 IObservable 和 OfType 进行仅接收的 OneBot 10 事件订阅。
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
                // This general subscription prints the complete retained packet for every parsed event.
                // 该通用订阅会为每个已解析事件打印完整保留报文。
                bot.EventReceived.Subscribe(oneBotEvent =>
                {
                    Console.WriteLine("Raw event / 原始报文: " + OneBot10Json.Serialize(oneBotEvent.RawJson));
                });

                // Category subscriptions receive every event in their corresponding category.
                // 分类订阅会接收对应分类中的每个事件。
                bot.MessageReceived.Subscribe(message =>
                    Console.WriteLine("Message category / 消息分类: type={0}", message.MessageType));
                bot.NoticeReceived.Subscribe(notice =>
                    Console.WriteLine("Notice category / 通知分类: type={0}", notice.NoticeType));
                bot.RequestReceived.Subscribe(request =>
                    Console.WriteLine("Request category / 请求分类: type={0}", request.RequestType));
                bot.MetaEventReceived.Subscribe(metaEvent =>
                    Console.WriteLine("Meta category / 元事件分类: type={0}", metaEvent.MetaEventType));

                // Event OfType selects private, group, and discussion-group message objects directly.
                // 事件 OfType 可直接筛选私聊、群和讨论组消息对象。
                bot.EventReceived.OfType<PrivateMessageEvent>().Subscribe(message =>
                    WriteMessage("Private message / 私聊消息", message));
                bot.EventReceived.OfType<GroupMessageEvent>().Subscribe(message =>
                    WriteMessage("Group message / 群消息", message));
                bot.EventReceived.OfType<DiscussMessageEvent>().Subscribe(message =>
                    WriteMessage("Discuss message / 讨论组消息", message));

                // Every official notice has its own concrete observable subscription.
                // 每个官方通知都有对应的具体 Observable 订阅。
                bot.Events.GroupUploadNotices.Subscribe(value => WriteConcrete("Group upload / 群文件上传", value));
                bot.Events.GroupAdminNotices.Subscribe(value => WriteConcrete("Group admin / 群管理员变动", value));
                bot.Events.GroupDecreaseNotices.Subscribe(value => WriteConcrete("Group decrease / 群成员减少", value));
                bot.Events.GroupIncreaseNotices.Subscribe(value => WriteConcrete("Group increase / 群成员增加", value));
                bot.Events.GroupBanNotices.Subscribe(value => WriteConcrete("Group ban / 群禁言", value));
                bot.Events.FriendAddNotices.Subscribe(value => WriteConcrete("Friend add / 好友添加", value));

                // Requests and meta events also expose concrete observable streams.
                // 请求和元事件同样公开具体 Observable 流。
                bot.Events.FriendRequests.Subscribe(value => WriteConcrete("Friend request / 加好友请求", value));
                bot.Events.GroupRequests.Subscribe(value => WriteConcrete("Group request / 群请求", value));
                bot.Events.LifecycleEvents.Subscribe(value => WriteConcrete("Lifecycle / 生命周期", value));
                bot.Events.Heartbeats.Subscribe(value => WriteConcrete("Heartbeat / 心跳", value));

                Console.WriteLine("Warning: the supplied NapCat configuration is OneBot 11 and cannot be used as a conforming OneBot 10 test server.");
                Console.WriteLine("警告：当前提供的 NapCat 配置是 OneBot 11，不能作为符合规范的 OneBot 10 测试服务器。");

                // StartAsync performs a read-only login check and then starts event listening.
                // StartAsync 先执行只读登录检查，然后启动事件监听。
                var login = await bot.StartAsync();
                Console.WriteLine("Connected account / 已连接账号: user={0}, nickname={1}", login.Data?.UserId, login.Data?.Nickname);
                Console.WriteLine("Listening with IObservable; no message is ever sent / 正在使用 IObservable 监听；程序不会发送任何消息。");
                Console.WriteLine("Press Enter to exit / 按 Enter 键退出。");
                Console.ReadLine();
            }
        }

        /// <summary>Writes common message fields and demonstrates message-chain OfType. / 输出公共消息字段并演示消息链 OfType。</summary>
        private static void WriteMessage(string title, OneBot10MessageEvent message)
        {
            Console.WriteLine("{0}: user={1}, message={2}", title, message.UserId, message.RawMessage);

            // LINQ OfType selects concrete objects directly from the strongly typed received message chain.
            // LINQ OfType 可直接从强类型接收消息链中筛选具体对象。
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
