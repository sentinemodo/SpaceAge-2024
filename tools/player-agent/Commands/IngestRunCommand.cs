using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class IngestRunCommand
{
    public static Command Create()
    {
        var command = new Command(
            "ingest-run",
            "Incremental faction RAG refresh for every AI seat after isolate (Phase 4).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.ClearOption);
        command.AddOption(CommandHelpers.FullCorpusOption);
        command.AddOption(CommandHelpers.MaxOrderTurnsOption);
        command.AddOption(CommandHelpers.FromFactionOption);
        command.AddOption(CommandHelpers.ToFactionOption);

        command.SetHandler(async (context) =>
        {
            _ = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var clear = context.ParseResult.GetValueForOption(CommandHelpers.ClearOption);
            var fullCorpus = context.ParseResult.GetValueForOption(CommandHelpers.FullCorpusOption);
            var maxOrderTurns = context.ParseResult.GetValueForOption(CommandHelpers.MaxOrderTurnsOption) ?? 3;
            var fromFaction = context.ParseResult.GetValueForOption(CommandHelpers.FromFactionOption) ?? 2;
            var toFaction = context.ParseResult.GetValueForOption(CommandHelpers.ToFactionOption) ?? 11;
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption)
                ?? throw new InvalidOperationException("--run is required for ingest-run.");

            if (fromFaction is < 2 or > 11 || toFaction is < 2 or > 11 || fromFaction > toFaction)
            {
                throw new InvalidOperationException("--from-faction and --to-faction must be between 2 and 11 with from <= to.");
            }

            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);
            var options = new FactionIngestOptions
            {
                ClearIndex = clear,
                FullCorpus = fullCorpus,
                MaxOrderTurns = maxOrderTurns,
            };

            Console.WriteLine($"Run id:           {runId}");
            Console.WriteLine($"Factions:         {fromFaction}..{toFaction}");
            Console.WriteLine($"Ingest mode:      {(fullCorpus ? "full corpus" : $"incremental (max-order-turns={maxOrderTurns})")}");

            var failures = new List<(int FactionId, string Error)>();
            for (var factionId = fromFaction; factionId <= toFaction; factionId++)
            {
                var factionDir = RepoPaths.FactionFolder(repoRoot, runId, factionId);
                var indexDir = RepoPaths.FactionIndexDirectory(settings.IndexDirectory, runId, factionId);
                Directory.CreateDirectory(indexDir);
                var sqlitePath = VectorIndexPaths.FactionSqlitePath(indexDir);

                Console.WriteLine();
                Console.WriteLine($"=== Faction {factionId} ===");
                Console.WriteLine($"Folder:           {factionDir}");
                Console.WriteLine($"Index:            {sqlitePath}");

                if (!Directory.Exists(factionDir))
                {
                    failures.Add((factionId, $"Faction folder not found: {factionDir}"));
                    Console.WriteLine("Skipped:          missing faction folder.");
                    continue;
                }

                try
                {
                    if (dryRun)
                    {
                        var plan = FactionIngestPlanner.BuildPlan(factionDir, options);
                        Console.WriteLine("Dry run plan:");
                        foreach (var path in plan.IngestPaths)
                        {
                            Console.WriteLine($"  ingest  {path}");
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
                                Console.WriteLine($"  prune   {path}");
                            }
                        }

                        continue;
                    }

                    using var client = new OllamaClient(settings);
                    var ingest = new CorpusIngestService(client);
                    var summary = await ingest.IngestFactionIncrementalAsync(
                        sqlitePath,
                        factionDir,
                        options,
                        context.GetCancellationToken());

                    Console.WriteLine($"Embedded chunks:  {summary.ChunkCount}");
                    Console.WriteLine($"Stored total:     {summary.TotalStored}");
                    if (summary.RemovedSources.Count > 0)
                    {
                        Console.WriteLine("Pruned sources:");
                        foreach (var path in summary.RemovedSources)
                        {
                            Console.WriteLine($"  {path}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    failures.Add((factionId, ex.Message));
                    Console.WriteLine($"Failed:           {ex.Message}");
                }
            }

            Console.WriteLine();
            if (failures.Count > 0)
            {
                throw new InvalidOperationException(
                    $"ingest-run completed with {failures.Count} failure(s): "
                    + string.Join("; ", failures.Select(f => $"faction {f.FactionId}: {f.Error}")));
            }

            Console.WriteLine("ingest-run complete.");
        });

        return command;
    }
}
