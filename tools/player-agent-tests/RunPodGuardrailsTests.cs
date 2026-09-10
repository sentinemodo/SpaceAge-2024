using NUnit.Framework;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class RunPodGuardrailsTests
{
    private string _indexDir = null!;
    private UsageLedger _ledger = null!;
    private RunPodGuardrails _guardrails = null!;
    private FakeUserPrompt _prompt = null!;

    [SetUp]
    public void SetUp()
    {
        _indexDir = Path.Combine(Path.GetTempPath(), $"player-agent-guard-{Guid.NewGuid():N}");
        _ledger = new UsageLedger(_indexDir);
        _ledger.EnsureLayout();
        _guardrails = new RunPodGuardrails(() => DateTimeOffset.Parse("2026-09-10T15:00:00Z"));
        _prompt = new FakeUserPrompt();
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
    public void ValidateBeforeSessionStart_MissingBudget_RefusesRemote()
    {
        var settings = RemoteSettings();
        var budget = new RunPodBudgetSettings { HardBudgetUsd = null };

        var result = _guardrails.ValidateBeforeSessionStart(settings, budget, _ledger, assumeYes: false, _prompt, overrideFile: null);

        Assert.That(result.Allowed, Is.False);
        Assert.That(result.Message, Does.Contain("PLAYER_AGENT_BUDGET_USD"));
    }

    [Test]
    public void ValidateBeforeSessionStart_HardCapExceeded_Refuses()
    {
        var settings = RemoteSettings();
        var budget = Budget(hard: 1.00, soft: 0.80);
        _ledger.AppendCompletedSession(RemoteSession("past", cost: 1.05, started: "2026-09-05T10:00:00Z"));

        var result = _guardrails.ValidateBeforeSessionStart(settings, budget, _ledger, assumeYes: true, _prompt, overrideFile: null);

        Assert.That(result.Allowed, Is.False);
        Assert.That(result.Message, Does.Contain("hard budget"));
    }

    [Test]
    public void ValidateBeforeSessionStart_SoftCapRequiresConfirmation()
    {
        var settings = RemoteSettings();
        var budget = Budget(hard: 10.00, soft: 0.50, requireConfirm: true);
        _ledger.AppendCompletedSession(RemoteSession("past", cost: 0.55, started: "2026-09-05T10:00:00Z"));

        var denied = _guardrails.ValidateBeforeSessionStart(settings, budget, _ledger, assumeYes: false, _prompt, overrideFile: null);
        Assert.That(denied.Allowed, Is.False);
        Assert.That(_prompt.LastMessage, Does.Contain("Soft budget").IgnoreCase);

        _prompt.Reset();
        _prompt.NextConfirm = true;
        var allowed = _guardrails.ValidateBeforeSessionStart(settings, budget, _ledger, assumeYes: false, _prompt, overrideFile: null);
        Assert.That(allowed.Allowed, Is.True);
    }

    [Test]
    public void AssertBeforeInferenceCall_MaxChatCallsTripsGuardrail()
    {
        var settings = RemoteSettings();
        var budget = Budget(hard: 10, maxChatCalls: 2);
        var active = ActiveSession(chatCalls: 2);

        var violation = _guardrails.AssertBeforeInferenceCall(settings, budget, _ledger, active, isChatCall: true);

        Assert.That(violation, Is.Not.Null);
        Assert.That(violation!.StopReason, Is.EqualTo(UsageStopReason.Guardrail));
        Assert.That(violation.Message, Does.Contain("chat-call cap"));
    }

    [Test]
    public void AssertBeforeInferenceCall_IdleTimeoutTripsGuardrail()
    {
        var settings = RemoteSettings();
        var budget = Budget(hard: 10, idleMinutes: 10);
        var active = ActiveSession(lastActivity: "2026-09-10T14:40:00Z", chatCalls: 1);

        var violation = _guardrails.AssertBeforeInferenceCall(settings, budget, _ledger, active, isChatCall: true);

        Assert.That(violation, Is.Not.Null);
        Assert.That(violation!.StopReason, Is.EqualTo(UsageStopReason.IdleTimeout));
    }

    [Test]
    public void AssertBeforeInferenceCall_SessionWallClockTripsGuardrail()
    {
        var settings = RemoteSettings();
        var budget = Budget(hard: 10, maxSessionHours: 1);
        var active = ActiveSession(
            started: "2026-09-10T13:30:00Z",
            lastActivity: "2026-09-10T14:58:00Z",
            chatCalls: 1);

        var violation = _guardrails.AssertBeforeInferenceCall(settings, budget, _ledger, active, isChatCall: true);

        Assert.That(violation, Is.Not.Null);
        Assert.That(violation!.Message, Does.Contain("session wall clock"));
    }

    [Test]
    public void ValidateBeforeSessionStart_StaleActiveSession_Refuses()
    {
        var settings = RemoteSettings();
        var budget = Budget(hard: 10);
        _ledger.SaveActiveSession(ActiveSession());

        var result = _guardrails.ValidateBeforeSessionStart(settings, budget, _ledger, assumeYes: true, _prompt, overrideFile: null);

        Assert.That(result.Allowed, Is.False);
        Assert.That(result.Message, Does.Contain("usage stop"));
    }

    private static RunPodBudgetSettings Budget(
        double hard,
        double? soft = null,
        bool requireConfirm = false,
        double? maxSessionHours = null,
        int? maxChatCalls = null,
        int idleMinutes = 15) =>
        new()
        {
            HardBudgetUsd = hard,
            SoftBudgetUsd = soft ?? hard * 0.8,
            RequireConfirm = requireConfirm,
            MaxPodHoursPerSession = maxSessionHours,
            MaxChatCallsPerSession = maxChatCalls,
            IdleTimeoutMinutes = idleMinutes,
        };

    private static PlayerAgentSettings RemoteSettings() =>
        new()
        {
            OllamaBaseUri = new Uri("https://pod.example/"),
            AllowRunPod = true,
            GpuClass = "RTX 4090",
            HourlyRateUsd = 0.44,
        };

    private UsageActiveSession ActiveSession(
        string started = "2026-09-10T14:00:00Z",
        string? lastActivity = null,
        int chatCalls = 0) =>
        new()
        {
            SessionId = "sess-1",
            StartedAt = DateTimeOffset.Parse(started),
            LastActivityAt = DateTimeOffset.Parse(lastActivity ?? started),
            Host = "https://pod.example",
            IsRemote = true,
            HourlyRateUsd = 0.44,
            ChatCalls = chatCalls,
        };

    private static UsageSessionRecord RemoteSession(string id, double cost, string started) =>
        new(
            SessionId: id,
            StartedAt: DateTimeOffset.Parse(started),
            EndedAt: DateTimeOffset.Parse(started).AddMinutes(30),
            DurationSec: 1800,
            Host: "https://pod.example",
            IsRemote: true,
            PodId: "pod-1",
            GpuClass: "RTX 4090",
            CloudTier: "Secure",
            RunId: "demo",
            FactionIds: [2],
            ChatCalls: 1,
            EmbedCalls: 2,
            HourlyRateUsd: 0.44,
            EstimatedCostUsd: cost,
            StopReason: UsageStopReason.Complete,
            Command: "draft-run");

    private sealed class FakeUserPrompt : IUserPrompt
    {
        public bool NextConfirm { get; set; }
        public string? LastMessage { get; private set; }

        public bool Confirm(string message)
        {
            LastMessage = message;
            return NextConfirm;
        }

        public void Reset() => LastMessage = null;
    }
}
