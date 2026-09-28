using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public sealed class StagedOrderDraftService
{
    private readonly IOllamaClient _client;
    private readonly PlayerAgentSettings _settings;

    public StagedOrderDraftService(IOllamaClient client, PlayerAgentSettings settings)
    {
        _client = client;
        _settings = settings;
    }

    public async Task<OrderDraftResult> DraftAsync(OrderDraftRequest request, CancellationToken cancellationToken)
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
        var rulesMarkdown = await File.ReadAllTextAsync(rulesPath, cancellationToken);
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(rulesMarkdown);
        var reportText = await ReadOptionalTextAsync(request.ReportPath, cancellationToken);
        var objectiveText = await ReadOptionalTextAsync(request.StoryPath, cancellationToken);
        var personaText = await ReadPersonaAsync(request, cancellationToken);
        var password = request.FactionDir is null
            ? null
            : FactionCredentials.TryReadPassword(request.FactionDir, request.FactionId);

        var promptPack = PromptPackBuilder.BuildPromptPack(
            request.FactionId,
            password,
            _settings.IsRemoteHost,
            rulesMarkdown,
            reportText,
            objectiveText);

        var hints = DraftPromptBuilder.BuildHints(objectiveText, reportText, personaText, request.DraftTurn);
        if (request.DryRun)
        {
            var retrievalQueryDry = DraftPromptBuilder.BuildRetrievalQuery(objectiveText, reportText, personaText);
            var verbBoostDry = VerbInference.InferBoostVerbs(personaText, objectiveText, reportText, retrievalQueryDry);
            var retrievedDry = await OrderDraftRetrieval.RetrieveAsync(
                _client,
                _settings,
                request,
                retrievalQueryDry,
                verbBoostDry,
                hints.PersonaPreference,
                cancellationToken);
            var dryTemplate = DraftPromptBuilder.ExtractOrdersTemplate(reportText);
            var dryPrompt = StagedOrderPromptBuilder.BuildPass1UserPrompt(
                promptPack,
                retrievedDry,
                StagedOrderPromptBuilder.FilterOrdersTemplate(
                    dryTemplate,
                    reportText,
                    StagedOrderPromptBuilderBootstrapTypes()),
                hints,
                reportText);
            return new OrderDraftResult(
                StagedOrderPromptBuilder.Pass1SystemPrompt + Environment.NewLine + Environment.NewLine + dryPrompt,
                retrievedDry,
                verbBoostDry,
                generatedText: null,
                lintResult: null,
                outputPath: request.OutputPath);
        }

        var retrievalQuery = DraftPromptBuilder.BuildRetrievalQuery(objectiveText, reportText, personaText);
        var verbBoost = VerbInference.InferBoostVerbs(personaText, objectiveText, reportText, retrievalQuery);
        var retrieved = await OrderDraftRetrieval.RetrieveAsync(
            _client,
            _settings,
            request,
            retrievalQuery,
            verbBoost,
            hints.PersonaPreference,
            cancellationToken);

        var fullTemplate = DraftPromptBuilder.ExtractOrdersTemplate(reportText);
        var pass1Template = StagedOrderPromptBuilder.FilterOrdersTemplate(
            fullTemplate,
            reportText,
            StagedOrderPromptBuilderBootstrapTypes());
        var pass1Prompt = StagedOrderPromptBuilder.BuildPass1UserPrompt(
            promptPack,
            retrieved,
            pass1Template,
            hints,
            reportText);

        var pass1Raw = await ChatWithLintOnlyAsync(
            pass1Prompt,
            StagedOrderPromptBuilder.Pass1SystemPrompt,
            allowlist,
            request.FactionId,
            password,
            cancellationToken);
        var pass1Prepared = OrderDraftWriter.PrepareForWrite(pass1Raw, request.FactionId, password);

        var pass2Template = StagedOrderPromptBuilder.FilterOrdersTemplate(
            fullTemplate,
            reportText,
            ProductionTypes());
        var pass2Prompt = StagedOrderPromptBuilder.BuildPass2UserPrompt(
            promptPack,
            retrieved,
            pass2Template,
            hints,
            reportText,
            pass1Prepared);

        var pass2Raw = await ChatWithLintOnlyAsync(
            pass2Prompt,
            StagedOrderPromptBuilder.Pass2SystemPrompt,
            allowlist,
            request.FactionId,
            password,
            cancellationToken);
        var pass2Prepared = OrderDraftWriter.PrepareForWrite(pass2Raw, request.FactionId, password);

        var merged = OrderDraftMerger.MergeBootstrapAndProduction(pass1Prepared, pass2Prepared);
        var maxAttempts = request.MaxQualityAttempts;
        OrderDraftLintResult lintResult = new(false, []);
        string prepared = merged;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            lintResult = OrderDraftLinter.Lint(prepared, allowlist);
            if (lintResult.IsValid && OrderDraftQuality.IsUsable(prepared, hints.PersonaPreference, reportText))
            {
                break;
            }

            if (attempt == maxAttempts)
            {
                break;
            }

            var fixPrompt = pass2Prompt
                + Environment.NewLine
                + Environment.NewLine
                + "Fix the FULL merged order file below. Output the complete file from #faction through #end."
                + Environment.NewLine
                + OrderDraftQuality.BuildRetryInstruction(hints.PersonaPreference, prepared, reportText)
                + Environment.NewLine
                + prepared;

            var fixedRaw = await _client.ChatAsync(fixPrompt, DraftPromptBuilder.SystemPrompt, cancellationToken);
            prepared = OrderDraftWriter.PrepareForWrite(fixedRaw, request.FactionId, password);
        }

        if (!lintResult.IsValid)
        {
            throw new InvalidOperationException(
                "Staged draft failed verb allowlist lint:\n  - " + string.Join("\n  - ", lintResult.Errors));
        }

        if (!OrderDraftQuality.IsUsable(prepared, hints.PersonaPreference, reportText))
        {
            throw new InvalidOperationException(
                "Staged draft failed quality gate after " + maxAttempts + " merge fix attempts.");
        }

        await OrderDraftWriter.WriteUtf8Async(request.OutputPath, prepared, cancellationToken);
        return new OrderDraftResult(
            pass1Prompt,
            retrieved,
            verbBoost,
            prepared,
            lintResult,
            request.OutputPath);
    }

    private static HashSet<string> StagedOrderPromptBuilderBootstrapTypes() =>
        new(StringComparer.OrdinalIgnoreCase) { "corphq", "cargob", "sdrill", "farms", "cplant", "wnplnt" };

    private static HashSet<string> ProductionTypes() =>
        new(StringComparer.OrdinalIgnoreCase) { "factry" };

    private async Task<string> ChatWithLintOnlyAsync(
        string userPrompt,
        string systemPrompt,
        IReadOnlySet<string> allowlist,
        int factionId,
        string? password,
        CancellationToken cancellationToken)
    {
        const int passAttempts = 3;
        var prompt = userPrompt;
        for (var attempt = 1; attempt <= passAttempts; attempt++)
        {
            var generated = await _client.ChatAsync(prompt, systemPrompt, cancellationToken);
            var prepared = OrderDraftWriter.PrepareForWrite(generated, factionId, password);
            var lint = OrderDraftLinter.Lint(prepared, allowlist);
            if (lint.IsValid)
            {
                return generated;
            }

            if (attempt == passAttempts)
            {
                return generated;
            }

            prompt = userPrompt
                + Environment.NewLine
                + Environment.NewLine
                + "Lint errors:\n  - "
                + string.Join("\n  - ", lint.Errors);
        }

        return string.Empty;
    }

    private static async Task<string?> ReadOptionalTextAsync(string? path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    private static async Task<string?> ReadPersonaAsync(OrderDraftRequest request, CancellationToken cancellationToken)
    {
        var path = request.PersonaPath ?? (request.FactionDir is null ? null : Path.Combine(request.FactionDir, "persona.md"));
        return await ReadOptionalTextAsync(path, cancellationToken);
    }
}
