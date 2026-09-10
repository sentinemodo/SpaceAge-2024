using System.CommandLine;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class AuditIsolationCommand
{
    public static Command Create()
    {
        var command = new Command(
            "audit-isolation",
            "Verify shared and per-faction RAG indexes contain no cross-seat contamination (Phase 6).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FromFactionOption);
        command.AddOption(CommandHelpers.ToFactionOption);
        command.AddOption(CommandHelpers.NoRecordAuditOption);

        command.SetHandler((context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context, checkRemote: false);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var fromFaction = context.ParseResult.GetValueForOption(CommandHelpers.FromFactionOption) ?? 2;
            var toFaction = context.ParseResult.GetValueForOption(CommandHelpers.ToFactionOption) ?? 11;
            var recordAudit = !context.ParseResult.GetValueForOption(CommandHelpers.NoRecordAuditOption);

            if (fromFaction is < 2 or > 11 || toFaction is < 2 or > 11 || fromFaction > toFaction)
            {
                throw new InvalidOperationException("--from-faction and --to-faction must be between 2 and 11 with from <= to.");
            }

            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);

            IsolationAuditResult result;
            if (string.IsNullOrWhiteSpace(runId))
            {
                var sharedPath = VectorIndexPaths.SharedSqlitePath(
                    RepoPaths.SharedIndexDirectory(settings.IndexDirectory, mode));
                result = IsolationAuditor.AuditSharedIndex(
                    sharedPath,
                    mode,
                    repoRoot,
                    RepoPaths.PlayerDirectory(repoRoot));
            }
            else
            {
                result = IsolationAuditor.AuditRun(
                    settings.IndexDirectory,
                    runId,
                    mode,
                    repoRoot,
                    fromFaction,
                    toFaction);
            }

            IsolationAuditor.PrintViolations(result);

            if (recordAudit && !string.IsNullOrWhiteSpace(runId))
            {
                var note = IsolationAuditor.BuildAuditNote(
                    runId,
                    mode,
                    fromFaction,
                    toFaction,
                    DateTimeOffset.UtcNow.ToString("O"),
                    result);
                var auditPath = Path.Combine(repoRoot, "play", "runs", runId, "gm", "isolation-audit.md");
                IsolationAuditor.AppendRunAuditNote(auditPath, note);
                Console.WriteLine($"Recorded audit: {auditPath}");
            }

            if (!result.IsClean)
            {
                throw new InvalidOperationException(
                    $"Isolation audit failed with {result.Violations.Count} violation(s).");
            }

            Console.WriteLine("audit-isolation complete.");
        });

        return command;
    }
}
