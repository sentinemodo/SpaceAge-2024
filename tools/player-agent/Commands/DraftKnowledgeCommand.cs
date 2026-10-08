using System.CommandLine;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Commands;

internal static class DraftKnowledgeCommand
{
    public static Command Create()
    {
        var command = new Command(
            "draft-knowledge",
            "Draft UTF-8 knowledge.md from report via Ollama (region map + scout/fuel notes for next turn).");
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.OutputOption);
        command.AddOption(CommandHelpers.ReportOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);

        command.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var assumeYes = context.ParseResult.GetValueForOption(CommandHelpers.YesOption);
            var output = context.ParseResult.GetValueForOption(CommandHelpers.OutputOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption)
                ?? throw new InvalidOperationException("--run is required.");
            var factionIdValue = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption)
                ?? throw new InvalidOperationException("--faction is required (2–11).");
            if (factionIdValue is < 2 or > 11)
            {
                throw new InvalidOperationException("--faction must be between 2 and 11.");
            }

            var reportOverride = context.ParseResult.GetValueForOption(CommandHelpers.ReportOption);
            var repoRoot = RepoPaths.FindRepositoryRoot();
            var factionDir = RepoPaths.FactionFolder(repoRoot, runId, factionIdValue);
            var reportPath = ResolveReportPath(factionDir, reportOverride)
                ?? throw new InvalidOperationException($"No report found under {factionDir}");
            if (!OrderFileNaming.TryParseReportFileName(reportPath, out var reportTurn, out var reportFaction)
                || reportFaction != factionIdValue)
            {
                throw new InvalidOperationException($"Could not parse report turn/faction from {reportPath}");
            }

            var outputPath = string.IsNullOrWhiteSpace(output)
                ? FactionCorpusPaths.KnowledgePath(factionDir)
                ?? Path.Combine(factionDir, "knowledge.md")
                : Path.GetFullPath(output);

            var personaPath = Path.Combine(factionDir, "persona.md");
            string? personaPreference = null;
            if (File.Exists(personaPath))
            {
                personaPreference = VerbInference.DetectPersonaPreference(await File.ReadAllTextAsync(personaPath, cancellationToken));
            }

            Console.WriteLine($"Faction:          {factionIdValue}");
            Console.WriteLine($"Report turn:      {reportTurn}");
            Console.WriteLine($"Knowledge output: {outputPath}");
            Console.WriteLine($"Report:           {reportPath}");
            Console.WriteLine($"Chat timeout:     {settings.ChatTimeoutSeconds}s");
            Console.WriteLine($"Remote host:      {settings.IsRemoteHost}");

            var request = new KnowledgeDraftRequest
            {
                FactionId = factionIdValue,
                ReportTurn = reportTurn,
                ReportPath = reportPath,
                OutputPath = outputPath,
                PersonaPreference = personaPreference,
                DryRun = dryRun,
            };

            var inference = RemoteInferenceContext.Create(
                settings,
                assumeYes,
                new ConsoleUserPrompt(),
                command: "draft-knowledge",
                runId,
                [factionIdValue],
                dryRun);
            var service = new KnowledgeDraftService(inference.Client);
            try
            {
                var result = await service.DraftAsync(request, context.GetCancellationToken());
                if (dryRun)
                {
                    Console.WriteLine();
                    Console.WriteLine("Dry run prompt:");
                    Console.WriteLine(result.UserPrompt);
                    return;
                }

                Console.WriteLine($"Wrote knowledge:  {result.OutputPath}");
            }
            finally
            {
                await inference.DisposeAsync();
            }
        });

        return command;
    }

    private static string? ResolveReportPath(string factionDir, string? reportOverride)
    {
        if (!string.IsNullOrWhiteSpace(reportOverride))
        {
            return Path.GetFullPath(reportOverride);
        }

        return FactionCorpusPaths.LatestReportPath(factionDir);
    }
}
