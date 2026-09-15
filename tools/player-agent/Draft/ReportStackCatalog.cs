using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class ReportStackCatalog
{
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

    // ; + farming complex [200006], 3 farming complexes [farms], immobile.
    [GeneratedRegex(@"\[(\d+)\][^\[]*\[(\w+)\],\s*immobile", RegexOptions.IgnoreCase)]
    private static partial Regex StackModuleTypeRegex();
}
