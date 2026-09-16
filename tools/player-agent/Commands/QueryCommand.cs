using System.CommandLine;
using System.Text.Json;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class QueryCommand
{
    public static Command Create()
    {
        var command = new Command("query", "Ad-hoc RAG-backed LLM query (visual-tool / game-host).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.QueryOption);
        command.AddOption(CommandHelpers.ReportOption);
        command.AddOption(CommandHelpers.TopOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);
        command.AddOption(IncludeStoryOption);
        command.AddOption(StoryPathOption);
        command.AddOption(PersonaPathOption);
        command.AddOption(JsonOutputOption);

        command.SetHandler(async (context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var query = context.ParseResult.GetValueForOption(CommandHelpers.QueryOption)
                ?? throw new InvalidOperationException("--query is required.");
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var factionIdValue = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption)
                ?? throw new InvalidOperationException("--faction is required (2–11).");
            if (factionIdValue is < 2 or > 11)
            {
                throw new InvalidOperationException("--faction must be between 2 and 11.");
            }

            var reportOverride = context.ParseResult.GetValueForOption(CommandHelpers.ReportOption);
            var includeStory = context.ParseResult.GetValueForOption(IncludeStoryOption);
            var storyOverride = context.ParseResult.GetValueForOption(StoryPathOption);
            var personaOverride = context.ParseResult.GetValueForOption(PersonaPathOption);
            var jsonOutput = context.ParseResult.GetValueForOption(JsonOutputOption);
            var topK = CommandHelpers.ResolveTopK(
                settings,
                context.ParseResult.GetValueForOption(CommandHelpers.TopOption));

            var repoRoot = RepoPaths.FindRepositoryRoot();
            var factionDir = string.IsNullOrWhiteSpace(runId)
                ? null
                : RepoPaths.FactionFolder(repoRoot, runId, factionIdValue);
            var reportPath = ResolveReportPath(factionDir, reportOverride);
            var storyPath = storyOverride
                ?? (factionDir is null ? null : FactionCorpusPaths.StoryPath(factionDir));
            var personaPath = personaOverride
                ?? (factionDir is null ? null : Path.Combine(factionDir, "persona.md"));

            var request = new QueryRequest
            {
                Mode = mode,
                FactionId = factionIdValue,
                Query = query,
                RunId = runId,
                ReportPath = reportPath,
                StoryPath = storyPath,
                PersonaPath = personaPath,
                IncludeStory = includeStory,
                TopK = topK,
            };

            await CommandHelpers.RunWithInferenceAsync(
                settings,
                context,
                command: "query",
                runId,
                [factionIdValue],
                async (client, cancellationToken) =>
                {
                    var service = new QueryService(client, settings);
                    var result = await service.QueryAsync(request, cancellationToken);
                    if (jsonOutput)
                    {
                        var payload = new
                        {
                            ok = true,
                            query = result.Query,
                            output = result.Output,
                            hits = result.Retrieved.Count,
                        };
                        Console.WriteLine("---JSON---");
                        Console.WriteLine(JsonSerializer.Serialize(payload));
                        return;
                    }

                    Console.WriteLine(result.Output);
                });
        });

        return command;
    }

    private static Option<bool> IncludeStoryOption { get; } = new("--include-story")
    {
        Description = "Include faction story.md in the prompt.",
    };

    private static Option<string?> StoryPathOption { get; } = new("--story-path")
    {
        Description = "Override path to story.md.",
    };

    private static Option<string?> PersonaPathOption { get; } = new("--persona-path")
    {
        Description = "Override path to persona.md.",
    };

    private static Option<bool> JsonOutputOption { get; } = new("--json")
    {
        Description = "Emit machine-readable JSON after ---JSON--- marker.",
    };

    private static string? ResolveReportPath(string? factionDir, string? reportOverride)
    {
        if (!string.IsNullOrWhiteSpace(reportOverride))
        {
            return Path.GetFullPath(reportOverride);
        }

        return factionDir is null ? null : FactionCorpusPaths.LatestReportPath(factionDir);
    }
}
