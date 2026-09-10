using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceAge.PlayerAgent.Lint;

public sealed record VerbAllowlistDocument(
    string SourcePath,
    string SourceContentHash,
    string GeneratedAtUtc,
    IReadOnlyList<string> Verbs);

public static class OrderVerbAllowlistExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string DefaultOutputPath(string repoRoot) =>
        Path.Combine(repoRoot, "tools", "player-agent", "Lint", "verb-allowlist.json");

    public static VerbAllowlistDocument Build(string sourcePath, string rulesMarkdown)
    {
        var verbs = OrderVerbAllowlist.FromRulesMarkdown(rulesMarkdown)
            .OrderBy(verb => verb, StringComparer.Ordinal)
            .ToList();

        return new VerbAllowlistDocument(
            SourcePath: sourcePath.Replace('\\', '/'),
            SourceContentHash: ComputeSha256Hex(rulesMarkdown),
            GeneratedAtUtc: DateTime.UtcNow.ToString("O"),
            Verbs: verbs);
    }

    public static VerbAllowlistDocument BuildFromRulesFile(string rulesPath, string? sourcePath = null)
    {
        var normalizedPath = (sourcePath ?? rulesPath).Replace('\\', '/');
        var content = File.ReadAllText(rulesPath);
        return Build(normalizedPath, content);
    }

    public static void WriteJson(string outputPath, VerbAllowlistDocument document)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(outputPath, json + Environment.NewLine, Encoding.UTF8);
    }

    public static VerbAllowlistDocument ReadJson(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<VerbAllowlistDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not parse verb allowlist JSON: {path}");
    }

    private static string ComputeSha256Hex(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
