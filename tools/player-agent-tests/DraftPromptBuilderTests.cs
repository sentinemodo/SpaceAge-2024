using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class DraftPromptBuilderTests
{
    [Test]
    public void Build_IncludesPromptPackAndRetrievedChunks()
    {
        var rules = "## Prefixes\n\n#faction\n\n## Immediate orders\n\nMOVE.\n\n### MOVE\n\nsyntax";
        var pack = PromptPackBuilder.BuildPromptPack(
            2,
            "secret",
            stripPassword: true,
            rules,
            reportText: "Report body",
            objectiveText: "Move to orbit");

        RetrievalResult[] chunks =
        [
            new RetrievalResult(
                new StoredChunk(
                    1,
                    new TextChunk("MOVE syntax details", new ChunkMetadata("rules", "MOVE", null, "rules.md", "MOVE")),
                    [1f, 0f]),
                0.9f),
        ];

        var hints = new OrderDraftHints { PersonaPreference = "researcher", AnomalyRegionId = "R00011" };
        var prompt = DraftPromptBuilder.BuildChatPrompt(pack, chunks, ordersTemplate: "#modulestack 101\n#end", hints);

        Assert.That(prompt, Does.Contain("#faction 2"));
        Assert.That(prompt, Does.Not.Contain("secret"));
        Assert.That(prompt, Does.Contain("Report body"));
        Assert.That(prompt, Does.Contain("MOVE syntax details"));
        Assert.That(prompt, Does.Contain("Orders template from report"));
        Assert.That(prompt, Does.Contain("Example output shape"));
        Assert.That(prompt, Does.Contain("use msrvtm as new109"));
        Assert.That(prompt, Does.Contain("@research R00011"));
    }
}
