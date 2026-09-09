namespace SpaceAge.PlayerAgent.Paths;

public static class RepoPaths
{
    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SpaceAge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repository root (SpaceAge.sln). Run from a built player-agent inside this repo.");
    }

    public static string PlayerDirectory(string repoRoot) =>
        Path.Combine(repoRoot, "player");

    public static string SharedIndexDirectory(string indexRoot, PlayMode mode) =>
        Path.Combine(indexRoot, $"shared-{PlayModeParser.ToCliValue(mode)}");

    public static string FactionIndexDirectory(string indexRoot, string runId, int factionId) =>
        Path.Combine(indexRoot, "runs", runId, $"faction-{factionId:D2}");

    public static IReadOnlyList<string> SharedManualPaths(string repoRoot, PlayMode mode)
    {
        var player = PlayerDirectory(repoRoot);
        var paths = new List<string>
        {
            Path.Combine(player, "rules.md"),
            Path.Combine(player, "battle.md"),
        };

        if (mode == PlayMode.Campaign)
        {
            paths.Add(Path.Combine(player, "campaign", "basic_technologies.md"));
            paths.Add(Path.Combine(player, "campaign", "advanced_technologies.md"));
        }
        else
        {
            paths.Add(Path.Combine(player, "basic_technologies.md"));
            paths.Add(Path.Combine(player, "advanced_technologies.md"));
        }

        return paths;
    }

    public static string ResolveDraftOutput(string repoRoot, string? outputPath, string? runId, int? factionId)
    {
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            return Path.GetFullPath(outputPath);
        }

        if (string.IsNullOrWhiteSpace(runId) || factionId is null)
        {
            throw new InvalidOperationException(
                "Draft output is required: pass --output <path> for dev/test drafts, "
                + "or --run <id> --faction <n> for campaign runs.");
        }

        if (factionId is < 2 or > 11)
        {
            throw new InvalidOperationException("Faction id must be between 2 and 11 for campaign runs.");
        }

        return Path.Combine(
            repoRoot,
            "play",
            "runs",
            runId,
            "factions",
            $"{factionId.Value:D2}",
            $"order.{factionId.Value}.txt");
    }

    public static string FactionFolder(string repoRoot, string runId, int factionId) =>
        Path.Combine(repoRoot, "play", "runs", runId, "factions", $"{factionId:D2}");

    public static void EnsureIndexLayout(string indexRoot)
    {
        Directory.CreateDirectory(indexRoot);
        Directory.CreateDirectory(Path.Combine(indexRoot, "runs"));
        Directory.CreateDirectory(Path.Combine(indexRoot, "usage"));
    }
}
