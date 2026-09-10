namespace SpaceAge.PlayerAgent.Lint;

public static class OrderDraftLinter
{
    public static OrderDraftLintResult Lint(string orderText, IReadOnlySet<string> allowlist)
    {
        var errors = new List<string>();
        var hasEnd = false;
        var hasFaction = false;
        var lineNumber = 0;

        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';'))
            {
                continue;
            }

            if (line.StartsWith("#faction", StringComparison.OrdinalIgnoreCase))
            {
                hasFaction = true;
                continue;
            }

            if (line.Equals("#end", StringComparison.OrdinalIgnoreCase))
            {
                hasEnd = true;
                continue;
            }

            if (line.StartsWith('#'))
            {
                continue;
            }

            var verb = ExtractVerb(line);
            if (verb.Length == 0)
            {
                continue;
            }

            if (!OrderVerbAllowlist.IsAllowed(verb, allowlist))
            {
                errors.Add($"Unknown verb '{verb}' on line {lineNumber}: {rawLine.Trim()}");
            }
        }

        if (!hasFaction)
        {
            errors.Add("Missing #faction header.");
        }

        if (!hasEnd)
        {
            errors.Add("Missing #end trailer.");
        }

        return new OrderDraftLintResult(errors.Count == 0, errors);
    }

    public static string ExtractVerb(string line)
    {
        var trimmed = line.Trim();
        var commentIndex = trimmed.IndexOf(';');
        if (commentIndex >= 0)
        {
            trimmed = trimmed[..commentIndex].Trim();
        }

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var firstToken = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        return OrderVerbAllowlist.NormalizeToken(firstToken);
    }
}
