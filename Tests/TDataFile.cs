using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

using SpaceAge;
using System.Xml;
using NUnit.Framework.Legacy;

namespace UnitTests
{
	[TestFixture]
	public class TDataFile : TTest, IDisposable
	{

		private DataFile dataFile;

        public TextReader TextReader { get; set; }
        public void Dispose()
        {
            if (this.TextReader != null)
                this.TextReader.Dispose();
        }

        public TDataFile()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.game = new Game();
		}

		[TearDown]
		public void tearDownDataFile()
		{
			this.game.ClearDictionaries();
		}

		[Test]
		public void SetupTeardown()
		{
			ClassicAssert.IsTrue(true);
		}

		[Test]
		public void LoadGameData_nullparameter()
		{
			Assert.Throws<ArgumentNullException>(
				delegate
				{
					this.dataFile = new DataFile(null);
					this.dataFile.LoadGame();
				});
		}

		[Test]
		public void LoadGameDocument()
		{
			// method changes private element, should pass without assertions			
			this.dataFile.LoadGameDocument();
		}

		[Test]
		public void LoadFactions()
		{
			this.LoadGameDocument();
			this.dataFile.LoadFactions();
			this.game = this.dataFile.Game;
			ClassicAssert.IsNotNull(this.game.Factions);
			ClassicAssert.AreEqual(2, this.game.Factions.Count);
			ClassicAssert.AreEqual("1", this.game.Factions["1"].Name);
			ClassicAssert.AreEqual("NPC", this.game.Factions["1"].FullName);
			ClassicAssert.AreEqual("2", this.game.Factions["2"].Name);
			ClassicAssert.AreEqual("Caste Prime", this.game.Factions["2"].FullName);
		}

		[Test]
		public void LoadConfDocument()
		{
			// method changes private element, should pass without assertions			
			this.dataFile.LoadConfDocument(Directory.GetCurrentDirectory());
		}

		[Test]
		public void LoadGameData_invalidOperation()
		{
			Assert.Throws<InvalidOperationException>(
				delegate
				{
					this.dataFile.LoadGame();
				});
		}


		[Test]
		public void LoadGalaxy()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
			ClassicAssert.IsNotNull(this.game.Galaxy);
			ClassicAssert.AreEqual(2, this.game.Galaxy.SpaceSystems.Count);
			ClassicAssert.AreEqual("Sol", this.game.Galaxy.SpaceSystems[0].FullName);
			ClassicAssert.AreEqual("Proxima Centauri", this.game.Galaxy.SpaceSystems[1].FullName);

