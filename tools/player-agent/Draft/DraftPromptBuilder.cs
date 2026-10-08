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
        var moduleTypes = ReportStackCatalog.ParseModuleTypes(reportText);
        return new OrderDraftHints
        {
            PersonaPreference = VerbInference.DetectPersonaPreference(combined),
            TacticalObjective = ExtractSection(objectiveText, "Tactical objective"),
            AnomalyRegionId = ExtractAnomalyRegionId(reportText, objectiveText),
            DraftTurn = draftTurn,
            WindPowerGrant = ReportStackCatalog.GrantUsesWindPowerPlant(reportText),
            GrantResourceIds = ReportRegionGraph.ParseGrantRegionResourceIds(reportText),
            FieldedTanks = moduleTypes.Values.Any(t => t.Equals("tanks", StringComparison.OrdinalIgnoreCase)),
            BankBalance = ReportRegionGraph.ParseBankBalance(reportText),
            FaunaFactionIdsOnPlanet = ReportBattleIntel.FaunaFactionsOnPlanet(reportText),
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

        AppendStackCatalog(builder, reportText, hints);
        AppendGrantRegionNavigation(builder, reportText);
        AppendGrantResourceConstraints(builder, reportText, hints);

        if (!string.IsNullOrWhiteSpace(hints.TacticalObjective))
        {
            builder.AppendLine("## Tactical objective (implement this quarter)");
            builder.AppendLine(hints.TacticalObjective.Trim());
            builder.AppendLine();
        }

        builder.AppendLine("## Example output shape");
        builder.AppendLine(BuildExampleOutput(hints));
        if (!string.IsNullOrWhiteSpace(reportText))
        {
            var templateStacks = ReportStackCatalog.ParseOrdersTemplateStackIds(reportText);
            if (templateStacks.Count > 0)
            {
                builder.AppendLine("## Orders template coverage (mandatory)");
                builder.AppendLine(
                    "Include one `#modulestack <id>` block for **every** stack id in the report Orders template "
                    + $"({string.Join(", ", templateStacks)}). "
                    + "Use an empty block (header only) when that stack has no verbs this turn. Do not omit stacks.");
                builder.AppendLine();
            }
        }

        builder.AppendLine("## Task");
        builder.AppendLine(BuildTask(hints));
        return builder.ToString().Trim();
    }

    private static void AppendStackCatalog(StringBuilder builder, string? reportText, OrderDraftHints hints)
    {
        var stackTypes = ReportStackCatalog.ParseModuleTypes(reportText);
        if (stackTypes.Count == 0)
        {
            return;
        }

        builder.AppendLine("## Stack id → module type (from report; verbs must match stack type)");
        foreach (var pair in stackTypes.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var hint = DescribeAllowedVerbsForModuleType(pair.Value, pair.Key, hints, reportText);
            builder.AppendLine(
                hint is null
                    ? $"- {pair.Key}: [{pair.Value}]"
                    : $"- {pair.Key}: [{pair.Value}] — {hint}");
        }

        builder.AppendLine();
    }

    private static void AppendGrantResourceConstraints(
        StringBuilder builder,
        string? reportText,
        OrderDraftHints hints)
    {
        if (hints.GrantResourceIds.Count == 0 && !hints.WindPowerGrant)
        {
            return;
        }

        builder.AppendLine("## Grant constraints (mandatory — quality gate)");
        if (hints.GrantResourceIds.Count > 0)
        {
            builder.AppendLine(
                "Home grant Resources line includes: "
                + string.Join(", ", hints.GrantResourceIds.OrderBy(static id => id, StringComparer.Ordinal)));
        }

        if (hints.WindPowerGrant)
        {
            var sdrillId = ReportStackCatalog.FindStackIdByModuleType(reportText, "sdrill");
            var wnplntId = ReportStackCatalog.FindStackIdByModuleType(reportText, "wnplnt");
            builder.AppendLine(
                "Wind-grant (Anvil): energy from `[wnplnt]` `@produce energy` + factory `use wndtrb` — not coal `[cplant]`.");
            builder.AppendLine(
                sdrillId is null
                    ? "Sdrill: `@use iminng` ONLY — never `@use hcdril` on wind grants."
                    : $"Sdrill {sdrillId}: `@use iminng` ONLY — never `@use hcdril` (no carbon/oil on Resources).");
            builder.AppendLine("Cargob: do not `@get all carbon from` sdrill; use `@get all iron from` sdrill when iminng runs.");
            if (wnplntId is not null)
            {
                builder.AppendLine(
                    $"Expand wind from factory: `5 use wndtrb for {wnplntId}` + `+get` iron (×2 batches if powering existing + new cdrill) before `#modulestack newN` `has 1 cdrill`.");
                builder.AppendLine($"`#modulestack {wnplntId}`: `@produce energy` only — never `use wndtrb` here.");
            }
        }
        else if (hints.GrantResourceIds.Contains("carbon") || hints.GrantResourceIds.Contains("oil"))
        {
            var sdrillId = ReportStackCatalog.FindStackIdByModuleType(reportText, "sdrill");
            if (sdrillId is not null)
            {
                builder.AppendLine(
                    $"Sdrill {sdrillId}: `@use hcdril` when carbon/oil on Resources; `@use iminng` only if iron listed — never both.");
            }
        }

        if (!hints.GrantResourceIds.Contains("titani"))
        {
            builder.AppendLine("No titani on Resources: omit `tminng` / titanium mining USE lines.");
        }

        builder.AppendLine("See retrieved `economic-wind-grant` / `economic-story` chunks when present.");
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

    private static string? DescribeAllowedVerbsForModuleType(
        string moduleType,
        string stackId,
        OrderDraftHints hints,
        string? reportText)
    {
        if (moduleType.Equals("sdrill", StringComparison.OrdinalIgnoreCase) && hints.WindPowerGrant)
        {
            return "`@use iminng` ONLY on wind grants — never `@use hcdril`";
        }

        return moduleType.ToLowerInvariant() switch
        {
            "cplant" => "@produce energy only (never @use hcdril / iminng on cplant)",
            "wnplnt" => "`@produce energy` only — factory builds turbines via `N use wndtrb for <this-id>` (never `use wndtrb` on this stack)",
            "sdrill" => hints.GrantResourceIds.Contains("iron") && !hints.GrantResourceIds.Contains("carbon")
                ? "`@use iminng` only (iron on Resources, no carbon/oil)"
                : "one `@use` only: hcdril if grant Resources list carbon/oil; iminng if iron — never both",
            "farms" => "@use farmng",
            "corphq" => "set hold 20 terran and @produce terran",
            "cargob" => hints.WindPowerGrant
                ? "@get food/iron; sell food — not @get carbon from sdrill on wind grants"
                : "@get / sell lines (not @use hcdril)",
            "factry" => "`N use wndtrb for <wnplnt-id>`, `use mcored as newN`, `use grndtr` — factory USE with +get from cargob (`+get 2 iron` only for grndtr)",
            _ => null,
        };
    }

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
                sell <market-buy-qty> food at <market-buy-price>

                #modulestack <cplant-id>
                get 10 carbon from <cargob-id>
                @produce energy

                #modulestack <cdrill-id>
                @give all to <cargob-id>
                2 use tminng
                use iminng
                10 use hcdril

                #modulestack <farms-id>
                @use farmng

                #modulestack <factry-id>
                grant item 25 iron to <cargob-id>
                use grndtr as new1 for <hq-id>
                +get 2 iron from <cargob-id>
                -use sdrill as new2 for <hq-id>
                +get 25 iron from <cargob-id>

                #modulestack new1
                +get 1 terran from <hq-id>
                +get 1 oil from <cargob-id>
                +get 8 food from <cargob-id>
                move R…
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
            if (hints.WindPowerGrant && hints.DraftTurn >= 2)
            {
                return """
                    #faction <id> "<password>"
                    #modulestack <moblab-id>
                    grant item 3 oil to <moblab-id>
                    grant item 8 food to <moblab-id>
                    move R00049
                    move R00056
                    @research R00056

                    #modulestack <wnplnt-id>
                    @produce energy
                    has 26 wnplnt
                    -synchro wind1

                    #modulestack <hq-id>
                    set hold 20 terran
                    @produce terran

                    #modulestack <cargob-id>
                    @get all food from <farms-id>
                    @get all iron from <sdrill-id>
                    sell 200 food at 2

                    #modulestack <sdrill-id>
                    @use iminng

                    #modulestack <factry-id>
                    5 use wndtrb for <wnplnt-id>
                    +get 5 iron from <cargob-id>
                    5 use wndtrb for <wnplnt-id>
                    +get 5 iron from <cargob-id>
                    use grndtr as new114 for <hq-id>
                    +get 2 iron from <cargob-id>

                    #modulestack <cdrill-id>
                    synchro wind1
                    -activate 1
                    2 use tminng
                    @use iminng
                    @give all to <cargob-id>

                    #modulestack <farms-id>
                    @use farmng
                    #end
                    """;
            }

            if (!hints.WindPowerGrant && hints.DraftTurn >= 2)
            {
                return """
                    #faction <id> "<password>"
                    #modulestack <hq-id>
                    set hold 20 terran
                    @produce terran

                    #modulestack <cplant-id>
                    @produce energy

                    #modulestack <cargob-id>
                    @get all food from <farms-id>
                    @get all carbon from <sdrill-id>
                    sell 200 food at 2

                    #modulestack <sdrill-id>
                    @use hcdril

                    #modulestack <factry-id>
                    use mcored as newN for <hq-id>
                    +get 25 iron from <cargob-id>
                    +get 10 titani from <cargob-id>

                    #modulestack newN
                    has 1 cdrill
                    -get 6 terran from <hq-id>
                    deactivate 1

                    #modulestack <moblab-id>
                    set online true
                    +get 1 terran from <hq-id>
                    +get 2 oil from <cargob-id>
                    +get 2 food from <cargob-id>
                    move R00028
                    @research R00028

                    #modulestack <farms-id>
                    @use farmng
                    #end
                    """;
            }

            if (hints.WindPowerGrant)
            {
                return """
                    #faction <id> "<password>"
                    #modulestack <wnplnt-id>
                    @produce energy

                    #modulestack <hq-id>
                    set hold 20 terran
                    @produce terran

                    #modulestack <cargob-id>
                    @get all food from <farms-id>
                    @get all iron from <sdrill-id>
                    sell 200 food at 2

                    #modulestack <sdrill-id>
                    @use iminng

                    #modulestack <factry-id>
                    grant technology mcored to <factry-id>
                    grant technology msrvtm to <factry-id>
                    5 use wndtrb for <wnplnt-id>
                    +get 5 iron from <cargob-id>
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
                    #end
                    """;
            }

            return """
                #faction <id> "<password>"
                #modulestack <cplant-id>
                @produce energy

                #modulestack <hq-id>
                set hold 20 terran
                @produce terran

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
                Write turn {hints.DraftTurn} orders for this contractor faction.
                Read **## Turn priority** in story.md: implement that quarter's focus (contract 50% / defence 25% / economy 15% / research 10% doctrine).
                Decompose the active CT into **tactical objectives** that fit one quarter: **energy → drill mix (`@give all`, `2 use tminng`, `use iminng`, `10 use hcdril`) → expand cplant/drill capacity when carbon/iron allow → oil scout (`grndtr` to first neighbor exit) → later `mobctr`/`[engtrk]`**. **Bootstrap:** when bank balance is **above ~5000** and the market does not sell needed inputs, prefer **`grant item … to <cargob-id>`** (or stack) before factory **`USE`** — do not stall a spare line waiting on **`iminng`** alone.
                **CEO `[exmgmt]`:** charter CEOs start with executive management (not trainable). **`#person <ceo-id>`** then **`stack <factry-id>`** / **`<sdrill-id>`** / **`<cplant-id>`** (same region) for the quarter for **+25% productivity**; **`#person <ceo-id>`** + **`stack <hq-id>`** to return. (production group = factory USE set in field) for remote **`fossil`/`sdrill`** and parallel **`agrplx`** at contract site — not hauling `[farms]` on cargo trucks.
                Match **market sell qty/price** to the report town **buy** line. Reserve cplant fuel with **`get 10 carbon`** then `@produce energy`.
                When focus is **contract**, execute the named **Active contract** CT (skip completed charters like CT0008). Delivery `transfer` at the contract location when modules exist.
                Use only stack ids from the Orders template. See `Draft/contractor-story-to-orders.md`.
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
            if (hints.WindPowerGrant)
            {
                var turn2Wind = hints.DraftTurn >= 2
                    ? """
                      Turn 2+ (tech already on factory): **no** repeat `grant technology` / **no** `use msrvtm` when moblab already exists — use `use grndtr` or `move` the existing moblab.
                      Factory **must** include **`5 use wndtrb for <wnplnt-id>`** with **`+get` iron** (×2 batches when powering existing + new cdrill) **before** `#modulestack newN` `has 1 cdrill`.
                      """
                    : string.Empty;
                return $"""
                    Write turn {hints.DraftTurn} orders for this economic faction (wind grant — read ## Grant constraints).
                    Energy FIRST: `#modulestack <wnplnt-id>` `@produce energy` only; **`N use wndtrb for <wnplnt-id>` on factory stack only** — never on wnplnt stack.
                    HQ: `set hold 20 terran` and `@produce terran`. Sdrill: **`@use iminng` only** — never `@use hcdril` on Anvil wind grants.
                    Cargob: `@get all food from` farms; `@get all iron from` sdrill — not `@get all carbon from` sdrill. Sell food at report market price.
                    Factory: `use mcored as newN` with `+get` iron/titani; nest `has 1 cdrill` only after wind margin.
                    {turn2Wind}
                    Use only stack ids from the Orders template. Lowercase immediate verbs; leftover lines use @ prefix.
                    """;
            }

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
            if (hints.FieldedTanks && hints.DraftTurn >= 2)
            {
                var faunaIds = hints.FaunaFactionIdsOnPlanet.Count > 0
                    ? string.Join(", ", hints.FaunaFactionIdsOnPlanet.OrderBy(id => id))
                    : "15 on Anvil (from Battles report)";
                var cashNote = hints.BankBalance is < 2000
                    ? "Bank under 2000: HQ **`@produce cash`** — not `@produce terran`; omit repeat `set hold 20 terran`."
                    : "HQ `@produce terran` or `@produce cash` per bank balance.";
                return $"""
                    Write turn {hints.DraftTurn} orders for this military faction (fielded armor — post-bootstrap).
                    `DECLARE FACTION <id> ENEMY` only for fauna on **this planet** (report battles: {faunaIds}) — not off-world ids.
                    {cashNote} **Do not sell food.** Wnplnt `@produce energy`; cargob `@get all food/iron`; sdrill `@use iminng`; farms `@use farmng` — no spurious `set online true`.
                    Damaged tanks: **`@repair all`**, **`-move`** back to grant (HQ region), then **`-give N copper|iron|titani`** to cargob — **never `@give all`** (it strips crew/fuel/food).
                    Hungry tanks in transit: **`grant item 32 food`** / **`grant item 8 oil`** to the tank stack id, then **`-move`** to scout; next bootstrap use **`move R…`** with **`+get 32 food from cargob`** under the move (not `has 1 tanks` + `-get food` + `-move`).
                    Avoid `-move` into regions where you lost until massed. Omit redundant `-get` on crewed tanks and repeat `tactic destroy` without a new `-move`.
                    Use only stack ids from the Orders template. `@` on continuous lines only (@produce, @get all, @use, @repair, @give).
                    """;
            }

            var faunaRegion = hints.AnomalyRegionId ?? "adjacent anomaly region-id from grant exits (Mid Vale)";
            return $"""
                Write turn {hints.DraftTurn} orders for this military faction.
                Fauna **14** on Arbor, **15** on Anvil — hostile packs attack on contact; `DECLARE FACTION <id> ENEMY` only for fauna on **this** planet after rumors.
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
    public bool WindPowerGrant { get; init; }
    public IReadOnlySet<string> GrantResourceIds { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public bool FieldedTanks { get; init; }
    public int? BankBalance { get; init; }
    public IReadOnlySet<int> FaunaFactionIdsOnPlanet { get; init; } = new HashSet<int>();
}
