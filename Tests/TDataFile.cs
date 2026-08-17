using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

using SpaceAge;
using System.Xml;
using NUnit.Framework.Legacy;
using System.Threading;

namespace UnitTests
{
	[TestFixture]
	public class TDataFile : TTest, IDisposable
	{

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
            Assert.That(true);
		}

		[Test]
		public void ValidateTypeNameUniqueness_ThrowsWhenItemAndModuleShareName()
		{
			new ItemType("collide");
			new ModuleType("collide");
			Assert.Throws<FileLoadException>(() => this.dataFile.ValidateTypeNameUniqueness());
		}

		[Test]
		public void ValidateTypeNameUniqueness_PassesWhenNamesAreDistinct()
		{
			new ItemType("someitem");
			new ModuleType("somemodule");
			Assert.DoesNotThrow(() => this.dataFile.ValidateTypeNameUniqueness());
		}

		[Test]
		public void LoadConfiguration_LoadsResearchContent()
		{
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());

			// helium-3 resource item
			Assert.That(ItemType.All.ContainsKey("heliu3"));

			// advanced research complex module (stronger cmplib)
			ModuleType advlib = ModuleType.All["advlib"];
			Assert.That(advlib.Group, Is.EqualTo(EModuleTypesGroup.research));
			Assert.That(advlib.ResearchOutput, Is.EqualTo(2));

			// helium-3 mining: mirrors uminng - an extraction tech that yields the heliu3 item
			Technology he3min = Technology.All["he3min"];
			Assert.That(he3min.Level, Is.EqualTo(2));
			Assert.That(he3min.Cost, Is.EqualTo(16));
			Assert.That(he3min.HasTag("production"));
			Assert.That(he3min.Requires, Is.EqualTo(Technology.All["uminng"]));
			Assert.That(he3min.UseProduceItems.ContainsKey(ItemType.All["heliu3"]));

			// dedicated helium-3 extractor (a costlier, he3-only core drill) built by an L3 tech
			Assert.That(ModuleType.All["he3ext"].Group, Is.EqualTo(EModuleTypesGroup.extraction));
			Technology he3drl = Technology.All["he3drl"];
			Assert.That(he3drl.Level, Is.EqualTo(3));
			Assert.That(he3drl.Cost, Is.EqualTo(32));
			Assert.That(he3drl.HasTag("production"));
			Assert.That(he3drl.Requires, Is.EqualTo(Technology.All["he3min"]));
			Assert.That(he3drl.UseProduceModules.Name, Is.EqualTo("he3ext"));

			Technology advres = Technology.All["advres"];
			Assert.That(advres.Level, Is.EqualTo(3));
			Assert.That(advres.Cost, Is.EqualTo(32));
			Assert.That(advres.HasTag("research"));
			Assert.That(advres.Requires, Is.EqualTo(Technology.All["filidx"]));

			// tag added to an existing technology
			Assert.That(Technology.All["stnrdf"].HasTag("military"));

			ModuleType engshp = ModuleType.All["engshp"];
			Assert.That(engshp.Group, Is.EqualTo(EModuleTypesGroup.production));
			Assert.That(engshp.Size, Is.EqualTo(25));
			Assert.That(engshp.CrewRequired, Is.EqualTo(2));
			Technology engshpTech = Technology.All["engshp"];
			Assert.That(engshpTech.Level, Is.EqualTo(1));
			Assert.That(engshpTech.Cost, Is.EqualTo(4));
			Assert.That(engshpTech.HasTag("production"));
			Assert.That(engshpTech.UseProduceModules.Name, Is.EqualTo("engshp"));
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
            Assert.That(this.game.Factions, Is.Not.Null);
            Assert.That(this.game.Factions.Count, Is.EqualTo(2));
            Assert.That(this.game.Factions["1"].Name, Is.EqualTo("1"));
            Assert.That(this.game.Factions["1"].FullName, Is.EqualTo("NPC"));
            Assert.That(this.game.Factions["2"].Name, Is.EqualTo("2"));
            Assert.That(this.game.Factions["2"].FullName, Is.EqualTo("Caste Prime"));
            Assert.That(this.game.Factions["2"].Password, Is.EqualTo(""));
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
            Assert.That(this.game.Galaxy, Is.Not.Null);
            Assert.That(this.game.Galaxy.SpaceSystems.Count, Is.EqualTo(2));
            Assert.That(this.game.Galaxy.SpaceSystems[0].FullName, Is.EqualTo("Sol"));
            Assert.That(this.game.Galaxy.SpaceSystems[1].FullName, Is.EqualTo("Proxima Centauri"));

