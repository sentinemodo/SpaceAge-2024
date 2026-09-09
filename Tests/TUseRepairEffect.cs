using System.Collections.Generic;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TUseRepairEffect : TTest
	{
		[SetUp]
		public void setupUseRepairEffect()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownUseRepairEffect()
		{
			this.ClearGame();
		}

		[Test]
		public void LoadConfiguration_RepairTech_HasProduceEffectMetadata()
		{
			Technology repair = Technology.All["repair"];

			Assert.That(repair.ProductionType, Is.EqualTo(EProductionType.Effects));
			Assert.That(repair.UseProduceEffectName, Is.EqualTo("repair"));
			Assert.That(repair.UseProduceTarget, Is.EqualTo("module-damage"));
			Assert.That(repair.UseProduceChange, Is.EqualTo(-1));
		}

		[Test]
		public void ExecuteUseRepair_RemovesOneDamageAfterTwoWeeks()
		{
			ModuleStack factory = this.game.ModuleStacks["000004"];
			ModuleStack city = this.game.ModuleStacks["000001"];
			factory.Technologies.Add(Technology.All["repair"]);
			factory.ItemStacks.Add(new ItemStack(ItemType.All["spare"], 1));
			city.Modules[0].Damage = 5;

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			UseOrder useOrder = (UseOrder)factory.Orders[0];

			this.executeOrder(factory, useOrder, 0);
			Assert.That(factory.ItemStacks.Has(ItemType.All["spare"]), Is.False);
			Assert.That(city.Modules[0].Damage, Is.EqualTo(5));
			Assert.That(factory.Effects.IsProducing, Is.True);
			Assert.That(useOrder.Executed, Is.False);

			this.executeOrder(factory, useOrder, 1);
			Assert.That(city.Modules[0].Damage, Is.EqualTo(4));
			Assert.That(factory.Effects.IsProducing, Is.False);
			Assert.That(useOrder.Executed);
			Assert.That(factory.EventReports[factory.EventReports.Count - 1].Description, Is.EqualTo("repaired 1 damage."));
		}

		[Test]
		public void ExecuteUseRepair_NoDamage_DoesNotConsumeSpare()
		{
			ModuleStack factory = this.game.ModuleStacks["000004"];
			factory.Technologies.Add(Technology.All["repair"]);
			factory.ItemStacks.Add(new ItemStack(ItemType.All["spare"], 1));

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			UseOrder useOrder = (UseOrder)factory.Orders[0];

			this.executeOrder(factory, useOrder, 0);

			Assert.That(factory.ItemStacks.Has(ItemType.All["spare"]));
			Assert.That(factory.Effects.IsProducing, Is.False);
			Assert.That(useOrder.Executed, Is.False);
			Assert.That(factory.EventReports[factory.EventReports.Count - 1].Description, Is.EqualTo("USE failed: no damage to repair."));
		}

		[Test]
		public void ExecuteUseRepair_RequiresProductionModule()
		{
			ModuleStack trucks = this.game.ModuleStacks["100001"];
			ModuleStack city = this.game.ModuleStacks["000001"];
			trucks.Technologies.Add(Technology.All["repair"]);
			trucks.ItemStacks.Add(new ItemStack(ItemType.All["spare"], 1));
			city.Modules[0].Damage = 5;

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"use repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			UseOrder useOrder = (UseOrder)trucks.Orders[0];

			this.executeOrder(trucks, useOrder, 0);

			Assert.That(trucks.ItemStacks.Has(ItemType.All["spare"]));
			Assert.That(trucks.Effects.IsProducing, Is.False);
			Assert.That(useOrder.Executed, Is.False);
			Assert.That(trucks.EventReports[trucks.EventReports.Count - 1].Description,
				Does.Contain("only usable in production"));
		}
	}
}
