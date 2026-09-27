using System;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TFoodGranaryTurn : TTest
	{
		private const string FixtureDirName = "food-granary-turn";
		private const string CargoBayId = "900002";
		private const string FarmId = "900003";
		private const string PowerPlantId = "900004";
		private const int WeeksPerTurn = 13;

		[Test]
		public void FullTurn_GranaryFoodEqualsQuarterProductionMinusCrewConsume()
		{
			this.game?.ClearDictionaries();
			string fixtureDir = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", FixtureDirName);
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadContracts();
			this.dataFile.LoadOrders();

			this.game = this.dataFile.Game;
			ModuleStack cargoBay = this.game.ModuleStacks[CargoBayId];
			ModuleStack farms = this.game.ModuleStacks[FarmId];
			ModuleStack powerPlant = this.game.ModuleStacks[PowerPlantId];
			ItemType food = ItemType.All["food"];

			Assert.That(cargoBay.ItemStacks.Quantity(food), Is.EqualTo(0), "seed cargo bay with no food");
			Assert.That(cargoBay.ItemStacks.Quantity(ItemType.All["cash"]), Is.GreaterThan(0), "seed cash for upkeep");
			Assert.That(farms.ItemStacks.Quantity(ItemType.All["terran"]), Is.EqualTo(farms.Quantity * farms.ModuleType.CrewRequired),
				"seed farms with full crew");
			Assert.That(powerPlant.ItemStacks.Quantity(ItemType.All["terran"]), Is.EqualTo(powerPlant.Quantity * powerPlant.ModuleType.CrewRequired),
				"seed powerplant with crew");
			Assert.That(powerPlant.ItemStacks.Quantity(ItemType.All["carbon"]), Is.GreaterThanOrEqualTo(5),
				"seed powerplant with fuel for a turn");

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.LoadOrders(Path.Combine(fixtureDir, "orders.2.txt"), false);

			this.game.Execute();

			int foodPerFarmModulePerUse = Technology.All["farmng"].UseProduceItems[food].Quantity;
			int quarterProduction = WeeksPerTurn * farms.Quantity * foodPerFarmModulePerUse;
			int crewFoodUpkeep = this.CountTerranFoodConsumeUnder(cargoBay.Parent as ModuleStack);
			int expectedFoodInCargoBay = quarterProduction - crewFoodUpkeep;

			Assert.That(
				cargoBay.ItemStacks.Quantity(food),
				Is.EqualTo(expectedFoodInCargoBay),
				string.Format(
					"cargo bay food after turn: expected {0} (= {1} weeks * {2} farms * {3} food/use - {4} crew food)",
					expectedFoodInCargoBay,
					WeeksPerTurn,
					farms.Quantity,
					foodPerFarmModulePerUse,
					crewFoodUpkeep));
		}

		private int CountTerranFoodConsumeUnder(ModuleStack root)
		{
			if (root == null)
			{
				return 0;
			}

			int total = 0;
			this.AccumulateTerranFoodConsume(root, ref total);
			return total;
		}

		private void AccumulateTerranFoodConsume(ModuleStack stack, ref int total)
		{
			ItemType terran = ItemType.All["terran"];
			int people = stack.ItemStacks.Quantity(terran) + stack.People.Count;
			if (people > 0 && Race.All["terran"].Consume.ContainsKey(ItemType.All["food"]))
			{
				total += people * Race.All["terran"].Consume[ItemType.All["food"]].Quantity;
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.AccumulateTerranFoodConsume(nested, ref total);
			}
		}
	}
}
