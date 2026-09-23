using System.Text;
using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Rag;

public static partial class MarkdownChunker
{
    public const int MaxChunkCharacters = 3500;
    public const int MaxRulesChunkCharacters = 4500;
    public const int ChunkOverlapCharacters = 350;

    private static readonly HashSet<string> OrderVerbHeadings = new(StringComparer.OrdinalIgnoreCase)
    {
        "ACTIVE", "ALIAS", "ATTACK", "BUY", "CAPTURE", "CONTRACT", "COPY", "DECLARE", "FORM", "GET",
        "GIVE", "HAS", "NAME", "PRESS", "SEE", "SELL", "SET", "STACK", "SYNCHRO", "TACTIC", "TRANSFER",
        "MOVE", "PRODUCE", "REPAIR", "RESEARCH", "TRAIN", "USE",
    };

    public static IReadOnlyList<TextChunk> ChunkManual(string sourcePath, string content, PlayMode mode)
    {
        var fileName = Path.GetFileName(sourcePath);
        return fileName.ToLowerInvariant() switch
        {
            "rules.md" => ChunkRules(sourcePath, content),
            "battle.md" => ChunkByHeading(sourcePath, content, doc: "battle", mode: null, h2Only: true),
            "basic_technologies.md" or "advanced_technologies.md" =>
                ChunkTechManual(sourcePath, content, PlayModeParser.ToCliValue(mode)),
            _ => ChunkByHeading(sourcePath, content, doc: "manual", mode: PlayModeParser.ToCliValue(mode), h2Only: true),
        };
    }

    public static IReadOnlyList<TextChunk> ChunkReport(string sourcePath, string content)
    {
        var chunks = new List<TextChunk>();
        var chunkIndex = 0;

        foreach (var section in SplitReportSections(content))
        {
            var heading = InferReportSectionHeading(section);
            foreach (var piece in SplitWithSizeCap(section, MaxChunkCharacters, ChunkOverlapCharacters))
            {
                var body = piece.Trim();
                if (string.IsNullOrWhiteSpace(body))
                {
                    continue;
                }

                chunkIndex++;
                chunks.Add(new TextChunk(
                    body,
                    new ChunkMetadata("report", null, null, sourcePath, $"report-{heading}-{chunkIndex}")));
            }
        }

        return chunks;
    }

    public static IReadOnlyList<TextChunk> ChunkStory(string sourcePath, string content) =>
    [
        new TextChunk(
            content.Trim(),
            new ChunkMetadata("story", null, null, sourcePath, "objective")),
    ];

    public static IReadOnlyList<TextChunk> ChunkOrderFile(string sourcePath, string content) =>
        ChunkOrderBlocks(sourcePath, content);

