using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Rag;

public sealed class FactionIngestPlan
{
    public required IReadOnlyList<string> IngestPaths { get; init; }
    public required IReadOnlyList<string> KeepSourcePaths { get; init; }
    public bool PruneIndexedFactionCorpus { get; init; }
}

public static class FactionIngestPlanner
{
    public static FactionIngestPlan BuildPlan(string factionDir, FactionIngestOptions options)
    {
        if (options.StoryOnly)
        {
            var storyPath = FactionCorpusPaths.StoryPath(factionDir)
                ?? throw new InvalidOperationException(
                    $"story.md not found under {factionDir}. Create or update story before --story-only ingest.");

            var normalizedStory = SourcePathNormalizer.Normalize(storyPath);
            return new FactionIngestPlan
            {
                IngestPaths = [storyPath],
                KeepSourcePaths = [normalizedStory],
                PruneIndexedFactionCorpus = false,
            };
        }

        if (options.FullCorpus)
        {
            var paths = FactionCorpusPaths.ReportPaths(factionDir)
                .Concat(FactionCorpusPaths.StoryPath(factionDir) is { } story ? [story] : [])
                .Concat(FactionCorpusPaths.OrderPaths(factionDir))
                .ToList();

            if (paths.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No report, story, or order files found under {factionDir}");
            }

            return new FactionIngestPlan
            {
                IngestPaths = paths,
                KeepSourcePaths = paths.Select(SourcePathNormalizer.Normalize).ToList(),
                PruneIndexedFactionCorpus = false,
            };
        }

        var reportPath = ResolveReportPath(factionDir, options.ReportPath);
        var ingestPaths = new List<string> { reportPath };
        var keepSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            SourcePathNormalizer.Normalize(reportPath),
        };

        if (FactionCorpusPaths.StoryPath(factionDir) is { } factionStory)
        {
            ingestPaths.Add(factionStory);
            keepSources.Add(SourcePathNormalizer.Normalize(factionStory));
        }

        foreach (var orderPath in FactionCorpusPaths.OrderPathsWithinTurnWindow(factionDir, options.MaxOrderTurns))
        {
            ingestPaths.Add(orderPath);
            keepSources.Add(SourcePathNormalizer.Normalize(orderPath));
        }

        return new FactionIngestPlan
        {
            IngestPaths = ingestPaths,
            KeepSourcePaths = keepSources.ToList(),
            PruneIndexedFactionCorpus = true,
        };
    }

    public static IReadOnlyList<string> FindStaleSources(
        IReadOnlyList<string> indexedSources,
        string factionDir,
        IReadOnlyCollection<string> keepSources)
    {
        var normalizedFactionDir = SourcePathNormalizer.Normalize(factionDir);
        var keep = new HashSet<string>(keepSources, StringComparer.OrdinalIgnoreCase);

        return indexedSources
            .Where(path =>
                !keep.Contains(path)
                && path.StartsWith(normalizedFactionDir, StringComparison.OrdinalIgnoreCase)
                && FactionCorpusPaths.IsFactionCorpusFile(path))
            .ToList();
    }

    private static string ResolveReportPath(string factionDir, string? reportPathOverride)
    {
        if (!string.IsNullOrWhiteSpace(reportPathOverride))
        {
            var fullPath = Path.GetFullPath(reportPathOverride);
            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException($"Report file not found: {fullPath}");
            }

            return fullPath;
        }

        return FactionCorpusPaths.LatestReportPath(factionDir)
            ?? throw new InvalidOperationException(
                $"No report*.txt found under {factionDir}. Run isolate or pass --report.");
    }
}
