using System;
using System.Linq;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Messages;

namespace OneBotSdk.Net.V12.ObservableExample
{
    /// <summary>
    /// Demonstrates receive-only OneBot 12 subscriptions with IObservable and OfType.
    /// 演示使用 IObservable 和 OfType 进行仅接收的 OneBot 12 事件订阅。
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            // The SDK default is false; this example explicitly enables less restrictive JSON escaping.
            // SDK 默认值为 false；本示例显式启用限制更少的 JSON 转义。
            OneBot12Json.UseUnsafeRelaxedJsonEscaping = true;

            // Action and Event endpoints each own their address, port, and token.
            // Action 与 Event 终结点分别拥有自己的地址、端口和令牌。
            var options = new OneBot12BotOptions(
                new OneBot12ActionEndpointOptions("127.0.0.1", 3000, "123456"),
                new OneBot12EventEndpointOptions("127.0.0.1", 3001, "123456"),
                new OneBot12Self("qq", "123xxxxxxx"));

            using (var bot = new OneBot12Bot(options))
            {
                // Print the complete retained source packet once for every known or unknown event.
                // 为每个已知或未知事件输出一次完整保留的原始报文。
                bot.EventReceived.Subscribe(oneBotEvent =>
                    Console.WriteLine("Raw event / 原始报文: " + OneBot12Json.Serialize(oneBotEvent.RawJson)));

                // OfType exposes each of the three standard message event objects directly.
                // OfType 直接公开三种标准消息事件对象。
                bot.EventReceived.OfType<PrivateMessageEvent>().Subscribe(value => WriteMessage("Private message / 私聊消息", value));
                bot.EventReceived.OfType<GroupMessageEvent>().Subscribe(value => WriteMessage("Group message / 群消息", value));
                bot.EventReceived.OfType<ChannelMessageEvent>().Subscribe(value => WriteMessage("Channel message / 频道消息", value));

                // Every official notice has a discoverable concrete OfType subscription.
                // 每个官方通知都有可发现的具体 OfType 订阅。
                bot.EventReceived.OfType<FriendIncreaseNoticeEvent>().Subscribe(value => WriteEvent("Friend increase / 好友增加", value));
                bot.EventReceived.OfType<FriendDecreaseNoticeEvent>().Subscribe(value => WriteEvent("Friend decrease / 好友减少", value));
                bot.EventReceived.OfType<PrivateMessageDeleteNoticeEvent>().Subscribe(value => WriteEvent("Private message delete / 私聊消息删除", value));
                bot.EventReceived.OfType<GroupMemberIncreaseNoticeEvent>().Subscribe(value => WriteEvent("Group member increase / 群成员增加", value));
                bot.EventReceived.OfType<GroupMemberDecreaseNoticeEvent>().Subscribe(value => WriteEvent("Group member decrease / 群成员减少", value));
                bot.EventReceived.OfType<GroupMessageDeleteNoticeEvent>().Subscribe(value => WriteEvent("Group message delete / 群消息删除", value));
                bot.EventReceived.OfType<GuildMemberIncreaseNoticeEvent>().Subscribe(value => WriteEvent("Guild member increase / 群组成员增加", value));
                bot.EventReceived.OfType<GuildMemberDecreaseNoticeEvent>().Subscribe(value => WriteEvent("Guild member decrease / 群组成员减少", value));
                bot.EventReceived.OfType<ChannelMemberIncreaseNoticeEvent>().Subscribe(value => WriteEvent("Channel member increase / 频道成员增加", value));
                bot.EventReceived.OfType<ChannelMemberDecreaseNoticeEvent>().Subscribe(value => WriteEvent("Channel member decrease / 频道成员减少", value));
                bot.EventReceived.OfType<ChannelMessageDeleteNoticeEvent>().Subscribe(value => WriteEvent("Channel message delete / 频道消息删除", value));
                bot.EventReceived.OfType<ChannelCreateNoticeEvent>().Subscribe(value => WriteEvent("Channel create / 频道创建", value));
                bot.EventReceived.OfType<ChannelDeleteNoticeEvent>().Subscribe(value => WriteEvent("Channel delete / 频道删除", value));

                // The three standard meta events use the same concrete OfType style.
                // 三种标准元事件使用相同的具体 OfType 风格。
                bot.EventReceived.OfType<ConnectMetaEvent>().Subscribe(value => WriteEvent("Connect / 连接", value));
                bot.EventReceived.OfType<HeartbeatMetaEvent>().Subscribe(value => WriteEvent("Heartbeat / 心跳", value));
                bot.EventReceived.OfType<StatusUpdateMetaEvent>().Subscribe(value => WriteEvent("Status update / 状态更新", value));

                Console.WriteLine("Warning: the supplied NapCat configuration is OneBot 11 and cannot be used to run this OneBot 12 example.");
                Console.WriteLine("警告：当前提供的 NapCat 配置是 OneBot 11，不能用于运行此 OneBot 12 示例。");

                // Start performs only read-only meta checks before opening the event connection; it never sends a chat message.
                // Start 仅在打开事件连接前执行只读元动作检查；它绝不会发送聊天消息。
                var start = bot.Start();
                Console.WriteLine("Implementation / 实现端: {0} {1}", start.VersionResponse.Data?.Impl, start.VersionResponse.Data?.Version);
                Console.WriteLine("Status good / 状态正常: {0}", start.StatusResponse.Data?.Good);
                Console.WriteLine("Listening with IObservable; no chat message is ever sent / 正在使用 IObservable 监听；程序不会发送聊天消息。");
                Console.WriteLine("Press Enter to exit / 按 Enter 键退出。");
                Console.ReadLine();
            }
        }

        private static void WriteMessage(string title, OneBot12MessageEvent messageEvent)
        {
            Console.WriteLine("{0}: user={1}, message={2}", title, messageEvent.UserId, messageEvent.AltMessage);
            if (messageEvent.Message == null)
            {
                return;
            }

            // LINQ OfType selects concrete objects directly from the strongly typed received message chain.
            // LINQ OfType 直接从强类型接收消息链中筛选具体对象。
            foreach (var text in messageEvent.Message.OfType<OneBot12TextReceivedSegment>())
            {
                Console.WriteLine("Text segment / 文本段: " + text.Text);
            }

            foreach (var image in messageEvent.Message.OfType<OneBot12ImageReceivedSegment>())
            {
                Console.WriteLine("Image segment / 图片段: file_id=" + image.FileId);
            }

            foreach (var unknown in messageEvent.Message.OfType<OneBot12UnknownReceivedSegment>())
            {
                Console.WriteLine("Unknown segment / 未知段: " + OneBot12Json.Serialize(unknown.RawJson));
            }
        }

        private static void WriteEvent(string title, OneBot12Event oneBotEvent)
        {
            Console.WriteLine("{0}: id={1}, detail={2}, sub_type={3}", title, oneBotEvent.Id, oneBotEvent.DetailType, oneBotEvent.SubType);
        }
    }
}
