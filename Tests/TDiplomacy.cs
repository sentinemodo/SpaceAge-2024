using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TDiplomacy : TTest
	{
		[SetUp]
		public void setupDiplomacy()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownDiplomacy()
		{
			this.ClearGame();
		}

		[Test]
		public void DeclarationReport_MatchesExpected()
		{
			Faction faction = this.game.Factions["2"];
			faction.Attitudes["1"] = FactionAttitude.Enemy;            // toward the NPC faction
			faction.UnitAttitudes["100021"] = FactionAttitude.Hostile; // toward a specific unit

			List<string> expected = new List<string>
            {
                "Declared stances:",
                "  default: neutral, unknown: hostile.",
                "  enemy toward NPC [1].",
                "  hostile toward Station [100021]."
            };

			List<string> actual = faction.ReportDeclarations();
			Assert.That(actual.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < expected.Count; i++)
			{
				Assert.That(actual[i], Is.EqualTo(expected[i]));
			}
		}

		[Test]
		public void DeclarationReport_AppearsBeforeBankReport()
		{
			Faction faction = this.game.Factions["2"];
			faction.Attitudes["1"] = FactionAttitude.Enemy;

			List<string> report = faction.Report();
			int declarationsIndex = report.FindIndex(l => l == "Declared stances:");
			int bankIndex = report.FindIndex(l => l.StartsWith("Bank report:"));

			Assert.That(declarationsIndex, Is.GreaterThanOrEqualTo(0), "declarations present");
			Assert.That(bankIndex, Is.GreaterThan(declarationsIndex), "declarations before the bank report");
		}

		[Test]
		public void DeclarationReport_EmptyAtBaseline()
		{
			// baseline stances (neutral default, hostile unknown, no declarations) render nothing,
			// so existing games' reports are unchanged.
			Faction faction = this.game.Factions["2"];
			Assert.That(faction.ReportDeclarations().Count, Is.EqualTo(0));
		}

		[Test]
		public void BaselineStances_NeutralDefaultHostileUnknown()
		{
			Faction faction = this.game.Factions["2"];
			Assert.That(faction.DefaultAttitude, Is.EqualTo(FactionAttitude.Neutral));
			Assert.That(faction.UnknownAttitude, Is.EqualTo(FactionAttitude.Hostile));
		}

		[Test]
		public void AttitudeToward_UsesDeclarationElseDefaultElseUnknown()
		{
			Faction faction = this.game.Factions["2"];
			Faction other = this.game.Factions["1"];

			// no declaration yet -> default stance
			Assert.That(faction.AttitudeToward(other), Is.EqualTo(FactionAttitude.Neutral));

			// explicit declaration wins
			faction.Attitudes[other.Name] = FactionAttitude.Enemy;
			Assert.That(faction.AttitudeToward(other), Is.EqualTo(FactionAttitude.Enemy));

			// unknown affiliation -> unknown stance
			Assert.That(faction.AttitudeToward(null), Is.EqualTo(FactionAttitude.Hostile));
		}

		[Test]
		public void AttitudeTowardUnit_FallsBackToOwnerThenDefault()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack enemyUnit = this.game.ModuleStacks["100021"]; // owned by faction 1

			// no per-unit or per-faction declaration -> default stance toward the owner
			Assert.That(faction.AttitudeTowardUnit(enemyUnit), Is.EqualTo(FactionAttitude.Neutral));

			// per-faction declaration applies to the owner's units
			faction.Attitudes[enemyUnit.Owner.Name] = FactionAttitude.Hostile;
			Assert.That(faction.AttitudeTowardUnit(enemyUnit), Is.EqualTo(FactionAttitude.Hostile));

			// per-unit declaration overrides the per-faction stance
			faction.UnitAttitudes[enemyUnit.Name] = FactionAttitude.Enemy;
			Assert.That(faction.AttitudeTowardUnit(enemyUnit), Is.EqualTo(FactionAttitude.Enemy));
		}

		[Test]
		public void DeclareOrder_ParseAndExecuteFactionUnitDefaultUnknown()
		{
			ModuleStack unit = this.game.ModuleStacks["100011"];
			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"declare faction 1 enemy",
				"declare unit 100021 hostile",
				"declare default friendly",
				"declare unknown ally",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			DeclareOrder factionOrder = (DeclareOrder)unit.Orders[0];
			Assert.That(factionOrder.TargetKind, Is.EqualTo(DeclareOrder.TargetFaction));
			Assert.That(factionOrder.TargetName, Is.EqualTo("1"));
			Assert.That(factionOrder.Attitude, Is.EqualTo(FactionAttitude.Enemy));

			DeclareOrder unitOrder = (DeclareOrder)unit.Orders[1];
			Assert.That(unitOrder.TargetKind, Is.EqualTo(DeclareOrder.TargetUnit));
			Assert.That(unitOrder.TargetName, Is.EqualTo("100021"));
			Assert.That(unitOrder.Attitude, Is.EqualTo(FactionAttitude.Hostile));

			DeclareOrder defaultOrder = (DeclareOrder)unit.Orders[2];
			Assert.That(defaultOrder.TargetKind, Is.EqualTo(DeclareOrder.TargetDefault));
			Assert.That(defaultOrder.Attitude, Is.EqualTo(FactionAttitude.Friendly));

			DeclareOrder unknownOrder = (DeclareOrder)unit.Orders[3];
			Assert.That(unknownOrder.TargetKind, Is.EqualTo(DeclareOrder.TargetUnknown));
			Assert.That(unknownOrder.Attitude, Is.EqualTo(FactionAttitude.Ally));

			foreach (Order order in unit.Orders)
			{
				order.Execute(this.game.Week);
			}

			Faction faction = this.game.Factions["2"];
			Assert.That(faction.Attitudes["1"], Is.EqualTo(FactionAttitude.Enemy));
			Assert.That(faction.AttitudeToward(this.game.Factions["1"]), Is.EqualTo(FactionAttitude.Enemy));
			Assert.That(faction.UnitAttitudes["100021"], Is.EqualTo(FactionAttitude.Hostile));
			Assert.That(faction.DefaultAttitude, Is.EqualTo(FactionAttitude.Friendly));
			Assert.That(faction.UnknownAttitude, Is.EqualTo(FactionAttitude.Ally));
		}

		[Test]
		public void DeclareOrder_SampleSyntaxDeclareFactionEnemy()
		{
			ModuleStack unit = this.game.ModuleStacks["100011"];
			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"see 100021",
				"-declare faction 1 enemy",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			DeclareOrder order = (DeclareOrder)unit.Orders[1];
			Assert.That(order.TargetKind, Is.EqualTo("faction"));
			Assert.That(order.TargetName, Is.EqualTo("1"));
			Assert.That(order.Attitude, Is.EqualTo(FactionAttitude.Enemy));
			Assert.That(order.Report(unit.Owner)[0], Is.EqualTo("-declare faction 1 enemy"));

			order.Execute(this.game.Week);
			Assert.That(unit.Owner.Attitudes["1"], Is.EqualTo(FactionAttitude.Enemy));
		}

		[Test]
		public void DeclareOrder_RejectsUnknownAttitude()
		{
			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"declare faction 1 war",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			Assert.Throws<Exception>(() => reader.AssignOrders(commands));
		}

		[Test]
		public void DeclareOrder_AttitudesPersistThroughSaveAndLoad()
		{
			Faction faction = this.game.Factions["2"];
			faction.Attitudes["1"] = FactionAttitude.Enemy;
			faction.UnitAttitudes["100021"] = FactionAttitude.Hostile;
			faction.UnknownAttitude = FactionAttitude.Enemy;

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.declare.test.xml";
			this.dataFile.SaveGame(testdir, testfile);

			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;

			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.game = this.dataFile.Game;

			faction = Faction.All["2"];
			Assert.That(faction.Attitudes["1"], Is.EqualTo(FactionAttitude.Enemy));
			Assert.That(faction.UnitAttitudes["100021"], Is.EqualTo(FactionAttitude.Hostile));
			Assert.That(faction.UnknownAttitude, Is.EqualTo(FactionAttitude.Enemy));
		}

		[Test]
		public void DropStaleUnitAttitudes_RemovesMissingEmptyAndOwnStacks()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack station = this.game.ModuleStacks["100021"];
			faction.UnitAttitudes[station.Name] = FactionAttitude.Enemy;
			faction.UnitAttitudes["gone-unit"] = FactionAttitude.Hostile;

			while (station.Quantity > 0)
			{
				station.RemoveModule(0);
			}
			Assert.That(station.Quantity, Is.EqualTo(0));

			ModuleStack ownUnit = this.game.ModuleStacks["100011"];
			faction.UnitAttitudes[ownUnit.Name] = FactionAttitude.Enemy;

			faction.DropStaleUnitAttitudes();

			Assert.That(faction.UnitAttitudes.ContainsKey(station.Name), Is.False);
			Assert.That(faction.UnitAttitudes.ContainsKey("gone-unit"), Is.False);
			Assert.That(faction.UnitAttitudes.ContainsKey(ownUnit.Name), Is.False);
		}

		[Test]
		public void DeclarationReport_OmitsStaleUnitStance()
		{
			Faction faction = this.game.Factions["2"];
			faction.Attitudes["1"] = FactionAttitude.Enemy;
			faction.UnitAttitudes["100021"] = FactionAttitude.Hostile;
			ModuleStack station = this.game.ModuleStacks["100021"];
			while (station.Quantity > 0)
			{
				station.RemoveModule(0);
			}

			this.game.DropStaleUnitAttitudes();
			List<string> actual = faction.ReportDeclarations();

			Assert.That(actual, Does.Contain("  enemy toward NPC [1]."));
			Assert.That(string.Join("\n", actual.ToArray()), Does.Not.Contain("100021"));
		}
	}
}
