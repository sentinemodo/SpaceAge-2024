using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Rag;

public static class SpotCheckRetriever
{
    public static async Task<IReadOnlyList<SpotCheckResult>> RunAsync(
        IOllamaClient client,
        string sqlitePath,
        IReadOnlyList<SpotCheckQuery> queries,
        int topK,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(sqlitePath))
        {
            throw new InvalidOperationException($"Index not found: {sqlitePath}. Run ingest-shared first.");
        }

        using var store = new SqliteVectorStore(sqlitePath);
        var corpus = store.ListAll();
        var results = new List<SpotCheckResult>();

        foreach (var query in queries)
        {
            var queryEmbedding = await client.EmbedAsync(query.Query, cancellationToken);
            var hits = VectorRetriever.Retrieve(corpus, queryEmbedding, topK, query.VerbFilter);
            results.Add(new SpotCheckResult(query, hits));
        }

        return results;
    }
}

public sealed record SpotCheckResult(SpotCheckQuery Query, IReadOnlyList<RetrievalResult> Hits);
