namespace SpaceAge.PlayerAgent.Rag;

public static class VectorRetriever
{
    public static IReadOnlyList<RetrievalResult> Retrieve(
        IReadOnlyList<StoredChunk> corpus,
        float[] queryEmbedding,
        int topK,
        string? verbFilter = null)
    {
        if (topK <= 0)
        {
            return [];
        }

        var scored = corpus
            .Select(chunk => new RetrievalResult(chunk, VectorMath.CosineSimilarity(queryEmbedding, chunk.Embedding)))
            .ToList();

        if (string.IsNullOrWhiteSpace(verbFilter))
        {
            return scored
                .OrderByDescending(result => result.Score)
                .Take(topK)
                .ToList();
        }

        var normalizedVerb = verbFilter.Trim().ToUpperInvariant();
        var verbMatches = scored
            .Where(result => string.Equals(result.Chunk.Chunk.Metadata.Verb, normalizedVerb, StringComparison.Ordinal))
            .OrderByDescending(result => result.Score)
            .ToList();

        var remainder = scored
            .Where(result => !string.Equals(result.Chunk.Chunk.Metadata.Verb, normalizedVerb, StringComparison.Ordinal))
            .OrderByDescending(result => result.Score);

        var combined = new List<RetrievalResult>();
        combined.AddRange(verbMatches);
        foreach (var result in remainder)
        {
            if (combined.Count >= topK)
            {
                break;
            }

            combined.Add(result);
        }

        return combined.Take(topK).ToList();
    }
}
