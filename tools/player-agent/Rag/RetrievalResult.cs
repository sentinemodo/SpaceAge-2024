namespace SpaceAge.PlayerAgent.Rag;

public sealed record RetrievalResult(StoredChunk Chunk, float Score);
