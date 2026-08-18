using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TResearch : TTest
	{
		[SetUp]
		public void setupResearch()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownResearch()
		{
			this.ClearGame();
		}

		private ModuleStack createResearchLab()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack lab = ModuleStack.All.GetOrCreateNewModuleStack(testFaction, "100000");
			lab.Parent = ModuleStack.All["000005"]; // nested so it is active without its own energy source
			lab.ModuleType = ModuleType.All["cmplib"];
			lab.AddModule();
			return lab;
		}

		private ResearchOrder assignResearch(ModuleStack lab, string command)
		{
			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack " + lab.Name,
				command,
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			return (ResearchOrder)lab.Orders[0];
		}

		private Technologies allTechnologies()
		{
			Technologies all = new Technologies();
			foreach (Technology technology in Technology.All)
			{
				all.Add(technology);
			}
			return all;
		}

		private EResearchType parseResearch(string parameter)
		{
			ResearchOrder order = new ResearchOrder(this.game.ModuleStacks["100001"]);
			order.Parse(parameter);
			return order.ResearchType;
		}

		[Test]
		public void AssignResearchOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleType computerLibrary = ModuleType.All["cmplib"];

			ModuleStack researchModuleStack = ModuleStack.All.GetOrCreateNewModuleStack(testFaction, "100000");
			researchModuleStack.ModuleType = computerLibrary;
			researchModuleStack.AddModule();

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100000",
				"research group military",
				"#end"
			};

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.That(researchModuleStack.Orders.Count, Is.EqualTo(1));

			Assert.That(researchModuleStack.Orders[0] is ResearchOrder);
			ResearchOrder researchOrder = (ResearchOrder)researchModuleStack.Orders[0];
			Assert.That(researchOrder.ResearchType, Is.EqualTo(EResearchType.Group));
			Assert.That(researchOrder.ResearchToken, Is.EqualTo("military"));
			Assert.That(researchOrder.Repeat, Is.EqualTo(1));
		}

		[Test]
		public void ExecuteResearchOrder()
		{
			Sequence.Ints.Push(50);
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleType computerLibrary = ModuleType.All["cmplib"];

			ModuleStack researchModuleStack = ModuleStack.All.GetOrCreateNewModuleStack(testFaction, "100000");
			researchModuleStack.Parent = ModuleStack.All["000005"];
			researchModuleStack.ModuleType = computerLibrary;
			researchModuleStack.AddModule();

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100000",
				"research group military",
				"#end"
			};

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			ResearchOrder researchOrder = (ResearchOrder)researchModuleStack.Orders[0];

			int week = this.game.Week;
			for (int i = 1; i <= 3; i++)
			{
				researchModuleStack.ExecutedLongOrder = false;
				researchModuleStack.Execute(week++);
			}

			// there should be breakthrough on the second week and on the third week there should be a 1 RP generated
			this.consoleOutReport("research progress report", researchModuleStack, researchModuleStack.Owner);

			// there should be a new technology seen
			this.consoleOutReport("technologies seen", testFaction.TechnologiesToShow.ReportDescriptions(testFaction, 1), researchModuleStack.Owner);
		}

		[Test]
		public void Research_CostDefaultsDoublePerLevelAndOverride()
		{
			Technology t1 = new Technology("test_l1"); t1.Level = 1;
			Technology t2 = new Technology("test_l2"); t2.Level = 2;
			Technology t3 = new Technology("test_l3"); t3.Level = 3;
			Assert.That(t1.Cost, Is.EqualTo(8));
			Assert.That(t2.Cost, Is.EqualTo(16));
			Assert.That(t3.Cost, Is.EqualTo(32));

			Technology overridden = new Technology("test_override"); overridden.Level = 1; overridden.Cost = 5;
			Assert.That(overridden.Cost, Is.EqualTo(5));

			Assert.That(Research.DefaultCostForLevel(0), Is.EqualTo(0));
			Assert.That(Research.DefaultCostForLevel(1), Is.EqualTo(8));
			Assert.That(Research.DefaultCostForLevel(2), Is.EqualTo(16));
			Assert.That(Research.DefaultCostForLevel(3), Is.EqualTo(32));
			Assert.That(Research.DefaultCostForLevel(4), Is.EqualTo(64));
		}

		[Test]
		public void Research_CostsMeetQuarterlyBreakthroughTargets()
		{
			// constant weekly hazard p = 1 - (1 - 1/cost)^output, single base lab output = 1
			double l1 = 1 - Math.Pow(1.0 - 1.0 / Research.DefaultCostForLevel(1), 13); // 1 quarter
			double l2 = 1 - Math.Pow(1.0 - 1.0 / Research.DefaultCostForLevel(2), 26); // 2 quarters
			double l3 = 1 - Math.Pow(1.0 - 1.0 / Research.DefaultCostForLevel(3), 52); // 4 quarters
			Assert.That(l1, Is.InRange(0.78, 0.84));
			Assert.That(l2, Is.InRange(0.78, 0.84));
			Assert.That(l3, Is.InRange(0.78, 0.84));
		}

		[Test]
		public void Research_AccruesResearchPointsWhenNoBreakthrough()
		{
			ModuleStack lab = this.createResearchLab();
			ResearchOrder order = this.assignResearch(lab, "research");

			Sequence.Ints.Push(5); // breakthrough roll != 0 -> no breakthrough
			order.Execute(this.game.Week);

			Assert.That(lab.ResearchPoints, Is.EqualTo(1)); // research-output 1 x 1 module
			Assert.That(lab.Technologies.Count, Is.EqualTo(0));
		}

		[Test]
		public void Research_BreakthroughAwardsTechnologyAndZeroesResearchPoints()
		{
			ModuleStack lab = this.createResearchLab();
			lab.ResearchPoints = 7; // accumulated progress from earlier weeks
			ResearchOrder order = this.assignResearch(lab, "research");

			Sequence.Ints.Push(1); // technology selection roll
			Sequence.Ints.Push(0); // breakthrough roll == 0 -> breakthrough (popped first)
			order.Execute(this.game.Week);

			Assert.That(lab.Technologies.Count, Is.EqualTo(1));
			Assert.That(lab.Owner.TechnologiesToShow.Count, Is.GreaterThanOrEqualTo(1));
			Assert.That(lab.Technologies.Contains(lab.Owner.TechnologiesToShow[0].Name), Is.True);
			Assert.That(lab.ResearchPoints, Is.EqualTo(0)); // zeroed on breakthrough
		}

		[Test]
		public void ResearchParse_ResolvesBareParameterKinds()
		{
			Assert.That(this.parseResearch("military"), Is.EqualTo(EResearchType.Tag));
			Assert.That(this.parseResearch("he3min"), Is.EqualTo(EResearchType.Technology));
			Assert.That(this.parseResearch("heliu3"), Is.EqualTo(EResearchType.ItemType));
			Assert.That(this.parseResearch("fusrec"), Is.EqualTo(EResearchType.ModuleType));
			Assert.That(this.parseResearch("R00001"), Is.EqualTo(EResearchType.SpaceObject));
		}

		[Test]
		public void ResearchPreference_TagResolvesTaggedTechnologies()
		{
			Technologies all = this.allTechnologies();
			ResearchOrder order = new ResearchOrder(this.game.ModuleStacks["100001"]);
			order.ResearchType = EResearchType.Tag;
			order.ResearchToken = "military";

			Technologies preferred = Research.PreferredTechnologies(order, all);
			Assert.That(preferred["stnrdf"], Is.Not.Null);
			Assert.That(preferred["miltac"], Is.Not.Null);
			Assert.That(preferred["fossil"], Is.Null); // production-tagged, not military
		}

		[Test]
		public void ResearchPreference_TechnologyResolvesEnabledBy()
		{
			Technologies all = this.allTechnologies();
			ResearchOrder order = new ResearchOrder(this.game.ModuleStacks["100001"]);
			order.ResearchType = EResearchType.Technology;
			order.Technology = Technology.All["he3min"];

			Technologies preferred = Research.PreferredTechnologies(order, all);
			Assert.That(preferred["he3fus"], Is.Not.Null); // requires he3min
			Assert.That(preferred["he3drl"], Is.Not.Null); // requires he3min
			Assert.That(preferred["fossil"], Is.Null);
		}

		[Test]
		public void ResearchPreference_ItemModuleAndSpaceObject()
		{
			Technologies all = this.allTechnologies();

			ResearchOrder itemOrder = new ResearchOrder(this.game.ModuleStacks["100001"]);
			itemOrder.ResearchType = EResearchType.ItemType;
			itemOrder.ItemType = ItemType.All["heliu3"];
			Technologies byItem = Research.PreferredTechnologies(itemOrder, all);
			Assert.That(byItem["he3min"], Is.Not.Null); // produces heliu3
			Assert.That(byItem["he3fus"], Is.Not.Null); // consumes heliu3

			ResearchOrder moduleOrder = new ResearchOrder(this.game.ModuleStacks["100001"]);
			moduleOrder.ResearchType = EResearchType.ModuleType;
			moduleOrder.ModuleType = ModuleType.All["fusrec"];
			Technologies byModule = Research.PreferredTechnologies(moduleOrder, all);
			Assert.That(byModule["he3fus"], Is.Not.Null); // produces fusrec

			ResearchOrder objectOrder = new ResearchOrder(this.game.ModuleStacks["100001"]);
			objectOrder.ResearchType = EResearchType.SpaceObject;
			objectOrder.ResearchToken = "R00001"; // region holding iron + food
			Technologies byObject = Research.PreferredTechnologies(objectOrder, all);
			Assert.That(byObject["fossil"], Is.Not.Null); // consumes iron present on R00001
		}

		[Test]
		public void Research_Preference_TagAwardsMilitaryTechnology()
		{
			ModuleStack lab = this.createResearchLab();
			ResearchOrder order = this.assignResearch(lab, "research military");

			Sequence.Ints.Push(50); // technology selection roll (weighted)
			Sequence.Ints.Push(10); // preference roll <= 50 -> pick from preferred pool
			Sequence.Ints.Push(0);  // breakthrough roll == 0 (popped first)
			order.Execute(this.game.Week);

			Assert.That(lab.Technologies.Count, Is.EqualTo(1));
			foreach (Technology technology in lab.Technologies)
			{
				Assert.That(technology.HasTag("military"), "awarded technology should be military-tagged");
			}
		}

		[Test]
		public void TechnologyReportDescriptions_MatchExpected()
		{
			Faction faction = this.game.Factions["2"];
			Technologies techs = new Technologies();
			techs.Add(Technology.All["miltac"]); // no product
			techs.Add(Technology.All["he3min"]); // produces the heliu3 item
			techs.Add(Technology.All["advres"]); // produces the advlib module
			techs.Add(Technology.All["rckter"]); // produces infantry equipment item

			List<string> expected = new List<string>
			{
				"+ military tactics [miltac]: Elementary military tactics, which enable a higher level of combat proficiency.",
				"+ helium-3 mining [he3min]: The extraction and refining of helium-3 from regolith and gas. Helium-3 mining can be carried out by any extraction module that has this technology loaded.",
				"  - unit of helium-3 [heliu3]: A light, non-radioactive helium isotope prized as clean fusion fuel; scarce on planets but abundant in lunar regolith.",
				"+ advanced computing [advres]: Next-generation computing enabling far larger research complexes.",
				"  - advanced research complex [advlib]: A large, high-throughput research facility building on computer-library methods.",
				"+ rocket launcher production [rckter]: Manufacture of portable rocket launchers issued to infantry battalions.",
				"  - rocket launchers [rctlnc]: Infantry equipment. Size: 100, mass: 100. Attack: 2, damage: 2. An infantry battalion carrying one gains +2 attack and +2 damage."
			};

			List<string> actual = techs.ReportDescriptions(faction, 0);
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < expected.Count; i++)
			{
				Assert.That(actual[i], Is.EqualTo(expected[i]));
			}
		}

		[Test]
		public void TechnologyReport_AppearsBetweenBankAndGalaxy()
		{
			Faction faction = this.game.Factions["2"];
			faction.TechnologiesToShow.Add(Technology.All["advres"]);
			foreach (Faction f in this.game.Factions.Values)
			{
				f.Options.XmlReport = false;
			}

			ReportWriter writer = new ReportWriter(this.game, this.dataFile, Directory.GetCurrentDirectory());
			writer.GenerateReports(Directory.GetCurrentDirectory());

			string[] lines = File.ReadAllLines(
				Path.Combine(Directory.GetCurrentDirectory(), string.Format("report.{0}.2.txt", this.game.Turn)),
				System.Text.Encoding.GetEncoding(1251));

			int bankIndex = Array.FindIndex(lines, l => l.StartsWith("Bank report:"));
			int galaxyIndex = Array.FindIndex(lines, l => l.StartsWith("Galaxy report:"));
			int technologyIndex = Array.FindIndex(lines, l => l == "Technology reports:");

			Assert.That(technologyIndex, Is.GreaterThanOrEqualTo(0), "technology report present");
			Assert.That(technologyIndex, Is.GreaterThan(bankIndex), "technology report after the bank report");
			Assert.That(technologyIndex, Is.LessThan(galaxyIndex), "technology report before the galaxy report");
			Assert.That(lines[technologyIndex - 1], Is.EqualTo(string.Empty), "single blank line before technology reports");
			Assert.That(lines[technologyIndex - 2], Does.StartWith("  Credit rate:"), "bank report immediately precedes technology section");
			Assert.That(lines[galaxyIndex - 1], Is.EqualTo(string.Empty), "single blank line before galaxy report");
			Assert.That(galaxyIndex, Is.GreaterThan(technologyIndex + 1), "technology report content precedes galaxy report");
		}

		[Test]
		public void FactionKnownTechnologies_PersistThroughSaveAndLoad()
		{
			Faction faction = this.game.Factions["2"];
			faction.TechnologiesSeen.Add(Technology.All["stnrdf"]);
			this.dataFile.SaveGame(Directory.GetCurrentDirectory(), "gameout.knowntech.test.xml");

			this.game.ClearDictionaries();

			DataFile reloaded = new DataFile(Directory.GetCurrentDirectory());
			reloaded.LoadConfiguration();
			reloaded.LoadGameDocument(Directory.GetCurrentDirectory(), "gameout.knowntech.test.xml");
			reloaded.LoadFactions();

			Assert.That(Faction.All["2"].TechnologiesSeen.Contains("stnrdf"));
		}

		[Test]
		public void FactionAllShown_PromotesShownTechnologiesToSeen()
		{
			Faction faction = this.game.Factions["2"];
			faction.TechnologiesToShow.Add(Technology.All["stnrdf"]);

			faction.AllShown();

			Assert.That(faction.TechnologiesToShow.Count, Is.EqualTo(0));
			Assert.That(faction.TechnologiesSeen.Contains("stnrdf"));
		}

		[Test]
		public void ResearchPoints_PersistThroughSaveAndLoad()
		{
			ModuleStack stack = this.game.ModuleStacks["100011"];
			stack.ResearchPoints = 5;
			this.dataFile.SaveGame(Directory.GetCurrentDirectory(), "gameout.rp.test.xml");

			this.game.ClearDictionaries();

			DataFile reloaded = new DataFile(Directory.GetCurrentDirectory());
			reloaded.LoadConfiguration();
			reloaded.LoadGameDocument(Directory.GetCurrentDirectory(), "gameout.rp.test.xml");
			reloaded.LoadFactions();
			reloaded.LoadGalaxy();

			Assert.That(ModuleStack.All["100011"].ResearchPoints, Is.EqualTo(5));
		}

		[Test]
		public void ResearchPoints_AppearInModuleStackReport()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack stack = this.game.ModuleStacks["100011"]; // owned by faction 2
			stack.ResearchPoints = 7;

			string report = string.Join("\n", stack.Report(faction).ToArray());
			Assert.That(report, Does.Contain("research points: 7"));
		}

		[Test]
		public void ResearchPoints_ZeroedOnBreakthroughButAccrueOtherwise()
		{
			// no breakthrough -> accrue this week's output
			ModuleStack accruing = this.createResearchLab();
			ResearchOrder accrueOrder = this.assignResearch(accruing, "research");
			Sequence.Ints.Push(5); // breakthrough roll != 0
			accrueOrder.Execute(this.game.Week);
			Assert.That(accruing.ResearchPoints, Is.EqualTo(1));

			// breakthrough -> research points reset to zero
			accruing.ResearchPoints = 9;
			Sequence.Ints.Push(1); // technology selection
			Sequence.Ints.Push(0); // breakthrough roll == 0
			accrueOrder.Execute(this.game.Week);
			Assert.That(accruing.ResearchPoints, Is.EqualTo(0));
		}

		[Test]
		public void Parse_StackId_IsModuleStackResearch()
		{
			ModuleStack wreck = new ModuleStack(Region.All["R00002"], Faction.All["1"], ModuleType.All["alnhul"], "200");
			wreck.AddModule();
			Assert.That(this.parseResearch("200"), Is.EqualTo(EResearchType.ModuleStack));
		}

		[Test]
		public void Execute_StackResearch_CreditsContractAndTransfersUnit()
		{
			ModuleStack wreck = new ModuleStack(Region.All["R00001"], Faction.All["1"], ModuleType.All["alnhul"], "200");
			wreck.AddModule();
			wreck.Technologies.Add(Technology.All["alndrn"]);
			ResearchWreckageTrigger trigger = new ResearchWreckageTrigger(wreck, 5);
			new Contract("CT0200", Region.All["R00001"], Faction.All["1"], trigger, wreck);

			ModuleStack lab = this.createResearchLab();
			ResearchOrder order = this.assignResearch(lab, "research 200");
			Assert.That(order.ResearchType, Is.EqualTo(EResearchType.ModuleStack));

			for (int week = 1; week <= 5; week++)
			{
				lab.ExecutedLongOrder = false;
				lab.Execute(week);
				Contract.All.Evaluate(week);
			}

			Assert.That(Contract.All.Count, Is.EqualTo(0));
			Assert.That(wreck.Owner.Name, Is.EqualTo("2"));
			Assert.That(Faction.All["2"].TechnologiesToShow.Contains("alndrn"), Is.True);
		}
	}
}
