using System;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TItemCombat : TTest
	{
		[SetUp]
		public void setup()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void ModuleStack_Attack_IncludesEligibleItemAttack()
		{
			ModuleStack infantry = this.infantryStack("atk001");
			infantry.ItemStacks.Add(new ItemStack(ItemType.All["rctlnc"], 1));

			Assert.That(infantry.Attack, Is.EqualTo(3));
		}

		[Test]
		public void ModuleStack_Defense_IncludesEligibleItemDefense()
		{
			ModuleStack infantry = this.infantryStack("def001");
			infantry.ItemStacks.Add(new ItemStack(ItemType.All["psnarm"], 1));

			Assert.That(infantry.Defense, Is.EqualTo(4));
		}

		[Test]
		public void ModuleStack_ModuleShotDamage_IsModuleTypeOnly()
		{
			ModuleStack infantry = this.infantryStack("dmg001");
			infantry.ItemStacks.Add(new ItemStack(ItemType.All["rctlnc"], 1));

			Assert.That(infantry.ModuleShotDamage(), Is.EqualTo(1));
		}

		[Test]
		public void ItemCombat_IgnoresIneligibleGroup()
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ModuleStack tanks = new ModuleStack(region, faction, ModuleType.All["tanks"], "tankitm");
			tanks.AddModule();
			tanks.ItemStacks.Add(new ItemStack(ItemType.All["rctlnc"], 1));

			Assert.That(tanks.Attack, Is.EqualTo(4));
			Assert.That(tanks.ModuleShotDamage(), Is.EqualTo(4));
		}

		[Test]
		public void ItemCombat_CapsAtModuleCount()
		{
			ModuleStack infantry = this.infantryStack("cap001");
			infantry.AddModule();
			infantry.AddModule();
			infantry.ItemStacks.Add(new ItemStack(ItemType.All["rctlnc"], 5));

			Assert.That(infantry.Attack, Is.EqualTo(9));
			Assert.That(
				infantry.ItemStacks.CombatDamageShotBudget(EModuleTypesGroup.infantry, infantry.QuantityActive),
				Is.EqualTo(3));
		}

		[Test]
		public void ItemCombat_ShotDamageBonusDepletesWithShots()
		{
			ItemStacks items = new ItemStacks();
			items.Add(new ItemStack(ItemType.All["rctlnc"], 2));
			int remaining = items.CombatDamageShotBudget(EModuleTypesGroup.infantry, 3);

			Assert.That(remaining, Is.EqualTo(2));
			Assert.That(items.CombatDamageBonusForShot(EModuleTypesGroup.infantry, ref remaining), Is.EqualTo(2));
			Assert.That(remaining, Is.EqualTo(1));
			Assert.That(items.CombatDamageBonusForShot(EModuleTypesGroup.infantry, ref remaining), Is.EqualTo(2));
			Assert.That(remaining, Is.EqualTo(0));
			Assert.That(items.CombatDamageBonusForShot(EModuleTypesGroup.infantry, ref remaining), Is.EqualTo(0));
		}

		[Test]
		public void Battle_ItemDamage_AddsToShotDamage()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "itmbtl");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["inftry"], "itmatk");
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["rctlnc"], 1));
			attacker.ApplyPrioritizeTactic("prioritize command");
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "itmtgt");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);

			Battle battle = new Battle(attacker, target);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Contain("doing 3 damage"));
		}

		[Test]
		public void Battle_ItemDamage_OnlyFirstShotsUntilItemCount()
		{
			Region region = new Region(Region.All["R00002"].RegionHolder, "itmbtl2");
			Faction attackerOwner = this.game.Factions["2"];
			Faction defenderOwner = this.game.Factions["1"];
			ModuleStack attacker = new ModuleStack(region, attackerOwner, ModuleType.All["inftry"], "itmatk2");
			attacker.AddModule();
			attacker.AddModule();
			attacker.AddModule();
			attacker.ItemStacks.Add(new ItemStack(ItemType.All["rctlnc"], 2));
			attacker.ApplyPrioritizeTactic("prioritize command");
			ModuleStack target = new ModuleStack(region, defenderOwner, ModuleType.All["corphq"], "itmtgt2");
			target.AddModule();
			target.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			for (int i = 0; i < 6; i++)
			{
				Sequence.Ints.Push(1);
			}

			Battle battle = new Battle(attacker, target);
			battle.Execute(this.game.Week);

			string report = string.Join("\n", battle.Report(attackerOwner).ToArray());
			Assert.That(report, Does.Contain("doing 3 damage"));
			Assert.That(report, Does.Contain("doing 1 damage"));
		}

		private ModuleStack infantryStack(string name)
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ModuleStack infantry = new ModuleStack(region, faction, ModuleType.All["inftry"], name);
			infantry.AddModule();
			return infantry;
		}
	}

}
