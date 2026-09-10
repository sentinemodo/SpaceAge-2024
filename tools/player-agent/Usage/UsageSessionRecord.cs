namespace SpaceAge.PlayerAgent.Usage;

public sealed record UsageSessionRecord(
    string SessionId,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    double DurationSec,
    string Host,
    bool IsRemote,
    string? PodId,
    string? GpuClass,
    string? CloudTier,
    string? RunId,
    IReadOnlyList<int> FactionIds,
    int ChatCalls,
    int EmbedCalls,
    double HourlyRateUsd,
    double EstimatedCostUsd,
    string StopReason,
    string Command);
