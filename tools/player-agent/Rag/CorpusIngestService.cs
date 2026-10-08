using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Rag;

public sealed class CorpusIngestService
{
    private readonly IOllamaClient _client;

    public CorpusIngestService(IOllamaClient client)
    {
        _client = client;
    }

    public async Task<IngestSummary> IngestSharedAsync(
        string sqlitePath,
        IReadOnlyList<string> manualPaths,
        PlayMode mode,
        bool clearIndex = false,
        CancellationToken cancellationToken = default)
    {
        using var store = new SqliteVectorStore(sqlitePath);
        var summary = new IngestSummary();
        if (clearIndex)
        {
            store.ClearAll();
            summary.ClearedExisting = true;
        }

        foreach (var manualPath in manualPaths)
        {
            if (!File.Exists(manualPath))
            {
                summary.MissingFiles.Add(manualPath);
                continue;
            }

            var normalizedPath = SourcePathNormalizer.Normalize(manualPath);
            var content = await File.ReadAllTextAsync(normalizedPath, cancellationToken);
            var chunks = MarkdownChunker.ChunkManual(normalizedPath, content, mode);
            var embedded = await EmbedChunksAsync(chunks, cancellationToken);
            store.ReplaceSource(normalizedPath, embedded);
            summary.Sources.Add(normalizedPath);
            summary.ChunkCount += embedded.Count;
        }

        summary.TotalStored = store.Count();
        return summary;
    }

    public async Task<IngestSummary> IngestFactionAsync(
        string sqlitePath,
        string factionDir,
        bool clearIndex = false,
        CancellationToken cancellationToken = default)
    {
        using var store = new SqliteVectorStore(sqlitePath);
        var summary = new IngestSummary();
        if (clearIndex)
        {
            store.ClearAll();
            summary.ClearedExisting = true;
        }

        foreach (var reportPath in FactionCorpusPaths.ReportPaths(factionDir))
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                reportPath,
                MarkdownChunker.ChunkReport,
                cancellationToken);
            summary.Sources.Add(SourcePathNormalizer.Normalize(reportPath));
        }

        var factionId = InferFactionIdFromFolder(factionDir);
        foreach (var storyPath in StoryFileNaming.AllStoryPaths(factionDir, factionId))
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                storyPath,
                MarkdownChunker.ChunkStory,
                cancellationToken);
            summary.Sources.Add(SourcePathNormalizer.Normalize(storyPath));
        }

        var knowledgePath = FactionCorpusPaths.KnowledgePath(factionDir);
        if (knowledgePath is not null)
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                knowledgePath,
                MarkdownChunker.ChunkStory,
                cancellationToken);
            summary.Sources.Add(SourcePathNormalizer.Normalize(knowledgePath));
        }

        foreach (var orderPath in FactionCorpusPaths.OrderPaths(factionDir))
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                orderPath,
                MarkdownChunker.ChunkOrderFile,
                cancellationToken);
            summary.Sources.Add(SourcePathNormalizer.Normalize(orderPath));
        }

        if (summary.Sources.Count == 0)
        {
            summary.MissingFiles.Add(factionDir);
        }

        summary.TotalStored = store.Count();
        return summary;
    }

    public async Task<IngestSummary> IngestFactionIncrementalAsync(
        string sqlitePath,
        string factionDir,
        FactionIngestOptions options,
        CancellationToken cancellationToken = default)
    {
        var factionId = InferFactionIdFromFolder(factionDir);
        var plan = FactionIngestPlanner.BuildPlan(factionDir, factionId, options);
        using var store = new SqliteVectorStore(sqlitePath);
        var summary = new IngestSummary();

        if (options.ClearIndex)
        {
            store.ClearAll();
            summary.ClearedExisting = true;
        }

        foreach (var sourcePath in plan.IngestPaths)
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                sourcePath,
                SelectChunkFactory(sourcePath),
                cancellationToken);
            summary.Sources.Add(SourcePathNormalizer.Normalize(sourcePath));
        }

        if (plan.PruneIndexedFactionCorpus)
        {
            var stale = FactionIngestPlanner.FindStaleSources(
                store.ListSourcePaths(),
                factionDir,
                plan.KeepSourcePaths);
            if (stale.Count > 0)
            {
                store.DeleteSources(stale);
                summary.RemovedSources.AddRange(stale);
            }
        }

        summary.TotalStored = store.Count();
        return summary;
    }

    private static Func<string, string, IReadOnlyList<TextChunk>> SelectChunkFactory(string sourcePath)
    {
        var fileName = Path.GetFileName(sourcePath);
        if (StoryFileNaming.IsStoryFileName(fileName)
            || sourcePath.EndsWith("knowledge.md", StringComparison.OrdinalIgnoreCase))
        {
            return MarkdownChunker.ChunkStory;
        }

        if (sourcePath.Contains("report", StringComparison.OrdinalIgnoreCase))
        {
            return MarkdownChunker.ChunkReport;
        }

        return MarkdownChunker.ChunkOrderFile;
    }

    private async Task<int> IngestFileAsync(
        SqliteVectorStore store,
        string sourcePath,
        Func<string, string, IReadOnlyList<TextChunk>> chunkFactory,
        CancellationToken cancellationToken)
    {
        var normalizedPath = SourcePathNormalizer.Normalize(sourcePath);
        var content = await File.ReadAllTextAsync(normalizedPath, cancellationToken);
        var chunks = chunkFactory(normalizedPath, content);
        var embedded = await EmbedChunksAsync(chunks, cancellationToken);
        store.ReplaceSource(normalizedPath, embedded);
        return embedded.Count;
    }

    private async Task<List<(TextChunk Chunk, float[] Embedding)>> EmbedChunksAsync(
        IReadOnlyList<TextChunk> chunks,
        CancellationToken cancellationToken)
    {
        var embedded = new List<(TextChunk, float[])>(chunks.Count);
        foreach (var chunk in chunks)
        {
            var embedding = await _client.EmbedAsync(chunk.Content, cancellationToken);
            embedded.Add((chunk, embedding));
        }

        return embedded;
    }

    private static int InferFactionIdFromFolder(string factionDir)
    {
        var name = Path.GetFileName(factionDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (int.TryParse(name, out var id))
        {
            return id;
        }

        throw new InvalidOperationException($"Could not infer faction id from folder name: {factionDir}");
    }
}

public sealed class IngestSummary
{
    public List<string> Sources { get; } = [];
    public List<string> RemovedSources { get; } = [];
    public List<string> MissingFiles { get; } = [];
    public int ChunkCount { get; set; }
    public int TotalStored { get; set; }
    public bool ClearedExisting { get; set; }
}
