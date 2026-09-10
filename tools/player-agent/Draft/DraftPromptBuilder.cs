using System.Text;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public static class DraftPromptBuilder
{
    public const string SystemPrompt =
        "You generate SpaceAge PBEM order files. Reply with order file text only: #faction, "
        + "#modulestack / #person headers, verb lines, comments with ;, and #end. "
        + "No prose, no markdown fences, no numbered lists, no explanations.";

    public static string BuildRetrievalQuery(string? objectiveText, string? reportText)
    {
        var builder = new StringBuilder();
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

    public static string BuildChatPrompt(
        string promptPack,
        IReadOnlyList<RetrievalResult> retrievedChunks,
        string? ordersTemplate)
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

        builder.AppendLine("## Example output shape");
        builder.AppendLine(
            """
            #faction 2
            #modulestack 200001
            ; activate headquarters
            active 200001
            #modulestack 200004
            active 200004
            #person 200010
            see 200001
            #end
            """);
        builder.AppendLine("## Task");
        builder.AppendLine(
            "Write turn 2 orders for this faction. Use only stack/person ids from the report or template. "
            + "Use only documented verbs (ACTIVE, STACK, USE, MOVE, GIVE, CONTRACT, etc.).");
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
}
