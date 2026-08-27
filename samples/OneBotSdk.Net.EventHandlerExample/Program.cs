using System;
using System.Linq;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Events;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;

namespace OneBotSdk.Net.EventHandlerExample
{
    /// <summary>
    /// Demonstrates receive-only OneBot event subscriptions with the standard EventHandler pattern.
    /// 演示仅接收模式下使用标准 EventHandler 模式订阅 OneBot 事件。
    /// </summary>
    internal static class Program
    {
        private static async Task Main()
        {
            // The SDK default is false; this example explicitly enables less restrictive JSON escaping.
            // SDK 默认值为 false；本示例显式启用限制更少的 JSON 转义方式。
            // This setting affects serialization only and does not relax JSON parsing or deserialization rules.
            // 此设置只影响序列化，不会放宽 JSON 解析或反序列化规则。
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = true;

            // Each endpoint owns its address, port, and token; values use the masked onebot11_123xxxxxxx.json example.
            // 每个终结点分别拥有地址、端口和令牌；以下值来自脱敏后的 onebot11_123xxxxxxx.json 示例。
            var options = new OneBot11BotOptions(
                new OneBot11ActionEndpointOptions("127.0.0.1", 3000, "123456"),
                new OneBot11EventEndpointOptions("127.0.0.1", 3001, "123456"));

            using (var bot = new OneBot11Bot(options))
            {
                // Print the complete raw JSON once for every parsed event, including unknown fallbacks.
                // 为每个已解析事件打印一次完整原始 JSON，包括未知回退事件。
                bot.Events.EventDispatched += (_, eventArgs) =>
                {
                    Console.WriteLine("Raw event / 原始报文: " + OneBot11Json.Serialize(eventArgs.Event.RawJson));
                };

                // Private messages. / 私聊消息。
                bot.Events.PrivateMessageReceived += (_, eventArgs) =>
                {
                    var message = eventArgs.Event;
                    Console.WriteLine("Private message / 私聊消息: user={0}, message={1}", message.UserId, message.RawMessage);

                    foreach (var text in message.MessageChain.OfType<TextReceivedSegment>())
                    {
                        Console.WriteLine("Private text segment / 私聊文本段: " + text.Text);
                    }
                };

                // Group messages. / 群消息。
                bot.Events.GroupMessageReceived += (_, eventArgs) =>
                {
                    var message = eventArgs.Event;
                    Console.WriteLine("Group message / 群消息: group={0}, user={1}, message={2}", message.GroupId, message.UserId, message.RawMessage);

                    // Message-chain OfType filters received segment classes; event delivery still uses EventHandler.
                    // 消息链 OfType 用于筛选接收消息段类型；事件分发仍然使用 EventHandler。
                    foreach (var text in message.MessageChain.OfType<TextReceivedSegment>())
                    {
                        Console.WriteLine("Group text segment / 群文本段: " + text.Text);
                    }

                    foreach (var image in message.MessageChain.OfType<ImageReceivedSegment>())
                    {
                        Console.WriteLine("Group image segment / 群图片段: file={0}, url={1}", image.File, image.Url);
                    }
                };

                // Group requests. / 群请求。
                bot.Events.GroupRequestReceived += (_, eventArgs) =>
                {
                    var request = eventArgs.Event;
                    Console.WriteLine("Group request / 群请求: group={0}, user={1}, comment={2}", request.GroupId, request.UserId, request.Comment);
                };

                // Friend requests. / 好友请求。
                bot.Events.FriendRequestReceived += (_, eventArgs) =>
                {
                    var request = eventArgs.Event;
                    Console.WriteLine("Friend request / 好友请求: user={0}, comment={1}", request.UserId, request.Comment);
                };

                // The category handler receives every standard and implementation-specific notice.
                // 分类处理器接收所有标准通知及实现端扩展通知。
                bot.Events.NoticeDispatched += (_, eventArgs) =>
                {
                    Console.WriteLine("Notice / 通知事件: type=" + eventArgs.Event.NoticeType);
                };

                // The category handler receives lifecycle and heartbeat meta events.
                // 分类处理器接收生命周期和心跳元事件。
                bot.Events.MetaEventDispatched += (_, eventArgs) =>
                {
                    Console.WriteLine("Meta event / 元事件: type=" + eventArgs.Event.MetaEventType);
                };

                // StartAsync performs the read-only get_login_info check and then starts event listening.
                // StartAsync 先执行只读的 get_login_info 检查，然后启动事件监听。
                var loginInfo = await bot.StartAsync();
                Console.WriteLine("Connected account / 已连接账号: user={0}, nickname={1}", loginInfo.Data?.UserId, loginInfo.Data?.Nickname);
                Console.WriteLine("Listening with EventHandler; this example never sends messages / 正在使用 EventHandler 监听；本示例不会发送任何消息。");
                Console.WriteLine("Press Enter to exit / 按 Enter 键退出。");
                Console.ReadLine();
            }
        }
    }
}
