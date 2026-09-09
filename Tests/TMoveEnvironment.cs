using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMoveEnvironment : TTest
	{
		[SetUp]
		public void setup()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.game = new Game();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void ExecuteMoveOrder_ShuttleSurfaceToOrbit_TerairConsumesEightH2o2()
		{
			this.LoadEnvironmentWorld();
			ModuleStack shuttle = ModuleStack.All["s00001"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00001");

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			Assert.That(shuttle.Parent, Is.EqualTo(Orbit.All["O00001"]));
			Assert.That(shuttle.ItemStacks.Quantity("h2o2"), Is.EqualTo(12));
			Assert.That(this.HasEvent(shuttle, "consumed"), Is.True);
			Assert.That(this.HasEvent(shuttle, "h2o2"), Is.True);
		}

		[Test]
		public void ExecuteMoveOrder_ShuttleOrbitToSurface_TerairConsumesEightH2o2()
		{
			this.LoadEnvironmentWorld();
			ModuleStack shuttle = ModuleStack.All["s00006"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "R00001");

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			Assert.That(shuttle.Parent, Is.EqualTo(Region.All["R00001"]));
			Assert.That(shuttle.ItemStacks.Quantity("h2o2"), Is.EqualTo(12));
		}

		[Test]
		public void ExecuteMoveOrder_ShuttleSurfaceToOrbit_ShortH2o2Fails()
		{
			this.LoadEnvironmentWorld();
			ModuleStack shuttle = ModuleStack.All["s00001"];
			shuttle.ItemStacks.Minus(ItemType.All["h2o2"], 17);
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00001");
			MoveOrder order = (MoveOrder)shuttle.Orders[0];

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			Assert.That(shuttle.Parent, Is.EqualTo(Region.All["R00001"]));
			Assert.That(shuttle.MovingTo, Is.Null);
			Assert.That(order.Executed, Is.True);
			Assert.That(shuttle.ItemStacks.Quantity("h2o2"), Is.EqualTo(3));
			Assert.That(this.HasEvent(shuttle, "MOVE failed"), Is.True);
			Assert.That(this.HasEvent(shuttle, "h2o2"), Is.True);
		}

		[Test]
		public void ExecuteMoveOrder_ShuttleSurfaceToOrbit_VacuumMoonConsumesZero()
		{
			this.LoadEnvironmentWorld();
			ModuleStack shuttle = ModuleStack.All["s00003"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00002");

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			Assert.That(shuttle.Parent, Is.EqualTo(Orbit.All["O00002"]));
			Assert.That(shuttle.ItemStacks.Quantity("h2o2"), Is.EqualTo(5));
		}

		[Test]
		public void ExecuteMoveOrder_ShuttleSurfaceToOrbit_OmittedAttrsUncharged()
		{
			this.LoadEnvironmentWorld();
			ModuleStack shuttle = ModuleStack.All["s00004"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00003");

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			Assert.That(shuttle.Parent, Is.EqualTo(Orbit.All["O00003"]));
			Assert.That(shuttle.ItemStacks.Quantity("h2o2"), Is.EqualTo(20));
			Assert.That(Planet.All["P00003"].HasEnvironmentAttrs, Is.False);
		}

		[Test]
		public void ExecuteMoveOrder_FrigateOrbitToSurface_AtmosphereBanned()
		{
			this.LoadEnvironmentWorld();
			ModuleStack frigate = ModuleStack.All["s00002"];
			this.AssignMove(frigate, "R00001");
			MoveOrder order = (MoveOrder)frigate.Orders[0];

			frigate.ExecutedLongOrder = false;
			frigate.Orders.Execute(this.game.Week);

			Assert.That(frigate.Parent, Is.EqualTo(Orbit.All["O00001"]));
			Assert.That(frigate.MovingTo, Is.Null);
			Assert.That(order.Executed, Is.True);
			Assert.That(this.HasEvent(frigate, "MOVE failed"), Is.True);
			Assert.That(this.HasEvent(frigate, "atmosphere"), Is.True);
		}

		[Test]
		public void ExecuteMoveOrder_FrigateOrbitToSurface_VacuumMoonAllowed()
		{
			this.LoadEnvironmentWorld();
			ModuleStack frigate = ModuleStack.All["s00005"];
			this.Prefuel(ModuleStack.All["s00052"]);
			this.AssignMove(frigate, "R00002");

			frigate.ExecutedLongOrder = false;
			frigate.Orders.Execute(this.game.Week);

			Assert.That(frigate.Parent, Is.EqualTo(Region.All["R00002"]));
			Assert.That(frigate.MovingTo, Is.Null);
		}

		[Test]
		public void SaveGame_WritesEnvironmentAttrsOnlyWhenEmitted()
		{
			this.LoadEnvironmentWorld();
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_environment.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement arbor = (XmlElement)saved.SelectSingleNode("//planet[@name='P00001']");
			XmlElement bare = (XmlElement)saved.SelectSingleNode("//planet[@name='P00003']");
			Assert.That(arbor.GetAttribute("gravity"), Is.EqualTo("normal"));
			Assert.That(arbor.GetAttribute("atmosphere"), Is.EqualTo("terair"));
			Assert.That(bare.HasAttribute("gravity"), Is.False);
			Assert.That(bare.HasAttribute("atmosphere"), Is.False);
		}

		private string EnvironmentFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "environment");
		}

		private void LoadEnvironmentWorld()
		{
			string fixtureDir = this.EnvironmentFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private void AssignMove(ModuleStack stack, string destination)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				"move " + destination,
				"#end"
			});
		}

		private void Prefuel(ModuleStack stack)
		{
			new Fuelled(stack, 13);
		}

		private bool HasEvent(ModuleStack stack, string fragment)
		{
			return stack.EventReports.Exists(eventReport => eventReport.Description.IndexOf(fragment) >= 0);
		}
	}
}
