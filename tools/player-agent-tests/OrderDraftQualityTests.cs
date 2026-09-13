using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderDraftQualityTests
{
    [Test]
    public void IsUsable_ResearcherHandDraft_Passes()
    {
        const string draft = """
            #faction 3 "pw"
            #modulestack 210005
            get 2 iron from 210003
            use moblib as new109
            #modulestack 210001
            @produce cash
            #modulestack 210003
            sell 200 food at average
            #modulestack new109
            @get 1 terran from 210001
            @move R00011
            @research R00011
            #end
            """;

        Assert.That(OrderDraftQuality.IsUsable(draft, "researcher"), Is.True);
    }

    [Test]
    public void IsUsable_ActiveOnlyDraft_Fails()
    {
        const string draft = """
            #faction 3 "pw"
            active 210001
            #end
            """;

        Assert.That(OrderDraftQuality.IsUsable(draft, "researcher"), Is.False);
    }
}
