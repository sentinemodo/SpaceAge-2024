using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class ReportStackCatalog
{
    /// <summary>Numeric stack ids listed in the report Orders Template footer.</summary>
    public static IReadOnlyList<string> ParseOrdersTemplateStackIds(string? reportText)
    {
        var ids = new List<string>();
        var template = DraftPromptBuilder.ExtractOrdersTemplate(reportText) ?? reportText;
        if (string.IsNullOrWhiteSpace(template))
        {
            return ids;
        }

        foreach (var rawLine in template.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var id = line["#modulestack".Length..].Trim();
            if (id.Length > 0
                && char.IsDigit(id[0])
                && !id.StartsWith("new", StringComparison.OrdinalIgnoreCase))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    public static IReadOnlyDictionary<string, string> ParseModuleTypes(string? reportText)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return map;
        }

        var template = DraftPromptBuilder.ExtractOrdersTemplate(reportText) ?? reportText;
        string? currentStack = null;

        foreach (var rawLine in template.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                currentStack = line["#modulestack".Length..].Trim();
                if (currentStack.StartsWith("new", StringComparison.OrdinalIgnoreCase))
                {
                    currentStack = null;
                }

                continue;
            }

            if (currentStack is null || !line.StartsWith(';'))
            {
                continue;
            }

            var match = StackModuleTypeRegex().Match(line);
            if (!match.Success || !string.Equals(match.Groups[1].Value, currentStack, StringComparison.Ordinal))
            {
                continue;
            }

            map[currentStack] = match.Groups[2].Value;
        }

        return map;
    }

    public static bool StackTemplateMentionsModuleTech(string? reportText, string stackId, string techId)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return false;
        }

        var template = DraftPromptBuilder.ExtractOrdersTemplate(reportText) ?? reportText;
        var blockMatch = Regex.Match(
            template,
            $@"#\s*modulestack\s+{Regex.Escape(stackId)}\b([\s\S]*?)(?=#\s*modulestack|#person|#end\b)",
            RegexOptions.IgnoreCase);
        return blockMatch.Success
               && blockMatch.Groups[1].Value.Contains($"[{techId}]", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Grant layout uses wind powerplants (typical Anvil) instead of coal cplant (Arbor).</summary>
    public static bool GrantUsesWindPowerPlant(string? reportText)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return false;
        }

        foreach (var typ in ParseModuleTypes(reportText).Values)
        {
            if (typ.Equals("wnplnt", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return WindPlantInReportRegex().IsMatch(reportText);
    }

    public static bool FactoryAlreadyHasTechnology(string? reportText, string techId) =>
        !string.IsNullOrWhiteSpace(reportText)
        && reportText.Contains($"[{techId}]", StringComparison.OrdinalIgnoreCase)
        && reportText.Contains("technologies:", StringComparison.OrdinalIgnoreCase)
        && reportText.Contains("factory [", StringComparison.OrdinalIgnoreCase);

    public static string? FindStackIdByModuleType(string? reportText, string moduleTypeId)
    {
        foreach (var pair in ParseModuleTypes(reportText))
        {
            if (pair.Value.Equals(moduleTypeId, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        return null;
    }

    // ; + farming complex [200006], 3 farming complexes [farms], immobile.
    [GeneratedRegex(@"\[(\d+)\][^\[]*\[(\w+)\],\s*(?:disabled,\s*)?immobile\.?", RegexOptions.IgnoreCase)]
    private static partial Regex StackModuleTypeRegex();

    [GeneratedRegex(@"\bwind powerplants?\s*\[[^\]]*\]\s*\[wnplnt\]", RegexOptions.IgnoreCase)]
    private static partial Regex WindPlantInReportRegex();
}
