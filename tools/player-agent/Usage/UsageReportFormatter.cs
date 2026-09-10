using System.Globalization;
using System.Text;

namespace SpaceAge.PlayerAgent.Usage;

public static class UsageReportFormatter
{
    public static string BuildReport(
        IReadOnlyList<UsageSessionRecord> sessions,
        DateOnly month,
        string? activeRunId,
        DateTimeOffset nowUtc)
    {
        var monthStart = new DateTimeOffset(month.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        var monthSessions = sessions
            .Where(session => session.StartedAt >= monthStart && session.StartedAt < monthEnd)
            .OrderBy(session => session.StartedAt)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine(
            $"Usage report — {month.ToString("MMMM yyyy", CultureInfo.InvariantCulture)} "
            + $"(generated {nowUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC)");
        builder.AppendLine();

        var totalDuration = monthSessions.Sum(session => session.DurationSec);
        var totalCost = monthSessions.Sum(session => session.EstimatedCostUsd);
        var remoteSessions = monthSessions.Count(session => session.IsRemote);

        builder.AppendLine($"Month totals: {monthSessions.Count} session(s), {FormatDuration(totalDuration)}, "
            + $"estimated {FormatUsd(totalCost)} ({remoteSessions} remote)");
        builder.AppendLine(
            $"Calls: {monthSessions.Sum(s => s.ChatCalls)} chat, {monthSessions.Sum(s => s.EmbedCalls)} embed");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(activeRunId))
        {
            var runSessions = monthSessions
                .Where(session => string.Equals(session.RunId, activeRunId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            builder.AppendLine($"Active run '{activeRunId}': {runSessions.Count} session(s), "
                + $"{FormatDuration(runSessions.Sum(s => s.DurationSec))}, "
                + $"estimated {FormatUsd(runSessions.Sum(s => s.EstimatedCostUsd))}");
            builder.AppendLine();
        }

        if (monthSessions.Count == 0)
        {
            builder.AppendLine("No sessions recorded for this month.");
            return builder.ToString().TrimEnd();
        }

        builder.AppendLine("Sessions:");
        foreach (var session in monthSessions)
        {
            builder.AppendLine(
                $"  {session.StartedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}  {session.SessionId}  "
                + $"{session.Command}  {FormatDuration(session.DurationSec)}  "
                + $"{FormatUsd(session.EstimatedCostUsd)}  {session.StopReason}"
                + (session.RunId is null ? string.Empty : $"  run={session.RunId}"));
        }

        builder.AppendLine();
        builder.AppendLine("Compare local estimates to the RunPod billing console; console is authoritative.");
        return builder.ToString().TrimEnd();
    }

    public static string FormatCostSummary(UsageSessionRecord record)
    {
        var duration = FormatDuration(record.DurationSec);
        var rate = record.IsRemote
            ? $" @ {FormatUsd(record.HourlyRateUsd)}/hr"
            : string.Empty;
        return $"Usage: {duration}{rate} ≈ {FormatUsd(record.EstimatedCostUsd)} (session {record.SessionId}, {record.StopReason})";
    }

    public static string FormatUsd(double amount) =>
        "$" + amount.ToString("F2", CultureInfo.InvariantCulture);

    public static string FormatDuration(double durationSec)
    {
        var span = TimeSpan.FromSeconds(durationSec);
        if (span.TotalHours >= 1)
        {
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        }

        if (span.TotalMinutes >= 1)
        {
            return $"{(int)span.TotalMinutes}m {span.Seconds}s";
        }

        return $"{span.Seconds}s";
    }

    public static bool TryParseMonth(string? value, out DateOnly month)
    {
        month = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (DateOnly.TryParseExact(value + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out month))
        {
            return true;
        }

        return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out month);
    }

    public static bool TryParseSince(string? value, out DateTimeOffset since)
    {
        since = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out since);
    }
}
