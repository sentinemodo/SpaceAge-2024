using System.Text.RegularExpressions;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class StoryDraftPromptBuilder
{
    public const string SystemPrompt =
        "You write SpaceAge campaign strategy briefs. Output markdown only with the exact headings "
        + "requested. Hard science, no magic. About 150-350 words in Narrative.";

    public const string ChunkSystemPrompt =
        "SpaceAge campaign brief writer. Markdown only, no fences, concise.";

    public static StoryDraftContext BuildContext(
        string factionName,
        int factionId,
        int reportTurn,
        string personaText,
        string reportText)
    {
        var reportExcerpt = BuildReportExcerpt(reportText, maxLines: 70);
        var contractHint = ExtractContractHint(reportText);
        var stackIds = ExtractStackIds(reportExcerpt, factionId);
        return new StoryDraftContext(
            factionName,
            factionId,
            reportTurn,
            personaText.Trim(),
            reportExcerpt,
            contractHint,
            stackIds);
    }

    public static string BuildMonolithicUserPrompt(StoryDraftContext context)
    {
        var includeWin = context.ReportTurn >= 10;
        var winHeading = includeWin
            ? "\n## Win objective\n(galaxy-wide; persona-tied)\n"
            : string.Empty;
        var reviewHeading = context.HasPriorStory
            ? "## Review\n(previous tactical vs this report; strategic still viable?)\n"
            : string.Empty;

        return $"""
            Write story.md for {context.FactionName} faction {context.FactionId}, turn {context.ReportTurn}
            {(context.HasPriorStory ? "(review prior story against this report)" : "(first quarter, no prior story)")}.

            Persona:
            {context.PersonaText}

            Report excerpt (use only these stack ids and contracts):
            {context.ReportExcerpt}

            Required headings:
            # {context.FactionName} — turn {context.ReportTurn}

            {reviewHeading}## Strategic objective
            (four quarters, Helios system, persona preference and doctrine)

            ## Tactical objective
            (next quarter bullets: economic loop + persona priority — researcher: build moblab, move to adjacent anomaly, RESEARCH region-id; else town contract via twnbld + transfer to faction 1 if CT open)

            {winHeading}## Narrative
            (150-350 words, hard SF tone)

            {(includeWin ? string.Empty : "Omit Win objective (T < 10). ")}
            {(context.HasPriorStory ? string.Empty : "Omit Review (first quarter). ")}
            Use stack ids from report: {context.StackIdsSummary}.
            Contract: {context.ContractHint}.
            """;
    }

    public static IReadOnlyList<StoryChunkPrompt> BuildChunkPrompts(StoryDraftContext context)
    {
        var isResearcher = context.PersonaText.Contains("Preference: researcher", StringComparison.OrdinalIgnoreCase);
        var personaBrief =
            $"{context.FactionName} faction {context.FactionId}. {SummarizePersona(context.PersonaText)} "
            + $"Contract: {context.ContractHint}. Stacks: {context.StackIdsSummary}.";

        var tacticalBullets = isResearcher
            ? """
                - Factory first: use moblib as new109 (mobile lab priority); defer filidx/cmplib and frminf escort to later quarters
                - Grant economic loop (@produce cash, @use farmng / @use hcdril, @produce energy, sell food via cargob)
                - @get 1 terran, food, and oil from HQ stacks onto new109 (moblab burns oil like trucks: 1 oil / 13 weeks move; no terair on terran ground)
                - @move adjacent anomaly region-id; @research that region (8-point threshold, +20 RP resolve)
                - Do not nest wind kits on moblab; HQ energy stays on coal plant. Defer UN town contract until survey column is staged
                """
            : """
                - Grant economic loop (@produce cash, @use farmng / @use hcdril, @produce energy, sell food via cargob)
                - Stage 30 iron + 2 titani on factory, use twnbld as new1, transfer 1 to faction 1 for the UN town contract
                """;

        var narrativeHook = isResearcher
            ? "adjacent HQ anomaly detected on grant exits, mobile lab field survey, 8-point investigation threshold"
            : "UN town charter via TRANSFER TO FACTION 1";

        return
        [
            new StoryChunkPrompt(
                "strategic",
                $"""
                {personaBrief}

                Report excerpt:
                {context.ReportExcerpt}

                Write ONLY the ## Strategic objective section (four quarters in Helios).
                Use a short prose paragraph or 4 bullets tied to persona doctrine and the open contract.
                Mention contract id, reward tech, and researcher/contractor priorities where relevant.
                No other headings.
                """),
            new StoryChunkPrompt(
                "tactical",
                $"""
                {personaBrief}

                Write ONLY ## Tactical objective with bullet list for the next quarter:
                {tacticalBullets}
                Use stack ids: {context.StackIdsSummary}.
                No other headings.
                """),
            new StoryChunkPrompt(
                "narrative",
                $"""
                {personaBrief}

                Write ONLY ## Narrative, 150-250 words hard SF prose for turn {context.ReportTurn} on the home grant,
                gold Helios light on Arbor, {narrativeHook}.
                No other headings.
                """),
        ];
    }

    public static string AssembleStory(string factionName, int reportTurn, IEnumerable<string> sectionBodies)
    {
        var sections = sectionBodies
            .Select(body => body.Trim())
            .Where(body => body.Length > 0)
            .ToList();

        return $"# {factionName} — turn {reportTurn}{Environment.NewLine}{Environment.NewLine}"
            + string.Join(Environment.NewLine + Environment.NewLine, sections)
            + Environment.NewLine;
    }

    private static string BuildReportExcerpt(string reportText, int maxLines)
    {
        var lines = reportText
            .Replace("\r\n", "\n")
            .Split('\n')
            .Take(maxLines);
        return string.Join(Environment.NewLine, lines);
    }

    private static string ExtractContractHint(string reportText)
    {
        foreach (var line in reportText.Split('\n'))
        {
            if (line.Contains("Contract reports:", StringComparison.OrdinalIgnoreCase)
                || line.TrimStart().StartsWith("CT", StringComparison.Ordinal))
            {
                return line.Trim();
            }
        }

        return "open UN give-module town contract on home grant";
    }

    private static string ExtractStackIds(string reportExcerpt, int factionId)
    {
        var prefix = factionId.ToString();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in StackIdRegex().Matches(reportExcerpt))
        {
            var id = match.Groups[1].Value;
            if (id.StartsWith(prefix, StringComparison.Ordinal))
            {
                ids.Add(id);
            }
        }

        return ids.Count == 0
            ? "use ids from report"
            : string.Join(", ", ids.OrderBy(id => id, StringComparer.Ordinal));
    }

    private static string SummarizePersona(string personaText)
    {
        var preference = PersonaFieldRegex("Preference").Match(personaText);
        var home = PersonaFieldRegex("Home").Match(personaText);
        var parts = new List<string>();
        if (preference.Success)
        {
            parts.Add(preference.Groups[1].Value.Trim());
        }

        if (home.Success)
        {
            parts.Add(home.Groups[1].Value.Trim());
        }

        return parts.Count == 0 ? "campaign Interest on Arbor in Helios" : string.Join("; ", parts);
    }

    private static Regex PersonaFieldRegex(string label) =>
        new($@"##?\s*{label}\s*:?\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    [GeneratedRegex(@"\[(\d{6})\]")]
    private static partial Regex StackIdRegex();
}

public sealed record StoryDraftContext(
    string FactionName,
    int FactionId,
    int ReportTurn,
    string PersonaText,
    string ReportExcerpt,
    string ContractHint,
    string StackIdsSummary)
{
    public bool HasPriorStory { get; init; }
}

public sealed record StoryChunkPrompt(string Name, string UserPrompt);
