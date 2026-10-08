using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class ReportBattleIntel
{
    /// <summary>Fauna owner faction ids seen in battles / hostile rumors on this report.</summary>
    public static IReadOnlySet<int> FaunaFactionsOnPlanet(string? reportText)
    {
        var ids = new HashSet<int>();
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return ids;
        }

        foreach (Match match in FaunaOwnerRegex().Matches(reportText))
        {
            if (int.TryParse(match.Groups[1].Value, out var id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    public static string? HomePlanetCatalogId(string? reportText)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return null;
        }

        var match = HomePlanetRegex().Match(reportText);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>Regions where defenders lost to fauna this report (avoid re-attack without mass).</summary>
    public static IReadOnlyList<string> RegionsWhereDefendersLost(string? reportText, int playerFactionId)
    {
        var regions = new List<string>();
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return regions;
        }

        var chunks = reportText.Split("Battle has commenced", StringSplitOptions.RemoveEmptyEntries);
        foreach (var chunk in chunks.Skip(1))
        {
            if (!chunk.Contains("Battle won by attackers", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var regionMatch = RegionInBattleRegex().Match(chunk);
            if (!regionMatch.Success)
            {
                continue;
            }

            if (Regex.IsMatch(chunk, $@"Defenders:[\s\S]*?\[{playerFactionId}\]", RegexOptions.IgnoreCase))
            {
                regions.Add(regionMatch.Groups[1].Value);
            }
        }

        return regions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static bool StackMarkedDisabledInTemplate(string? reportText, string stackId)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return false;
        }

        var blockMatch = Regex.Match(
            reportText,
            $@"#\s*modulestack\s+{Regex.Escape(stackId)}\b([\s\S]*?)(?=#\s*modulestack|\#person|\#end\b|$)",
            RegexOptions.IgnoreCase);
        return blockMatch.Success
               && blockMatch.Groups[1].Value.Contains("disabled", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>True when report stack detail shows hull damage (current HP below max).</summary>
    public static bool StackNeedsRepair(string? reportText, string stackId)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return false;
        }

        var body = reportText;
        var templateIdx = reportText.IndexOf("Orders Template:", StringComparison.OrdinalIgnoreCase);
        if (templateIdx > 0)
        {
            body = reportText[..templateIdx];
        }

        var pattern = $@"\btanks\s*\[{Regex.Escape(stackId)}\][\s\S]{{0,1200}}?hit points:\s*(\d+)/(\d+)";
        Match? best = null;
        foreach (Match match in Regex.Matches(body, pattern, RegexOptions.IgnoreCase))
        {
            best = match;
        }

        if (best is null || !best.Success)
        {
            return false;
        }

        if (!int.TryParse(best.Groups[1].Value, out var left)
            || !int.TryParse(best.Groups[2].Value, out var right))
        {
            return false;
        }

        var max = Math.Max(left, right);
        var current = Math.Min(left, right);
        return current < max;
    }

    public static bool StackCanOperateWithoutRefuelInTemplate(string? reportText, string stackId)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return false;
        }

        var blockMatch = Regex.Match(
            reportText,
            $@"#\s*modulestack\s+{Regex.Escape(stackId)}\b([\s\S]*?)(?=#\s*modulestack|\#person|\#end\b|$)",
            RegexOptions.IgnoreCase);
        return blockMatch.Success
               && Regex.IsMatch(
                   blockMatch.Groups[1].Value,
                   @"can operate for another \d+ weeks without refueling",
                   RegexOptions.IgnoreCase);
    }

    [GeneratedRegex(@"owned by\s+(?:\w+\s+)?Fauna\s*\[(\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex FaunaOwnerRegex();

    [GeneratedRegex(@"on\s+(Anvil|Arbor)\s*\[(P\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex HomePlanetRegex();

    [GeneratedRegex(@"\bat\s+[^\[]+\[(R\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex RegionInBattleRegex();
}
