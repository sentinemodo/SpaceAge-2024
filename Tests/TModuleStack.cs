using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TModuleStack
	{
		private ModuleStack moduleStack;
		private ModuleType moduleType;
		private Faction faction;
		private Region region;
		private Planet planet;
		private SpaceSystem system;

		public TModuleStack()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.moduleType = new ModuleType("testModuleType");
			this.faction = new Faction("testFaction", "testFaction");
			this.system = new SpaceSystem("testSystem");
			this.planet = new Planet(this.system, "testPlanet");
			this.region = new Region(this.planet, "testRegion");
			this.moduleStack = new ModuleStack(this.region, this.faction, this.moduleType, "testModuleStack");
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
			ClassicAssert.IsTrue(true);
		}

		[Test]
		public void TotalSize()
		{
			this.moduleType.Size = 10;
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			ClassicAssert.AreEqual(10, this.moduleType.Size);		
			ClassicAssert.AreEqual(50, this.moduleStack.Size);		
		}

		[Test]
		public void TechnologyCapacity()
		{
			this.moduleType.TechnologyCapacity = 10;
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			ClassicAssert.AreEqual(10, this.moduleType.TechnologyCapacity);
			ClassicAssert.AreEqual(10, this.moduleStack.TechnologyCapacity);
		}

		[Test]
		public void ConsumeItemStacks()
		{
			ItemType itemType = new ItemType("consumable");
			ItemStack itemStack = new ItemStack(itemType, 10);

			this.moduleType.Consume.Add(itemStack);
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			ClassicAssert.AreEqual(10, this.moduleType.Consume[itemType].Quantity);
			ClassicAssert.AreEqual(50, this.moduleStack.Consume[itemType].Quantity);
		}

		[Test]
		public void AlltheSame()
		{
			ClassicAssert.AreSame(this.moduleStack, ModuleStack.All[this.region][this.moduleStack.Name]);
			ClassicAssert.AreSame(this.moduleStack, ModuleStack.All[this.faction][this.moduleStack.Name]);
			ClassicAssert.AreSame(this.moduleStack, ModuleStack.All[this.moduleStack.Name]);
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
			ClassicAssert.AreEqual(2, ModuleStack.All[this.region][this.moduleStack.Name].Quantity);
			ClassicAssert.AreEqual(2, ModuleStack.All[this.faction][this.moduleStack.Name].Quantity);
			ClassicAssert.AreEqual(2, ModuleStack.All[this.moduleStack.Name].Quantity);
		}

		[Test]
		public void CreateModuleStackFromSameAliasAsPerson()
		{
			// shouldn't create a modulestack if a person was created with that same alias (like new1)
			// otherwise multiple problems arise as whther to treat the created objects as person or stacks

            try
            {
                ModuleStack moduleStack = ModuleStack.All.GetOrCreateNewModuleStack(this.faction, "new1");
                Person person = Person.All.GetOrCreateNewPerson(this.faction, "new1");
            }
            catch (TypeInitializationException ex)
            {
                Console.WriteLine("Exception caught" + ex.Message);
                return;
            }

            // shouldn't get here
            Assert.Fail();
		}

		[Test]
		public void ModuleStackVisibleByNPCFaction()
		{
			Faction faction = new Faction("1", "NPC");
			ClassicAssert.IsTrue(this.moduleStack.Visible(faction));
		}

		[Test]
		public void ModuleStackVisibleByOwnerFaction()
		{
			ClassicAssert.AreEqual(this.region.Name, this.moduleStack.Location.Name, "this are not the same locations");
			ClassicAssert.AreEqual(this.region.ModuleStacks.Count, 1, "more than 1 modulestack");
			ClassicAssert.IsTrue(this.moduleStack.Visible(this.faction));
		}

		[Test]
		public void ModuleStackNotVisibleByFaction()
		{
			Faction otherFaction = new Faction("2", "otherFaction");
			ClassicAssert.IsFalse(this.moduleStack.Visible(otherFaction));
		}

		[Test]
		public void ModuleStackVisibleByFaction_ByOtherModuleStack()
		{
			Faction otherFaction = new Faction("2", "otherFaction");
			ModuleStack otherModuleStack = new ModuleStack(this.region, otherFaction, this.moduleType, "otherModuleStack");
			ClassicAssert.IsTrue(this.moduleStack.Visible(otherFaction));
		}

	}
}
