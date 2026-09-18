using Microsoft.Extensions.Configuration;

namespace SpaceAge.PlayerAgent.Configuration;

public sealed class PlayerAgentSettings
{
    public const string LocalDefaultChatModel = "qwen2.5-coder:7b";
    public const string RunPodDefaultChatModel = "qwen3-coder:30b";
    public const string DefaultEmbedModel = "nomic-embed-text";
    public const int DefaultChatTimeoutSeconds = 900;
    public const int LocalDefaultContextTokens = 8192;
    public const int RunPodDefaultContextTokens = 16384;
    public const int DefaultMaxOutputTokens = 4096;
    public const int LocalDefaultTopK = 6;
    public const int RunPodDefaultTopK = 8;

    public Uri OllamaBaseUri { get; set; } = new Uri("http://127.0.0.1:11434");
    public int ChatTimeoutSeconds { get; set; } = DefaultChatTimeoutSeconds;
    public string ChatModel { get; set; } = LocalDefaultChatModel;
    public int ChatContextTokens { get; set; } = LocalDefaultContextTokens;
    public int ChatMaxOutputTokens { get; set; } = DefaultMaxOutputTokens;
    public string EmbedModel { get; set; } = DefaultEmbedModel;
    public string IndexDirectory { get; set; } = DefaultIndexDirectory;
    public bool AllowRunPod { get; set; }
    public string? RunPodPodId { get; set; }
    public string? GpuClass { get; set; }
    public string? CloudTier { get; set; }
    public double HourlyRateUsd { get; set; } = DefaultRunPodHourlyRateUsd;

    public const double DefaultRunPodHourlyRateUsd = 0.44;

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
        var chatModel = ResolveChatModel(configuration["PLAYER_AGENT_CHAT_MODEL"], baseUri);

        var embedModel = configuration["PLAYER_AGENT_EMBED_MODEL"] ?? DefaultEmbedModel;
        var indexDir = configuration["PLAYER_AGENT_INDEX_DIR"] ?? DefaultIndexDirectory;
        var allowRunPod = allowRunPodFlag
            || string.Equals(configuration["PLAYER_AGENT_ALLOW_RUNPOD"], "1", StringComparison.Ordinal)
            || string.Equals(configuration["PLAYER_AGENT_ALLOW_RUNPOD"], "true", StringComparison.OrdinalIgnoreCase);

        var hourlyRate = ParseDouble(configuration["PLAYER_AGENT_RUNPOD_HOURLY_RATE_USD"])
            ?? DefaultRunPodHourlyRateUsd;
        var chatTimeoutSeconds = ParseInt(configuration["PLAYER_AGENT_CHAT_TIMEOUT_SECONDS"])
            ?? DefaultChatTimeoutSeconds;
        var defaultContextTokens = isRemote ? RunPodDefaultContextTokens : LocalDefaultContextTokens;
        var chatContextTokens = ParseInt(configuration["PLAYER_AGENT_CHAT_CONTEXT_TOKENS"])
            ?? defaultContextTokens;
        var chatMaxOutputTokens = ParseInt(configuration["PLAYER_AGENT_CHAT_MAX_OUTPUT_TOKENS"])
            ?? DefaultMaxOutputTokens;

        return new PlayerAgentSettings
        {
            OllamaBaseUri = baseUri,
            ChatModel = chatModel,
            ChatContextTokens = chatContextTokens,
            ChatMaxOutputTokens = chatMaxOutputTokens,
            EmbedModel = embedModel,
            IndexDirectory = Path.GetFullPath(indexDir),
            AllowRunPod = allowRunPod,
            RunPodPodId = configuration["PLAYER_AGENT_RUNPOD_POD_ID"],
            GpuClass = configuration["PLAYER_AGENT_GPU_CLASS"],
            CloudTier = configuration["PLAYER_AGENT_CLOUD_TIER"],
            HourlyRateUsd = hourlyRate,
            ChatTimeoutSeconds = chatTimeoutSeconds,
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

    public int DefaultTopK => IsRemoteHost ? RunPodDefaultTopK : LocalDefaultTopK;

    /// <summary>
    /// Picks chat model from env or host: unset / <c>auto</c> → local 7B, remote qwen3 30B.
    /// </summary>
    public static string ResolveChatModel(string? configured, Uri baseUri)
    {
        if (string.IsNullOrWhiteSpace(configured)
            || string.Equals(configured.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
        {
            return IsRemoteOllamaHost(baseUri)
                ? RunPodDefaultChatModel
                : LocalDefaultChatModel;
        }

        return configured.Trim();
    }

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

    private static double? ParseDouble(string? value) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
