using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TGasAtmosphere : TTest
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
		public void LoadGalaxy_OrbitResources_LoadsQuantities()
		{
			this.LoadGasAtmosphereWorld();

			Planet giant = Planet.All["P00002"];
			Assert.That(giant.PlanetType.Name, Is.EqualTo("gasgnt"));
			Assert.That(giant.AtmosphereBand, Is.EqualTo(EAtmosphereBand.hostile));

			Orbit orbit = Orbit.All["O00002"];
			Assert.That(orbit, Is.EqualTo(giant.Orbit));
			Assert.That(orbit.Resources.Count, Is.EqualTo(2));
			Assert.That(orbit.Resources.Contains(ItemType.All["heliu3"]), Is.True);
			Assert.That(orbit.Resources.Contains(ItemType.All["deutrm"]), Is.True);
			Assert.That(ResourceQuantity(orbit, "heliu3"), Is.EqualTo(120));
			Assert.That(ResourceQuantity(orbit, "deutrm"), Is.EqualTo(80));
		}

		[Test]
		public void SaveGame_PersistsOrbitResources()
		{
			this.LoadGasAtmosphereWorld();
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_gasAtmosphere.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elOrbit = (XmlElement)saved.SelectSingleNode("//planet[@name='P00002']/orbit[@name='O00002']");
			Assert.That(elOrbit, Is.Not.Null);
			Assert.That(elOrbit.SelectSingleNode("resource[@type='heliu3' and @quantity='120']"), Is.Not.Null);
			Assert.That(elOrbit.SelectSingleNode("resource[@type='deutrm' and @quantity='80']"), Is.Not.Null);

			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.LoadRamscoCatalog();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Orbit orbit = Orbit.All["O00002"];
			Assert.That(orbit.Resources.Count, Is.EqualTo(2));
			Assert.That(ResourceQuantity(orbit, "heliu3"), Is.EqualTo(120));
			Assert.That(ResourceQuantity(orbit, "deutrm"), Is.EqualTo(80));
		}

		[Test]
		public void EffectiveLocationType_GasGiantOrbit_IsAtmosphere()
		{
			this.LoadGasAtmosphereWorld();

			Orbit gasOrbit = Orbit.All["O00002"];
			Assert.That(BodyEnvironment.EffectiveLocationType(gasOrbit), Is.EqualTo(ELocationType.atmosphere));
			Assert.That(gasOrbit.LocationType, Is.EqualTo(ELocationType.orbit));
		}

		[Test]
		public void EffectiveLocationType_HabitableOrbit_StaysOrbit()
		{
			this.LoadGasAtmosphereWorld();

			Orbit habitableOrbit = Orbit.All["O00001"];
			Assert.That(BodyEnvironment.EffectiveLocationType(habitableOrbit), Is.EqualTo(ELocationType.orbit));
		}

		[Test]
		public void LoadLocationType_Atmosphere_DoesNotThrow()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<entry location-type=\"atmosphere\" />");
			Assert.That(this.dataFile.LoadLocationType((XmlElement)doc.DocumentElement), Is.EqualTo(ELocationType.atmosphere));
		}

		[Test]
		public void ExecuteProduceOrder_RamscoOnGasOrbit_Succeeds()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack scoop = ModuleStack.All["s00002"];
			this.AssignProduce(scoop, "heliu3");

			scoop.ExecutedLongOrder = false;
			scoop.Orders.Execute(this.game.Week);

			Assert.That(scoop.Effects.IsProducing, Is.True);
			Assert.That(this.HasEvent(scoop, "cannot operate"), Is.False);
		}

		[Test]
		public void ExecuteProduceOrder_RamscoOnDustRegion_FailsLocationGate()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack scoop = ModuleStack.All["s00003"];
			this.AssignProduce(scoop, "heliu3");

			scoop.ExecutedLongOrder = false;
			scoop.Orders.Execute(this.game.Week);

			Assert.That(scoop.Effects.IsProducing, Is.False);
			Assert.That(this.HasEvent(scoop, "cannot operate"), Is.True);
		}

		[Test]
		public void ExecuteUseOrder_SkimOnGasOrbit_Succeeds()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack scoop = ModuleStack.All["s00002"];
			this.AssignUse(scoop, "skimmn");

			scoop.ExecutedLongOrder = false;
			scoop.Orders.Execute(this.game.Week);

			Assert.That(this.HasEvent(scoop, "USE failed"), Is.False);
			Assert.That(scoop.Effects.IsProducing, Is.True);
		}

		[Test]
		public void ExecuteUseOrder_SkimOnDustRegion_FailsLocationGate()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack scoop = ModuleStack.All["s00003"];
			this.AssignUse(scoop, "skimmn");

			scoop.ExecutedLongOrder = false;
			scoop.Orders.Execute(this.game.Week);

			Assert.That(this.HasEvent(scoop, "USE failed"), Is.True);
			Assert.That(this.HasEvent(scoop, "cannot operate"), Is.True);
			Assert.That(scoop.Effects.IsProducing, Is.False);
		}

		[Test]
		public void Orbit_HasAtmosphere_GasGiantHostile_IsTrue()
		{
			this.LoadGasAtmosphereWorld();

			Orbit gasOrbit = Orbit.All["O00002"];
			Assert.That(gasOrbit.HasAtmosphere, Is.True);
		}

		[Test]
		public void ExecuteUseOrder_FisheryHarvestOnTerairSea_Succeeds()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack fishery = ModuleStack.All["s00004"];
			this.AssignUse(fishery, "fshhrv");

			fishery.ExecutedLongOrder = false;
			fishery.Orders.Execute(this.game.Week);

			Assert.That(this.HasEvent(fishery, "USE failed"), Is.False);
			Assert.That(fishery.Effects.IsProducing, Is.True);
		}

		[Test]
		public void ExecuteUseOrder_FisheryHarvestOnVacuumSea_FailsTerairGate()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack fishery = ModuleStack.All["s00005"];
			this.AssignUse(fishery, "fshhrv");

			fishery.ExecutedLongOrder = false;
			fishery.Orders.Execute(this.game.Week);

			Assert.That(this.HasEvent(fishery, "USE failed"), Is.True);
			Assert.That(this.HasEvent(fishery, "cannot operate"), Is.True);
			Assert.That(fishery.Effects.IsProducing, Is.False);
		}

		[Test]
		public void ExecuteProduceOrder_WindPlantOnVacuumSea_FailsTerairGate()
		{
			this.LoadGasAtmosphereWorld();
			ModuleStack windPlant = ModuleStack.All["s00006"];
			this.AssignProduce(windPlant, "energy");

			windPlant.ExecutedLongOrder = false;
			windPlant.Orders.Execute(this.game.Week);

			Assert.That(windPlant.Effects.IsProducing, Is.False);
			Assert.That(this.HasEvent(windPlant, "cannot operate"), Is.True);
		}

		private static int ResourceQuantity(Orbit orbit, string itemTypeName)
		{
			ItemType itemType = ItemType.All[itemTypeName];
			foreach (Resource resource in orbit.Resources)
			{
				if (resource.ItemType == itemType)
				{
					return resource.Quantity;
				}
			}
			Assert.Fail("Orbit missing resource " + itemTypeName);
			return 0;
		}

		private string GasAtmosphereFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "gas-atmosphere");
		}

		private void LoadGasAtmosphereWorld()
		{
			string fixtureDir = this.GasAtmosphereFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.LoadRamscoCatalog();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private void LoadRamscoCatalog()
		{
			XmlDocument extra = new XmlDocument();
			extra.Load(Path.Combine(this.GasAtmosphereFixtureDir(), "data-ramsco.xml"));
			CatalogLoader loader = new CatalogLoader(this.dataFile);
			loader.LoadItems(extra, this.dataFile.Game, true);
			loader.LoadItems(extra, this.dataFile.Game, false);
		}

		private void AssignProduce(ModuleStack stack, string itemType)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				"produce " + itemType,
				"#end"
			});
		}

		private void AssignUse(ModuleStack stack, string technology)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				"use " + technology,
				"#end"
			});
		}

		private bool HasEvent(ModuleStack stack, string fragment)
		{
			return stack.EventReports.Exists(eventReport => eventReport.Description.IndexOf(fragment) >= 0);
		}
	}
}
