using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class ReportRegionGraph
{
    /// <summary>Corporate grant cell on the homeworld (e.g. Northwind Grant [R00008]).</summary>
    public static string? ParseGrantRegionId(string? reportText)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return null;
        }

        var match = GrantRegionRegex().Match(reportText);
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }

    /// <summary>Ground region id → adjacent ground region ids listed under Exits.</summary>
    public static IReadOnlyDictionary<string, HashSet<string>> ParseGroundRegionAdjacency(string? reportText)
    {
        var adjacency = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return adjacency;
        }

        string? currentRegion = null;
        var inExits = false;

        foreach (var rawLine in reportText.Replace("\r\n", "\n").Split('\n'))
        {
            if (rawLine.StartsWith("Rumors:", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var regionHeader = RegionHeaderRegex().Match(rawLine);
            if (regionHeader.Success)
            {
                currentRegion = regionHeader.Groups[1].Value.ToUpperInvariant();
                adjacency.TryAdd(currentRegion, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                inExits = false;
                continue;
            }

            if (currentRegion is null)
            {
                continue;
            }

            if (rawLine.TrimStart().StartsWith("Exits:", StringComparison.OrdinalIgnoreCase))
            {
                inExits = true;
                continue;
            }

            if (!inExits)
            {
                continue;
            }

            if (rawLine.TrimStart().StartsWith("Resources:", StringComparison.OrdinalIgnoreCase)
                || rawLine.TrimStart().StartsWith("Market report:", StringComparison.OrdinalIgnoreCase)
                || rawLine.TrimStart().StartsWith("Contracts:", StringComparison.OrdinalIgnoreCase)
                || rawLine.TrimStart().StartsWith("+ ", StringComparison.Ordinal))
            {
                inExits = false;
                continue;
            }

            var exitMatch = ExitRegionRegex().Match(rawLine);
            if (!exitMatch.Success)
            {
                continue;
            }

            adjacency[currentRegion].Add(exitMatch.Groups[1].Value.ToUpperInvariant());
        }

        return adjacency;
    }

    /// <summary>Resource item ids listed under the corporate grant region (e.g. iron, carbon).</summary>
    public static IReadOnlySet<string> ParseGrantRegionResourceIds(string? reportText)
    {
        var resources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return resources;
        }

        var grantRegion = ParseGrantRegionId(reportText);
        if (grantRegion is null)
        {
            return resources;
        }

        string? currentRegion = null;
        var inResources = false;

        foreach (var rawLine in reportText.Replace("\r\n", "\n").Split('\n'))
        {
            if (rawLine.StartsWith("Rumors:", StringComparison.OrdinalIgnoreCase)
                && currentRegion is not null
                && !string.Equals(currentRegion, grantRegion, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var regionHeader = RegionHeaderRegex().Match(rawLine);
            if (regionHeader.Success)
            {
                currentRegion = regionHeader.Groups[1].Value.ToUpperInvariant();
                inResources = false;
                continue;
            }

            if (!string.Equals(currentRegion, grantRegion, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (rawLine.TrimStart().StartsWith("Resources:", StringComparison.OrdinalIgnoreCase))
            {
                inResources = true;
                foreach (Match match in ResourceItemRegex().Matches(rawLine))
                {
                    resources.Add(match.Groups[1].Value.ToLowerInvariant());
                }

                continue;
            }

            if (!inResources)
            {
                continue;
            }

            if (rawLine.TrimStart().StartsWith("Contracts:", StringComparison.OrdinalIgnoreCase)
                || rawLine.TrimStart().StartsWith("+ ", StringComparison.Ordinal))
            {
                inResources = false;
                continue;
            }

            foreach (Match match in ResourceItemRegex().Matches(rawLine))
            {
                resources.Add(match.Groups[1].Value.ToLowerInvariant());
            }
        }

        return resources;
    }

    /// <summary>Ground region id where a module stack is listed in the galaxy report (e.g. moblab away from grant).</summary>
    public static string? ParseModuleStackRegionId(string? reportText, string stackId)
    {
        if (string.IsNullOrWhiteSpace(reportText) || string.IsNullOrWhiteSpace(stackId))
        {
            return null;
        }

        string? currentRegion = null;
        var stackToken = $"[{stackId}]";

        foreach (var rawLine in reportText.Replace("\r\n", "\n").Split('\n'))
        {
            if (rawLine.StartsWith("Rumors:", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var regionHeader = RegionHeaderRegex().Match(rawLine);
            if (regionHeader.Success)
            {
                currentRegion = regionHeader.Groups[1].Value.ToUpperInvariant();
                continue;
            }

            if (currentRegion is not null
                && rawLine.Contains(stackToken, StringComparison.Ordinal)
                && rawLine.TrimStart().StartsWith("+ ", StringComparison.Ordinal))
            {
                return currentRegion;
            }
        }

        return null;
    }

    /// <summary>Markdown table for prompt: grant region ground exits (MOVE targets).</summary>
    public static string FormatGrantRegionNavigationTable(string? reportText)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return string.Empty;
        }

        var grantRegion = ParseGrantRegionId(reportText);
        if (grantRegion is null)
        {
            return string.Empty;
        }

        var adjacency = ParseGroundRegionAdjacency(reportText);
        var resources = ParseGrantRegionResourceIds(reportText);
        if (!adjacency.TryGetValue(grantRegion, out var exits) || exits.Count == 0)
        {
            return string.Empty;
        }

        var groundExits = exits.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList();
        var resourceList = resources.Count == 0
            ? "(none listed)"
            : string.Join(", ", resources.OrderBy(id => id, StringComparer.OrdinalIgnoreCase));

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("## Grant region navigation (GROUND `move R…` / `-move R…` must use an exit id below)");
        builder.AppendLine($"| Region | Ground exits | Resources in grant cell |");
        builder.AppendLine($"| {grantRegion} | {string.Join(", ", groundExits)} | {resourceList} |");
        return builder.ToString().TrimEnd();
    }

    public static int? ParseBankBalance(string? reportText)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return null;
        }

        var match = BankBalanceRegex().Match(reportText);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var balance))
        {
            return null;
        }

        return balance;
    }

    [GeneratedRegex(@"\bGrant\s+\[(R\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex GrantRegionRegex();

    [GeneratedRegex(@"\[([a-z0-9]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex ResourceItemRegex();

    [GeneratedRegex(@"Bank account balance:\s*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex BankBalanceRegex();

    // Two-space region lines (not nested "+ HQ" modules).
    [GeneratedRegex(@"^\s{2}(?!\+)\S.*\[(R\d+)\].*,.*\bregion\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegionHeaderRegex();

    [GeneratedRegex(@"\[(R\d+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExitRegionRegex();
}
