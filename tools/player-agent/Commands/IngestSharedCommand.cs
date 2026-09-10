using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class IngestSharedCommand
{
    public static Command Create()
    {
        var command = new Command("ingest-shared", "Embed player manuals into the shared RAG index (Phase 2).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.ClearOption);

        command.SetHandler(async (context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var clear = context.ParseResult.GetValueForOption(CommandHelpers.ClearOption);
            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);

            var sharedIndexDir = RepoPaths.SharedIndexDirectory(settings.IndexDirectory, mode);
            Directory.CreateDirectory(sharedIndexDir);
            var sqlitePath = VectorIndexPaths.SharedSqlitePath(sharedIndexDir);
            var manualPaths = RepoPaths.SharedManualPaths(repoRoot, mode);

            Console.WriteLine($"Mode:             {PlayModeParser.ToCliValue(mode)}");
            Console.WriteLine($"Shared index dir: {sharedIndexDir}");
            Console.WriteLine($"SQLite path:      {sqlitePath}");
            Console.WriteLine("Manual sources:");

            foreach (var path in manualPaths)
            {
                var exists = File.Exists(path) ? "ok" : "MISSING";
                Console.WriteLine($"  [{exists}] {path}");
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
                foreach (var path in manualPaths.Where(File.Exists))
                {
                    var normalizedPath = SourcePathNormalizer.Normalize(path);
                    var content = await File.ReadAllTextAsync(normalizedPath, context.GetCancellationToken());
                    var chunks = MarkdownChunker.ChunkManual(normalizedPath, content, mode);
                    Console.WriteLine($"  {Path.GetFileName(path)}: {chunks.Count} chunks");
                }

                return;
            }

            using var client = new OllamaClient(settings);
            var ingest = new CorpusIngestService(client);
            var summary = await ingest.IngestSharedAsync(
                sqlitePath,
                manualPaths,
                mode,
                clear,
                context.GetCancellationToken());

            if (summary.MissingFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    "Missing manual files: " + string.Join(", ", summary.MissingFiles));
            }

            if (summary.ClearedExisting)
            {
                Console.WriteLine("Index cleared before ingest.");
            }

            Console.WriteLine($"Embedded chunks:  {summary.ChunkCount}");
            Console.WriteLine($"Stored total:     {summary.TotalStored}");
            Console.WriteLine("Shared ingest complete.");
        });

        return command;
    }
}
