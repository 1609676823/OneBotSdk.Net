using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V11.Json;
using OneBotSdk.Net.V11.Transports;
using OneBotSdk.Net.V11.Transports.Http;
using Xunit;

namespace OneBotSdk.Net.Tests;

[CollectionDefinition("Global JSON configuration", DisableParallelization = true)]
public sealed class JsonConfigurationCollection
{
    public const string Name = "Global JSON configuration";
}

[Collection(JsonConfigurationCollection.Name)]
public sealed class JsonTests
{
    [Fact]
    public void GlobalEncoder_UsesSafeDefaultAndExplicitUnsafeOptIn()
    {
        var previous = OneBot11Json.UseUnsafeRelaxedJsonEscaping;
        try
        {
            var value = new JsonObject { ["text"] = "<中文&>" };

            OneBot11Json.UseUnsafeRelaxedJsonEscaping = false;
            var safeOptions = OneBot11Json.CreateSerializerOptions();
            var safeJson = OneBot11Json.Serialize(value);

            OneBot11Json.UseUnsafeRelaxedJsonEscaping = true;
            var relaxedOptions = OneBot11Json.CreateSerializerOptions();
            var relaxedJson = OneBot11Json.Serialize(value);

            Assert.Same(JavaScriptEncoder.Default, safeOptions.Encoder);
            Assert.Same(JavaScriptEncoder.UnsafeRelaxedJsonEscaping, relaxedOptions.Encoder);
            Assert.DoesNotContain("<中文&>", safeJson);
            Assert.Contains("<中文&>", relaxedJson);
            Assert.Equal("<中文&>", OneBot11Json.Deserialize<JsonObject>(safeJson)!["text"]!.GetValue<string>());
            Assert.Equal("<中文&>", OneBot11Json.Parse(relaxedJson)!["text"]!.GetValue<string>());

            // Returned options belong to the caller and remain independently configurable.
            // 返回的选项由调用方拥有，并且仍可独立配置。
            safeOptions.WriteIndented = true;
            Assert.True(safeOptions.WriteIndented);
        }
        finally
        {
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = previous;
        }
    }

    [Fact]
    public async Task GlobalEncoder_RemainsThreadSafeWhileTheModeChanges()
    {
        var previous = OneBot11Json.UseUnsafeRelaxedJsonEscaping;
        try
        {
            var tasks = new Task[8];
            for (var worker = 0; worker < tasks.Length; worker++)
            {
                var workerNumber = worker;
                tasks[worker] = Task.Run(() =>
                {
                    for (var iteration = 0; iteration < 100; iteration++)
                    {
                        OneBot11Json.UseUnsafeRelaxedJsonEscaping = (workerNumber + iteration) % 2 == 0;
                        var json = OneBot11Json.Serialize(new JsonObject { ["text"] = "<并发&>" });
                        using (var document = JsonDocument.Parse(json))
                        {
                            Assert.Equal("<并发&>", document.RootElement.GetProperty("text").GetString());
                        }
                    }
                });
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = previous;
        }
    }

    [Fact]
    public async Task HttpActionTransport_UsesTheSelectedGlobalEncoderForWireJson()
    {
        var bodies = new List<string>();
        var handler = new StubHttpMessageHandler(async request =>
        {
            bodies.Add(await request.Content!.ReadAsStringAsync().ConfigureAwait(false));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\",\"retcode\":0,\"data\":null}")
            };
        });
        using var httpClient = new HttpClient(handler);
        using var transport = new OneBot11HttpActionTransport(
            new OneBot11HttpActionTransportOptions(new Uri("http://127.0.0.1:3000/")),
            httpClient);
        var parameters = new JsonObject { ["text"] = "<中文&>" };
        var previous = OneBot11Json.UseUnsafeRelaxedJsonEscaping;

        try
        {
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = false;
            await transport.SendAsync("test_safe", parameters, null, CancellationToken.None);

            OneBot11Json.UseUnsafeRelaxedJsonEscaping = true;
            await transport.SendAsync("test_relaxed", parameters, null, CancellationToken.None);
        }
        finally
        {
            OneBot11Json.UseUnsafeRelaxedJsonEscaping = previous;
        }

        Assert.Equal(2, bodies.Count);
        Assert.DoesNotContain("<中文&>", bodies[0]);
        Assert.Contains("<中文&>", bodies[1]);
        Assert.Equal("<中文&>", OneBot11Json.Parse(bodies[0])!["text"]!.GetValue<string>());
        Assert.Equal("<中文&>", OneBot11Json.Parse(bodies[1])!["text"]!.GetValue<string>());
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        internal StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _handler(request);
        }
    }
}
