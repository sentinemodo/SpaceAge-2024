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
        set hold 20 terran
        @produce terran

        #modulestack 200003
        grant item 50 iron to 200003
        grant item 20 oil to 200003
        grant item 10 titani to 200003
        @get all food from 200006
        @get all carbon from 200004

        #modulestack 200004
        @use hcdril

        #modulestack 200005
        grant technology armcbt to 200005
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
        -move R00009
        tactic destroy

        #modulestack new3
        has 1 tanks
        -get 16 terran from 200001
        -get 8 oil from 200003
        -get 32 food from 200006
        -move R00009
        tactic destroy

        #end
        """;

    [Test]
    public void IsUsable_GoldenNorthwindOrders_Passes()
    {
        Assert.That(OrderDraftQuality.IsUsable(GoldenNorthwind, "military"), Is.True);
    }

    [Test]
    public void IsUsable_MilitaryMissingArmcbtGrant_Fails()
    {
        var draft = GoldenNorthwind.Replace("grant technology armcbt to 200005", string.Empty);
        Assert.That(OrderDraftQuality.DescribeMilitaryPersonaViolations(draft), Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "military"), Is.False);
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
        var draft = GoldenNorthwind.Replace("-move R00009", "@move R00009");
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

    private const string GoldenResearcherSilicate = """
        #faction 10 "silicat"
        #modulestack 280001
        set hold 20 terran
        @produce terran

        #modulestack 280003
        grant item 2 iron to 280003
        grant item 2 silici to 280003
        @get all food from 280006
        @get all carbon from 280004
        sell 200 food at average

        #modulestack 280004
        @use hcdril

        #modulestack 280006
        @use farmng

        #modulestack 280007
        @produce energy

        #modulestack 280005
        grant technology msrvtm to 280005
        use msrvtm as new110 for 280001
        +get 2 iron from 280003
        +get 2 silici from 280003

        #modulestack new110
        move R00052
        +get 1 terran from 280001
        +get 2 oil from 280003
        +get 2 food from 280003
        @research R00052
        #end
        """;

    [Test]
    public void IsUsable_ResearcherHandDraft_Passes()
    {
        Assert.That(OrderDraftQuality.IsUsable(GoldenResearcherSilicate, "researcher"), Is.True);
    }

    [Test]
    public void IsUsable_ResearcherMissingMsrvtmGrant_Fails()
    {
        var draft = GoldenResearcherSilicate.Replace("grant technology msrvtm to 280005", string.Empty);
        Assert.That(OrderDraftQuality.DescribeResearcherPersonaViolations(draft), Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "researcher"), Is.False);
    }

    [Test]
    public void IsUsable_ResearcherIminngOnDrill_Fails()
    {
        var draft = GoldenResearcherSilicate.Replace("@use hcdril", "@use hcdril\n@use iminng");
        Assert.That(OrderDraftQuality.IsUsable(draft, "researcher"), Is.False);
        Assert.That(OrderDraftQuality.DescribeResearcherPersonaViolations(draft), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_MilitaryWithoutTwoTanks_Fails()
    {
        var draft = GoldenNorthwind.Replace("use armcbt as new3 for 200001", string.Empty, StringComparison.Ordinal);
        Assert.That(OrderDraftQuality.IsUsable(draft, "military"), Is.False);
    }

    [Test]
    public void IsUsable_TankWithout32Food_FailsTravelGate()
    {
        var draft = GoldenNorthwind.Replace("-get 32 food from 200003", "-get 2 food from 200003");
        Assert.That(OrderDraftQuality.HasTravelProvisioningViolations(draft), Is.True);
    }

    private const string GoldenAbsentNorthwind = """
        #faction 2 "northwnd"
        #modulestack 200001
        set hold 20 terran
        @produce cash

        #modulestack 200003
        @get all food from 200006
        @get all carbon from 200004

        #modulestack 200004
        @use hcdril

        #modulestack 200006
        @use farmng

        #modulestack 200007
        @produce energy
        #end
        """;

    [Test]
    public void IsUsable_AbsentPlayerMaintenanceOrders_Passes()
    {
        Assert.That(OrderDraftQuality.IsUsable(GoldenAbsentNorthwind, "absent-player"), Is.True);
    }

    [Test]
    public void IsUsable_AbsentPlayerProduceTerran_Fails()
    {
        var draft = GoldenAbsentNorthwind.Replace("@produce cash", "@produce terran");
        Assert.That(OrderDraftQuality.IsUsable(draft, "absent-player"), Is.False);
        Assert.That(OrderDraftQuality.DescribeAbsentPlayerPersonaViolations(draft), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_AbsentPlayerMissingHcdril_Fails()
    {
        var draft = GoldenAbsentNorthwind.Replace("@use hcdril", string.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "absent-player"), Is.False);
        Assert.That(OrderDraftQuality.DescribeAbsentPlayerPersonaViolations(draft), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_AbsentPlayerIminngOnDrill_Fails()
    {
        var draft = GoldenAbsentNorthwind + "\n#modulestack 200004\n@use iminng\n";
        Assert.That(OrderDraftQuality.IsUsable(draft, "absent-player"), Is.False);
    }

    private const string GoldenEconomicSundock = """
        #faction 5 "sundock"
        #modulestack 230007
        @produce energy

        #modulestack 230001
        set hold 20 terran
        @produce terran

        #modulestack 230004
        grant item 50 iron to 230004
        grant item 10 titani to 230004
        @use hcdril
        @use iminng

        #modulestack 230005
        grant technology mcored to factory
        grant technology msrvtm to factory
        use mcored as new108 for 230001
        +get 25 iron from 230003
        +get 10 titani from 230003

        #modulestack new108
        has 1 cdrill
        -get 6 terran from 230001
        deactivate 1

        #modulestack 230006
        @use farmng

        #modulestack 230003
        @get all food from 230006
        @get all carbon from 230004
        sell 200 food at average
        #end
        """;

    [Test]
    public void IsUsable_EconomicGrantBootstrap_Passes()
    {
        Assert.That(OrderDraftQuality.IsUsable(GoldenEconomicSundock, "economic"), Is.True);
    }

    [Test]
    public void IsUsable_EconomicMcoredWithoutGrant_Fails()
    {
        var draft = GoldenEconomicSundock.Replace("grant technology mcored to factory", string.Empty);
        var violations = OrderDraftQuality.DescribeEconomicPersonaViolations(draft);
        Assert.That(violations, Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "economic"), Is.False);
    }
}
