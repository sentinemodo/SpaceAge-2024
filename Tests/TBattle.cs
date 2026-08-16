using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBattle
	{
		private DataFile datafile;
		private Game game;

		public TBattle()
		{			
		}
			
		[SetUp]
		public void setupReport()
		{
			this.datafile = new DataFile(Directory.GetCurrentDirectory());
			this.datafile.LoadConfiguration();
			this.datafile.LoadGame();
			this.game = this.datafile.Game;
		}

		[TearDown]
		public void teardownReport()
		{
			this.game.Week = 1;
			this.game.ClearDictionaries();
			this.game = null;
			this.datafile = null;			
		}

		[Test]
		public void SetupTeardown()
		{
            Assert.That(true);
		}


		[Test]
		public void FindAllies_singleModulestack()
		{
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];
			Battle battle = new Battle(frigate, station);
            Assert.That(battle.Attackers.Count, Is.EqualTo(1));
            Assert.That(battle.Attackers.Contains("100011"));
            Assert.That(battle.Defenders.Count, Is.EqualTo(1));
            Assert.That(battle.Defenders.Contains("100021"));
		}

		[Test]
		public void AttackOrder_DeclaresTargetUnitEnemy()
		{
			ModuleStack attacker = this.game.ModuleStacks["100011"]; // faction 2
			ModuleStack target = this.game.ModuleStacks["100021"];   // faction 1

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "attack 100021",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			AttackOrder order = (AttackOrder)attacker.Orders[0];
			Assert.That(order.TargetName, Is.EqualTo("100021"));

			order.Execute(this.game.Week);

			Faction faction = this.game.Factions["2"];
			Assert.That(faction.UnitAttitudes.ContainsKey("100021"));
			Assert.That(faction.UnitAttitudes["100021"], Is.EqualTo(FactionAttitude.Enemy));
			// a one-way enemy declaration toward the specific unit
			Assert.That(faction.AttitudeTowardUnit(target), Is.EqualTo(FactionAttitude.Enemy));
		}
	}
}
