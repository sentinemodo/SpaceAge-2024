using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Draft;

public sealed class KnowledgeDraftService
{
    private readonly IOllamaClient _client;

    public KnowledgeDraftService(IOllamaClient client)
    {
        _client = client;
    }

    public async Task<KnowledgeDraftResult> DraftAsync(KnowledgeDraftRequest request, CancellationToken cancellationToken)
    {
        var reportText = await File.ReadAllTextAsync(request.ReportPath, cancellationToken);
        var factionName = ExtractFactionName(reportText, request.FactionId);
        string? prior = null;
        if (File.Exists(request.OutputPath))
        {
            prior = await File.ReadAllTextAsync(request.OutputPath, cancellationToken);
        }

        var userPrompt = KnowledgeDraftPromptBuilder.BuildUserPrompt(
            factionName,
            request.FactionId,
            request.ReportTurn,
            reportText,
            prior,
            request.PersonaPreference);

        if (request.DryRun)
        {
            return new KnowledgeDraftResult(userPrompt, null, request.OutputPath);
        }

        var generated = await _client.ChatAsync(userPrompt, KnowledgeDraftPromptBuilder.SystemPrompt, cancellationToken);
        var text = generated.Trim();
        if (!text.StartsWith('#'))
        {
            text = $"# {factionName} — knowledge (turn {request.ReportTurn})\r\n\r\n" + text;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(request.OutputPath)!);
        await File.WriteAllTextAsync(request.OutputPath, text.TrimEnd() + Environment.NewLine, cancellationToken);
        return new KnowledgeDraftResult(userPrompt, text, request.OutputPath);
    }

    private static string ExtractFactionName(string reportText, int factionId)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            reportText,
            $@"report for\s+([^\[]+)\s*\[{factionId}\]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : $"Faction {factionId}";
    }
}

public sealed record KnowledgeDraftRequest
{
    public required int FactionId { get; init; }
    public required int ReportTurn { get; init; }
    public required string ReportPath { get; init; }
    public required string OutputPath { get; init; }
    public string? PersonaPreference { get; init; }
    public bool DryRun { get; init; }
}

public sealed record KnowledgeDraftResult(string UserPrompt, string? KnowledgeText, string OutputPath);
