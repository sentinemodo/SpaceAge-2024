namespace SpaceAge.PlayerAgent.Rag;

public static class VectorRetriever
{
    public static IReadOnlyList<RetrievalResult> Retrieve(
        IReadOnlyList<StoredChunk> corpus,
        float[] queryEmbedding,
        int topK,
        string? verbFilter = null,
        IReadOnlyList<string>? verbBoost = null)
    {
        if (topK <= 0)
        {
            return [];
        }

        var scored = corpus
            .Select(chunk => new RetrievalResult(chunk, VectorMath.CosineSimilarity(queryEmbedding, chunk.Embedding)))
            .ToList();

        var boostSet = BuildBoostSet(verbFilter, verbBoost);
        if (boostSet.Count == 0)
        {
            return scored
                .OrderByDescending(result => result.Score)
                .Take(topK)
                .ToList();
        }

        var verbMatches = scored
            .Where(result => MatchesBoost(result.Chunk.Chunk.Metadata.Verb, boostSet))
            .OrderByDescending(result => result.Score)
            .ToList();

        var remainder = scored
            .Where(result => !MatchesBoost(result.Chunk.Chunk.Metadata.Verb, boostSet))
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

    private static HashSet<string> BuildBoostSet(string? verbFilter, IReadOnlyList<string>? verbBoost)
    {
        var boostSet = new HashSet<string>(StringComparer.Ordinal);
        if (!string.IsNullOrWhiteSpace(verbFilter))
        {
            boostSet.Add(verbFilter.Trim().ToUpperInvariant());
        }

        if (verbBoost is not null)
        {
            foreach (var verb in verbBoost)
            {
                if (!string.IsNullOrWhiteSpace(verb))
                {
                    boostSet.Add(verb.Trim().ToUpperInvariant());
                }
            }
        }

        return boostSet;
    }

    private static bool MatchesBoost(string? verb, HashSet<string> boostSet) =>
        !string.IsNullOrWhiteSpace(verb) && boostSet.Contains(verb.Trim().ToUpperInvariant());
}
