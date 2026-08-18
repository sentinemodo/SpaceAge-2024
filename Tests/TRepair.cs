using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TRepair : TTest
	{
		[SetUp]
		public void setupRepair()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownRepair()
		{
			this.ClearGame();
		}

		[Test]
		public void AssignRepairOrder()
		{
			ModuleStack trucks = this.game.ModuleStacks["100001"];
			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"repair",
				"#end"
			};

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			Assert.That(trucks.Orders.Count, Is.EqualTo(1));
			RepairOrder order = (RepairOrder)trucks.Orders[0];
			Assert.That(order.Type, Is.EqualTo(EOrderType.repair));
			Assert.That(order.IsEngineeringShop, Is.False);
			Assert.That(order.RepairPoints, Is.EqualTo(RepairOrder.ManualRepairPoints));
			Assert.That(order.SparePartsRequired, Is.EqualTo(1));
			Assert.That(order.Report(trucks.Owner)[0], Is.EqualTo("repair"));
		}

		[Test]
		public void Execute_OtherModuleRepairsTenHitPointsWhenSpareAvailable()
		{
			ModuleStack trucks = this.game.ModuleStacks["100001"];
			trucks.Modules[0].Damage = 15;
			trucks.ItemStacks.Add(new ItemStack(ItemType.All["spare"], 1));

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			RepairOrder order = (RepairOrder)trucks.Orders[0];

			trucks.ExecutedLongOrder = false;
			trucks.Execute(this.game.Week);

			Assert.That(trucks.Modules[0].Damage, Is.EqualTo(5));
			Assert.That(trucks.Modules[1].Damage, Is.EqualTo(0));
			Assert.That(trucks.ItemStacks.Has(ItemType.All["spare"]), Is.False);
			Assert.That(order.Executed);
			Assert.That(order.Executing, Is.False);
		}

		[Test]
		public void Execute_WithoutSparePartsRepairsOneHitPoint()
		{
			ModuleStack trucks = this.game.ModuleStacks["100001"];
			trucks.Modules[0].Damage = 15;

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			RepairOrder order = (RepairOrder)trucks.Orders[0];

			trucks.ExecutedLongOrder = false;
			trucks.Execute(this.game.Week);

			Assert.That(trucks.Modules[0].Damage, Is.EqualTo(14));
			Assert.That(order.Executed);
		}

		[Test]
		public void Execute_DoesNothingWhenThereIsNoDamage()
		{
			ModuleStack trucks = this.game.ModuleStacks["100001"];
			trucks.ItemStacks.Add(new ItemStack(ItemType.All["spare"], 1));
			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			RepairOrder order = (RepairOrder)trucks.Orders[0];

			trucks.ExecutedLongOrder = false;
			trucks.Execute(this.game.Week);

			Assert.That(trucks.Modules[0].Damage, Is.EqualTo(0));
			Assert.That(trucks.ItemStacks.Has(ItemType.All["spare"]));
			Assert.That(order.Executed, Is.False);
			Assert.That(order.Executing, Is.False);
			Assert.That(trucks.ExecutedLongOrder, Is.False);
		}

		[Test]
		public void Execute_EngineeringShopRepairsParentAndNestedAndConsumesSpare()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			ModuleStack reactor = this.game.ModuleStacks["100013"];
			ModuleStack shop = this.addEngineeringShop(frigate, "e100099");
			frigate.Modules[0].Damage = 15;
			reactor.Modules[0].Damage = 15;
			frigate.ItemStacks.Add(new ItemStack(ItemType.All["spare"], 1));

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack e100099",
				"repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			RepairOrder order = (RepairOrder)shop.Orders[0];
			Assert.That(order.IsEngineeringShop);
			Assert.That(order.RepairPoints, Is.EqualTo(20));
			Assert.That(order.RepairScope, Is.EqualTo(frigate));

			shop.ExecutedLongOrder = false;
			shop.Execute(this.game.Week);

			Assert.That(frigate.Modules[0].Damage, Is.EqualTo(0));
			Assert.That(reactor.Modules[0].Damage, Is.EqualTo(10));
			Assert.That(frigate.ItemStacks.Has(ItemType.All["spare"]), Is.False);
			Assert.That(order.Executed);
		}

		[Test]
		public void Execute_EngineeringShopWithoutSparePartsRepairsOneHitPoint()
		{
			ModuleStack frigate = this.game.ModuleStacks["100011"];
			ModuleStack shop = this.addEngineeringShop(frigate, "e100098");
			frigate.Modules[0].Damage = 12;

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack e100098",
				"repair",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(testcommands);
			RepairOrder order = (RepairOrder)shop.Orders[0];

			shop.ExecutedLongOrder = false;
			shop.Execute(this.game.Week);

			Assert.That(frigate.Modules[0].Damage, Is.EqualTo(11));
			Assert.That(order.Executed);
		}

		private ModuleStack addEngineeringShop(ModuleStack parent, string name)
		{
			ModuleStack shop = new ModuleStack(parent, parent.Owner, ModuleType.All["engshp"], name);
			shop.AddModule();
			shop.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 2));
			return shop;
		}
	}
}
