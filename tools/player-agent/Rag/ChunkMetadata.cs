namespace SpaceAge.PlayerAgent.Rag;

public sealed record ChunkMetadata(
    string Doc,
    string? Verb,
    string? Mode,
    string SourcePath,
    string? Heading);
