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
        var isEconomic = context.PersonaText.Contains("Preference: economic", StringComparison.OrdinalIgnoreCase);
        var isMilitary = context.PersonaText.Contains("Preference: military", StringComparison.OrdinalIgnoreCase);
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
            : isEconomic
                ? """
                    - Factory first: seeded cdrill tech (−1000 balance at init) — get 25 iron + 10 titani, use cdrill as new108 for the first core drill on the grant
                    - Energy before scale: @produce energy on cplant; add fossil/cplant copies when carbon tight before stacking more drills
                    - Grant loop on surface drill until core drill online (@use hcdril / @use iminng on sdrill-id, @use farmng, sell surplus food)
                    - Scout with moblib/moblab carrying a cdrill technology copy (not trucks): adjacent exits show deep pocket of resources detected; move lab into pocket cell to read Deep resources assays
                    - Defer CT town charter until home grant production is maxed; next build agrplx farms or cdrill on deep pockets by market bottleneck
                    """
                : isMilitary
                    ? """
                        - Turn-1 fauna rumor counts as contact: DECLARE FACTION 14 ENEMY before engaging Arbor Fauna
                        - Grant economic loop (@produce cash, @use farmng / @use hcdril, @produce energy, sell food via cargob)
                        - Factory: use grndtr scout truck to Farm Belt (safe adjacent grant); use armcbt tanks for Mid Vale fauna cull
                        - Tanks @move Mid Vale [R00009], @attack brush pack; claim CT0016 (1000 cash bounty) when stack cleared
                        - Secure Mid Vale oil after cull; defer CT0006 UN town charter until armored lane is safe
                        """
                    : """
                        - Grant economic loop (@produce cash, @use farmng / @use hcdril, @produce energy, sell food via cargob)
                        - Stage 30 iron + 2 titani on factory, use twnbld as new1, transfer 1 to faction 1 for the UN town contract
                        """;

        var narrativeHook = isResearcher
            ? "adjacent HQ anomaly detected on grant exits, mobile lab field survey, 8-point investigation threshold"
            : isEconomic
                ? "surface drill bootstrap, paid cdrill tech copy, moblab deep-pocket scouting column before grant expansion"
                : isMilitary
                    ? "anonymous Mid Vale fauna rumor, scout truck on Farm Belt, armored column clearing brush for CT0016 cash"
                    : "UN town charter via TRANSFER TO FACTION 1";

        var strategicGuidance = isEconomic
            ? """
                Four-quarter arc: max home-grant extraction (surface drill → core drill → farms/deep pockets), moblab deep-pocket survey column, then UN town charter when production is saturated.
                Name CT0007 and preventive servicing reward only as a deferred milestone — do NOT make hosting the UN market town the turn-1 priority.
                """
            : isResearcher
                ? """
                    Four-quarter arc: moblab anomaly survey and RESEARCH on adjacent grant exits, grant economic loop, defer UN town charter until survey column is staged.
                    Mention contract id and reward tech as later-quarter milestones.
                    """
                : isMilitary
                    ? """
                        Four-quarter arc: clear adjacent fauna (CT0016/CT0019 cash bounties), secure oil pockets, cplant/fossil energy for barracks, frminf infantry from barracks, then Gate orbit when ready.
                        Defer CT0006 UN town charter until the armored lane is secure. Fauna factions 14-17 start neutral — declare only after rumor or scout contact.
                        """
                    : """
                        Four quarters tied to persona doctrine and the open contract; mention contract id and reward tech where relevant.
                        """;

        return
        [
            new StoryChunkPrompt(
                "strategic",
                $"""
                {personaBrief}

                Report excerpt:
                {context.ReportExcerpt}

                Write ONLY the ## Strategic objective section (four quarters in Helios).
                {strategicGuidance}
                Use a short prose paragraph or 4 bullets. No other headings.
                """),
            new StoryChunkPrompt(
                "tactical",
                $"""
                {personaBrief}

                Write ONLY ## Tactical objective (bullet list for the next quarter).
                {tacticalBullets}
                Substitute real stack ids from the report where placeholders appear: {context.StackIdsSummary}.
                Do not echo these instructions. No other headings.
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
        var hints = new List<string>();
        foreach (var line in reportText.Split('\n'))
        {
            if (line.Contains("Rumors:", StringComparison.OrdinalIgnoreCase)
                || line.Contains("Hostile fauna", StringComparison.OrdinalIgnoreCase))
            {
                hints.Add(line.Trim());
            }

            if (line.TrimStart().StartsWith("CT", StringComparison.Ordinal))
            {
                hints.Add(line.Trim());
            }
        }

        return hints.Count == 0
            ? "open UN give-module town contract on home grant"
            : string.Join("; ", hints.Take(4));
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
