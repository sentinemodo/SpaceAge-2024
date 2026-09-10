using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMoveMode : TTest
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
		public void MoveModeXml_Parse_KnownModes()
		{
			Assert.That(MoveModeXml.Parse("ground"), Is.EqualTo(EMoveMode.ground));
			Assert.That(MoveModeXml.Parse("space"), Is.EqualTo(EMoveMode.space));
			Assert.That(MoveModeXml.Parse("naval"), Is.EqualTo(EMoveMode.naval));
		}

		[Test]
		public void MoveModeXml_Parse_UnknownMode_Throws()
		{
			Assert.Throws<FileLoadException>(() => MoveModeXml.Parse("hover"));
		}

		[Test]
		public void LoadGalaxy_NavalExitMode_ParsesDuration()
		{
			this.LoadNavalWorld();
			Assert.That(Region.All["R00001"].Exits[0].ExitModes.ContainsKey(EMoveMode.naval), Is.True);
			Assert.That(Region.All["R00001"].Exits[0].ExitModes[EMoveMode.naval].Duration, Is.EqualTo(4));
			Assert.That(Region.All["R00001"].Exits[0].ExitModes.ContainsKey(EMoveMode.ground), Is.False);
		}

		[Test]
		public void LoadGalaxy_UnknownExitMode_Throws()
		{
			string fixtureDir = this.NavalFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.unknown-exit.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.dataFile.LoadFactions();
			Assert.Throws<Exception>(() => this.dataFile.LoadGalaxy());
		}

		[Test]
		public void LoadConfiguration_NavalModuleMove_ParsesMode()
		{
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.LoadExtraCatalog("data-naval-module.xml");
			Assert.That(ModuleType.All["coastr"].MoveModes.ContainsKey(EMoveMode.naval), Is.True);
			Assert.That(ModuleType.All["coastr"].MoveModes[EMoveMode.naval].Speed, Is.EqualTo(1));
			Assert.That(ModuleType.All["coastr"].MoveModes.ContainsKey(EMoveMode.ground), Is.False);
		}

		[Test]
		public void LoadConfiguration_UnknownModuleMove_Throws()
		{
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			Assert.Throws<Exception>(() => this.LoadExtraCatalog("data-unknown-move.xml"));
		}

		[Test]
		public void SaveGame_PersistsNavalExitMode()
		{
			this.LoadNavalWorld();
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_navalExit.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elExitMode = (XmlElement)saved.SelectSingleNode("//region[@name='R00001']/exit/exitmode");
			Assert.That(elExitMode, Is.Not.Null);
			Assert.That(elExitMode.GetAttribute("mode"), Is.EqualTo("naval"));
			Assert.That(elExitMode.GetAttribute("duration"), Is.EqualTo("4"));
		}

		[Test]
		public void ExecuteMoveOrder_NavalSameBody_UsesNavalDuration()
		{
			this.LoadNavalWorld();
			ModuleStack boat = this.CreateBoat("n00001", Faction.All["2"], Region.All["R00001"]);
			this.AssignMove(boat, "R00002");
			MoveOrder order = (MoveOrder)boat.Orders[0];

			boat.ExecutedLongOrder = false;
			boat.Orders.Execute(this.game.Week);

			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.naval));
			Assert.That(order.DurationLeft, Is.EqualTo(3));
			Assert.That(boat.MovingTo, Is.EqualTo(Region.All["R00002"]));
			Assert.That(boat.Parent, Is.EqualTo(Region.All["R00001"]));

			for (int week = 0; week < 3; week++)
			{
				boat.ExecutedLongOrder = false;
				boat.Orders.Execute(this.game.Week);
			}

			Assert.That(boat.MovingTo, Is.Null);
			Assert.That(boat.Parent, Is.EqualTo(Region.All["R00002"]));
			Assert.That(order.Executed, Is.True);
		}

		[Test]
		public void ExecuteMoveOrder_GroundMoverOnNavalExit_DoesNotMove()
		{
			this.LoadNavalWorld();
			ModuleStack trucks = new ModuleStack(Region.All["R00001"], Faction.All["2"], ModuleType.All["trucks"], "t00001");
			trucks.AddModules(1);
			this.AssignMove(trucks, "R00002");

			trucks.ExecutedLongOrder = false;
			trucks.Orders.Execute(this.game.Week);

			Assert.That(trucks.MovingTo, Is.Null);
			Assert.That(trucks.Parent, Is.EqualTo(Region.All["R00001"]));
		}

		private string NavalFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "naval");
		}

		private void LoadNavalWorld()
		{
			string fixtureDir = this.NavalFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.LoadExtraCatalog("data-naval-module.xml");
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private void LoadExtraCatalog(string fileName)
		{
			XmlDocument extra = new XmlDocument();
			extra.Load(Path.Combine(this.NavalFixtureDir(), fileName));
			CatalogLoader loader = new CatalogLoader(this.dataFile);
			loader.LoadItems(extra, this.dataFile.Game, true);
			loader.LoadItems(extra, this.dataFile.Game, false);
		}

		private ModuleStack CreateBoat(string name, Faction owner, IHolder parent)
		{
			ModuleStack boat = new ModuleStack(parent, owner, ModuleType.All["coastr"], name);
			boat.AddModules(1);
			return boat;
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
