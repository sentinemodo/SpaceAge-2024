using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBelt : TTest
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
		public void LoadGalaxy_Belt_LoadsCompositionWithoutOrbitOrRegions()
		{
			this.LoadBeltWorld();
			Assert.That(Planet.All.ContainsKey("P00003"), Is.False);
			Belt belt = Belt.All["P00003"];
			Assert.That(belt.FullName, Is.EqualTo("Harbor Belt"));
			Assert.That(belt.AU, Is.EqualTo(2.7).Within(0.0000001));
			Assert.That(belt.Planet, Is.Null);
			Assert.That(belt, Is.Not.InstanceOf<IOrbitHolder>());
			Assert.That(belt.Composition.Count, Is.EqualTo(2));
			Assert.That(belt.Composition[0].ItemType.Name, Is.EqualTo("uraniu"));
			Assert.That(belt.Composition[0].Quantity, Is.EqualTo(80));
			Assert.That(belt.Composition[0].Probability, Is.EqualTo(0.5).Within(0.0000001));
			Assert.That(Region.All.ContainsKey("R00096"), Is.False);
			Assert.That(Region.All["R00001"].Exits.Contains(belt), Is.True);
			Assert.That(Region.All["R00001"].Exits[belt].ExitModes[EMoveMode.space].Duration, Is.EqualTo(13));
			Assert.That(belt.Exits.Contains(Region.All["R00001"]), Is.True);
		}

		[Test]
		public void LoadGalaxy_RingBelt_LoadsOnGasGiant()
		{
			this.LoadBeltWorld();
			Planet giant = Planet.All["P00002"];
			Belt ring = Belt.All["P00021"];
			Assert.That(ring.Planet, Is.EqualTo(giant));
			Assert.That(ring.AU, Is.EqualTo(0.002).Within(0.0000001));
			Assert.That(ring.Composition[0].ItemType.Name, Is.EqualTo("water"));
			Assert.That(ring, Is.Not.InstanceOf<IOrbitHolder>());
		}

		[Test]
		public void SaveGame_PersistsBeltCompositionAndExits()
		{
			this.LoadBeltWorld();
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_belt.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elBelt = (XmlElement)saved.SelectSingleNode("//system/belt[@name='P00003']");
			Assert.That(elBelt, Is.Not.Null);
			Assert.That(elBelt.GetAttribute("AU"), Is.EqualTo("2.7"));
			Assert.That(elBelt.SelectSingleNode("orbit"), Is.Null);
			Assert.That(elBelt.SelectSingleNode("region"), Is.Null);
			XmlElement elResource = (XmlElement)elBelt.SelectSingleNode("composition/resource[@type='uraniu']");
			Assert.That(elResource, Is.Not.Null);
			Assert.That(elResource.GetAttribute("quantity"), Is.EqualTo("80"));
			Assert.That(elResource.GetAttribute("probability"), Is.EqualTo("0.5"));
			Assert.That(elBelt.SelectSingleNode("exit[@region='R00001']/exitmode[@mode='space']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//region[@name='R00001']/exit[@belt='P00003']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//planet[@name='P00002']/belt[@name='P00021']"), Is.Not.Null);
		}

		[Test]
		public void ExecuteMoveOrder_SpaceToBelt_UsesExitDuration()
		{
			this.LoadBeltWorld();
			ModuleStack ship = ModuleStack.All["s00001"];
			this.AssignMove(ship, "P00003");
			MoveOrder order = (MoveOrder)ship.Orders[0];

			ship.ExecutedLongOrder = false;
			ship.Orders.Execute(this.game.Week);

			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.space));
			Assert.That(order.DurationLeft, Is.EqualTo(12));
			Assert.That(ship.MovingTo, Is.EqualTo(Belt.All["P00003"]));
			Assert.That(ship.Parent, Is.EqualTo(Region.All["R00001"]));

			for (int week = 0; week < 12; week++)
			{
				ship.ExecutedLongOrder = false;
				ship.Orders.Execute(this.game.Week);
			}

			Assert.That(ship.MovingTo, Is.Null);
			Assert.That(ship.Parent, Is.EqualTo(Belt.All["P00003"]));
			Assert.That(order.Executed, Is.True);
		}

		private string BeltFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "belt");
		}

		private void LoadBeltWorld()
		{
			string fixtureDir = this.BeltFixtureDir();
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
			reader.AssignOrders(new System.Collections.Generic.List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				"move " + destination,
				"#end"
			});
		}
	}
}
