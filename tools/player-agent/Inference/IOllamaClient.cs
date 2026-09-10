namespace SpaceAge.PlayerAgent.Inference;

public interface IOllamaClient
{
    Task<string> ChatAsync(string userPrompt, string? systemPrompt, CancellationToken cancellationToken = default);

    Task<float[]> EmbedAsync(string input, CancellationToken cancellationToken = default);
}
