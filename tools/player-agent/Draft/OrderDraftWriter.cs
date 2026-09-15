using System.Text;
using System.Text.RegularExpressions;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class OrderDraftWriter
{
    public static string PrepareForWrite(string generatedText, int factionId, string? password)
    {
        var normalized = ExtractOrderBody(generatedText);
        normalized = StripReportTemplateComments(normalized);
        normalized = EnsureFactionHeader(normalized, factionId, password);
        normalized = EnsureEndTrailer(normalized);
        return normalized.TrimEnd() + Environment.NewLine;
    }

    private static string StripReportTemplateComments(string orderText)
    {
        var kept = new List<string>();
        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))
        {
            var trimmed = rawLine.TrimStart();
            if (trimmed.StartsWith(';')
                && (trimmed.StartsWith("; +", StringComparison.Ordinal)
                    || trimmed.Contains('[', StringComparison.Ordinal)
                    || trimmed.StartsWith("; items:", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("; technologies:", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            kept.Add(rawLine);
        }

        return string.Join(Environment.NewLine, kept);
    }

    public static async Task WriteUtf8Async(string outputPath, string orderText, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(outputPath, orderText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);
    }

    private static string ExtractOrderBody(string generatedText)
    {
        var trimmed = generatedText.Trim();
        var fenceMatch = MarkdownFenceRegex().Match(trimmed);
        if (fenceMatch.Success)
        {
            return fenceMatch.Groups[1].Value.Trim();
        }

        return trimmed;
    }

    private static string EnsureFactionHeader(string orderText, int factionId, string? password)
    {
        var lines = orderText.Replace("\r\n", "\n").Split('\n').ToList();
        var header = password is null
            ? $"#faction {factionId}"
            : PromptPackBuilder.BuildFactionLine(factionId, password, stripPassword: false);

        for (var index = 0; index < lines.Count; index++)
        {
            if (lines[index].StartsWith("#faction", StringComparison.OrdinalIgnoreCase))
            {
                lines[index] = header;
                return string.Join(Environment.NewLine, lines);
            }
        }

        lines.Insert(0, header);
        return string.Join(Environment.NewLine, lines);
    }

    private static string EnsureEndTrailer(string orderText)
    {
        if (orderText.Contains("#end", StringComparison.OrdinalIgnoreCase))
        {
            return orderText;
        }

        return orderText.TrimEnd() + Environment.NewLine + "#end";
    }

    [GeneratedRegex(@"```(?:\w*\n)?([\s\S]*?)```", RegexOptions.Multiline)]
    private static partial Regex MarkdownFenceRegex();
}
