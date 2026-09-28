using System.Text;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static class StagedOrderPromptBuilder
{
    private static readonly HashSet<string> BootstrapModuleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "corphq", "cargob", "sdrill", "farms", "cplant", "wnplnt",
    };

    private static readonly HashSet<string> ProductionModuleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "factry",
    };

    public const string Pass1SystemPrompt =
        "You generate ONLY the bootstrap portion of a SpaceAge PBEM order file (pass 1 of 2). "
        + "Output #faction line, optional DECLARE, and #modulestack sections for HQ, energy, drill, farms, cargob only. "
        + "No factory USE, no newN mobile stacks yet. End with #end. No markdown fences.";

    public const string Pass2SystemPrompt =
        "You generate ONLY the production/mobile portion of a SpaceAge PBEM order file (pass 2 of 2). "
        + "Do NOT output #faction. Output #modulestack sections for factory and all newN stacks only, then #end. "
        + "No markdown fences.";

    public static string BuildPass1UserPrompt(
        string promptPack,
        IReadOnlyList<RetrievalResult> retrievedChunks,
        string? filteredTemplate,
        OrderDraftHints hints,
        string? reportText)
    {
        var builder = new StringBuilder();
        builder.AppendLine(promptPack.Trim());
        builder.AppendLine();
        AppendRetrieved(builder, retrievedChunks);
        if (!string.IsNullOrWhiteSpace(filteredTemplate))
        {
            builder.AppendLine("## Orders template (bootstrap stacks only)");
            builder.AppendLine(filteredTemplate.Trim());
            builder.AppendLine();
        }

        AppendStackCatalog(builder, reportText, BootstrapModuleTypes);
        AppendGrantNav(builder, reportText);
        builder.AppendLine("## Pass 1 task");
        builder.AppendLine(BuildPass1Task(hints, reportText));
        return builder.ToString().Trim();
    }

    public static string BuildPass2UserPrompt(
        string promptPack,
        IReadOnlyList<RetrievalResult> retrievedChunks,
        string? filteredTemplate,
        OrderDraftHints hints,
        string? reportText,
        string bootstrapPass)
    {
        var builder = new StringBuilder();
        builder.AppendLine(promptPack.Trim());
        builder.AppendLine();
        AppendRetrieved(builder, retrievedChunks);
        builder.AppendLine("## Pass 1 bootstrap (already written — do not repeat these stacks except factry/mobile)");
        builder.AppendLine(bootstrapPass.Trim());
        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(filteredTemplate))
        {
            builder.AppendLine("## Orders template (factory + mobile stacks)");
            builder.AppendLine(filteredTemplate.Trim());
            builder.AppendLine();
        }

        AppendStackCatalog(builder, reportText, ProductionModuleTypes);
        AppendGrantNav(builder, reportText);
        builder.AppendLine("## Pass 2 task");
        builder.AppendLine(BuildPass2Task(hints, reportText));
        return builder.ToString().Trim();
    }

    public static string? FilterOrdersTemplate(string? ordersTemplate, string? reportText, IReadOnlySet<string> moduleTypes)
    {
        if (string.IsNullOrWhiteSpace(ordersTemplate))
        {
            return null;
        }

        var stackTypes = ReportStackCatalog.ParseModuleTypes(reportText);
        if (stackTypes.Count == 0)
        {
            return ordersTemplate;
        }

        var allowedIds = stackTypes
            .Where(pair => moduleTypes.Contains(pair.Value))
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var kept = new List<string>();
        var include = false;
        foreach (var line in ordersTemplate.Replace("\r\n", "\n").Split('\n'))
        {
            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                var id = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
                include = id is not null && allowedIds.Contains(id);
            }

            if (include || line.StartsWith("Orders Template", StringComparison.OrdinalIgnoreCase))
            {
                kept.Add(line);
            }
        }

        return kept.Count == 0 ? ordersTemplate : string.Join(Environment.NewLine, kept);
    }

    private static void AppendRetrieved(StringBuilder builder, IReadOnlyList<RetrievalResult> retrievedChunks)
    {
        if (retrievedChunks.Count == 0)
        {
            return;
        }

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

    private static void AppendStackCatalog(StringBuilder builder, string? reportText, IReadOnlySet<string> filterTypes)
    {
        var stackTypes = ReportStackCatalog.ParseModuleTypes(reportText);
        if (stackTypes.Count == 0)
        {
            return;
        }

        builder.AppendLine("## Stack ids for this pass");
        foreach (var pair in stackTypes.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (!filterTypes.Contains(pair.Value))
            {
                continue;
            }

            builder.AppendLine($"- {pair.Key}: [{pair.Value}]");
        }

        builder.AppendLine();
    }

    private static void AppendGrantNav(StringBuilder builder, string? reportText)
    {
        var table = ReportRegionGraph.FormatGrantRegionNavigationTable(reportText);
        if (string.IsNullOrWhiteSpace(table))
        {
            return;
        }

        builder.AppendLine(table);
        builder.AppendLine();
    }

    private static string BuildPass1Task(OrderDraftHints hints, string? reportText)
    {
        var wind = ReportStackCatalog.GrantUsesWindPowerPlant(reportText);
        var energyStack = wind ? "wnplnt: @produce energy (optional use wndtrb as newN for wnplnt-id)" : "cplant: @get all carbon on cargob from sdrill; @produce energy";
        var drillUse = wind
            ? "sdrill: grant item iron/titani then @use iminng only (not hcdril)"
            : "sdrill: grant item iron/titani then @use hcdril only (not iminng) if carbon/oil in grant Resources";

        if (string.Equals(hints.PersonaPreference, "military", StringComparison.OrdinalIgnoreCase))
        {
            return $"""
                Pass 1 only: `#faction`, `DECLARE FACTION 14 ENEMY` if fauna offensive, HQ `set hold 20 terran` + grant terran if tanks planned, cargob grants + @get food, {drillUse}, farms @use farmng, {energyStack}.
                No factory, no grndtr/tanks yet.
                """;
        }

        return $"""
            Pass 1 only: HQ `set hold 20 terran` + `@produce terran`, {energyStack}, {drillUse}, farms `@use farmng`, cargob `@get all food` (+ carbon if coal grant).
            No factory USE yet.
            """;
    }

    private static string BuildPass2Task(OrderDraftHints hints, string? reportText)
    {
        if (string.Equals(hints.PersonaPreference, "military", StringComparison.OrdinalIgnoreCase))
        {
            var fauna = hints.AnomalyRegionId ?? "valid exit from grant table";
            return $"""
                Pass 2: factry — grant technology armcbt, use grndtr as new1, two use armcbt as new2/new3 with +get iron/titani.
                new1: move {fauna}, +get terran/oil/food. new2/new3: has 1 tanks, -get provisioning, -move {fauna}, tactic destroy.
                Ground moves must use grant Exits ids only.
                """;
        }

        if (string.Equals(hints.PersonaPreference, "economic", StringComparison.OrdinalIgnoreCase))
        {
            return """
                Pass 2: factry — grant mcored + msrvtm, use mcored as newN with +get iron/titani, has 1 cdrill / deactivate on nest.
                use msrvtm as newM moblab with +get and move to a grant exit toward deep metals. wndtrb on wnplnt if not done in pass 1.
                cargob sell food if not in pass 1.
                """;
        }

        return "Pass 2: factory and mobile stacks per persona; use template ids only.";
    }
}
