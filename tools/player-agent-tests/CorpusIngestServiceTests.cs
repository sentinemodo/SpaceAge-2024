using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class FactionIngestPlannerTests
{
    private string _factionDir = null!;

    [SetUp]
    public void SetUp()
    {
        _factionDir = Path.Combine(Path.GetTempPath(), $"player-agent-faction-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_factionDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_factionDir))
        {
            Directory.Delete(_factionDir, recursive: true);
        }
    }

    [Test]
    public void BuildPlan_Incremental_UsesLatestReportAndPrunesOlderReports()
    {
        File.WriteAllText(Path.Combine(_factionDir, "report.1.2.txt"), "turn 1");
        File.WriteAllText(Path.Combine(_factionDir, "report.2.2.txt"), "turn 2");
        File.WriteAllText(Path.Combine(_factionDir, "story.md"), "objective");

        var plan = FactionIngestPlanner.BuildPlan(
            _factionDir,
            new FactionIngestOptions { MaxOrderTurns = 2 });

        Assert.That(plan.IngestPaths.Select(Path.GetFileName), Is.EquivalentTo(new[]
        {
            "report.2.2.txt",
            "story.md",
        }));

        var stale = FactionIngestPlanner.FindStaleSources(
            [
                SourcePathNormalizer.Normalize(Path.Combine(_factionDir, "report.1.2.txt")),
                SourcePathNormalizer.Normalize(Path.Combine(_factionDir, "report.2.2.txt")),
            ],
            _factionDir,
            plan.KeepSourcePaths);

        Assert.That(stale, Has.Count.EqualTo(1));
        Assert.That(Path.GetFileName(stale[0]), Is.EqualTo("report.1.2.txt"));
    }

    [Test]
    public void BuildPlan_StoryOnly_IngestsStoryWithoutReportOrOrders()
    {
        File.WriteAllText(Path.Combine(_factionDir, "report.1.2.txt"), "turn 1");
        File.WriteAllText(Path.Combine(_factionDir, "story.md"), "objective");
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.2.1.txt"), "orders");

        var plan = FactionIngestPlanner.BuildPlan(
            _factionDir,
            new FactionIngestOptions { StoryOnly = true });

        Assert.That(plan.IngestPaths.Select(Path.GetFileName), Is.EquivalentTo(new[] { "story.md" }));
        Assert.That(plan.PruneIndexedFactionCorpus, Is.False);
    }

    [Test]
    public void BuildPlan_Full_IngestsEveryReportAndOrder()
    {
        File.WriteAllText(Path.Combine(_factionDir, "report.1.2.txt"), "turn 1");
        File.WriteAllText(Path.Combine(_factionDir, "report.2.2.txt"), "turn 2");
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.1.1.txt"), "t1");
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.2.1.txt"), "t2");
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.3.1.txt"), "t3");

        var plan = FactionIngestPlanner.BuildPlan(
            _factionDir,
            new FactionIngestOptions { FullCorpus = true });

        Assert.That(plan.IngestPaths.Select(Path.GetFileName), Is.EquivalentTo(new[]
        {
            "report.1.2.txt",
            "report.2.2.txt",
            "orders.2.1.1.txt",
            "orders.2.2.1.txt",
            "orders.2.3.1.txt",
        }));
    }
}
