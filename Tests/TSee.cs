using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TSee : TTest
	{
		[SetUp]
		public void setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}


        [Test]
        public void AssignSeeOrder()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100011"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                // check not seeing officer
                "see person 200001",
                // check seeing officer
                "see person 200002",
                // check not seeing modulestack
                "see 100001",
                // check seeing modulestack
                "see 100012",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(4));

            SeeOrder order = (SeeOrder)testModuleStack.Orders[0];
        }


        [Test]
        public void ExecuteSeeOrder()
        {
            this.AssignSeeOrder();
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["100011"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["100001"];
            ModuleStack testModuleStack3 = this.game.ModuleStacks["100012"];
            Person testPerson1 = this.game.People["200001"];
            Person testPerson2 = this.game.People["200002"];

            // check if the world state changes corretly				
            Assert.That(testModuleStack1.Orders.Count, Is.EqualTo(4));
            Assert.That(Person.All[testModuleStack1.Location, true].ContainsKey("200001"), Is.False);
            Assert.That(Person.All[testModuleStack1.Location, false].ContainsKey("200002"), Is.False);
            Assert.That(Person.All[testModuleStack1.Location, true].ContainsKey("200002"));

            Assert.That(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100001"), Is.False);
            Assert.That(ModuleStack.All[testModuleStack1.Location, false].ContainsKey("100012"), Is.False);
            Assert.That(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100012"));

            testModuleStack1.Orders[0].Execute(this.game.Week);
            testModuleStack1.Orders[1].Execute(this.game.Week);
            testModuleStack1.Orders[2].Execute(this.game.Week);
            testModuleStack1.Orders[3].Execute(this.game.Week);
            testModuleStack1.Orders.RemoveExecuted();
            Assert.That(testModuleStack1.Orders.Count, Is.EqualTo(2));
        }


        [Test]
        public void AssignSeeOrder_person()
        {
            Faction testFaction = this.game.Factions["2"];
            Person testPerson = this.game.People["200002"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#person 200002",
                // check not seeing officer
                "see person 200001",
                // check seeing officer
                "see person 200002",
                // check not seeing modulestack
                "see 100001",
                // check seeing modulestack
                "see 100012",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testPerson.Orders.Count, Is.EqualTo(4));

            SeeOrder order = (SeeOrder)testPerson.Orders[0];
        }


        [Test]
        public void ExecuteSeeOrder_person()
        {
            this.AssignSeeOrder_person();
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["100011"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["100001"];
            ModuleStack testModuleStack3 = this.game.ModuleStacks["100012"];
            Person testPerson1 = this.game.People["200001"];
            Person testPerson2 = this.game.People["200002"];

            // check if the world state changes corretly				
            Assert.That(testPerson2.Orders.Count, Is.EqualTo(4));
            Assert.That(Person.All[testModuleStack1.Location, true].ContainsKey("200001"), Is.False);
            Assert.That(Person.All[testModuleStack1.Location, false].ContainsKey("200002"), Is.False);
            Assert.That(Person.All[testModuleStack1.Location, true].ContainsKey("200002"));

            Assert.That(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100001"), Is.False);
            Assert.That(ModuleStack.All[testModuleStack1.Location, false].ContainsKey("100012"), Is.False);
            Assert.That(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100012"));

            testPerson2.Orders[0].Execute(this.game.Week);
            testPerson2.Orders[1].Execute(this.game.Week);
            testPerson2.Orders[2].Execute(this.game.Week);
            testPerson2.Orders[3].Execute(this.game.Week);
            testPerson2.Orders.RemoveExecuted();
            Assert.That(testPerson2.Orders.Count, Is.EqualTo(2));
        }


        [Test]
        public void AssignSeeOrder_alias()
        {
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                // check not seeing alias
                "see new1",
                "form new with 1 as new1",
                // check seeing alias
                "-see new1",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));

            SeeOrder order = (SeeOrder)testModuleStack.Orders[0];

            List<string> report = testModuleStack.Orders.Report(testFaction);
            Console.WriteLine(string.Concat("#modulestack ", testModuleStack.Name));
            Console.WriteLine(string.Concat("; ", testModuleStack.ReportName));

            for (int i = 0; i < report.Count; i++)
            {
                Console.WriteLine(report[i]);
            }

            Assert.That(report[0], Is.EqualTo("see new100"));
            Assert.That(report[1], Is.EqualTo("form new with 1 as new100"));
            Assert.That(report[2], Is.EqualTo("-see new100"));
        }


        [Test]
        public void ExecuteSeeOrder_alias()
        {
            this.AssignSeeOrder_alias();
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
            ModuleStack testModuleStack_formed = this.game.ModuleStacks["100"];

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));
            Assert.That(ModuleStack.All[testModuleStack.Location].ContainsKey("100001"));
            Assert.That(ModuleStack.All[testModuleStack.Location].ContainsKey("100"), Is.False);
            Assert.That(testModuleStack_formed.IsFormed, Is.False);

            // shouldn't execute, since the stack wasn't formed yet
            testModuleStack.Orders[0].Execute(this.game.Week);
            testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));

            // shouldn't execute, since the order is conditioned
            testModuleStack.Orders[2].Execute(this.game.Week);
            testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));

            // should execute form
            testModuleStack.Orders[1].Execute(this.game.Week);
            FormOrder order = (FormOrder)(testModuleStack.Orders[1]);
            List<string> report = order.Formed.Report(testFaction);
            Console.WriteLine("Formed stack: ");
            for (int i = 0; i < report.Count; i++)
            {
                Console.WriteLine(report[i]);
            }

            testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(ModuleStack.All[testModuleStack.Location].ContainsKey("100001"));
            Assert.That(testModuleStack_formed.IsFormed);
            Assert.That(testModuleStack_formed.Location, Is.EqualTo(testModuleStack.Location));
            Assert.That(ModuleStack.All[testModuleStack.Location].ContainsKey("100"));
            

            // should execute now
            testModuleStack.Orders[0].Execute(this.game.Week);
            testModuleStack.Orders[1].Execute(this.game.Week);
            testModuleStack.Orders.RemoveExecuted();
            
            report = testModuleStack.EventReports.Report(testFaction);
            Console.WriteLine("event reports: ");
            for (int i = 0; i < report.Count; i++)
            {
                Console.WriteLine(report[i]);
            }
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
        }


        [Test]
        public void PeacefulScout108_See109_DeclareEnemy_StartsBattleAfter109Arrives()
        {
            this.game.ClearDictionaries();
            string fixtureDir = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "scout-declare");
            DataFile dataFile = new DataFile(fixtureDir);
            dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
            dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
            dataFile.LoadFactions();
            dataFile.LoadGalaxy();
            dataFile.LoadContracts();
            dataFile.LoadOrders();

            Game game = dataFile.Game;
            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.LoadOrders(Path.Combine(fixtureDir, "orders.2.2.txt"), false);
            ordersReader.LoadOrders(Path.Combine(fixtureDir, "orders.2.3.txt"), false);

            ModuleStack scout = game.ModuleStacks["108"];
            Faction gelvaren = game.Factions["3"];

            for (int week = 1; week <= 13; week++)
            {
                game.Week = week;
                game.ClearExecutedLongOrder();
                game.ClearExecutedImmediateOrders();
                game.ExecuteOrders();
                game.ExecuteBattles();
            }

            Assert.That(scout.IsArmed, Is.True);
            Assert.That(
                scout.EventReports.Any(e => e.Description.Contains("saw") && e.Description.Contains("[109]")),
                Is.True);
            Assert.That(gelvaren.AttitudeToward(game.Factions["2"]), Is.EqualTo(FactionAttitude.Enemy));
            Assert.That(game.Battles.Count, Is.GreaterThan(0));
            Assert.That(
                scout.EventReports.Any(e => e.Week == 9 && e.Description.Contains("declared enemy")),
                Is.True);
        }

	}
}
