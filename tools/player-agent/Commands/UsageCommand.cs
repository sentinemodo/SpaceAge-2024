using System.CommandLine;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Commands;

internal static class UsageCommand
{
    private static readonly Option<string?> CommandNameOption = new("--command")
    {
        Description = "Originating command name (default: manual).",
    };

    private static readonly Option<string?> ReasonOption = new("--reason")
    {
        Description = "Stop reason: user, complete, error, cancelled, guardrail, idle-timeout (default: user).",
    };

    private static readonly Option<string?> MonthOption = new("--month")
    {
        Description = "Calendar month as yyyy-MM (default: current UTC month).",
    };

    private static readonly Option<string?> SinceOption = new("--since")
    {
        Description = "Include sessions started on or after this ISO timestamp.",
    };

    public static Command Create()
    {
        var command = new Command("usage", "RunPod usage ledger and reports (Phase 7).");

        command.AddCommand(CreateStartCommand());
        command.AddCommand(CreateStopCommand());
        command.AddCommand(CreateStatusCommand());
        command.AddCommand(CreateReportCommand());
        command.AddCommand(CreateReclaimCommand());
        command.AddCommand(CreateSyncCommand());

        return command;
    }

    private static Command CreateStartCommand()
    {
        var command = new Command("start", "Begin a RunPod usage session.");
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandNameOption);

        command.SetHandler((context) =>
        {
            var settings = CommandHelpers.LoadSettings(context);
            var assumeYes = context.ParseResult.GetValueForOption(CommandHelpers.YesOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var commandName = context.ParseResult.GetValueForOption(CommandNameOption) ?? "manual";

            var ledger = new UsageLedger(settings.IndexDirectory);
            ledger.EnsureLayout();
            var budget = RunPodBudgetSettings.LoadFromEnvironment(settings.IsRemoteHost);
            var guardrails = new RunPodGuardrails();
            var overrideFile = RunPodGuardrailOverride.TryLoad(settings.IndexDirectory, DateTimeOffset.UtcNow);
            var check = guardrails.ValidateBeforeSessionStart(
                settings,
                budget,
                ledger,
                assumeYes,
                new ConsoleUserPrompt(),
                overrideFile);
            if (!check.Allowed)
            {
                throw new InvalidOperationException(check.Message);
            }

            var manager = new UsageSessionManager(ledger);
            var active = manager.StartSession(settings, commandName, runId, factionIds: null);

            Console.WriteLine($"Started usage session: {active.SessionId}");
            Console.WriteLine($"Host:             {active.Host}");
            Console.WriteLine($"Remote:           {active.IsRemote}");
            Console.WriteLine($"Hourly rate:      {UsageReportFormatter.FormatUsd(active.HourlyRateUsd)}/hr");
            if (!string.IsNullOrWhiteSpace(active.PodId))
            {
                Console.WriteLine($"Pod id:           {active.PodId}");
            }
        });

        return command;
    }

    private static Command CreateReclaimCommand()
    {
        var command = new Command(
            "reclaim",
            "Close a stale active session after a crashed runner (stop the RunPod pod manually in the console).");

        command.SetHandler(() =>
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            var ledger = new UsageLedger(settings.IndexDirectory);
            var active = ledger.LoadActiveSession();
            if (active is null)
            {
                Console.WriteLine("No active usage session to reclaim.");
                return;
            }

            var manager = new UsageSessionManager(ledger);
            var record = manager.StopSession(UsageStopReason.User);
            Console.WriteLine($"Reclaimed session: {record.SessionId}");
            Console.WriteLine(UsageReportFormatter.FormatCostSummary(record));
            Console.WriteLine("Stop or terminate the RunPod pod in the console if it is still running.");
        });

