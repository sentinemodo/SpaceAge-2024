using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class SqliteVectorStoreTests
{
    [Test]
    public void ReplaceSource_RemovesOldChunksForSamePath()
    {
        var sqlitePath = Path.Combine(Path.GetTempPath(), $"player-agent-test-{Guid.NewGuid():N}.sqlite");
        try
        {
            using var store = new SqliteVectorStore(sqlitePath);
            var moveChunk = new TextChunk(
                "MOVE syntax",
                new ChunkMetadata("rules", "MOVE", null, "rules.md", "MOVE"));
            store.ReplaceSource("rules.md", [(moveChunk, [1f, 0f])]);
            Assert.That(store.Count(), Is.EqualTo(1));

            var useChunk = new TextChunk(
                "USE syntax",
                new ChunkMetadata("rules", "USE", null, "rules.md", "USE"));
            store.ReplaceSource("rules.md", [(useChunk, [0f, 1f])]);
            Assert.That(store.Count(), Is.EqualTo(1));

            var stored = store.ListAll().Single();
            Assert.That(stored.Chunk.Metadata.Verb, Is.EqualTo("USE"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(sqlitePath))
            {
                File.Delete(sqlitePath);
            }
        }
    }
}
