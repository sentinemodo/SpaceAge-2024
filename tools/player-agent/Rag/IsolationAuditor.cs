using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Rag;

public sealed record IsolationViolation(
    string IndexLabel,
    string Reason,
    string SourcePath,
    string? Doc);

public sealed record IsolationAuditResult(IReadOnlyList<IsolationViolation> Violations)
{
    public bool IsClean => Violations.Count == 0;
}

public static class IsolationAuditor
{
    private static readonly HashSet<string> SharedForbiddenDocs = new(StringComparer.OrdinalIgnoreCase)
    {
        "report",
        "story",
        "order",
    };

    public static IsolationAuditResult AuditRun(
        string indexRoot,
        string runId,
        PlayMode mode,
        string repoRoot,
        int fromFaction,
        int toFaction)
    {
        var violations = new List<IsolationViolation>();
        var playerDir = RepoPaths.PlayerDirectory(repoRoot);
        var sharedPath = VectorIndexPaths.SharedSqlitePath(
            RepoPaths.SharedIndexDirectory(indexRoot, mode));

        if (File.Exists(sharedPath))
        {
            violations.AddRange(AuditSharedIndex(sharedPath, mode, repoRoot, playerDir).Violations);
        }

        for (var factionId = fromFaction; factionId <= toFaction; factionId++)
        {
            var factionPath = VectorIndexPaths.FactionSqlitePath(
                RepoPaths.FactionIndexDirectory(indexRoot, runId, factionId));
            if (!File.Exists(factionPath))
            {
                continue;
            }

            violations.AddRange(AuditFactionIndex(factionPath, runId, factionId, repoRoot).Violations);
        }

        return new IsolationAuditResult(violations);
    }

    public static IsolationAuditResult AuditSharedIndex(
        string sqlitePath,
        PlayMode mode,
        string repoRoot,
        string playerDirectory)
    {
        if (!File.Exists(sqlitePath))
        {
            return new IsolationAuditResult([]);
        }

        var indexLabel = $"shared-{PlayModeParser.ToCliValue(mode)}";
        var normalizedPlayerDir = SourcePathNormalizer.Normalize(playerDirectory);
        var violations = new List<IsolationViolation>();

        using var store = new SqliteVectorStore(sqlitePath);
        foreach (var stored in store.ListAll())
        {
            var metadata = stored.Chunk.Metadata;
            var sourcePath = metadata.SourcePath;

            if (SharedForbiddenDocs.Contains(metadata.Doc))
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    $"Forbidden doc type '{metadata.Doc}' in shared index.",
                    sourcePath,
                    metadata.Doc));
                continue;
            }

            if (FactionCorpusPaths.IsFactionCorpusFile(sourcePath))
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    "Faction corpus file must not appear in shared index.",
                    sourcePath,
                    metadata.Doc));
                continue;
            }

            if (ContainsForbiddenPathSegment(sourcePath))
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    "Shared index source must not reference play runs or engine XML.",
                    sourcePath,
                    metadata.Doc));
                continue;
            }

            var normalizedSource = SourcePathNormalizer.Normalize(sourcePath);
            if (!IsUnderDirectory(normalizedSource, normalizedPlayerDir))
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    "Shared index source must live under player/ manuals.",
                    sourcePath,
                    metadata.Doc));
            }
        }

        return new IsolationAuditResult(violations);
    }

    public static IsolationAuditResult AuditFactionIndex(
        string sqlitePath,
        string runId,
        int factionId,
        string repoRoot)
    {
        if (!File.Exists(sqlitePath))
        {
            return new IsolationAuditResult([]);
        }

        var indexLabel = $"faction-{factionId:D2}";
        var expectedFactionDir = SourcePathNormalizer.Normalize(
            RepoPaths.FactionFolder(repoRoot, runId, factionId));
        var violations = new List<IsolationViolation>();

        using var store = new SqliteVectorStore(sqlitePath);
        foreach (var stored in store.ListAll())
        {
            var metadata = stored.Chunk.Metadata;
            var sourcePath = metadata.SourcePath;
            var normalizedSource = SourcePathNormalizer.Normalize(sourcePath);

            if (!IsUnderDirectory(normalizedSource, expectedFactionDir))
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    $"Source must stay under factions/{factionId:D2}/ for this seat.",
                    sourcePath,
                    metadata.Doc));
                continue;
            }

            var fileName = Path.GetFileName(sourcePath);
            if (OrderFileNaming.TryParseReportFileName(fileName, out _, out var reportFaction)
                && reportFaction != factionId)
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    $"Report filename faction id {reportFaction} does not match seat {factionId}.",
                    sourcePath,
                    metadata.Doc));
            }

            if (OrderFileNaming.TryParseOrderFileName(fileName, out var orderFaction, out _, out _)
                && orderFaction != factionId)
            {
                violations.Add(new IsolationViolation(
                    indexLabel,
                    $"Order filename faction id {orderFaction} does not match seat {factionId}.",
                    sourcePath,
                    metadata.Doc));
            }
        }

        return new IsolationAuditResult(violations);
    }

    public static string BuildAuditNote(
        string runId,
        PlayMode mode,
        int fromFaction,
        int toFaction,
        string auditedAtUtc,
        IsolationAuditResult result)
    {
        var date = auditedAtUtc.Length >= 10 ? auditedAtUtc[..10] : auditedAtUtc;
        var status = result.IsClean ? "clean" : $"{result.Violations.Count} violation(s)";
        var header =
            $"- Isolation audit ({date}, run `{runId}`, {PlayModeParser.ToCliValue(mode)}, factions {fromFaction}..{toFaction}): **{status}**.";

        if (result.IsClean)
        {
            return header;
        }

        var lines = result.Violations
            .Select(violation =>
                $"  - `{violation.IndexLabel}`: {violation.Reason} (`{Path.GetFileName(violation.SourcePath)}`, doc={violation.Doc ?? "-"})")
            .Prepend(header);

        return string.Join(Environment.NewLine, lines);
    }

    public static void AppendRunAuditNote(string auditPath, string note)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(auditPath)!);
        if (!File.Exists(auditPath))
        {
            File.WriteAllText(
                auditPath,
                "# Isolation audit" + Environment.NewLine + Environment.NewLine + note + Environment.NewLine);
            return;
        }

        File.AppendAllText(auditPath, note + Environment.NewLine);
    }

    public static void PrintViolations(IsolationAuditResult result)
    {
        if (result.IsClean)
        {
            Console.WriteLine("Isolation audit:  clean");
            return;
        }

        Console.WriteLine($"Isolation audit:  {result.Violations.Count} violation(s)");
        foreach (var violation in result.Violations)
        {
            Console.WriteLine(
                $"  [{violation.IndexLabel}] {violation.Reason} doc={violation.Doc ?? "-"} path={violation.SourcePath}");
        }
    }

    private static bool ContainsForbiddenPathSegment(string sourcePath)
    {
        var normalized = sourcePath.Replace('\\', '/');
        return normalized.Contains("/play/runs/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/gamein.xml", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/gameout", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith("/data.xml", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnderDirectory(string path, string directory)
    {
        var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedDirectory = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var prefix = normalizedDirectory + Path.DirectorySeparatorChar;
        return normalizedPath.Equals(normalizedDirectory, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}
