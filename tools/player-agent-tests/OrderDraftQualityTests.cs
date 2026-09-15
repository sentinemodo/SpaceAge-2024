using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderDraftQualityTests
{
    private const string GoldenNorthwind = """
        #faction 2 "FMUZ72H9Zh"
        DECLARE FACTION 14 ENEMY

        #modulestack 200001
        @produce terran

        #modulestack 200003
        @get all food from 200006
        @get all carbon from 200004

        #modulestack 200004
        @use hcdril

        #modulestack 200005
        use armcbt as new2 for 200001
        +get 8 iron from 200003
        +get 2 titani from 200003
        use armcbt as new3 for 200001
        +get 8 iron from 200003
        +get 2 titani from 200003
        use grndtr as new1 for 200001
        +get 30 iron from 200003
        +get 2 titani from 200003

        #modulestack 200006
        @use farmng

        #modulestack 200007
        @produce energy

        #modulestack new1
        move R00014
        +get 1 terran from 200001
        +get 2 oil from 200003
        +get 2 food from 200003

        #modulestack new2
        has 1 tanks
        -get 16 terran from 200001
        -get 8 oil from 200003
        -get 32 food from 200003
        active new2
        move R00009
        tactic destroy

        #modulestack new3
        has 1 tanks
        -get 16 terran from 200001
        -get 8 oil from 200003
        -get 32 food from 200006
        move R00009
        tactic destroy

        #end
        """;

    [Test]
    public void IsUsable_GoldenNorthwindOrders_Passes()
    {
        Assert.That(OrderDraftQuality.IsUsable(GoldenNorthwind, "military"), Is.True);
    }

    [Test]
    public void IsUsable_MilitarySellFood_Fails()
    {
        var draft = GoldenNorthwind.Replace(
            "@get all carbon from 200004",
            "@get all carbon from 200004\nsell 200 food at average");
        Assert.That(OrderDraftQuality.IsUsable(draft, "military"), Is.False);
        Assert.That(OrderDraftQuality.DescribeMilitaryPersonaViolations(draft), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_MilitaryAtMove_FailsMobileAtGate()
    {
        var draft = GoldenNorthwind.Replace("move R00009", "@move R00009");
        Assert.That(OrderDraftQuality.IsUsable(draft, "military"), Is.False);
        Assert.That(OrderDraftQuality.DescribeMobileAtViolations(draft), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_FactoryWithoutPlusGet_Fails()
    {
        var draft = GoldenNorthwind.Replace("+get", "get");
        Assert.That(OrderDraftQuality.HasFactoryPlusGetViolations(draft, "military"), Is.True);
        Assert.That(OrderDraftQuality.IsUsable(draft, "military"), Is.False);
    }

    [Test]
    public void IsUsable_ResearcherHandDraft_Passes()
    {
        const string draft = """
            #faction 3 "pw"
            #modulestack 210005
            @get 2 iron from 210003
            use moblib as new109
            #modulestack 210001
            @produce cash
            #modulestack 210003
            sell 200 food at average
            #modulestack new109
            @get 1 terran from 210001
            @get 2 oil from 210003
            @move R00011
            @research R00011
            #end
            """;

        Assert.That(OrderDraftQuality.IsUsable(draft, "researcher"), Is.True);
    }

    [Test]
    public void IsUsable_MilitaryWithoutTwoTanks_Fails()
    {
        var draft = GoldenNorthwind.Replace(
            """
            use armcbt as new3 for 200001
            +get 8 iron from 200003
            +get 2 titani from 200003

            #modulestack 200006
            """,
            "#modulestack 200006");
        Assert.That(OrderDraftQuality.IsUsable(draft, "military"), Is.False);
    }

    [Test]
    public void IsUsable_TankWithout32Food_FailsTravelGate()
    {
        var draft = GoldenNorthwind.Replace("-get 32 food from 200003", "-get 2 food from 200003");
        Assert.That(OrderDraftQuality.HasTravelProvisioningViolations(draft), Is.True);
    }
}
