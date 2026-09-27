using NUnit.Framework;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

/// <summary>Live Ollama diagnostic — run explicitly when investigating quality-gate failures.</summary>
[TestFixture]
public class Faction5DraftDiagnosticTests
{
    [Test]
    [Explicit("Calls local Ollama; ~4 min for 1 attempt (F smoke).")]
    public async Task CaptureFaction5SingleAttemptWithCatalog()
    {
        await CaptureAttemptsAsync(maxAttempts: 1);
    }

    [Test]
    [Explicit("Calls local Ollama; ~15+ min for 5 attempts.")]
    public async Task CaptureFaction5DraftAttempts()
    {
        await CaptureAttemptsAsync(maxAttempts: 5);
    }

    private static async Task CaptureAttemptsAsync(int maxAttempts)
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var settings = PlayerAgentSettings.Load(allowRunPodFlag: true);
        var factionId = 5;
        var runId = "beta-1";
        var factionDir = RepoPaths.FactionFolder(repoRoot, runId, factionId);
        var reportPath = Path.Combine(factionDir, "report.1.5.txt");
        var storyPath = Path.Combine(factionDir, "story.md");
        var personaPath = Path.Combine(factionDir, "persona.md");

        var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
        var rulesMarkdown = await File.ReadAllTextAsync(rulesPath);
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(rulesMarkdown);
        var reportText = await File.ReadAllTextAsync(reportPath);
        var objectiveText = await File.ReadAllTextAsync(storyPath);
        var personaText = await File.ReadAllTextAsync(personaPath);
        var password = FactionCredentials.TryReadPassword(factionDir, factionId);

        var hints = DraftPromptBuilder.BuildHints(objectiveText, reportText, personaText, draftTurn: 2);
        var retrievalQuery = DraftPromptBuilder.BuildRetrievalQuery(objectiveText, reportText, personaText);
        var verbBoost = VerbInference.InferBoostVerbs(personaText, objectiveText, reportText, retrievalQuery);

        var promptPack = PromptPackBuilder.BuildPromptPack(
            factionId,
            password,
            stripPassword: false,
            rulesMarkdown,
            reportText,
            objectiveText);

        using var client = new OllamaClient(settings);
        var service = new OrderDraftService(client, settings);

        var sharedPath = VectorIndexPaths.SharedSqlitePath(
            RepoPaths.SharedIndexDirectory(settings.IndexDirectory, PlayMode.Campaign));
        var factionPath = VectorIndexPaths.FactionSqlitePath(
            RepoPaths.FactionIndexDirectory(settings.IndexDirectory, runId, factionId));

        List<RetrievalResult> retrieved = [];
        using (var sharedStore = new SqliteVectorStore(sharedPath))
        {
            var emb = await client.EmbedAsync(retrievalQuery);
            retrieved.AddRange(VectorRetriever.Retrieve(sharedStore.ListAll(), emb, 3, verbBoost: verbBoost));
        }

        using (var factionStore = new SqliteVectorStore(factionPath))
        {
            var emb = await client.EmbedAsync(retrievalQuery);
            retrieved.AddRange(VectorRetriever.Retrieve(factionStore.ListAll(), emb, 3, verbBoost: verbBoost));
        }

        retrieved = retrieved.OrderByDescending(r => r.Score).Take(6).ToList();
        var ordersTemplate = DraftPromptBuilder.ExtractOrdersTemplate(reportText);
        var chatPrompt = DraftPromptBuilder.BuildChatPrompt(promptPack, retrieved, ordersTemplate, hints, reportText);
        var prompt = chatPrompt;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            TestContext.WriteLine($"========== Attempt {attempt} ==========");
            var generated = await client.ChatAsync(prompt, DraftPromptBuilder.SystemPrompt);
            TestContext.WriteLine("--- raw model output ---");
            TestContext.WriteLine(generated);
            var prepared = OrderDraftWriter.PrepareForWrite(generated, factionId, password);
            TestContext.WriteLine("--- prepared ---");
            TestContext.WriteLine(prepared);
            var lint = OrderDraftLinter.Lint(prepared, allowlist);
            TestContext.WriteLine($"lint valid: {lint.IsValid}");
            foreach (var e in lint.Errors)
            {
                TestContext.WriteLine($"  lint: {e}");
            }

            var pref = hints.PersonaPreference;
            TestContext.WriteLine($"IsUsable: {OrderDraftQuality.IsUsable(prepared, pref, reportText)}");
            DumpViolations(prepared, pref, reportText);

            if (lint.IsValid && OrderDraftQuality.IsUsable(prepared, pref, reportText))
            {
                Assert.Pass($"Attempt {attempt} passed.");
            }

            if (attempt == maxAttempts)
            {
                break;
            }

            prompt = chatPrompt + Environment.NewLine + Environment.NewLine
                + OrderDraftQuality.BuildRetryInstruction(pref, prepared, reportText);
        }

        Assert.Fail($"All {maxAttempts} attempt(s) failed (see TestContext output).");
    }

    private static void DumpViolations(string prepared, string? pref, string reportText)
    {
        foreach (var v in OrderDraftQuality.DescribeMoveReadinessViolations(prepared))
        {
            TestContext.WriteLine($"  move: {v}");
        }

        foreach (var v in OrderDraftQuality.DescribeUseTechPlacementViolations(prepared, reportText))
        {
            TestContext.WriteLine($"  use: {v}");
        }

        foreach (var v in OrderDraftQuality.DescribeInvalidItemTypeViolations(prepared))
        {
            TestContext.WriteLine($"  item: {v}");
        }

        foreach (var v in OrderDraftQuality.DescribeDeferredNestGetViolations(prepared))
        {
            TestContext.WriteLine($"  nest: {v}");
        }

        foreach (var v in OrderDraftQuality.DescribeFactoryPlusGetViolations(prepared, pref))
        {
            TestContext.WriteLine($"  plusget: {v}");
        }

        foreach (var v in OrderDraftQuality.DescribeSetHoldViolations(prepared))
        {
            TestContext.WriteLine($"  sethold: {v}");
        }
    }
}
