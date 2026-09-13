using System.CommandLine;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Commands;

internal static class DraftStoryCommand
{
    public static Command Create()
    {
        var command = new Command(
            "draft-story",
            "Draft UTF-8 story.md from persona + report via Ollama (campaign-ai handoff).");
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.OutputOption);
        command.AddOption(CommandHelpers.ReportOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);
        command.AddOption(ChunkedOption);

        command.SetHandler(async context =>
        {
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var assumeYes = context.ParseResult.GetValueForOption(CommandHelpers.YesOption);
            var chunked = context.ParseResult.GetValueForOption(ChunkedOption);
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
            var personaPath = Path.Combine(factionDir, "persona.md");
            if (!File.Exists(personaPath))
            {
                throw new FileNotFoundException($"Missing persona.md under {factionDir}");
            }

            if (!OrderFileNaming.TryParseReportFileName(reportPath, out var reportTurn, out var reportFaction)
                || reportFaction != factionIdValue)
            {
                throw new InvalidOperationException($"Could not parse report turn/faction from {reportPath}");
            }

            var outputPath = string.IsNullOrWhiteSpace(output)
                ? Path.Combine(factionDir, "story.md")
                : Path.GetFullPath(output);
            var hasPriorStory = File.Exists(outputPath);

            Console.WriteLine($"Faction:          {factionIdValue}");
            Console.WriteLine($"Report turn:      {reportTurn}");
            Console.WriteLine($"Story output:     {outputPath}");
            Console.WriteLine($"Report:           {reportPath}");
            Console.WriteLine($"Persona:          {personaPath}");
            Console.WriteLine($"Chat timeout:     {settings.ChatTimeoutSeconds}s");
            Console.WriteLine($"Strategy:         {(chunked ? "chunked" : "monolithic (chunked fallback on timeout)")}");
            Console.WriteLine($"Remote host:      {settings.IsRemoteHost}");

            var request = new StoryDraftRequest
            {
                FactionId = factionIdValue,
                ReportTurn = reportTurn,
                OutputPath = outputPath,
                PersonaPath = personaPath,
                ReportPath = reportPath,
                HasPriorStory = hasPriorStory,
                Chunked = chunked,
                DryRun = dryRun,
            };

            var inference = RemoteInferenceContext.Create(
                settings,
                assumeYes,
                new ConsoleUserPrompt(),
                command: "draft-story",
                runId,
                [factionIdValue],
                dryRun);
            var service = new StoryDraftService(inference.Client);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                var result = await service.DraftAsync(request, cancellationToken);
                if (dryRun)
                {
                    Console.WriteLine();
                    Console.WriteLine("Dry run monolithic prompt:");
                    Console.WriteLine(result.MonolithicPrompt);
                    return;
                }

                Console.WriteLine($"Strategy used:    {result.Strategy}");
                if (result.TimedOutBeforeFallback)
                {
                    Console.WriteLine("Note:             monolithic call timed out; chunked fallback succeeded.");
                }

                Console.WriteLine($"Wrote story:      {outputPath}");
            }
            catch (OperationCanceledException)
            {
                inference.UsageScope?.MarkCancelled();
                throw;
            }
            catch (RunPodGuardrailException)
            {
                throw;
            }
            catch
            {
                inference.UsageScope?.MarkError();
                throw;
            }
            finally
            {
                await inference.DisposeAsync();
            }
        });

        return command;
    }

    private static Option<bool> ChunkedOption { get; } = new("--chunked")
    {
        Description = "Use three smaller chat calls (strategic, tactical, narrative) instead of one monolithic prompt.",
    };

    private static string? ResolveReportPath(string factionDir, string? reportOverride)
    {
        if (!string.IsNullOrWhiteSpace(reportOverride))
        {
            return Path.GetFullPath(reportOverride);
        }

        return FactionCorpusPaths.LatestReportPath(factionDir);
    }
}
