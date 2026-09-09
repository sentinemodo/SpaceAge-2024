using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Inference;

public sealed class OllamaClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PlayerAgentSettings _settings;

    public OllamaClient(PlayerAgentSettings settings, HttpClient? httpClient = null)
    {
        _settings = settings;
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = settings.OpenAiBaseUri,
            Timeout = TimeSpan.FromMinutes(5),
        };
    }

    public async Task SmokeTestAsync(CancellationToken cancellationToken = default)
    {
        var chat = await ChatAsync(
            "Reply with exactly: ok",
            cancellationToken);

        if (string.IsNullOrWhiteSpace(chat))
        {
            throw new InvalidOperationException("Chat smoke test returned an empty response.");
        }

        var embedding = await EmbedAsync("MOVE order syntax", cancellationToken);
        if (embedding.Length == 0)
        {
            throw new InvalidOperationException("Embedding smoke test returned an empty vector.");
        }
    }

    public async Task<string> ChatAsync(string userPrompt, CancellationToken cancellationToken = default)
    {
        var payload = new ChatCompletionRequest
        {
            Model = _settings.ChatModel,
            Messages =
            [
                new ChatMessage { Role = "user", Content = userPrompt },
            ],
            Stream = false,
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Chat request failed ({(int)response.StatusCode}): {body}");
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(body);
        return completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
            ?? throw new InvalidOperationException("Chat response did not include message content.");
    }

    public async Task<float[]> EmbedAsync(string input, CancellationToken cancellationToken = default)
    {
        var payload = new EmbeddingRequest
        {
            Model = _settings.EmbedModel,
            Input = input,
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "embeddings",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Embedding request failed ({(int)response.StatusCode}): {body}");
        }

        var embedding = JsonSerializer.Deserialize<EmbeddingResponse>(body);
        return embedding?.Data?.FirstOrDefault()?.Embedding
            ?? throw new InvalidOperationException("Embedding response did not include vector data.");
    }

    public void Dispose() => _httpClient.Dispose();

    private sealed class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }
    }

    private sealed class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData>? Data { get; set; }
    }

    private sealed class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }
}
