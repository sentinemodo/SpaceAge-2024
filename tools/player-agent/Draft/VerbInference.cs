using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class VerbInference
{
    private static readonly string[] KnownVerbs =
    [
        "CONTRACT", "TRANSFER", "RESEARCH", "PRODUCE", "CAPTURE", "DECLARE", "TRAIN", "REPAIR",
        "ATTACK", "ACTIVE", "TACTIC", "STACK", "MOVE", "GIVE", "SELL", "COPY", "FORM", "USE",
        "NAME", "ALIAS", "PRESS", "HAS", "BUY", "GET", "SEE", "SET",
    ];

    private static readonly Dictionary<string, string[]> PersonaBoostVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["researcher"] = ["USE", "GET", "MOVE", "RESEARCH", "PRODUCE"],
        ["contractor"] = ["USE", "GET", "TRANSFER", "PRODUCE", "CONTRACT"],
        ["economic"] = ["PRODUCE", "USE", "GET", "SELL", "BUY"],
        ["military"] = ["MOVE", "USE", "GET", "ATTACK", "DECLARE", "PRODUCE"],
    };

    /// <summary>Legacy single-verb filter for spot-check tooling.</summary>
    public static string? InferFromText(params string?[] sources) =>
        InferBoostVerbs(sources).FirstOrDefault();

    public static IReadOnlyList<string> InferBoostVerbs(params string?[] sources)
    {
        var combined = string.Join('\n', sources.Where(source => !string.IsNullOrWhiteSpace(source)));
        if (combined.Length == 0)
        {
            return [];
        }

        var persona = DetectPersonaPreference(combined);
        if (persona is not null && PersonaBoostVerbs.TryGetValue(persona, out var personaVerbs))
        {
            return personaVerbs;
        }

        var scored = ScoreActionableVerbs(StripDeferredActions(combined));
        return scored
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => Array.IndexOf(KnownVerbs, pair.Key))
            .Where(pair => pair.Value > 0)
            .Take(4)
            .Select(pair => pair.Key)
            .ToList();
    }

    public static string? DetectPersonaPreference(string text)
    {
        var preferenceMatch = PersonaPreferenceRegex().Match(text);
        if (preferenceMatch.Success)
        {
            return preferenceMatch.Groups[1].Value.Trim().ToLowerInvariant();
        }

        var upper = text.ToUpperInvariant();
        if (upper.Contains("MOBLIB", StringComparison.Ordinal) || upper.Contains("MOBLAB", StringComparison.Ordinal))
        {
            return "researcher";
        }

        if (upper.Contains("TWNBLD", StringComparison.Ordinal) || upper.Contains("TRANSFER TO FACTION 1", StringComparison.Ordinal))
        {
            return "contractor";
        }

        return null;
    }

    private static string StripDeferredActions(string text) =>
        DeferActionRegex().Replace(text, " ");

    private static Dictionary<string, int> ScoreActionableVerbs(string text)
    {
        var upper = text.ToUpperInvariant();
        var scores = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var verb in KnownVerbs)
        {
            var count = 0;
            var index = 0;
            while ((index = upper.IndexOf(verb, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += verb.Length;
            }

            if (count > 0)
            {
                scores[verb] = count;
            }
        }

        if (scores.TryGetValue("CONTRACT", out var contractScore) && contractScore > 0)
        {
            scores["CONTRACT"] = Math.Max(0, contractScore - CountDeferredContractMentions(text));
        }

        return scores;
    }

    private static int CountDeferredContractMentions(string text)
    {
        var count = 0;
        foreach (Match match in DeferActionRegex().Matches(text))
        {
            if (match.Value.Contains("contract", StringComparison.OrdinalIgnoreCase)
                || match.Value.Contains("ct", StringComparison.OrdinalIgnoreCase)
                || match.Value.Contains("town", StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    [GeneratedRegex(@"##?\s*Preference\s*:\s*(\w+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PersonaPreferenceRegex();

    [GeneratedRegex(@"\bdefer\b[^.\n\r]{0,80}\b(contract|ct\d{4}|town|charter)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DeferActionRegex();
}
