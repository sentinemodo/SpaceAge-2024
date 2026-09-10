namespace SpaceAge.PlayerAgent.Rag;

public sealed record SpotCheckQuery(string Label, string Query, string? VerbFilter);

public static class SharedIndexRefreshPlanner
{
    public static IReadOnlyList<PlayMode> ParseModes(string modeValue)
    {
        if (string.Equals(modeValue, "both", StringComparison.OrdinalIgnoreCase))
        {
            return [PlayMode.Test, PlayMode.Campaign];
        }

        if (!PlayModeParser.TryParse(modeValue, out var mode))
        {
            throw new InvalidOperationException("--mode is required and must be test, campaign, or both.");
        }

        return [mode];
    }

    public static IReadOnlyList<SpotCheckQuery> BuildSpotCheckQueries(string verb, string? techId)
    {
        var queries = new List<SpotCheckQuery>
        {
            new(
                "verb",
                $"{verb} order syntax and parameters",
                verb.Trim().ToUpperInvariant()),
        };

        if (!string.IsNullOrWhiteSpace(techId))
        {
            queries.Add(new SpotCheckQuery(
                "tech",
                $"technology {techId.Trim()} modules and requirements",
                null));
        }

        return queries;
    }

    public static string BuildRevisionNote(
        PlayMode mode,
        string? engineVersion,
        string? catalogRevision,
        string refreshedAtUtc)
    {
        var indexName = $"shared-{PlayModeParser.ToCliValue(mode)}";
        var date = refreshedAtUtc.Length >= 10 ? refreshedAtUtc[..10] : refreshedAtUtc;
        var versionParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(engineVersion))
        {
            versionParts.Add($"engine {engineVersion.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(catalogRevision))
        {
            versionParts.Add($"catalog {catalogRevision.Trim()}");
        }

        var versionText = versionParts.Count > 0
            ? $" ({string.Join(", ", versionParts)})"
            : string.Empty;

        return $"- Rebuilt `{indexName}` shared RAG index on {date}{versionText}.";
    }

    public static string? TryReadEngineVersion(string repoRoot)
    {
        var programPath = Path.Combine(repoRoot, "Game", "Program.cs");
        if (!File.Exists(programPath))
        {
            return null;
        }

        foreach (var line in File.ReadAllLines(programPath))
        {
            const string prefix = "public const string EngineVersion = \"";
            var index = line.IndexOf(prefix, StringComparison.Ordinal);
            if (index < 0)
            {
                continue;
            }

            var start = index + prefix.Length;
            var end = line.IndexOf('"', start);
            if (end > start)
            {
                return line[start..end];
            }
        }

        return null;
    }

    public static void AppendRunRevisionNote(string runReadmePath, string noteLine)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(runReadmePath)!);
        var sectionHeader = "## RAG shared index";
        var sectionText = sectionHeader + Environment.NewLine + Environment.NewLine + noteLine + Environment.NewLine;

        if (!File.Exists(runReadmePath))
        {
            File.WriteAllText(
                runReadmePath,
                "# Campaign run" + Environment.NewLine + Environment.NewLine + sectionText);
            return;
        }

        var existing = File.ReadAllText(runReadmePath);
        if (existing.Contains(sectionHeader, StringComparison.Ordinal))
        {
            File.AppendAllText(runReadmePath, noteLine + Environment.NewLine);
            return;
        }

        File.AppendAllText(
            runReadmePath,
            Environment.NewLine + Environment.NewLine + sectionText);
    }
}
