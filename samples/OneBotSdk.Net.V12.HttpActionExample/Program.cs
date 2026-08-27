using System;
using System.Threading.Tasks;
using OneBotSdk.Net.V12;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Messages;
using OneBotSdk.Net.V12.Responses;
using OneBotSdk.Net.V12.Transports.Http;

namespace OneBotSdk.Net.V12.HttpActionExample
{
    /// <summary>
    /// Demonstrates all 31 standard OneBot 12 HTTP actions without opening an event connection.
    /// 演示全部 31 个 OneBot 12 标准 HTTP 动作，不建立事件连接。
    /// </summary>
    internal static class Program
    {
        private static readonly Uri ActionEndpoint = new Uri("http://127.0.0.1:3000/");
        private static readonly OneBot12Self BotSelf = new OneBot12Self("qq", "123xxxxxxx");

        private const string AccessToken = "123456";
        private const string UserId = "123xxxxxxx";
        private const string GroupId = "782351597";
        private const string GuildId = "replace-with-guild-id";
        private const string ChannelId = "replace-with-channel-id";
        private const string MessageId = "replace-with-message-id";
        private const string FileId = "replace-with-file-id";
        private const string SampleSha256 = "9417d9a3474a248147afdb1dd56c2e920754f84fc596622dcfa7b3a4f5f16ae4";

