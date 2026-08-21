using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TTransfer : TTest
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
		public void Parse_TransferQuantityToStack_MatchesXmlLoad()
		{
			ModuleStack transferer = this.game.ModuleStacks["100001"];
			ModuleStack receiver = this.game.ModuleStacks["100002"];
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"transfer 1 to 100002",
				"#end"
			});

			TransferOrder parsed = (TransferOrder)transferer.Orders[0];
			Assert.That(parsed.Quantity, Is.EqualTo(1));
			Assert.That(parsed.Receiver, Is.EqualTo(receiver));
			Assert.That(parsed.ModuleType, Is.EqualTo(transferer.ModuleType));

			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<orders/>");
			XmlElement elOrder = parsed.SaveXml(doc, "modulestack");
			XmlElement elTransfer = (XmlElement)elOrder.SelectSingleNode("transfer");
			Assert.That(elTransfer.GetAttribute("quantity"), Is.EqualTo("1"));
			Assert.That(elTransfer.GetAttribute("receiver"), Is.EqualTo("100002"));

			transferer.Orders.Clear();
			XmlDocument loadDoc = new XmlDocument();
			loadDoc.LoadXml(elOrder.OuterXml);
			TransferOrder loaded = new TransferOrder(transferer);
			loaded.LoadXml(loadDoc.DocumentElement);
			Assert.That(loaded.Quantity, Is.EqualTo(1));
			Assert.That(loaded.Receiver, Is.EqualTo(receiver));
		}

		[Test]
		public void Execute_HangarCraft_CannotTransferOntoHull()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack hull = new ModuleStack(region, owner, ModuleType.All["sshull"], "xferhull");
			hull.AddModule();
			ModuleStack drones = new ModuleStack(region, owner, ModuleType.All["alndrn"], "xferdrn");
			drones.AddModule();
			drones.AddModule();

			TransferOrder order = new TransferOrder(drones, hull, ModuleType.All["alndrn"], 2, 0);
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.False);
			Assert.That(drones.Quantity, Is.EqualTo(2));
			Assert.That(drones.Parent, Is.EqualTo(region));
		}

		[Test]
		public void Execute_HangarCraft_TransfersIntoDroneBay()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack hull = new ModuleStack(region, owner, ModuleType.All["sshull"], "bayhull");
			hull.AddModule();
			ModuleStack bay = new ModuleStack(hull, owner, ModuleType.All["drnbay"], "xferbay");
			bay.AddModule();
			ModuleStack drones = new ModuleStack(region, owner, ModuleType.All["alndrn"], "baydrn");
			drones.AddModule();
			drones.AddModule();

			TransferOrder order = new TransferOrder(drones, bay, ModuleType.All["alndrn"], 2, 0);
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			int nestedDrones = 0;
			foreach (ModuleStack nested in bay.ModuleStacks.Values)
			{
				if (nested.ModuleType != null && nested.ModuleType.Name == "alndrn")
				{
					nestedDrones += nested.Quantity;
				}
			}
			Assert.That(nestedDrones, Is.EqualTo(2));
		}

		[Test]
		public void Execute_HangarCraft_StackUnderHullFails()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack hull = new ModuleStack(region, owner, ModuleType.All["sshull"], "stackhull");
			hull.AddModule();
			ModuleStack drones = new ModuleStack(region, owner, ModuleType.All["alndrn"], "stackdrn");
			drones.AddModule();

			StackOrder order = new StackOrder(drones, hull);
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.False);
			Assert.That(drones.Parent, Is.EqualTo(region));
		}
	}
}
