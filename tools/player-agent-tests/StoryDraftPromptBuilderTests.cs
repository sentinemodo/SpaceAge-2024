using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class StoryDraftPromptBuilderTests
{
    [Test]
    public void BuildChunkPrompts_ReturnsThreeSections()
    {
        var context = new StoryDraftContext(
            "Greenwell",
            3,
            1,
            "## Preference: researcher\nHome: Arbor in Helios",
            "CT0007: Greenwell bazaar charter.\n+ Greenwell Headquarters [210001]",
            "CT0007 town charter reward servic",
            "210001, 210003, 210005")
        {
            HasPriorStory = false,
        };

        var chunks = StoryDraftPromptBuilder.BuildChunkPrompts(context);

        Assert.That(chunks.Count, Is.EqualTo(3));
        Assert.That(chunks.Select(chunk => chunk.Name), Is.EqualTo(new[] { "strategic", "tactical", "narrative" }));
        Assert.That(chunks[1].UserPrompt, Does.Contain("use msrvtm as new109"));
        Assert.That(chunks[1].UserPrompt, Does.Contain("8-point threshold"));
        Assert.That(chunks[1].UserPrompt, Does.Contain("oil"));
        Assert.That(chunks[1].UserPrompt, Does.Not.Contain("wndtrb"));
    }

    [Test]
    public void BuildChunkPrompts_EconomicPersona_PrioritizesCoreDrillAndMoblabScout()
    {
        var context = new StoryDraftContext(
            "Greenwell",
            3,
            1,
            "## Preference: economic\nHome: Arbor in Helios",
            "CT0007: Greenwell bazaar charter.\n+ Greenwell Headquarters [210001]",
            "CT0007 town charter reward servic",
            "210001, 210003, 210005")
        {
            HasPriorStory = false,
        };

        var chunks = StoryDraftPromptBuilder.BuildChunkPrompts(context);

        Assert.That(chunks[1].UserPrompt, Does.Contain("use mcored as new108"));
        Assert.That(chunks[1].UserPrompt, Does.Contain("moblab"));
        Assert.That(chunks[1].UserPrompt, Does.Contain("deep pocket"));
        Assert.That(chunks[1].UserPrompt, Does.Not.Contain("grndtr"));
    }

    [Test]
    public void AssembleStory_PrefixesTitleAndJoinsSections()
    {
        var story = StoryDraftPromptBuilder.AssembleStory(
            "Greenwell",
            1,
            ["## Strategic objective\nOne.", "## Tactical objective\nTwo."]);

        Assert.That(story, Does.StartWith("# Greenwell — turn 1"));
        Assert.That(story, Does.Contain("## Strategic objective"));
        Assert.That(story, Does.Contain("## Tactical objective"));
    }
}
