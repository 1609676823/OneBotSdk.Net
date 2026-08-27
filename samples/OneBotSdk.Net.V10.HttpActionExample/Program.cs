using System;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Json;
using OneBotSdk.Net.V10.Messages;
using OneBotSdk.Net.V10.Responses;
using OneBotSdk.Net.V10.Transports;
using OneBotSdk.Net.V10.Transports.Http;

namespace OneBotSdk.Net.V10.HttpActionExample
{
    /// <summary>
    /// Demonstrates all 37 public OneBot 10 actions over HTTP without receiving events.
    /// 演示不接收事件时通过 HTTP 使用全部 37 个 OneBot 10 公开动作。
    /// </summary>
    internal static class Program
    {
        private static readonly Uri ActionEndpoint = new Uri("http://127.0.0.1:3000/");

        private const string AccessToken = "123456";
        private const long GroupId = 782351597;
        private const long DiscussId = 0;
        private const string InvalidImageFile = "onebotsdk-net-v10-example.image";
        private const string InvalidRecordFile = "onebotsdk-net-v10-example.record";
        private const string InvalidAnonymousFlag = "onebotsdk-net-v10-example-anonymous-flag";
        private const string InvalidFriendRequestFlag = "onebotsdk-net-v10-example-friend-request-flag";
        private const string InvalidGroupRequestFlag = "onebotsdk-net-v10-example-group-request-flag";

