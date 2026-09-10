using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Rag;

public static class FactionCorpusPaths
{
    public static string? LatestReportPath(string factionDir)
    {
        var reports = ReportPaths(factionDir);
        if (reports.Count == 0)
        {
            return null;
        }

        return reports
            .Select(path => (path, turn: ParseReportTurn(path)))
            .OrderByDescending(tuple => tuple.turn)
            .ThenBy(tuple => tuple.path, StringComparer.OrdinalIgnoreCase)
            .First()
            .path;
    }

    public static IReadOnlyList<string> OrderPathsWithinTurnWindow(string factionDir, int maxOrderTurns)
    {
        var allOrders = OrderPaths(factionDir);
        if (maxOrderTurns <= 0 || allOrders.Count == 0)
        {
            return allOrders;
        }

        return allOrders
            .Select(path => (path, turn: ParseOrderTurn(path)))
            .GroupBy(tuple => tuple.turn)
            .OrderByDescending(group => group.Key)
            .Take(maxOrderTurns)
            .SelectMany(group => group.Select(tuple => tuple.path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool IsFactionCorpusFile(string path)
    {
        var fileName = Path.GetFileName(path);
        if (string.Equals(fileName, "story.md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return OrderFileNaming.TryParseReportFileName(fileName, out _, out _)
            || OrderFileNaming.TryParseOrderFileName(fileName, out _, out _, out _);
    }

    public static IReadOnlyList<string> ReportPaths(string factionDir)
    {
        if (!Directory.Exists(factionDir))
        {
            return [];
        }

        return Directory
            .GetFiles(factionDir, "report*.txt")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string? StoryPath(string factionDir)
    {
        var path = Path.Combine(factionDir, "story.md");
        return File.Exists(path) ? path : null;
    }

    public static IReadOnlyList<string> OrderPaths(string factionDir)
    {
        if (!Directory.Exists(factionDir))
        {
            return [];
        }

        return Directory
            .GetFiles(factionDir, "orders.*.txt")
            .Concat(Directory.GetFiles(factionDir, "order.*.txt"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int ParseReportTurn(string path)
    {
        return OrderFileNaming.TryParseReportFileName(Path.GetFileName(path), out var turn, out _)
            ? turn
            : 0;
    }

    private static int ParseOrderTurn(string path)
    {
        return OrderFileNaming.TryParseOrderFileName(Path.GetFileName(path), out _, out var turn, out _)
            ? turn
            : 0;
    }
}
