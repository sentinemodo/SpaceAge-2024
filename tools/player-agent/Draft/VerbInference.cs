namespace SpaceAge.PlayerAgent.Draft;

public static class VerbInference
{
    private static readonly string[] KnownVerbs =
    [
        "CONTRACT", "TRANSFER", "RESEARCH", "PRODUCE", "CAPTURE", "DECLARE", "TRAIN", "REPAIR",
        "ATTACK", "ACTIVE", "TACTIC", "STACK", "MOVE", "GIVE", "SELL", "COPY", "FORM", "USE",
        "NAME", "ALIAS", "PRESS", "HAS", "BUY", "GET", "SEE", "SET",
    ];

    public static string? InferFromText(params string?[] sources)
    {
        var combined = string.Join('\n', sources.Where(source => !string.IsNullOrWhiteSpace(source)));
        if (combined.Length == 0)
        {
            return null;
        }

        var upper = combined.ToUpperInvariant();
        foreach (var verb in KnownVerbs)
        {
            if (upper.Contains(verb, StringComparison.Ordinal))
            {
                return verb;
            }
        }

        return null;
    }
}
