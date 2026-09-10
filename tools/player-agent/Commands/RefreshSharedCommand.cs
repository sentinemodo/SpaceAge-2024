using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Commands;

internal static class RefreshSharedCommand
{
    public static Command Create()
    {
        var modeOption = new Option<string>("--mode")
        {
            Description = "Shared index mode to rebuild: test, campaign, or both.",
            IsRequired = true,
        };

        var spotCheckVerbOption = new Option<string>("--spot-check-verb")
        {
            Description = "Verb heading to spot-check after ingest (default MOVE).",
        };

        var spotCheckTechOption = new Option<string?>("--spot-check-tech")
        {
            Description = "Optional catalog tech id to spot-check after ingest.",
        };

        var noteRunOption = new Option<string?>("--note-run")
        {
            Description = "Append a RAG rebuild note to play/runs/<id>/README.md.",
        };

        var catalogRevisionOption = new Option<string?>("--catalog-revision")
        {
            Description = "Optional catalog revision label for run notes (e.g. data.xml commit).",
        };

        var skipIngestOption = new Option<bool>("--skip-ingest")
        {
            Description = "Regenerate allowlist and spot-check only; do not call ingest-shared.",
        };

        var skipAllowlistOption = new Option<bool>("--skip-allowlist")
        {
            Description = "Skip verb allowlist regeneration.",
        };

        var skipSpotCheckOption = new Option<bool>("--skip-spot-check")
        {
            Description = "Skip post-ingest retrieve spot-checks.",
        };

        var command = new Command(
            "refresh-shared",
            "Rebuild shared RAG after engine/catalog/manual updates (Phase 5).");
        command.AddOption(modeOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.ClearOption);
        command.AddOption(spotCheckVerbOption);
        command.AddOption(spotCheckTechOption);
        command.AddOption(noteRunOption);
        command.AddOption(catalogRevisionOption);
        command.AddOption(skipIngestOption);
        command.AddOption(skipAllowlistOption);
        command.AddOption(skipSpotCheckOption);

        command.SetHandler(async (context) =>
        {
            var modeValue = context.ParseResult.GetValueForOption(modeOption)
                ?? throw new InvalidOperationException("--mode is required.");
            var modes = SharedIndexRefreshPlanner.ParseModes(modeValue);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var clear = context.ParseResult.GetValueForOption(CommandHelpers.ClearOption);
            var spotCheckVerb = context.ParseResult.GetValueForOption(spotCheckVerbOption) ?? "MOVE";
            var spotCheckTech = context.ParseResult.GetValueForOption(spotCheckTechOption);
            var noteRunId = context.ParseResult.GetValueForOption(noteRunOption);
            var catalogRevision = context.ParseResult.GetValueForOption(catalogRevisionOption);
            var skipIngest = context.ParseResult.GetValueForOption(skipIngestOption);
            var skipAllowlist = context.ParseResult.GetValueForOption(skipAllowlistOption);
            var skipSpotCheck = context.ParseResult.GetValueForOption(skipSpotCheckOption);

            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);
            var engineVersion = SharedIndexRefreshPlanner.TryReadEngineVersion(repoRoot);
            var refreshedAtUtc = DateTime.UtcNow.ToString("O");
            var spotChecks = SharedIndexRefreshPlanner.BuildSpotCheckQueries(spotCheckVerb, spotCheckTech);

            Console.WriteLine("Phase 5 checklist:");
            Console.WriteLine("  [ ] Player manuals updated for the change set (run /player docs-only first)");
            Console.WriteLine("  [ ] ingest-shared for affected mode(s)");
            Console.WriteLine("  [ ] Verb allowlist regenerated");
            Console.WriteLine("  [ ] Spot-check retrieve for changed headings");
            Console.WriteLine();

            if (engineVersion is not null)
            {
                Console.WriteLine($"Engine version:   {engineVersion}");
            }

            Console.WriteLine($"Modes:            {string.Join(", ", modes.Select(PlayModeParser.ToCliValue))}");
            Console.WriteLine($"Spot-check verb:  {spotCheckVerb}");
            if (!string.IsNullOrWhiteSpace(spotCheckTech))
            {
                Console.WriteLine($"Spot-check tech:  {spotCheckTech}");
            }

            if (!skipAllowlist)
            {
                var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
                if (!File.Exists(rulesPath))
                {
                    throw new InvalidOperationException($"Rules file not found: {rulesPath}");
                }

                var allowlistPath = OrderVerbAllowlistExporter.DefaultOutputPath(repoRoot);
                var document = OrderVerbAllowlistExporter.BuildFromRulesFile(
                    rulesPath,
                    "player/rules.md");
                if (dryRun)
                {
                    Console.WriteLine($"Dry run allowlist: {document.Verbs.Count} verbs -> {allowlistPath}");
                }
                else
                {
                    OrderVerbAllowlistExporter.WriteJson(allowlistPath, document);
                    Console.WriteLine($"Allowlist:        {allowlistPath} ({document.Verbs.Count} verbs)");
                }
            }

            foreach (var mode in modes)
            {
                Console.WriteLine();
                Console.WriteLine($"=== Mode {PlayModeParser.ToCliValue(mode)} ===");

                var sharedIndexDir = RepoPaths.SharedIndexDirectory(settings.IndexDirectory, mode);
                Directory.CreateDirectory(sharedIndexDir);
                var sqlitePath = VectorIndexPaths.SharedSqlitePath(sharedIndexDir);
                var manualPaths = RepoPaths.SharedManualPaths(repoRoot, mode);

                Console.WriteLine($"Shared index dir: {sharedIndexDir}");
                Console.WriteLine("Manual sources:");
                foreach (var path in manualPaths)
                {
                    var exists = File.Exists(path) ? "ok" : "MISSING";
                    Console.WriteLine($"  [{exists}] {path}");
                }

                var missingManuals = manualPaths.Where(path => !File.Exists(path)).ToList();
                if (missingManuals.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Missing manual files: " + string.Join(", ", missingManuals));
                }

                if (!skipIngest)
                {
                    if (dryRun)
                    {
                        Console.WriteLine("Dry run ingest: chunking only; no embed calls.");
                        foreach (var path in manualPaths)
                        {
                            var normalizedPath = SourcePathNormalizer.Normalize(path);
                            var content = await File.ReadAllTextAsync(normalizedPath, context.GetCancellationToken());
                            var chunks = MarkdownChunker.ChunkManual(normalizedPath, content, mode);
                            Console.WriteLine($"  {Path.GetFileName(path)}: {chunks.Count} chunks");
                        }
                    }
                    else
                    {
                        using var client = new OllamaClient(settings);
                        var ingest = new CorpusIngestService(client);
                        var summary = await ingest.IngestSharedAsync(
                            sqlitePath,
                            manualPaths,
                            mode,
                            clear,
                            context.GetCancellationToken());

                        if (summary.ClearedExisting)
                        {
                            Console.WriteLine("Index cleared before ingest.");
                        }

                        Console.WriteLine($"Embedded chunks:  {summary.ChunkCount}");
                        Console.WriteLine($"Stored total:     {summary.TotalStored}");
                    }
                }

                if (!skipSpotCheck && !dryRun)
                {
                    using var client = new OllamaClient(settings);
                    var results = await SpotCheckRetriever.RunAsync(
                        client,
                        sqlitePath,
                        spotChecks,
                        topK: 4,
                        context.GetCancellationToken());

                    Console.WriteLine("Spot-check retrieve:");
                    foreach (var result in results)
                    {
                        Console.WriteLine($"  [{result.Query.Label}] query={result.Query.Query}");
                        if (result.Hits.Count == 0)
                        {
                            throw new InvalidOperationException(
                                $"Spot-check '{result.Query.Label}' returned no hits for mode {PlayModeParser.ToCliValue(mode)}.");
                        }

                        foreach (var hit in result.Hits.Take(2))
                        {
                            var metadata = hit.Chunk.Chunk.Metadata;
                            Console.WriteLine(
                                $"    [{hit.Score:F3}] doc={metadata.Doc} verb={metadata.Verb ?? "-"} heading={metadata.Heading ?? "-"}");
                        }
                    }
                }
                else if (!skipSpotCheck)
                {
                    Console.WriteLine("Dry run spot-check: skipped (requires live index + Ollama).");
                }

                if (!string.IsNullOrWhiteSpace(noteRunId) && !dryRun)
                {
                    var note = SharedIndexRefreshPlanner.BuildRevisionNote(
                        mode,
                        engineVersion,
                        catalogRevision,
                        refreshedAtUtc);
                    var runReadmePath = Path.Combine(repoRoot, "play", "runs", noteRunId, "README.md");
                    SharedIndexRefreshPlanner.AppendRunRevisionNote(runReadmePath, note);
                    Console.WriteLine($"Run note:         {runReadmePath}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("refresh-shared complete.");
        });

        return command;
    }
}
