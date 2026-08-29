using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TAlderson : TTest
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
		public void LoadGalaxy_Alderson_LoadsOrbitWithoutRegions()
		{
			this.LoadAldersonWorld();
			Assert.That(Planet.All.ContainsKey("P00009"), Is.False);
			Assert.That(Planet.All.ContainsKey("P00010"), Is.False);
			Assert.That(Region.All.ContainsKey("R01490"), Is.False);
			Alderson gate = Alderson.All["P00009"];
			Assert.That(gate.FullName, Is.EqualTo("Helios Gate"));
			Assert.That(gate.AU, Is.EqualTo(80).Within(0.0000001));
			Assert.That(gate.PairName, Is.EqualTo("P00010"));
			Assert.That(gate.Orbit, Is.Not.Null);
			Assert.That(gate.Orbit.Name, Is.EqualTo("O00110"));
			Assert.That(gate, Is.InstanceOf<IOrbitHolder>());
			Assert.That(Region.All["R00001"].Exits.Contains(gate.Orbit), Is.True);
			Assert.That(Region.All["R00001"].Exits[gate.Orbit].ExitModes[EMoveMode.space].Duration, Is.EqualTo(13));
			Assert.That(gate.Orbit.Exits.Contains(Region.All["R00001"]), Is.True);
			Assert.That(Alderson.All["P00010"].PairName, Is.EqualTo("P00009"));
		}

		[Test]
		public void SaveGame_PersistsAldersonPairAndExits()
		{
			this.LoadAldersonWorld();
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_alderson.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elGate = (XmlElement)saved.SelectSingleNode("//system/alderson[@name='P00009']");
			Assert.That(elGate, Is.Not.Null);
			Assert.That(elGate.GetAttribute("AU"), Is.EqualTo("80"));
			Assert.That(elGate.GetAttribute("pair"), Is.EqualTo("P00010"));
			Assert.That(elGate.SelectSingleNode("orbit[@name='O00110']"), Is.Not.Null);
			Assert.That(elGate.SelectSingleNode("region"), Is.Null);
			Assert.That(elGate.SelectSingleNode("exit[@region='R00001']/exitmode[@mode='space']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//region[@name='R00001']/exit[@alderson='P00009']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//planet[@name='P00009']"), Is.Null);
		}

		[Test]
		public void ExecuteMoveOrder_SpaceToAlderson_UsesExitDuration()
		{
			this.LoadAldersonWorld();
			ModuleStack ship = ModuleStack.All["s00001"];
			this.AssignOrders(ship, "move P00009");
			MoveOrder order = (MoveOrder)ship.Orders[0];

			ship.ExecutedLongOrder = false;
			ship.Orders.Execute(this.game.Week);

			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.space));
			Assert.That(order.DurationLeft, Is.EqualTo(12));
			Assert.That(ship.MovingTo, Is.EqualTo(Alderson.All["P00009"].Orbit));
			Assert.That(ship.Parent, Is.EqualTo(Region.All["R00001"]));

			for (int week = 0; week < 12; week++)
			{
				ship.ExecutedLongOrder = false;
				ship.Orders.Execute(this.game.Week);
			}

			Assert.That(ship.MovingTo, Is.Null);
			Assert.That(ship.Parent, Is.EqualTo(Alderson.All["P00009"].Orbit));
			Assert.That(order.Executed, Is.True);
		}

		[Test]
		public void ExecuteJumpOrder_PairedAlderson_ArrivesAtPairOrbit()
		{
			this.LoadAldersonWorld();
			ModuleStack ship = ModuleStack.All["s00002"];
			Assert.That(ship.Parent, Is.EqualTo(Alderson.All["P00009"].Orbit));
			this.AssignOrders(ship, "jump P00010");
			JumpOrder order = (JumpOrder)ship.Orders[0];

			ship.ExecutedLongOrder = false;
			ship.Orders.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			Assert.That(ship.Parent, Is.EqualTo(Alderson.All["P00010"].Orbit));
			Assert.That(ship.MovingTo, Is.Null);
		}

		private string AldersonFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "alderson");
		}

		private void LoadAldersonWorld()
		{
			string fixtureDir = this.AldersonFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private void AssignOrders(ModuleStack stack, string command)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				command,
				"#end"
			});
		}
	}
}
