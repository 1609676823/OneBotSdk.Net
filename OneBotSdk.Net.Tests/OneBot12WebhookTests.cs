using System;
using System.Text;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V12;
using OneBotSdk.Net.V12.Events;
using OneBotSdk.Net.V12.Json;
using OneBotSdk.Net.V12.Transports;
using OneBotSdk.Net.V12.Transports.Http;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class OneBot12WebhookTests
{
    [Fact]
    public void Ingress_ValidatesStandardHeadersAndDispatchesTypedEvent()
    {
        var dispatcher = new OneBot12EventDispatcher();
        GroupMessageEvent? delivered = null;
        dispatcher.GroupMessageReceived += (_, args) => delivered = args.Event;
        var ingress = new OneBot12HttpWebhookIngress(
            dispatcher,
            new OneBot12HttpWebhookIngressOptions
            {
                AccessToken = "secret"
            });
        var metadata = new OneBot12HttpWebhookIngressMetadata(
            "application/json; charset=utf-8",
            "OneBot/12 test",
            "12",
            "test-impl",
            "Bearer secret");
        var body = Encoding.UTF8.GetBytes(
            "{" +
            "\"id\":\"event-1\"," +
            "\"self\":{\"platform\":\"qq\",\"user_id\":\"bot-1\"}," +
            "\"time\":1700000000.25," +
            "\"type\":\"message\"," +
            "\"detail_type\":\"group\"," +
            "\"sub_type\":\"normal\"," +
            "\"message_id\":\"message-1\"," +
            "\"message\":[{\"type\":\"text\",\"data\":{\"text\":\"hello\"}}]," +
            "\"alt_message\":\"hello\"," +
            "\"user_id\":\"user-1\"," +
            "\"group_id\":\"group-1\"" +
            "}");

        var parsed = Assert.IsType<GroupMessageEvent>(
            ingress.ParseAndDispatch(body, metadata));

        Assert.Same(parsed, delivered);
        Assert.Equal("group-1", parsed.GroupId);
        Assert.Equal("hello", parsed.Message!.PlainText);
    }

    [Fact]
    public void Ingress_RejectsWrongHeaderEvenWhenQueryTokenMatches()
    {
        var ingress = new OneBot12HttpWebhookIngress(
            new OneBot12EventDispatcher(),
            new OneBot12HttpWebhookIngressOptions { AccessToken = "secret" });
        var metadata = new OneBot12HttpWebhookIngressMetadata(
            "application/json",
            "OneBot/12 test",
            "12",
            "test-impl",
            "Bearer wrong",
            "secret");

        var exception = Assert.Throws<OneBot12TransportException>(() =>
            ingress.ParseAndDispatch(Encoding.UTF8.GetBytes("{}"), metadata));

        Assert.Equal(OneBot12TransportError.AuthenticationFailed, exception.Error);
    }

    [Fact]
    public void WebhookResponse_WritesNoContentOrCompleteActionRequestArray()
    {
        var noContent = OneBot12HttpWebhookIngressResponse.NoContent();
        var actions = OneBot12HttpWebhookIngressResponse.FromActions(new[]
        {
            new OneBot12HttpWebhookIngressActionRequest(
                "send_message",
                new JsonObject
                {
                    ["detail_type"] = "private",
                    ["user_id"] = "user-1",
                    ["message"] = "hello"
                },
                "echo-1",
                new OneBot12Self("qq", "bot-1"))
        });

        Assert.Equal(204, noContent.StatusCode);
        Assert.Empty(noContent.Body);
        Assert.Null(noContent.ContentType);
        Assert.Equal(200, actions.StatusCode);
        Assert.Equal("application/json", actions.ContentType);

        var array = Assert.IsType<JsonArray>(
            OneBot12Json.Parse(Encoding.UTF8.GetString(actions.Body)));
        var action = Assert.IsType<JsonObject>(Assert.Single(array));
        Assert.Equal("send_message", action["action"]!.GetValue<string>());
        Assert.Equal("private", action["params"]!["detail_type"]!.GetValue<string>());
        Assert.Equal("echo-1", action["echo"]!.GetValue<string>());
        Assert.Equal("bot-1", action["self"]!["user_id"]!.GetValue<string>());
    }
}