        return command;
    }

    private static Command CreateStopCommand()
    {
        var command = new Command("stop", "End the active usage session.");
        command.AddOption(ReasonOption);

        command.SetHandler((context) =>
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            var reason = context.ParseResult.GetValueForOption(ReasonOption) ?? UsageStopReason.User;

            var ledger = new UsageLedger(settings.IndexDirectory);
            var manager = new UsageSessionManager(ledger);
            var record = manager.StopSession(reason);

            Console.WriteLine(UsageReportFormatter.FormatCostSummary(record));
            Console.WriteLine($"Ledger:           {ledger.SessionsPath}");
        });

        return command;
    }

    private static Command CreateStatusCommand()
    {
        var command = new Command("status", "Show the active usage session.");
        command.SetHandler(() =>
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            var ledger = new UsageLedger(settings.IndexDirectory);
            var active = ledger.LoadActiveSession();

            if (active is null)
            {
                Console.WriteLine("No active usage session.");
                Console.WriteLine($"Ledger directory: {ledger.UsageDirectory}");
                return;
            }

            var elapsed = DateTimeOffset.UtcNow - active.StartedAt;
            Console.WriteLine($"Session id:       {active.SessionId}");
            Console.WriteLine($"Started:          {active.StartedAt:O}");
            Console.WriteLine($"Elapsed:          {UsageReportFormatter.FormatDuration(elapsed.TotalSeconds)}");
            Console.WriteLine($"Host:             {active.Host}");
            Console.WriteLine($"Remote:           {active.IsRemote}");
            Console.WriteLine($"Command:          {active.Command}");
            Console.WriteLine($"Run id:           {active.RunId ?? "(none)"}");
            Console.WriteLine($"Factions:         {(active.FactionIds.Count == 0 ? "(none)" : string.Join(", ", active.FactionIds))}");
            Console.WriteLine($"Chat calls:       {active.ChatCalls}");
            Console.WriteLine($"Embed calls:      {active.EmbedCalls}");
            Console.WriteLine($"Hourly rate:      ${active.HourlyRateUsd:F2}/hr");
            if (active.IsRemote)
            {
                var estimated = elapsed.TotalSeconds / 3600d * active.HourlyRateUsd;
                Console.WriteLine($"Estimated cost:   ${estimated:F2} (so far)");
            }
        });

        return command;
    }

    private static Command CreateReportCommand()
    {
        var command = new Command("report", "Summarize usage for a period.");
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(MonthOption);
        command.AddOption(SinceOption);

        command.SetHandler((context) =>
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var monthValue = context.ParseResult.GetValueForOption(MonthOption);
            var sinceValue = context.ParseResult.GetValueForOption(SinceOption);

            var ledger = new UsageLedger(settings.IndexDirectory);
            var sessions = ledger.LoadAllSessions();

            if (UsageReportFormatter.TryParseSince(sinceValue, out var since))
            {
                Console.WriteLine(BuildSinceReport(sessions, since, runId));
                return;
            }

            var month = UsageReportFormatter.TryParseMonth(monthValue, out var parsedMonth)
                ? new DateOnly(parsedMonth.Year, parsedMonth.Month, 1)
                : new DateOnly(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1);

            Console.WriteLine(UsageReportFormatter.BuildReport(
                sessions,
                month,
                runId,
                DateTimeOffset.UtcNow));
        });

        return command;
    }

    private static Command CreateSyncCommand()
    {
        var command = new Command("sync", "Reconcile local ledger with RunPod API (optional).");
        command.SetHandler(() =>
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            var ledger = new UsageLedger(settings.IndexDirectory);
            var sessionCount = ledger.LoadAllSessions().Count;

            Console.WriteLine("RunPod API sync (Phase 7B) is optional and not yet automated.");
            Console.WriteLine($"Local ledger:     {ledger.SessionsPath}");
            Console.WriteLine($"Completed rows:   {sessionCount}");
            Console.WriteLine("Set RUNPOD_API_KEY and PLAYER_AGENT_RUNPOD_POD_ID for future reconciliation.");
            Console.WriteLine("The RunPod billing console remains authoritative for actual charges.");
        });

        return command;
    }

    private static string BuildSinceReport(
        IReadOnlyList<UsageSessionRecord> sessions,
        DateTimeOffset since,
        string? runId)
    {
        if (!string.IsNullOrWhiteSpace(runId))
        {
            sessions = sessions
                .Where(session => string.Equals(session.RunId, runId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        sessions = sessions.Where(session => session.StartedAt >= since).ToList();

        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"Usage report — since {since:O}");
        builder.AppendLine($"Sessions:         {sessions.Count}");
        builder.AppendLine($"Duration:         {UsageReportFormatter.FormatDuration(sessions.Sum(s => s.DurationSec))}");
        builder.AppendLine($"Estimated cost:   {UsageReportFormatter.FormatUsd(sessions.Sum(s => s.EstimatedCostUsd))}");
        builder.AppendLine();
        foreach (var session in sessions.OrderBy(s => s.StartedAt))
        {
            builder.AppendLine(
                $"  {session.StartedAt.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)}  {session.SessionId}  "
                + $"{session.Command}  {UsageReportFormatter.FormatUsd(session.EstimatedCostUsd)}  {session.StopReason}");
        }

        return builder.ToString().TrimEnd();
    }
}
