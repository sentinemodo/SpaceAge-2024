using System.CommandLine;
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

        command.SetHandler((context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);

            var sharedIndexDir = RepoPaths.SharedIndexDirectory(settings.IndexDirectory, mode);
            Directory.CreateDirectory(sharedIndexDir);
            var sqlitePath = VectorIndexPaths.SharedSqlitePath(sharedIndexDir);

            Console.WriteLine($"Mode:             {PlayModeParser.ToCliValue(mode)}");
            Console.WriteLine($"Shared index dir: {sharedIndexDir}");
            Console.WriteLine($"SQLite path:      {sqlitePath}");
            Console.WriteLine("Manual sources:");

            foreach (var path in RepoPaths.SharedManualPaths(repoRoot, mode))
            {
                var exists = File.Exists(path) ? "ok" : "MISSING";
                Console.WriteLine($"  [{exists}] {path}");
            }

            if (dryRun)
            {
                Console.WriteLine("Dry run: skipping embed calls (Phase 2 will implement ingest).");
                return;
            }

            Console.WriteLine("Phase 0 stub: ingest-shared is not implemented until Phase 2.");
        });

        return command;
    }
}
