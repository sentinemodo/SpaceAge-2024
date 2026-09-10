using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public sealed class OrderDraftService
{
    private readonly OllamaClient _client;
    private readonly PlayerAgentSettings _settings;

    public OrderDraftService(OllamaClient client, PlayerAgentSettings settings)
    {
        _client = client;
        _settings = settings;
    }

    public async Task<OrderDraftResult> DraftAsync(OrderDraftRequest request, CancellationToken cancellationToken)
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
        if (!File.Exists(rulesPath))
        {
            throw new InvalidOperationException($"Rules manual not found: {rulesPath}");
        }

        var rulesMarkdown = await File.ReadAllTextAsync(rulesPath, cancellationToken);
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(rulesMarkdown);
        var reportText = await ReadOptionalTextAsync(request.ReportPath, cancellationToken);
        var objectiveText = await ReadOptionalTextAsync(request.StoryPath, cancellationToken);
        var password = request.FactionDir is null
            ? null
            : FactionCredentials.TryReadPassword(request.FactionDir, request.FactionId);

        var stripPassword = _settings.IsRemoteHost;
        var promptPack = PromptPackBuilder.BuildPromptPack(
            request.FactionId,
            password,
            stripPassword,
            rulesMarkdown,
            reportText,
            objectiveText);

        var retrievalQuery = DraftPromptBuilder.BuildRetrievalQuery(objectiveText, reportText);
        var verbFilter = VerbInference.InferFromText(objectiveText, reportText, retrievalQuery);
        var retrieved = await RetrieveAsync(
            repoRoot,
            request,
            retrievalQuery,
            verbFilter,
            cancellationToken);

        var ordersTemplate = DraftPromptBuilder.ExtractOrdersTemplate(reportText);
        var chatPrompt = DraftPromptBuilder.BuildChatPrompt(promptPack, retrieved, ordersTemplate);

        if (request.DryRun)
        {
            return new OrderDraftResult(
                DraftPromptBuilder.SystemPrompt + Environment.NewLine + Environment.NewLine + chatPrompt,
                retrieved,
                verbFilter,
                generatedText: null,
                lintResult: null,
                outputPath: request.OutputPath);
        }

        var generated = await _client.ChatAsync(
            chatPrompt,
            DraftPromptBuilder.SystemPrompt,
            cancellationToken);
        var prepared = OrderDraftWriter.PrepareForWrite(generated, request.FactionId, password);
        var lintResult = OrderDraftLinter.Lint(prepared, allowlist);
        if (!lintResult.IsValid)
        {
            throw new InvalidOperationException(
                "Draft failed verb allowlist lint:\n  - " + string.Join("\n  - ", lintResult.Errors));
        }

        await OrderDraftWriter.WriteUtf8Async(request.OutputPath, prepared, cancellationToken);
        return new OrderDraftResult(
            chatPrompt,
            retrieved,
            verbFilter,
            prepared,
            lintResult,
            request.OutputPath);
    }

    private async Task<IReadOnlyList<RetrievalResult>> RetrieveAsync(
        string repoRoot,
        OrderDraftRequest request,
        string query,
        string? verbFilter,
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
            results.AddRange(VectorRetriever.Retrieve(sharedStore.ListAll(), queryEmbedding, sharedTop, verbFilter));
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
                results.AddRange(VectorRetriever.Retrieve(factionStore.ListAll(), queryEmbedding, factionTop, verbFilter));
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

public sealed class OrderDraftRequest
{
    public required PlayMode Mode { get; init; }
    public required int FactionId { get; init; }
    public required string OutputPath { get; init; }
    public string? RunId { get; init; }
    public string? FactionDir { get; init; }
    public string? ReportPath { get; init; }
    public string? StoryPath { get; init; }
    public bool DryRun { get; init; }
    public int TopK { get; init; } = 6;
}

public sealed class OrderDraftResult
{
    public OrderDraftResult(
        string chatPrompt,
        IReadOnlyList<RetrievalResult> retrievedChunks,
        string? verbFilter,
        string? generatedText,
        OrderDraftLintResult? lintResult,
        string outputPath)
    {
        ChatPrompt = chatPrompt;
        RetrievedChunks = retrievedChunks;
        VerbFilter = verbFilter;
        GeneratedText = generatedText;
        LintResult = lintResult;
        OutputPath = outputPath;
    }

    public string ChatPrompt { get; }
    public IReadOnlyList<RetrievalResult> RetrievedChunks { get; }
    public string? VerbFilter { get; }
    public string? GeneratedText { get; }
    public OrderDraftLintResult? LintResult { get; }
    public string OutputPath { get; }
}
