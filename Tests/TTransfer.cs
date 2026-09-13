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

		[Test]
		public void Parse_TransferAllDamagedModulesToStack()
		{
			ModuleStack transferer = this.game.ModuleStacks["100001"];
			ModuleStack receiver = this.game.ModuleStacks["100002"];
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"transfer all damaged modules to 100002",
				"#end"
			});

			TransferOrder parsed = (TransferOrder)transferer.Orders[0];
			Assert.That(parsed.TransferAll, Is.True);
			Assert.That(parsed.DamagedOnly, Is.True);
			Assert.That(parsed.Receiver, Is.EqualTo(receiver));
		}

		[Test]
		public void Parse_TransferModuleIndexToStack()
		{
			ModuleStack transferer = this.game.ModuleStacks["100001"];
			ModuleStack receiver = this.game.ModuleStacks["100002"];
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"transfer module 2 to 100002",
				"#end"
			});

			TransferOrder parsed = (TransferOrder)transferer.Orders[0];
			Assert.That(parsed.Index, Is.EqualTo(2));
			Assert.That(parsed.Quantity, Is.EqualTo(1));
			Assert.That(parsed.Receiver, Is.EqualTo(receiver));
		}

		[Test]
		public void Parse_TransferToFaction()
		{
			ModuleStack transferer = this.game.ModuleStacks["100001"];
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"transfer 1 to faction 1",
				"#end"
			});

			TransferOrder parsed = (TransferOrder)transferer.Orders[0];
			Assert.That(parsed.Quantity, Is.EqualTo(1));
			Assert.That(parsed.ReceiverFaction, Is.EqualTo(this.game.Factions["1"]));
			Assert.That(parsed.Receiver, Is.Null);
		}

		[Test]
		public void Execute_TransferAll_EmptiesSourceStack()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack receiver = new ModuleStack(region, owner, ModuleType.All["inftry"], "allrcv");
			receiver.AddModule();
			ModuleStack source = new ModuleStack(region, owner, ModuleType.All["inftry"], "allsrc");
			source.AddModules(3);

			TransferOrder order = new TransferOrder(source, receiver, ModuleType.All["inftry"], 0, 0);
			order.TransferAll = true;
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			Assert.That(source.Quantity, Is.EqualTo(0));
			Assert.That(ModuleStack.All.ContainsKey("allsrc"), Is.False);
			Assert.That(receiver.Quantity, Is.EqualTo(4));
		}

		[Test]
		public void Execute_TransferDamagedOnly_SkipsHealthyModules()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack receiver = new ModuleStack(region, owner, ModuleType.All["inftry"], "dmgrcv");
			ModuleStack source = new ModuleStack(region, owner, ModuleType.All["inftry"], "dmgsrc");
			source.AddModule();
			source.AddModule(new Module(source, source.ModuleType.DamageCapacity / 2));
			source.AddModule();

			TransferOrder order = new TransferOrder(source, receiver, ModuleType.All["inftry"], 0, 0);
			order.TransferAll = true;
			order.DamagedOnly = true;
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			Assert.That(source.Quantity, Is.EqualTo(2));
			Assert.That(receiver.Quantity, Is.EqualTo(1));
		}

		[Test]
		public void Execute_TransferModuleByIndex()
		{
			Region region = Region.All["R00002"];
			Faction owner = this.game.Factions["2"];
			ModuleStack receiver = new ModuleStack(region, owner, ModuleType.All["inftry"], "idxrcv");
			ModuleStack source = new ModuleStack(region, owner, ModuleType.All["inftry"], "idxsrc");
			source.AddModules(3);
			Module marked = source.Modules[1];
			marked.Damage = marked.HitPoints / 2;

			TransferOrder order = new TransferOrder(source, receiver, 2);
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			Assert.That(source.Quantity, Is.EqualTo(2));
			Assert.That(receiver.Quantity, Is.EqualTo(1));
			Assert.That(receiver.Modules[0].Damage, Is.EqualTo(marked.HitPoints / 2));
		}

		[Test]
		public void Execute_TransferToFaction_ChangesOwnerAndPlacesAtLocation()
		{
			Region region = Region.All["R00002"];
			Faction player = this.game.Factions["2"];
			Faction npc = this.game.Factions["1"];
			ModuleStack source = new ModuleStack(region, player, ModuleType.All["inftry"], "facsrc");
			source.AddModule();

			TransferOrder order = new TransferOrder(source, null, ModuleType.All["inftry"], 1, 0);
			order.ReceiverFaction = npc;
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			Assert.That(ModuleStack.All.ContainsKey("facsrc"), Is.False);
			ModuleStack delivered = null;
			foreach (ModuleStack stack in region.ModuleStacks.Values)
			{
				if (stack.Owner == npc && stack.ModuleType != null && stack.ModuleType.Name == "inftry")
				{
					delivered = stack;
					break;
				}
			}
			Assert.That(delivered, Is.Not.Null);
			Assert.That(delivered.Quantity, Is.EqualTo(1));
			Assert.That(delivered.Parent, Is.EqualTo(region));
		}

		[Test]
		public void Execute_TransferToFaction_CompletesGiveModuleContractWithoutLocalReceiver()
		{
			Region region = Region.All["R00002"];
			Faction player = this.game.Factions["2"];
			Faction npc = this.game.Factions["1"];
			ModuleStack source = new ModuleStack(region, player, ModuleType.All["inftry"], "ctsrc");
			source.AddModule();
			GiveModuleTrigger trigger = new GiveModuleTrigger(
				1,
				ModuleType.All["inftry"],
				ModuleStack.All["000001"]);
			new Contract("CT9001", region, npc, trigger, Technology.All["rckter"]);

			TransferOrder order = new TransferOrder(source, null, ModuleType.All["inftry"], 1, 0);
			order.ReceiverFaction = npc;
			order.Execute(1);
			Contract.All.Evaluate(1);

			Assert.That(Contract.All.Count, Is.EqualTo(0));
			Assert.That(player.TechnologiesToShow.Contains("rckter"), Is.True);
		}
	}
}
