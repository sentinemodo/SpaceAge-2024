using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

/// <summary>
/// Deterministic <c>NAME "…"</c> injection for AI corporate factions (2–11) on freshly aliased stacks.
/// </summary>
public static partial class OrderDraftStackNaming
{
    public static string InjectMissingNames(string orderText, int factionId, string? personaPreference)
    {
        if (factionId is < 2 or > 11)
        {
            return orderText;
        }

        if (string.IsNullOrWhiteSpace(orderText))
        {
            return orderText;
        }

        var useTechByAlias = ParseFreshStackTechMap(orderText);
        if (useTechByAlias.Count == 0)
        {
            return orderText;
        }

        var lines = orderText.Replace("\r\n", "\n").Split('\n').ToList();
        var output = new List<string>(lines.Count + useTechByAlias.Count);
        string? currentStack = null;
        var blockLines = new List<string>();
        var blockHasName = false;

        void FlushBlock()
        {
            if (currentStack is null)
            {
                return;
            }

            if (useTechByAlias.TryGetValue(currentStack, out var tech)
                && !blockHasName
                && currentStack.StartsWith("new", StringComparison.OrdinalIgnoreCase))
            {
                var name = GenerateDisplayName(tech, personaPreference, factionId, currentStack);
                var insertAt = FindNameInsertIndex(blockLines, currentStack);
                blockLines.Insert(insertAt, $"NAME \"{name}\"");
            }

            output.Add($"#modulestack {currentStack}");
            output.AddRange(blockLines);
            currentStack = null;
            blockLines = new List<string>();
            blockHasName = false;
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine;
            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                FlushBlock();
                currentStack = line["#modulestack".Length..].Trim();
                continue;
            }

            if (line.StartsWith('#'))
            {
                FlushBlock();
                output.Add(line);
                continue;
            }

            if (currentStack is null)
            {
                output.Add(line);
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.Length > 0 && !trimmed.StartsWith(';'))
            {
                if (IsNameLine(trimmed))
                {
                    blockHasName = true;
                }

                blockLines.Add(line);
            }
            else
            {
                blockLines.Add(line);
            }
        }

        FlushBlock();
        return string.Join(Environment.NewLine, output);
    }

    private static int FindNameInsertIndex(IReadOnlyList<string> blockLines, string stackId)
    {
        var index = 0;
        while (index < blockLines.Count && blockLines[index].TrimStart().StartsWith(';'))
        {
            index++;
        }

        if (index < blockLines.Count)
        {
            var trimmed = blockLines[index].TrimStart();
            if (ActiveStackRegex().IsMatch(trimmed)
                && trimmed.Contains(stackId, StringComparison.OrdinalIgnoreCase))
            {
                index++;
            }
        }

        return index;
    }

    private static bool IsNameLine(string trimmed) =>
        trimmed.StartsWith("NAME ", StringComparison.OrdinalIgnoreCase)
        || trimmed.StartsWith("name ", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, string> ParseFreshStackTechMap(string orderText)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in FreshStackAliasRegex().Matches(orderText))
        {
            map[match.Groups[2].Value] = match.Groups[1].Value;
        }

        return map;
    }

    public static string GenerateDisplayName(
        string useTech,
        string? personaPreference,
        int factionId,
        string stackAlias)
    {
        var tech = useTech.ToUpperInvariant();
        var persona = NormalizePersona(personaPreference, tech);
        var pool = SelectNamePool(persona, tech);
        var index = ExtractNewStackIndex(stackAlias) + factionId;
        return pool[index % pool.Length];
    }

    private static string NormalizePersona(string? personaPreference, string tech)
    {
        if (!string.IsNullOrWhiteSpace(personaPreference))
        {
            return personaPreference.Trim().ToLowerInvariant();
        }

        return tech switch
        {
            "ARMCBT" or "GRNDTR" or "TANKS" or "FRMINF" or "ENGTRK" => "military",
            "MSRVTM" or "FILIDX" or "MOBLAB" => "researcher",
            "TWNBLD" or "MOBCTR" => "contractor",
            _ => "economic",
        };
    }

    private static string[] SelectNamePool(string persona, string tech) =>
        (persona, tech) switch
        {
            ("military", "GRNDTR") =>
            [
                "Grant scout", "Slope rider", "Anvil pathfinder", "Iron patrol", "Ridge watcher",
            ],
            ("military", "ARMCBT") or ("military", "TANKS") =>
            [
                "Burrow breaker", "Slope striker", "Ironclad hammer", "Grant heavy section", "Fauna cull team",
            ],
            ("researcher", "MSRVTM") or ("researcher", "MOBLAB") =>
            [
                "Bench survey team", "FTIR field lab", "Anomaly cartograph", "Silicate spectrometer", "Grant survey rover",
            ],
            ("researcher", "FILIDX") =>
            [
                "Catalog indexer", "Spectral archive truck", "Field taxonomy unit",
            ],
            ("contractor", "TWNBLD") or ("economic", "TWNBLD") =>
            [
                "UN terrace shell", "Charter prefab town", "Exchange charter hull", "Marsh charter module",
            ],
            ("contractor", _) =>
            [
                "Contract hauler", "Charter logistics truck", "UN staging module",
            ],
            ("economic", "MCORED") or ("economic", "CDRILL") =>
            [
                "Core rig Alpha", "Deep metals nest", "Grant extraction stack",
            ],
            ("economic", "MSRVTM") =>
            [
                "Prospect survey lab", "Trade route scout lab",
            ],
            ("economic", "GRNDTR") =>
            [
                "Supply route scout", "Grant commerce truck",
            ],
            ("economic", "WNDTRB") =>
            [
                "Ridge wind batch", "Grant turbine string",
            ],
            _ =>
            [
                "Field unit", "Grant mobile stack", "Operations module",
            ],
        };

    private static int ExtractNewStackIndex(string stackAlias)
    {
        var match = NewIndexRegex().Match(stackAlias);
        return match.Success && int.TryParse(match.Groups[1].Value, out var index) ? index : stackAlias.GetHashCode();
    }

    [GeneratedRegex(
        @"(?im)^\s*(?:\+|\-|\d+\s+)?(?:@)?(?:use|train)\s+(\w+)\s+as\s+(new\d+)",
        RegexOptions.None)]
    private static partial Regex FreshStackAliasRegex();

    [GeneratedRegex(@"^active\s+(new\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ActiveStackRegex();

    [GeneratedRegex(@"new(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex NewIndexRegex();
}
