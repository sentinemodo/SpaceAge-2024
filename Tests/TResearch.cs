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
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardownResearch()
		{
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
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
	}
}
