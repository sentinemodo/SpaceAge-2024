using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class IsolationAuditorTests
{
    private string _tempRoot = null!;
    private string _playerDir = null!;
    private string _indexRoot = null!;

    [SetUp]
    public void SetUp()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"player-agent-audit-{Guid.NewGuid():N}");
        _playerDir = Path.Combine(_tempRoot, "player");
        _indexRoot = Path.Combine(_tempRoot, ".data");
        Directory.CreateDirectory(_playerDir);
        Directory.CreateDirectory(_indexRoot);
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Test]
    public void AuditSharedIndex_CleanManualIndex_HasNoViolations()
    {
        var rulesPath = Path.Combine(_playerDir, "rules.md");
        File.WriteAllText(rulesPath, "## MOVE\nMove a stack.");
        var sqlitePath = Path.Combine(_indexRoot, "shared-test", "shared.sqlite");
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

        using (var store = new SqliteVectorStore(sqlitePath))
        {
            store.ReplaceSource(
                rulesPath,
                [
                    (
                        new TextChunk(
                            "Move a stack.",
                            new ChunkMetadata("rules", "MOVE", null, rulesPath, "MOVE")),
                        [1f, 0f]),
                ]);
        }

        var result = IsolationAuditor.AuditSharedIndex(
            sqlitePath,
            PlayMode.Test,
            _tempRoot,
            _playerDir);

        Assert.That(result.IsClean, Is.True);
    }

    [Test]
    public void AuditSharedIndex_ReportChunk_IsViolation()
    {
        var reportPath = Path.Combine(_tempRoot, "play", "runs", "demo", "factions", "02", "report.1.2.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllText(reportPath, "report body");
        var sqlitePath = Path.Combine(_indexRoot, "shared-campaign", "shared.sqlite");
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

        using (var store = new SqliteVectorStore(sqlitePath))
        {
            store.ReplaceSource(
                reportPath,
                [
                    (
                        new TextChunk(
                            "report body",
                            new ChunkMetadata("report", null, null, reportPath, "report-1")),
                        [1f, 0f]),
                ]);
        }

        var result = IsolationAuditor.AuditSharedIndex(
            sqlitePath,
            PlayMode.Campaign,
            _tempRoot,
            _playerDir);

        Assert.That(result.IsClean, Is.False);
        Assert.That(result.Violations, Has.Count.EqualTo(1));
        Assert.That(result.Violations[0].Doc, Is.EqualTo("report"));
    }

    [Test]
    public void AuditFactionIndex_SourceFromOtherSeat_IsViolation()
    {
        var runId = "demo";
        var faction2Dir = Path.Combine(_tempRoot, "play", "runs", runId, "factions", "02");
        var faction3Dir = Path.Combine(_tempRoot, "play", "runs", runId, "factions", "03");
        Directory.CreateDirectory(faction2Dir);
        Directory.CreateDirectory(faction3Dir);

        var foreignReport = Path.Combine(faction3Dir, "report.1.3.txt");
        File.WriteAllText(foreignReport, "other seat");
        var ownStory = Path.Combine(faction2Dir, "story.md");
        File.WriteAllText(ownStory, "objective");

        var sqlitePath = Path.Combine(_indexRoot, "runs", runId, "faction-02", "faction.sqlite");
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

        using (var store = new SqliteVectorStore(sqlitePath))
        {
            store.ReplaceSource(
                foreignReport,
                [
                    (
                        new TextChunk(
                            "other seat",
                            new ChunkMetadata("report", null, null, foreignReport, "report-1")),
                        [1f, 0f]),
                ]);
            store.ReplaceSource(
                ownStory,
                [
                    (
                        new TextChunk(
                            "objective",
                            new ChunkMetadata("story", null, null, ownStory, "objective")),
                        [1f, 0f]),
                ]);
        }

        var result = IsolationAuditor.AuditFactionIndex(
            sqlitePath,
            runId,
            factionId: 2,
            _tempRoot);

        Assert.That(result.IsClean, Is.False);
        Assert.That(result.Violations, Has.Count.EqualTo(1));
        Assert.That(result.Violations[0].SourcePath, Does.Contain("factions\\03").Or.Contain("factions/03"));
    }

    [Test]
    public void AuditFactionIndex_WrongFactionIdInReportName_IsViolation()
    {
        var runId = "demo";
        var faction2Dir = Path.Combine(_tempRoot, "play", "runs", runId, "factions", "02");
        Directory.CreateDirectory(faction2Dir);

        var mismatchedReport = Path.Combine(faction2Dir, "report.1.3.txt");
        File.WriteAllText(mismatchedReport, "wrong id");

        var sqlitePath = Path.Combine(_indexRoot, "runs", runId, "faction-02", "faction.sqlite");
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

        using (var store = new SqliteVectorStore(sqlitePath))
        {
            store.ReplaceSource(
                mismatchedReport,
                [
                    (
                        new TextChunk(
                            "wrong id",
                            new ChunkMetadata("report", null, null, mismatchedReport, "report-1")),
                        [1f, 0f]),
                ]);
        }

        var result = IsolationAuditor.AuditFactionIndex(
            sqlitePath,
            runId,
            factionId: 2,
            _tempRoot);

        Assert.That(result.IsClean, Is.False);
        Assert.That(result.Violations[0].Reason, Does.Contain("faction id"));
    }

    [Test]
    public void BuildAuditNote_IncludesCleanSummary()
    {
        var note = IsolationAuditor.BuildAuditNote(
            runId: "smoke-test",
            PlayMode.Campaign,
            fromFaction: 2,
            toFaction: 11,
            auditedAtUtc: "2026-09-10T16:00:00Z",
            result: new IsolationAuditResult([]));

        Assert.That(note, Does.Contain("smoke-test"));
        Assert.That(note, Does.Contain("clean"));
        Assert.That(note, Does.Contain("factions 2..11"));
    }
}
