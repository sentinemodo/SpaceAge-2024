using Microsoft.Extensions.Configuration;

namespace SpaceAge.PlayerAgent.Configuration;

public sealed class PlayerAgentSettings
{
    public const string LocalDefaultChatModel = "qwen2.5-coder:7b";
    public const string LocalSmokeChatModel = "smollm2";
    public const string RunPodDefaultChatModel = "qwen2.5-coder:14b";
    public const string DefaultEmbedModel = "nomic-embed-text";

    public Uri OllamaBaseUri { get; set; } = new Uri("http://127.0.0.1:11434");
    public string ChatModel { get; set; } = LocalDefaultChatModel;
    public string EmbedModel { get; set; } = DefaultEmbedModel;
    public string IndexDirectory { get; set; } = DefaultIndexDirectory;
    public bool AllowRunPod { get; set; }

    public static string DefaultIndexDirectory =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".data"));

    public static PlayerAgentSettings Load(bool allowRunPodFlag)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var host = configuration["OLLAMA_HOST"]
            ?? configuration["PLAYER_AGENT_INFERENCE_BASE_URL"]
            ?? "http://127.0.0.1:11434";

        if (!host.EndsWith('/'))
        {
            host += "/";
        }

        if (!Uri.TryCreate(host, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException($"Invalid OLLAMA_HOST / PLAYER_AGENT_INFERENCE_BASE_URL: {host}");
        }

        var isRemote = IsRemoteOllamaHost(baseUri);
        var chatModel = configuration["PLAYER_AGENT_CHAT_MODEL"]
            ?? (isRemote ? RunPodDefaultChatModel : LocalDefaultChatModel);

        var embedModel = configuration["PLAYER_AGENT_EMBED_MODEL"] ?? DefaultEmbedModel;
        var indexDir = configuration["PLAYER_AGENT_INDEX_DIR"] ?? DefaultIndexDirectory;
        var allowRunPod = allowRunPodFlag
            || string.Equals(configuration["PLAYER_AGENT_ALLOW_RUNPOD"], "1", StringComparison.Ordinal)
            || string.Equals(configuration["PLAYER_AGENT_ALLOW_RUNPOD"], "true", StringComparison.OrdinalIgnoreCase);

        return new PlayerAgentSettings
        {
            OllamaBaseUri = baseUri,
            ChatModel = chatModel,
            EmbedModel = embedModel,
            IndexDirectory = Path.GetFullPath(indexDir),
            AllowRunPod = allowRunPod,
        };
    }

    public Uri OpenAiBaseUri
    {
        get
        {
            var builder = new UriBuilder(OllamaBaseUri)
            {
                Path = CombinePath(OllamaBaseUri.AbsolutePath, "v1"),
            };
            var uri = builder.Uri;
            var absolute = uri.AbsoluteUri;
            return absolute.EndsWith('/') ? uri : new Uri(absolute + "/");
        }
    }

    public bool IsRemoteHost => IsRemoteOllamaHost(OllamaBaseUri);

    public static bool IsRemoteOllamaHost(Uri baseUri)
    {
        if (!baseUri.IsAbsoluteUri)
        {
            return true;
        }

        if (!string.Equals(baseUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var host = baseUri.Host;
        return !string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            && host != "::1";
    }

    private static string CombinePath(string basePath, string segment)
    {
        var trimmed = basePath.TrimEnd('/');
        return $"{trimmed}/{segment.TrimStart('/')}";
    }
}
