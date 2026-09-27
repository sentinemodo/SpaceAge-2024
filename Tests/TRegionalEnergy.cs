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

			Assert.That(factory.UnitEnergyProduction(), Is.EqualTo(0));
			Assert.That(factory.UnitEnergyRequired(), Is.EqualTo(15));
			Assert.That(factory.HasRegionalEnergySurplus(), Is.False);
			Assert.That(factory.QuantityOperational, Is.EqualTo(0));

			ModuleStack plant = new ModuleStack(region, faction, ModuleType.All["wnplnt"], "100802");
			plant.AddModules(5);
			plant.ActivateModules(5);

			Assert.That(plant.UnitEnergyProduction(), Is.EqualTo(20));
			Assert.That(plant.UnitEnergyRequired(), Is.EqualTo(5));
			Assert.That(factory.RegionalEnergyProduction(), Is.EqualTo(20));
			Assert.That(factory.RegionalEnergyRequired(), Is.EqualTo(20));
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
	}
}
