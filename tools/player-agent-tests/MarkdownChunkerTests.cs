using NUnit.Framework;
using SpaceAge.PlayerAgent;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class MarkdownChunkerTests
{
    [Test]
    public void ChunkRules_ExtractsMoveVerb_WithRulesMetadata()
    {
        var rulesPath = Path.Combine(TestRepoPaths.FindRepositoryRoot(), "player", "rules.md");
        var content = File.ReadAllText(rulesPath);

        var chunks = MarkdownChunker.ChunkRules(rulesPath, content);
        var moveChunk = chunks.FirstOrDefault(chunk =>
            string.Equals(chunk.Metadata.Verb, "MOVE", StringComparison.Ordinal));

        Assert.That(moveChunk, Is.Not.Null);
        Assert.That(moveChunk!.Metadata.Doc, Is.EqualTo("rules"));
        Assert.That(moveChunk.Content, Does.Contain("MOVE"));
    }

    [Test]
    public void ChunkRules_IncludesPrefixesSection()
    {
        var rulesPath = Path.Combine(TestRepoPaths.FindRepositoryRoot(), "player", "rules.md");
        var content = File.ReadAllText(rulesPath);

        var chunks = MarkdownChunker.ChunkRules(rulesPath, content);
        var fileHeaders = chunks.FirstOrDefault(chunk =>
            string.Equals(chunk.Metadata.Heading, "File headers", StringComparison.OrdinalIgnoreCase));

        Assert.That(fileHeaders, Is.Not.Null);
        Assert.That(fileHeaders!.Content, Does.Contain("#faction"));
    }

    [Test]
    public void ChunkTechManual_SplitsByTechEntry()
    {
        var techPath = Path.Combine(TestRepoPaths.FindRepositoryRoot(), "player", "basic_technologies.md");
        var content = File.ReadAllText(techPath);

        var chunks = MarkdownChunker.ChunkTechManual(techPath, content, "test");
        var filidx = chunks.FirstOrDefault(chunk =>
            chunk.Metadata.Heading?.Contains("filidx", StringComparison.OrdinalIgnoreCase) == true);

        Assert.That(filidx, Is.Not.Null);
        Assert.That(filidx!.Metadata.Doc, Is.EqualTo("tech"));
        Assert.That(filidx.Metadata.Mode, Is.EqualTo("test"));
    }

    [Test]
    public void ChunkOrderFile_SplitsByModulestackBlock()
    {
        const string order = """
            #faction 2 "xyzzy"
            #modulestack 101
            move O00001

            #modulestack 100
            stack 101
            """;

        var chunks = MarkdownChunker.ChunkOrderFile("order.2.txt", order);

        Assert.That(chunks, Has.Count.EqualTo(3));
        Assert.That(chunks[1].Metadata.Doc, Is.EqualTo("order"));
        Assert.That(chunks[1].Metadata.Heading, Is.EqualTo("#modulestack"));
    }
}