        private static async Task Main()
        {
            // WARNING: The server currently configured at this address implements OneBot 11.
            // OneBot 12 uses a different request envelope and cannot run directly against that server.
            // 警告：当前地址所配置的服务器实现的是 OneBot 11。
            // OneBot 12 使用不同的请求信封，不能直接在该服务器上运行本示例。
            Console.WriteLine("WARNING: the configured server is OneBot 11; this OneBot 12 example is not directly runnable.");
            Console.WriteLine("警告：当前配置的服务器是 OneBot 11；此 OneBot 12 示例不能直接兼容运行。");
            Console.WriteLine();

            // The SDK default is false; this example explicitly enables less restrictive JSON escaping.
            // SDK 默认值为 false；本示例显式启用限制更少的 JSON 转义方式。
            OneBot12Json.UseUnsafeRelaxedJsonEscaping = true;

            var transportOptions = new OneBot12HttpActionTransportOptions(ActionEndpoint)
            {
                AccessToken = AccessToken
            };

            using (var transport = new OneBot12HttpActionTransport(transportOptions))
            {
                // The default self identity is added only to non-meta actions.
                // 默认机器人身份只会添加到非元动作请求中。
                var client = new OneBot12Client(transport, BotSelf);

                Console.WriteLine("OneBot 12 HTTP endpoint / OneBot 12 HTTP 地址: {0}", ActionEndpoint);
                Console.WriteLine("Access token / 访问令牌: {0}", AccessToken);
                Console.WriteLine("Self / 机器人身份: platform={0}, userId={1}", BotSelf.Platform, BotSelf.UserId);
                Console.WriteLine("Test group / 测试群: {0}", GroupId);
                Console.WriteLine();

                // Polls buffered events through the standard HTTP meta action.
                // 通过标准 HTTP 元动作轮询缓冲事件。
                WriteTitle("GetLatestEventsAsync");
                Console.WriteLine("Request variables / 请求变量: limit={0}, timeoutSeconds={1}, echo={2}", 10, 0, "get_latest_events-example");
                var latestEventsResponse = await client.GetLatestEventsAsync(10, 0, "get_latest_events-example");
                WriteResponse(latestEventsResponse);

                // Gets the actions advertised by the implementation.
                // 获取实现端声明支持的动作。
                WriteTitle("GetSupportedActionsAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}", "get_supported_actions-example");
                var supportedActionsResponse = await client.GetSupportedActionsAsync("get_supported_actions-example");
                WriteResponse(supportedActionsResponse);

                // Gets implementation-wide and per-bot status.
                // 获取实现端整体状态和机器人状态。
                WriteTitle("GetStatusAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}", "get_status-example");
                var statusResponse = await client.GetStatusAsync("get_status-example");
                WriteResponse(statusResponse);

                // Gets implementation and protocol version information.
                // 获取实现端和协议版本信息。
                WriteTitle("GetVersionAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}", "get_version-example");
                var versionResponse = await client.GetVersionAsync("get_version-example");
                WriteResponse(versionResponse);

                // WARNING: Sending a message creates visible content in the configured group.
                // Keep this complete example commented until a OneBot 12 server and target are confirmed.
                // 警告：发送消息会在配置群中创建可见内容。
                // 确认 OneBot 12 服务器和目标之前，请保持下面的完整示例为注释状态。
                // WriteTitle("SendMessageAsync");
                // var sendMessage = new OneBot12SendMessage().Text("[OneBotSdk.Net V12 HTTP action example]");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, message={1}, echo={2}", GroupId, OneBot12Json.Serialize(sendMessage), "send_message-example");
                // var sendMessageResponse = await client.SendMessageAsync("group", sendMessage, groupId: GroupId, echo: "send_message-example");
                // WriteResponse(sendMessageResponse);

                // WARNING: Deleting a message recalls content and cannot be demonstrated as a read-only request.
                // Keep this complete example commented and replace MessageId only during an intentional manual test.
                // 警告：删除消息会撤回内容，无法作为只读请求演示。
                // 请保持下面的完整示例为注释状态，仅在有意手动测试时替换 MessageId。
                // WriteTitle("DeleteMessageAsync");
                // Console.WriteLine("Request variables / 请求变量: messageId={0}, echo={1}", MessageId, "delete_message-example");
                // var deleteMessageResponse = await client.DeleteMessageAsync(MessageId, "delete_message-example");
                // WriteResponse(deleteMessageResponse);

                // Gets the selected bot account information.
                // 获取所选机器人账号信息。
                WriteTitle("GetSelfInfoAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}, self={1}", "get_self_info-example", OneBot12Json.Serialize(BotSelf));
                var selfInfoResponse = await client.GetSelfInfoAsync("get_self_info-example");
                WriteResponse(selfInfoResponse);

                // Gets one user by the string identifier required by OneBot 12.
                // 使用 OneBot 12 要求的字符串标识获取一个用户。
                WriteTitle("GetUserInfoAsync");
                Console.WriteLine("Request variables / 请求变量: userId={0}, echo={1}", UserId, "get_user_info-example");
                var userInfoResponse = await client.GetUserInfoAsync(UserId, "get_user_info-example");
                WriteResponse(userInfoResponse);

                // Gets the friend or follower list.
                // 获取好友或关注者列表。
                WriteTitle("GetFriendListAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}", "get_friend_list-example");
                var friendListResponse = await client.GetFriendListAsync("get_friend_list-example");
                WriteResponse(friendListResponse);

                // Gets the configured single-level group.
                // 获取配置的单级群。
                WriteTitle("GetGroupInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, echo={1}", GroupId, "get_group_info-example");
                var groupInfoResponse = await client.GetGroupInfoAsync(GroupId, "get_group_info-example");
                WriteResponse(groupInfoResponse);

                // Gets all single-level groups joined by the selected bot.
                // 获取所选机器人加入的全部单级群。
                WriteTitle("GetGroupListAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}", "get_group_list-example");
                var groupListResponse = await client.GetGroupListAsync("get_group_list-example");
                WriteResponse(groupListResponse);

                // Gets one member from the configured group.
                // 获取配置群中的一个成员。
                WriteTitle("GetGroupMemberInfoAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, userId={1}, echo={2}", GroupId, UserId, "get_group_member_info-example");
                var groupMemberInfoResponse = await client.GetGroupMemberInfoAsync(GroupId, UserId, "get_group_member_info-example");
                WriteResponse(groupMemberInfoResponse);

                // Gets every member of the configured group.
                // 获取配置群的全部成员。
                WriteTitle("GetGroupMemberListAsync");
                Console.WriteLine("Request variables / 请求变量: groupId={0}, echo={1}", GroupId, "get_group_member_list-example");
                var groupMemberListResponse = await client.GetGroupMemberListAsync(GroupId, "get_group_member_list-example");
                WriteResponse(groupMemberListResponse);

                // WARNING: Renaming a group changes visible server state for every member.
                // Keep this complete example commented and restore the original name after any manual test.
                // 警告：修改群名称会改变所有成员可见的服务器状态。
                // 请保持下面的完整示例为注释状态，手动测试后应恢复原名称。
                // WriteTitle("SetGroupNameAsync");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, groupName={1}, echo={2}", GroupId, "OneBotSdk.Net V12 test", "set_group_name-example");
                // var setGroupNameResponse = await client.SetGroupNameAsync(GroupId, "OneBotSdk.Net V12 test", "set_group_name-example");
                // WriteResponse(setGroupNameResponse);

                // DANGER: Leaving a group changes membership and may be irreversible.
                // A platform or implementation may also dismiss a group owned by the bot. Never run this automatically.
                // 危险：退出群会改变成员关系，并且可能无法撤销。
                // 平台或实现端还可能解散机器人拥有的群。绝不要自动执行此调用。
                // WriteTitle("LeaveGroupAsync (manual test only / 仅限手动测试)");
                // Console.WriteLine("Request variables / 请求变量: groupId={0}, echo={1}", GroupId, "leave_group-example");
                // var leaveGroupResponse = await client.LeaveGroupAsync(GroupId, "leave_group-example");
                // WriteResponse(leaveGroupResponse);

                // Gets one two-level guild. Replace the visible placeholder before using a real OneBot 12 server.
                // 获取一个两级群组。在真实 OneBot 12 服务器上使用前请替换可见占位值。
                WriteTitle("GetGuildInfoAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, echo={1}", GuildId, "get_guild_info-example");
                var guildInfoResponse = await client.GetGuildInfoAsync(GuildId, "get_guild_info-example");
                WriteResponse(guildInfoResponse);

                // Gets all guilds joined by the selected bot.
                // 获取所选机器人加入的全部群组。
                WriteTitle("GetGuildListAsync");
                Console.WriteLine("Request variables / 请求变量: echo={0}", "get_guild_list-example");
                var guildListResponse = await client.GetGuildListAsync("get_guild_list-example");
                WriteResponse(guildListResponse);

                // WARNING: Renaming a guild changes visible server state.
                // Keep this complete example commented until an intentional manual test.
                // 警告：修改群组名称会改变可见的服务器状态。
                // 在有意手动测试之前，请保持下面的完整示例为注释状态。
                // WriteTitle("SetGuildNameAsync");
                // Console.WriteLine("Request variables / 请求变量: guildId={0}, guildName={1}, echo={2}", GuildId, "OneBotSdk.Net V12 test", "set_guild_name-example");
                // var setGuildNameResponse = await client.SetGuildNameAsync(GuildId, "OneBotSdk.Net V12 test", "set_guild_name-example");
                // WriteResponse(setGuildNameResponse);

                // Gets one guild member.
                // 获取一个群组成员。
                WriteTitle("GetGuildMemberInfoAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, userId={1}, echo={2}", GuildId, UserId, "get_guild_member_info-example");
                var guildMemberInfoResponse = await client.GetGuildMemberInfoAsync(GuildId, UserId, "get_guild_member_info-example");
                WriteResponse(guildMemberInfoResponse);

                // Gets every member in a guild.
                // 获取群组中的全部成员。
                WriteTitle("GetGuildMemberListAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, echo={1}", GuildId, "get_guild_member_list-example");
                var guildMemberListResponse = await client.GetGuildMemberListAsync(GuildId, "get_guild_member_list-example");
                WriteResponse(guildMemberListResponse);

                // DANGER: Leaving a guild changes membership and may be irreversible. Never run this automatically.
                // 危险：退出群组会改变成员关系，并且可能无法撤销。绝不要自动执行此调用。
                // WriteTitle("LeaveGuildAsync (manual test only / 仅限手动测试)");
                // Console.WriteLine("Request variables / 请求变量: guildId={0}, echo={1}", GuildId, "leave_guild-example");
                // var leaveGuildResponse = await client.LeaveGuildAsync(GuildId, "leave_guild-example");
                // WriteResponse(leaveGuildResponse);

                // Gets one channel in a guild.
                // 获取群组中的一个频道。
                WriteTitle("GetChannelInfoAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, channelId={1}, echo={2}", GuildId, ChannelId, "get_channel_info-example");
                var channelInfoResponse = await client.GetChannelInfoAsync(GuildId, ChannelId, "get_channel_info-example");
                WriteResponse(channelInfoResponse);

                // Gets channels visible to the selected bot.
                // 获取所选机器人可见的频道。
                WriteTitle("GetChannelListAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, joinedOnly={1}, echo={2}", GuildId, false, "get_channel_list-example");
                var channelListResponse = await client.GetChannelListAsync(GuildId, false, "get_channel_list-example");
                WriteResponse(channelListResponse);

                // WARNING: Renaming a channel changes visible server state.
                // Keep this complete example commented until an intentional manual test.
                // 警告：修改频道名称会改变可见的服务器状态。
                // 在有意手动测试之前，请保持下面的完整示例为注释状态。
                // WriteTitle("SetChannelNameAsync");
                // Console.WriteLine("Request variables / 请求变量: guildId={0}, channelId={1}, channelName={2}, echo={3}", GuildId, ChannelId, "OneBotSdk.Net V12 test", "set_channel_name-example");
                // var setChannelNameResponse = await client.SetChannelNameAsync(GuildId, ChannelId, "OneBotSdk.Net V12 test", "set_channel_name-example");
                // WriteResponse(setChannelNameResponse);

                // Gets one member in a channel.
                // 获取频道中的一个成员。
                WriteTitle("GetChannelMemberInfoAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, channelId={1}, userId={2}, echo={3}", GuildId, ChannelId, UserId, "get_channel_member_info-example");
                var channelMemberInfoResponse = await client.GetChannelMemberInfoAsync(GuildId, ChannelId, UserId, "get_channel_member_info-example");
                WriteResponse(channelMemberInfoResponse);

                // Gets every member in a channel.
                // 获取频道中的全部成员。
                WriteTitle("GetChannelMemberListAsync");
                Console.WriteLine("Request variables / 请求变量: guildId={0}, channelId={1}, echo={2}", GuildId, ChannelId, "get_channel_member_list-example");
                var channelMemberListResponse = await client.GetChannelMemberListAsync(GuildId, ChannelId, "get_channel_member_list-example");
                WriteResponse(channelMemberListResponse);

                // DANGER: Leaving a channel changes membership and may be irreversible. Never run this automatically.
                // 危险：退出频道会改变成员关系，并且可能无法撤销。绝不要自动执行此调用。
                // WriteTitle("LeaveChannelAsync (manual test only / 仅限手动测试)");
                // Console.WriteLine("Request variables / 请求变量: guildId={0}, channelId={1}, echo={2}", GuildId, ChannelId, "leave_channel-example");
                // var leaveChannelResponse = await client.LeaveChannelAsync(GuildId, ChannelId, "leave_channel-example");
                // WriteResponse(leaveChannelResponse);

                // WARNING: Uploading stores data on the implementation or platform and is not read-only.
                // Keep this complete inline-data example commented until an intentional manual test.
                // 警告：上传会在实现端或平台存储数据，并非只读操作。
                // 在有意手动测试之前，请保持下面的完整内联数据示例为注释状态。
                // WriteTitle("UploadFileAsync");
                // var uploadRequest = OneBot12UploadFileRequest.FromData("onebotsdk-net-v12-example.txt", new byte[] { 0x4f, 0x42, 0x31, 0x32 }, SampleSha256);
                // Console.WriteLine("Request variables / 请求变量: name={0}, type={1}, echo={2}", uploadRequest.Name, uploadRequest.Type, "upload_file-example");
                // var uploadFileResponse = await client.UploadFileAsync(uploadRequest, "upload_file-example");
                // WriteResponse(uploadFileResponse);

                // WARNING: Every fragmented-upload stage can create or modify stored server data.
                // Keep the complete prepare, transfer, and finish sequence commented.
                // 警告：分片上传的每个阶段都可能创建或修改服务器存储数据。
                // 请保持准备、传输和结束的完整序列为注释状态。
                // WriteTitle("UploadFileFragmentedAsync (prepare)");
                // Console.WriteLine("Request variables / 请求变量: name={0}, totalSize={1}, echo={2}", "onebotsdk-net-v12-example.bin", 4, "upload_file_fragmented-prepare-example");
                // var prepareUploadResponse = await client.UploadFileFragmentedAsync("onebotsdk-net-v12-example.bin", 4, "upload_file_fragmented-prepare-example");
                // WriteResponse(prepareUploadResponse);
                // var temporaryFileId = prepareUploadResponse.Data?.FileId ?? FileId;
                //
                // WriteTitle("UploadFileFragmentedAsync (transfer)");
                // Console.WriteLine("Request variables / 请求变量: fileId={0}, offset={1}, data={2}, echo={3}", temporaryFileId, 0, "T0IxMg==", "upload_file_fragmented-transfer-example");
                // var transferUploadResponse = await client.UploadFileFragmentedAsync(temporaryFileId, 0, new byte[] { 0x4f, 0x42, 0x31, 0x32 }, "upload_file_fragmented-transfer-example");
                // WriteResponse(transferUploadResponse);
                //
                // WriteTitle("UploadFileFragmentedAsync (finish)");
                // Console.WriteLine("Request variables / 请求变量: fileId={0}, sha256={1}, echo={2}", temporaryFileId, SampleSha256, "upload_file_fragmented-finish-example");
                // var finishUploadResponse = await client.UploadFileFragmentedAsync(temporaryFileId, SampleSha256, "upload_file_fragmented-finish-example");
                // WriteResponse(finishUploadResponse);

                // Gets a complete file as a URL. Replace FileId with an identifier returned by the server.
                // 以 URL 形式获取完整文件。请将 FileId 替换为服务器返回的标识。
                WriteTitle("GetFileAsync");
                Console.WriteLine("Request variables / 请求变量: fileId={0}, type={1}, echo={2}", FileId, OneBot12FileAccessType.Url, "get_file-example");
                var getFileResponse = await client.GetFileAsync(FileId, OneBot12FileAccessType.Url, "get_file-example");
                WriteResponse(getFileResponse);

                // Prepares a fragmented download and then requests the first 1024 bytes.
                // 准备分片下载，然后请求前 1024 字节。
                WriteTitle("GetFileFragmentedAsync (prepare)");
                Console.WriteLine("Request variables / 请求变量: fileId={0}, echo={1}", FileId, "get_file_fragmented-prepare-example");
                var prepareDownloadResponse = await client.GetFileFragmentedAsync(FileId, "get_file_fragmented-prepare-example");
                WriteResponse(prepareDownloadResponse);

                WriteTitle("GetFileFragmentedAsync (transfer)");
                Console.WriteLine("Request variables / 请求变量: fileId={0}, offset={1}, size={2}, echo={3}", FileId, 0, 1024, "get_file_fragmented-transfer-example");
                var downloadFragmentResponse = await client.GetFileFragmentedAsync(FileId, 0, 1024, "get_file_fragmented-transfer-example");
                WriteResponse(downloadFragmentResponse);
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
        /// Writes every requested trace field for a response whose Data is already raw JSON.
        /// 为 Data 本身已经是原始 JSON 的响应输出全部要求的追踪字段。
        /// </summary>
        private static void WriteResponse(OneBot12Response response)
        {
            WriteCommonResponse(response);
            Console.WriteLine("Data / 返回数据: {0}", OneBot12Json.Serialize(response.Data));

            // Untyped Data is already raw, so no duplicate RawData property exists on this response type.
            // 非泛型 Data 本身已经是原始数据，因此该响应类型不再提供重复的 RawData 属性。
            Console.WriteLine("RawData / 原始返回数据: not a separate property; Data is already raw / 非独立属性；Data 已是原始数据");
            Console.WriteLine("RawResponseJson / 返回报文: {0}", response.RawResponseJson);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes every requested trace field for a strongly typed response.
        /// 为强类型响应输出全部要求的追踪字段。
        /// </summary>
        private static void WriteResponse<TData>(OneBot12Response<TData> response)
        {
            WriteCommonResponse(response);
            Console.WriteLine("Data / 强类型返回数据: {0}", OneBot12Json.Serialize(response.Data));
            Console.WriteLine("RawData / 原始返回数据: {0}", OneBot12Json.Serialize(response.RawData));
            Console.WriteLine("RawResponseJson / 返回报文: {0}", response.RawResponseJson);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes fields shared by every OneBot 12 action response.
        /// 输出每个 OneBot 12 动作响应共有的字段。
        /// </summary>
        private static void WriteCommonResponse(OneBot12ResponseBase response)
        {
            Console.WriteLine("Action / 动作: {0}", response.Action);
            Console.WriteLine("RequestParameters / 请求参数: {0}", OneBot12Json.Serialize(response.RequestParameters));
            Console.WriteLine("RequestEcho / 请求关联值: {0}", OneBot12Json.Serialize(response.RequestEcho));
            Console.WriteLine("RequestSelf / 请求机器人身份: {0}", OneBot12Json.Serialize(response.RequestSelf));
            Console.WriteLine("RawRequestJson / 请求报文: {0}", response.RawRequestJson);
            Console.WriteLine("Status / 返回状态: {0}", response.Status);
            Console.WriteLine("RetCode / 返回码: {0}", response.RetCode);
            Console.WriteLine("IsSuccess / 是否成功: {0}", response.IsSuccess);
        }
    }
}
