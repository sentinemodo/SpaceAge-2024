using NUnit.Framework;
using SpaceAge;
using System.Xml;

namespace UnitTests
{
	[TestFixture]
	public class TConsume : TTest
	{
		[SetUp]
		public void Setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void Teardown()
		{
			Sequence.Ints.Clear();
			this.ClearGame();
		}

		[Test]
		public void LoadConfiguration_WoundedNeedMedicinesOrDie()
		{
			Race wounded = Race.All["wndtrn"];
			Assert.That(wounded.Consume.ContainsKey(ItemType.All["medici"]));
			Assert.That(wounded.NoConsumeEffect, Is.EqualTo("death"));
			Assert.That(wounded.NoConsumeChance, Is.EqualTo(25));
		}

		[Test]
		public void ExecuteMedicalConsume_WithoutMedicines_DoesNotKillDuringWeek()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);

			this.game.Week = 1;
			this.game.ExecuteMedicalConsume();

			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
		}

		[Test]
		public void ExecuteMedicalConsume_WithMedicines_ConsumesMedicinesWithoutWeeklyDeath()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			stack.ItemStacks.Add(new ItemStack(ItemType.All["medici"], 2));
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);

			this.game.Week = 1;
			this.game.ExecuteMedicalConsume();

			Assert.That(stack.ItemStacks.Has(ItemType.All["medici"]), Is.False);
			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
		}

		[Test]
		public void ExecuteQuarterlyWoundedOutcome_RollsDeathStayAndRecover()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			int terranBefore = stack.ItemStacks.Quantity(ItemType.All["terran"]);
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			// LIFO: first pop dies (<25), next two stay (25-74), last recovers (>=75)
			Sequence.Ints.Push(80);
			Sequence.Ints.Push(50);
			Sequence.Ints.Push(50);
			Sequence.Ints.Push(0);

			this.game.ExecuteQuarterlyWoundedOutcome();

			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(2));
			Assert.That(stack.ItemStacks.Quantity(ItemType.All["terran"]), Is.EqualTo(terranBefore + 1));
			bool diedEvent = false;
			bool recoveredEvent = false;
			foreach (EventReport eventReport in stack.EventReports)
			{
				if (eventReport.Week == 13 && eventReport.Description.IndexOf("died of their wounds") >= 0)
				{
					diedEvent = true;
				}
				if (eventReport.Week == 13 && eventReport.Description.IndexOf("recovered from their wounds") >= 0)
				{
					recoveredEvent = true;
				}
			}
			Assert.That(diedEvent, Is.True);
			Assert.That(recoveredEvent, Is.True);
		}

		[Test]
		public void ExecuteMedicalConsume_DoesNotDeductFoodOrAir()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			int foodBefore = stack.ItemStacks.ContainsKey(ItemType.All["food"])
				? stack.ItemStacks[ItemType.All["food"]].Quantity
				: 0;
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 1));
			Sequence.Ints.Push(99);

			this.game.Week = 1;
			this.game.ExecuteMedicalConsume();

			int foodAfter = stack.ItemStacks.ContainsKey(ItemType.All["food"])
				? stack.ItemStacks[ItemType.All["food"]].Quantity
				: 0;
			Assert.That(foodAfter, Is.EqualTo(foodBefore));
			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(1));
		}

		[Test]
		public void LoadConfiguration_LoadsUpkeepAndConsumeShortageEffects()
		{
			ModuleType gunPlacement = ModuleType.All["gunplc"];
			Assert.That(gunPlacement.NoUpkeepEffect, Is.EqualTo("damage"));
			Assert.That(gunPlacement.NoUpkeepChance, Is.EqualTo(25));

			ModuleType trucks = ModuleType.All["trucks"];
			Assert.That(trucks.NoConsumeEffect, Is.EqualTo("damage"));
			Assert.That(trucks.NoConsumeChance, Is.EqualTo(25));

			Race terran = Race.All["terran"];
			Assert.That(terran.NoConsumeEffect, Is.EqualTo("wound"));
			Assert.That(terran.NoConsumeChance, Is.EqualTo(25));
			Assert.That(terran.NoUpkeepEffect, Is.EqualTo("off-duty"));
			Assert.That(terran.NoUpkeepChance, Is.EqualTo(50));
		}

		[Test]
		public void Execute_ChargesMaintenanceOnceAtEndOfTurn()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			Region earthRegion = Region.All["R00002"];
			Region isolated = new Region(earthRegion.RegionHolder, "maintonce");
			isolated.RegionType = earthRegion.RegionType;
			factory.Parent = isolated;
			factory.ItemStacks.Add(new ItemStack(ItemType.All["food"], 130));
			factory.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 130));

			this.game.Execute();

			Assert.That(factory.ItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(120));
			Assert.That(factory.ItemStacks[ItemType.All["terair"]].Quantity, Is.EqualTo(130));
			Assert.That(factory.ItemStacks[ItemType.All["cash"]].Quantity, Is.EqualTo(230));
		}

		[Test]
		public void ExecuteMaintenance_PaysCashUpkeepFromStack()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			int bankBefore = (int)factory.Owner.Bank.Balance;

			this.game.Week = 1;
			this.game.ExecuteMaintenance();

			Assert.That(factory.ItemStacks[ItemType.All["cash"]].Quantity, Is.EqualTo(230));
			Assert.That((int)factory.Owner.Bank.Balance, Is.LessThanOrEqualTo(bankBefore));
		}

		[Test]
		public void ExecuteMaintenance_LaunchedDrone_PaysCashUpkeepFromBank()
		{
			Orbit orbit = new Orbit(Orbit.All["O00003"].OrbitHolder, "OUPK1");
			Faction owner = this.game.Factions["2"];
			Faction enemyOwner = this.game.Factions["1"];
			int bankBefore = (int)owner.Bank.Balance;
			ModuleStack hull = new ModuleStack(orbit, owner, ModuleType.All["sshull"], "upkhull");
			hull.AddModule();
			ModuleStack bay = new ModuleStack(hull, owner, ModuleType.All["drnbay"], "upkbay");
			bay.AddModule();
			ModuleStack drone = new ModuleStack(bay, owner, ModuleType.All["alndrn"], "upkdrn");
			drone.AddModule();
			drone.ItemStacks.Add(new ItemStack(ItemType.All["heliu3"], 1));
			ModuleStack enemy = new ModuleStack(orbit, enemyOwner, ModuleType.All["alndrn"], "upkenemy");
			enemy.AddModule();
			enemy.ItemStacks.Add(new ItemStack(ItemType.All["heliu3"], 1));

			Battle battle = new Battle(enemy, hull);
			battle.Execute(this.game.Week);
			Assert.That(drone.IsRootModuleStack, Is.True);

			drone.ExecuteMaintenance(13);

			Assert.That((int)owner.Bank.Balance, Is.EqualTo(bankBefore - 20));
			Assert.That(this.hasEvent(drone, 13, "paid 20 cash [cash] upkeep"), Is.True);
		}

		[Test]
		public void ExecuteMaintenance_UnpaidCashDamagesModule()
		{
			Faction owner = this.game.Factions["2"];
			owner.Bank.Balance = 0;
			ModuleStack gun = new ModuleStack(
				Region.All["R00002"],
				owner,
				ModuleType.All["gunplc"],
				"100200");
			gun.AddModule();
			Sequence.Ints.Push(0);

			gun.ExecuteMaintenance(1);

			Assert.That(gun.Damage, Is.EqualTo(1));
			Assert.That((int)owner.Bank.Balance, Is.EqualTo(0));
			Assert.That(this.hasEvent(gun, 1, "Damaged from lack of upkeep"), Is.True);
		}

		[Test]
		public void ExecuteMaintenance_ConsumesFoodOnEarthWithoutTerair()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			factory.ItemStacks.Add(new ItemStack(ItemType.All["food"], 20));
			factory.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 10));

			factory.ExecuteMaintenance(1);

			Assert.That(factory.ItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(10));
			Assert.That(factory.ItemStacks[ItemType.All["terair"]].Quantity, Is.EqualTo(10));
			Assert.That(factory.ItemStacks[ItemType.All["cash"]].Quantity, Is.EqualTo(230));
		}

		[Test]
		public void ExecuteMaintenance_ConsumesTerairInOrbit()
		{
			ModuleStack quarters = ModuleStack.All["100016"];
			quarters.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 20));

			quarters.ExecuteMaintenance(1);

			Assert.That(quarters.ItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(989));
			Assert.That(quarters.ItemStacks[ItemType.All["terair"]].Quantity, Is.EqualTo(9));
			Assert.That(quarters.ItemStacks[ItemType.All["cash"]].Quantity, Is.EqualTo(9949));
		}

		[Test]
		public void ExecuteMaintenance_ConsumesTerairOnMoon()
		{
			Region moonRegion = this.firstMoonRegion();
			Assert.That(moonRegion, Is.Not.Null);
			ModuleStack quarters = new ModuleStack(
				moonRegion,
				this.game.Factions["2"],
				ModuleType.All["crwqrt"],
				"100300");
			quarters.AddModule();
			quarters.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 1));
			quarters.ItemStacks.Add(new ItemStack(ItemType.All["food"], 5));
			quarters.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 5));
			quarters.ItemStacks.Add(new ItemStack(ItemType.All["cash"], 100));

			quarters.ExecuteMaintenance(1);

			Assert.That(quarters.ItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(4));
			Assert.That(quarters.ItemStacks[ItemType.All["terair"]].Quantity, Is.EqualTo(4));
			Assert.That(quarters.ItemStacks[ItemType.All["cash"]].Quantity, Is.EqualTo(79));
		}

		[Test]
		public void ExecuteMaintenance_UnpaidFoodWoundsTerrans()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			Region earthRegion = Region.All["R00002"];
			Region isolated = new Region(earthRegion.RegionHolder, "starveiso");
			isolated.RegionType = earthRegion.RegionType;
			factory.Parent = isolated;
			for (int i = 0; i < 10; i++)
			{
				Sequence.Ints.Push(0);
			}

			factory.ExecuteMaintenance(1);

			Assert.That(factory.ItemStacks.Has(ItemType.All["terran"]), Is.False);
			Assert.That(factory.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(10));
			Assert.That(this.hasEvent(factory, 1, "wounded from lack of supplies"), Is.True);
		}

		[Test]
		public void ExecuteSickBayHeal_WithMedicines_ConvertsFourPerWeek()
		{
			ModuleStack bay = this.createSickBay(1, 8, 4);

			this.game.Week = 1;
			this.game.ExecuteSickBayHeal();

			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
			Assert.That(bay.ItemStacks[ItemType.All["terran"]].Quantity, Is.EqualTo(4));
			Assert.That(bay.ItemStacks.Has(ItemType.All["medici"]), Is.False);
			Assert.That(bay.SickBayUnmedicatedWeeks, Is.EqualTo(0));
		}

		[Test]
		public void ExecuteSickBayHeal_WithoutMedicines_ConvertsTwoAfterFourWeeks()
		{
			ModuleStack bay = this.createSickBay(1, 8, 0);

			for (int week = 1; week <= 3; week++)
			{
				bay.ExecuteSickBayHeal(week);
				Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(8));
			}

			bay.ExecuteSickBayHeal(4);

			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(6));
			Assert.That(bay.ItemStacks[ItemType.All["terran"]].Quantity, Is.EqualTo(2));
			Assert.That(bay.SickBayUnmedicatedWeeks, Is.EqualTo(0));
		}

		[Test]
		public void ExecuteSickBayHeal_MediciWeek_DoesNotTickUnmedicatedClock()
		{
			ModuleStack bay = this.createSickBay(1, 10, 1);

			bay.ExecuteSickBayHeal(1);
			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(9));
			Assert.That(bay.SickBayUnmedicatedWeeks, Is.EqualTo(0));

			for (int week = 2; week <= 4; week++)
			{
				bay.ExecuteSickBayHeal(week);
			}
			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(9));

			bay.ExecuteSickBayHeal(5);
			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(7));
		}

		[Test]
		public void ExecuteSickBayHeal_DoesNotConvertMadTerrans()
		{
			ModuleStack bay = this.createSickBay(1, 0, 4);
			bay.ItemStacks.Add(new ItemStack(ItemType.All["madtrn"], 4));

			bay.ExecuteSickBayHeal(1);

			Assert.That(bay.ItemStacks[ItemType.All["madtrn"]].Quantity, Is.EqualTo(4));
			Assert.That(bay.ItemStacks.Has(ItemType.All["terran"]), Is.False);
		}

		[Test]
		public void ExecuteMedicalConsume_AfterSickBayHeal_BillsRemainingWounded()
		{
			ModuleStack bay = this.createSickBay(1, 8, 5);

			this.game.Week = 1;
			this.game.ExecuteSickBayHeal();
			this.game.ExecuteMedicalConsume();

			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
			Assert.That(bay.ItemStacks.Has(ItemType.All["medici"]), Is.False);
		}

		[Test]
		public void ExecuteSickBayHeal_TwoBays_ConvertsEightWithMedicines()
		{
			ModuleStack bay = this.createSickBay(2, 16, 8);

			this.game.Week = 1;
			this.game.ExecuteSickBayHeal();

			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(8));
			Assert.That(bay.ItemStacks[ItemType.All["terran"]].Quantity, Is.EqualTo(8));
			Assert.That(bay.ItemStacks.Has(ItemType.All["medici"]), Is.False);
		}

		[Test]
		public void ExecuteSickBayHeal_PartialMedicines_LimitedBySupply()
		{
			ModuleStack bay = this.createSickBay(1, 8, 2);

			this.game.Week = 1;
			this.game.ExecuteSickBayHeal();

			Assert.That(bay.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(6));
			Assert.That(bay.ItemStacks[ItemType.All["terran"]].Quantity, Is.EqualTo(2));
			Assert.That(bay.ItemStacks.Has(ItemType.All["medici"]), Is.False);
		}

		[Test]
		public void ExecuteSickBayHeal_Week13_BeforeQuarterlyOutcome()
		{
			ModuleStack bay = this.createSickBay(1, 8, 4);

			this.game.Week = 13;
			this.game.ExecuteSickBayHeal();
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			this.game.ExecuteQuarterlyWoundedOutcome();

			Assert.That(bay.ItemStacks.Quantity(ItemType.All["wndtrn"]), Is.EqualTo(0));
			Assert.That(bay.ItemStacks.Quantity(ItemType.All["terran"]), Is.EqualTo(4));
		}

		[Test]
		public void ExecuteSickBayHeal_Medfac_DoesNotConvertWounded()
		{
			ModuleStack clinic = new ModuleStack(
				Region.All["R00002"],
				this.game.Factions["2"],
				ModuleType.All["medfac"],
				"100402");
			clinic.AddModules(1);
			clinic.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			clinic.ItemStacks.Add(new ItemStack(ItemType.All["medici"], 4));

			clinic.ExecuteSickBayHeal(1);

			Assert.That(clinic.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
			Assert.That(clinic.ItemStacks.Has(ItemType.All["terran"]), Is.False);
		}

		[Test]
		public void SaveLoad_PreservesSickBayUnmedicatedWeeks()
		{
			ModuleStack bay = this.createSickBay(1, 4, 0);
			bay.SickBayUnmedicatedWeeks = 3;

			XmlDocument doc = new XmlDocument();
			XmlElement saved = bay.SaveXml(doc);
			doc.AppendChild(saved);

			ModuleStack loaded = new ModuleStack(
				Region.All["R00002"],
				this.game.Factions["2"],
				ModuleType.All["sckbay"],
				"100401");
			loaded.LoadXml(saved);

			Assert.That(loaded.SickBayUnmedicatedWeeks, Is.EqualTo(3));
		}

		private ModuleStack createSickBay(int quantity, int wounded, int medici)
		{
			ModuleStack bay = new ModuleStack(
				Region.All["R00002"],
				this.game.Factions["2"],
				ModuleType.All["sckbay"],
				"100400");
			bay.AddModules(quantity);
			if (wounded > 0)
			{
				bay.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], wounded));
			}
			if (medici > 0)
			{
				bay.ItemStacks.Add(new ItemStack(ItemType.All["medici"], medici));
			}
			return bay;
		}

		private bool hasEvent(ModuleStack stack, int week, string fragment)
		{
			foreach (EventReport eventReport in stack.EventReports)
			{
				if (eventReport.Week == week && eventReport.Description.IndexOf(fragment) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		[Test]
		public void ExecuteMaintenance_PullsFoodFromNestedChild()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			ModuleStack locker = new ModuleStack(
				factory,
				factory.Owner,
				ModuleType.All["cargob"],
				"100501");
			locker.AddModule();
			locker.ItemStacks.Add(new ItemStack(ItemType.All["food"], 10));

			factory.ExecuteMaintenance(1);

			Assert.That(locker.ItemStacks.Has(ItemType.All["food"]), Is.False);
			Assert.That(this.hasEvent(factory, 1, "wounded from lack of supplies"), Is.False);
		}

		[Test]
		public void ExecuteMaintenance_PullsFoodFromParent()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			ModuleStack city = (ModuleStack)factory.Parent;
			city.ItemStacks.Add(new ItemStack(ItemType.All["food"], 10));

			factory.ExecuteMaintenance(1);

			Assert.That(city.ItemStacks.Quantity(ItemType.All["food"]), Is.EqualTo(0));
			Assert.That(this.hasEvent(factory, 1, "wounded from lack of supplies"), Is.False);
		}

		[Test]
		public void ExecuteMaintenance_PullsFoodFromSiblingAtSameLocation()
		{
			Faction owner = this.game.Factions["2"];
			ModuleStack hungry = new ModuleStack(
				Region.All["R00002"],
				owner,
				ModuleType.All["tanks"],
				"100510");
			hungry.AddModule();
			hungry.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			hungry.ItemStacks.Add(new ItemStack(ItemType.All["cash"], 100));
			ModuleStack depot = new ModuleStack(
				Region.All["R00002"],
				owner,
				ModuleType.All["cargob"],
				"100511");
			depot.AddModule();
			depot.ItemStacks.Add(new ItemStack(ItemType.All["food"], 32));

			hungry.ExecuteMaintenance(1);

			Assert.That(depot.ItemStacks.Has(ItemType.All["food"]), Is.False);
			Assert.That(this.hasEvent(hungry, 1, "wounded from lack of supplies"), Is.False);
		}

		[Test]
		public void ExecuteMaintenance_DoesNotPullFoodFromOtherFaction()
		{
			Faction owner = this.game.Factions["2"];
			ModuleStack hungry = new ModuleStack(
				Region.All["R00002"],
				owner,
				ModuleType.All["tanks"],
				"100512");
			hungry.AddModule();
			hungry.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 16));
			hungry.ItemStacks.Add(new ItemStack(ItemType.All["cash"], 100));
			ModuleStack foreign = new ModuleStack(
				Region.All["R00002"],
				this.game.Factions["1"],
				ModuleType.All["cargob"],
				"100513");
			foreign.AddModule();
			foreign.ItemStacks.Add(new ItemStack(ItemType.All["food"], 16));
			for (int i = 0; i < 16; i++)
			{
				Sequence.Ints.Push(0);
			}

			hungry.ExecuteMaintenance(1);

			Assert.That(foreign.ItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(16));
			Assert.That(this.hasEvent(hungry, 1, "wounded from lack of supplies"), Is.True);
		}

		private Region firstMoonRegion()
		{
			foreach (Region region in Region.All.Values)
			{
				if (region.RegionHolder is Moon)
				{
					return region;
				}
			}
			return null;
		}
	}
}
