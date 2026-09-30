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
        var reportExcerpt = BuildReportExcerpt(reportText, maxLines: 120);
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
            {(IsContractorPersona(context.PersonaText) ? ContractorTurnPriorityHeadingRequirement : string.Empty)}
            Use stack ids from report: {context.StackIdsSummary}.
            Contract: {context.ContractHint}.
            """;
    }

    public static IReadOnlyList<StoryChunkPrompt> BuildChunkPrompts(
        StoryDraftContext context,
        bool omitStrategicSection = false)
    {
        var isContractor = IsContractorPersona(context.PersonaText);
        var isResearcher = context.PersonaText.Contains("Preference: researcher", StringComparison.OrdinalIgnoreCase);
        var isEconomic = context.PersonaText.Contains("Preference: economic", StringComparison.OrdinalIgnoreCase);
        var isMilitary = context.PersonaText.Contains("Preference: military", StringComparison.OrdinalIgnoreCase);
        var isAbsentPlayer = context.PersonaText.Contains("Preference: absent-player", StringComparison.OrdinalIgnoreCase)
            || context.PersonaText.Contains("Persona: absent-player", StringComparison.OrdinalIgnoreCase);
        var personaBrief =
            $"{context.FactionName} faction {context.FactionId}. {SummarizePersona(context.PersonaText)} "
            + $"Contract: {context.ContractHint}. Stacks: {context.StackIdsSummary}.";

        var tacticalBullets = isResearcher
            ? """
                - HQ: set hold 20 terran beside @produce terran
                - GRANT bootstrap: grant item 2 iron + 2 silici to cargob id; grant technology msrvtm to factry stack id; use msrvtm as newNNN with +get iron/silici from cargob
                - Grant economic loop (@use farmng, @use hcdril on sdrill — no iminng turn 1, @produce energy, cargob @get all + sell food)
                - Moblab stack: +get terran/oil/food; move adjacent anomaly region-id; @research that region (+20 RP resolve)
                - Defer filidx/cmplib, frminf escort, and UN town contract until survey column is staged
                """
                : isEconomic
                ? """
                    - HQ: set hold 20 terran and @produce terran; @produce energy on cplant before nested cdrill draw
                    - GRANT bootstrap: grant item iron/titani to sdrill id, grant technology mcored + msrvtm to factry stack id, then ONE drill @use matching grant Resources (hcdril if carbon/oil; iminng if iron — never both), use mcored as newN, use msrvtm moblab as newN toward deep metals, expand wnplnt with use wndtrb + @produce energy before cdrill nest
                    - Nest cdrill: has 1 cdrill, -get 6 terran, deactivate 1 until energy margin; @use farmng, cargob @get all + sell food
                    - Scout with msrvtm/moblab carrying an mcored technology copy (not trucks): adjacent exits show deep pocket of resources detected; move lab into pocket cell to read Deep resources assays
                    - Defer CT town charter until home grant production is maxed; next build agrplx farms or cdrill on deep pockets by market bottleneck
                    """
                : isMilitary
                    ? """
                        - Turn-1 fauna rumor counts as contact: DECLARE FACTION 14 ENEMY before engaging Arbor Fauna
                        - HQ: set hold 20 terran and @produce terran; DECLARE hostile fauna when rumors confirm contact
                        - Cargob: grant item iron/oil/titani to cargob, then @get all food/carbon; grant loop + @use hcdril, @produce energy — no sell food
                        - Factory: grant technology armcbt to factry stack id, then grndtr scout and two armcbt builds with +get iron/titani
                        - Both tank squads: has 1 tanks, -get provisioning, -move Mid Vale [R00009], tactic destroy; claim CT0016 (1000 cash bounty) when stack cleared
                        - Secure Mid Vale oil after cull; defer CT0006 UN town charter until armored lane is safe
                        """
                : isAbsentPlayer
                    ? """
                        - HQ: set hold 20 terran and @produce cash (not @produce terran — minimizes crew upkeep)
                        - Cplant @produce energy; cargob @get all food from farms and @get all carbon from sdrill for coal-plant fuel
                        - Sdrill @use hcdril (carbon for cplant); @get all carbon on cargob; no @use iminng — no sell food or resources
                        - No factory USE, tanks, town charter, or fauna offensives — upkeep and existing grant only
                        - REPAIR if report shows damage; RESEARCH only if a lab exists and energy margin allows
                        """
                : isContractor
                    ? """
                        - Match bullets to **## Turn priority** focus (contract 50% / defence 25% / economy 15% / research 10% doctrine)
                        - **contract:** stage `use twnbld` / wind / food / drill modules per active CT; `transfer 1 to faction 1` on give-module jobs; `CONTRACT` accept or presence steps from rumors
                        - **defence:** escort trucks, clear fauna lanes when rumors confirm; DECLARE FACTION 14 ENEMY only after contact intel
                        - **economy:** HQ `@produce terran`, grant loop `@use farmng` / `@use hcdril`, `@produce energy`, stage 30 iron + 2 titani on factory cargob feeds
                        - **research:** moblab + `@research` only when a contract reward or wreck charter requires it
                        """
                    : """
                        - Grant economic loop (@produce cash, @use farmng / @use hcdril, @produce energy, sell food via cargob)
                        - Stage 30 iron + 2 titani on factory, use twnbld as new1, transfer 1 to faction 1 for the UN town contract
                        """;

        var narrativeHook = isResearcher
            ? "adjacent HQ anomaly detected on grant exits, mobile lab field survey, 8-point investigation threshold"
            : isEconomic
                ? "surface drill bootstrap, paid mcored tech copy, moblab deep-pocket scouting column before grant expansion"
                : isMilitary
                    ? "anonymous Mid Vale fauna rumor, scout truck on Farm Belt, two armored tank squads clearing brush for CT0016 cash"
                : isAbsentPlayer
                    ? "quiet grant maintenance under gold Helios light, coal plant and drills keeping the line fed while the CEO holds the charter paperwork"
                : isContractor
                    ? "charter mercenary boardroom, UN contract postings and rumor releases, trucks scouting for the quarter's chosen focus"
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
                : isContractor
                    ? """
                        Four-quarter arc weighted to contractor doctrine: ~50% quarters push open UN contracts (name CT ids), ~25% defence of grant lanes and escorts, ~15% economy staging for module builds, ~10% research/moblab when rewards require it.
                        Quarters may blend focuses when contract execution needs economy or research first.
                        """
                    : """
                        Four quarters tied to persona doctrine and the open contract; mention contract id and reward tech where relevant.
                        """;

        var prompts = new List<StoryChunkPrompt>();
        if (!omitStrategicSection)
        {
            prompts.Add(new StoryChunkPrompt(
                "strategic",
                $"""
                {personaBrief}

                Report excerpt:
                {context.ReportExcerpt}

                Write ONLY the ## Strategic objective section (four quarters in Helios).
                {strategicGuidance}
                Use a short prose paragraph or 4 bullets. No other headings.
                """));
        }

        var reportBlock = omitStrategicSection
            ? $"""

            Report excerpt (turn {context.ReportTurn}; honor new contracts/rumors and stack ids):
            {context.ReportExcerpt}
            """
            : string.Empty;

        if (isContractor)
        {
            prompts.Add(new StoryChunkPrompt(
                "turn-priority",
                $"""
                {personaBrief}
                {reportBlock}

                Write ONLY ## Turn priority for turn {context.ReportTurn}.
                {ContractorTurnPriorityBodyInstructions}
                Open contracts / rumors hint: {context.ContractHint}.
                Pick **contract** when an open CT is achievable this quarter; else defence if fauna/rumors threaten; else economy staging; else research.
                No other headings.
                """));
        }

        prompts.Add(new StoryChunkPrompt(
            "tactical",
            $"""
            {personaBrief}
            {reportBlock}

            Write ONLY ## Tactical objective (bullet list for the next quarter).
            {(isContractor ? "Execute the focus declared in ## Turn priority (same story)." : string.Empty)}
            {tacticalBullets}
            Substitute real stack ids from the report where placeholders appear: {context.StackIdsSummary}.
            Do not echo these instructions. No other headings.
            """));

        prompts.Add(new StoryChunkPrompt(
            "narrative",
            $"""
            {personaBrief}
            {reportBlock}

            Write ONLY ## Narrative, 150-250 words hard SF prose for turn {context.ReportTurn} on the home grant,
            gold Helios light on Arbor, {narrativeHook}.
            Reflect this report and any new contract/rumor releases; do not rewrite the four-quarter strategic arc.
            {(isContractor ? "State the quarter's Turn priority focus and name the active CTxxxx when focus is contract." : string.Empty)}
            No other headings.
            """));

        return prompts;
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

    private static bool IsContractorPersona(string personaText) =>
        personaText.Contains("Preference: contractor", StringComparison.OrdinalIgnoreCase);

    private const string ContractorTurnPriorityHeadingRequirement =
        """
        ## Turn priority
        (contractor: Focus this quarter contract|defence|economy|research; Rationale; Active contract CTxxxx when contract focus)
        """;

    private const string ContractorTurnPriorityBodyInstructions =
        """
        Required bullets under the heading:
        - **Focus this quarter:** contract | defence | economy | research (one primary; doctrine weights 50/25/15/10)
        - **Rationale:** one sentence from this report or rumor
        - **Active contract:** `CTxxxx` — required when focus is **contract** (id + next milestone); omit when not contract-focused
        - **Supporting work:** optional one line when economy/research/defence aligns with the contract
        """;

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
