using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class ValidateCampaignOrdersOnce
{
    private const string RunId = "campaign-2026-09-14-northwind";
    private const int DraftTurn = 2;

    [Test]
    public void ValidateAllFactionOrders()
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(File.ReadAllText(rulesPath));
        var failures = new List<string>();

        for (var factionId = 2; factionId <= 11; factionId++)
        {
            var folder = Path.Combine(repoRoot, "play", "runs", RunId, "factions", factionId.ToString("D2"));
            var orderPath = OrderFileNaming.ResolveActiveOrderPath(folder, factionId, DraftTurn)
                ?? OrderFileNaming.ResolveActiveOrderPath(folder, factionId, DraftTurn - 1)
                ?? Path.Combine(folder, $"order.{factionId}.txt");

            if (!File.Exists(orderPath))
            {
                failures.Add($"faction {factionId}: missing order file");
                continue;
            }

            var personaPath = Path.Combine(folder, "persona.md");
            var persona = File.Exists(personaPath) ? File.ReadAllText(personaPath) : null;
            var pref = VerbInference.DetectPersonaPreference(persona ?? string.Empty);
            var reportPath = Path.Combine(folder, $"report.1.{factionId}.txt");
            var reportText = File.Exists(reportPath) ? File.ReadAllText(reportPath) : null;
            var text = File.ReadAllText(orderPath);
            var lint = OrderDraftLinter.Lint(text, allowlist);
            var moveReady = !OrderDraftQuality.HasMoveReadinessViolations(text);
            var usePlaced = !OrderDraftQuality.HasUseTechPlacementViolations(text, reportText);
            var usable = OrderDraftQuality.IsUsable(text, pref, reportText);

            TestContext.WriteLine($"=== Faction {factionId} ({pref}) ===");
            TestContext.WriteLine($"  file: {orderPath}");
            TestContext.WriteLine($"  lint: {(lint.IsValid ? "PASS" : "FAIL")}");
            TestContext.WriteLine($"  move-ready: {(moveReady ? "PASS" : "FAIL")}");
            TestContext.WriteLine($"  use-placed: {(usePlaced ? "PASS" : "FAIL")}");
            TestContext.WriteLine($"  usable: {(usable ? "PASS" : "FAIL")}");
            if (!lint.IsValid)
            {
                foreach (var err in lint.Errors)
                {
                    TestContext.WriteLine($"    lint: {err}");
                }
            }

            if (!moveReady)
            {
                foreach (var err in OrderDraftQuality.DescribeMoveReadinessViolations(text))
                {
                    TestContext.WriteLine($"    move: {err}");
                }
            }

            if (!usePlaced)
            {
                foreach (var err in OrderDraftQuality.DescribeUseTechPlacementViolations(text, reportText))
                {
                    TestContext.WriteLine($"    use: {err}");
                }
            }

            if (!lint.IsValid || !usable || !moveReady || !usePlaced)
            {
                failures.Add($"faction {factionId}: lint={(lint.IsValid ? "ok" : "fail")} usable={(usable ? "ok" : "fail")} move={(moveReady ? "ok" : "fail")} use={(usePlaced ? "ok" : "fail")}");
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join("; ", failures));
        }
    }
}
