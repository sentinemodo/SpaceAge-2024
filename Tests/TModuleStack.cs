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
            Assert.That(true);
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
            Assert.That(this.moduleType.Size, Is.EqualTo(10));
            Assert.That(this.moduleStack.Size, Is.EqualTo(50));		
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
            Assert.That(this.moduleType.TechnologyCapacity, Is.EqualTo(10));
            Assert.That(this.moduleStack.TechnologyCapacity, Is.EqualTo(10));
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
            Assert.That(this.moduleType.Consume[itemType].Quantity, Is.EqualTo(10));
            Assert.That(this.moduleStack.Consume[itemType].Quantity, Is.EqualTo(50));
		}

		[Test]
		public void AlltheSame()
		{
            Assert.That(ModuleStack.All[this.region][this.moduleStack.Name], Is.SameAs(this.moduleStack));
            Assert.That(ModuleStack.All[this.faction][this.moduleStack.Name], Is.SameAs(this.moduleStack));
            Assert.That(ModuleStack.All[this.moduleStack.Name], Is.SameAs(this.moduleStack));
			this.moduleStack.AddModule();
			this.moduleStack.AddModule();
            Assert.That(ModuleStack.All[this.region][this.moduleStack.Name].Quantity, Is.EqualTo(2));
            Assert.That(ModuleStack.All[this.faction][this.moduleStack.Name].Quantity, Is.EqualTo(2));
            Assert.That(ModuleStack.All[this.moduleStack.Name].Quantity, Is.EqualTo(2));
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
            Assert.That(this.moduleStack.Visible(faction));
		}

		[Test]
		public void ModuleStackVisibleByOwnerFaction()
		{
            Assert.That(this.moduleStack.Location.Name, Is.EqualTo(this.region.Name), "this are not the same locations");
            Assert.That(this.region.ModuleStacks.Count, Is.EqualTo(1), "more than 1 modulestack");
            Assert.That(this.moduleStack.Visible(this.faction));
		}

		[Test]
		public void ModuleStackNotVisibleByFaction()
		{
			Faction otherFaction = new Faction("2", "otherFaction");
            Assert.That(this.moduleStack.Visible(otherFaction), Is.False);
		}

		[Test]
		public void ModuleStackVisibleByFaction_ByOtherModuleStack()
		{
			Faction otherFaction = new Faction("2", "otherFaction");
			ModuleStack otherModuleStack = new ModuleStack(this.region, otherFaction, this.moduleType, "otherModuleStack");
            Assert.That(this.moduleStack.Visible(otherFaction), Is.False);
		}

        [Test]
        public void RandomId_SkipsDuplicateGeneratedName()
        {
            Sequence.Ints.Push(101);
            Sequence.Ints.Push(100);
            Sequence.Ints.Push(100);

			Faction testFaction = new Faction("2", "CastePrime");
			ModuleStack moduleStack1 = new ModuleStack(testFaction, "new1");
			ModuleStack moduleStack2 = new ModuleStack(testFaction, "new2");

			Assert.That(moduleStack1.Name, Is.EqualTo("100"));
			Assert.That(moduleStack2.Name, Is.EqualTo("101"));
			Assert.That(moduleStack1.Name, Is.Not.EqualTo(moduleStack2.Name));
        }

    }
}
