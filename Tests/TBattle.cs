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
			Assert.That(station.Orders[0].Executed, Is.True);
		}

		[Test]
		public void CaptureOrder_ImmobileLogsOnceEvenWhenPeerStacksKeepExecuting()
		{
			ModuleStack station = this.game.ModuleStacks["100021"];
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			Assert.That(station.IsImmobile);
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 1",
				"#modulestack 100021",
				"capture all",
				"#faction 2",
				"#modulestack 100011",
				"tactic evade",
				"#end"
			});

			this.game.ExecuteOrdersByModuleStack();
			this.game.ExecuteOrdersByModuleStack();

			int fails = 0;
			foreach (EventReport eventReport in station.EventReports)
			{
				if (eventReport.Description == "CAPTURE failed. Immobile units may only use destroy.")
				{
					fails++;
				}
			}
			Assert.That(fails, Is.EqualTo(1));
			Assert.That(station.HasCapture, Is.False);
			Assert.That(frigate.HasEvade);
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
		public void CaptureShot_SplitsTwentyFivePercentHitPoints()
		{
			Assert.That(Battle.HpDamageFromShot(10), Is.EqualTo(2));
			Assert.That(Battle.CaptureDamageFromShot(10), Is.EqualTo(8));
			Assert.That(Battle.HpDamageFromShot(4), Is.EqualTo(1));
			Assert.That(Battle.CaptureDamageFromShot(4), Is.EqualTo(3));
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

		[Test]
		public void IsArmed_TrueForModuleTypesWithAttack()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack tanks = new ModuleStack(Region.All["R00002"], faction, ModuleType.All["tanks"], "tanktest");
			ModuleStack infantry = new ModuleStack(Region.All["R00002"], faction, ModuleType.All["inftry"], "inftrytest");

			Assert.That(tanks.IsArmed, Is.True);
			Assert.That(infantry.IsArmed, Is.True);
		}

		[Test]
		public void Execute_TankStack_FiresInBattle()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "tankregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack tanks = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "tankfire");
			tanks.AddModule();
			tanks.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			tanks.ApplyPrioritizeTactic("prioritize command");
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "tanktgt");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(tanks, target);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Contain("fires"));
			Assert.That(report, Does.Contain("tanks [tanks]"));
		}

		[Test]
		public void CollectDefenders_IncludesHeadquartersSiblings()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "hqregion");
			Faction gelvaren = new Faction("3", "Gelvaren");
			ModuleStack city = new ModuleStack(region, Faction.All["1"], ModuleType.All["city"], "hqcity");
			ModuleStack headquarters = new ModuleStack(city, Faction.All["2"], ModuleType.All["corphq"], "hqdef");
			headquarters.AddModule();
			ModuleStack cargoBay = new ModuleStack(city, Faction.All["2"], ModuleType.All["cargob"], "hqcargo");
			cargoBay.AddModule();
			ModuleStack attacker = new ModuleStack(region, gelvaren, ModuleType.All["tanks"], "hqatk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));

			gelvaren.Attitudes["2"] = FactionAttitude.Enemy;

			Battle battle = new Battle(attacker, headquarters);

			Assert.That(battle.Defenders.Contains(headquarters.Name), Is.True);
			Assert.That(battle.Defenders.Contains(cargoBay.Name), Is.True);
		}

		[Test]
		public void StartAtLocations_NestedEnemyStackCanBeDefender()
		{
			Faction gelvaren = new Faction("3", "Gelvaren");
			Region region = Region.All["R00002"];
			ModuleStack city = new ModuleStack(region, Faction.All["1"], ModuleType.All["city"], "citytest");
			ModuleStack attacker = new ModuleStack(region, gelvaren, ModuleType.All["tanks"], "atktest");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			ModuleStack nestedDefender = new ModuleStack(city, Faction.All["2"], ModuleType.All["corphq"], "deftest");
			nestedDefender.AddModule();

			gelvaren.Attitudes["2"] = FactionAttitude.Enemy;

			Battle.All.Clear();
			List<Battle> started = Battle.StartAtLocations(1);

			Assert.That(started.Count, Is.GreaterThan(0));
			bool foundNestedDefender = false;
			foreach (Battle battle in started)
			{
				foreach (ModuleStack defender in battle.Defenders.Values)
				{
					if (!defender.IsRootModuleStack && defender.Owner == Faction.All["2"])
					{
						foundNestedDefender = true;
						break;
					}
				}
				if (foundNestedDefender)
				{
					break;
				}
			}
			Assert.That(foundNestedDefender, Is.True);
		}

		[Test]
		public void CaptureCommandModule_DoesNotCaptureParentWithDifferentOwner()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "capregion");
			Faction cityOwner = this.game.Factions["1"];
			Faction hqOwner = this.game.Factions["2"];
			Faction attackerOwner = new Faction("3", "Gelvaren");
			ModuleStack city = new ModuleStack(region, cityOwner, ModuleType.All["city"], "capcity");
			ModuleStack headquarters = new ModuleStack(city, hqOwner, ModuleType.All["corphq"], "caphq");
			headquarters.AddModule();
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "capatk");
			attacker.AddModule();

			Battle battle = new Battle(attacker, headquarters);
			headquarters.Modules[0].CaptureDamage = headquarters.Modules[0].HitPoints;
			Assert.That(battle.Defenders.Contains(headquarters.Name), Is.True);

			battle.ApplyCaptures(this.game.Week);

			Assert.That(city.Owner, Is.EqualTo(cityOwner));
			Assert.That(battle.Defenders.Contains(headquarters.Name), Is.False);
		}

		[Test]
		public void TacticOrder_PrioritizeArmedCoexistsWithDestroy()
		{
			ModuleStack tanks = this.game.ModuleStacks["100011"];
			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"tactic destroy",
				"tactic prioritize armed",
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			tanks.Orders[0].Execute(this.game.Week);
			tanks.Orders[1].Execute(this.game.Week);

			Assert.That(tanks.Tactics.ContainsName("destroy"));
			Assert.That(tanks.HasPrioritizeArmed);
		}

		[Test]
		public void Execute_PrioritizeArmed_TargetsDisabledGunPlacement()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "prioregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "prioatk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			attacker.ApplyTactic("destroy");
			attacker.ApplyPrioritizeTactic("prioritize armed");
			ModuleStack headquarters = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "priohq");
			headquarters.AddModule();
			headquarters.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));
			ModuleStack guns = new ModuleStack(region, defenderOwner, ModuleType.All["gunplc"], "priogun");
			guns.AddModule();
			Assert.That(guns.IsActive, Is.False);
			Assert.That(guns.IsArmed, Is.True);

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(attacker, headquarters);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Contain("on gun placement [priogun]"));
		}

		[Test]
		public void Execute_PrioritizeCommand_TargetsHeadquarters()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "cmdregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "cmdatk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			attacker.ApplyTactic("capture");
			attacker.ApplyPrioritizeTactic("prioritize command");
			ModuleStack headquarters = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "cmdhq");
			headquarters.AddModule();
			ModuleStack guns = new ModuleStack(region, defenderOwner, ModuleType.All["gunplc"], "cmdgun");
			guns.AddModule();

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(attacker, headquarters);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Contain("cmdhq"));
		}

		[Test]
		public void TacticOrder_PrioritizeCargoCoexistsWithCapture()
		{
			ModuleStack tanks = this.game.ModuleStacks["100011"];
			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"tactic capture",
				"tactic prioritize storage",
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			tanks.Orders[0].Execute(this.game.Week);
			tanks.Orders[1].Execute(this.game.Week);

			Assert.That(tanks.Tactics.ContainsName("capture"));
			Assert.That(tanks.HasPrioritizeCargo);
			Assert.That(tanks.Tactics.ContainsName("prioritize storage"));
		}

		[Test]
		public void Execute_PrioritizeStorage_TargetsCargoBay()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "cargoregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "cargoatk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			attacker.ApplyTactic("capture");
			attacker.ApplyPrioritizeTactic("prioritize storage");
			ModuleStack headquarters = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "cargohq");
			headquarters.AddModule();
			ModuleStack guns = new ModuleStack(region, defenderOwner, ModuleType.All["gunplc"], "cargogun");
			guns.AddModule();
			guns.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 2));
			ModuleStack cargo = new ModuleStack(region, defenderOwner, ModuleType.All["cargob"], "cargobay");
			cargo.AddModule();

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(attacker, headquarters);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Contain("on small cargo bay [cargobay]"));
		}

		[Test]
		public void Execute_HangarCraft_LaunchesFromBayAndDoesNotFireRoundOne()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "hangarregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "hangaratk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			attacker.ApplyPrioritizeTactic("prioritize command");
			ModuleStack headquarters = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "hangarhq");
			headquarters.AddModule();
			ModuleStack bay = new ModuleStack(headquarters, defenderOwner, ModuleType.All["drnbay"], "hangarbay");
			bay.AddModule();
			ModuleStack drones = new ModuleStack(bay, defenderOwner, ModuleType.All["alndrn"], "hangardrn");
			drones.AddModule();
			drones.ItemStacks.Add(new ItemStack(ItemType.All["heliu3"], 1));

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(attacker, headquarters);
			battle.Execute(this.game.Week);

			Assert.That(drones.Parent, Is.EqualTo(region));
			string report = string.Join("\n", battle.Report(defenderOwner).ToArray());
			int roundOne = report.IndexOf("Round 1:", StringComparison.Ordinal);
			int roundTwo = report.IndexOf("Round 2:", StringComparison.Ordinal);
			Assert.That(roundOne, Is.GreaterThanOrEqualTo(0));
			Assert.That(roundTwo, Is.GreaterThan(roundOne));
			string beforeRounds = report.Substring(0, roundOne);
			string roundOneText = report.Substring(roundOne, roundTwo - roundOne);
			Assert.That(beforeRounds, Does.Not.Contain("launches"));
			Assert.That(roundOneText, Does.Contain("hangarhq] launches alien fighter drone [hangardrn] from fighter drone bay [drnbay]"));
			Assert.That(roundOneText, Does.Not.Contain("hangardrn] fires"));
			Assert.That(report, Does.Not.Contain("fires fighter drone bay"));
			Assert.That(report.Substring(roundTwo), Does.Contain("hangardrn] fires"));
		}

		[Test]
		public void SetOrder_OnlineTrue_ActivatesDeactivatedModules()
		{
			ModuleStack tanks = this.game.ModuleStacks["100011"];
			if (tanks.Quantity < 1)
			{
				tanks.AddModule();
			}
			tanks.Modules[0].Online = false;
			Assert.That(tanks.Modules[0].IsActive, Is.False);

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"set online true",
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			tanks.Orders[0].Execute(this.game.Week);

			Assert.That(tanks.Modules[0].Online);
			Assert.That(tanks.Online);
		}

		[Test]
		public void Execute_ImmobileTarget_HasHigherHitChance()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "immregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "immatk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			attacker.ApplyPrioritizeTactic("prioritize command");
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "immtgt");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(attacker, target);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Match(@"chance: \d+/9\)"));
			Assert.That(report, Does.Contain("(chance: 3/9)"));
		}

		private void disableByHeavyDamage(ModuleStack stack)
		{
			stack.Modules[0].Damage = (stack.Modules[0].HitPoints / 2) + 1;
			Assert.That(stack.IsArmed, Is.True);
			Assert.That(stack.HasOperationalModules, Is.False);
		}

		[Test]
		public void CollectAttackers_DisabledStack_IsNotIncluded()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "disatkregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack active = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "disatkactive");
			active.AddModule();
			active.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			ModuleStack disabled = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "disatkdisabled");
			disabled.AddModule();
			disabled.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			this.disableByHeavyDamage(disabled);
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "disatktgt");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			Battle battle = new Battle(active, target);

			Assert.That(battle.Attackers.Contains(active.Name), Is.True);
			Assert.That(battle.Attackers.Contains(disabled.Name), Is.False);
		}

		[Test]
		public void CollectAttackers_DisabledInitiator_IsNotIncluded()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "disinitregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack disabled = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "disinit");
			disabled.AddModule();
			disabled.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			this.disableByHeavyDamage(disabled);
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "disinittgt");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			Battle battle = new Battle(disabled, target);

			Assert.That(battle.Attackers.Contains(disabled.Name), Is.False);
		}

		[Test]
		public void StartAtLocations_DisabledArmedStack_DoesNotInitiateBattle()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "disstartregion");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack disabled = new ModuleStack(region, attackerOwner, ModuleType.All["tanks"], "disstart");
			disabled.AddModule();
			disabled.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			this.disableByHeavyDamage(disabled);
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "disstarttgt");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));
			attackerOwner.Attitudes["1"] = FactionAttitude.Enemy;

			Battle.All.Clear();
			List<Battle> started = Battle.StartAtLocations(1);

			foreach (Battle battle in started)
			{
				Assert.That(battle.Attacker.Name, Is.Not.EqualTo(disabled.Name));
				Assert.That(battle.Attackers.Contains(disabled.Name), Is.False);
			}
		}

		[Test]
		public void IsImmobile_NestedModuleOnMobileHull_FollowsRoot()
		{
			ModuleStack hull = this.game.ModuleStacks["100011"];
			ModuleStack bridge = this.game.ModuleStacks["100012"];
			Assert.That(hull.IsImmobile, Is.False);
			Assert.That(bridge.IsImmobile, Is.False);
		}

		[Test]
		public void IsImmobile_NestedModuleOnCity_IsTrue()
		{
			ModuleStack city = this.game.ModuleStacks["000001"];
			ModuleStack factory = this.game.ModuleStacks["000004"];
			Assert.That(city.IsImmobile, Is.True);
			Assert.That(factory.IsImmobile, Is.True);
		}

		[Test]
		public void IsImmobile_StationAndNestedBridge_AreImmobile()
		{
			ModuleStack station = this.game.ModuleStacks["100021"];
			ModuleStack bridge = this.game.ModuleStacks["100022"];
			Assert.That(station.IsImmobile, Is.True);
			Assert.That(bridge.IsImmobile, Is.True);
		}

		[Test]
		public void IsImmobile_TanksOutOfFuelOrDisabled()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack fueled = new ModuleStack(region, owner, ModuleType.All["tanks"], "immobfuel");
			fueled.AddModule();
			fueled.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			fueled.ItemStacks.Add(new ItemStack(ItemType.All["oil"], 4));
			Assert.That(fueled.IsImmobile, Is.False);

			ModuleStack dry = new ModuleStack(region, owner, ModuleType.All["tanks"], "immobdry");
			dry.AddModule();
			dry.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			Assert.That(dry.IsImmobile, Is.True);

			ModuleStack wrecked = new ModuleStack(region, owner, ModuleType.All["tanks"], "immobdmg");
			wrecked.AddModule();
			wrecked.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			wrecked.ItemStacks.Add(new ItemStack(ItemType.All["oil"], 4));
			this.disableByHeavyDamage(wrecked);
			Assert.That(wrecked.IsImmobile, Is.True);
		}

		[Test]
		public void IsImmobile_LaunchedDroneWithFuel_IsFalse()
		{
			ModuleStack drone = new ModuleStack(
				Orbit.All["O00003"],
				this.game.Factions["2"],
				ModuleType.All["alndrn"],
				"immobdrone");
			drone.AddModule();
			drone.ItemStacks.Add(new ItemStack(ItemType.All["heliu3"], 1));
			Assert.That(drone.IsImmobile, Is.False);
		}

		[Test]
		public void IsImmobile_LaunchedDroneOutOfFuel_IsTrue()
		{
			ModuleStack drone = new ModuleStack(
				Orbit.All["O00003"],
				this.game.Factions["2"],
				ModuleType.All["alndrn"],
				"immobdrydrone");
			drone.AddModule();
			Assert.That(drone.IsImmobile, Is.True);
		}

		[Test]
		public void CollectDefenders_NestedWeaponOnSameRoot_IsNotASeparateCombatant()
		{
			Orbit orbit = Orbit.All["O00003"];
			Faction dronesOwner = this.game.Factions["2"];
			Faction shuttleOwner = this.game.Factions["1"];
			shuttleOwner.Attitudes["2"] = FactionAttitude.Enemy;
			dronesOwner.Attitudes["1"] = FactionAttitude.Enemy;

			ModuleStack shuttle = new ModuleStack(orbit, shuttleOwner, ModuleType.All["shuttl"], "rptshut");
			shuttle.AddModule();
			shuttle.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 2));
			shuttle.ItemStacks.Add(new ItemStack(ItemType.All["uraniu"], 1));
			shuttle.ItemStacks.Add(new ItemStack(ItemType.All["h2o2"], 1));
			ModuleStack launcher = new ModuleStack(shuttle, shuttleOwner, ModuleType.All["orbrkt"], "rptlnch");
			launcher.AddModule();
			ModuleStack drones = new ModuleStack(orbit, dronesOwner, ModuleType.All["alndrn"], "rptdrn");
			drones.AddModule();
			drones.ItemStacks.Add(new ItemStack(ItemType.All["heliu3"], 1));

			Battle battle = new Battle(drones, shuttle);

			Assert.That(battle.Defenders.Contains("rptshut"), Is.True);
			Assert.That(battle.Defenders.Contains("rptlnch"), Is.False, "nested launcher is listed under the shuttle, not twice");
			string roster = string.Join("\n", shuttle.BattleReport(dronesOwner).ToArray());
			Assert.That(roster, Does.Contain("rptlnch"));
		}

		[Test]
		public void BattlesReport_BlankLineBetweenBattles()
		{
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];
			Battle first = new Battle(frigate, station);
			first.Week = 1;
			Battle second = new Battle(frigate, station);
			second.Week = 8;

			Battles battles = new Battles();
			battles.Add(first);
			battles.Add(second);
			List<string> lines = battles.Report(frigate.Owner);

			int weekEight = -1;
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].IndexOf("Week 8.") >= 0)
				{
					weekEight = i;
					break;
				}
			}
			Assert.That(weekEight, Is.GreaterThan(0));
			Assert.That(lines[weekEight - 1], Is.EqualTo(""), "blank line between consecutive battles");
		}

	}
}
