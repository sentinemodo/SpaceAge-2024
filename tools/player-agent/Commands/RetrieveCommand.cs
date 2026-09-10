using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class RetrieveCommand
{
    public static Command Create()
    {
        var command = new Command("retrieve", "Search a RAG index (Phase 2 dev helper).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.IndexOption);
        command.AddOption(CommandHelpers.QueryOption);
        command.AddOption(CommandHelpers.VerbOption);
        command.AddOption(CommandHelpers.TopOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);

        command.SetHandler(async (context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var indexKind = context.ParseResult.GetValueForOption(CommandHelpers.IndexOption)
                ?? throw new InvalidOperationException("--index is required and must be shared or faction.");
            var query = context.ParseResult.GetValueForOption(CommandHelpers.QueryOption)
                ?? throw new InvalidOperationException("--query is required.");
            var verb = context.ParseResult.GetValueForOption(CommandHelpers.VerbOption);
            var topK = context.ParseResult.GetValueForOption(CommandHelpers.TopOption) ?? 6;
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var factionId = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption);

            var sqlitePath = ResolveSqlitePath(settings.IndexDirectory, mode, indexKind, runId, factionId);
            if (!File.Exists(sqlitePath))
            {
                throw new InvalidOperationException($"Index not found: {sqlitePath}. Run ingest first.");
            }

            await CommandHelpers.RunWithInferenceAsync(
                settings,
                context,
                command: "retrieve",
                runId,
                factionId is null ? null : [factionId.Value],
                async (client, cancellationToken) =>
                {
                    using var store = new SqliteVectorStore(sqlitePath);
                    var queryEmbedding = await client.EmbedAsync(query, cancellationToken);
                    var results = VectorRetriever.Retrieve(store.ListAll(), queryEmbedding, topK, verb);

                    Console.WriteLine($"Index:  {sqlitePath}");
                    Console.WriteLine($"Query:  {query}");
                    if (!string.IsNullOrWhiteSpace(verb))
                    {
                        Console.WriteLine($"Verb:   {verb}");
                    }

                    Console.WriteLine($"Hits:   {results.Count}");
                    foreach (var result in results)
                    {
                        var metadata = result.Chunk.Chunk.Metadata;
                        Console.WriteLine();
                        Console.WriteLine(
                            $"[{result.Score:F3}] doc={metadata.Doc} verb={metadata.Verb ?? "-"} heading={metadata.Heading ?? "-"}");
                        Console.WriteLine($"source: {metadata.SourcePath}");
                        Console.WriteLine(result.Chunk.Chunk.Content.Length > 240
                            ? result.Chunk.Chunk.Content[..240] + "…"
                            : result.Chunk.Chunk.Content);
                    }
                });
        });

        return command;
    }

    private static string ResolveSqlitePath(
        string indexRoot,
        PlayMode mode,
        string indexKind,
        string? runId,
        int? factionId)
    {
        switch (indexKind.Trim().ToLowerInvariant())
        {
            case "shared":
                return VectorIndexPaths.SharedSqlitePath(RepoPaths.SharedIndexDirectory(indexRoot, mode));
            case "faction":
                if (string.IsNullOrWhiteSpace(runId) || factionId is null)
                {
                    throw new InvalidOperationException("--run and --faction are required when --index faction.");
                }

                if (factionId is < 2 or > 11)
                {
                    throw new InvalidOperationException("--faction must be between 2 and 11.");
                }

                return VectorIndexPaths.FactionSqlitePath(
                    RepoPaths.FactionIndexDirectory(indexRoot, runId, factionId.Value));
            default:
                throw new InvalidOperationException("--index must be shared or faction.");
        }
    }
}
