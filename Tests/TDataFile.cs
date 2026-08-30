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
			Assert.That(Technology.All["stnrdf"].Level, Is.EqualTo(0));
			Assert.That(Technology.All["servic"].HasTag("repair"));
			Assert.That(Technology.All["servic"].UseProduceItems.ContainsKey(ItemType.All["spare"]));
			Assert.That(Technology.All["engshp"].HasTag("repair"));
			Assert.That(Technology.All["engshp"].HasTag("production"));
			Assert.That(Technology.All["medtec"].HasTag("repair"));
			Assert.That(Technology.All["medirf"].HasTag("repair"));

			Technology lasopt = Technology.All["lasopt"];
			Assert.That(lasopt.UseProduceModules.Name, Is.EqualTo("bltlas"));
			ModuleType bltlas = ModuleType.All["bltlas"];
			Assert.That(bltlas.Attack, Is.EqualTo(6));
			Assert.That(bltlas.Damage, Is.EqualTo(6));
			Assert.That(bltlas.DamageCapacity, Is.EqualTo(40));
			Assert.That(bltlas.EnergyRequired, Is.EqualTo(5));

			Technology xraylo = Technology.All["xraylo"];
			Assert.That(xraylo.Level, Is.EqualTo(2));
			Assert.That(xraylo.Requires, Is.EqualTo(Technology.All["lasopt"]));
			Assert.That(xraylo.UseProduceModules.Name, Is.EqualTo("xraylz"));

			Technology lstrrt = Technology.All["lstrrt"];
			Assert.That(lstrrt.Level, Is.EqualTo(1));
			Assert.That(lstrrt.HasTag("military"));
			Assert.That(lstrrt.Requires, Is.EqualTo(Technology.All["lasopt"]));
			Assert.That(lstrrt.UseProduceModules.Name, Is.EqualTo("laztrt"));
			ModuleType laztrt = ModuleType.All["laztrt"];
			Assert.That(laztrt.Attack, Is.EqualTo(6));
			Assert.That(laztrt.Damage, Is.EqualTo(6));
			Assert.That(laztrt.DamageCapacity, Is.EqualTo(100));
			Assert.That(laztrt.EnergyRequired, Is.EqualTo(5));

			ModuleType gunplc = ModuleType.All["gunplc"];
			Assert.That(gunplc.Attack, Is.EqualTo(1));
			Assert.That(gunplc.Damage, Is.EqualTo(1));
			Assert.That(gunplc.DamageCapacity, Is.EqualTo(100));

			ModuleType tanks = ModuleType.All["tanks"];
			Assert.That(tanks.Attack, Is.EqualTo(4));
			Assert.That(tanks.DamageCapacity, Is.EqualTo(100));

			ModuleType engshp = ModuleType.All["engshp"];
			Assert.That(engshp.Group, Is.EqualTo(EModuleTypesGroup.production));
			Assert.That(engshp.Size, Is.EqualTo(25));
			Assert.That(engshp.CrewRequired, Is.EqualTo(2));
			Technology engshpTech = Technology.All["engshp"];
			Assert.That(engshpTech.Level, Is.EqualTo(1));
			Assert.That(engshpTech.Cost, Is.EqualTo(4));
			Assert.That(engshpTech.HasTag("production"));
			Assert.That(engshpTech.UseProduceModules.Name, Is.EqualTo("engshp"));

			Technology rckter = Technology.All["rckter"];
			Assert.That(rckter.Level, Is.EqualTo(1));
			Assert.That(rckter.Cost, Is.EqualTo(4));
			Assert.That(rckter.HasTag("military"));
			Assert.That(rckter.UseCondition_ModuleTypesGroup, Is.EqualTo(EModuleTypesGroup.production));
			Assert.That(rckter.UseProduceItems.ContainsKey(ItemType.All["rctlnc"]));
			Assert.That(rckter.UseProduceModules, Is.Null);

			Technology airgen = Technology.All["airgen"];
			Assert.That(airgen.Level, Is.EqualTo(0));
			Assert.That(airgen.Name, Is.Not.EqualTo("lifsys"));
			Assert.That(airgen.UseProduceModules.Name, Is.EqualTo("lifsys"));
			ModuleType lifsysModule = ModuleType.All["lifsys"];
			Assert.That(lifsysModule.Group, Is.EqualTo(EModuleTypesGroup.habitat));
			Assert.That(lifsysModule.Size, Is.EqualTo(100));
			Assert.That(lifsysModule.CrewRequired, Is.EqualTo(0));
			Assert.That(lifsysModule.ItemsProduction.ContainsKey(ItemType.All["terair"]));

			Technology habcns = Technology.All["habcns"];
			Assert.That(habcns.Level, Is.EqualTo(2));
			Assert.That(habcns.Cost, Is.EqualTo(16));
			Assert.That(habcns.Name, Is.Not.EqualTo("smhabi"));
			Assert.That(habcns.UseProduceModules.Name, Is.EqualTo("smhabi"));
			ModuleType smhabiModule = ModuleType.All["smhabi"];
			Assert.That(smhabiModule.Group, Is.EqualTo(EModuleTypesGroup.habitat));
			Assert.That(smhabiModule.Size, Is.EqualTo(1000));
			Assert.That(smhabiModule.Capacity, Is.EqualTo(250));
			Assert.That(smhabiModule.CrewRequired, Is.EqualTo(0));

			Technology dmecns = Technology.All["dmecns"];
			Assert.That(dmecns.Level, Is.EqualTo(3));
			Assert.That(dmecns.Cost, Is.EqualTo(32));
			Assert.That(dmecns.Name, Is.Not.EqualTo("dmdcty"));
			Assert.That(dmecns.UseProduceModules.Name, Is.EqualTo("dmdcty"));
			ModuleType dmdctyModule = ModuleType.All["dmdcty"];
			Assert.That(dmdctyModule.Group, Is.EqualTo(EModuleTypesGroup.settlement));
			Assert.That(dmdctyModule.Size, Is.EqualTo(5000));

			Technology alnfgh = Technology.All["alnfgh"];
			Assert.That(alnfgh.Level, Is.EqualTo(4));
			Assert.That(alnfgh.Cost, Is.EqualTo(64));
			Assert.That(alnfgh.HasTag("military"));
			Assert.That(alnfgh.Name, Is.Not.EqualTo("alndrn"));
			Assert.That(alnfgh.UseProduceModules.Name, Is.EqualTo("alndrn"));
			ModuleType alndrnModule = ModuleType.All["alndrn"];
			Assert.That(alndrnModule.CrewRequired, Is.EqualTo(0));
			Assert.That(alndrnModule.Attack, Is.EqualTo(4));
			Assert.That(alndrnModule.Damage, Is.EqualTo(2));
			Assert.That(alndrnModule.Defense, Is.EqualTo(1));
			Assert.That(alndrnModule.DamageCapacity, Is.EqualTo(8));
			Assert.That(alndrnModule.Capacity, Is.EqualTo(1));
			Assert.That(alndrnModule.Initiative, Is.EqualTo(20));
			Assert.That(alndrnModule.MoveModes.ContainsKey(EMoveMode.space));
			Assert.That(alndrnModule.MoveModes[EMoveMode.space].Speed, Is.EqualTo(1));
			Assert.That(alndrnModule.MoveModes[EMoveMode.space].MassCapacity, Is.EqualTo(750));
			Assert.That(alndrnModule.FuelDuration, Is.EqualTo(13));
			Assert.That(alndrnModule.Fuel.ContainsKey(ItemType.All["heliu3"]));
			Assert.That(alndrnModule.Fuel[ItemType.All["heliu3"]].Quantity, Is.EqualTo(1));
			Assert.That(alndrnModule.Group, Is.EqualTo(EModuleTypesGroup.shuttle));
			Assert.That(alndrnModule.IsShuttleUnit, Is.True);
			Assert.That(ModuleType.All["shuttl"].IsShuttleUnit, Is.True);
			Assert.That(ModuleType.All["orbrkt"].IsShuttleUnit, Is.False);

			Assert.That(Technology.All["autfab"].Level, Is.EqualTo(2));
			Assert.That(Technology.All["autfab"].UseProduceModules.Name, Is.EqualTo("robofc"));
			Assert.That(ModuleType.All["robofc"].CrewRequired, Is.EqualTo(0));
			Assert.That(Technology.All["autctl"].Level, Is.EqualTo(3));
			Assert.That(Technology.All["autctl"].UseProduceModules.Name, Is.EqualTo("autcmd"));
			Assert.That(ModuleType.All["autcmd"].CrewRequired, Is.EqualTo(0));
			Assert.That(Technology.All["autprp"].Level, Is.EqualTo(3));
			Assert.That(Technology.All["autprp"].UseProduceModules.Name, Is.EqualTo("autdrv"));
			Assert.That(Technology.All["he3unc"].Level, Is.EqualTo(3));
			Assert.That(Technology.All["he3unc"].UseProduceModules.Name, Is.EqualTo("he3aut"));
			Assert.That(ModuleType.All["he3aut"].CrewRequired, Is.EqualTo(0));
			Assert.That(Technology.All["drnhng"].Level, Is.EqualTo(3));
			Assert.That(Technology.All["drnhng"].UseProduceModules.Name, Is.EqualTo("drnbay"));
			Assert.That(Technology.All["ahlcns"].Name, Is.Not.EqualTo("alnhul"));
			Assert.That(Technology.All["ahlcns"].UseProduceModules.Name, Is.EqualTo("alnhul"));
			Assert.That(ModuleType.All["alnhul"].Group, Is.EqualTo(EModuleTypesGroup.frigate));
			Assert.That(ModuleType.All["alnhul"].Capacity, Is.EqualTo(6000));
		}

		[Test]
		public void LoadConfiguration_LoadsOrbitalRocketLauncher()
		{
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());

			Technology orbrkt = Technology.All["orbrkt"];
			Assert.That(orbrkt.Level, Is.EqualTo(1));
			Assert.That(orbrkt.HasTag("military"));
			Assert.That(orbrkt.UseTime, Is.EqualTo(8));
			Assert.That(orbrkt.UseCondition_ModuleTypesGroup, Is.EqualTo(EModuleTypesGroup.production));
			Assert.That(orbrkt.UseCondition_LocationTypes, Is.Null.Or.Empty,
				"factory on the ground or a shuttle in orbit may both USE orbrkt");
			Assert.That(orbrkt.UseProduceModules.Name, Is.EqualTo("orbrkt"));
			Assert.That(orbrkt.UseConsumeItems.ContainsKey(ItemType.All["iron"]));
			Assert.That(orbrkt.UseConsumeItems[ItemType.All["iron"]].Quantity, Is.EqualTo(4));

			ModuleType launcher = ModuleType.All["orbrkt"];
			ModuleType shuttle = ModuleType.All["shuttl"];
			ModuleType drones = ModuleType.All["alndrn"];
			Assert.That(launcher.Group, Is.EqualTo(EModuleTypesGroup.military));
			double haulReserve =
				2 * ItemType.All["terran"].Size
				+ 20 * ItemType.All["food"].Size
				+ 20 * ItemType.All["terair"].Size
				+ ItemType.All["uraniu"].Size
				+ ItemType.All["h2o2"].Size;
			Assert.That(launcher.Size, Is.LessThanOrEqualTo(shuttle.Capacity - haulReserve),
				"shuttle must still hold crew, food, terair, and fuel beside the launcher");
			Assert.That(launcher.OperationCondition_LocationTypes, Does.Contain(ELocationType.orbit));
			Assert.That(launcher.OperationCondition_LocationTypes, Does.Not.Contain(ELocationType.solidSurface));
			Assert.That(launcher.Attack, Is.GreaterThan(0));
			Assert.That(launcher.Damage, Is.GreaterThan(0));
			Assert.That(launcher.Attack, Is.LessThan(drones.Attack * 4), "four drones should outgun one launcher");
			Assert.That(launcher.DamageCapacity, Is.LessThan(drones.DamageCapacity * 8), "drones can still chew through the launcher");
			Assert.That(launcher.Initiative, Is.LessThan(drones.Initiative));
		}

		[Test]
		public void LoadConfiguration_LoadsSickBay()
		{
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());

			Technology sckcns = Technology.All["sckcns"];
			Assert.That(sckcns.Level, Is.EqualTo(3));
			Assert.That(sckcns.Cost, Is.EqualTo(32));
			Assert.That(sckcns.HasTag("research"));
			Assert.That(sckcns.Requires, Is.EqualTo(Technology.All["medtec"]));
			Assert.That(sckcns.UseProduceModules.Name, Is.EqualTo("sckbay"));

			Technology pharms = Technology.All["pharms"];
			Assert.That(pharms.Level, Is.EqualTo(3));
			Assert.That(pharms.UseCondition_ModuleTypesGroup, Is.EqualTo(EModuleTypesGroup.habitat));
			Assert.That(pharms.UseCondition_ModuleType, Is.EqualTo("sckbay"));
			Assert.That(pharms.UseConsumeItems.ContainsKey(ItemType.All["food"]));
			Assert.That(pharms.UseProduceItems.ContainsKey(ItemType.All["medici"]));

			ModuleType sickBay = ModuleType.All["sckbay"];
			Assert.That(sickBay.Group, Is.EqualTo(EModuleTypesGroup.habitat));
			Assert.That(sickBay.Size, Is.EqualTo(380));
			Assert.That(sickBay.HealTarget, Is.EqualTo("wndtrn"));
			Assert.That(sickBay.HealQuantity, Is.EqualTo(2));
			Assert.That(sickBay.HealWeeks, Is.EqualTo(4));
			Assert.That(sickBay.HealQuantityWithItem, Is.EqualTo(4));
			Assert.That(sickBay.HealWeeksWithItem, Is.EqualTo(1));
			Assert.That(sickBay.HealConsumeItem, Is.EqualTo("medici"));
			Assert.That(sickBay.HealConsumeQuantity, Is.EqualTo(1));
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
		public void LoadGalaxy_MoonNameComesFromMoonElement()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Assert.That(Moon.All.ContainsKey("P00003"), Is.True);
			Moon moon = Moon.All["P00003"];
			Assert.That(moon.FullName, Is.EqualTo("Luna"));
			Assert.That(moon.Planet.Name, Is.EqualTo("P00002"));
			Assert.That(Moon.All.ContainsKey("P00002"), Is.False);
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
		public void LoadExits_MoonRegion()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Region region = Region.All["R00010"];
			Assert.That(region, Is.Not.Null);
			Assert.That(region.RegionHolder, Is.InstanceOf(typeof(Moon)));
			Assert.That(((Moon)region.RegionHolder).Name, Is.EqualTo("P00003"));
			Assert.That(region.Exits.Count, Is.EqualTo(1));
			Assert.That(region.Exits[0].To, Is.EqualTo(Region.All["R00001"]));
			Assert.That(region.Exits[0].ExitModes[EMoveMode.ground], Is.Not.Null);
			Assert.That(region.Exits[0].ExitModes[EMoveMode.ground].Duration, Is.EqualTo(5));
		}

		[Test]
		public void SaveLoad_PersistsOrbitExitTarget()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_orbitExit.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elExit = (XmlElement)saved.SelectSingleNode("//region[@name='R00011']/exit");
			Assert.That(elExit, Is.Not.Null);
			Assert.That(elExit.GetAttribute("orbit"), Is.EqualTo("O00004"));
			Assert.That(elExit.HasAttribute("region"), Is.False);

			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;

			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Region region = Region.All["R00011"];
			Assert.That(region.Exits.Count, Is.EqualTo(1));
			Assert.That(region.Exits[0].To, Is.InstanceOf(typeof(Orbit)));
			Assert.That(region.Exits[0].To.Name, Is.EqualTo("O00004"));
			Assert.That(region.Exits[0].ExitModes[EMoveMode.space].Duration, Is.EqualTo(2));
		}

		[Test]
		public void Report_OrbitExitTarget_ListsOrbitDestination()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Region region = Region.All["R00011"];
			List<string> lines = region.Exits.Report;
			Assert.That(lines, Does.Contain("Exits:"));
			Assert.That(lines, Has.Some.Contains("orbit [O00004]"));
			Assert.That(lines, Has.Some.Contains("space travel duration 2 weeks"));
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
		public void LoadTechnologyConsumeModules_CityPlanningConsumesCity()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			Technology technology = Technology.All["ctypln"];

			Assert.That(technology, Is.Not.Null);
			Assert.That(technology.UseConsumeModules, Is.SameAs(ModuleType.All["city"]));
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

        [Test]
		public void SaveLoadUseOrder_withUseOrderInProgress_sameOrder()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack factory = ModuleStack.All["000004"];
			ItemType iron = ItemType.All["iron"];
			int ironBefore = factory.ItemStacks[iron].Quantity;

			Sequence.Ints.Push(100);
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use agrplx as \"farms1\"",
				"#end"
			});

			this.executeFactoryWeek(factory, 0);
			ProducingModule farms = this.producingModule(factory, "agrplx");
			Assert.That(farms, Is.Not.Null);
			Assert.That(farms.Duration, Is.EqualTo(3));
			Assert.That(factory.ItemStacks[iron].Quantity, Is.EqualTo(ironBefore - 10));
			string originalReceiver = ((ModuleStack)farms.Receiver).Name;

			this.reloadSavedGame("gameout.saved_useSame.xml");
			factory = ModuleStack.All["000004"];
			iron = ItemType.All["iron"];
			farms = this.producingModule(factory, "agrplx");
			Assert.That(farms, Is.Not.Null, "in-progress production must persist across save/load");
			Assert.That(farms.Duration, Is.EqualTo(3));

			this.executeFactoryWeek(factory, 1);
			farms = this.producingModule(factory, "agrplx");
			Assert.That(farms.Duration, Is.EqualTo(2));
			Assert.That(factory.ItemStacks[iron].Quantity, Is.EqualTo(ironBefore - 10), "continuing the same USE must not consume again");

			Sequence.Ints.Push(200);
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use agrplx as \"farms2\"",
				"#end"
			});
			this.executeFactoryWeek(factory, 2);
			farms = this.producingModule(factory, "agrplx");
			Assert.That(farms.Duration, Is.EqualTo(1));
			Assert.That(((ModuleStack)farms.Receiver).Name, Is.Not.EqualTo(originalReceiver), "same tech with a new target updates the producing effect");
			Assert.That(factory.ItemStacks[iron].Quantity, Is.EqualTo(ironBefore - 10));
		}

		[Test]
		public void SaveLoadUseOrder_withUseOrderInProgress_newOrder()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack factory = ModuleStack.All["000004"];
			ItemType iron = ItemType.All["iron"];
			int ironBefore = factory.ItemStacks[iron].Quantity;

			Sequence.Ints.Push(100);
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use armcbt as \"tanks1\"",
				"#end"
			});
			this.executeFactoryWeek(factory, 0);
			ProducingModule tanks = this.producingModule(factory, "armcbt");
			Assert.That(tanks, Is.Not.Null);
			Assert.That(tanks.Duration, Is.EqualTo(3));
			Assert.That(factory.ItemStacks[iron].Quantity, Is.EqualTo(ironBefore - 4));
			string tanksReceiver = ((ModuleStack)tanks.Receiver).Name;

			this.reloadSavedGame("gameout.saved_useNew.xml");
			factory = ModuleStack.All["000004"];
			iron = ItemType.All["iron"];
			Assert.That(this.producingModule(factory, "armcbt").Duration, Is.EqualTo(3));

			Sequence.Ints.Push(200);
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use wndtrb as \"wind1\"",
				"#end"
			});
			this.executeFactoryWeek(factory, 1);
			tanks = this.producingModule(factory, "armcbt");
			ProducingModule windmills = this.producingModule(factory, "wndtrb");
			Assert.That(windmills, Is.Not.Null, "a different USE starts a new production");
			Assert.That(windmills.Duration, Is.EqualTo(1));
			Assert.That(tanks, Is.Not.Null, "previous production stays frozen");
			Assert.That(tanks.Duration, Is.EqualTo(3));
			Assert.That(factory.ItemStacks[iron].Quantity, Is.EqualTo(ironBefore - 4 - 1));

			this.executeFactoryWeek(factory, 2);
			Assert.That(this.producingModule(factory, "wndtrb"), Is.Null, "windmills finish");
			tanks = this.producingModule(factory, "armcbt");
			Assert.That(tanks.Duration, Is.EqualTo(3), "frozen tanks must not tick while windmills run");

			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use armcbt as \"" + tanksReceiver + "\"",
				"#end"
			});
			this.executeFactoryWeek(factory, 3);
			tanks = this.producingModule(factory, "armcbt");
			Assert.That(tanks, Is.Not.Null);
			Assert.That(tanks.Duration, Is.EqualTo(2), "resuming the original USE continues the frozen production");
			Assert.That(factory.ItemStacks[iron].Quantity, Is.EqualTo(ironBefore - 4 - 1), "resume must not consume tanks resources again");
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
		public void SaveConditionOrder_DoesNotDuplicateNestedXml()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

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

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<game/>");
			this.dataFile.SaveOrders(doc);

			Assert.That(doc.SelectNodes("//orders//order").Count, Is.EqualTo(6));
		}

		[Test]
		public void SaveLoad_PersistsConditionalOrderGraph()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack moduleStack = ModuleStack.All["100002"];
			Assert.That(moduleStack.Orders.Count, Is.EqualTo(0));

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

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			Assert.That(moduleStack.Orders.Count, Is.EqualTo(6));
			MoveOrder move = (MoveOrder)moduleStack.Orders[0];
			UseOrder assembly = (UseOrder)moduleStack.Orders[1];
			Assert.That(assembly.Technology.Name, Is.EqualTo("ssassm"));
			Assert.That(assembly.Level, Is.EqualTo(1));
			Assert.That(move.ConditionalOrders.Count, Is.EqualTo(5));
			Assert.That(move.ConditionalOrders.Contains(assembly));

			this.reloadSavedGame("gameout.saved_conditionOrdersLoad.xml");

			moduleStack = ModuleStack.All["100002"];
			Assert.That(moduleStack.Orders.Count, Is.EqualTo(6));
			move = (MoveOrder)moduleStack.Orders[0];
			assembly = null;
			foreach (Order order in moduleStack.Orders)
			{
				UseOrder use = order as UseOrder;
				if (use != null && use.Technology != null && use.Technology.Name == "ssassm")
				{
					assembly = use;
					break;
				}
			}
			Assert.That(assembly, Is.Not.Null);
			Assert.That(assembly.Level, Is.EqualTo(1));
			Assert.That(move.ConditionalOrders.Count, Is.EqualTo(5));
			Assert.That(move.ConditionalOrders.Contains(assembly));
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

		[Test]
		public void ModuleTypeGroupXml_Parse_MapsCatalogTokensIncludingResearch()
		{
			Assert.That(ModuleTypeGroupXml.Parse("research"), Is.EqualTo(EModuleTypesGroup.research));
			Assert.That(ModuleTypeGroupXml.Parse("space station"), Is.EqualTo(EModuleTypesGroup.spaceStation));
			Assert.That(ModuleTypeGroupXml.Parse("spacestation"), Is.EqualTo(EModuleTypesGroup.spaceStation));
			Assert.That(ModuleTypeGroupXml.Parse("spaceStation"), Is.EqualTo(EModuleTypesGroup.spaceStation));
			Assert.That(ModuleTypeGroupXml.Parse("settlement"), Is.EqualTo(EModuleTypesGroup.settlement));
			Assert.That(ModuleTypeGroupXml.Parse("shuttle"), Is.EqualTo(EModuleTypesGroup.shuttle));
		}

		[Test]
		public void ModuleTypeGroupXml_Parse_MapsShipHullGroups()
		{
			Assert.That(ModuleTypeGroupXml.Parse("corvette"), Is.EqualTo(EModuleTypesGroup.corvette));
			Assert.That(ModuleTypeGroupXml.Parse("destroyer"), Is.EqualTo(EModuleTypesGroup.destroyer));
			Assert.That(ModuleTypeGroupXml.Parse("cruiser"), Is.EqualTo(EModuleTypesGroup.cruiser));
			Assert.That(ModuleTypeGroupXml.Parse("capital"), Is.EqualTo(EModuleTypesGroup.capital));
			Assert.That(ModuleTypeGroupXml.Parse("ark"), Is.EqualTo(EModuleTypesGroup.ark));
		}

		[Test]
		public void ModuleTypeGroupXml_ToToken_MapsShipHullGroups()
		{
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.corvette), Is.EqualTo("corvette"));
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.destroyer), Is.EqualTo("destroyer"));
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.cruiser), Is.EqualTo("cruiser"));
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.capital), Is.EqualTo("capital"));
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.ark), Is.EqualTo("ark"));
		}

		[Test]
		public void ModuleType_IsShipHull_RecognizesHullGroups()
		{
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.frigate), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.corvette), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.destroyer), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.cruiser), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.capital), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.ark), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.shuttle), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.spacecraft), Is.True);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.military), Is.False);
			Assert.That(ModuleType.IsShipHull(EModuleTypesGroup.settlement), Is.False);
		}

		[Test]
		public void ModuleTypeGroupXml_Parse_UnknownToken_ThrowsKeyNotFound()
		{
			Assert.Throws<KeyNotFoundException>(() => ModuleTypeGroupXml.Parse("not-a-group"));
		}

		[Test]
		public void ModuleTypeGroupXml_ToToken_MapsResearchGroup()
		{
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.research), Is.EqualTo("research"));
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.spaceStation), Is.EqualTo("space station"));
			Assert.That(ModuleTypeGroupXml.ToToken(EModuleTypesGroup.settlement), Is.EqualTo("settlement"));
		}

		[Test]
		public void SaveLoad_PersistsProducingModuleTechnology()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack factory = ModuleStack.All["000004"];
			Sequence.Ints.Push(100);
			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use agrplx as \"farms1\"",
				"#end"
			});
			this.executeFactoryWeek(factory, 0);
			Assert.That(this.producingModule(factory, "agrplx"), Is.Not.Null);

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_producingModuleTech.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elEffect = (XmlElement)saved.SelectSingleNode("//modulestack[@name='000004']/effect[@type='producing-modules']");
			Assert.That(elEffect, Is.Not.Null);
			Assert.That(elEffect.GetAttribute("technology"), Is.EqualTo("agrplx"), "producing-modules save must keep the technology id");

			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			factory = ModuleStack.All["000004"];
			ProducingModule farms = this.producingModule(factory, "agrplx");
			Assert.That(farms, Is.Not.Null);
			Assert.That(farms.Technology.Name, Is.EqualTo("agrplx"));
		}

		[Test]
		public void SaveLoad_PersistsProducingItemsEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack farms = ModuleStack.All["000008"];
			Technology farming = Technology.All["farmng"];
			new ProducingItems(farms, farming, 3);

			this.reloadSavedGame("gameout.saved_producingItems.xml");
			farms = ModuleStack.All["000008"];
			ProducingItems producing = this.producingItems(farms, "farmng");
			Assert.That(producing, Is.Not.Null, "in-progress producing-items must persist across save/load");
			Assert.That(producing.Duration, Is.EqualTo(3));
			Assert.That(producing.ProducedItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(15));
		}

		[Test]
		public void SaveLoad_PersistsProducingEnergyEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack plants = ModuleStack.All["000011"];
			new ProducingEnergy(plants, Technology.All["farmng"], 4);

			this.reloadSavedGame("gameout.saved_producingEnergy.xml");
			plants = ModuleStack.All["000011"];
			ProducingEnergy producing = null;
			foreach (Effect effect in plants.Effects)
			{
				producing = effect as ProducingEnergy;
				if (producing != null)
				{
					break;
				}
			}
			Assert.That(producing, Is.Not.Null, "in-progress producing-energy must persist across save/load");
			Assert.That(producing.Duration, Is.EqualTo(4));
			Assert.That(producing.Technology.Name, Is.EqualTo("farmng"));
		}

		[Test]
		public void SaveLoad_PersistsProducingEnergyEffect_WithoutTechnology()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack plants = ModuleStack.All["000011"];
			new ProducingEnergy(plants, null, 4);

			this.reloadSavedGame("gameout.saved_producingEnergyNoTech.xml");
			plants = ModuleStack.All["000011"];
			ProducingEnergy producing = null;
			foreach (Effect effect in plants.Effects)
			{
				producing = effect as ProducingEnergy;
				if (producing != null)
				{
					break;
				}
			}
			Assert.That(producing, Is.Not.Null, "in-progress producing-energy with no technology must persist across save/load");
			Assert.That(producing.Duration, Is.EqualTo(4));
			Assert.That(producing.Technology, Is.Null);
		}

		[Test]
		public void SaveLoad_PersistsReceivingItemsEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack factory = ModuleStack.All["000004"];
			ModuleStack farms = ModuleStack.All["000008"];
			new ReceivingItems(factory, farms, ItemStack.Cash(7), 2);

			this.reloadSavedGame("gameout.saved_receivingItems.xml");
			factory = ModuleStack.All["000004"];
			ReceivingItems receiving = null;
			foreach (Effect effect in factory.Effects)
			{
				receiving = effect as ReceivingItems;
				if (receiving != null)
				{
					break;
				}
			}
			Assert.That(receiving, Is.Not.Null, "in-progress receiving-items must persist across save/load");
			Assert.That(receiving.Duration, Is.EqualTo(2));
			Assert.That(receiving.Transferer.Name, Is.EqualTo("000008"));
			Assert.That(receiving.ItemStack.ItemType.Name, Is.EqualTo("cash"));
			Assert.That(receiving.ItemStack.Quantity, Is.EqualTo(7));
		}

		[Test]
		public void SaveLoad_PersistsReceivingModulesEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack factory = ModuleStack.All["000004"];
			new ReceivingModules(factory, ModuleStack.All["000007"], ModuleStack.All["000006"], 3);

			this.reloadSavedGame("gameout.saved_receivingModules.xml");
			factory = ModuleStack.All["000004"];
			ReceivingModules receiving = null;
			foreach (Effect effect in factory.Effects)
			{
				receiving = effect as ReceivingModules;
				if (receiving != null)
				{
					break;
				}
			}
			Assert.That(receiving, Is.Not.Null, "in-progress receiving-modules must persist across save/load");
			Assert.That(receiving.Duration, Is.EqualTo(3));
			Assert.That(receiving.Transferer.Name, Is.EqualTo("000007"));
			Assert.That(receiving.ModuleStack.Name, Is.EqualTo("000006"));
		}

		[Test]
		public void SaveLoad_PersistsReceivingTechnologyEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack factory = ModuleStack.All["000004"];
			new ReceivingTechnology(factory, ModuleStack.All["000008"], Technology.All["farmng"], 2);

			this.reloadSavedGame("gameout.saved_receivingTechnology.xml");
			factory = ModuleStack.All["000004"];
			ReceivingTechnology receiving = null;
			foreach (Effect effect in factory.Effects)
			{
				receiving = effect as ReceivingTechnology;
				if (receiving != null)
				{
					break;
				}
			}
			Assert.That(receiving, Is.Not.Null, "in-progress receiving-technology must persist across save/load");
			Assert.That(receiving.Duration, Is.EqualTo(2));
			Assert.That(receiving.Transferer.Name, Is.EqualTo("000008"));
			Assert.That(receiving.Technology.Name, Is.EqualTo("farmng"));
		}

		[Test]
		public void SaveLoad_PersistsLightlyDamagedEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack drill = ModuleStack.All["000006"];
			new LightlyDamaged(drill);

			this.reloadSavedGame("gameout.saved_lightlyDamaged.xml");
			drill = ModuleStack.All["000006"];
			LightlyDamaged damaged = null;
			foreach (Effect effect in drill.Effects)
			{
				damaged = effect as LightlyDamaged;
				if (damaged != null)
				{
					break;
				}
			}
			Assert.That(damaged, Is.Not.Null, "lightly-damaged must persist across save/load");
		}

		[Test]
		public void SaveLoad_PersistsTrainingSkillEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Person trainee = Person.All["200001"];
			new TrainingSkill(trainee, 4, SkillType.All["arpldr"]);

			this.reloadSavedGame("gameout.saved_trainingSkill.xml");
			trainee = Person.All["200001"];
			TrainingSkill training = null;
			foreach (Effect effect in trainee.Effects)
			{
				training = effect as TrainingSkill;
				if (training != null)
				{
					break;
				}
			}
			Assert.That(training, Is.Not.Null, "in-progress training-skill must persist across save/load");
			Assert.That(training.Duration, Is.EqualTo(4));
			Assert.That(training.SkillType.Name, Is.EqualTo("arpldr"));
		}

		[Test]
		public void SaveLoad_PersistsTrainingOfficerEffect()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			ModuleStack hq = ModuleStack.All["000112"];
			new TrainingOfficer(hq, 5, Race.All["terran"], Person.All["000101"], hq);

			this.reloadSavedGame("gameout.saved_trainingOfficer.xml");
			hq = ModuleStack.All["000112"];
			TrainingOfficer training = null;
			foreach (Effect effect in hq.Effects)
			{
				training = effect as TrainingOfficer;
				if (training != null)
				{
					break;
				}
			}
			Assert.That(training, Is.Not.Null, "in-progress training-officer must persist across save/load");
			Assert.That(training.Duration, Is.EqualTo(5));
			Assert.That(training.Race.Name, Is.EqualTo("terran"));
			Assert.That(training.Officer.Name, Is.EqualTo("000101"));
			Assert.That(training.OfficerParent.Name, Is.EqualTo("000112"));
		}

		[Test]
		public void SaveLoad_PersistsResearchCapacityGroup()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Region region = Region.All["R00001"];
			Capacity research = new Capacity();
			research.Group = EModuleTypesGroup.research;
			research.Quantity = 3;
			region.Capacities.Add(research);

			this.reloadSavedGame("gameout.saved_researchCapacity.xml");
			region = Region.All["R00001"];
			Capacity loaded = null;
			foreach (Capacity capacity in region.Capacities)
			{
				if (capacity.Group == EModuleTypesGroup.research)
				{
					loaded = capacity;
					break;
				}
			}
			Assert.That(loaded, Is.Not.Null);
			Assert.That(loaded.Quantity, Is.EqualTo(3));
		}

		[Test]
		public void SaveGame_FactionXmlReport_OmitsOtherFactionsAndInvisibleRegions()
		{
			this.LoadGameDocument();
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadOrders();
			this.game = this.dataFile.Game;

			Faction castePrime = this.game.Factions["2"];
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_factionXmlReport.xml";
			this.dataFile.SaveGame(testdir, testfile, castePrime);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			Assert.That(saved.SelectNodes("/game/faction[@name='2']").Count, Is.EqualTo(1));
			Assert.That(saved.SelectNodes("/game/faction[@name='1']").Count, Is.EqualTo(0));
			Assert.That(saved.SelectSingleNode("//region[@name='R00001']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//region[@name='R10001']"), Is.Null);
		}

		private void executeFactoryWeek(ModuleStack factory, int weekOffset)
		{
			factory.ExecutedLongOrder = false;
			factory.Execute(this.game.Week + weekOffset);
		}

		private void reloadSavedGame(string testfile)
		{
			string testdir = Directory.GetCurrentDirectory();
			this.dataFile.SaveGame(testdir, testfile);
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;

			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadOrders();
			this.game = this.dataFile.Game;
		}

		private ProducingModule producingModule(ModuleStack factory, string technologyName)
		{
			foreach (Effect effect in factory.Effects)
			{
				ProducingModule producing = effect as ProducingModule;
				if (producing != null
					&& !producing.Executed
					&& producing.Technology != null
					&& producing.Technology.Name == technologyName)
				{
					return producing;
				}
			}
			return null;
		}

		private ProducingItems producingItems(ModuleStack producer, string technologyName)
		{
			foreach (Effect effect in producer.Effects)
			{
				ProducingItems producing = effect as ProducingItems;
				if (producing != null
					&& producing.Technology != null
					&& producing.Technology.Name == technologyName)
				{
					return producing;
				}
			}
			return null;
		}
    }
}
