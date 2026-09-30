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

    private static bool IsRepeatCountToken(string token)
    {
        if (token.Length == 0)
        {
            return false;
        }

        var start = 0;
        if (token[0] == '+' || token[0] == '-')
        {
            start = 1;
        }

        if (start >= token.Length)
        {
            return false;
        }

        for (var i = start; i < token.Length; i++)
        {
            if (!char.IsDigit(token[i]))
            {
                return false;
            }
        }

        return true;
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

        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var index = 0;
        if (parts.Length > index + 1 && IsRepeatCountToken(parts[index]))
        {
            index += 1;
        }

        return OrderVerbAllowlist.NormalizeToken(parts[index]);
    }
}