    public static IReadOnlyList<TextChunk> ChunkRules(string sourcePath, string content)
    {
        var chunks = new List<TextChunk>();
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var currentH2 = string.Empty;
        var inOrderSection = false;
        var buffer = new StringBuilder();
        string? currentHeading = null;
        string? currentVerb = null;

        void Flush()
        {
            var body = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(body))
            {
                buffer.Clear();
                return;
            }

            foreach (var piece in SplitWithSizeCap(body, MaxRulesChunkCharacters, ChunkOverlapCharacters))
            {
                chunks.Add(new TextChunk(
                    piece.Trim(),
                    new ChunkMetadata(
                        "rules",
                        currentVerb,
                        null,
                        sourcePath,
                        currentHeading ?? currentH2)));
            }

            buffer.Clear();
        }

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();
                currentH2 = line["## ".Length..].Trim();
                currentHeading = currentH2;
                currentVerb = null;
                inOrderSection = currentH2.Equals("Immediate orders", StringComparison.OrdinalIgnoreCase)
                    || currentH2.Equals("Long orders", StringComparison.OrdinalIgnoreCase);
                buffer.AppendLine(line);
                continue;
            }

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                var heading = line["### ".Length..].Trim();
                if (inOrderSection && OrderVerbHeadings.Contains(heading))
                {
                    Flush();
                    currentHeading = heading;
                    currentVerb = heading.ToUpperInvariant();
                    buffer.AppendLine(line);
                    continue;
                }

                if (!inOrderSection)
                {
                    Flush();
                    currentHeading = heading;
                    currentVerb = null;
                    buffer.AppendLine(line);
                    continue;
                }
            }

            buffer.AppendLine(line);
        }

        Flush();
        return chunks;
    }

    public static IReadOnlyList<TextChunk> ChunkTechManual(string sourcePath, string content, string mode)
    {
        var chunks = new List<TextChunk>();
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var buffer = new StringBuilder();
        string? currentHeading = null;

        void Flush()
        {
            var body = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(body))
            {
                buffer.Clear();
                return;
            }

            foreach (var piece in SplitWithSizeCap(body, MaxChunkCharacters, ChunkOverlapCharacters))
            {
                chunks.Add(new TextChunk(
                    piece.Trim(),
                    new ChunkMetadata("tech", null, mode, sourcePath, currentHeading)));
            }

            buffer.Clear();
        }

        foreach (var line in lines)
        {
            if (TechEntryRegex().IsMatch(line))
            {
                Flush();
                currentHeading = line.Trim().Trim('*').Trim();
                buffer.AppendLine(line);
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal) && buffer.Length == 0)
            {
                currentHeading = line["## ".Length..].Trim();
            }

            buffer.AppendLine(line);
        }

        Flush();
        return chunks;
    }

    public static IReadOnlyList<TextChunk> ChunkByHeading(
        string sourcePath,
        string content,
        string doc,
        string? mode,
        bool h2Only)
    {
        var chunks = new List<TextChunk>();
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var buffer = new StringBuilder();
        string? currentHeading = null;

        void Flush()
        {
            var body = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(body))
            {
                buffer.Clear();
                return;
            }

            foreach (var piece in SplitWithSizeCap(body, MaxChunkCharacters, ChunkOverlapCharacters))
            {
                chunks.Add(new TextChunk(
                    piece.Trim(),
                    new ChunkMetadata(doc, null, mode, sourcePath, currentHeading)));
            }

            buffer.Clear();
        }

        foreach (var line in lines)
        {
            var isBoundary = h2Only
                ? line.StartsWith("## ", StringComparison.Ordinal)
                : line.StartsWith("## ", StringComparison.Ordinal) || line.StartsWith("### ", StringComparison.Ordinal);

            if (isBoundary)
            {
                Flush();
                currentHeading = line.TrimStart('#', ' ').Trim();
                buffer.AppendLine(line);
                continue;
            }

            buffer.AppendLine(line);
        }

        Flush();
        return chunks;
    }

    public static IReadOnlyList<TextChunk> ChunkOrderBlocks(string sourcePath, string content)
    {
        var chunks = new List<TextChunk>();
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var buffer = new StringBuilder();
        string? currentHeading = "header";

        void Flush()
        {
            var body = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(body))
            {
                buffer.Clear();
                return;
            }

            chunks.Add(new TextChunk(
                body,
                new ChunkMetadata("order", null, null, sourcePath, currentHeading)));

            buffer.Clear();
        }

        foreach (var line in lines)
        {
            if (line.StartsWith("#modulestack ", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("#person ", StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                currentHeading = line.Split(' ', 2)[0];
                buffer.AppendLine(line);
                continue;
            }

            buffer.AppendLine(line);
        }

        Flush();
        return chunks;
    }

    public static IReadOnlyList<string> SplitWithSizeCap(
        string content,
        int maxCharacters,
        int overlapCharacters = 0)
    {
        if (content.Length <= maxCharacters)
        {
            return [content];
        }

        overlapCharacters = Math.Clamp(overlapCharacters, 0, maxCharacters / 2);
        var step = Math.Max(1, maxCharacters - overlapCharacters);
        var parts = new List<string>();
        var paragraphs = content.Split("\n\n", StringSplitOptions.None);
        var buffer = new StringBuilder();

        void SeedOverlap(string text)
        {
            if (overlapCharacters <= 0 || text.Length <= overlapCharacters)
            {
                return;
            }

            buffer.Append(text[^overlapCharacters..]);
        }

        void Flush()
        {
            if (buffer.Length == 0)
            {
                return;
            }

            var text = buffer.ToString().TrimEnd();
            parts.Add(text);
            buffer.Clear();
            SeedOverlap(text);
        }

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > maxCharacters)
            {
                Flush();
                for (var offset = 0; offset < paragraph.Length; offset += step)
                {
                    var length = Math.Min(maxCharacters, paragraph.Length - offset);
                    parts.Add(paragraph.Substring(offset, length).Trim());
                }

                continue;
            }

            if (buffer.Length + paragraph.Length + 2 > maxCharacters)
            {
                Flush();
            }

            if (buffer.Length > 0)
            {
                buffer.AppendLine();
                buffer.AppendLine();
            }

            buffer.Append(paragraph);
        }

        Flush();
        return parts;
    }

    public static IReadOnlyList<string> SplitReportSections(string content)
    {
        var normalized = content.Replace("\r\n", "\n");
        var lines = normalized.Split('\n');
        var sections = new List<string>();
        var buffer = new StringBuilder();

        void Flush()
        {
            if (buffer.Length == 0)
            {
                return;
            }

            sections.Add(buffer.ToString().TrimEnd());
            buffer.Clear();
        }

        foreach (var line in lines)
        {
            if (IsReportSectionBoundary(line) && buffer.Length > 0)
            {
                Flush();
            }

            buffer.AppendLine(line);
        }

        Flush();
        return sections;
    }

    internal static string InferReportSectionHeading(string section)
    {
        foreach (var line in section.Replace("\r\n", "\n").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.Trim();
            if (trimmed.StartsWith("+ ", StringComparison.Ordinal))
            {
                return SanitizeHeading(trimmed);
            }

            if (trimmed.EndsWith(':'))
            {
                return SanitizeHeading(trimmed.TrimEnd(':'));
            }

            return SanitizeHeading(trimmed);
        }

        return "section";
    }

    private static bool IsReportSectionBoundary(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        if (line.StartsWith("Orders Template:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ReportStackLineRegex().IsMatch(line))
        {
            return true;
        }

        if (ReportRegionLineRegex().IsMatch(line))
        {
            return true;
        }

        return ReportTopSectionRegex().IsMatch(line);
    }

    private static string SanitizeHeading(string value)
    {
        var collapsed = WhitespaceRegex().Replace(value.Trim(), " ");
        if (collapsed.Length <= 48)
        {
            return collapsed;
        }

        return collapsed[..48];
    }

    [GeneratedRegex(@"^\s{2}\+\s")]
    private static partial Regex ReportStackLineRegex();

    [GeneratedRegex(@"^\s{2}[A-Za-z].*\[[ROMPS]\d+\]")]
    private static partial Regex ReportRegionLineRegex();

    [GeneratedRegex(@"^[A-Za-z].*:\s*$")]
    private static partial Regex ReportTopSectionRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"^\*\*.+\*\*\s*$")]
    private static partial Regex TechEntryRegex();
}
