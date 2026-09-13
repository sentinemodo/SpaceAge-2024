using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Draft;

public sealed partial class StoryDraftService
{
    private readonly IOllamaClient _client;

    public StoryDraftService(IOllamaClient client)
    {
        _client = client;
    }

    public async Task<StoryDraftResult> DraftAsync(StoryDraftRequest request, CancellationToken cancellationToken)
    {
        var personaText = await ReadRequiredTextAsync(request.PersonaPath, "persona.md");
        var reportText = await ReadRequiredTextAsync(request.ReportPath, "report");
        var factionName = ExtractFactionName(reportText, request.FactionId);
        var context = StoryDraftPromptBuilder.BuildContext(
            factionName,
            request.FactionId,
            request.ReportTurn,
            personaText,
            reportText) with
        {
            HasPriorStory = request.HasPriorStory,
        };

        if (request.DryRun)
        {
            var monolithicPrompt = StoryDraftPromptBuilder.BuildMonolithicUserPrompt(context);
            return new StoryDraftResult(
                StoryDraftStrategy.Monolithic,
                monolithicPrompt,
                null,
                request.OutputPath);
        }

        if (request.Chunked)
        {
            var chunked = await DraftChunkedAsync(context, cancellationToken);
            await WriteStoryAsync(request.OutputPath, chunked.StoryText, cancellationToken);
            return chunked;
        }

        try
        {
            var monolithic = await DraftMonolithicAsync(context, cancellationToken);
            await WriteStoryAsync(request.OutputPath, monolithic.StoryText, cancellationToken);
            return monolithic;
        }
        catch (Exception ex) when (IsTimeout(ex) && request.FallbackToChunkedOnTimeout)
        {
            Console.WriteLine(
                "Monolithic story chat timed out; retrying with chunked prompts (strategic + tactical + narrative).");
            var chunked = await DraftChunkedAsync(context, cancellationToken);
            await WriteStoryAsync(request.OutputPath, chunked.StoryText, cancellationToken);
            return chunked with { TimedOutBeforeFallback = true };
        }
    }

    private async Task<StoryDraftResult> DraftMonolithicAsync(
        StoryDraftContext context,
        CancellationToken cancellationToken)
    {
        var prompt = StoryDraftPromptBuilder.BuildMonolithicUserPrompt(context);
        var generated = await _client.ChatAsync(
            prompt,
            StoryDraftPromptBuilder.SystemPrompt,
            cancellationToken);
        var story = NormalizeStory(generated, context.FactionName, context.ReportTurn);
        return new StoryDraftResult(StoryDraftStrategy.Monolithic, prompt, story, null);
    }

    private async Task<StoryDraftResult> DraftChunkedAsync(
        StoryDraftContext context,
        CancellationToken cancellationToken)
    {
        var chunks = StoryDraftPromptBuilder.BuildChunkPrompts(context);
        var sections = new List<string>(chunks.Count);
        foreach (var chunk in chunks)
        {
            Console.WriteLine($"Story chunk: {chunk.Name}");
            var generated = await _client.ChatAsync(
                chunk.UserPrompt,
                StoryDraftPromptBuilder.ChunkSystemPrompt,
                cancellationToken);
            sections.Add(NormalizeSection(generated));
        }

        var story = StoryDraftPromptBuilder.AssembleStory(
            context.FactionName,
            context.ReportTurn,
            sections);
        return new StoryDraftResult(StoryDraftStrategy.Chunked, null, story, null);
    }

    private static string NormalizeStory(string generated, string factionName, int reportTurn)
    {
        var trimmed = generated.Trim();
        if (trimmed.StartsWith('#'))
        {
            return trimmed + Environment.NewLine;
        }

        return StoryDraftPromptBuilder.AssembleStory(factionName, reportTurn, [trimmed]);
    }

    private static string NormalizeSection(string generated)
    {
        var trimmed = generated.Trim();
        var fenceMatch = MarkdownFenceRegex().Match(trimmed);
        if (fenceMatch.Success)
        {
            trimmed = fenceMatch.Groups[1].Value.Trim();
        }

        return trimmed;
    }

    private static bool IsTimeout(Exception ex) =>
        ex is TaskCanceledException
        || (ex is HttpRequestException && ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase));

    private static async Task WriteStoryAsync(
        string outputPath,
        string? storyText,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storyText))
        {
            throw new InvalidOperationException("Story draft returned empty content.");
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(
            outputPath,
            storyText,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
    }

    private static async Task<string> ReadRequiredTextAsync(string path, string label)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Missing {label}: {path}");
        }

        return await File.ReadAllTextAsync(path);
    }

    private static string ExtractFactionName(string reportText, int factionId)
    {
        foreach (var line in reportText.Split('\n'))
        {
            var match = FactionReportRegex().Match(line);
            if (match.Success && int.Parse(match.Groups[2].Value) == factionId)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return $"Faction {factionId}";
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"report for (.+?) \[(\d+)\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex FactionReportRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"```(?:\w*\n)?([\s\S]*?)```", System.Text.RegularExpressions.RegexOptions.Multiline)]
    private static partial System.Text.RegularExpressions.Regex MarkdownFenceRegex();
}

public enum StoryDraftStrategy
{
    Monolithic,
    Chunked,
}

public sealed record StoryDraftRequest
{
    public required int FactionId { get; init; }
    public required int ReportTurn { get; init; }
    public required string OutputPath { get; init; }
    public required string PersonaPath { get; init; }
    public required string ReportPath { get; init; }
    public bool HasPriorStory { get; init; }
    public bool Chunked { get; init; }
    public bool FallbackToChunkedOnTimeout { get; init; } = true;
    public bool DryRun { get; init; }
}

public sealed record StoryDraftResult(
    StoryDraftStrategy Strategy,
    string? MonolithicPrompt,
    string? StoryText,
    string? OutputPath)
{
    public bool TimedOutBeforeFallback { get; init; }
}
