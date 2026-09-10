using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class FactionCorpusPathsTests
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
    public void LatestReportPath_ReturnsHighestTurnReport()
    {
        File.WriteAllText(Path.Combine(_factionDir, "report.1.2.txt"), "turn 1");
        File.WriteAllText(Path.Combine(_factionDir, "report.2.2.txt"), "turn 2");

        var latest = FactionCorpusPaths.LatestReportPath(_factionDir);

        Assert.That(latest, Is.Not.Null);
        Assert.That(Path.GetFileName(latest), Is.EqualTo("report.2.2.txt"));
    }

    [Test]
    public void OrderPathsWithinTurnWindow_KeepsMostRecentTurnsOnly()
    {
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.1.1.txt"), "t1");
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.2.1.txt"), "t2");
        File.WriteAllText(Path.Combine(_factionDir, "orders.2.3.1.txt"), "t3");

        var paths = FactionCorpusPaths.OrderPathsWithinTurnWindow(_factionDir, maxOrderTurns: 2);

        Assert.That(paths.Select(Path.GetFileName), Is.EquivalentTo(new[]
        {
            "orders.2.2.1.txt",
            "orders.2.3.1.txt",
        }));
    }

    [Test]
    public void IsFactionCorpusFile_MatchesReportStoryAndOrders()
    {
        Assert.That(FactionCorpusPaths.IsFactionCorpusFile("report.1.2.txt"), Is.True);
        Assert.That(FactionCorpusPaths.IsFactionCorpusFile("story.md"), Is.True);
        Assert.That(FactionCorpusPaths.IsFactionCorpusFile("orders.2.2.1.txt"), Is.True);
        Assert.That(FactionCorpusPaths.IsFactionCorpusFile("persona.md"), Is.False);
    }
}
