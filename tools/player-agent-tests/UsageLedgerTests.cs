using NUnit.Framework;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class UsageLedgerTests
{
    private string _indexDir = null!;

    [SetUp]
    public void SetUp()
    {
        _indexDir = Path.Combine(Path.GetTempPath(), $"player-agent-usage-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_indexDir))
        {
            Directory.Delete(_indexDir, recursive: true);
        }
    }

    [Test]
    public void AppendCompletedSession_WritesJsonLine()
    {
        var ledger = new UsageLedger(_indexDir);
        ledger.EnsureLayout();

        var record = SampleRecord("sess-1", started: "2026-09-10T10:00:00Z", ended: "2026-09-10T10:30:00Z");
        ledger.AppendCompletedSession(record);

        var lines = File.ReadAllLines(ledger.SessionsPath);
        Assert.That(lines, Has.Length.EqualTo(1));
        Assert.That(lines[0], Does.Contain("\"sessionId\":\"sess-1\""));
    }

    [Test]
    public void LoadAllSessions_ReturnsEveryAppendedRecord()
    {
        var ledger = new UsageLedger(_indexDir);
        ledger.EnsureLayout();
        ledger.AppendCompletedSession(SampleRecord("a", "2026-09-01T10:00:00Z", "2026-09-01T10:10:00Z"));
        ledger.AppendCompletedSession(SampleRecord("b", "2026-09-02T10:00:00Z", "2026-09-02T10:20:00Z"));

        var sessions = ledger.LoadAllSessions();

        Assert.That(sessions.Select(s => s.SessionId), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void ActiveSession_RoundTripsThroughSaveAndLoad()
    {
        var ledger = new UsageLedger(_indexDir);
        ledger.EnsureLayout();

        var startedAt = DateTimeOffset.Parse("2026-09-10T12:00:00Z");
        var active = new UsageActiveSession
        {
            SessionId = "active-1",
            StartedAt = startedAt,
            LastActivityAt = startedAt,
            Host = "https://pod.example",
            IsRemote = true,
            Command = "draft-run",
            RunId = "smoke-test",
            FactionIds = [2, 3],
            HourlyRateUsd = 0.44,
        };

        ledger.SaveActiveSession(active);
        var loaded = ledger.LoadActiveSession();

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.SessionId, Is.EqualTo("active-1"));
        Assert.That(loaded.RunId, Is.EqualTo("smoke-test"));
        Assert.That(loaded.FactionIds, Is.EqualTo(new[] { 2, 3 }));
    }

    [Test]
    public void ClearActiveSession_RemovesActiveFile()
    {
        var ledger = new UsageLedger(_indexDir);
        ledger.EnsureLayout();
        var now = DateTimeOffset.UtcNow;
        ledger.SaveActiveSession(new UsageActiveSession
        {
            SessionId = "x",
            StartedAt = now,
            LastActivityAt = now,
            Host = "local",
        });

        ledger.ClearActiveSession();

        Assert.That(ledger.LoadActiveSession(), Is.Null);
        Assert.That(File.Exists(ledger.ActiveSessionPath), Is.False);
    }

    private static UsageSessionRecord SampleRecord(string id, string started, string ended) =>
        new(
            SessionId: id,
            StartedAt: DateTimeOffset.Parse(started),
            EndedAt: DateTimeOffset.Parse(ended),
            DurationSec: 600,
            Host: "https://pod.example",
            IsRemote: true,
            PodId: "pod-1",
            GpuClass: "RTX 4090",
            CloudTier: "Secure",
            RunId: "demo",
            FactionIds: [2],
            ChatCalls: 3,
            EmbedCalls: 6,
            HourlyRateUsd: 0.44,
            EstimatedCostUsd: 0.07,
            StopReason: UsageStopReason.Complete,
            Command: "draft-run");
}
