using System.CommandLine;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class DraftCommand
{
    public static Command Create()
    {
        var command = new Command("draft", "Draft UTF-8 orders from report + RAG (Phase 3).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.OutputOption);
        command.AddOption(CommandHelpers.ReportOption);
        command.AddOption(CommandHelpers.TurnOption);
        command.AddOption(CommandHelpers.IterationOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.TopOption);

        command.SetHandler(async (context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var output = context.ParseResult.GetValueForOption(CommandHelpers.OutputOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var factionIdValue = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption)
                ?? throw new InvalidOperationException("--faction is required (2–11).");
            if (factionIdValue is < 2 or > 11)
            {
                throw new InvalidOperationException("--faction must be between 2 and 11.");
            }

            var reportOverride = context.ParseResult.GetValueForOption(CommandHelpers.ReportOption);
            var turnOverride = context.ParseResult.GetValueForOption(CommandHelpers.TurnOption);
            var iterationOverride = context.ParseResult.GetValueForOption(CommandHelpers.IterationOption);
            var topK = context.ParseResult.GetValueForOption(CommandHelpers.TopOption) ?? 6;

            var repoRoot = RepoPaths.FindRepositoryRoot();

            var factionDir = string.IsNullOrWhiteSpace(runId)
                ? null
                : RepoPaths.FactionFolder(repoRoot, runId, factionIdValue);

            var reportPath = ResolveReportPath(factionDir, reportOverride);
            var draftPath = RepoPaths.ResolveDraftOutput(
                repoRoot,
                output,
                runId,
                factionIdValue,
                turnOverride,
                iterationOverride,
                reportPath);
            var storyPath = factionDir is null ? null : FactionCorpusPaths.StoryPath(factionDir);

            Console.WriteLine($"Mode:             {PlayModeParser.ToCliValue(mode)}");
            Console.WriteLine($"Faction:          {factionIdValue}");
            Console.WriteLine($"Draft output:     {draftPath}");
            Console.WriteLine($"Report:           {reportPath ?? "(none)"}");
            Console.WriteLine($"Story:            {storyPath ?? "(none)"}");
            Console.WriteLine($"Remote host:      {settings.IsRemoteHost}");
            Console.WriteLine($"Top-k retrieval:  {topK}");

            var request = new OrderDraftRequest
            {
                Mode = mode,
                FactionId = factionIdValue,
                OutputPath = draftPath,
                RunId = runId,
                FactionDir = factionDir,
                ReportPath = reportPath,
                StoryPath = storyPath,
                DryRun = dryRun,
                TopK = topK,
            };

            if (dryRun)
            {
                using var client = new OllamaClient(settings);
                var service = new OrderDraftService(client, settings);
                var result = await service.DraftAsync(request, context.GetCancellationToken());
                PrintRetrievalSummary(result);
                Console.WriteLine();
                Console.WriteLine("Dry run prompt pack:");
                Console.WriteLine(result.ChatPrompt);
                return;
            }

            using (var client = new OllamaClient(settings))
            {
                var service = new OrderDraftService(client, settings);
                var result = await service.DraftAsync(request, context.GetCancellationToken());
                PrintRetrievalSummary(result);
                Console.WriteLine($"Wrote draft:      {result.OutputPath}");
                Console.WriteLine("Reminder: UTF-8 draft; play/turn.ps1 converts to Windows-1251 for Game.exe.");
            }
        });

        return command;
    }

    private static string? ResolveReportPath(string? factionDir, string? reportOverride)
    {
        if (!string.IsNullOrWhiteSpace(reportOverride))
        {
            return Path.GetFullPath(reportOverride);
        }

        if (factionDir is null)
        {
            return null;
        }

        return FactionCorpusPaths.ReportPaths(factionDir).LastOrDefault();
    }

    private static void PrintRetrievalSummary(OrderDraftResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.VerbFilter))
        {
            Console.WriteLine($"Verb filter:      {result.VerbFilter}");
        }

        Console.WriteLine($"Retrieved chunks: {result.RetrievedChunks.Count}");
        foreach (var chunk in result.RetrievedChunks)
        {
            var metadata = chunk.Chunk.Chunk.Metadata;
            Console.WriteLine(
                $"  [{chunk.Score:F3}] doc={metadata.Doc} verb={metadata.Verb ?? "-"} heading={metadata.Heading ?? "-"}");
        }

        if (result.LintResult is not null)
        {
            Console.WriteLine($"Lint:             {(result.LintResult.IsValid ? "pass" : "fail")}");
        }
    }
}
