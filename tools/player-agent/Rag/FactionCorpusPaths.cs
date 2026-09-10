namespace SpaceAge.PlayerAgent.Rag;

public static class FactionCorpusPaths
{
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
}
