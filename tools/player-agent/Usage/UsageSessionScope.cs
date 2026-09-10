using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Usage;

public sealed class UsageSessionScope : IAsyncDisposable
{
    private readonly UsageSessionManager _manager;
    private readonly bool _ownsSession;
    private string _stopReason = UsageStopReason.Complete;
    private bool _stopped;

    private UsageSessionScope(UsageSessionManager manager, UsageActiveSession active, bool ownsSession)
    {
        _manager = manager;
        Active = active;
        _ownsSession = ownsSession;
    }

    public UsageActiveSession Active { get; }

    public static UsageSessionScope? BeginRemote(
        PlayerAgentSettings settings,
        UsageLedger ledger,
        UsageSessionManager manager,
        RunPodGuardrails guardrails,
        RunPodBudgetSettings budget,
        bool assumeYes,
        IUserPrompt prompt,
        string command,
        string? runId,
        IReadOnlyList<int>? factionIds,
        bool dryRun)
    {
        if (dryRun || !settings.IsRemoteHost)
        {
            return null;
        }

        var overrideFile = RunPodGuardrailOverride.TryLoad(settings.IndexDirectory, DateTimeOffset.UtcNow);
        var check = guardrails.ValidateBeforeSessionStart(
            settings,
            budget,
            ledger,
            assumeYes,
            prompt,
            overrideFile);
        if (!check.Allowed)
        {
            throw new InvalidOperationException(check.Message);
        }

        var active = manager.StartSession(settings, command, runId, factionIds);
        Console.WriteLine(
            $"Usage session started: {active.SessionId} ({active.Host}, "
            + $"{UsageReportFormatter.FormatUsd(active.HourlyRateUsd)}/hr)");
        return new UsageSessionScope(manager, active, ownsSession: true);
    }

    public GuardedOllamaClient CreateClient(
        PlayerAgentSettings settings,
        RunPodBudgetSettings budget,
        UsageLedger ledger,
        RunPodGuardrails guardrails,
        HttpClient? httpClient = null)
    {
        var inner = new OllamaClient(settings, httpClient);
        return new GuardedOllamaClient(
            inner,
            settings,
            budget,
            ledger,
            _manager,
            guardrails,
            this);
    }

    public void MarkCancelled() => _stopReason = UsageStopReason.Cancelled;

    public void MarkError() => _stopReason = UsageStopReason.Error;

    public void MarkGuardrail() => _stopReason = UsageStopReason.Guardrail;

    public void MarkStopReason(string stopReason) => _stopReason = stopReason;

    public UsageSessionRecord? CompletedRecord { get; private set; }

    public UsageSessionRecord? TryStop()
    {
        if (!_ownsSession || _stopped || _manager.GetActiveSession() is null)
        {
            return CompletedRecord;
        }

        _stopped = true;
        CompletedRecord = _manager.StopSession(_stopReason);
        return CompletedRecord;
    }

    public ValueTask DisposeAsync()
    {
        var record = TryStop();
        if (record is not null)
        {
            Console.WriteLine(UsageReportFormatter.FormatCostSummary(record));
        }

        return ValueTask.CompletedTask;
    }
}
