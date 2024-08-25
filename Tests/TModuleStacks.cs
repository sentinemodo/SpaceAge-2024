using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TModuleStacks
	{
		private ModuleStacks moduleStacks;
		private ModuleType moduleType;
		private Faction faction;
		private Region region;
		private Planet planet;
		private SpaceSystem system;

		public TModuleStacks()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.moduleType = new ModuleType("testModuleType");
			this.faction = new Faction("testFaction", "testFaction");
			this.system = new SpaceSystem("testSystem");
			this.planet = new Planet(system, "testPlanet");
			this.region = new Region(planet, "testRegion");			
			this.moduleStacks = new ModuleStacks();
			this.moduleStacks.Add("testModuleStack 1", new ModuleStack(this.region, this.faction, this.moduleType, "testModuleStack 1"));
			this.moduleStacks.Add("testModuleStack 2", new ModuleStack(this.region, this.faction, this.moduleType, "testModuleStack 2"));
		}

		[TearDown]
		public void tearDownDataFile()
		{
			ItemType.All.Clear();
			ModuleType.All.Clear();
			ModuleStack.All.Clear();
			Person.All.Clear();
			Region.All.Clear();
			Faction.All.Clear();
		}

		[Test]
		public void SetupTeardown()
		{
			Assert.IsTrue(true);
		}

		[Test]
		public void Count()
		{
			Assert.AreEqual(2, this.moduleStacks.Count);		
		}

		[Test]
		public void ReportList()
		{
			List<string> testlines = new List<string>();
			testlines.Add("- Berlin [000005], 2 cities [city].");
			testlines.Add("- Berlin [000005], 2 cities [city].");

			testlines.Add("Exits:");

		}

		[Test]
		public void OwnersList()
		{
			Assert.AreEqual(1, this.moduleStacks.Owners.Count);
			Assert.AreEqual("testFaction", this.moduleStacks.Owners[0].Name);

			Faction faction2 = new Faction("testFaction2", "testFaction2");
			this.moduleStacks.Add("testModuleStack 3", new ModuleStack(this.region, faction2, this.moduleType, "testModuleStack 3"));

			Assert.AreEqual(2, this.moduleStacks.Owners.Count);
			Assert.AreEqual("testFaction", this.moduleStacks.Owners[0].Name);
			Assert.AreEqual("testFaction2", this.moduleStacks.Owners[1].Name);
		}

	}
}
