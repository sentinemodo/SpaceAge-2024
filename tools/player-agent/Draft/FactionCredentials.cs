using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Draft;

public static partial class FactionCredentials
{
    public static string? TryReadPassword(string factionDir, int factionId)
    {
        var reportPaths = Directory.Exists(factionDir)
            ? Directory.GetFiles(factionDir, "report*.txt")
                .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        foreach (var reportPath in reportPaths)
        {
            var password = TryParseFactionPassword(() => File.ReadAllText(reportPath), factionId);
            if (password is not null)
            {
                return password;
            }
        }

        var personaPath = Path.Combine(factionDir, "persona.md");
        if (File.Exists(personaPath))
        {
            var password = TryParseFactionPassword(() => File.ReadAllText(personaPath), factionId);
            if (password is not null)
            {
                return password;
            }
        }

        return null;
    }

    private static string? TryParseFactionPassword(Func<string> readText, int factionId)
    {
        var text = readText();
        var match = FactionPasswordRegex().Match(text);
        while (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out var id) && id == factionId)
            {
                return match.Groups[2].Value;
            }

            match = match.NextMatch();
        }

        var personaMatch = PersonaPasswordRegex().Match(text);
        return personaMatch.Success ? personaMatch.Groups[1].Value : null;
    }

    [GeneratedRegex(@"#faction\s+(\d+)\s+""([^""]*)""", RegexOptions.IgnoreCase)]
    private static partial Regex FactionPasswordRegex();

    [GeneratedRegex(@"-\s*Password:\s*`([^`]+)`", RegexOptions.IgnoreCase)]
    private static partial Regex PersonaPasswordRegex();
}
