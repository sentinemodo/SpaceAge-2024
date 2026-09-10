using NUnit.Framework;
using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderFileNamingTests
{
    [Test]
    public void TryParseReportFileName_ExtractsTurnAndFaction()
    {
        Assert.That(
            OrderFileNaming.TryParseReportFileName("report.1.2.txt", out var turn, out var faction),
            Is.True);
        Assert.That(turn, Is.EqualTo(1));
        Assert.That(faction, Is.EqualTo(2));
    }

    [Test]
    public void FormatFileName_UsesFactionTurnIteration()
    {
        Assert.That(
            OrderFileNaming.FormatFileName(factionId: 2, turn: 2, iteration: 1),
            Is.EqualTo("orders.2.2.1.txt"));
    }

    [Test]
    public void ResolveNextIteration_StartsAtOne()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            Assert.That(OrderFileNaming.ResolveNextIteration(tempDir, factionId: 2, turn: 2), Is.EqualTo(1));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public void ResolveNextIteration_IncrementsExisting()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "orders.2.2.1.txt"), "#end");
            File.WriteAllText(Path.Combine(tempDir, "orders.2.2.3.txt"), "#end");
            Assert.That(OrderFileNaming.ResolveNextIteration(tempDir, factionId: 2, turn: 2), Is.EqualTo(4));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public void ResolveActiveOrderPath_PicksHighestIteration()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "orders.2.2.1.txt"), "old");
            var latest = Path.Combine(tempDir, "orders.2.2.2.txt");
            File.WriteAllText(latest, "new");

            Assert.That(
                OrderFileNaming.ResolveActiveOrderPath(tempDir, factionId: 2, turn: 2),
                Is.EqualTo(latest));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public void InferDraftTurn_FromReportTurn_IsReportTurnPlusOne()
    {
        Assert.That(OrderFileNaming.InferDraftTurnFromReportTurn(1), Is.EqualTo(2));
    }
}
