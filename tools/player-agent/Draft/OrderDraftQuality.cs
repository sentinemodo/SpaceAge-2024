namespace SpaceAge.PlayerAgent.Draft;

public static class OrderDraftQuality
{
    public static bool IsUsable(string orderText, string? personaPreference = null)
    {
        if (string.IsNullOrWhiteSpace(orderText))
        {
            return false;
        }

        var lines = orderText.Replace("\r\n", "\n").Split('\n');
        var moduleStacks = 0;
        var actionLines = 0;
        var hasImmediateVerb = false;
        var hasLeftoverVerb = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';'))
            {
                continue;
            }

            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                moduleStacks++;
                continue;
            }

            if (line.StartsWith('#'))
            {
                continue;
            }

            actionLines++;
            if (line.StartsWith("@", StringComparison.Ordinal))
            {
                hasLeftoverVerb = true;
            }
            else if (!line.StartsWith("active", StringComparison.OrdinalIgnoreCase)
                && !line.StartsWith("see", StringComparison.OrdinalIgnoreCase))
            {
                hasImmediateVerb = true;
            }
        }

        if (moduleStacks < 2 || actionLines < 6)
        {
            return false;
        }

        if (!hasImmediateVerb || !hasLeftoverVerb)
        {
            return false;
        }

        if (string.Equals(personaPreference, "researcher", StringComparison.OrdinalIgnoreCase))
        {
            var upper = orderText.ToUpperInvariant();
            if (!upper.Contains("USE ", StringComparison.Ordinal) && !upper.Contains("\nUSE ", StringComparison.Ordinal))
            {
                return false;
            }

            if (!upper.Contains("@MOVE", StringComparison.Ordinal) && !upper.Contains("@RESEARCH", StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    public static string BuildRetryInstruction(string? personaPreference) =>
        string.Equals(personaPreference, "researcher", StringComparison.OrdinalIgnoreCase)
            ? """
              Your previous draft was incomplete. Rewrite the full order file with at least:
              - #modulestack <factory-id> first: get iron and silici from cargob, use moblib as newNNN
              - economic loop: @produce cash, @use farmng, @use hcdril, @produce energy, sell food
              - #modulestack newNNN: @get terran, food, oil; @move <anomaly-region-id>; @research <same-region-id>
              Use stack ids from the Orders template. Do not reply with only active/see lines. Include #end.
              """
            : """
              Your previous draft was incomplete. Rewrite the full order file using the Orders template:
              factory USE lines, economic @produce/@use loop, and contract TRANSFER if the open charter applies this quarter.
              Use stack ids from the report template. Do not reply with only active/see lines. Include #end.
              """;
}
