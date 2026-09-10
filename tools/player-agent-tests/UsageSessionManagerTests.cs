using NUnit.Framework;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class UsageSessionManagerTests
{
    private string _indexDir = null!;
    private UsageLedger _ledger = null!;
    private UsageSessionManager _manager = null!;

    [SetUp]
    public void SetUp()
    {
        _indexDir = Path.Combine(Path.GetTempPath(), $"player-agent-usage-mgr-{Guid.NewGuid():N}");
        _ledger = new UsageLedger(_indexDir);
        _ledger.EnsureLayout();
        _manager = new UsageSessionManager(_ledger, () => DateTimeOffset.Parse("2026-09-10T14:00:00Z"));
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
    public void StartSession_CreatesActiveSessionWithMetadata()
    {
        var settings = RemoteSettings();

        var active = _manager.StartSession(
            settings,
            command: "draft-run",
            runId: "smoke-test",
            factionIds: [2, 3, 4]);

        Assert.That(active.SessionId, Is.Not.Empty);
        Assert.That(active.StartedAt, Is.EqualTo(DateTimeOffset.Parse("2026-09-10T14:00:00Z")));
        Assert.That(active.IsRemote, Is.True);
        Assert.That(active.RunId, Is.EqualTo("smoke-test"));
        Assert.That(_ledger.LoadActiveSession()?.SessionId, Is.EqualTo(active.SessionId));
    }

    [Test]
    public void StartSession_WhenActiveExists_Throws()
    {
        var settings = RemoteSettings();
        _manager.StartSession(settings, "draft", runId: null, factionIds: null);

        Assert.Throws<InvalidOperationException>(() =>
            _manager.StartSession(settings, "draft", runId: null, factionIds: null));
    }

    [Test]
    public void StopSession_ComputesDurationAndCost()
    {
        var settings = RemoteSettings();
        var active = _manager.StartSession(settings, "draft-run", "demo", [2]);
        active.ChatCalls = 2;
        active.EmbedCalls = 4;
        _ledger.SaveActiveSession(active);

        _manager = new UsageSessionManager(_ledger, () => DateTimeOffset.Parse("2026-09-10T14:30:00Z"));
        var record = _manager.StopSession(UsageStopReason.Complete);

        Assert.That(record.DurationSec, Is.EqualTo(30 * 60).Within(0.1));
        Assert.That(record.EstimatedCostUsd, Is.EqualTo(0.44 * 0.5).Within(0.001));
        Assert.That(record.ChatCalls, Is.EqualTo(2));
        Assert.That(record.EmbedCalls, Is.EqualTo(4));
        Assert.That(record.StopReason, Is.EqualTo(UsageStopReason.Complete));
        Assert.That(_ledger.LoadActiveSession(), Is.Null);
        Assert.That(_ledger.LoadAllSessions(), Has.Count.EqualTo(1));
    }

    [Test]
    public void RecordChatCall_IncrementsActiveSession()
    {
        var settings = RemoteSettings();
        _manager.StartSession(settings, "draft", runId: null, factionIds: [2]);

        _manager.RecordChatCall();
        _manager.RecordChatCall();
        _manager.RecordEmbedCall();

        var active = _ledger.LoadActiveSession();
        Assert.That(active!.ChatCalls, Is.EqualTo(2));
        Assert.That(active.EmbedCalls, Is.EqualTo(1));
    }

    private static PlayerAgentSettings RemoteSettings() =>
        new()
        {
            OllamaBaseUri = new Uri("https://pod.example/"),
            AllowRunPod = true,
            IndexDirectory = Path.GetTempPath(),
            RunPodPodId = "pod-abc",
            GpuClass = "RTX 4090",
            CloudTier = "Secure",
            HourlyRateUsd = 0.44,
        };
}
