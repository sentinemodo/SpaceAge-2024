using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public sealed class OrderDraftService
{
    private readonly IOllamaClient _client;
    private readonly PlayerAgentSettings _settings;

    public OrderDraftService(IOllamaClient client, PlayerAgentSettings settings)
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
        var personaText = await ReadPersonaTextAsync(request, cancellationToken);
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

        var hints = DraftPromptBuilder.BuildHints(objectiveText, reportText, personaText, request.DraftTurn);
        var retrievalQuery = DraftPromptBuilder.BuildRetrievalQuery(objectiveText, reportText, personaText);
        var verbBoost = VerbInference.InferBoostVerbs(personaText, objectiveText, reportText, retrievalQuery);
        var retrieved = await RetrieveAsync(
            repoRoot,
            request,
            retrievalQuery,
            verbBoost,
            cancellationToken);

        var ordersTemplate = DraftPromptBuilder.ExtractOrdersTemplate(reportText);
        var chatPrompt = DraftPromptBuilder.BuildChatPrompt(promptPack, retrieved, ordersTemplate, hints);

        if (request.DryRun)
        {
            return new OrderDraftResult(
                DraftPromptBuilder.SystemPrompt + Environment.NewLine + Environment.NewLine + chatPrompt,
                retrieved,
                verbBoost,
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

        if (lintResult.IsValid && !OrderDraftQuality.IsUsable(prepared, hints.PersonaPreference))
        {
            var retryPrompt = chatPrompt
                + Environment.NewLine
                + Environment.NewLine
                + OrderDraftQuality.BuildRetryInstruction(hints.PersonaPreference);
            generated = await _client.ChatAsync(
                retryPrompt,
                DraftPromptBuilder.SystemPrompt,
                cancellationToken);
            prepared = OrderDraftWriter.PrepareForWrite(generated, request.FactionId, password);
            lintResult = OrderDraftLinter.Lint(prepared, allowlist);
        }

        if (!lintResult.IsValid)
        {
            throw new InvalidOperationException(
                "Draft failed verb allowlist lint:\n  - " + string.Join("\n  - ", lintResult.Errors));
        }

        if (!OrderDraftQuality.IsUsable(prepared, hints.PersonaPreference))
        {
            throw new InvalidOperationException(
                "Draft failed quality gate: expected factory USE, economic leftovers, and moblab @move/@research for researcher seats.");
        }

        await OrderDraftWriter.WriteUtf8Async(request.OutputPath, prepared, cancellationToken);
        return new OrderDraftResult(
            chatPrompt,
            retrieved,
            verbBoost,
            prepared,
            lintResult,
            request.OutputPath);
    }

    private async Task<IReadOnlyList<RetrievalResult>> RetrieveAsync(
        string repoRoot,
        OrderDraftRequest request,
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

    private static async Task<string?> ReadPersonaTextAsync(OrderDraftRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.PersonaPath))
        {
            return await ReadOptionalTextAsync(request.PersonaPath, cancellationToken);
        }

        if (request.FactionDir is null)
        {
            return null;
        }

        return await ReadOptionalTextAsync(Path.Combine(request.FactionDir, "persona.md"), cancellationToken);
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
    public string? PersonaPath { get; init; }
    public bool DryRun { get; init; }
    public int TopK { get; init; } = 6;
    public int DraftTurn { get; init; } = 2;
}

public sealed class OrderDraftResult
{
    public OrderDraftResult(
        string chatPrompt,
        IReadOnlyList<RetrievalResult> retrievedChunks,
        IReadOnlyList<string> verbBoost,
        string? generatedText,
        OrderDraftLintResult? lintResult,
        string outputPath)
    {
        ChatPrompt = chatPrompt;
        RetrievedChunks = retrievedChunks;
        VerbBoost = verbBoost;
        GeneratedText = generatedText;
        LintResult = lintResult;
        OutputPath = outputPath;
    }

    public string ChatPrompt { get; }
    public IReadOnlyList<RetrievalResult> RetrievedChunks { get; }
    public IReadOnlyList<string> VerbBoost { get; }
    public string? GeneratedText { get; }
    public OrderDraftLintResult? LintResult { get; }
    public string OutputPath { get; }
}
