using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class VectorRetrieverTests
{
    [Test]
    public void Retrieve_WithVerbFilter_PrefersMatchingRulesChunk()
    {
        var moveChunk = new StoredChunk(
            1,
            new TextChunk("MOVE details", new ChunkMetadata("rules", "MOVE", null, "rules.md", "MOVE")),
            [1f, 0f, 0f]);

        var useChunk = new StoredChunk(
            2,
            new TextChunk("USE details", new ChunkMetadata("rules", "USE", null, "rules.md", "USE")),
            [0.99f, 0.01f, 0f]);

        var reportChunk = new StoredChunk(
            3,
            new TextChunk("shuttle move report", new ChunkMetadata("report", null, null, "report.txt", "report-1")),
            [0.95f, 0.05f, 0f]);

        var query = new[] { 1f, 0f, 0f };
        var results = VectorRetriever.Retrieve([moveChunk, useChunk, reportChunk], query, topK: 2, verbFilter: "MOVE");

        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[0].Chunk.Chunk.Metadata.Verb, Is.EqualTo("MOVE"));
    }
}
