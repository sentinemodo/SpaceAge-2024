namespace SpaceAge.PlayerAgent.Draft;

public static class KnowledgeDraftPromptBuilder
{
    public const string SystemPrompt =
        "You maintain SpaceAge faction knowledge for multi-turn AI play. Output markdown only, no fences. "
        + "Preserve region ids, exit travel weeks, resource lines, and module stack ids exactly as in the report.";

    public static string BuildUserPrompt(
        string factionName,
        int factionId,
        int reportTurn,
        string reportText,
        string? priorKnowledge,
        string? personaPreference = null)
    {
        var excerpt = StoryDraftPromptBuilder.BuildReportExcerpt(reportText, maxLines: 180);
        var battles = StoryDraftPromptBuilder.ExtractBattlesReportSection(reportText);
        var isMilitary = string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase);
        var prior = string.IsNullOrWhiteSpace(priorKnowledge)
            ? "(none — first knowledge file for this run)"
            : priorKnowledge.Trim();

        var battleBlock = string.IsNullOrWhiteSpace(battles)
            ? string.Empty
            : $"""

              Battles report (full section — prioritize for military knowledge):
              {battles}
              """;

        var observedEnemiesSection = isMilitary
            ? """

              ## Observed enemy units (battles this report)
              - One subsection per engagement: **region id**, **week**, **outcome** (won/lost/draw).
              - **Attackers:** stack id, module type id (e.g. urstlk, slgmnt), owner faction, HP/attack/defense, copy damage state if shown.
              - **Defenders:** our stack ids, module types, tactics, damage/disabled/destruction.
              - **Takeaways:** adjacency threats, when to avoid move, repair/rebuild priorities, fauna faction ids (14/15).

              """
            : string.Empty;

        return $"""
            Write `knowledge.md` for {factionName} [faction {factionId}] after report turn {reportTurn}.
            This file is ingested next turn for story/order drafting when galaxy detail leaves the report.

            Prior knowledge (merge forward; drop stale claims contradicted by the report):
            {prior}

            Report excerpt (source of truth for this update):
            {excerpt}
            {battleBlock}

            Required headings:

            # {factionName} — knowledge (turn {reportTurn})
            {observedEnemiesSection}
            ## Moblab / mobile survey
            - Stack id, current region, disabled/fuel/crew state, mcored copy aboard.
            - Fuel plan: oil/food needed for upcoming quarters; bank GRANT vs cargob +get when grant oil is scarce.
            - Scout plan: target region ids (e.g. deep pocket / anomaly exits), **multi-hop** ground paths with week counts.

            ## Region map (preserve when leaving visibility)
            For each region the faction still cares about (home grant, moblab location, scout targets, known deep pockets):
            - Region id, name, coords, settlement capacity if shown.
            - **Exits:** neighbor id, terrain, ground travel weeks, anomaly/deep-pocket flags.
            - **Resources** and **Deep resources** lines when present.

            ## Modules without orders this turn
            - List immobile stacks that may need an empty `#modulestack <id>` block in orders when idle.

            ## Next-turn reminders
            - 3–6 bullets: energy, cdrill nest, UN charter deferrals, fauna rumors, contract CT ids.

            Be factual; no narrative prose. Use catalog ids (R00056, titani, mcored).
            """;
    }
}
