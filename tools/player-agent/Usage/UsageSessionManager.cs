using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Usage;

public sealed class UsageSessionManager
{
    private readonly UsageLedger _ledger;
    private readonly Func<DateTimeOffset> _clock;

    public UsageSessionManager(UsageLedger ledger, Func<DateTimeOffset>? clock = null)
    {
        _ledger = ledger;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public UsageActiveSession? GetActiveSession() => _ledger.LoadActiveSession();

    public UsageActiveSession StartSession(
        PlayerAgentSettings settings,
        string command,
        string? runId,
        IReadOnlyList<int>? factionIds)
    {
        if (_ledger.LoadActiveSession() is not null)
        {
            throw new InvalidOperationException(
                "A usage session is already active. Run 'usage stop' or wait for the current batch to finish.");
        }

        var startedAt = _clock();
        var active = new UsageActiveSession
        {
            SessionId = Guid.NewGuid().ToString("N")[..12],
            StartedAt = startedAt,
            LastActivityAt = startedAt,
            Host = settings.OllamaBaseUri.ToString().TrimEnd('/'),
            IsRemote = settings.IsRemoteHost,
            PodId = settings.RunPodPodId,
            GpuClass = settings.GpuClass,
            CloudTier = settings.CloudTier,
            Command = command,
            RunId = runId,
            FactionIds = factionIds ?? [],
            HourlyRateUsd = settings.HourlyRateUsd,
        };

        _ledger.SaveActiveSession(active);
        return active;
    }

    public UsageSessionRecord StopSession(string stopReason)
    {
        var active = _ledger.LoadActiveSession()
            ?? throw new InvalidOperationException("No active usage session to stop.");

        var endedAt = _clock();
        var durationSec = Math.Max(0, (endedAt - active.StartedAt).TotalSeconds);
        var estimatedCost = active.IsRemote
            ? durationSec / 3600d * active.HourlyRateUsd
            : 0d;

        var record = new UsageSessionRecord(
            SessionId: active.SessionId,
            StartedAt: active.StartedAt,
            EndedAt: endedAt,
            DurationSec: durationSec,
            Host: active.Host,
            IsRemote: active.IsRemote,
            PodId: active.PodId,
            GpuClass: active.GpuClass,
            CloudTier: active.CloudTier,
            RunId: active.RunId,
            FactionIds: active.FactionIds,
            ChatCalls: active.ChatCalls,
            EmbedCalls: active.EmbedCalls,
            HourlyRateUsd: active.HourlyRateUsd,
            EstimatedCostUsd: estimatedCost,
            StopReason: stopReason,
            Command: active.Command);

        _ledger.AppendCompletedSession(record);
        _ledger.ClearActiveSession();
        return record;
    }

    public void RecordChatCall() => RecordCall(isChat: true);

    public void RecordEmbedCall() => RecordCall(isChat: false);

    public void TouchActivity()
    {
        var active = RequireActive();
        active.LastActivityAt = _clock();
        _ledger.SaveActiveSession(active);
    }

    private void RecordCall(bool isChat)
    {
        var active = RequireActive();
        if (isChat)
        {
            active.ChatCalls++;
        }
        else
        {
            active.EmbedCalls++;
        }

        active.LastActivityAt = _clock();
        _ledger.SaveActiveSession(active);
    }

    private UsageActiveSession RequireActive() =>
        _ledger.LoadActiveSession()
        ?? throw new InvalidOperationException("No active usage session.");
}
