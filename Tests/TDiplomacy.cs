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
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardownDiplomacy()
		{
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
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
	}
}