        private static async Task Main()
        {
            // The SDK default is false; this example explicitly enables less restrictive JSON escaping.
            // SDK 默认值为 false；本示例显式启用限制更少的 JSON 转义。
            OneBot10Json.UseUnsafeRelaxedJsonEscaping = true;

            var transportOptions = new OneBot10HttpActionTransportOptions(ActionEndpoint)
            {
                AccessToken = AccessToken
            };

            using (var transport = new OneBot10HttpActionTransport(transportOptions))
            {
                // This example creates only an action client and never opens an event connection.
                // 本示例只创建动作客户端，永远不会建立事件连接。
                var client = new OneBot10Client(transport);

                Console.WriteLine("OneBot 10 HTTP endpoint / OneBot 10 HTTP 地址: {0}", ActionEndpoint);
                Console.WriteLine("Access token / 访问令牌: {0}", AccessToken);
                Console.WriteLine("Message test group / 消息测试群: {0}", GroupId);
                Console.WriteLine("Warning: this address currently serves OneBot 11; do not run the V10 calls until it points to a conforming OneBot 10 server.");
                Console.WriteLine("警告：该地址当前提供 OneBot 11；在它改为符合规范的 OneBot 10 服务器前，请勿运行这些 V10 调用。");
                Console.WriteLine();

                // GetLoginInfoAsync is called first so later examples can reuse the current account ID.
                // 首先调用 GetLoginInfoAsync，以便后续示例复用当前账号 ID。
                WriteTitle("GetLoginInfoAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var loginResponse = await client.GetLoginInfoAsync();
                WriteResponse(loginResponse);
                var loginUserId = loginResponse.Data?.UserId ?? 0;

                WriteTitle("SendPrivateMessageAsync");
                Console.WriteLine("Request variables / 请求变量: userId={0}, message={1}", loginUserId, "OneBotSdk.Net V10 private HTTP test");
                var privateMessageResponse = await client.SendPrivateMessageAsync(loginUserId, OneBot10SendMessage.FromString("OneBotSdk.Net V10 private HTTP test"));
                WriteResponse(privateMessageResponse);

                // All group-message examples use only the user-authorized group below.
                // 所有群消息示例只使用下方用户授权的群。
                WriteTitle("SendGroupMessageAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, message={1}", GroupId, "OneBotSdk.Net V10 group HTTP test");
                var groupMessageResponse = await client.SendGroupMessageAsync(GroupId, OneBot10SendMessage.FromString("OneBotSdk.Net V10 group HTTP test"));
                WriteResponse(groupMessageResponse);
                var groupMessageId = groupMessageResponse.Data?.MessageId ?? 0;

                // DiscussId=0 is intentionally invalid so this action demonstrates its request without messaging an unrelated discussion group.
                // DiscussId=0 是有意设置的无效值，使该动作可演示请求且不会向无关讨论组发消息。
                WriteTitle("SendDiscussMessageAsync");
                Console.WriteLine("Request variables / 请求变量: discussId={0}, message={1}", DiscussId, "OneBotSdk.Net V10 discuss HTTP test");
                var discussMessageResponse = await client.SendDiscussMessageAsync(DiscussId, OneBot10SendMessage.FromString("OneBotSdk.Net V10 discuss HTTP test"));
                WriteResponse(discussMessageResponse);

                WriteTitle("SendMessageAsync");
                Console.WriteLine("Request variables / 请求变量: messageType={0}, groupId={1}, message={2}", OneBot10MessageType.Group, GroupId, "OneBotSdk.Net V10 conditional group HTTP test");
                var conditionalMessageResponse = await client.SendMessageAsync(OneBot10SendMessage.FromString("OneBotSdk.Net V10 conditional group HTTP test"), OneBot10MessageType.Group, groupId: GroupId);
                WriteResponse(conditionalMessageResponse);

                // DANGER: Recalling a message cannot be automatically undone. Review the message ID before manually enabling this block.
                // 危险：撤回消息无法自动恢复。手动启用此代码块前必须检查消息 ID。
                // WriteTitle("DeleteMessageAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: messageId={0}", groupMessageId);
                // var deleteMessageResponse = await client.DeleteMessageAsync(groupMessageId);
                // WriteResponse(deleteMessageResponse);

                WriteTitle("SendLikeAsync");
                Console.WriteLine("Request variables / 请求变量: userId={0}, times={1}", loginUserId, 1);
                var sendLikeResponse = await client.SendLikeAsync(loginUserId, 1);
                WriteResponse(sendLikeResponse);

                // DANGER: Kicking a member changes group membership and cannot be automatically undone. Never run this block without an explicit target review.
                // 危险：踢出成员会改变群成员关系且无法自动撤销。未明确检查目标时绝对不要运行此代码块。
                // WriteTitle("SetGroupKickAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, rejectAddRequest={2}", GroupId, loginUserId, false);
                // var groupKickResponse = await client.SetGroupKickAsync(GroupId, loginUserId, false);
                // WriteResponse(groupKickResponse);

                // DANGER: Muting a member changes live group state. Select the intended member and duration before manually enabling this block.
                // 危险：禁言成员会修改实时群状态。手动启用此代码块前必须确认目标成员和时长。
                // WriteTitle("SetGroupBanAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, duration={2}", GroupId, loginUserId, 60);
                // var groupBanResponse = await client.SetGroupBanAsync(GroupId, loginUserId, 60);
                // WriteResponse(groupBanResponse);

                // DANGER: Anonymous bans cannot be canceled through the protocol. Use only a flag copied from the intended event.
                // 危险：匿名禁言无法通过协议取消。只能使用从目标事件中复制的 flag。
                // WriteTitle("SetGroupAnonymousBanAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, anonymousFlag={1}, duration={2}", GroupId, InvalidAnonymousFlag, 60);
                // var anonymousBanResponse = await client.SetGroupAnonymousBanAsync(GroupId, InvalidAnonymousFlag, 60);
                // WriteResponse(anonymousBanResponse);

                // DANGER: Whole-group muting immediately affects every member. This block is intentionally disabled.
                // 危险：全员禁言会立即影响所有成员。此代码块已有意禁用。
                // WriteTitle("SetGroupWholeBanAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, enable={1}", GroupId, true);
                // var wholeBanResponse = await client.SetGroupWholeBanAsync(GroupId, true);
                // WriteResponse(wholeBanResponse);

                // DANGER: Administrator changes alter permissions for the group. This block is intentionally disabled.
                // 危险：管理员变更会修改群权限。此代码块已有意禁用。
                // WriteTitle("SetGroupAdminAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, enable={2}", GroupId, loginUserId, true);
                // var groupAdminResponse = await client.SetGroupAdminAsync(GroupId, loginUserId, true);
                // WriteResponse(groupAdminResponse);

                // DANGER: This changes the group's anonymous-chat setting. This block is intentionally disabled.
                // 危险：此动作会修改群匿名聊天设置。此代码块已有意禁用。
                // WriteTitle("SetGroupAnonymousAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, enable={1}", GroupId, true);
                // var groupAnonymousResponse = await client.SetGroupAnonymousAsync(GroupId, true);
                // WriteResponse(groupAnonymousResponse);

                // DANGER: This changes a live member card. Save the original value before manually enabling this block.
                // 危险：此动作会修改实时群名片。手动启用前必须保存原值。
                // WriteTitle("SetGroupCardAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, card={2}", GroupId, loginUserId, "OneBotSdk.Net V10 HTTP test");
                // var groupCardResponse = await client.SetGroupCardAsync(GroupId, loginUserId, "OneBotSdk.Net V10 HTTP test");
                // WriteResponse(groupCardResponse);

                // EXTREME DANGER: A group owner may permanently dissolve the group. Never enable this example block during automated testing.
                // 极度危险：群主可能永久解散该群。自动测试期间绝对不要启用此示例代码块。
                // WriteTitle("SetGroupLeaveAsync - EXTREME DANGER / 极度危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, isDismiss={1}", GroupId, false);
                // var groupLeaveResponse = await client.SetGroupLeaveAsync(GroupId, false);
                // WriteResponse(groupLeaveResponse);

                // DANGER: This changes a live member title. Save the original value before manually enabling this block.
                // 危险：此动作会修改实时成员头衔。手动启用前必须保存原值。
                // WriteTitle("SetGroupSpecialTitleAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, specialTitle={2}, duration={3}", GroupId, loginUserId, "HTTP test", -1);
                // var specialTitleResponse = await client.SetGroupSpecialTitleAsync(GroupId, loginUserId, "HTTP test", -1);
                // WriteResponse(specialTitleResponse);

                // EXTREME DANGER: Leaving a discussion group changes membership and cannot be automatically undone.
                // 极度危险：退出讨论组会改变成员关系且无法自动撤销。
                // WriteTitle("SetDiscussLeaveAsync - EXTREME DANGER / 极度危险");
                // Console.WriteLine("Request variables / 请求变量: discussId={0}", DiscussId);
                // var discussLeaveResponse = await client.SetDiscussLeaveAsync(DiscussId);
                // WriteResponse(discussLeaveResponse);

                // DANGER: Processing a request is externally visible and cannot be replayed reliably. Supply a real reviewed flag before enabling.
                // 危险：处理请求对外可见且无法可靠重放。启用前必须提供经过检查的真实 flag。
                // WriteTitle("SetFriendAddRequestAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: flag={0}, approve={1}, remark={2}", InvalidFriendRequestFlag, false, "OneBotSdk.Net V10 HTTP test");
                // var friendRequestResponse = await client.SetFriendAddRequestAsync(InvalidFriendRequestFlag, false, "OneBotSdk.Net V10 HTTP test");
                // WriteResponse(friendRequestResponse);

                // DANGER: Processing a group request is externally visible and cannot be replayed reliably. Supply a real reviewed flag before enabling.
                // 危险：处理群请求对外可见且无法可靠重放。启用前必须提供经过检查的真实 flag。
                // WriteTitle("SetGroupAddRequestAsync - DANGER / 危险");
                // Console.WriteLine("Request variables / 请求变量: flag={0}, requestType={1}, approve={2}, reason={3}", InvalidGroupRequestFlag, OneBot10GroupRequestType.Add, false, "OneBotSdk.Net V10 HTTP test");
                // var groupRequestResponse = await client.SetGroupAddRequestAsync(InvalidGroupRequestFlag, OneBot10GroupRequestType.Add, false, "OneBotSdk.Net V10 HTTP test");
                // WriteResponse(groupRequestResponse);

                WriteTitle("GetStrangerInfoAsync");
                Console.WriteLine("Request variables / 请求变量: userId={0}, noCache={1}", loginUserId, true);
                var strangerResponse = await client.GetStrangerInfoAsync(loginUserId, true);
                WriteResponse(strangerResponse);

                WriteTitle("GetFriendListAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var friendListResponse = await client.GetFriendListAsync();
                WriteResponse(friendListResponse);

                WriteTitle("GetGroupListAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var groupListResponse = await client.GetGroupListAsync();
                WriteResponse(groupListResponse);

                WriteTitle("GetGroupInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, noCache={1}", GroupId, true);
                var groupInfoResponse = await client.GetGroupInfoAsync(GroupId, true);
                WriteResponse(groupInfoResponse);

                WriteTitle("GetGroupMemberInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, noCache={2}", GroupId, loginUserId, true);
                var groupMemberResponse = await client.GetGroupMemberInfoAsync(GroupId, loginUserId, true);
                WriteResponse(groupMemberResponse);

                WriteTitle("GetGroupMemberListAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}", GroupId);
                var groupMemberListResponse = await client.GetGroupMemberListAsync(GroupId);
                WriteResponse(groupMemberListResponse);

                WriteTitle("GetCookiesAsync");
                Console.WriteLine("Request variables / 请求变量: domain={0}", string.Empty);
                var cookiesResponse = await client.GetCookiesAsync();
                WriteResponse(cookiesResponse);

                WriteTitle("GetCsrfTokenAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var csrfTokenResponse = await client.GetCsrfTokenAsync();
                WriteResponse(csrfTokenResponse);

                WriteTitle("GetCredentialsAsync");
                Console.WriteLine("Request variables / 请求变量: domain={0}", string.Empty);
                var credentialsResponse = await client.GetCredentialsAsync();
                WriteResponse(credentialsResponse);

                WriteTitle("GetRecordAsync");
                Console.WriteLine("Request variables / 请求变量: file={0}, outputFormat={1}, fullPath={2}", InvalidRecordFile, OneBot10RecordFormat.Mp3, false);
                var recordResponse = await client.GetRecordAsync(InvalidRecordFile, OneBot10RecordFormat.Mp3, false);
                WriteResponse(recordResponse);

                WriteTitle("GetImageAsync");
                Console.WriteLine("Request variables / 请求变量: file={0}", InvalidImageFile);
                var imageResponse = await client.GetImageAsync(InvalidImageFile);
                WriteResponse(imageResponse);

                WriteTitle("CanSendImageAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var canSendImageResponse = await client.CanSendImageAsync();
                WriteResponse(canSendImageResponse);

                WriteTitle("CanSendRecordAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var canSendRecordResponse = await client.CanSendRecordAsync();
                WriteResponse(canSendRecordResponse);

                WriteTitle("GetStatusAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var statusResponse = await client.GetStatusAsync();
                WriteResponse(statusResponse);

                WriteTitle("GetVersionInfoAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var versionResponse = await client.GetVersionInfoAsync();
                WriteResponse(versionResponse);

                // EXTREME DANGER: Restarting CQHTTP interrupts active connections and the current response may never arrive.
                // 极度危险：重启 CQHTTP 会中断活动连接，当前响应可能永远无法返回。
                // WriteTitle("SetRestartPluginAsync - EXTREME DANGER / 极度危险");
                // Console.WriteLine("Request variables / 请求变量: delay={0}", 0);
                // var restartResponse = await client.SetRestartPluginAsync(0);
                // WriteResponse(restartResponse);

                // EXTREME DANGER: Cleaning a data directory permanently deletes implementation files. This block must remain disabled during normal testing.
                // 极度危险：清理数据目录会永久删除实现端文件。正常测试时必须保持此代码块禁用。
                // WriteTitle("CleanDataDirectoryAsync - EXTREME DANGER / 极度危险");
                // Console.WriteLine("Request variables / 请求变量: dataDirectory={0}", OneBot10DataDirectory.Image);
                // var cleanDataResponse = await client.CleanDataDirectoryAsync(OneBot10DataDirectory.Image);
                // WriteResponse(cleanDataResponse);

                // EXTREME DANGER: Cleaning the plug-in log permanently removes diagnostic history. This block must remain disabled during normal testing.
                // 极度危险：清理插件日志会永久删除诊断历史。正常测试时必须保持此代码块禁用。
                // WriteTitle("CleanPluginLogAsync - EXTREME DANGER / 极度危险");
                // Console.WriteLine("Request variables / 请求变量: none / 无");
                // var cleanLogResponse = await client.CleanPluginLogAsync();
                // WriteResponse(cleanLogResponse);
            }
        }

        /// <summary>Writes a readable title before an action call. / 在动作调用前输出易读标题。</summary>
        private static void WriteTitle(string methodName)
        {
            Console.WriteLine("========== {0} ==========", methodName);
        }

        /// <summary>
        /// Writes the trace and raw data of an untyped action response.
        /// 输出非泛型动作响应的跟踪信息和原始数据。
        /// </summary>
        private static void WriteResponse(OneBot10Response response)
        {
            WriteCommonResponse(response);
            Console.WriteLine("Data / 返回数据: {0}", OneBot10Json.Serialize(response.Data));
            Console.WriteLine("RawData / 原始返回数据: exposed as Data for an untyped response / 非泛型响应通过 Data 公开");
            Console.WriteLine("RawResponseJson / 返回报文: {0}", response.RawResponseJson);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes the trace, typed data, and independent raw data of a typed action response.
        /// 输出强类型动作响应的跟踪信息、强类型数据和独立原始数据。
        /// </summary>
        private static void WriteResponse<TData>(OneBot10Response<TData> response)
        {
            WriteCommonResponse(response);
            Console.WriteLine("Data / 强类型返回数据: {0}", OneBot10Json.Serialize(response.Data));
            Console.WriteLine("RawData / 原始返回数据: {0}", OneBot10Json.Serialize(response.RawData));
            Console.WriteLine("RawResponseJson / 返回报文: {0}", response.RawResponseJson);
            Console.WriteLine();
        }

        /// <summary>Writes request and envelope fields shared by every response. / 输出每个响应共享的请求和信封字段。</summary>
        private static void WriteCommonResponse(OneBot10ResponseBase response)
        {
            Console.WriteLine("Action / 动作: {0}", response.Action);
            Console.WriteLine("RequestParameters / 请求参数: {0}", OneBot10Json.Serialize(response.RequestParameters));
            Console.WriteLine("RequestEcho / 请求关联值: {0}", OneBot10Json.Serialize(response.RequestEcho));
            Console.WriteLine("RawRequestJson / 请求报文: {0}", response.RawRequestJson);
            Console.WriteLine("Status / 返回状态: {0}", response.Status);
            Console.WriteLine("RetCode / 返回码: {0}", response.RetCode);
        }
    }
}
