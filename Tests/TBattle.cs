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

		[Test]
		public void TacticOrder_SetsDestroyAndIsExclusiveWithCapture()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "tactic destroy",
                "tactic capture",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			frigate.Orders[0].Execute(this.game.Week);
			Assert.That(frigate.Tactics.ContainsName("destroy"));
			Assert.That(frigate.Tactics.ContainsName("capture"), Is.False);

			frigate.Orders[1].Execute(this.game.Week);
			Assert.That(frigate.HasCapture);
			Assert.That(frigate.FiringTactic, Is.EqualTo(ETactic.capture));
			Assert.That(frigate.Tactics.ContainsName("destroy"), Is.False);
		}

		[Test]
		public void TacticOrder_EvadeCoexistsWithDestroy()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "tactic destroy",
                "tactic evade",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			frigate.Orders[0].Execute(this.game.Week);
			frigate.Orders[1].Execute(this.game.Week);

			Assert.That(frigate.FiringTactic, Is.EqualTo(ETactic.destroy));
			Assert.That(frigate.HasEvade);
		}

		[Test]
		public void TacticOrder_ImmobileRejectsCapture()
		{
			ModuleStack station = this.game.ModuleStacks["100021"];
			Assert.That(station.IsImmobile);
			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 100021",
                "tactic capture",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			station.Orders[0].Execute(this.game.Week);

			Assert.That(station.HasCapture, Is.False);
			Assert.That(station.Orders[0].Executed, Is.False);
		}

		[Test]
		public void CaptureOrder_SetsCaptureTacticAndPreferredTarget()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "capture 100021",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			CaptureOrder order = (CaptureOrder)frigate.Orders[0];
			Assert.That(order.TargetName, Is.EqualTo("100021"));
			order.Execute(this.game.Week);

			Assert.That(frigate.HasCapture);
			Assert.That(frigate.PreferredTargetName, Is.EqualTo("100021"));
			Assert.That(frigate.Owner.AttitudeTowardUnit(this.game.ModuleStacks["100021"]), Is.EqualTo(FactionAttitude.Enemy));
		}

		[Test]
		public void CaptureOrder_AllSetsCaptureWithoutSinglePreference()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "capture all",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			frigate.Orders[0].Execute(this.game.Week);

			Assert.That(frigate.HasCapture);
			Assert.That(frigate.PreferredTargetName, Is.EqualTo("all"));
		}

		[Test]
		public void CaptureShot_SplitsTenPercentHitPoints()
		{
			Assert.That(Battle.HpDamageFromShot(10), Is.EqualTo(1));
			Assert.That(Battle.CaptureDamageFromShot(10), Is.EqualTo(9));
			Assert.That(Battle.HpDamageFromShot(1), Is.EqualTo(0));
			Assert.That(Battle.CaptureDamageFromShot(1), Is.EqualTo(1));
		}

		[Test]
		public void Module_CaptureCompletesBeforeWreck()
		{
			ModuleStack station = this.game.ModuleStacks["100021"];
			Module hull = station.Modules[0];
			hull.Damage = 0;
			hull.CaptureDamage = hull.HitPoints;
			Assert.That(hull.IsCaptureComplete);
			Assert.That(hull.IsWrecked, Is.False);

			hull.Damage = hull.HitPoints;
			Assert.That(hull.IsWrecked);
			Assert.That(hull.IsCaptureComplete, Is.False);
		}

		[Test]
		public void ExecuteBattles_OneWayEnemyCoLocation_StartsBattle()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			ModuleStack station = this.game.ModuleStacks["100021"];
			frigate.Owner.UnitAttitudes[station.Name] = FactionAttitude.Enemy;

			int before = this.game.Battles.Count;
			this.game.Week = 1;
			this.game.ExecuteBattles();

			Assert.That(this.game.Battles.Count, Is.GreaterThan(before));
			Assert.That(frigate.Owner.EventReports.Count, Is.GreaterThan(0));
		}

		[Test]
		public void DefaultFiringTactic_IsDestroy()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			frigate.Tactics.Clear();
			Assert.That(frigate.FiringTactic, Is.EqualTo(ETactic.destroy));
		}

		[Test]
		public void Execute_DoesNotExceedTenRounds()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			ModuleStack station = this.game.ModuleStacks["100021"];
			frigate.ApplyTactic("capture");

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			for (int i = 0; i < 30; i++)
			{
				Sequence.Ints.Push(250);
				Sequence.Ints.Push(1);
			}
			Sequence.Ints.Push(13);

			Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			Assert.That(battle.Round, Is.LessThanOrEqualTo(Battle.MaxRounds));
			Assert.That(battle.Round, Is.EqualTo(Battle.MaxRounds));

			string report = string.Join("\n", this.game.Battles.Report(frigate.Owner).ToArray());
			Assert.That(report, Does.Not.Contain("Round 11"));
			Assert.That(report, Does.Contain("Round 10:"));
		}

		[Test]
		public void Execute_ZerosCaptureDamageAtBattleStart()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			ModuleStack station = this.game.ModuleStacks["100021"];
			ModuleStack reactor = this.game.ModuleStacks["100023"];
			ModuleStack cargo = this.game.ModuleStacks["100024"];

			station.Modules[0].CaptureDamage = 12;
			reactor.Modules[0].CaptureDamage = 8;
			cargo.Modules[0].CaptureDamage = 20;
			cargo.Modules[1].CaptureDamage = 5;
			frigate.Modules[0].CaptureDamage = 3;

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			for (int i = 0; i < 40; i++)
			{
				Sequence.Ints.Push(13);
			}

			Battle battle = new Battle(frigate, station);
			Assert.That(station.Modules[0].CaptureDamage, Is.EqualTo(12));
			Assert.That(reactor.Modules[0].CaptureDamage, Is.EqualTo(8));

			battle.Execute(this.game.Week);

			Assert.That(station.Modules[0].CaptureDamage, Is.EqualTo(0));
			Assert.That(reactor.Modules[0].CaptureDamage, Is.EqualTo(0));
			Assert.That(cargo.Modules[0].CaptureDamage, Is.EqualTo(0));
			Assert.That(cargo.Modules[1].CaptureDamage, Is.EqualTo(0));
			Assert.That(frigate.Modules[0].CaptureDamage, Is.EqualTo(0));
		}
	}
}
