namespace SpaceAge.PlayerAgent.Rag;

public sealed record StoredChunk(long Id, TextChunk Chunk, float[] Embedding);
