using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class IngestFactionCommand
{
    public static Command Create()
    {
        var command = new Command("ingest-faction", "Embed an isolated faction report into per-seat RAG (Phase 2).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.ClearOption);

        command.SetHandler(async (context) =>
        {
            _ = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var clear = context.ParseResult.GetValueForOption(CommandHelpers.ClearOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption)
                ?? throw new InvalidOperationException("--run is required for ingest-faction.");
            var factionId = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption)
                ?? throw new InvalidOperationException("--faction is required and must be between 2 and 11.");
            if (factionId is < 2 or > 11)
            {
                throw new InvalidOperationException("--faction must be between 2 and 11.");
            }

            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);
            var factionDir = RepoPaths.FactionFolder(repoRoot, runId, factionId);
            var indexDir = RepoPaths.FactionIndexDirectory(settings.IndexDirectory, runId, factionId);
            Directory.CreateDirectory(indexDir);
            var sqlitePath = VectorIndexPaths.FactionSqlitePath(indexDir);

            Console.WriteLine($"Faction folder:   {factionDir}");
            Console.WriteLine($"Faction index:    {indexDir}");
            Console.WriteLine($"SQLite path:      {sqlitePath}");

            if (!Directory.Exists(factionDir))
            {
                throw new InvalidOperationException($"Faction folder not found: {factionDir}");
            }

            var sourcePaths = FactionCorpusPaths.ReportPaths(factionDir)
                .Concat(FactionCorpusPaths.StoryPath(factionDir) is { } story ? [story] : [])
                .Concat(FactionCorpusPaths.OrderPaths(factionDir))
                .ToList();

            Console.WriteLine("Faction sources:");
            foreach (var path in sourcePaths)
            {
                Console.WriteLine($"  [ok] {path}");
            }

            if (sourcePaths.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No report, story, or order files found under {factionDir}");
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
                foreach (var path in sourcePaths)
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

                return;
            }

            using var client = new OllamaClient(settings);
            var ingest = new CorpusIngestService(client);
            var summary = await ingest.IngestFactionAsync(
                sqlitePath,
                factionDir,
                clear,
                context.GetCancellationToken());

            if (summary.ClearedExisting)
            {
                Console.WriteLine("Index cleared before ingest.");
            }

            Console.WriteLine($"Embedded chunks:  {summary.ChunkCount}");
            Console.WriteLine($"Stored total:     {summary.TotalStored}");
            Console.WriteLine("Faction ingest complete.");
        });

        return command;
    }
}
