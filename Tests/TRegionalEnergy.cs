using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TRegionalEnergy : TTest
	{
		[SetUp]
		public void setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void RegionalEnergySharing_SiblingPlantPowersFactory()
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ItemType terran = ItemType.All["terran"];

			ModuleStack factory = new ModuleStack(region, faction, ModuleType.All["factry"], "100801");
			factory.AddModule(0);
			factory.ActivateModules(1);
			factory.ItemStacks.Add(new ItemStack(terran, 10));

			Assert.That(factory.NominalUnitEnergyProduction(), Is.EqualTo(0));
			Assert.That(factory.NominalUnitEnergyRequired(), Is.EqualTo(15));
			Assert.That(factory.HasRegionalEnergySurplus(), Is.False);
			Assert.That(factory.QuantityOperational, Is.EqualTo(0));

			ModuleStack plant = new ModuleStack(region, faction, ModuleType.All["wnplnt"], "100802");
			plant.AddModules(5);
			plant.ActivateModules(5);

			Assert.That(plant.NominalUnitEnergyProduction(), Is.EqualTo(20));
			Assert.That(plant.NominalUnitEnergyRequired(), Is.EqualTo(5));
			Assert.That(factory.RegionalNominalEnergyProduction(), Is.EqualTo(20));
			Assert.That(factory.RegionalNominalEnergyRequired(), Is.EqualTo(20));
			Assert.That(factory.HasRegionalEnergySurplus(), Is.True);
			Assert.That(factory.QuantityOperational, Is.EqualTo(1));
			Assert.That(factory.IsActive, Is.True);
		}

		[Test]
		public void RegionalEnergySharing_RespectsSharingFalse()
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ItemType terran = ItemType.All["terran"];

			ModuleStack factory = new ModuleStack(region, faction, ModuleType.All["factry"], "100803");
			factory.AddModule(0);
			factory.ActivateModules(1);
			factory.ItemStacks.Add(new ItemStack(terran, 10));

			ModuleStack plant = new ModuleStack(region, faction, ModuleType.All["wnplnt"], "100804");
			plant.AddModules(5);
			plant.ActivateModules(5);
			plant.Sharing = false;

			Assert.That(factory.RegionalEnergyProduction(), Is.EqualTo(0));
			Assert.That(factory.HasRegionalEnergySurplus(), Is.False);
			Assert.That(factory.QuantityOperational, Is.EqualTo(0));
		}

		[Test]
		public void EnergyPriorityShutdown_ShedsProductionBeforeFactory()
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ItemType terran = ItemType.All["terran"];

			ModuleStack plant = new ModuleStack(region, faction, ModuleType.All["wnplnt"], "100805");
			plant.AddModules(4);
			plant.ActivateModules(4);

			ModuleStack factory = new ModuleStack(region, faction, ModuleType.All["factry"], "100806");
			factory.AddModule(0);
			factory.ActivateModules(1);
			factory.ItemStacks.Add(new ItemStack(terran, 10));

			Assert.That(factory.RegionalNominalEnergyProduction(), Is.EqualTo(16));
			Assert.That(factory.RegionalNominalEnergyRequired(), Is.EqualTo(19));
			Assert.That(plant.QuantityOperational, Is.EqualTo(4));
			Assert.That(factory.QuantityOperational, Is.EqualTo(0));
		}

		[Test]
		public void EnergyPriorityShutdown_CustomPriorityProtectsStack()
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ItemType terran = ItemType.All["terran"];

			ModuleStack plant = new ModuleStack(region, faction, ModuleType.All["wnplnt"], "100807");
			plant.AddModules(5);
			plant.ActivateModules(5);

			ModuleStack factoryA = new ModuleStack(region, faction, ModuleType.All["factry"], "100808");
			factoryA.AddModule(0);
			factoryA.ActivateModules(1);
			factoryA.ItemStacks.Add(new ItemStack(terran, 10));

			ModuleStack factoryB = new ModuleStack(region, faction, ModuleType.All["factry"], "100809");
			factoryB.AddModule(0);
			factoryB.ActivateModules(1);
			factoryB.SetEnergyPriority(10);
			factoryB.ItemStacks.Add(new ItemStack(terran, 10));

			Assert.That(factoryA.QuantityOperational, Is.EqualTo(1));
			Assert.That(factoryB.QuantityOperational, Is.EqualTo(0));
		}

		[Test]
		public void EnergyPriorityShutdown_DoesNotShedZeroEnergyNeedUnits()
		{
			Region region = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];
			ItemType terran = ItemType.All["terran"];

			ModuleStack plant = new ModuleStack(region, faction, ModuleType.All["wnplnt"], "100810");
			plant.AddModules(4);
			plant.ActivateModules(4);

			ModuleStack factory = new ModuleStack(region, faction, ModuleType.All["factry"], "100811");
			factory.AddModule(0);
			factory.ActivateModules(1);
			factory.ItemStacks.Add(new ItemStack(terran, 10));

			ModuleStack truck = new ModuleStack(region, faction, ModuleType.All["trucks"], "100812");
			truck.AddModule(0);
			truck.ActivateModules(1);
			truck.ItemStacks.Add(new ItemStack(terran, 1));

			Assert.That(truck.ModuleType.EnergyRequired, Is.EqualTo(0));
			Assert.That(factory.RegionalNominalEnergyProduction(), Is.EqualTo(16));
			Assert.That(factory.RegionalNominalEnergyRequired(), Is.EqualTo(19));
			Assert.That(factory.QuantityOperational, Is.EqualTo(0));
			Assert.That(truck.QuantityOperational, Is.EqualTo(1));
			Assert.That(truck.IsActive, Is.True);
		}

		[Test]
		public void SetOrder_EnergyPriority_StoresOverride()
		{
			ModuleStack factory = this.game.ModuleStacks["000004"];
			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"set energy 12",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			((SetOrder)factory.Orders[0]).Execute(this.game.Week);

			Assert.That(factory.EffectiveEnergyPriority, Is.EqualTo(12));
			Assert.That(factory.HasNonDefaultEnergyPriority, Is.True);
		}

		[Test]
		public void DefaultEnergyPriority_MatchesModuleGroup()
		{
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.energy), Is.EqualTo(0));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.habitat), Is.EqualTo(1));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.command), Is.EqualTo(2));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.military), Is.EqualTo(3));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.propulsion), Is.EqualTo(4));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.extraction), Is.EqualTo(5));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.production), Is.EqualTo(6));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.storage), Is.EqualTo(10));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.research), Is.EqualTo(10));
			Assert.That(ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.agricultural), Is.EqualTo(8));
		}
	}
}
