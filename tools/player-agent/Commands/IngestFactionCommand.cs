using System.CommandLine;
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

        command.SetHandler((context) =>
        {
            _ = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
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

            Console.WriteLine($"Faction folder:   {factionDir}");
            Console.WriteLine($"Faction index:    {indexDir}");
            Console.WriteLine($"SQLite path:      {VectorIndexPaths.FactionSqlitePath(indexDir)}");

            if (!Directory.Exists(factionDir))
            {
                throw new InvalidOperationException($"Faction folder not found: {factionDir}");
            }

            if (dryRun)
            {
                Console.WriteLine("Dry run: skipping embed calls (Phase 2 will implement ingest).");
                return;
            }

            Console.WriteLine("Phase 0 stub: ingest-faction is not implemented until Phase 2.");
        });

        return command;
    }
}
