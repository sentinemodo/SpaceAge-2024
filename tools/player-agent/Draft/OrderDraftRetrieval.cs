using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static class OrderDraftRetrieval
{
    public static IReadOnlyList<string> BuildSupplementaryQueries(
        string? personaPreference,
        string? reportText)
    {
        var queries = new List<string>
        {
            "GRANT item iron titani terran oil to numeric modulestack id grant technology mcored msrvtm armcbt",
            "MOVE ground region exits grant cell move R tactic destroy has tanks -get terran oil food",
            "produce energy wind wndtrb coal cplant iminng hcdril sdrill one use only grant resources",
        };

        if (string.Equals(personaPreference, "economic", StringComparison.OrdinalIgnoreCase))
        {
            queries.Add("economic bootstrap mcored msrvtm moblab cdrill deactivate wndtrb sell food cargob");
        }
        else if (string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase))
        {
            queries.Add("military armcbt grndtr two tanks declare faction 14 enemy grant terran hq");
        }
        else if (string.Equals(personaPreference, "researcher", StringComparison.OrdinalIgnoreCase))
        {
            queries.Add("researcher msrvtm moblab research anomaly grant iron silici");
        }

        if (!string.IsNullOrWhiteSpace(reportText)
            && ReportStackCatalog.GrantUsesWindPowerPlant(reportText))
        {
            queries.Add("Anvil wind powerplant wnplnt produce energy iminng not hcdril wndtrb expand");
        }

        return queries;
    }

    public static async Task<IReadOnlyList<RetrievalResult>> RetrieveAsync(
        IOllamaClient client,
        PlayerAgentSettings settings,
        OrderDraftRequest request,
        string primaryQuery,
        IReadOnlyList<string> verbBoost,
        string? personaPreference,
        CancellationToken cancellationToken)
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var sharedPath = VectorIndexPaths.SharedSqlitePath(
            RepoPaths.SharedIndexDirectory(settings.IndexDirectory, request.Mode));
        var extraQueries = BuildSupplementaryQueries(
            personaPreference,
            await ReadReportSnippetAsync(request, cancellationToken));

        var allQueries = new List<string> { primaryQuery };
        allQueries.AddRange(extraQueries);

        var merged = new Dictionary<string, RetrievalResult>(StringComparer.OrdinalIgnoreCase);
        var perQueryTop = Math.Max(2, (request.TopK + allQueries.Count - 1) / allQueries.Count);

        foreach (var query in allQueries)
        {
            var batch = await RetrieveSingleQueryAsync(
                client,
                settings,
                request,
                sharedPath,
                query,
                verbBoost,
                perQueryTop,
                cancellationToken);
            foreach (var hit in batch)
            {
                var key = hit.Chunk.Chunk.Metadata.SourcePath + "|" + hit.Chunk.Chunk.Metadata.Heading;
                if (!merged.TryGetValue(key, out var existing) || hit.Score > existing.Score)
                {
                    merged[key] = hit;
                }
            }
        }

        return merged.Values
            .OrderByDescending(result => result.Score)
            .Take(request.TopK)
            .ToList();
    }

    private static async Task<IReadOnlyList<RetrievalResult>> RetrieveSingleQueryAsync(
        IOllamaClient client,
        PlayerAgentSettings settings,
        OrderDraftRequest request,
        string sharedPath,
        string query,
        IReadOnlyList<string> verbBoost,
        int topK,
        CancellationToken cancellationToken)
    {
        var results = new List<RetrievalResult>();
        var sharedTop = Math.Max(1, topK / 2);
        var queryEmbedding = await client.EmbedAsync(query, cancellationToken);

        if (File.Exists(sharedPath))
        {
            using var sharedStore = new SqliteVectorStore(sharedPath);
            results.AddRange(VectorRetriever.Retrieve(sharedStore.ListAll(), queryEmbedding, sharedTop, verbBoost: verbBoost));
        }

        if (request.RunId is not null)
        {
            var factionPath = VectorIndexPaths.FactionSqlitePath(
                RepoPaths.FactionIndexDirectory(settings.IndexDirectory, request.RunId, request.FactionId));
            if (File.Exists(factionPath))
            {
                using var factionStore = new SqliteVectorStore(factionPath);
                var factionTop = Math.Max(1, topK - results.Count);
                results.AddRange(VectorRetriever.Retrieve(factionStore.ListAll(), queryEmbedding, factionTop, verbBoost: verbBoost));
            }
        }

        return results
            .OrderByDescending(result => result.Score)
            .Take(topK)
            .ToList();
    }

    private static async Task<string?> ReadReportSnippetAsync(OrderDraftRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReportPath) || !File.Exists(request.ReportPath))
        {
            return null;
        }

        var text = await File.ReadAllTextAsync(request.ReportPath, cancellationToken);
        return text.Length <= 4000 ? text : text[..4000];
    }
}
