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
	}
}
