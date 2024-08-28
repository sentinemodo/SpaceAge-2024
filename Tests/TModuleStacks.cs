using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Legacy;
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
			this.moduleStacks = new ModuleStacks
            {
                { "testModuleStack 1", new ModuleStack(this.region, this.faction, this.moduleType, "testModuleStack 1") },
                { "testModuleStack 2", new ModuleStack(this.region, this.faction, this.moduleType, "testModuleStack 2") }
            };
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
            Assert.That(true);
		}

		[Test]
		public void Count()
		{
            Assert.That(this.moduleStacks.Count, Is.EqualTo(2));		
		}

		[Test]
		public void ReportList()
		{
			List<string> testlines = new List<string>
            {
                "- Berlin [000005], 2 cities [city].",
                "- Berlin [000005], 2 cities [city].",
                "Exits:"
            };

		}

		[Test]
		public void OwnersList()
		{
            Assert.That(this.moduleStacks.Owners.Count, Is.EqualTo(1));
            Assert.That(this.moduleStacks.Owners[0].Name, Is.EqualTo("testFaction"));

			Faction faction2 = new Faction("testFaction2", "testFaction2");
			this.moduleStacks.Add("testModuleStack 3", new ModuleStack(this.region, faction2, this.moduleType, "testModuleStack 3"));

            Assert.That(this.moduleStacks.Owners.Count, Is.EqualTo(2));
            Assert.That(this.moduleStacks.Owners[0].Name, Is.EqualTo("testFaction"));
            Assert.That(this.moduleStacks.Owners[1].Name, Is.EqualTo("testFaction2"));
		}

    }
}
