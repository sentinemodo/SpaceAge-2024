using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderDraftQualityTests
{
    private const string GoldenNorthwind = """
        #faction 2 "FMUZ72H9Zh"
        DECLARE FACTION 14 ENEMY

        #modulestack 200001
        set hold 20 terran
        grant item 16 terran to 200001
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
        @use iminng

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
        Assert.That(OrderDraftQuality.IsUsable(GoldenResearcherSilicate, "researcher", SilicateWindGrantTemplate), Is.True);
    }

    [Test]
    public void IsUsable_ResearcherMissingMsrvtmGrant_Fails()
    {
        var draft = GoldenResearcherSilicate.Replace("grant technology msrvtm to 280005", string.Empty);
        Assert.That(OrderDraftQuality.DescribeResearcherPersonaViolations(draft), Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "researcher"), Is.False);
    }

    private const string SilicateWindGrantTemplate = """
        Orders Template:
        #modulestack 280001
        ; + Silicate Headquarters [280001], corporate headquarters [corphq], immobile.
        #modulestack 280004
        ; + surface drill [280004], surface drill [sdrill], immobile.
        #modulestack 280007
        ; + wind powerplant [280007], 8 wind powerplants [wnplnt], immobile.
        #end
        """;

    [Test]
    public void IsUsable_ResearcherIminngOnArborDrill_Fails()
    {
        var draft = GoldenResearcherSilicate
            .Replace("@use iminng", "@use hcdril\n@use iminng");
        Assert.That(OrderDraftQuality.DescribeResearcherPersonaViolations(draft, reportText: null), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_WindGrant_HcdrilOnDrill_Fails()
    {
        var draft = GoldenNorthwind.Replace("@use hcdril", "@use hcdril");
        var report = SilicateWindGrantTemplate;
        Assert.That(OrderDraftQuality.DescribeWindGrantDrillViolations(draft, report, "military"), Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "military", report), Is.False);
    }

    [Test]
    public void IsUsable_WindGrant_IminngOnDrill_PassesWindGate()
    {
        var draft = GoldenNorthwind
            .Replace("@use hcdril", "@use iminng")
            .Replace("@get all carbon from 200004", string.Empty);
        Assert.That(OrderDraftQuality.DescribeWindGrantDrillViolations(draft, SilicateWindGrantTemplate, "military"), Is.Empty);
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

        #modulestack 230005
        grant technology mcored to 230005
        grant technology msrvtm to 230005
        use mcored as new108 for 230001
        +get 25 iron from 230003
        +get 10 titani from 230003
        use msrvtm as new109 for 230001
        +get 25 iron from 230003
        +get 10 titani from 230003

        #modulestack new108
        has 1 cdrill
        -get 6 terran from 230001
        deactivate 1

        #modulestack new109
        move R00009
        +get 1 terran from 230001
        +get 2 oil from 230003
        +get 2 food from 230003

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
        var draft = GoldenEconomicSundock.Replace("grant technology mcored to 230005", string.Empty);
        var violations = OrderDraftQuality.DescribeEconomicPersonaViolations(draft);
        Assert.That(violations, Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "economic"), Is.False);
    }

    [Test]
    public void IsUsable_GrantToFactoryAlias_Fails()
    {
        var draft = GoldenEconomicSundock.Replace("grant technology mcored to 230005", "grant technology mcored to factory");
        Assert.That(OrderDraftQuality.DescribeGrantTargetViolations(draft), Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "economic"), Is.False);
    }

    [Test]
    public void IsUsable_GrantUnderFactionHeader_PassesPersonaGate()
    {
        var draft = GoldenEconomicSundock
            .Replace("#modulestack 230004\n        grant item 50 iron to 230004\n        grant item 10 titani to 230004\n        ", string.Empty)
            .Replace(
                "#faction 5 \"sundock\"\n",
                """
                #faction 5 "sundock"
                grant item 50 iron to 230004
                grant item 10 titani to 230004

                """);
        Assert.That(OrderDraftQuality.IsUsable(draft, "economic"), Is.True);
    }

    private const string NorthwindGrantExcerpt = """
          Northwind Grant [R00008] (1,1), grassland region, settlement capacity 8/0.
          Exits:
            West Deep [R00007] (0,1), ocean region, naval travel duration 3 weeks.
            Mid Vale [R00009] (2,1), grassland region, ground travel duration 3 weeks, anomaly detected.
            Shelf [R00002] (1,0), sea region, naval travel duration 3 weeks.
            Farm Belt [R00014] (1,2), grassland region, ground travel duration 3 weeks, settlement detected.
          Resources: 600 units of food [food], 40 units of carbon [carbon]
        """;

    [Test]
    public void IsUsable_MoveToGrantExit_PassesWithReport()
    {
        var draft = GoldenNorthwind.Replace("-move R00009", "-move R00014");
        Assert.That(OrderDraftQuality.IsUsable(draft, "military", NorthwindGrantExcerpt), Is.True);
    }

    [Test]
    public void IsUsable_MoveToUnreachableRegion_FailsWithReport()
    {
        var draft = GoldenNorthwind.Replace("-move R00009", "-move R00099");
        Assert.That(OrderDraftQuality.DescribeMoveRegionReachabilityViolations(draft, NorthwindGrantExcerpt), Is.Not.Empty);
        Assert.That(OrderDraftQuality.IsUsable(draft, "military", NorthwindGrantExcerpt), Is.False);
    }

    [Test]
    public void IsUsable_SdrillHcdrilWithoutCarbonInRegion_Fails()
    {
        const string anvilGrant = """
              Ironclad Grant [R00045] (1,1), grassland region, settlement capacity 8/0.
              Exits:
                Slope [R00046] (2,1), grassland region, ground travel duration 3 weeks.
              Resources: 140 units of food [food], 20 units of iron [iron]
            """;
        var draft = """
            #modulestack 250004
            @use hcdril
            #end
            """;
        Assert.That(OrderDraftQuality.DescribeDrillUseResourceViolations(draft, anvilGrant), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_SdrillBothDrillTechs_Fails()
    {
        var draft = """
            #modulestack 230004
            @use hcdril
            @use iminng
            #end
            """;
        Assert.That(OrderDraftQuality.DescribeCombinedDrillUseViolations(draft), Is.Not.Empty);
    }

    [Test]
    public void IsUsable_MilitaryTanksWithoutTerranGrant_FailsWhenBankHigh()
    {
        const string bankReport = "Bank account balance: 10000.\n" + NorthwindGrantExcerpt;
        var draft = GoldenNorthwind.Replace("grant item 16 terran to 200001", string.Empty);
        Assert.That(OrderDraftQuality.DescribeMilitaryPersonaViolations(draft, bankReport), Is.Not.Empty);
    }

    [Test]
    public void MergeBootstrapAndProduction_CombinesStacksAndKeepsPreamble()
    {
        const string bootstrap = """
            #faction 8 "pwd"
            grant item 10 iron to 800003

            #modulestack 800001
            @produce terran

            #modulestack 800004
            @use hcdril

            #end
            """;

        const string production = """
            #faction 8 "pwd"

            #modulestack 800005
            grant technology mcored to 800005
            use mcored as new10 for 800001
            +get 25 iron from 800003

            #end
            """;

        var merged = OrderDraftMerger.MergeBootstrapAndProduction(bootstrap, production);
        Assert.That(merged, Does.Contain("#faction 8"));
        Assert.That(merged, Does.Contain("#modulestack 800001"));
        Assert.That(merged, Does.Contain("#modulestack 800004"));
        Assert.That(merged, Does.Contain("#modulestack 800005"));
        Assert.That(merged, Does.Contain("use mcored as new10"));
        Assert.That(merged.Split("#end", StringSplitOptions.None).Length, Is.EqualTo(2));
        Assert.That(merged.TrimEnd(), Does.EndWith("#end"));
    }

    [Test]
    public void IsUsable_Beta1RedraftFactionOrders_PassQualityGate()
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        const int reportTurn = 1;
        for (var faction = 4; faction <= 11; faction++)
        {
            var folder = RepoPaths.FactionFolder(repoRoot, "beta-1", faction);
            var ordersPath = OrderFileNaming.ResolveActiveOrderPath(folder, faction, reportTurn)
                ?? throw new InvalidOperationException($"faction {faction}: no active orders file");
            var reportPath = Path.Combine(folder, $"report.{reportTurn}.{faction}.txt");
            var personaPath = Path.Combine(folder, "persona.md");
            var persona = File.Exists(personaPath)
                ? VerbInference.DetectPersonaPreference(File.ReadAllText(personaPath))
                : null;
            var orders = File.ReadAllText(ordersPath);
            var report = File.ReadAllText(reportPath);
            Assert.That(
                OrderDraftQuality.IsUsable(orders, persona, report),
                Is.True,
                () => $"faction {faction} {ordersPath}");
        }
    }

    [Test]
    public void IsUsable_F9Orders522_Turn2FailsOnGiveAllBeforeMove()
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var folder = RepoPaths.FactionFolder(repoRoot, "beta-1", 9);
        var ordersPath = Path.Combine(folder, "orders.9.2.2.txt");
        if (!File.Exists(ordersPath))
        {
            Assert.Ignore("orders.9.2.2.txt not present in this checkout");
        }

        var report = File.ReadAllText(Path.Combine(folder, "report.2.9.txt"));
        var orders = File.ReadAllText(ordersPath);
        var violations = OrderDraftQuality.DescribeMilitaryFieldedTurnViolations(orders, report);
        Assert.That(
            violations.Any(v => v.Contains("give all", StringComparison.OrdinalIgnoreCase)),
            Is.True,
            () => string.Join("; ", violations));
    }

    [Test]
    public void IsUsable_F9Orders524_Turn2PassesQualityAndLint()
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var folder = RepoPaths.FactionFolder(repoRoot, "beta-1", 9);
        var ordersPath = Path.Combine(folder, "orders.9.2.4.txt");
        var reportPath = Path.Combine(folder, "report.2.9.txt");
        var persona = VerbInference.DetectPersonaPreference(File.ReadAllText(Path.Combine(folder, "persona.md")));
        var orders = File.ReadAllText(ordersPath);
        var report = File.ReadAllText(reportPath);
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(File.ReadAllText(Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md")));
        var lint = OrderDraftLinter.Lint(orders, allowlist);
        Assert.That(lint.IsValid, Is.True, () => string.Join("; ", lint.Errors));
        Assert.That(
            OrderDraftQuality.IsUsable(orders, persona, report),
            Is.True,
            () => OrderDraftQuality.BuildRetryInstruction(persona, orders, report));
    }
}
