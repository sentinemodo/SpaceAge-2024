namespace SpaceAge.PlayerAgent.Usage;

public static class RunPodBudgetTracker
{
    public static double MonthlyRemoteSpend(IReadOnlyList<UsageSessionRecord> sessions, DateTimeOffset nowUtc)
    {
        var monthStart = new DateTimeOffset(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        return sessions
            .Where(session => session.IsRemote)
            .Where(session => session.StartedAt >= monthStart && session.StartedAt < monthEnd)
            .Sum(session => session.EstimatedCostUsd);
    }

    public static double MonthlyRemoteHours(IReadOnlyList<UsageSessionRecord> sessions, DateTimeOffset nowUtc)
    {
        var monthStart = new DateTimeOffset(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        return sessions
            .Where(session => session.IsRemote)
            .Where(session => session.StartedAt >= monthStart && session.StartedAt < monthEnd)
            .Sum(session => session.DurationSec) / 3600d;
    }

    public static double ProjectedSessionCost(UsageActiveSession active, DateTimeOffset nowUtc) =>
        Math.Max(0, (nowUtc - active.StartedAt).TotalSeconds) / 3600d * active.HourlyRateUsd;

    public static double ProjectedMonthlySpend(
        IReadOnlyList<UsageSessionRecord> completedSessions,
        UsageActiveSession? activeSession,
        DateTimeOffset nowUtc)
    {
        var spent = MonthlyRemoteSpend(completedSessions, nowUtc);
        if (activeSession is { IsRemote: true })
        {
            spent += ProjectedSessionCost(activeSession, nowUtc);
        }

        return spent;
    }
}
