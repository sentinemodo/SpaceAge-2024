using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Paths;

public static partial class StoryFileNaming
{
    public static string FileName(int factionId, int turn) => $"story.{factionId}.{turn}.md";

    public static string PathFor(string factionDir, int factionId, int turn) =>
        Path.Combine(factionDir, FileName(factionId, turn));

    public static bool IsStoryFileName(string fileName) =>
        TryParseFileName(fileName, out _, out _)
        || string.Equals(fileName, "story.md", StringComparison.OrdinalIgnoreCase);

    public static bool TryParseFileName(string fileName, out int factionId, out int turn)
    {
        factionId = 0;
        turn = 0;
        var match = StoryFileRegex().Match(fileName);
        if (!match.Success)
        {
            return false;
        }

        factionId = int.Parse(match.Groups[1].Value);
        turn = int.Parse(match.Groups[2].Value);
        return true;
    }

    /// <summary>Latest turn story file for a faction, or legacy story.md.</summary>
    public static string? ResolveActiveStoryPath(string factionDir, int factionId)
    {
        if (!Directory.Exists(factionDir))
        {
            return null;
        }

        string? bestPath = null;
        var bestTurn = -1;
        foreach (var path in Directory.GetFiles(factionDir, $"story.{factionId}.*.md"))
        {
            if (!TryParseFileName(Path.GetFileName(path), out var id, out var turn) || id != factionId)
            {
                continue;
            }

            if (turn > bestTurn)
            {
                bestTurn = turn;
                bestPath = path;
            }
        }

        if (bestPath is not null)
        {
            return bestPath;
        }

        var legacy = Path.Combine(factionDir, "story.md");
        return File.Exists(legacy) ? legacy : null;
    }

    public static string? ResolveStoryPathForTurn(string factionDir, int factionId, int turn)
    {
        var path = PathFor(factionDir, factionId, turn);
        return File.Exists(path) ? path : null;
    }

    public static IReadOnlyList<string> AllStoryPaths(string factionDir, int factionId)
    {
        if (!Directory.Exists(factionDir))
        {
            return [];
        }

        var list = Directory
            .GetFiles(factionDir, $"story.{factionId}.*.md")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var legacy = Path.Combine(factionDir, "story.md");
        if (File.Exists(legacy))
        {
            list.Add(legacy);
        }

        return list;
    }

    [GeneratedRegex(@"^story\.(\d+)\.(\d+)\.md$", RegexOptions.IgnoreCase)]
    private static partial Regex StoryFileRegex();
}
