using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TPersonOrdersTemplate : TTest
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
		public void OrderPersonReferenceToken_UnformedTrainee_UsesNewAlias()
		{
			Faction faction = this.game.Factions["2"];
			Person trainee = new Person(faction, "new7");
			ClassicAssert.AreNotEqual("new7", trainee.Name);
			ClassicAssert.AreEqual("new7", trainee.OrderPersonReferenceToken());
		}

		[Test]
		public void IncludeInOrdersTemplate_OmitsUnformedWithoutOrders()
		{
			Faction faction = this.game.Factions["2"];
			Person ghost = new Person(faction, "000001");
			ClassicAssert.IsFalse(ghost.IncludeInOrdersTemplate(faction));
		}

		[Test]
		public void IncludeInOrdersTemplate_IncludesFormedCeoWithoutOrders()
		{
			Person ceo = this.game.People["000101"];
			Assert.That(ceo, Is.Not.Null, "formed CEO 000101 must exist in Tests/gamein.xml");
			Assert.That(ceo.IsFormed, Is.True);
			Assert.That(ceo.Orders.Count, Is.EqualTo(0));
			ClassicAssert.IsTrue(ceo.IncludeInOrdersTemplate(ceo.Owner));
		}

		[Test]
		public void HasOrder_Parse_DoesNotCreatePerson()
		{
			int before = Person.All.Count;
			Faction faction = this.game.Factions["2"];
			ModuleStack stack = this.game.ModuleStacks["100001"];
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + faction.Name,
				"#modulestack " + stack.Name,
				"has person 000001",
				"#end"
			});
			ClassicAssert.AreEqual(before, Person.All.Count);
			HasOrder order = (HasOrder)stack.Orders[0];
			ClassicAssert.IsNull(order.Person);
			ClassicAssert.AreEqual("000001", order.PersonName);
		}

		[Test]
		public void TrainOrder_Report_UnformedOfficer_MatchesPersonTemplateAlias()
		{
			Sequence.Ints.Push(102209);
			Faction faction = this.game.Factions["2"];
			ModuleStack hq = this.game.ModuleStacks["000112"];

			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + faction.Name,
				"#modulestack " + hq.Name,
				"train terran officer as new1",
				"#end"
			});

			TrainOrder train = (TrainOrder)hq.Orders[0];
			string trainLine = train.Report(faction)[0];
			ClassicAssert.IsTrue(trainLine.Contains("train terran officer as new1"));
			ClassicAssert.IsFalse(trainLine.Contains("train terran officer as new102209"));
			ClassicAssert.AreEqual("new1", train.Officer.OrderPersonReferenceToken());
		}

		[Test]
		public void OrdersTemplate_TraineeWithMove_UsesNewAlias()
		{
			Sequence.Ints.Push(902209);
			Faction faction = this.game.Factions["2"];
			ModuleStack hq = this.game.ModuleStacks["000112"];

			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + faction.Name,
				"#modulestack " + hq.Name,
				"train terran officer as new902209",
				"#person new902209",
				"move R00002",
				"#end"
			});

			TrainOrder train = (TrainOrder)hq.Orders[0];
			Person trainee = train.Officer;
			ClassicAssert.AreSame(trainee, trainee.Orders[0].Subject);

			foreach (Faction f in this.game.Factions.Values)
			{
				f.Options.XmlReport = false;
			}

			string turnDir = Path.Combine(Directory.GetCurrentDirectory(), "person-template-test");
			Directory.CreateDirectory(turnDir);
			try
			{
				ReportWriter writer = new ReportWriter(this.game, this.dataFile, turnDir);
				writer.GenerateReports(turnDir);

				string reportPath = Path.Combine(turnDir, string.Format("report.{0}.{1}.txt", this.game.Turn, faction.Name));
				string text = File.ReadAllText(reportPath, System.Text.Encoding.GetEncoding(1251));

				ClassicAssert.IsTrue(text.Contains("train terran officer as new902209"));
				ClassicAssert.IsTrue(text.Contains("#person new902209"));
				ClassicAssert.IsTrue(text.Contains("; + unformed officer [new902209]."));
				ClassicAssert.IsFalse(text.Contains("#person " + trainee.Name + Environment.NewLine));
				ClassicAssert.IsFalse(text.Contains("#person 000001" + Environment.NewLine));
			}
			finally
			{
				if (Directory.Exists(turnDir))
				{
					Directory.Delete(turnDir, true);
				}
			}
		}
	}
}
