using System.Linq;
using System.Text.Json.Nodes;
using OneBotSdk.Net.V11;
using OneBotSdk.Net.V11.Responses;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class ProtocolResponseTests
{
    [Fact]
    public void ProtocolIdentity_IsExplicitlyV11()
    {
        Assert.Equal(11, OneBot11Protocol.MajorVersion);
        Assert.Equal("v11", OneBot11Protocol.Version);
    }

    [Theory]
    [InlineData("ok", 0, OneBot11ResponseStatus.Ok, true, false)]
    [InlineData("async", 1, OneBot11ResponseStatus.Async, false, true)]
    [InlineData("failed", 1404, OneBot11ResponseStatus.Failed, false, false)]
    [InlineData("implementation_extension", 9, OneBot11ResponseStatus.Unknown, false, false)]
    public void ResponseEnvelope_RecognizesProtocolStatusWithoutDiscardingRawValue(
        string status,
        long retCode,
        OneBot11ResponseStatus expected,
        bool isSuccess,
        bool isAsync)
    {
        var response = OneBot11Response.Parse(new JsonObject
        {
            ["status"] = status,
            ["retcode"] = retCode,
            ["data"] = null
        });

        Assert.Equal(status, response.Status);
        Assert.Equal(expected, response.StatusKind);
        Assert.Equal(isSuccess, response.IsSuccess);
        Assert.Equal(isAsync, response.IsAsync);
    }

    [Fact]
    public void ResponseEnvelope_IsolatesMalformedFieldsAndRetainsExtensions()
    {
        var response = OneBot11Response.Parse(new JsonObject
        {
            ["status"] = "ok",
            ["retcode"] = new JsonObject { ["invalid"] = true },
            ["data"] = new JsonObject
            {
                ["usable"] = 42,
                ["unserializable_data_extension"] = JsonValue.Create<object>(new CyclicExtension())
            },
            ["echo"] = new JsonArray(1, "two", false),
            ["implementation_field"] = "retained"
        });

        Assert.Equal("ok", response.Status);
        Assert.Null(response.RetCode);
        Assert.Equal(42, response.Data!["usable"]!.GetValue<int>());
        Assert.False(response.Data.AsObject().ContainsKey("unserializable_data_extension"));
        Assert.Equal("two", response.Echo![1]!.GetValue<string>());
        Assert.Equal("retained", response.RawJson["implementation_field"]!.GetValue<string>());
        Assert.Null(response.Action);
        Assert.Null(response.RequestParameters);
        Assert.Null(response.RequestEcho);
        Assert.Null(response.RawRequestJson);
        Assert.Null(response.RawResponseJson);
    }

    [Fact]
    public void ResponseEnvelope_ParsesValidFieldsEvenWhenAnExtensionCannotBeCloned()
    {
        var source = new JsonObject
        {
            ["status"] = "ok",
            ["retcode"] = "0",
            ["data"] = new JsonObject { ["usable"] = 42 },
            ["echo"] = "response-echo",
            ["unserializable_extension"] = JsonValue.Create<object>(new CyclicExtension())
        };

        var response = OneBot11Response.Parse(source);

        Assert.Equal("ok", response.Status);
        Assert.Equal(0L, response.RetCode);
        Assert.Equal(42, response.Data!["usable"]!.GetValue<int>());
        Assert.Equal("response-echo", response.Echo!.GetValue<string>());
        Assert.Equal("ok", response.RawJson["status"]!.GetValue<string>());
        Assert.False(response.RawJson.ContainsKey("unserializable_extension"));
    }

    [Fact]
    public void ProductionAssembly_DoesNotReferenceNewtonsoftJson()
    {
        var references = typeof(OneBot11Protocol).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(references, reference => reference.Name == "Newtonsoft.Json");
        Assert.Contains(references, reference => reference.Name == "System.Text.Json");
    }

    private sealed class CyclicExtension
    {
        public CyclicExtension Self => this;
    }
}
