namespace SpaceAge.PlayerAgent.Usage;

public static class LlmUsageNoteWriter
{
    public static void AppendRunNote(string repoRoot, string runId, UsageSessionRecord record)
    {
        var notePath = Path.Combine(repoRoot, "play", "runs", runId, "gm", "llm-usage.md");
        Directory.CreateDirectory(Path.GetDirectoryName(notePath)!);

        var line = $"- {record.EndedAt:yyyy-MM-dd HH:mm} UTC — {record.Command} — "
            + $"{UsageReportFormatter.FormatDuration(record.DurationSec)} — "
            + $"{UsageReportFormatter.FormatUsd(record.EstimatedCostUsd)} — "
            + $"{record.ChatCalls} chat / {record.EmbedCalls} embed — {record.StopReason}";

        if (File.Exists(notePath))
        {
            File.AppendAllText(notePath, line + Environment.NewLine);
            return;
        }

        var header = "# LLM usage (cost estimates only — no secrets)\n\n"
            + "Compare to the RunPod billing console; console is authoritative.\n\n";
        File.WriteAllText(notePath, header + line + Environment.NewLine);
    }
}
