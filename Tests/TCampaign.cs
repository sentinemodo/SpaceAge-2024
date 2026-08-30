using System;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TCampaign : TTest
	{
		public static string RepoRoot()
		{
			string dir = TestContext.CurrentContext.TestDirectory;
			for (int i = 0; i < 10; i++)
			{
				if (File.Exists(Path.Combine(dir, "campaign", "data.xml")))
				{
					return dir;
				}
				DirectoryInfo parent = Directory.GetParent(dir);
				if (parent == null)
				{
					break;
				}
				dir = parent.FullName;
			}
			throw new DirectoryNotFoundException("Could not find campaign/data.xml from " + TestContext.CurrentContext.TestDirectory);
		}

		public static string CampaignDir()
		{
			return Path.Combine(RepoRoot(), "campaign");
		}

		[SetUp]
		public void setup()
		{
			this.dataFile = new DataFile(CampaignDir());
			this.game = new Game();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void LoadGame_CampaignGamein1_LoadsWorld()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadContracts();
			this.game = this.dataFile.Game;

			Assert.That(this.game.Galaxy.SpaceSystems.Count, Is.EqualTo(10));
			Assert.That(Planet.All.ContainsKey("P00009"), Is.False);
			Assert.That(Planet.All.ContainsKey("P00010"), Is.False);
			Assert.That(Alderson.All.ContainsKey("P00009"));
			Assert.That(Alderson.All.ContainsKey("P00010"));
			Assert.That(Alderson.All["P00009"].PairName, Is.EqualTo("P00010"));
			Assert.That(Alderson.All["P00010"].PairName, Is.EqualTo("P00009"));
			Assert.That(Region.All.ContainsKey("R01490"), Is.False);
			Assert.That(Region.All.ContainsKey("R01491"), Is.False);
			Assert.That(ModuleStack.All.ContainsKey("120001"));
			Assert.That(ModuleStack.All["120001"].Location.Name, Is.EqualTo("R00014"));
			Assert.That(ModuleStack.All.ContainsKey("130001"));
			Assert.That(ModuleStack.All["130001"].Location.Name, Is.EqualTo("R00060"));
			int hqCount = 0;
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.ModuleType != null && stack.ModuleType.Name == "corphq")
				{
					hqCount++;
				}
			}
			Assert.That(hqCount, Is.EqualTo(10));
		}

		[Test]
		public void LoadGame_CampaignGamein1_SurfaceSeedRules()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Assert.That(Region.All["R00001"].Exits.Contains(Region.All["R00006"]), Is.True);
			Assert.That(Region.All["R00006"].Exits.Contains(Region.All["R00001"]), Is.True);
			Assert.That(Moon.All["M00001"].AU, Is.EqualTo(0.003).Within(0.0000001));
			Assert.That(Moon.All["M00005"].AU, Is.EqualTo(0.009).Within(0.0000001));
			foreach (Region region in Region.All.Values)
			{
				Assert.That(region.Resources.Contains(this.game.ItemTypes["terair"]), Is.False,
					region.Name + " should not seed regional terair");
				Assert.That(region.Resources.Contains(this.game.ItemTypes["h2o2"]), Is.False,
					region.Name + " should not seed regional h2o2");
				foreach (Exit exit in region.Exits)
				{
					Assert.That(exit.To, Is.Not.InstanceOf<Orbit>(),
						region.Name + " should not have a region-to-orbit exit");
				}
			}
		}

		[Test]
		public void LoadGame_CampaignGamein1_LiquidExitsAreNaval()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Assert.That(Region.All["R00001"].Exits[Region.All["R00002"]].ExitModes.ContainsKey(EMoveMode.naval), Is.True);
			Assert.That(Region.All["R00003"].Exits[Region.All["R00002"]].ExitModes.ContainsKey(EMoveMode.naval), Is.True);
			Assert.That(Region.All["R00003"].Exits[Region.All["R00004"]].ExitModes.ContainsKey(EMoveMode.ground), Is.True);
			Assert.That(Region.All["R00001"].Exits[Region.All["R00006"]].ExitModes.ContainsKey(EMoveMode.naval), Is.True);

			foreach (Region region in Region.All.Values)
			{
				foreach (Exit exit in region.Exits)
				{
					if (exit.ExitModes.ContainsKey(EMoveMode.space))
					{
						continue;
					}
					Region destination = exit.To as Region;
					if (destination == null)
					{
						continue;
					}
					bool liquid = this.IsLiquid(region) || this.IsLiquid(destination);
					if (liquid)
					{
						Assert.That(exit.ExitModes.ContainsKey(EMoveMode.naval), Is.True,
							region.Name + " -> " + destination.Name + " should be naval");
						Assert.That(exit.ExitModes.ContainsKey(EMoveMode.ground), Is.False,
							region.Name + " -> " + destination.Name + " should not also be ground");
					}
					else
					{
						Assert.That(exit.ExitModes.ContainsKey(EMoveMode.ground), Is.True,
							region.Name + " -> " + destination.Name + " should be ground");
					}
				}
			}
		}

		private bool IsLiquid(Region region)
		{
			return region.RegionType != null
				&& region.RegionType.LocationType == ELocationType.liquidSurface;
		}

		[Test]
		public void LoadConfiguration_CampaignCatalog_LoadsAdpnt()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.game = this.dataFile.Game;

			Assert.That(this.game.PlanetTypes.ContainsKey("adpnt"));
			Assert.DoesNotThrow(() => this.dataFile.ValidateTypeNameUniqueness());
			foreach (ModuleType moduleType in ModuleType.All.Values)
			{
				Assert.That(ModuleTypeGroupXml.ToToken(moduleType.Group), Is.Not.Null,
					"unknown module group on " + moduleType.Name);
			}
		}

		[Test]
		public void LoadConfiguration_CampaignCatalog_ShipHullGroups()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.game = this.dataFile.Game;

			Assert.That(ModuleType.All["corhul"].Group, Is.EqualTo(EModuleTypesGroup.corvette));
			Assert.That(ModuleType.All["deshul"].Group, Is.EqualTo(EModuleTypesGroup.destroyer));
			Assert.That(ModuleType.All["cruhul"].Group, Is.EqualTo(EModuleTypesGroup.cruiser));
			Assert.That(ModuleType.All["arkhul"].Group, Is.EqualTo(EModuleTypesGroup.ark));
			Assert.That(ModuleType.All["corhul"].IsShipHullType, Is.True);
			Assert.That(ModuleType.All["arkhul"].IsShipHullType, Is.True);
		}

		[Test]
		public void LoadConfiguration_CampaignCatalog_LoadsNavalModules()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.game = this.dataFile.Game;

			Technology nvltrs = Technology.All["nvltrs"];
			Assert.That(nvltrs.Level, Is.EqualTo(0));
			Assert.That(nvltrs.UseProduceModules.Name, Is.EqualTo("coastr"));
			ModuleType coastr = ModuleType.All["coastr"];
			Assert.That(coastr.Group, Is.EqualTo(EModuleTypesGroup.vehicle));
			Assert.That(coastr.MoveModes.ContainsKey(EMoveMode.naval), Is.True);
			Assert.That(coastr.MoveModes[EMoveMode.naval].Speed, Is.EqualTo(1));
			Assert.That(coastr.MoveModes.ContainsKey(EMoveMode.ground), Is.False);
			Assert.That(coastr.OperationCondition_LocationTypes, Does.Contain(ELocationType.solidSurface));
			Assert.That(coastr.OperationCondition_LocationTypes, Does.Contain(ELocationType.liquidSurface));

			Technology nvlcbt = Technology.All["nvlcbt"];
			Assert.That(nvlcbt.Level, Is.EqualTo(1));
			Assert.That(nvlcbt.UseProduceModules.Name, Is.EqualTo("gunbot"));
			ModuleType gunbot = ModuleType.All["gunbot"];
			Assert.That(gunbot.MoveModes.ContainsKey(EMoveMode.naval), Is.True);
			Assert.That(gunbot.MoveModes.ContainsKey(EMoveMode.ground), Is.False);
			Assert.That(gunbot.OperationCondition_LocationTypes, Does.Contain(ELocationType.solidSurface));
			Assert.That(gunbot.OperationCondition_LocationTypes, Does.Contain(ELocationType.liquidSurface));

			Technology fshng = Technology.All["fshng"];
			Assert.That(fshng.Level, Is.EqualTo(0));
			Assert.That(fshng.UseProduceModules.Name, Is.EqualTo("fshfrm"));
			ModuleType fshfrm = ModuleType.All["fshfrm"];
			Assert.That(fshfrm.Group, Is.EqualTo(EModuleTypesGroup.agricultural));
			Assert.That(fshfrm.OperationCondition_LocationTypes, Does.Contain(ELocationType.liquidSurface));
			Assert.That(fshfrm.OperationCondition_LocationTypes, Does.Not.Contain(ELocationType.solidSurface));
			Assert.That(fshfrm.OperationCondition_AtmosphereResources.ContainsKey("terair"), Is.True);

			Technology fshhrv = Technology.All["fshhrv"];
			Assert.That(fshhrv.Level, Is.EqualTo(0));
			Assert.That(fshhrv.UseProduceItems.ContainsKey(ItemType.All["food"]));
			Assert.That(fshhrv.UseCondition_ModuleType, Is.EqualTo("fshfrm"));
			Assert.That(fshhrv.UseCondition_PlanetTypes, Is.Null.Or.Empty);
			Assert.That(fshhrv.UseCondition_AtmosphereResources.ContainsKey("terair"), Is.True);

			ModuleType wnplnt = ModuleType.All["wnplnt"];
			Assert.That(wnplnt.OperationCondition_LocationTypes, Does.Contain(ELocationType.solidSurface));
			Assert.That(wnplnt.OperationCondition_LocationTypes, Does.Contain(ELocationType.liquidSurface));
		}

		[Test]
		public void LoadGame_CampaignGamein1_HabitableBodiesHaveRace()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Assert.That(Planet.All["P00001"].Races.ContainsKey("terran"), Is.True);
			Assert.That(Planet.All["P00001"].Orbit.Races.Count, Is.EqualTo(0));
			Assert.That(Planet.All["P00001"].Orbit.HasAtmosphere, Is.True);
			Assert.That(Planet.All["P00005"].Races.ContainsKey("terran"), Is.True);
			Assert.That(Planet.All["P00005"].Orbit.Races.Count, Is.EqualTo(0));
			Assert.That(Planet.All["P00005"].Orbit.HasAtmosphere, Is.True);
		}

		[Test]
		public void LoadGame_CampaignGamein1_BeltsHaveComposition()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Assert.That(Planet.All.ContainsKey("P00003"), Is.False);
			Belt heliosBelt = Belt.All["P00003"];
			Assert.That(heliosBelt, Is.Not.InstanceOf<IOrbitHolder>());
			Assert.That(heliosBelt.Composition.Count, Is.GreaterThan(0));
			Assert.That(heliosBelt.Composition[0].ItemType.Name, Is.EqualTo("uraniu"));
			Assert.That(ModuleStack.All["W00001"].Parent, Is.EqualTo(heliosBelt));
			Assert.That(Region.All["R00006"].Exits.Contains(heliosBelt), Is.True);
			Assert.That(Region.All.ContainsKey("R00096"), Is.False);

			Assert.That(Planet.All.ContainsKey("P00007"), Is.False);
			Belt fomalBelt = Belt.All["P00007"];
			Assert.That(ModuleStack.All["W00002"].Parent, Is.EqualTo(fomalBelt));
			Assert.That(Region.All["R00039"].Exits.Contains(fomalBelt), Is.True);

			Assert.That(Belt.All["P00091"].Planet, Is.EqualTo(Planet.All["P00004"]));
			Assert.That(Belt.All["P00092"].Planet, Is.EqualTo(Planet.All["P00008"]));
			foreach (Planet planet in Planet.All.Values)
			{
				Assert.That(planet.PlanetType.Name, Is.Not.EqualTo("abelt"), planet.Name);
				Assert.That(planet.PlanetType.Name, Is.Not.EqualTo("adpnt"), planet.Name);
			}
		}

		[Test]
		public void LoadGame_CampaignGamein1_AldersonGatesHaveOrbitNoCorona()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;

			Alderson heliosGate = Alderson.All["P00009"];
			Assert.That(heliosGate.Orbit, Is.Not.Null);
			Assert.That(heliosGate.Orbit.Name, Is.EqualTo("O00110"));
			Assert.That(heliosGate.Orbit.Exits.Count, Is.EqualTo(0));
			Assert.That(Region.All["R00006"].Exits.Contains(heliosGate.Orbit), Is.False);

			Alderson fomalGate = Alderson.All["P00010"];
			Assert.That(fomalGate.Orbit.Name, Is.EqualTo("O00111"));
			Assert.That(fomalGate.Orbit.Exits.Count, Is.EqualTo(0));
			Assert.That(Region.All["R00039"].Exits.Contains(fomalGate.Orbit), Is.False);
			foreach (Region region in Region.All.Values)
			{
				foreach (Exit exit in region.Exits)
				{
					Orbit orbit = exit.To as Orbit;
					if (orbit != null)
					{
						Assert.That(orbit.OrbitHolder, Is.Not.InstanceOf<Alderson>(),
							region.Name + " should not exit to an Alderson Gate");
					}
				}
			}
		}
	}
}
