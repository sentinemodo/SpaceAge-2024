using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class SqliteVectorStoreTests
{
    [Test]
    public void ClearAll_RemovesEveryChunk()
    {
        var sqlitePath = Path.Combine(Path.GetTempPath(), $"player-agent-test-{Guid.NewGuid():N}.sqlite");
        try
        {
            using var store = new SqliteVectorStore(sqlitePath);
            var chunk = new TextChunk(
                "MOVE syntax",
                new ChunkMetadata("rules", "MOVE", null, "rules.md", "MOVE"));
            store.ReplaceSource("rules.md", [(chunk, [1f, 0f])]);
            Assert.That(store.Count(), Is.EqualTo(1));

            store.ClearAll();
            Assert.That(store.Count(), Is.EqualTo(0));
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

    [Test]
    public void ReplaceSource_IsIdempotentForSameNormalizedPath()
    {
        var sqlitePath = Path.Combine(Path.GetTempPath(), $"player-agent-test-{Guid.NewGuid():N}.sqlite");
        var sourcePath = Path.Combine(Path.GetTempPath(), "rules.md");
        try
        {
            using var store = new SqliteVectorStore(sqlitePath);
            var chunk = new TextChunk(
                "MOVE syntax",
                new ChunkMetadata("rules", "MOVE", null, sourcePath, "MOVE"));
            store.ReplaceSource(sourcePath, [(chunk, [1f, 0f])]);
            store.ReplaceSource(sourcePath.ToUpperInvariant(), [(chunk, [1f, 0f])]);

            Assert.That(store.Count(), Is.EqualTo(1));
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

    [Test]
    public void ListSourcePaths_ReturnsDistinctNormalizedPaths()
    {
        var sqlitePath = Path.Combine(Path.GetTempPath(), $"player-agent-test-{Guid.NewGuid():N}.sqlite");
        try
        {
            using var store = new SqliteVectorStore(sqlitePath);
            var moveChunk = new TextChunk(
                "MOVE syntax",
                new ChunkMetadata("rules", "MOVE", null, "rules.md", "MOVE"));
            var useChunk = new TextChunk(
                "USE syntax",
                new ChunkMetadata("rules", "USE", null, "battle.md", "USE"));
            store.ReplaceSource("rules.md", [(moveChunk, [1f, 0f])]);
            store.ReplaceSource("battle.md", [(useChunk, [0f, 1f])]);

            Assert.That(
                store.ListSourcePaths().Select(Path.GetFileName),
                Is.EquivalentTo(new[] { "rules.md", "battle.md" }));
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

    [Test]
    public void DeleteSources_RemovesMatchingChunksOnly()
    {
        var sqlitePath = Path.Combine(Path.GetTempPath(), $"player-agent-test-{Guid.NewGuid():N}.sqlite");
        try
        {
            using var store = new SqliteVectorStore(sqlitePath);
            var moveChunk = new TextChunk(
                "MOVE syntax",
                new ChunkMetadata("rules", "MOVE", null, "rules.md", "MOVE"));
            var useChunk = new TextChunk(
                "USE syntax",
                new ChunkMetadata("rules", "USE", null, "battle.md", "USE"));
            store.ReplaceSource("rules.md", [(moveChunk, [1f, 0f])]);
            store.ReplaceSource("battle.md", [(useChunk, [0f, 1f])]);

            var removed = store.DeleteSources(new[] { "rules.md" });
            Assert.That(removed, Is.EqualTo(1));
            Assert.That(store.Count(), Is.EqualTo(1));
            Assert.That(
                store.ListSourcePaths().Select(Path.GetFileName),
                Is.EquivalentTo(new[] { "battle.md" }));
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
