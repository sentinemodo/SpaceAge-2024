using System.Text;
using System.Text.RegularExpressions;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class DraftPromptBuilder
{
    public const string SystemPrompt =
        "You generate SpaceAge PBEM order files. Reply with order file text only: #faction, "
        + "#modulestack headers, verb lines, optional ; comments you write yourself, and #end. "
        + "Do not paste report Orders template comment lines (; + …). Do not use #person unless TRAIN/ACTIVE/SEE. "
        + "Turn 1: every HQ stack needs `set hold 20 terran` beside `@produce terran`. "
        + "Disabled stacks cannot move: on mobile stacks use definite `get`/`-get`/`move`/`-move` (like use/repair); use `@research` for continuous research (like `@produce`). Stage terran crew and oil before move. "
        + "@use farmng only on farms stacks; @use hcdril only on sdrill stacks (match Orders template module types). "
        + "No prose, no markdown fences, no numbered lists, no explanations.";

    public static string BuildRetrievalQuery(string? objectiveText, string? reportText, string? personaText = null)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(personaText))
        {
            builder.AppendLine(personaText.Trim());
        }

        if (!string.IsNullOrWhiteSpace(objectiveText))
        {
            builder.AppendLine(objectiveText.Trim());
        }

        if (!string.IsNullOrWhiteSpace(reportText))
        {
            builder.AppendLine(PromptPackBuilder.BuildReportExcerpt(reportText, maxCharacters: 1200));
        }

        builder.AppendLine(
            "MOVE readiness: disabled stacks cannot move. Before @move stage @get terran crew, "
            + "@get oil fuel for trucks/tanks/moblab, @get h2o2 for surface-orbit hops, @repair damage, "
            + "use spctrl bridge for ship hulls.");
        var query = builder.ToString().Trim();
        return query.Length == 0 ? "SpaceAge order syntax and stack movement" : query;
    }

    public static OrderDraftHints BuildHints(
        string? objectiveText,
        string? reportText,
        string? personaText,
        int draftTurn = 2)
    {
        var combined = string.Join(
            '\n',
            new[] { personaText, objectiveText, reportText }.Where(text => !string.IsNullOrWhiteSpace(text)));
        return new OrderDraftHints
        {
            PersonaPreference = VerbInference.DetectPersonaPreference(combined),
            TacticalObjective = ExtractSection(objectiveText, "Tactical objective"),
            AnomalyRegionId = ExtractAnomalyRegionId(reportText, objectiveText),
            DraftTurn = draftTurn,
        };
    }

    public static string BuildChatPrompt(
        string promptPack,
        IReadOnlyList<RetrievalResult> retrievedChunks,
        string? ordersTemplate,
        OrderDraftHints hints,
        string? reportText = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine(promptPack.Trim());
        builder.AppendLine();

        if (retrievedChunks.Count > 0)
        {
            builder.AppendLine("## Retrieved reference chunks");
            foreach (var result in retrievedChunks)
            {
                var metadata = result.Chunk.Chunk.Metadata;
                builder.AppendLine(
                    $"### [{metadata.Doc}] {metadata.Heading ?? metadata.Verb ?? "chunk"} (score {result.Score:F2})");
                builder.AppendLine(result.Chunk.Chunk.Content.Trim());
                builder.AppendLine();
            }
        }

        if (!string.IsNullOrWhiteSpace(ordersTemplate))
        {
            builder.AppendLine("## Orders template from report (reuse these ids; add verb lines under each stack)");
            builder.AppendLine(ordersTemplate.Trim());
            builder.AppendLine();
        }

        AppendStackCatalog(builder, reportText);
        AppendGrantRegionNavigation(builder, reportText);

        if (!string.IsNullOrWhiteSpace(hints.TacticalObjective))
        {
            builder.AppendLine("## Tactical objective (implement this quarter)");
            builder.AppendLine(hints.TacticalObjective.Trim());
            builder.AppendLine();
        }

        builder.AppendLine("## Example output shape");
        builder.AppendLine(BuildExampleOutput(hints));
        builder.AppendLine("## Task");
        builder.AppendLine(BuildTask(hints));
        return builder.ToString().Trim();
    }

    private static void AppendStackCatalog(StringBuilder builder, string? reportText)
    {
        var stackTypes = ReportStackCatalog.ParseModuleTypes(reportText);
        if (stackTypes.Count == 0)
        {
            return;
        }

        builder.AppendLine("## Stack id → module type (from report; verbs must match stack type)");
        foreach (var pair in stackTypes.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var hint = DescribeAllowedVerbsForModuleType(pair.Value);
            builder.AppendLine(
                hint is null
                    ? $"- {pair.Key}: [{pair.Value}]"
                    : $"- {pair.Key}: [{pair.Value}] — {hint}");
        }

        builder.AppendLine();
    }

    private static void AppendGrantRegionNavigation(StringBuilder builder, string? reportText)
    {
        var table = ReportRegionGraph.FormatGrantRegionNavigationTable(reportText);
        if (string.IsNullOrWhiteSpace(table))
        {
            return;
        }

        builder.AppendLine(table);
        builder.AppendLine();
    }

    private static string? DescribeAllowedVerbsForModuleType(string moduleType) =>
        moduleType.ToLowerInvariant() switch
        {
            "cplant" => "@produce energy only (never @use hcdril / iminng on cplant)",
            "wnplnt" => "@use wndtrb (never @use hcdril on wnplnt)",
            "sdrill" => "one `@use` only: hcdril if grant Resources list carbon/oil; iminng if iron — never both on the same sdrill",
            "farms" => "@use farmng",
            "corphq" => "set hold 20 terran and @produce terran",
            "cargob" => "@get / sell lines (not @use hcdril)",
            "factry" => "use <tech> as newN with +get from cargob (not @use hcdril)",
            _ => null,
        };

    public static string? ExtractOrdersTemplate(string? reportText)
    {
        if (string.IsNullOrWhiteSpace(reportText))
        {
            return null;
        }

        const string marker = "Orders Template:";
        var index = reportText.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        return reportText[(index + marker.Length)..].Trim();
    }

    private static string BuildExampleOutput(OrderDraftHints hints)
    {
        if (string.Equals(hints.PersonaPreference, "researcher", StringComparison.OrdinalIgnoreCase))
        {
            var anomaly = hints.AnomalyRegionId ?? "R00011";
            return $$"""
                #faction <id> "<password>"
                #modulestack <hq-id>
                set hold 20 terran
                @produce terran

                #modulestack <cargob-id>
                grant item 2 iron to <cargob-id>
                grant item 2 silici to <cargob-id>
                @get all food from <farms-id>
                @get all carbon from <sdrill-id>
                sell 200 food at average

                #modulestack <sdrill-id>
                @use hcdril

                #modulestack <farms-id>
                @use farmng

                #modulestack <cplant-id>
                @produce energy

                #modulestack <factry-id>
                grant technology msrvtm to <factry-id>
                use msrvtm as new109 for <hq-id>
                +get 2 iron from <cargob-id>
                +get 2 silici from <cargob-id>

                #modulestack new109
                move {{anomaly}}
                +get 1 terran from <hq-id>
                +get 2 oil from <cargob-id>
                +get 2 food from <cargob-id>
                @research {{anomaly}}
                #end
                """;
        }

        if (string.Equals(hints.PersonaPreference, "contractor", StringComparison.OrdinalIgnoreCase))
        {
            return """
                #faction <id> "<password>"
                #modulestack <hq-id>
                set hold 20 terran
                @produce terran

                #modulestack <cargob-id>
                @get all food from <farms-id>
                @get all carbon from <cdrill-id>
                sell 200 food at average

                #modulestack <cdrill-id>
                @use hcdril

                #modulestack <farms-id>
                @use farmng

                #modulestack <cplant-id>
                @produce energy

                #modulestack <factry-id>
                get 30 iron from <cargob-id>
                get 2 titani from <cargob-id>
                use twnbld as new1

                #modulestack new1
                transfer 1 to faction 1
                #end
                """;
        }

        if (string.Equals(hints.PersonaPreference, "absent-player", StringComparison.OrdinalIgnoreCase))
        {
            return """
                #faction <id> "<password>"
                #modulestack <hq-id>
                set hold 20 terran
                @produce cash

                #modulestack <cargob-id>
                @get all food from <farms-id>
                @get all carbon from <sdrill-id>

                #modulestack <sdrill-id>
                @use hcdril

                #modulestack <farms-id>
                @use farmng

                #modulestack <cplant-id>
                @produce energy
                #end
                """;
        }

        if (string.Equals(hints.PersonaPreference, "economic", StringComparison.OrdinalIgnoreCase))
        {
            return """
                #faction <id> "<password>"
                #modulestack <cplant-id>
                @produce energy

                #modulestack <hq-id>
                set hold 20 terran
                @produce terran

                #modulestack <wnplnt-id>
                @produce energy
                use wndtrb as new109 for <wnplnt-id>

                #modulestack <sdrill-id>
                grant item 50 iron to <sdrill-id>
                grant item 10 titani to <sdrill-id>
                @use iminng

                #modulestack <factry-id>
                grant technology mcored to <factry-id>
                grant technology msrvtm to <factry-id>
                use mcored as new108 for <hq-id>
                +get 25 iron from <cargob-id>
                +get 10 titani from <cargob-id>
                use msrvtm as new110 for <hq-id>
                +get 25 iron from <cargob-id>
                +get 10 titani from <cargob-id>

                #modulestack new108
                has 1 cdrill
                -get 6 terran from <hq-id>
                deactivate 1

                #modulestack new110
                move <ground-exit-from-table>
                +get 1 terran from <hq-id>
                +get 2 oil from <cargob-id>
                +get 2 food from <cargob-id>

                #modulestack <farms-id>
                @use farmng

                #modulestack <cargob-id>
                @get all food from <farms-id>
                @get all carbon from <sdrill-id>
                sell 200 food at average
                #end
                """;
        }

        if (string.Equals(hints.PersonaPreference, "military", StringComparison.OrdinalIgnoreCase))
        {
            var safeScout = hints.AnomalyRegionId is null ? "R00014" : "R00014";
            var faunaRegion = hints.AnomalyRegionId ?? "R00009";
            return $$"""
                #faction <id> "<password>"
                DECLARE FACTION 14 ENEMY

                #modulestack <hq-id>
                set hold 20 terran
                grant item 16 terran to <hq-id>
                @produce terran

                #modulestack <cargob-id>
                grant item 50 iron to <cargob-id>
                grant item 20 oil to <cargob-id>
                grant item 10 titani to <cargob-id>
                @get all food from <farms-id>
                @get all carbon from <sdrill-id>

                #modulestack <sdrill-id>
                @use hcdril

                #modulestack <farms-id>
                @use farmng

                #modulestack <cplant-id>
                @produce energy

                #modulestack <factry-id>
                grant technology armcbt to <factry-id>
                use grndtr as new1 for <hq-id>
                +get 30 iron from <cargob-id>
                +get 2 titani from <cargob-id>
                use armcbt as new2 for <hq-id>
                +get 8 iron from <cargob-id>
                +get 2 titani from <cargob-id>
                use armcbt as new3 for <hq-id>
                +get 8 iron from <cargob-id>
                +get 2 titani from <cargob-id>

                #modulestack new1
                move {{safeScout}}
                +get 1 terran from <hq-id>
                +get 2 oil from <cargob-id>
                +get 2 food from <cargob-id>

                #modulestack new2
                has 1 tanks
                -get 16 terran from <hq-id>
                -get 8 oil from <cargob-id>
                -get 32 food from <cargob-id>
                -move {{faunaRegion}}
                tactic destroy

                #modulestack new3
                has 1 tanks
                -get 16 terran from <hq-id>
                -get 8 oil from <cargob-id>
                -get 32 food from <cargob-id>
                -move {{faunaRegion}}
                tactic destroy
                #end
                """;
        }

        return """
            #faction <id> "<password>"
            #modulestack <hq-id>
            set hold 20 terran
            @produce terran

            #modulestack <cargob-id>
            @get all food from <farms-id>
            @get all carbon from <cdrill-id>
            sell 200 food at average

            #modulestack <cdrill-id>
            @use hcdril

            #modulestack <farms-id>
            @use farmng

            #modulestack <cplant-id>
            @produce energy
            #end
            """;
    }

    private static string BuildTask(OrderDraftHints hints)
    {
        if (string.Equals(hints.PersonaPreference, "researcher", StringComparison.OrdinalIgnoreCase))
        {
            var anomaly = hints.AnomalyRegionId ?? "the adjacent anomaly region-id from the report exits";
            return $"""
                Write turn {hints.DraftTurn} orders for this faction.
                Issue bank GRANT lines before moblab USE: `grant item 2 iron` + `grant item 2 silici` to cargob-id; `grant technology msrvtm to <factry-id>`; then `use msrvtm as newNNN for <hq-id>` with `+get 2 iron` and `+get 2 silici` from cargob.
                Run the grant economic loop (HQ `@produce terran`, cargob `@get all food/carbon`, sell food, sdrill `@use hcdril` only — no `@use iminng` turn 1).
                On the new moblab stack: definite `move {anomaly}`; `+get` terran/oil/food; continuous `@research {anomaly}`.
                Defer UN town charter until the survey column is staged. Use only stack ids from the Orders template.
                """;
        }

        if (string.Equals(hints.PersonaPreference, "contractor", StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Write turn {hints.DraftTurn} orders for this faction.
                Run the grant economic loop, then factory-build `twnbld` and `transfer 1 to faction 1` for the open UN town contract if due this quarter.
                Use only stack ids from the Orders template.
                """;
        }

        if (string.Equals(hints.PersonaPreference, "absent-player", StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Write turn {hints.DraftTurn} orders for this faction (absent-player / maintenance only).
                HQ: `set hold 20 terran` and `@produce cash` (NOT `@produce terran` — crew upkeep).
                Cargob: `@get all food from <farms-id>`, `@get all carbon from <sdrill-id>` for cplant fuel (not from cplant).
                Sdrill: `@use hcdril` on the sdrill stack (coal/carbon for cplant). Cplant: `@produce energy`. Farms: `@use farmng`. Do NOT `@use iminng` (iron) this quarter.
                Do NOT sell food or resources. No factory USE, tanks, or town charter unless REPAIR is required on a damaged module.
                Use only stack ids from the Orders template and the stack catalog. Lowercase immediate verbs; leftover lines use @ prefix.
                """;
        }

        if (string.Equals(hints.PersonaPreference, "economic", StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Write turn {hints.DraftTurn} orders for this faction.
                Priority: `@produce energy` on cplant FIRST — HQ is often 80/80 with no headroom; do not activate a nested cdrill (+5 draw) until energy margin allows.
                HQ: `set hold 20 terran` and `@produce terran`. Issue GRANT lines (bank debit) before bootstrap USE: `grant item 50 iron` + `grant item 10 titani` to <sdrill-id>. Sdrill: **one** `@use` only — `@use hcdril` if grant Resources lists carbon/oil; `@use iminng` if iron — never both on the same sdrill stack.
                Factory: `grant technology mcored to <factry-id>`, `grant technology msrvtm to <factry-id>` (numeric stack id — GRANT may sit under `#faction` or `#modulestack`), then `use mcored as newNNN for <hq-id>` with `+get` 25 iron / 10 titani from cargob; on `#modulestack newNNN`: `has 1 cdrill`, `-get` 6 terran, `deactivate 1`. Ground `move R…` targets must be listed under Exits from the grant region in the report.
                Cargob: `@get all food/carbon`, sell surplus food. Turn 2+: msrvtm/moblab scout when energy allows; defer UN town charters until home grant is maxed.
                Use only stack ids from the Orders template. Lowercase immediate verbs (grant, get, use); leftover lines use @ prefix.
                """;
        }

        if (string.Equals(hints.PersonaPreference, "military", StringComparison.OrdinalIgnoreCase))
        {
            var faunaRegion = hints.AnomalyRegionId ?? "adjacent anomaly region-id from grant exits (Mid Vale)";
            return $"""
                Write turn {hints.DraftTurn} orders for this military faction.
                Fauna **14** on Arbor, **15** on Anvil — hostile packs attack on contact; optional `DECLARE FACTION <id> ENEMY` after rumors.
                HQ: `set hold 20 terran` and `@produce terran`. **Do not sell food** — tanks need 16 food + 4 oil per quarter.
                Cargob: **`grant item 50 iron`**, **`grant item 20 oil`**, **`grant item 10 titani`** to cargob-id, then `@get all food/carbon`.
                Factory: **`grant technology armcbt to <factry-id>`**, then **`use grndtr`** scout (new1) with **`+get`**, then **two `use armcbt`** (new2, new3) each with **`+get` iron/titani**.
                Scout `#modulestack new1`: **`move` then `+get` terran/oil/food** (no `@` on move/get).
                Tanks `#modulestack new2/new3`: **`has 1 tanks`**, **`-get` 16 terran / 8 oil / 32 food**, **`-move` {faunaRegion}**, **`tactic destroy`** — never `@move`, `@tactic`, `@active`, or bare `move` under `has`.
                Defer UN town charter until Mid Vale fauna is cleared.
                Use only stack ids from the Orders template. `@` only on continuous HQ/cargob pulls (@produce, @get all, @use).
                """;
        }

        return $"""
            Write turn {hints.DraftTurn} orders for this faction.
            Implement the grant economic bootstrap loop and any factory builds from the tactical objective.
            Before any @move: stage crew (@get terran) and fuel (@get oil for trucks/tanks/moblab; @get h2o2 for orbit hops) on that stack — disabled units cannot move.
            Use only stack/person ids from the report or template. Do not reply with only active/see lines.
            """;
    }

    private static string? ExtractSection(string? markdown, string heading)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return null;
        }

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

        var text = capture.ToString().Trim();
        return text.Length == 0 ? null : text;
    }

    private static string? ExtractAnomalyRegionId(string? reportText, string? objectiveText = null)
    {
        if (!string.IsNullOrWhiteSpace(objectiveText))
        {
            foreach (Match match in RegionIdRegex().Matches(objectiveText))
            {
                var regionId = match.Groups[1].Value;
                if (ReportMentionsAnomaly(reportText, regionId))
                {
                    return regionId;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(reportText))
        {
            return null;
        }

        foreach (Match match in AnomalyExitRegex().Matches(reportText))
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    private static bool ReportMentionsAnomaly(string? reportText, string regionId) =>
        !string.IsNullOrWhiteSpace(reportText)
        && reportText.Contains($"[{regionId}]", StringComparison.OrdinalIgnoreCase)
        && reportText.Contains("anomaly detected", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"\[(R\d{5})\][^\n\r]*anomaly detected", RegexOptions.IgnoreCase)]
    private static partial Regex AnomalyExitRegex();

    [GeneratedRegex("\\[(R\\d{5})\\]")]
    private static partial Regex RegionIdRegex();
}

public sealed class OrderDraftHints
{
    public string? PersonaPreference { get; init; }
    public string? TacticalObjective { get; init; }
    public string? AnomalyRegionId { get; init; }
    public int DraftTurn { get; init; } = 2;
}
