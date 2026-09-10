using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class SharedIndexRefreshPlannerTests
{
    [Test]
    public void ParseModes_BothExpandsToTestAndCampaign()
    {
        var modes = SharedIndexRefreshPlanner.ParseModes("both");

        Assert.That(modes, Is.EqualTo(new[] { PlayMode.Test, PlayMode.Campaign }));
    }

    [Test]
    public void ParseModes_SingleModeReturnsOneEntry()
    {
        Assert.That(SharedIndexRefreshPlanner.ParseModes("test"), Is.EqualTo(new[] { PlayMode.Test }));
        Assert.That(SharedIndexRefreshPlanner.ParseModes("campaign"), Is.EqualTo(new[] { PlayMode.Campaign }));
    }

    [Test]
    public void ParseModes_RejectsUnknownValue()
    {
        Assert.Throws<InvalidOperationException>(() => SharedIndexRefreshPlanner.ParseModes("samplegame"));
    }

    [Test]
    public void BuildRevisionNote_IncludesEngineVersionWhenPresent()
    {
        var note = SharedIndexRefreshPlanner.BuildRevisionNote(
            PlayMode.Campaign,
            engineVersion: "0.1.148",
            catalogRevision: null,
            refreshedAtUtc: "2026-09-10T12:00:00Z");

        Assert.That(note, Does.Contain("shared-campaign"));
        Assert.That(note, Does.Contain("0.1.148"));
        Assert.That(note, Does.Contain("2026-09-10"));
    }

    [Test]
    public void BuildSpotCheckQueries_UsesVerbAndTechWhenProvided()
    {
        var queries = SharedIndexRefreshPlanner.BuildSpotCheckQueries("JUMP", "helium");

        Assert.That(queries, Has.Count.EqualTo(2));
        Assert.That(queries[0].Label, Is.EqualTo("verb"));
        Assert.That(queries[0].Query, Does.Contain("JUMP"));
        Assert.That(queries[0].VerbFilter, Is.EqualTo("JUMP"));
        Assert.That(queries[1].Label, Is.EqualTo("tech"));
        Assert.That(queries[1].Query, Does.Contain("helium"));
    }
}
