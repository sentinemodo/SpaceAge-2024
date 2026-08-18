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
	public class TFormStack : TTest
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
		public void AssignFormOrder()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000009",
                "form new with 2 as \"new1\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is FormOrder);
			FormOrder formOrder = (FormOrder)testModuleStack.Orders[0];
            Assert.That(formOrder.Quantity, Is.EqualTo(2));
            Assert.That(formOrder.Alias, Is.EqualTo("new1"));
		}


		[Test]
		public void ExecuteFormOrder()
		{
			this.AssignFormOrder();
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];
            Assert.That(testModuleStack, Is.Not.Null);
			FormOrder order = (FormOrder)testModuleStack.Orders[0];

			// check if the world state changes correctly				
			ModuleStack formedModuleStack;
			formedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(formedModuleStack.IsFormed, Is.False);
			testModuleStack.Execute(this.game.Week);
			formedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(formedModuleStack.IsFormed);
		}


		[Test]
		public void AssignOrder_UnFormed()
		{
			Faction testFaction = this.game.Factions["2"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack new1",
                // check if active - should execute
                "active 000002",
                "#end"
            };

			Console.WriteLine("Count: " + ModuleStack.All.Count);
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				Console.WriteLine(stack.ReportName);
			}
            Assert.That(ModuleStack.All.Count, Is.EqualTo(25));

            Assert.That(ModuleStack.All[testFaction, "new1", true], Is.Null);

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			
			ModuleStack testModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(testModuleStack, Is.Not.Null, "should find it");
            Console.WriteLine("Name: " + testModuleStack.Name + " Alias: " + testModuleStack.Alias);

            Assert.That(testModuleStack.Name, Is.Not.EqualTo("new1"));
            Assert.That(testModuleStack.Alias, Is.EqualTo("2_new1"));
            Assert.That(testModuleStack.IsFormed, Is.False, testModuleStack.ReportName + " is formed and it should not since this is new test.");

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

			ActiveOrder order = (ActiveOrder)testModuleStack.Orders[0];

			ModuleStack observed = ModuleStack.All[testFaction, order.Observed.Name];
            Assert.That(observed.ReportName, Is.EqualTo("core drill [000002]"));
            Assert.That(observed.IsFormed);
		}


		[Test]
		public void CleanUpVirtualModulestacksAndPeople()
		{
            Sequence.Ints.Push(101);
            Sequence.Ints.Push(100);

            //Assert.Fail("I don't know why, but this stuck");
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000009",
                "form new with 2 as \"new1\"",
                "active new1",
                "active new2",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			// check if the world state changes correctly				
			ModuleStack formedModuleStack1, formedModuleStack2;

			formedModuleStack1 = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(formedModuleStack1, Is.Not.Null);
            Assert.That(formedModuleStack1.IsFormed, Is.False);
			formedModuleStack2 = this.game.ModuleStacks[testFaction, "new2", true];
            Assert.That(formedModuleStack2, Is.Not.Null);
            Assert.That(formedModuleStack2.IsFormed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
			formedModuleStack1 = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(formedModuleStack1, Is.Not.Null);
            Assert.That(formedModuleStack1.IsFormed);
			formedModuleStack2 = this.game.ModuleStacks[testFaction, "new2", true];
            Assert.That(formedModuleStack2, Is.Not.Null);
            Assert.That(formedModuleStack2.IsFormed, Is.False);

			this.game.ClearUnformed();
			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
			formedModuleStack1 = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(formedModuleStack1, Is.Not.Null);
            Assert.That(formedModuleStack1.IsFormed);
			formedModuleStack2 = this.game.ModuleStacks[testFaction, "new2", true];
            Assert.That(formedModuleStack2, Is.Null);
		}


		[Test]
		public void AssignStackOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100002"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100002",
                "stack 000004",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            // 2 because the first one is imported with gamein file
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

			StackOrder order = (StackOrder)testModuleStack.Orders[1];
            Assert.That(ModuleStack.All[order.ParentName].ReportName, Is.EqualTo("factory [000004]"));
		}


		[Test]
		public void AssignOrder_UnFormed2()
		{
			Faction testFaction = this.game.Factions["2"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "active new1",
                "#modulestack new1",
                "active new1",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			ModuleStack testModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(testModuleStack.Name, Is.Not.EqualTo("new1"));
            Assert.That(testModuleStack.Alias, Is.EqualTo("2_new1"));
            Assert.That(testModuleStack.IsFormed, Is.False);

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

			ActiveOrder order = (ActiveOrder)testModuleStack.Orders[0];

			ModuleStack observed = ModuleStack.All[testFaction, order.Observed.Name];
            Assert.That(observed.Alias, Is.EqualTo("2_new1"));
            Assert.That(observed.IsFormed, Is.False);
		}


		[Test]
		public void ExecuteLongOrder()
		{
			// shouldn't execute the same week
			// preventing running long orders in units that already eecuted long order in a given week (eg. use)
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "2 use agrplx as \"new1\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(useOrder.Subject, Is.SameAs(testModuleStack));
            Assert.That(useOrder.Subject.ExecutedLongOrder, Is.False);
            Assert.That(testModuleStack.ExecutedLongOrder, Is.False);

			// check if the world state changes correctly				

			int week = this.game.Week;
			testModuleStack.Execute(week++);
            Assert.That(useOrder.Subject, Is.SameAs(testModuleStack));
            Assert.That(useOrder.Subject.ExecutedLongOrder);
            Assert.That(testModuleStack.ExecutedLongOrder);
		}


        [Test]
        public void AssignStackOrder_person_alias()
        {
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            Person testPerson = this.game.People["200002"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100013"];            

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100013",
                "form new with 1 as new1",
                "#person 200002",
                "stack new1",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testPerson.Orders.Count, Is.EqualTo(1));
            Assert.That(testPerson.Orders[0] is StackOrder);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[0] is FormOrder);
        }


        [Test]
        public void ExecuteStackOrder_person_alias()
        {
            this.AssignStackOrder_person_alias();

            Faction testFaction = this.game.Factions["2"];
            Person testPerson = this.game.People["200002"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100013"];            
            

            // form should execute with no problems
            testModuleStack.Orders[0].Execute(this.game.Week);
            FormOrder order = (FormOrder)(testModuleStack.Orders[0]);
            this.consoleOutReport("Formed stack:", order.Formed, testFaction);
            ModuleStack testModuleStack_formed = this.game.ModuleStacks["100"];

            Assert.That(testPerson.Parent, Is.Not.EqualTo(testModuleStack_formed));
            testPerson.Orders[0].Execute(this.game.Week);
            Assert.That(testPerson.Parent, Is.EqualTo(testModuleStack_formed));
        }


        [Test]
        public void ExecuteLongOrderAfterForm_FormImmediately()
        {
            // should execute the same week
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000006"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000006",
                "form new with 1 as new1",
                "-give 6 terran to new1",
                "#modulestack new1",
                "use hcdril",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

            Assert.That(testModuleStack.Orders[0] is FormOrder);
            FormOrder formOrder = (FormOrder)testModuleStack.Orders[0];

            Assert.That(testModuleStack.Quantity, Is.EqualTo(2));
            this.game.ClearExecutedLongOrder();
            //this.game.ClearFailedToExecuteImmediateOrders();
            this.game.ClearExecutedImmediateOrders();
            this.game.ExecuteOrders();

            ModuleStack newModuleStack = this.game.ModuleStacks["100"];
            ItemType carbon = ItemType.All["carbon"];
            
            this.consoleOutReport("formed unit: ", newModuleStack, testFaction);

            Assert.That(newModuleStack.Quantity, Is.EqualTo(1));
            Assert.That(newModuleStack.ItemStacks[carbon].Quantity, Is.EqualTo(1));
        }


        [Test]
        public void ExecuteLongOrderAfterForm_FormAfterLong()
        {
            // should execute the same week
            Sequence.Ints.Push(101);
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000006"];

            this.consoleOutReport("module stack:", testModuleStack, testModuleStack.Owner);

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000006",
                "use hcdril",
                "-form new with 2 as new1",
                "-give 6 terran to new1",
                "#modulestack new1",
                "use hcdril",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));

            Assert.That(testModuleStack.Orders[1] is FormOrder);
            FormOrder formOrder = (FormOrder)testModuleStack.Orders[1];

            Assert.That(testModuleStack.Quantity, Is.EqualTo(2));

            this.game.Execute();

            ModuleStack newModuleStack = this.game.ModuleStacks["100"];
            ItemType carbon = ItemType.All["carbon"];

            this.consoleOutReport("old unit: ", testModuleStack, testFaction);
            this.consoleOutReport("formed unit: ", newModuleStack, testFaction);

            Assert.That(newModuleStack.Quantity, Is.EqualTo(2));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(carbon));
            Assert.That(testModuleStack.ItemStacks[carbon].Quantity, Is.EqualTo(2));

            //TODO: the itemstacks should drop to the ground from empty stack
        }

	}
}
