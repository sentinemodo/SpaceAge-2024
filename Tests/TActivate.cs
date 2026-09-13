using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TActivate : TTest
	{
		[SetUp]
		public void setupActivate()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardownActivate()
		{
			this.ClearGame();
		}

		[Test]
		public void DeactivateOrder_DefaultAll_DeactivatesEveryModule()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 3)
			{
				stack.AddModule();
			}

			DeactivateOrder order = new DeactivateOrder(stack);
			order.Parse(string.Empty);
			this.executeOrder(stack, order, 0);

			Assert.That(stack.QuantityInactive, Is.EqualTo(3));
			Assert.That(stack.QuantityActive, Is.EqualTo(0));
			Assert.That(order.Executed, Is.True);
		}

		[Test]
		public void DeactivateOrder_Quantity_DeactivatesRequestedCount()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 5)
			{
				stack.AddModule();
			}

			DeactivateOrder order = new DeactivateOrder(stack);
			order.Parse("2 modules");
			this.executeOrder(stack, order, 0);

			Assert.That(stack.QuantityInactive, Is.EqualTo(2));
			Assert.That(stack.QuantityActive, Is.EqualTo(3));
		}

		[Test]
		public void ActivateOrder_Quantity_ActivatesInactiveModules()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 4)
			{
				stack.AddModule();
			}
			stack.DeactivateModules(3);

			ActivateOrder order = new ActivateOrder(stack);
			order.Parse("2");
			this.executeOrder(stack, order, 0);

			Assert.That(stack.QuantityInactive, Is.EqualTo(1));
			Assert.That(stack.QuantityActive, Is.EqualTo(3));
		}

		[Test]
		public void InactiveModules_ReduceUpkeepNotMassOrCapacity()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 3)
			{
				stack.AddModule();
			}

			ItemType cash = ItemType.All["cash"];
			double massBefore = stack.MassNetto;
			double capacityBefore = stack.Capacity;
			int upkeepBefore = stack.UpkeepNetto[cash].Quantity;

			stack.DeactivateModules(2);

			Assert.That(stack.MassNetto, Is.EqualTo(massBefore));
			Assert.That(stack.Capacity, Is.EqualTo(capacityBefore));
			Assert.That(stack.UpkeepNetto[cash].Quantity, Is.EqualTo(upkeepBefore * 1 / 3));
		}

		[Test]
		public void ReportHeader_ShowsInactiveModuleCount()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 5)
			{
				stack.AddModule();
			}
			stack.DeactivateModules(2);

			Faction owner = stack.Owner;
			string header = stack.ReportHeader(owner);

			Assert.That(header, Does.Contain("2 modules inactive"));
		}

		[Test]
		public void DamagedAndInactiveModules_Coexist()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 10)
			{
				stack.AddModule();
			}

			int heavilyDamagedThreshold = stack.ModuleType.DamageCapacity / 2 + 1;
			stack.Modules[7].Damage = heavilyDamagedThreshold;
			stack.Modules[8].Damage = heavilyDamagedThreshold;
			stack.Modules[9].Damage = heavilyDamagedThreshold;
			stack.DeactivateModules(4);

			Assert.That(stack.Quantity, Is.EqualTo(10));
			Assert.That(stack.QuantityInactive, Is.EqualTo(4));
			Assert.That(stack.QuantityActive, Is.EqualTo(3));
		}

		[Test]
		public void SaveLoad_PersistsInactiveModules()
		{
			ModuleStack stack = this.game.ModuleStacks["000007"];
			while (stack.Quantity < 2)
			{
				stack.AddModule();
			}
			stack.DeactivateModules(1);

			XmlDocument doc = new XmlDocument();
			XmlElement saved = stack.SaveXml(doc, stack.Owner);
			Assert.That(saved.SelectSingleNode("module[@activated='false']"), Is.Not.Null);

			ModuleStack loaded = new ModuleStack(stack.Location, stack.Owner, stack.ModuleType, "loadedtest");
			loaded.LoadXml(saved);

			Assert.That(loaded.QuantityInactive, Is.EqualTo(1));
			Assert.That(loaded.QuantityActive, Is.EqualTo(1));
		}
	}
}
