namespace SpaceAge.PlayerAgent.Usage;

public sealed class UsageActiveSession
{
    public required string SessionId { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset LastActivityAt { get; set; }
    public required string Host { get; init; }
    public bool IsRemote { get; init; }
    public string? PodId { get; init; }
    public string? GpuClass { get; init; }
    public string? CloudTier { get; init; }
    public string Command { get; init; } = string.Empty;
    public string? RunId { get; init; }
    public IReadOnlyList<int> FactionIds { get; init; } = [];
    public double HourlyRateUsd { get; init; }
    public int ChatCalls { get; set; }
    public int EmbedCalls { get; set; }
}
