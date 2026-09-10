using NUnit.Framework;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class UsageReportFormatterTests
{
    [Test]
    public void BuildReport_IncludesMonthlyTotalsAndActiveRun()
    {
        var sessions = new[]
        {
            Sample("s1", "2026-09-05T10:00:00Z", "2026-09-05T10:30:00Z", runId: "alpha", cost: 0.10),
            Sample("s2", "2026-09-08T12:00:00Z", "2026-09-08T13:00:00Z", runId: "smoke-test", cost: 0.44),
            Sample("s3", "2026-08-30T12:00:00Z", "2026-08-30T13:00:00Z", runId: "old", cost: 1.00),
        };

        var report = UsageReportFormatter.BuildReport(
            sessions,
            month: new DateOnly(2026, 9, 1),
            activeRunId: "smoke-test",
            nowUtc: DateTimeOffset.Parse("2026-09-10T15:00:00Z"));

        Assert.That(report, Does.Contain("September 2026"));
        Assert.That(report, Does.Contain("$0.54"));
        Assert.That(report, Does.Contain("smoke-test"));
        Assert.That(report, Does.Not.Contain("old"));
    }

    [Test]
    public void FormatCostSummary_OneLineForBatchEnd()
    {
        var record = Sample("s1", "2026-09-10T10:00:00Z", "2026-09-10T10:12:00Z", cost: 0.09);

        var line = UsageReportFormatter.FormatCostSummary(record);

        Assert.That(line, Does.Contain("12m"));
        Assert.That(line, Does.Contain("$0.09"));
        Assert.That(line, Does.Contain("s1"));
    }

    private static UsageSessionRecord Sample(
        string id,
        string started,
        string ended,
        string runId = "demo",
        double cost = 0.05) =>
        new(
            SessionId: id,
            StartedAt: DateTimeOffset.Parse(started),
            EndedAt: DateTimeOffset.Parse(ended),
            DurationSec: (DateTimeOffset.Parse(ended) - DateTimeOffset.Parse(started)).TotalSeconds,
            Host: "https://pod.example",
            IsRemote: true,
            PodId: "pod-1",
            GpuClass: "RTX 4090",
            CloudTier: "Secure",
            RunId: runId,
            FactionIds: [2],
            ChatCalls: 1,
            EmbedCalls: 2,
            HourlyRateUsd: 0.44,
            EstimatedCostUsd: cost,
            StopReason: UsageStopReason.Complete,
            Command: "draft-run");
}
