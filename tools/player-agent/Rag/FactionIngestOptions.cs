namespace SpaceAge.PlayerAgent.Rag;

public sealed class FactionIngestOptions
{
    public bool ClearIndex { get; init; }

    /// <summary>Phase 2 full ingest: every report and order file on disk.</summary>
    public bool FullCorpus { get; init; }

    /// <summary>Campaign-ai handoff: re-embed story.md only.</summary>
    public bool StoryOnly { get; init; }

    /// <summary>Override report path (defaults to latest report in the faction folder).</summary>
    public string? ReportPath { get; init; }

    /// <summary>Keep orders from the last N turns in the faction index (default 3). Ignored when <see cref="FullCorpus"/> is true.</summary>
    public int MaxOrderTurns { get; init; } = 3;
}