			SpaceSystem system = this.game.Galaxy.SpaceSystems[0];
            Assert.That(system.Objects, Is.Not.Null);
            Assert.That(system.Objects.Count, Is.EqualTo(3));
            Assert.That(system.Objects["S00001"], Is.InstanceOf(typeof(Star)));
            Assert.That(system.Objects["P00001"], Is.InstanceOf(typeof(Planet)));
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
            Assert.That(region, Is.Not.Null);
            Assert.That(region.Exits.Count, Is.EqualTo(1));
            Assert.That(region.Exits[0].To, Is.EqualTo(Region.All["R00002"]));
            Assert.That(region.Exits[0].ExitModes[EMoveMode.ground], Is.Not.Null);
            Assert.That(region.Exits[0].ExitModes[EMoveMode.ground].Duration, Is.EqualTo(3));
		}

		[Test]
		public void LoadItems()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			ItemType itemtype = this.game.ItemTypes["iron"];
            Assert.That(itemtype, Is.Not.Null);
            Assert.That(itemtype.Name, Is.EqualTo("iron"));
            Assert.That(itemtype.FullName, Is.EqualTo("unit of iron"));
            Assert.That(itemtype.FullNameMultiple, Is.EqualTo("units of iron"));
            Assert.That(itemtype.ReportName, Is.EqualTo("unit of iron [iron]"));
            Assert.That(itemtype.ReportNameMultiple, Is.EqualTo("units of iron [iron]"));
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
            Assert.That(itemType, Is.Not.Null);
            Assert.That(itemType.Group, Is.EqualTo(EItemTypesGroup.crew));
			Race race = Race.All["terran"];
            Assert.That(race, Is.Not.Null);
            Assert.That(race.Size, Is.EqualTo(4));
            Assert.That(race.Mass, Is.EqualTo(4));
            Assert.That(race.Capacity, Is.EqualTo(3));
			Person person = Person.All["200001"];
            Assert.That(person, Is.Not.Null);
            Assert.That(person.Race, Is.EqualTo(race));			
		}

		[Test]
		public void LoadUpkeep()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			ModuleType moduleType = ModuleType.All["cdrill"];
            Assert.That(moduleType, Is.Not.Null);
			ItemType itemType = ItemType.All["cash"];
            Assert.That(moduleType.Upkeep[itemType].Quantity, Is.EqualTo(40));

			ItemType crewType = ItemType.All["terran"];
            Assert.That(crewType.Upkeep[itemType].Quantity, Is.EqualTo(1));

			ItemStack crewStack = new ItemStack(crewType, 6);
            Assert.That(crewStack.Quantity, Is.EqualTo(6));
            Assert.That(crewStack.Upkeep[itemType].Quantity, Is.EqualTo(6));

			ItemStacks itemStacks = new ItemStacks
            {
                crewStack
            };
            Assert.That(itemStacks[crewType].Quantity, Is.EqualTo(6));

			ItemStack itemStack2 = itemStacks[crewType];
            Assert.That(itemStack2.Quantity, Is.EqualTo(6));
            Assert.That(itemStack2.Upkeep[itemType].Quantity, Is.EqualTo(6));
            Assert.That(itemStacks[crewType].Upkeep[itemType].Quantity, Is.EqualTo(6));

			ModuleStack moduleStack = new ModuleStack(null, null, moduleType, "000000");
			moduleStack.AddModule();
			moduleStack.ItemStacks.Add(crewStack);
            Assert.That(moduleStack.UpkeepNetto[itemType].Quantity, Is.EqualTo(40));
            Assert.That(moduleStack.ItemStacks[crewType].Quantity, Is.EqualTo(6));
            Assert.That(moduleStack.ItemStacks[crewType].Upkeep.Count, Is.EqualTo(1));
            Assert.That(moduleStack.ItemStacks[crewType].Upkeep[itemType].Quantity, Is.EqualTo(6));
            Assert.That(moduleStack.Upkeep[itemType].Quantity, Is.EqualTo(40 + 6));
		}

		[Test]
		public void LoadTechnologyConsumeItems()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			Technology technology = Technology.All["agrplx"];
            Assert.That(technology, Is.Not.Null);
			ItemType itemType = ItemType.All["iron"];

            Assert.That(technology.UseConsumeItems.Count, Is.EqualTo(1));
            Assert.That(technology.UseConsumeItems[itemType].Quantity, Is.EqualTo(10));
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
            Assert.That(moduleStack, Is.Not.Null);
            Assert.That(moduleStack.Technologies.Count, Is.EqualTo(1));
			Technology technology = Technology.All["hcdril"];
            Assert.That(technology, Is.Not.Null, "technology is not loaded");
            Assert.That(moduleStack.Technologies["hcdril"], Is.Not.Null, "modulestack has not loaded the echnology");
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
                        Assert.That(
                            ex.InnerException.InnerException.Message,
                            Is.EqualTo("Modulestack with name [000001] already exists"));
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
            Assert.That(race, Is.Not.Null);
			ItemType itemType = ItemType.All["cash"];
            Assert.That(race.Upkeep[itemType].Quantity, Is.EqualTo(10));
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
            Assert.That(race, Is.Not.Null);
			Orbit orbit = Orbit.All["O00003"];
            Assert.That(orbit.Races.Count, Is.EqualTo(1));

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
            Assert.That(planet, Is.Not.Null);
            Assert.That(planet.SurfaceSizeX, Is.EqualTo(6));
            Assert.That(planet.SurfaceSizeY, Is.EqualTo(4));

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
            Assert.That(moduleStack, Is.Not.Null);
            Assert.That(moduleStack.Orders.Count, Is.EqualTo(1));
			
			MoveOrder order = (MoveOrder)moduleStack.Orders[0];
            Assert.That(order.Route.Count, Is.EqualTo(2));

		}

        [Test]
        public void LoadOrder_unlimited()
        {
            this.LoadGameDocument();
            this.dataFile.LoadConfiguration();
            this.dataFile.LoadFactions();
            this.dataFile.LoadGalaxy();
            this.dataFile.LoadOrders();
            this.game = this.dataFile.Game;

            ModuleStack moduleStack = ModuleStack.All["000011"];
            Assert.That(moduleStack, Is.Not.Null);
            Assert.That(moduleStack.Orders.Count, Is.EqualTo(1));

            ProduceOrder order = (ProduceOrder)moduleStack.Orders[0];
            Assert.That(order.IsUnlimited, Is.True);
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
            Assert.That(moduleStack, Is.Not.Null);
            Assert.That(moduleStack.Orders.Count, Is.EqualTo(0));

            Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "get 1 iron from 000006",
                "@get 1 iron from 000006",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			GetOrder order = (GetOrder) testModuleStack.Orders[0];
            Assert.That(order.Quantity, Is.EqualTo(1));
            Assert.That(order.ItemType.Name, Is.EqualTo("iron"));
            Assert.That(order.Transferer.ReportName, Is.EqualTo("core drill [000006]"));
            Assert.That(order.Repeat, Is.EqualTo(1));

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

            List<string> testlines = new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"windows-1251\"?>",
                "<game>",
                "	<orders>",
                "		<order subject=\"modulestack\" name=\"100001\">",
                "			<get item=\"iron\" quantity=\"1\" transferer=\"000006\" />",
                "		</order>",
                "		<order subject=\"modulestack\" name=\"100001\" repeat=\"unlimited\">",
                "			<get item=\"iron\" quantity=\"1\" transferer=\"000006\" />",
                "		</order>",
                "	</orders>",
                "</game>"
            };
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
                Assert.That(generatedFile[i], Is.EqualTo(testlines[i]));
            }
            Assert.That(generatedFile.Count, Is.EqualTo(testlines.Count));

        }

        [Test]
        public void SaveUnlimitedProduceOrder()
        {
            this.LoadGameDocument();
            this.dataFile.LoadConfiguration();
            this.dataFile.LoadFactions();
            this.dataFile.LoadGalaxy();
            this.game = this.dataFile.Game;

            ModuleStack moduleStack = ModuleStack.All["100002"];

			Assert.That(moduleStack, Is.Not.Null);
			Assert.That(moduleStack.Orders.Count, Is.Zero);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000112"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "@produce cash",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            ProduceOrder order = (ProduceOrder)testModuleStack.Orders[0];
			Assert.That(order.IsUnlimited, Is.True);
			Assert.That(order.Producer.ReportName, Is.EqualTo("Caste Prime Headquarters [000112]"));
			Assert.That(order.ItemType.Name, Is.EqualTo("cash"));

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<game/>");
            this.dataFile.SaveOrders(doc);

            string testdir = Directory.GetCurrentDirectory();
            string testfile = "gameout.saved_unlimitedProduceOrder.xml";
            XmlTextWriter xmlWriter = new XmlTextWriter(
                Path.Combine(testdir, testfile),
                System.Text.Encoding.GetEncoding(1251));
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.IndentChar = '\t';
            xmlWriter.Indentation = 1;
            xmlWriter.WriteStartDocument();
            doc.WriteContentTo(xmlWriter);
            xmlWriter.Close();

            List<string> testlines = new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"windows-1251\"?>",
                "<game>",
                "	<orders>",
                "		<order subject=\"modulestack\" name=\"000112\" repeat=\"unlimited\">",
                "			<produce produce-type=\"item\" item=\"cash\" />",
                "		</order>",
                "	</orders>",
                "</game>"
            };
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
				Assert.That(generatedFile[i], Is.EqualTo(testlines[i]));
            }
			Assert.That(generatedFile.Count, Is.EqualTo(testlines.Count));

            List<string> testlinesReport = new List<string>
            {
                "@produce cash"
            };
			
			List<string> generatedReport = order.Report(testFaction);

            for (int i = 0; i < testlinesReport.Count; i++)
            {
                Console.WriteLine(testlinesReport[i]);
                Assert.That(generatedReport[i], Is.EqualTo(testlinesReport[i]));
            }
            Assert.That(generatedReport.Count, Is.EqualTo(testlinesReport.Count));

        }

        public void LoadXML_UnlimitedProduceOrder()
        {
            this.LoadGameDocument();
            this.dataFile.LoadConfiguration();
            this.dataFile.LoadFactions();
            this.dataFile.LoadGalaxy();
            
            this.game = this.dataFile.Game;

            ModuleStack moduleStack = ModuleStack.All["100002"];

            Assert.That(moduleStack, Is.Not.Null);
            Assert.That(moduleStack.Orders.Count, Is.Zero);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000112"];

            List<string> testcommands = new List<string>
            {
                "<order subject=\"modulestack\" name=\"000012\" repeat=\"unlimited\">",
                "<produce produce-type=\"item\" item=\"cash\" />",
                "</order>"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            ProduceOrder order = (ProduceOrder)testModuleStack.Orders[0];
            Assert.That(order.IsUnlimited, Is.True);
            Assert.That(order.Producer.ReportName, Is.EqualTo("Caste Prime Headquarters [000112]"));
            Assert.That(order.ItemType.Name, Is.EqualTo("cash"));

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<game/>");
            this.dataFile.SaveOrders(doc);

            string testdir = Directory.GetCurrentDirectory();
            string testfile = "gameout.saved_unlimitedProduceOrder.xml";
            XmlTextWriter xmlWriter = new XmlTextWriter(
                Path.Combine(testdir, testfile),
                System.Text.Encoding.GetEncoding(1251));
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.IndentChar = '\t';
            xmlWriter.Indentation = 1;
            xmlWriter.WriteStartDocument();
            doc.WriteContentTo(xmlWriter);
            xmlWriter.Close();

            List<string> testlines = new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"windows-1251\"?>",
                "<game>",
                "	<orders>",
                "		<order subject=\"modulestack\" name=\"000112\" repeat=\"unlimited\">",
                "			<produce produce-type=\"item\" item=\"cash\" />",
                "		</order>",
                "	</orders>",
                "</game>"
            };
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
                Assert.That(generatedFile[i], Is.EqualTo(testlines[i]));
            }
            Assert.That(generatedFile.Count, Is.EqualTo(testlines.Count));

            List<string> testlinesReport = new List<string>
            {
                "@produce cash"
            };

            List<string> generatedReport = order.Report(testFaction);

            for (int i = 0; i < testlinesReport.Count; i++)
            {
                Console.WriteLine(testlinesReport[i]);
                Assert.That(generatedReport[i], Is.EqualTo(testlinesReport[i]));
            }
            Assert.That(generatedReport.Count, Is.EqualTo(testlinesReport.Count));

        }
        [Test]
        public void SaveConditionOrder()
        {
            this.LoadGameDocument();
            this.dataFile.LoadConfiguration();
            this.dataFile.LoadFactions();
            this.dataFile.LoadGalaxy();
            this.game = this.dataFile.Game;

            ModuleStack moduleStack = ModuleStack.All["100002"];
            Assert.That(moduleStack, Is.Not.Null);
            Assert.That(moduleStack.Orders.Count, Is.EqualTo(0));

            Faction testFaction = this.game.Factions["2"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100002",
                "move R00001",
                "+use ssassm as new101",
                "+-use crewhs as new105",
                "+--give -20 terran to new105",
                "+-use strans as new106",
                "+--give all iron to new106",
                "#end"
            };

            Sequence.Ints.Clear();
            Sequence.Ints.Push(102);
            Sequence.Ints.Push(101);
            Sequence.Ints.Push(100);

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            MoveOrder order = (MoveOrder)moduleStack.Orders[0];
            Assert.That(order, Is.Not.Null);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<game/>");
            this.dataFile.SaveOrders(doc);

            string testdir = Directory.GetCurrentDirectory();
            string testfile = "gameout.saved_conditionOrders.xml";
            XmlTextWriter xmlWriter = new XmlTextWriter(
                Path.Combine(testdir, testfile),
                System.Text.Encoding.GetEncoding(1251));
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.IndentChar = '\t';
            xmlWriter.Indentation = 1;
            xmlWriter.WriteStartDocument();
            doc.WriteContentTo(xmlWriter);
            xmlWriter.Close();

            this.consoleOutFile("gameout.saved_conditionOrders.xml");
            this.compareFiles("gameout.conditionOrders.xml", "gameout.saved_conditionOrders.xml");

        }

		[Test]
		public void SaveLoad_PersistsModuleDamageBetweenTurns()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack stack = ModuleStack.All["000006"];
			Assert.That(stack.Quantity, Is.EqualTo(2));
			Assert.That(stack.Modules[0].Damage, Is.EqualTo(0));
			Assert.That(stack.Modules[0].CaptureDamage, Is.EqualTo(0));
			Assert.That(stack.Modules[0].Online, Is.True);

			stack.Modules[0].Damage = 26;
			stack.Modules[0].CaptureDamage = 9;
			stack.Modules[1].Online = false;

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.moduleDamage.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elStack = (XmlElement)saved.SelectSingleNode("//modulestack[@name='000006']");
			Assert.That(elStack, Is.Not.Null);
			XmlNodeList moduleNodes = elStack.SelectNodes("module");
			Assert.That(moduleNodes.Count, Is.EqualTo(2));
			Assert.That(((XmlElement)moduleNodes[0]).GetAttribute("damage"), Is.EqualTo("26"));
			Assert.That(((XmlElement)moduleNodes[0]).GetAttribute("capture"), Is.EqualTo("9"));
			Assert.That(((XmlElement)moduleNodes[0]).HasAttribute("online"), Is.False);
			Assert.That(((XmlElement)moduleNodes[1]).HasAttribute("damage"), Is.False);
			Assert.That(((XmlElement)moduleNodes[1]).HasAttribute("capture"), Is.False);
			Assert.That(((XmlElement)moduleNodes[1]).GetAttribute("online"), Is.EqualTo("false"));

			XmlElement elUndamaged = (XmlElement)saved.SelectSingleNode("//modulestack[@name='100001']");
			Assert.That(elUndamaged, Is.Not.Null);
			Assert.That(elUndamaged.SelectNodes("module").Count, Is.EqualTo(0));

			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;

			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			stack = ModuleStack.All["000006"];
			Assert.That(stack.Quantity, Is.EqualTo(2));
			Assert.That(stack.Modules[0].Damage, Is.EqualTo(26));
			Assert.That(stack.Modules[0].CaptureDamage, Is.EqualTo(9));
			Assert.That(stack.Modules[0].Online, Is.True);
			Assert.That(stack.Modules[1].Damage, Is.EqualTo(0));
			Assert.That(stack.Modules[1].CaptureDamage, Is.EqualTo(0));
			Assert.That(stack.Modules[1].Online, Is.False);
		}
    }
}
