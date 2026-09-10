using System.Text;
using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Rag;

public static partial class PromptPackBuilder
{
    public static string BuildPrefixesExcerpt(string rulesMarkdown, int maxCharacters = 2500)
    {
        var section = ExtractSection(rulesMarkdown, "Prefixes and subjects");
        if (string.IsNullOrWhiteSpace(section))
        {
            return Truncate(rulesMarkdown, maxCharacters);
        }

        return Truncate(section.Trim(), maxCharacters);
    }

    public static string BuildReportExcerpt(string reportText, int maxCharacters = 6000) =>
        Truncate(reportText.Trim(), maxCharacters);

    public static string BuildFactionLine(int factionId, string? password, bool stripPassword) =>
        stripPassword
            ? $"#faction {factionId}"
            : $"#faction {factionId} \"{password ?? string.Empty}\"";

    public static string BuildPromptPack(
        int factionId,
        string? password,
        bool stripPassword,
        string rulesMarkdown,
        string? reportText,
        string? objectiveText)
    {
        var builder = new StringBuilder();
        builder.AppendLine(BuildFactionLine(factionId, password, stripPassword));
        builder.AppendLine();
        builder.AppendLine("## Order syntax excerpt");
        builder.AppendLine(BuildPrefixesExcerpt(rulesMarkdown));
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(objectiveText))
        {
            builder.AppendLine("## Objective");
            builder.AppendLine(objectiveText.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(reportText))
        {
            builder.AppendLine("## Latest report excerpt");
            builder.AppendLine(BuildReportExcerpt(reportText));
        }

        return builder.ToString().Trim();
    }

    public static string StripFactionPassword(string orderText) =>
        FactionPasswordRegex().Replace(orderText, match => $"#faction {match.Groups[1].Value}");

    private static string ExtractSection(string markdown, string heading)
    {
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var capture = new StringBuilder();
        var inSection = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                var title = line["## ".Length..].Trim();
                if (title.Equals(heading, StringComparison.OrdinalIgnoreCase))
                {
                    inSection = true;
                    capture.AppendLine(line);
                    continue;
                }

                if (inSection)
                {
                    break;
                }
            }
            else if (inSection)
            {
                capture.AppendLine(line);
            }
        }

        return capture.ToString();
    }

    private static string Truncate(string value, int maxCharacters) =>
        value.Length <= maxCharacters ? value : value[..maxCharacters] + "\n…";

    [GeneratedRegex(@"#faction\s+(\d+)\s+""[^""]*""", RegexOptions.IgnoreCase)]
    private static partial Regex FactionPasswordRegex();
}
