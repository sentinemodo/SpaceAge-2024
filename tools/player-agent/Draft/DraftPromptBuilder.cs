using System.Text;
using System.Text.RegularExpressions;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class DraftPromptBuilder
{
    public const string SystemPrompt =
        "You generate SpaceAge PBEM order files. Reply with order file text only: #faction, "
        + "#modulestack / #person headers, verb lines, comments with ;, and #end. "
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
        OrderDraftHints hints)
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
                #modulestack <factry-id>
                get 2 iron from <cargob-id>
                get 2 silici from <cargob-id>
                use moblib as new109

                #modulestack <hq-id>
                @produce cash

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

                #modulestack new109
                @get 1 terran from <hq-id>
                @get 60 food from <cargob-id>
                @get 2 oil from <cargob-id>
                @move {{anomaly}}
                @research {{anomaly}}
                #end
                """;
        }

        if (string.Equals(hints.PersonaPreference, "contractor", StringComparison.OrdinalIgnoreCase))
        {
            return """
                #faction <id> "<password>"
                #modulestack <hq-id>
                @produce cash

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

        if (string.Equals(hints.PersonaPreference, "economic", StringComparison.OrdinalIgnoreCase))
        {
            return """
                #faction <id> "<password>"
                #modulestack <cplant-id>
                @produce energy

                #modulestack <hq-id>
                @produce cash

                #modulestack <sdrill-id>
                @use hcdril
                @use iminng

                #modulestack <farms-id>
                @use farmng

                #modulestack <cargob-id>
                @get all food from <farms-id>
                @get all carbon from <sdrill-id>
                sell 200 food at average

                #modulestack <factry-id>
                get 25 iron from <cargob-id>
                get 10 titani from <cargob-id>
                use cdrill as new108 for <hq-id>
                #modulestack new108
                @get 6 terran from <hq-id>
                deactivate 1
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
                @produce cash

                #modulestack <cargob-id>
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
                get 30 iron from <cargob-id>
                use grndtr as scout1 for <hq-id>
                use armcbt as tanks1 for <hq-id>

                #modulestack scout1
                @move {{safeScout}}

                #modulestack tanks1
                @move {{faunaRegion}}
                @tactic destroy
                #end
                """;
        }

        return """
            #faction <id> "<password>"
            #modulestack <hq-id>
            @produce cash

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
                Priority: factory stack FIRST — get materials from cargob, then `use moblib as newNNN`.
                Then run the grant economic loop (@produce, @use, sell food).
                On the new moblab stack: @get terran, food, and oil (moblab burns oil like trucks); @move {anomaly}; @research {anomaly}.
                Do not use active/see unless required. Do not implement deferred town/CONTRACT charters this quarter.
                Use only stack ids from the Orders template. Lowercase immediate verbs (get, use); leftover lines use @ prefix.
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

        if (string.Equals(hints.PersonaPreference, "economic", StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Write turn {hints.DraftTurn} orders for this faction.
                Priority: `@produce energy` on cplant FIRST — HQ is often 80/80 with no headroom; do not activate a nested cdrill (+5 draw) until a 3rd cplant (fossil, 100 iron) is online.
                Surface drill: @use hcdril + @use iminng (iron for next cplant). Factory: use cdrill as newNNN for HQ-id, stage 6 terran, deactivate 1 until energy margin.
                Turn 2+: moblib/moblab with cdrill tech copy to scout deep pockets (exit hint from grant; Deep resources line on-site). Activate cdrill @use iminng on pocket. Next agrplx/farms vs pocket cdrill by bottleneck.
                Defer UN town/CONTRACT charters until home grant production is maxed.
                Use only stack ids from the Orders template. Lowercase immediate verbs (get, use); leftover lines use @ prefix.
                """;
        }

        if (string.Equals(hints.PersonaPreference, "military", StringComparison.OrdinalIgnoreCase))
        {
            var faunaRegion = hints.AnomalyRegionId ?? "adjacent anomaly region-id from grant exits (Mid Vale)";
            return $"""
                Write turn {hints.DraftTurn} orders for this military faction.
                After turn-1 fauna rumor or scout contact: `DECLARE FACTION 14 ENEMY` (or local fauna id from report) before engaging wildlife.
                Priority: factory `use grndtr` scout truck to a *safe* adjacent grant (Farm Belt R00014 — not the anomaly); factory `use armcbt` tanks squad; run grant economic loop (@produce cash, @use farmng/hcdril, @produce energy, sell food).
                When tanks exist: @move tanks to {faunaRegion}, @tactic destroy (fauna stack id may be unknown until arrival — do not invent placeholder ids); claim CT0016 cash bounty when stack cleared.
                Defer UN town charter (CT0006 twnbld) until armored lane is secure. Oil in Mid Vale is a follow-on objective after the cull.
                Use only stack ids from the Orders template. Lowercase immediate verbs (get, use, declare); leftover lines use @ prefix.
                """;
        }

        return $"""
            Write turn {hints.DraftTurn} orders for this faction.
            Implement the grant economic bootstrap loop and any factory builds from the tactical objective.
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
