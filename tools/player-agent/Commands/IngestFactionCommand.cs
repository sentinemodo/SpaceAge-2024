using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class IngestFactionCommand
{
    public static Command Create()
    {
        var command = new Command(
            "ingest-faction",
            "Embed isolated faction report/story/orders into per-seat RAG (incremental by default).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.ReportOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.ClearOption);
        command.AddOption(CommandHelpers.FullCorpusOption);
        command.AddOption(CommandHelpers.StoryOnlyOption);
        command.AddOption(CommandHelpers.MaxOrderTurnsOption);

        command.SetHandler(async (context) =>
        {
            _ = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var clear = context.ParseResult.GetValueForOption(CommandHelpers.ClearOption);
            var fullCorpus = context.ParseResult.GetValueForOption(CommandHelpers.FullCorpusOption);
            var storyOnly = context.ParseResult.GetValueForOption(CommandHelpers.StoryOnlyOption);
            var maxOrderTurns = context.ParseResult.GetValueForOption(CommandHelpers.MaxOrderTurnsOption) ?? 3;
            var reportOverride = context.ParseResult.GetValueForOption(CommandHelpers.ReportOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption)
                ?? throw new InvalidOperationException("--run is required for ingest-faction.");
            var factionId = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption)
                ?? throw new InvalidOperationException("--faction is required and must be between 2 and 11.");
            if (factionId is < 2 or > 11)
            {
                throw new InvalidOperationException("--faction must be between 2 and 11.");
            }

            if (storyOnly && fullCorpus)
            {
                throw new InvalidOperationException("Use either --story-only or --full, not both.");
            }

            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);
            var factionDir = RepoPaths.FactionFolder(repoRoot, runId, factionId);
            var indexDir = RepoPaths.FactionIndexDirectory(settings.IndexDirectory, runId, factionId);
            Directory.CreateDirectory(indexDir);
            var sqlitePath = VectorIndexPaths.FactionSqlitePath(indexDir);
            var options = new FactionIngestOptions
            {
                ClearIndex = clear,
                FullCorpus = fullCorpus,
                StoryOnly = storyOnly,
                ReportPath = reportOverride,
                MaxOrderTurns = maxOrderTurns,
            };

            Console.WriteLine($"Faction folder:   {factionDir}");
            Console.WriteLine($"Faction index:    {indexDir}");
            Console.WriteLine($"SQLite path:      {sqlitePath}");
            Console.WriteLine(
                $"Ingest mode:      {(storyOnly ? "story-only" : fullCorpus ? "full corpus" : $"incremental (max-order-turns={maxOrderTurns})")}");

            if (!Directory.Exists(factionDir))
            {
                throw new InvalidOperationException($"Faction folder not found: {factionDir}");
            }

            var plan = FactionIngestPlanner.BuildPlan(factionDir, options);
            Console.WriteLine("Faction sources:");
            foreach (var path in plan.IngestPaths)
            {
                Console.WriteLine($"  [ok] {path}");
            }

            if (dryRun)
            {
                if (clear && File.Exists(sqlitePath))
                {
                    using var store = new SqliteVectorStore(sqlitePath);
                    var removed = store.Count();
                    store.ClearAll();
                    Console.WriteLine($"Cleared index:    {removed} existing chunk(s) removed.");
                }

                Console.WriteLine("Dry run: chunking only; no embed calls.");
                foreach (var path in plan.IngestPaths)
                {
                    var normalizedPath = SourcePathNormalizer.Normalize(path);
                    var content = await File.ReadAllTextAsync(normalizedPath, context.GetCancellationToken());
                    var chunks = path.EndsWith("story.md", StringComparison.OrdinalIgnoreCase)
                        ? MarkdownChunker.ChunkStory(normalizedPath, content)
                        : path.Contains("report", StringComparison.OrdinalIgnoreCase)
                            ? MarkdownChunker.ChunkReport(normalizedPath, content)
                            : MarkdownChunker.ChunkOrderFile(normalizedPath, content);
                    Console.WriteLine($"  {Path.GetFileName(path)}: {chunks.Count} chunks");
                }

                if (plan.PruneIndexedFactionCorpus && File.Exists(sqlitePath))
                {
                    using var store = new SqliteVectorStore(sqlitePath);
                    var stale = FactionIngestPlanner.FindStaleSources(
                        store.ListSourcePaths(),
                        factionDir,
                        plan.KeepSourcePaths);
                    foreach (var path in stale)
                    {
                        Console.WriteLine($"  prune {Path.GetFileName(path)}");
                    }
                }

                return;
            }

            IngestSummary summary = null!;
            await CommandHelpers.RunWithInferenceAsync(
                settings,
                context,
                command: "ingest-faction",
                runId,
                factionIds: [factionId],
                async (client, cancellationToken) =>
                {
                    var ingest = new CorpusIngestService(client);
                    summary = fullCorpus
                        ? await ingest.IngestFactionAsync(sqlitePath, factionDir, clear, cancellationToken)
                        : await ingest.IngestFactionIncrementalAsync(sqlitePath, factionDir, options, cancellationToken);
                });

            if (summary.ClearedExisting)
            {
                Console.WriteLine("Index cleared before ingest.");
            }

            Console.WriteLine($"Embedded chunks:  {summary.ChunkCount}");
            Console.WriteLine($"Stored total:     {summary.TotalStored}");
            if (!fullCorpus && summary.RemovedSources.Count > 0)
            {
                Console.WriteLine("Pruned sources:");
                foreach (var path in summary.RemovedSources)
                {
                    Console.WriteLine($"  {path}");
                }
            }

            Console.WriteLine("Faction ingest complete.");
        });

        return command;
    }
}
