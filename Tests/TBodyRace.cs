using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBodyRace : TTest
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
		public void LoadGalaxy_PlanetRace_LoadsOnPlanetNotOrbit()
		{
			this.LoadBodyRaceWorld();
			Planet planet = Planet.All["P00001"];
			Assert.That(planet.Races.ContainsKey("terran"), Is.True);
			Assert.That(planet.Orbit.Races.Count, Is.EqualTo(0));
			Assert.That(planet.Orbit.HasAtmosphere, Is.True);
			string header = string.Join(" ", planet.Orbit.Report(Faction.All["2"]));
			Assert.That(header, Does.Contain("suitable for"));
			Assert.That(header, Does.Contain(Race.All["terran"].ReportName));
		}

		[Test]
		public void LoadGalaxy_MoonRace_LoadsOnMoonNotOrbit()
		{
			this.LoadBodyRaceWorld();
			Moon moon = Moon.All["M00001"];
			Assert.That(moon.Races.ContainsKey("terran"), Is.True);
			Assert.That(moon.Orbit.Races.Count, Is.EqualTo(0));
			Assert.That(moon.Orbit.HasAtmosphere, Is.True);
			Assert.That(Planet.All["P00002"].Orbit.HasAtmosphere, Is.False);
		}

		[Test]
		public void LoadGalaxy_TerairWithoutRace_HasAtmosphere()
		{
			this.LoadBodyRaceWorld();
			Orbit orbit = Orbit.All["O00004"];
			Assert.That(Planet.All["P00003"].Races.Count, Is.EqualTo(0));
			Assert.That(orbit.Races.Count, Is.EqualTo(0));
			Assert.That(orbit.HasAtmosphere, Is.True);
			string header = string.Join(" ", orbit.Report(Faction.All["2"]));
			Assert.That(header, Does.Contain("has atmosphere"));
			Assert.That(header, Does.Not.Contain("suitable for"));
		}

		[Test]
		public void LoadGalaxy_OrbitRace_StillLoadsAsFallback()
		{
			this.LoadBodyRaceWorld();
			Planet planet = Planet.All["P00004"];
			Assert.That(planet.Races.Count, Is.EqualTo(0));
			Assert.That(planet.Orbit.Races.ContainsKey("terran"), Is.True);
			Assert.That(planet.Orbit.HasAtmosphere, Is.True);
		}

		[Test]
		public void SaveGame_PersistsPlanetAndMoonRace()
		{
			this.LoadBodyRaceWorld();
			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_bodyRace.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			Assert.That(saved.SelectSingleNode("//planet[@name='P00001']/race[@type='terran']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//planet[@name='P00001']/orbit/race"), Is.Null);
			Assert.That(saved.SelectSingleNode("//moon[@name='M00001']/race[@type='terran']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//moon[@name='M00001']/orbit/race"), Is.Null);
			Assert.That(saved.SelectSingleNode("//planet[@name='P00004']/orbit/race[@type='terran']"), Is.Not.Null);
			Assert.That(saved.SelectSingleNode("//planet[@name='P00004']/race"), Is.Null);
		}

		private string BodyRaceFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "body-race");
		}

		private void LoadBodyRaceWorld()
		{
			string fixtureDir = this.BodyRaceFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}
	}
}
