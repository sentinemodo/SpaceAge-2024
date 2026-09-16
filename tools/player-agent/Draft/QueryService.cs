using System.Text;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public sealed record QueryRequest
{
    public required PlayMode Mode { get; init; }
    public required int FactionId { get; init; }
    public required string Query { get; init; }
    public string? RunId { get; init; }
    public string? ReportPath { get; init; }
    public string? StoryPath { get; init; }
    public string? PersonaPath { get; init; }
    public bool IncludeStory { get; init; }
    public int TopK { get; init; } = 8;
}

public sealed record QueryResult(string Query, string Output, IReadOnlyList<RetrievalResult> Retrieved);

public static class QueryPromptBuilder
{
    public const string SystemPrompt =
        "You are a SpaceAge PBEM campaign advisor. Use retrieved rule excerpts, the faction report, "
        + "and optional story context. Answer clearly in plain text. Do not invent catalog ids or passwords.";

    public static string BuildUserPrompt(
        string query,
        string? reportText,
        string? storyText,
        string? personaText,
        IReadOnlyList<RetrievalResult> retrieved)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(personaText))
        {
            builder.AppendLine("## Persona");
            builder.AppendLine(personaText.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(storyText))
        {
            builder.AppendLine("## Story");
            builder.AppendLine(storyText.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(reportText))
        {
            builder.AppendLine("## Report excerpt");
            builder.AppendLine(PromptPackBuilder.BuildReportExcerpt(reportText, maxCharacters: 4000));
            builder.AppendLine();
        }

        if (retrieved.Count > 0)
        {
            builder.AppendLine("## Retrieved reference");
            foreach (var result in retrieved)
            {
                var metadata = result.Chunk.Chunk.Metadata;
                builder.AppendLine(
                    $"### [{metadata.Doc}] {metadata.Heading ?? metadata.Verb ?? "chunk"} (score {result.Score:F2})");
                builder.AppendLine(result.Chunk.Chunk.Content.Trim());
                builder.AppendLine();
            }
        }

        builder.AppendLine("## Question");
        builder.AppendLine(query.Trim());
        return builder.ToString();
    }
}

public sealed class QueryService
{
    private readonly IOllamaClient _client;
    private readonly PlayerAgentSettings _settings;

    public QueryService(IOllamaClient client, PlayerAgentSettings settings)
    {
        _client = client;
        _settings = settings;
    }

    public async Task<QueryResult> QueryAsync(QueryRequest request, CancellationToken cancellationToken)
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var reportText = await ReadOptionalTextAsync(request.ReportPath, cancellationToken);
        var storyText = request.IncludeStory
            ? await ReadOptionalTextAsync(request.StoryPath, cancellationToken)
            : null;
        var personaText = await ReadOptionalTextAsync(request.PersonaPath, cancellationToken);
        var retrievalQuery = DraftPromptBuilder.BuildRetrievalQuery(
            storyText,
            reportText,
            personaText);
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            retrievalQuery = request.Query.Trim() + Environment.NewLine + retrievalQuery;
        }

        var verbBoost = VerbInference.InferBoostVerbs(personaText, storyText, reportText, retrievalQuery);
        var retrieved = await RetrieveAsync(repoRoot, request, retrievalQuery, verbBoost, cancellationToken);
        var prompt = QueryPromptBuilder.BuildUserPrompt(
            request.Query,
            reportText,
            storyText,
            personaText,
            retrieved);
        var output = await _client.ChatAsync(prompt, QueryPromptBuilder.SystemPrompt, cancellationToken);
        return new QueryResult(request.Query, output.Trim(), retrieved);
    }

    private async Task<IReadOnlyList<RetrievalResult>> RetrieveAsync(
        string repoRoot,
        QueryRequest request,
        string query,
        IReadOnlyList<string> verbBoost,
        CancellationToken cancellationToken)
    {
        var sharedPath = VectorIndexPaths.SharedSqlitePath(
            RepoPaths.SharedIndexDirectory(_settings.IndexDirectory, request.Mode));
        var results = new List<RetrievalResult>();
        var sharedTop = Math.Max(1, request.TopK / 2);

        if (File.Exists(sharedPath))
        {
            using var sharedStore = new SqliteVectorStore(sharedPath);
            var queryEmbedding = await _client.EmbedAsync(query, cancellationToken);
            results.AddRange(VectorRetriever.Retrieve(sharedStore.ListAll(), queryEmbedding, sharedTop, verbBoost: verbBoost));
        }

        if (request.RunId is not null)
        {
            var factionPath = VectorIndexPaths.FactionSqlitePath(
                RepoPaths.FactionIndexDirectory(_settings.IndexDirectory, request.RunId, request.FactionId));
            if (File.Exists(factionPath))
            {
                using var factionStore = new SqliteVectorStore(factionPath);
                var queryEmbedding = await _client.EmbedAsync(query, cancellationToken);
                var factionTop = Math.Max(1, request.TopK - results.Count);
                results.AddRange(VectorRetriever.Retrieve(factionStore.ListAll(), queryEmbedding, factionTop, verbBoost: verbBoost));
            }
        }

        return results
            .OrderByDescending(result => result.Score)
            .Take(request.TopK)
            .ToList();
    }

    private static async Task<string?> ReadOptionalTextAsync(string? path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }
}
