using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Messages;
using OneBotSdk.Net.V11.Responses;
using OneBotSdk.Net.V11.Transports;
using OneBotSdk.Net.V11.Transports.Http;

namespace OneBotSdk.Net.HttpActionExample
{
    /// <summary>
    /// Demonstrates every HTTP action call directly without opening an event connection.
    /// 直接演示每个 HTTP 动作调用，不建立事件连接。
    /// </summary>
    internal static class Program
    {
        private static readonly Uri ActionEndpoint = new Uri("http://127.0.0.1:3000/");

        private const string AccessToken = "123456";
        private const long GroupId = 782351597;
        private const string InvalidForwardId = "onebotsdk-net-http-example-forward-id";
        private const string InvalidImageFile = "onebotsdk-net-http-example-image.file";
        private const string InvalidRecordFile = "onebotsdk-net-http-example-record.file";
        private const string InvalidAnonymousFlag = "onebotsdk-net-http-example-anonymous-flag";
        private const string InvalidFriendRequestFlag = "onebotsdk-net-http-example-friend-request-flag";
        private const string InvalidGroupRequestFlag = "onebotsdk-net-http-example-group-request-flag";

        private static async Task Main()
        {
            // The SDK default is false; this example explicitly enables less restrictive JSON escaping.
            // SDK 默认值为 false；本示例显式启用限制更少的 JSON 转义方式。
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = true;

            var transportOptions = new OneBot11HttpActionTransportOptions(ActionEndpoint)
            {
                AccessToken = AccessToken
            };

            using (var transport = new OneBot11HttpActionTransport(transportOptions))
            {
                // HTTP actions only need the action transport and client; no event endpoint is connected.
                // HTTP 动作只需要动作传输和客户端；不会连接事件终结点。
                var client = new OneBot11Client(transport);

                Console.WriteLine("OneBot HTTP endpoint / OneBot HTTP 地址: {0}", ActionEndpoint);
                Console.WriteLine("Access token / 访问令牌: {0}", AccessToken);
                Console.WriteLine("Test group / 测试群: {0}", GroupId);
                Console.WriteLine();

                // Gets the current login account and reuses its identifier in later examples.
                // 获取当前登录账号，并在后续示例中复用账号标识。
                WriteTitle("GetLoginInfoAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var loginResponse = await client.GetLoginInfoAsync();
                WriteResponse(loginResponse);
                var loginUserId = loginResponse.Data?.UserId ?? 0;

                // Calls an action through the untyped extension entry point.
                // 通过非泛型扩展入口调用动作。
                WriteTitle("CallActionAsync");
                Console.WriteLine("Request variables / 请求变量: action={0}", OneBot11Actions.GetStatus);
                var rawActionResponse = await client.CallActionAsync(OneBot11Actions.GetStatus);
                WriteResponse(rawActionResponse);

                // Calls an action with a caller-supplied data parser.
                // 使用调用方提供的数据解析器调用动作。
                WriteTitle("CallActionAsync<TData>");
                Console.WriteLine("Request variables / 请求变量: action={0}, dataParser=node => node", OneBot11Actions.GetVersionInfo);
                var typedActionResponse = await client.CallActionAsync<JsonNode>(OneBot11Actions.GetVersionInfo, node => node);
                WriteResponse(typedActionResponse);

                // Executes the hidden quick-operation action with an explicit test context.
                // 使用明确的测试上下文执行隐藏的快速操作动作。
                var quickContext = new JsonObject
                {
                    ["post_type"] = "message",
                    ["message_type"] = "group",
                    ["group_id"] = GroupId,
                    ["user_id"] = loginUserId,
                    ["message_id"] = 0
                };
                var quickOperation = new JsonObject();
                WriteTitle("HandleQuickOperationAsync");
                Console.WriteLine("Request variables / 请求变量: context={0}, operation={1}", OneBot11Json.Serialize(quickContext), OneBot11Json.Serialize(quickOperation));
                var quickOperationResponse = await client.HandleQuickOperationAsync(quickContext, quickOperation);
                WriteResponse(quickOperationResponse);

                // Gets information about the current account as a QQ user.
                // 获取当前账号对应的 QQ 用户信息。
                WriteTitle("GetStrangerInfoAsync");
                Console.WriteLine("Request variables / 请求变量: userId={0}, noCache={1}", loginUserId, true);
                var strangerResponse = await client.GetStrangerInfoAsync(loginUserId, true);
                WriteResponse(strangerResponse);

                // Gets the complete friend list.
                // 获取完整好友列表。
                WriteTitle("GetFriendListAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var friendListResponse = await client.GetFriendListAsync();
                WriteResponse(friendListResponse);

                // Gets the configured test group and remembers its original name.
                // 获取配置的测试群，并保存原群名称。
                WriteTitle("GetGroupInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, noCache={1}", GroupId, true);
                var groupInfoResponse = await client.GetGroupInfoAsync(GroupId, true);
                WriteResponse(groupInfoResponse);
                var originalGroupName = groupInfoResponse.Data?.GroupName ?? "BNS查询测试";

                // Gets all groups visible to the current account.
                // 获取当前账号可见的全部群。
                WriteTitle("GetGroupListAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var groupListResponse = await client.GetGroupListAsync();
                WriteResponse(groupListResponse);

                // Gets the current account's member information and remembers mutable values.
                // 获取当前账号的群成员信息，并保存可变字段的原始值。
                WriteTitle("GetGroupMemberInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, noCache={2}", GroupId, loginUserId, true);
                var groupMemberInfoResponse = await client.GetGroupMemberInfoAsync(GroupId, loginUserId, true);
                WriteResponse(groupMemberInfoResponse);
                var originalGroupCard = groupMemberInfoResponse.Data?.Card ?? string.Empty;
                var originalSpecialTitle = groupMemberInfoResponse.Data?.Title ?? string.Empty;

                // Gets the complete member list of the configured group.
                // 获取配置群的完整成员列表。
                WriteTitle("GetGroupMemberListAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}", GroupId);
                var groupMemberListResponse = await client.GetGroupMemberListAsync(GroupId);
                WriteResponse(groupMemberListResponse);

                // Gets every standard group-honor category.
                // 获取全部标准群荣誉类别。
                WriteTitle("GetGroupHonorInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, honorType={1}", GroupId, OneBot11GroupHonorType.All);
                var groupHonorResponse = await client.GetGroupHonorInfoAsync(GroupId, OneBot11GroupHonorType.All);
                WriteResponse(groupHonorResponse);

                // Gets cookies without restricting the domain.
                // 获取不限定域名的 Cookies。
                WriteTitle("GetCookiesAsync");
                Console.WriteLine("Request variables / 请求变量: domain={0}", string.Empty);
                var cookiesResponse = await client.GetCookiesAsync(string.Empty);
                WriteResponse(cookiesResponse);

                // Gets the current CSRF token.
                // 获取当前 CSRF Token。
                WriteTitle("GetCsrfTokenAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var csrfTokenResponse = await client.GetCsrfTokenAsync();
                WriteResponse(csrfTokenResponse);

                // Gets cookies and the CSRF token together.
                // 同时获取 Cookies 和 CSRF Token。
                WriteTitle("GetCredentialsAsync");
                Console.WriteLine("Request variables / 请求变量: domain={0}", string.Empty);
                var credentialsResponse = await client.GetCredentialsAsync(string.Empty);
                WriteResponse(credentialsResponse);

                // Uses an explicit file variable; replace it with a record file value from a received message when available.
                // 使用明确的文件变量；实际使用时可替换为接收消息中的语音 file 值。
                WriteTitle("GetRecordAsync");
                Console.WriteLine("Request variables / 请求变量: file={0}, outputFormat={1}", InvalidRecordFile, OneBot11RecordFormat.Mp3);
                var recordResponse = await client.GetRecordAsync(InvalidRecordFile, OneBot11RecordFormat.Mp3);
                WriteResponse(recordResponse);

                // Uses an explicit file variable; replace it with an image file value from a received message when available.
                // 使用明确的文件变量；实际使用时可替换为接收消息中的图片 file 值。
                WriteTitle("GetImageAsync");
                Console.WriteLine("Request variables / 请求变量: file={0}", InvalidImageFile);
                var imageResponse = await client.GetImageAsync(InvalidImageFile);
                WriteResponse(imageResponse);

                // Queries image-send capability.
                // 查询图片发送能力。
                WriteTitle("CanSendImageAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var canSendImageResponse = await client.CanSendImageAsync();
                WriteResponse(canSendImageResponse);

                // Queries record-send capability.
                // 查询语音发送能力。
                WriteTitle("CanSendRecordAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var canSendRecordResponse = await client.CanSendRecordAsync();
                WriteResponse(canSendRecordResponse);

                // Gets the implementation status.
                // 获取实现端状态。
                WriteTitle("GetStatusAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var statusResponse = await client.GetStatusAsync();
                WriteResponse(statusResponse);

                // Gets implementation and protocol version information.
                // 获取实现端和协议版本信息。
                WriteTitle("GetVersionInfoAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var versionInfoResponse = await client.GetVersionInfoAsync();
                WriteResponse(versionInfoResponse);

                // Sends a private message with the outgoing-only message model.
                // 使用仅出站消息模型发送私聊消息。
                var privateSendMessage = new OneBot11SendMessage().Text("[OneBotSdk.Net HTTP test] SendPrivateMessageAsync");
                WriteTitle("SendPrivateMessageAsync (OneBot11SendMessage)");
                Console.WriteLine("Request variables / 请求变量: userId={0}, message={1}, autoEscape={2}", loginUserId, OneBot11Json.Serialize(privateSendMessage), false);
                var privateSendResponse = await client.SendPrivateMessageAsync(loginUserId, privateSendMessage, false);
                WriteResponse(privateSendResponse);

                // Sends a private message with the compatibility message model.
                // 使用兼容消息模型发送私聊消息。
                var privateCompatibilityMessage = OneBot11Message.FromString("[OneBotSdk.Net HTTP test] SendPrivateMessageAsync compatibility overload");
                WriteTitle("SendPrivateMessageAsync (OneBot11Message)");
                Console.WriteLine("Request variables / 请求变量: userId={0}, message={1}, autoEscape={2}", loginUserId, OneBot11Json.Serialize(privateCompatibilityMessage), false);
                var privateCompatibilityResponse = await client.SendPrivateMessageAsync(loginUserId, privateCompatibilityMessage, false);
                WriteResponse(privateCompatibilityResponse);

                // Sends a group message with the outgoing-only message model.
                // 使用仅出站消息模型发送群消息。
                var groupSendMessage = new OneBot11SendMessage().Text("[OneBotSdk.Net HTTP test] SendGroupMessageAsync");
                WriteTitle("SendGroupMessageAsync (OneBot11SendMessage)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, message={1}, autoEscape={2}", GroupId, OneBot11Json.Serialize(groupSendMessage), false);
                var groupSendResponse = await client.SendGroupMessageAsync(GroupId, groupSendMessage, false);
                WriteResponse(groupSendResponse);
                var groupSendMessageId = groupSendResponse.Data?.MessageId ?? 0;

                // Gets the message that was just sent.
                // 获取刚刚发送的消息。
                WriteTitle("GetMessageAsync");
                Console.WriteLine("Request variables / 请求变量: messageId={0}", groupSendMessageId);
                var getMessageResponse = await client.GetMessageAsync(groupSendMessageId);
                WriteResponse(getMessageResponse);

                // Deletes the message that was just queried.
                // 撤回刚刚查询的消息。
                WriteTitle("DeleteMessageAsync");
                Console.WriteLine("Request variables / 请求变量: messageId={0}", groupSendMessageId);
                var deleteMessageResponse = await client.DeleteMessageAsync(groupSendMessageId);
                WriteResponse(deleteMessageResponse);

                // Sends a group message with the compatibility message model.
                // 使用兼容消息模型发送群消息。
                var groupCompatibilityMessage = OneBot11Message.FromString("[OneBotSdk.Net HTTP test] SendGroupMessageAsync compatibility overload");
                WriteTitle("SendGroupMessageAsync (OneBot11Message)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, message={1}, autoEscape={2}", GroupId, OneBot11Json.Serialize(groupCompatibilityMessage), false);
                var groupCompatibilityResponse = await client.SendGroupMessageAsync(GroupId, groupCompatibilityMessage, false);
                WriteResponse(groupCompatibilityResponse);
                var groupCompatibilityMessageId = groupCompatibilityResponse.Data?.MessageId ?? 0;

                // Deletes the compatibility-overload test message after its response has been printed.
                // 打印兼容重载响应后撤回对应测试消息。
                WriteTitle("DeleteMessageAsync (compatibility message cleanup)");
                Console.WriteLine("Request variables / 请求变量: messageId={0}", groupCompatibilityMessageId);
                var deleteCompatibilityMessageResponse = await client.DeleteMessageAsync(groupCompatibilityMessageId);
                WriteResponse(deleteCompatibilityMessageResponse);

                // Sends a conditionally targeted group message with the outgoing-only model.
                // 使用仅出站模型发送指定目标类型的群消息。
                var conditionalSendMessage = new OneBot11SendMessage().Text("[OneBotSdk.Net HTTP test] SendMessageAsync");
                WriteTitle("SendMessageAsync (OneBot11SendMessage)");
                Console.WriteLine("Request variables / 请求变量: messageType={0}, groupId={1}, message={2}, autoEscape={3}", OneBot11MessageType.Group, GroupId, OneBot11Json.Serialize(conditionalSendMessage), false);
                var conditionalSendResponse = await client.SendMessageAsync(conditionalSendMessage, OneBot11MessageType.Group, groupId: GroupId, autoEscape: false);
                WriteResponse(conditionalSendResponse);
                var conditionalSendMessageId = conditionalSendResponse.Data?.MessageId ?? 0;

                // Deletes the conditionally targeted test message.
                // 撤回指定目标类型的测试消息。
                WriteTitle("DeleteMessageAsync (SendMessageAsync cleanup)");
                Console.WriteLine("Request variables / 请求变量: messageId={0}", conditionalSendMessageId);
                var deleteConditionalMessageResponse = await client.DeleteMessageAsync(conditionalSendMessageId);
                WriteResponse(deleteConditionalMessageResponse);

                // Sends a conditionally targeted group message with the compatibility model.
                // 使用兼容模型发送指定目标类型的群消息。
                var conditionalCompatibilityMessage = OneBot11Message.FromString("[OneBotSdk.Net HTTP test] SendMessageAsync compatibility overload");
                WriteTitle("SendMessageAsync (OneBot11Message)");
                Console.WriteLine("Request variables / 请求变量: messageType={0}, groupId={1}, message={2}, autoEscape={3}", OneBot11MessageType.Group, GroupId, OneBot11Json.Serialize(conditionalCompatibilityMessage), false);
                var conditionalCompatibilityResponse = await client.SendMessageAsync(conditionalCompatibilityMessage, OneBot11MessageType.Group, groupId: GroupId, autoEscape: false);
                WriteResponse(conditionalCompatibilityResponse);
                var conditionalCompatibilityMessageId = conditionalCompatibilityResponse.Data?.MessageId ?? 0;

                // Deletes the compatibility SendMessageAsync test message.
                // 撤回兼容 SendMessageAsync 测试消息。
                WriteTitle("DeleteMessageAsync (compatibility SendMessageAsync cleanup)");
                Console.WriteLine("Request variables / 请求变量: messageId={0}", conditionalCompatibilityMessageId);
                var deleteConditionalCompatibilityResponse = await client.DeleteMessageAsync(conditionalCompatibilityMessageId);
                WriteResponse(deleteConditionalCompatibilityResponse);

                // Uses an explicit forward identifier; replace it with an identifier from a received forward segment when available.
                // 使用明确的合并转发标识；实际使用时可替换为接收转发消息段中的标识。
                WriteTitle("GetForwardMessageAsync");
                Console.WriteLine("Request variables / 请求变量: id={0}", InvalidForwardId);
                var forwardMessageResponse = await client.GetForwardMessageAsync(InvalidForwardId);
                WriteResponse(forwardMessageResponse);

                // Sends one like to the current account identifier.
                // 向当前账号标识发送一个赞。
                WriteTitle("SendLikeAsync");
                Console.WriteLine("Request variables / 请求变量: userId={0}, times={1}", loginUserId, 1);
                var sendLikeResponse = await client.SendLikeAsync(loginUserId, 1);
                WriteResponse(sendLikeResponse);

                // Bans and then unbans the current account in the configured group.
                // 在配置群中禁言当前账号，然后解除禁言。
                WriteTitle("SetGroupBanAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, duration={2}", GroupId, loginUserId, 1);
                var groupBanResponse = await client.SetGroupBanAsync(GroupId, loginUserId, 1);
                WriteResponse(groupBanResponse);

                WriteTitle("SetGroupBanAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, duration={2}", GroupId, loginUserId, 0);
                var groupUnbanResponse = await client.SetGroupBanAsync(GroupId, loginUserId, 0);
                WriteResponse(groupUnbanResponse);

                // Calls the anonymous-ban overload that accepts an event flag.
                // 调用接收事件 flag 的匿名禁言重载。
                WriteTitle("SetGroupAnonymousBanAsync (string)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, anonymousFlag={1}, duration={2}", GroupId, InvalidAnonymousFlag, 1);
                var anonymousFlagBanResponse = await client.SetGroupAnonymousBanAsync(GroupId, InvalidAnonymousFlag, 1);
                WriteResponse(anonymousFlagBanResponse);

                // Calls the anonymous-ban overload that accepts the complete anonymous object.
                // 调用接收完整匿名对象的匿名禁言重载。
                var anonymous = new JsonObject { ["flag"] = InvalidAnonymousFlag };
                WriteTitle("SetGroupAnonymousBanAsync (JsonObject)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, anonymous={1}, duration={2}", GroupId, OneBot11Json.Serialize(anonymous), 1);
                var anonymousObjectBanResponse = await client.SetGroupAnonymousBanAsync(GroupId, anonymous, 1);
                WriteResponse(anonymousObjectBanResponse);

                // Enables and then disables whole-group mute.
                // 启用全员禁言，然后关闭全员禁言。
                WriteTitle("SetGroupWholeBanAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, enable={1}", GroupId, true);
                var enableWholeBanResponse = await client.SetGroupWholeBanAsync(GroupId, true);
                WriteResponse(enableWholeBanResponse);

                WriteTitle("SetGroupWholeBanAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, enable={1}", GroupId, false);
                var disableWholeBanResponse = await client.SetGroupWholeBanAsync(GroupId, false);
                WriteResponse(disableWholeBanResponse);

                // Enables and then removes administrator status for the current account.
                // 为当前账号设置管理员，然后取消管理员身份。
                WriteTitle("SetGroupAdminAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, enable={2}", GroupId, loginUserId, true);
                var enableAdminResponse = await client.SetGroupAdminAsync(GroupId, loginUserId, true);
                WriteResponse(enableAdminResponse);

                WriteTitle("SetGroupAdminAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, enable={2}", GroupId, loginUserId, false);
                var disableAdminResponse = await client.SetGroupAdminAsync(GroupId, loginUserId, false);
                WriteResponse(disableAdminResponse);

                // Enables and then disables anonymous group chat.
                // 启用群匿名聊天，然后关闭群匿名聊天。
                WriteTitle("SetGroupAnonymousAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, enable={1}", GroupId, true);
                var enableAnonymousResponse = await client.SetGroupAnonymousAsync(GroupId, true);
                WriteResponse(enableAnonymousResponse);

                WriteTitle("SetGroupAnonymousAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, enable={1}", GroupId, false);
                var disableAnonymousResponse = await client.SetGroupAnonymousAsync(GroupId, false);
                WriteResponse(disableAnonymousResponse);

                // Changes the current account's group card and then restores it.
                // 修改当前账号的群名片，然后恢复原值。
                WriteTitle("SetGroupCardAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, card={2}", GroupId, loginUserId, "OneBotSdk.Net HTTP测试");
                var setGroupCardResponse = await client.SetGroupCardAsync(GroupId, loginUserId, "OneBotSdk.Net HTTP测试");
                WriteResponse(setGroupCardResponse);

                WriteTitle("SetGroupCardAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, card={2}", GroupId, loginUserId, originalGroupCard);
                var restoreGroupCardResponse = await client.SetGroupCardAsync(GroupId, loginUserId, originalGroupCard);
                WriteResponse(restoreGroupCardResponse);

                // Changes the configured group's name and then restores it.
                // 修改配置群的名称，然后恢复原值。
                WriteTitle("SetGroupNameAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, groupName={1}", GroupId, "BNS查询测试-OneBotSdk.Net HTTP");
                var setGroupNameResponse = await client.SetGroupNameAsync(GroupId, "BNS查询测试-OneBotSdk.Net HTTP");
                WriteResponse(setGroupNameResponse);

                WriteTitle("SetGroupNameAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, groupName={1}", GroupId, originalGroupName);
                var restoreGroupNameResponse = await client.SetGroupNameAsync(GroupId, originalGroupName);
                WriteResponse(restoreGroupNameResponse);

                // Changes the current account's special title and then restores it.
                // 修改当前账号的群专属头衔，然后恢复原值。
                WriteTitle("SetGroupSpecialTitleAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, specialTitle={2}, duration={3}", GroupId, loginUserId, "HTTP测试", -1);
                var setSpecialTitleResponse = await client.SetGroupSpecialTitleAsync(GroupId, loginUserId, "HTTP测试", -1);
                WriteResponse(setSpecialTitleResponse);

                WriteTitle("SetGroupSpecialTitleAsync (restore)");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, specialTitle={2}, duration={3}", GroupId, loginUserId, originalSpecialTitle, -1);
                var restoreSpecialTitleResponse = await client.SetGroupSpecialTitleAsync(GroupId, loginUserId, originalSpecialTitle, -1);
                WriteResponse(restoreSpecialTitleResponse);

                // Processes a friend-request flag supplied as a visible test variable.
                // 处理作为可见测试变量提供的好友请求 flag。
                WriteTitle("SetFriendAddRequestAsync");
                Console.WriteLine("Request variables / 请求变量: flag={0}, approve={1}, remark={2}", InvalidFriendRequestFlag, false, "OneBotSdk.Net HTTP test");
                var friendRequestResponse = await client.SetFriendAddRequestAsync(InvalidFriendRequestFlag, false, "OneBotSdk.Net HTTP test");
                WriteResponse(friendRequestResponse);

                // Processes a group-request flag supplied as a visible test variable.
                // 处理作为可见测试变量提供的加群请求 flag。
                WriteTitle("SetGroupAddRequestAsync");
                Console.WriteLine("Request variables / 请求变量: flag={0}, requestType={1}, approve={2}, reason={3}", InvalidGroupRequestFlag, OneBot11GroupRequestType.Add, false, "OneBotSdk.Net HTTP test");
                var groupRequestResponse = await client.SetGroupAddRequestAsync(InvalidGroupRequestFlag, OneBot11GroupRequestType.Add, false, "OneBotSdk.Net HTTP test");
                WriteResponse(groupRequestResponse);

                // IMPORTANT: Do not kick the current login account automatically. If it is the group owner,
                // implementation-specific behavior may remove the account or affect the group unexpectedly.
                // 重要：不要自动踢出当前登录账号。如果该账号是群主，
                // 实现端的特有行为可能移除该账号或对群产生意外影响。
                // WriteTitle("SetGroupKickAsync (manual test only / 仅限手动测试)");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, rejectAddRequest={2}", GroupId, loginUserId, false);
                // var groupKickResponse = await client.SetGroupKickAsync(GroupId, loginUserId, false);
                // WriteResponse(groupKickResponse);

                // Cleans the implementation cache near the end of the example.
                // 在示例接近结束时清理实现端缓存。
                WriteTitle("CleanCacheAsync");
                Console.WriteLine("Request variables / 请求变量: none / 无");
                var cleanCacheResponse = await client.CleanCacheAsync();
                WriteResponse(cleanCacheResponse);

                // IMPORTANT: Do not execute this action automatically. A group owner may leave or dismiss the group,
                // and some OneBot implementations may still dismiss it even when isDismiss is false.
                // 重要：不要自动执行此动作。群主账号可能退出或解散群，
                // 并且部分 OneBot 实现即使 isDismiss 为 false 仍可能解散群。
                // WriteTitle("SetGroupLeaveAsync (manual test only / 仅限手动测试)");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, isDismiss={1}", GroupId, false);
                // var groupLeaveResponse = await client.SetGroupLeaveAsync(GroupId, false);
                // WriteResponse(groupLeaveResponse);

                // Restarts the implementation last because the HTTP endpoint may become temporarily unavailable.
                // 最后重启实现端，因为 HTTP 终结点可能暂时不可用。
                WriteTitle("SetRestartAsync");
                Console.WriteLine("Request variables / 请求变量: delay={0}", 0);
                var restartResponse = await client.SetRestartAsync(0);
                WriteResponse(restartResponse);
            }
        }

        /// <summary>
        /// Writes a readable heading before one action call.
        /// 在每个动作调用前输出清晰的标题。
        /// </summary>
        private static void WriteTitle(string methodName)
        {
            Console.WriteLine("========== {0} ==========", methodName);
        }

        /// <summary>
        /// Writes the request variables, response variables, and raw JSON for an untyped response.
        /// 输出非泛型响应的请求变量、返回变量和原始 JSON。
        /// </summary>
        private static void WriteResponse(OneBot11Response response)
        {
            WriteCommonResponse(response);
            Console.WriteLine("Data / 返回数据: {0}", OneBot11Json.Serialize(response.Data));
            Console.WriteLine("RawResponseJson / 返回报文: {0}", response.RawResponseJson);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes the request variables, typed data, raw data, and raw JSON for a typed response.
        /// 输出泛型响应的请求变量、强类型数据、原始数据和原始 JSON。
        /// </summary>
        private static void WriteResponse<TData>(OneBot11Response<TData> response)
        {
            WriteCommonResponse(response);
            Console.WriteLine("Data / 强类型返回数据: {0}", OneBot11Json.Serialize(response.Data));
            Console.WriteLine("RawData / 原始返回数据: {0}", OneBot11Json.Serialize(response.RawData));
            Console.WriteLine("RawResponseJson / 返回报文: {0}", response.RawResponseJson);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes fields shared by every OneBot action response.
        /// 输出每个 OneBot 动作响应共有的字段。
        /// </summary>
        private static void WriteCommonResponse(OneBot11ResponseBase response)
        {
            Console.WriteLine("Action / 动作: {0}", response.Action);
            Console.WriteLine("RequestParameters / 请求参数: {0}", OneBot11Json.Serialize(response.RequestParameters));
            Console.WriteLine("RequestEcho / 请求关联值: {0}", OneBot11Json.Serialize(response.RequestEcho));
            Console.WriteLine("RawRequestJson / 请求报文: {0}", response.RawRequestJson);
            Console.WriteLine("Status / 返回状态: {0}", response.Status);
            Console.WriteLine("RetCode / 返回码: {0}", response.RetCode);
            Console.WriteLine("IsSuccess / 是否成功: {0}", response.IsSuccess);
        }
    }
}