			SpaceSystem system = this.game.Galaxy.SpaceSystems[0];
			ClassicAssert.IsNotNull(system.Objects);
			ClassicAssert.AreEqual(3, system.Objects.Count);
			ClassicAssert.IsInstanceOf(typeof(Star), system.Objects["S00001"]);
			ClassicAssert.IsInstanceOf(typeof(Planet), system.Objects["P00001"]);
		}

		[Test]
		public void LoadConfiguration()
		{
			this.dataFile.LoadConfiguration();
		}

		[Test]
		public void LoadExits()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
			Region region = Region.All["R00001"];
			ClassicAssert.IsNotNull(region);
			ClassicAssert.AreEqual(1, region.Exits.Count);
			ClassicAssert.AreEqual(Region.All["R00002"], region.Exits[0].To);
			ClassicAssert.IsNotNull(region.Exits[0].ExitModes[EMoveMode.ground]);
			ClassicAssert.AreEqual(3, region.Exits[0].ExitModes[EMoveMode.ground].Duration);
		}

		[Test]
		public void LoadItems()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			ItemType itemtype = this.game.ItemTypes["iron"];
			ClassicAssert.IsNotNull(itemtype);
			ClassicAssert.AreEqual("iron", itemtype.Name);
			ClassicAssert.AreEqual("unit of iron", itemtype.FullName);
			ClassicAssert.AreEqual("units of iron", itemtype.FullNameMultiple);
			ClassicAssert.AreEqual("unit of iron [iron]", itemtype.ReportName);
			ClassicAssert.AreEqual("units of iron [iron]", itemtype.ReportNameMultiple);
		}

		[Test]
		public void LoadPerson()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
			ItemType itemType = ItemType.All["terran"];
			ClassicAssert.IsNotNull(itemType);
			ClassicAssert.AreEqual(EItemTypesGroup.crew, itemType.Group);
			Race race = Race.All["terran"];
			ClassicAssert.IsNotNull(race);
			ClassicAssert.AreEqual(4, race.Size);
			ClassicAssert.AreEqual(4, race.Mass);
			ClassicAssert.AreEqual(3, race.Capacity);
			Person person = Person.All["200001"];
			ClassicAssert.IsNotNull(person);
			ClassicAssert.AreEqual(race, person.Race);			
		}

		[Test]
		public void LoadUpkeep()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			ModuleType moduleType = ModuleType.All["cdrill"];
			ClassicAssert.IsNotNull(moduleType);
			ItemType itemType = ItemType.All["cash"];
			ClassicAssert.AreEqual(40, moduleType.Upkeep[itemType].Quantity);

			ItemType crewType = ItemType.All["terran"];
			ClassicAssert.AreEqual(1, crewType.Upkeep[itemType].Quantity);

			ItemStack crewStack = new ItemStack(crewType, 6);
			ClassicAssert.AreEqual(6, crewStack.Quantity);
			ClassicAssert.AreEqual(6, crewStack.Upkeep[itemType].Quantity);

			ItemStacks itemStacks = new ItemStacks();
			itemStacks.Add(crewStack);
			ClassicAssert.AreEqual(6, itemStacks[crewType].Quantity);

			ItemStack itemStack2 = itemStacks[crewType];
			ClassicAssert.AreEqual(6, itemStack2.Quantity);
			ClassicAssert.AreEqual(6, itemStack2.Upkeep[itemType].Quantity);
			ClassicAssert.AreEqual(6, itemStacks[crewType].Upkeep[itemType].Quantity);

			ModuleStack moduleStack = new ModuleStack(null, null, moduleType, "000000");
			moduleStack.AddModule();
			moduleStack.ItemStacks.Add(crewStack);
			ClassicAssert.AreEqual(40, moduleStack.UpkeepNetto[itemType].Quantity);
			ClassicAssert.AreEqual(6, moduleStack.ItemStacks[crewType].Quantity);
			ClassicAssert.AreEqual(1, moduleStack.ItemStacks[crewType].Upkeep.Count);
			ClassicAssert.AreEqual(6, moduleStack.ItemStacks[crewType].Upkeep[itemType].Quantity);
			ClassicAssert.AreEqual(40 + 6, moduleStack.Upkeep[itemType].Quantity);
		}

		[Test]
		public void LoadTechnologyConsumeItems()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			Technology technology = Technology.All["agrplx"];
			ClassicAssert.IsNotNull(technology);
			ItemType itemType = ItemType.All["iron"];

			ClassicAssert.AreEqual(1, technology.UseConsumeItems.Count);
			ClassicAssert.AreEqual(10, technology.UseConsumeItems[itemType].Quantity);
		}

		[Test]
		public void LoadModuleStackTechnology()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
			ModuleStack moduleStack = ModuleStack.All["000006"];			
			ClassicAssert.IsNotNull(moduleStack);
			ClassicAssert.AreEqual(1, moduleStack.Technologies.Count);
			Technology technology = Technology.All["hcdril"];
			ClassicAssert.IsNotNull(technology, "technology is not loaded");
			ClassicAssert.IsNotNull(moduleStack.Technologies["hcdril"], "modulestack has not loaded the echnology");
		}

		[Test]
		public void LoadDoubleModuleStack()
		{
			Assert.Throws<Exception>(
				delegate
				{
					this.dataFile.LoadGameDocument(Directory.GetCurrentDirectory(), "gamein.double.xml");
					this.dataFile.LoadConfiguration();
					this.dataFile.LoadFactions();
					try
					{
						this.dataFile.LoadGalaxy();
					}
					catch (Exception ex)
					{
						ClassicAssert.AreEqual(
                            "Modulestack with name [000001] already exists",
                            ex.InnerException.InnerException.Message);
						throw ex;
					}
					this.consoleOutReport("loaded region", Region.All["R10001"], Faction.All["1"]);
				});
        }

		[Test]
		public void LoadRaceUpkeep()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			Race race = Race.All["terran"];
			ClassicAssert.IsNotNull(race);
			ItemType itemType = ItemType.All["cash"];
			ClassicAssert.AreEqual(10, race.Upkeep[itemType].Quantity);
		}

		[Test]
		public void LoadOrbit()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Race race = Race.All["terran"];
			ClassicAssert.IsNotNull(race);
			Orbit orbit = Orbit.All["O00003"];
			ClassicAssert.AreEqual(1, orbit.Races.Count);

		}

		[Test]
		public void LoadPlanet()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Planet planet = Planet.All["P00002"];
			ClassicAssert.IsNotNull(planet);
			ClassicAssert.AreEqual(6, planet.SurfaceSizeX);
			ClassicAssert.AreEqual(4, planet.SurfaceSizeY);

		}

		[Test]
		public void LoadOrder()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadOrders();
			this.game = this.dataFile.Game;

			ModuleStack moduleStack = ModuleStack.All["100002"];
			ClassicAssert.IsNotNull(moduleStack);
			ClassicAssert.AreEqual(1, moduleStack.Orders.Count);
			
			MoveOrder order = (MoveOrder)moduleStack.Orders[0];
			ClassicAssert.AreEqual(2, order.Route.Count);

		}

		[Test, Ignore("not ready")]
		public void SaveLoadUseOrder_withUseOrderInProgress_sameOrder()
		{
			Assert.Fail("don't know");
		}
		
		[Test, Ignore("not ready")]
		public void SaveLoadUseOrder_withUseOrderInProgress_newOrder()
		{
			Assert.Fail("don't know");
		}

        [Test]
        public void SaveGetOrder()
        {
            this.LoadGameDocument();
            this.dataFile.LoadConfiguration();
            this.dataFile.LoadFactions();
            this.dataFile.LoadGalaxy();
            this.game = this.dataFile.Game;

            ModuleStack moduleStack = ModuleStack.All["100002"];
            ClassicAssert.IsNotNull(moduleStack);
            ClassicAssert.AreEqual(0, moduleStack.Orders.Count);

            Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100001");
			testcommands.Add("get 1 iron from 000006");
   			testcommands.Add("@get 1 iron from 000006");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			GetOrder order = (GetOrder) testModuleStack.Orders[0];
			ClassicAssert.AreEqual(1, order.Quantity);
			ClassicAssert.AreEqual("iron", order.ItemType.Name);
			ClassicAssert.AreEqual("core drill [000006]", order.Transferer.ReportName);
            ClassicAssert.AreEqual(1, order.Repeat);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<game/>"); 
            this.dataFile.SaveOrders(doc);

            string testdir = Directory.GetCurrentDirectory();
            string testfile = "gameout.saved_getorders.xml";
            XmlTextWriter xmlWriter = new XmlTextWriter(
                Path.Combine(testdir, testfile),
                System.Text.Encoding.GetEncoding(1251));
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.IndentChar = '\t';
            xmlWriter.Indentation = 1;
            xmlWriter.WriteStartDocument();
            doc.WriteContentTo(xmlWriter);
            xmlWriter.Close();

            List<string> testlines = new List<string>();
            testlines.Add("<?xml version=\"1.0\" encoding=\"windows-1251\"?>");
            testlines.Add("<game>");
            testlines.Add("	<orders>");
            testlines.Add("		<order subject=\"modulestack\" name=\"100001\">");
            testlines.Add("			<get item=\"iron\" quantity=\"1\" transferer=\"000006\" />");
            testlines.Add("		</order>");
            testlines.Add("		<order subject=\"modulestack\" name=\"100001\" repeat=\"unlimited\">");
            testlines.Add("			<get item=\"iron\" quantity=\"1\" transferer=\"000006\" />");
            testlines.Add("		</order>");
            testlines.Add("	</orders>");
            testlines.Add("</game>");
            this.TextReader = new StreamReader(
                Path.Combine(testdir, testfile), 
                System.Text.Encoding.GetEncoding(1251));
            List<string> generatedFile = new List<string>();
            string line;

            while ((line = this.TextReader.ReadLine()) != null)
                generatedFile.Add(line);

            for (int i = 0; i < testlines.Count; i++)
            {
                Console.WriteLine(testlines[i]);
                ClassicAssert.AreEqual(testlines[i], generatedFile[i]);
            }
            ClassicAssert.AreEqual(testlines.Count, generatedFile.Count);

        }

	}
}
