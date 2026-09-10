using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Lint;

public static partial class OrderVerbAllowlist
{
    private static readonly HashSet<string> FallbackVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "ACTIVE", "ALIAS", "ATTACK", "BUY", "CAPTURE", "CONTRACT", "COPY", "DECLARE", "FORM", "GET",
        "GIVE", "HAS", "NAME", "PRESS", "SEE", "SELL", "SET", "STACK", "TACTIC", "TRANSFER",
        "MOVE", "PRODUCE", "REPAIR", "RESEARCH", "TRAIN", "USE",
    };

    public static HashSet<string> FromRulesMarkdown(string rulesMarkdown)
    {
        var verbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = rulesMarkdown.Replace("\r\n", "\n").Split('\n');
        var inOrderSection = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                var heading = line["## ".Length..].Trim();
                inOrderSection = heading.Equals("Immediate orders", StringComparison.OrdinalIgnoreCase)
                    || heading.Equals("Long orders", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inOrderSection || !line.StartsWith("### ", StringComparison.Ordinal))
            {
                continue;
            }

            var verbHeading = line["### ".Length..].Trim();
            if (FallbackVerbs.Contains(verbHeading))
            {
                verbs.Add(verbHeading.ToUpperInvariant());
            }
        }

        if (verbs.Count == 0)
        {
            foreach (var verb in FallbackVerbs)
            {
                verbs.Add(verb);
            }
        }

        return verbs;
    }

    public static bool IsAllowed(string verb, IReadOnlySet<string> allowlist) =>
        allowlist.Contains(verb);

    public static string NormalizeToken(string token) =>
        VerbTokenRegex().Replace(token.Trim(), string.Empty).ToUpperInvariant();

    [GeneratedRegex(@"^[@+\-]+", RegexOptions.IgnoreCase)]
    private static partial Regex VerbTokenRegex();
}
