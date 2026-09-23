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
        var rulesPath = Path.Combine(TestRepoPaths.PlayerDirectory(TestRepoPaths.FindRepositoryRoot()), "rules.md");
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
        var rulesPath = Path.Combine(TestRepoPaths.PlayerDirectory(TestRepoPaths.FindRepositoryRoot()), "rules.md");
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
        var techPath = Path.Combine(TestRepoPaths.PlayerDirectory(TestRepoPaths.FindRepositoryRoot()), "basic_technologies.md");
        var content = File.ReadAllText(techPath);

        var chunks = MarkdownChunker.ChunkTechManual(techPath, content, "test");
        var filidx = chunks.FirstOrDefault(chunk =>
            chunk.Metadata.Heading?.Contains("filidx", StringComparison.OrdinalIgnoreCase) == true);

        Assert.That(filidx, Is.Not.Null);
        Assert.That(filidx!.Metadata.Doc, Is.EqualTo("tech"));
        Assert.That(filidx.Metadata.Mode, Is.EqualTo("test"));
    }

    [Test]
    public void SplitWithSizeCap_AppliesOverlapBetweenParts()
    {
        var content = new string('a', 4000) + "\n\n" + new string('b', 4000);
        var parts = MarkdownChunker.SplitWithSizeCap(content, 3500, overlapCharacters: 350);

        Assert.That(parts, Has.Count.GreaterThan(1));
        Assert.That(parts[1], Does.StartWith(new string('a', 350)));
    }

    [Test]
    public void ChunkReport_SplitsByStackSection()
    {
        const string report = """
            Galaxy report:
            ------------------------------------------------------------
              Northwind Grant [R00008] (1,1), grassland region.
              Resources: 600 units of food [food].

              + Northwind Headquarters [200001], corporate headquarters [corphq], immobile.
                size: 1000, crew: 81/92.
                items: 30 terrans [terran].

              + small cargo bay [200003], 2 small cargo bays [cargob], immobile.
                items: 487 units of food [food].
            """;

        var chunks = MarkdownChunker.ChunkReport("report.2.2.txt", report);
        var headings = chunks.Select(chunk => chunk.Metadata.Heading).ToList();

        Assert.That(headings.Any(heading =>
            heading?.Contains("Northwind Headquarters", StringComparison.OrdinalIgnoreCase) == true), Is.True);
        Assert.That(headings.Any(heading =>
            heading?.Contains("small cargo bay", StringComparison.OrdinalIgnoreCase) == true), Is.True);
        Assert.That(chunks.Any(chunk => chunk.Content.Contains("200001")), Is.True);
        Assert.That(chunks.Any(chunk => chunk.Content.Contains("200003")), Is.True);
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
