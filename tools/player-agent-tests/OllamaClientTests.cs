using System.Net;
using System.Text.Json;
using NUnit.Framework;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OllamaClientTests
{
    [Test]
    public async Task ChatAsync_SendsContextAndOutputLimits()
    {
        string? capturedBody = null;
        var handler = new CaptureHandler(body => capturedBody = body);
        var settings = new PlayerAgentSettings
        {
            OllamaBaseUri = new Uri("http://127.0.0.1:11434/"),
            ChatModel = "qwen2.5-coder:7b",
            ChatContextTokens = 8192,
            ChatMaxOutputTokens = 4096,
        };

        using var client = new OllamaClient(settings, new HttpClient(handler)
        {
            BaseAddress = settings.OpenAiBaseUri,
        });

        var reply = await client.ChatAsync("hello", systemPrompt: "system", CancellationToken.None);

        Assert.That(reply, Is.EqualTo("ok"));
        Assert.That(capturedBody, Is.Not.Null);
        using var document = JsonDocument.Parse(capturedBody!);
        var root = document.RootElement;
        Assert.That(root.GetProperty("max_tokens").GetInt32(), Is.EqualTo(4096));
        Assert.That(root.GetProperty("options").GetProperty("num_ctx").GetInt32(), Is.EqualTo(8192));
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        private readonly Action<string> _onBody;

        public CaptureHandler(Action<string> onBody) => _onBody = onBody;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _onBody(request.Content!.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult());
            var payload = """
                {
                  "choices": [
                    { "message": { "role": "assistant", "content": "ok" } }
                  ]
                }
                """;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload),
            });
        }
    }
}
