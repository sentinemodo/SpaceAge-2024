using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class Beta1Turn1OrdersTests
{
    private const string RunId = "beta-1";
    private const int ReportTurn = 1;

    [Test]
    public void ActiveOrders_Factions4Through11_PassLintAndQuality()
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(File.ReadAllText(rulesPath));
        var failures = new List<string>();

        for (var factionId = 4; factionId <= 11; factionId++)
        {
            var folder = RepoPaths.FactionFolder(repoRoot, RunId, factionId);
            var orderPath = OrderFileNaming.ResolveActiveOrderPath(folder, factionId, ReportTurn);
            if (orderPath is null || !File.Exists(orderPath))
            {
                failures.Add($"faction {factionId}: no active orders.{factionId}.{ReportTurn}.*.txt");
                continue;
            }

            var personaPath = Path.Combine(folder, "persona.md");
            var persona = File.Exists(personaPath) ? File.ReadAllText(personaPath) : string.Empty;
            var pref = VerbInference.DetectPersonaPreference(persona);
            var reportPath = Path.Combine(folder, $"report.{ReportTurn}.{factionId}.txt");
            var reportText = File.Exists(reportPath) ? File.ReadAllText(reportPath) : null;
            var text = File.ReadAllText(orderPath);
            var lint = OrderDraftLinter.Lint(text, allowlist);
            var usable = OrderDraftQuality.IsUsable(text, pref, reportText);

            TestContext.WriteLine($"faction {factionId} ({pref}): {Path.GetFileName(orderPath)} lint={(lint.IsValid ? "ok" : "fail")} usable={(usable ? "ok" : "fail")}");
            if (!lint.IsValid)
            {
                foreach (var e in lint.Errors.Take(5))
                {
                    TestContext.WriteLine($"  lint: {e}");
                }
            }

            if (!usable)
            {
                foreach (var v in OrderDraftQuality.DescribeMoveReadinessViolations(text)
                             .Concat(OrderDraftQuality.DescribeUseTechPlacementViolations(text, reportText))
                             .Concat(OrderDraftQuality.DescribeInvalidItemTypeViolations(text))
                             .Concat(OrderDraftQuality.DescribeDeferredNestGetViolations(text))
                             .Take(8))
                {
                    TestContext.WriteLine($"  quality: {v}");
                }
            }

            if (!lint.IsValid || !usable)
            {
                failures.Add($"faction {factionId}: {Path.GetFileName(orderPath)}");
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join("; ", failures));
        }
    }
}
