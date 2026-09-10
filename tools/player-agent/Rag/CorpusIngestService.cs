using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Rag;

public sealed class CorpusIngestService
{
    private readonly OllamaClient _client;

    public CorpusIngestService(OllamaClient client)
    {
        _client = client;
    }

    public async Task<IngestSummary> IngestSharedAsync(
        string sqlitePath,
        IReadOnlyList<string> manualPaths,
        PlayMode mode,
        CancellationToken cancellationToken = default)
    {
        using var store = new SqliteVectorStore(sqlitePath);
        var summary = new IngestSummary();

        foreach (var manualPath in manualPaths)
        {
            if (!File.Exists(manualPath))
            {
                summary.MissingFiles.Add(manualPath);
                continue;
            }

            var content = await File.ReadAllTextAsync(manualPath, cancellationToken);
            var chunks = MarkdownChunker.ChunkManual(manualPath, content, mode);
            var embedded = await EmbedChunksAsync(chunks, cancellationToken);
            store.ReplaceSource(manualPath, embedded);
            summary.Sources.Add(manualPath);
            summary.ChunkCount += embedded.Count;
        }

        summary.TotalStored = store.Count();
        return summary;
    }

    public async Task<IngestSummary> IngestFactionAsync(
        string sqlitePath,
        string factionDir,
        CancellationToken cancellationToken = default)
    {
        using var store = new SqliteVectorStore(sqlitePath);
        var summary = new IngestSummary();

        foreach (var reportPath in FactionCorpusPaths.ReportPaths(factionDir))
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                reportPath,
                content => MarkdownChunker.ChunkReport(reportPath, content),
                cancellationToken);
            summary.Sources.Add(reportPath);
        }

        var storyPath = FactionCorpusPaths.StoryPath(factionDir);
        if (storyPath is not null)
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                storyPath,
                content => MarkdownChunker.ChunkStory(storyPath, content),
                cancellationToken);
            summary.Sources.Add(storyPath);
        }

        foreach (var orderPath in FactionCorpusPaths.OrderPaths(factionDir))
        {
            summary.ChunkCount += await IngestFileAsync(
                store,
                orderPath,
                content => MarkdownChunker.ChunkOrderFile(orderPath, content),
                cancellationToken);
            summary.Sources.Add(orderPath);
        }

        if (summary.Sources.Count == 0)
        {
            summary.MissingFiles.Add(factionDir);
        }

        summary.TotalStored = store.Count();
        return summary;
    }

    private async Task<int> IngestFileAsync(
        SqliteVectorStore store,
        string sourcePath,
        Func<string, IReadOnlyList<TextChunk>> chunkFactory,
        CancellationToken cancellationToken)
    {
        var content = await File.ReadAllTextAsync(sourcePath, cancellationToken);
        var chunks = chunkFactory(content);
        var embedded = await EmbedChunksAsync(chunks, cancellationToken);
        store.ReplaceSource(sourcePath, embedded);
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
}

public sealed class IngestSummary
{
    public List<string> Sources { get; } = [];
    public List<string> MissingFiles { get; } = [];
    public int ChunkCount { get; set; }
    public int TotalStored { get; set; }
}
