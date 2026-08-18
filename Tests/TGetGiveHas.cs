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
	public class TGetGiveHas : TTest
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
		public void AssignGetOrder()
		{			
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                // get exact amount from defined modulestack
                "get 1 iron from 000006",
                // get all except exact amount from defined modulestack
                "get -5 iron from 000006",
                // get all iron from defined modulestack
                "get all iron from 000006",
                // get all resources from defined modulestack
                "get all from 000006",
                // get all resources from all modulestacks
                "get all iron",
                // get all resources from all modulestacks
                "get all",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(6));

			GetOrder order = (GetOrder) testModuleStack.Orders[0];
            Assert.That(order.Quantity, Is.EqualTo(1));
            Assert.That(order.ItemType.Name, Is.EqualTo("iron"));
            Assert.That(order.Transferer.ReportName, Is.EqualTo("core drill [000006]"));
		}


		[Test]
		public void ExecuteGetOrder()
		{
			this.AssignGetOrder();
			ModuleStack receiver = this.game.ModuleStacks["100001"];
			ModuleStack holder = this.game.ModuleStacks["000006"];
			ItemType iron = ItemType.All["iron"];
			ItemType terran = ItemType.All["terran"];

            Assert.That(iron, Is.Not.Null);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(2));

            // check if the world state changes corretly				
            Assert.That(receiver.ItemStacks.ContainsKey(iron), Is.False);
            Assert.That(holder.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(receiver.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(holder.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get exact amount from defined modulestack
			// testcommands.Add("get 1 iron from 000006");
            this.consoleOutReport("before get 1 iron holder", holder, holder.Owner);
            this.consoleOutReport("before get 1 iron receiver", receiver, receiver.Owner);
            this.consoleOutReport("location", holder.Location, holder.Owner);
            this.consoleOutReport("order", receiver.Orders[0], receiver.Owner); 
            
            receiver.Orders[0].Execute(this.game.Week);
            this.consoleOutReport("after get 1 iron", holder, holder.Owner);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(receiver.ItemStacks.ContainsKey(iron));
            Assert.That(receiver.ItemStacks[iron].Quantity, Is.EqualTo(1));
            Assert.That(holder.ItemStacks[iron].Quantity, Is.EqualTo(9));
            Assert.That(receiver.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(holder.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get all except exact amount from defined modulestack
			// testcommands.Add("get -5 iron from 000006");
			receiver.Orders[1].Execute(this.game.Week);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(receiver.ItemStacks[iron].Quantity, Is.EqualTo(5));
            Assert.That(holder.ItemStacks[iron].Quantity, Is.EqualTo(5));
            Assert.That(receiver.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(holder.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get all iron from defined modulestack
			// testcommands.Add("get all iron from 000006");
			receiver.Orders[2].Execute(this.game.Week);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(receiver.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(holder.ItemStacks.ContainsKey(iron), Is.False);
            Assert.That(receiver.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(holder.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get all resources from defined modulestack
			// testcommands.Add("get all from 000006");
			receiver.Orders[3].Execute(this.game.Week);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(receiver.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(receiver.ItemStacks[terran].Quantity, Is.EqualTo(13));
			// get all resources from all modulestacks
			// testcommands.Add("get all iron");
			receiver.Orders[4].Execute(this.game.Week);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(receiver.ItemStacks[iron].Quantity, Is.EqualTo(15));
            Assert.That(receiver.ItemStacks[terran].Quantity, Is.EqualTo(13));
			// get all resources from all modulestacks
			// testcommands.Add("get all");
			receiver.Orders[5].Execute(this.game.Week);
            Assert.That(receiver.ItemStacks.Count, Is.EqualTo(6));

			List<string> reportLines = receiver.EventReports.Report(receiver.Owner);
			foreach (string reportLine in reportLines)
			{
				System.Console.WriteLine(reportLine);
			}
		}


		[Test]
		public void AssignGiveOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000006"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000006",
                // give exact amount to defined modulestack
                "give 1 iron to 100001",
                // give all except exact amount to defined modulestack
                "give -5 iron to 100001",
                // give all iron to  defined modulestack
                "give all iron to 100001",
                // give all resources to defined modulestack
                "give all to 100001",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(4));

			GiveOrder order = (GiveOrder)testModuleStack.Orders[0];
            Assert.That(order.Quantity, Is.EqualTo(1));
            Assert.That(order.ItemType.Name, Is.EqualTo("iron"));
            Assert.That(ModuleStack.All[order.ReceiverName].ReportName, Is.EqualTo("trucks [100001]"));
		}


		[Test]
		public void ExecuteGiveOrder()
		{
			//@give 20 iron to new1
			this.AssignGiveOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			ModuleStack testModuleStack2 = this.game.ModuleStacks["000006"];
						
			ItemType iron = ItemType.All["iron"];
			ItemType terran = ItemType.All["terran"];

            Assert.That(iron, Is.Not.Null);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(2));

            // check if the world state changes corretly				
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);
            Assert.That(testModuleStack2.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(testModuleStack2.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get exact amount from defined modulestack
			//testcommands.Add("give 1 iron to 100001");			
			testModuleStack2.Orders[0].Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(1));
            Assert.That(testModuleStack2.ItemStacks[iron].Quantity, Is.EqualTo(9));
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(testModuleStack2.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get all except exact amount from defined modulestack
			//testcommands.Add("give -5 iron to 100001");
			testModuleStack2.Orders[1].Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(5));
            Assert.That(testModuleStack2.ItemStacks[iron].Quantity, Is.EqualTo(5));
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(testModuleStack2.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get all iron from defined modulestack
			//testcommands.Add("give all iron to 100001");
			testModuleStack2.Orders[2].Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack2.ItemStacks.ContainsKey(iron), Is.False);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(1));
            Assert.That(testModuleStack2.ItemStacks[terran].Quantity, Is.EqualTo(12));
			// get all resources from defined modulestack
			//testcommands.Add("give all to 100001"); 
			testModuleStack2.Orders[3].Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(13));

			List<string> reportLines = testModuleStack2.EventReports.Report(testModuleStack.Owner);
			foreach (string reportLine in reportLines)
			{
				System.Console.WriteLine(reportLine);
			}
		}


		[Test]
		public void AssignHasOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                // check having iron
                "has 3 iron",
                "get 1 iron from 000006",
                "get 2 iron from 000006",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));

			HasOrder order = (HasOrder)testModuleStack.Orders[0];
            Assert.That(order.Quantity, Is.EqualTo(3));
		}


		[Test]
		public void ExecuteHasOrder()
		{
			//has 20 iron
			this.AssignHasOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			ItemType iron = ItemType.All["iron"];

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);
			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);
			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(1));

			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(1));

			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(3));

			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
		}


		[Test]
		public void AssignAndExecuteHasOrder_ModuleType_Recursive()
		{
			// Frigate 100011 (type sshull) contains a nested stack 100013 of 2 'fisrec' modules.
			// 'has <qty> <moduletype>' counts modules of that type across the stack and its
			// nested sub-stacks (semantics B: recursive containment).
			ModuleStack frigate = this.game.ModuleStacks["100011"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "has 2 fisrec", // 2 fisrec modules live in nested stack 100013
                "has 3 fisrec", // one more than exist -> should not execute
                "has 1 sshull", // the observed stack itself is 1 sshull module
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.That(frigate.Orders.Count, Is.EqualTo(3));

			HasOrder has2fisrec = (HasOrder)frigate.Orders[0];
			HasOrder has3fisrec = (HasOrder)frigate.Orders[1];
			HasOrder has1sshull = (HasOrder)frigate.Orders[2];

			Assert.That(has2fisrec.Quantity, Is.EqualTo(2));

			has2fisrec.Execute(this.game.Week);
			Assert.That(has2fisrec.Executed, "has 2 fisrec should execute (2 fisrec modules in nested stack 100013)");

			has3fisrec.Execute(this.game.Week);
			Assert.That(has3fisrec.Executed, Is.False, "has 3 fisrec should not execute (only 2 exist)");

			has1sshull.Execute(this.game.Week);
			Assert.That(has1sshull.Executed, "has 1 sshull should execute (the stack itself is 1 sshull module)");
		}


		[Test]
		public void ExecuteGiveOrder_toUnformed()
		{
			//@give 20 iron to new1
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000006"];

			ItemType iron = ItemType.All["iron"];
            Assert.That(iron, Is.Not.Null);

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000006",
                // give exact amount to unformed modulestack
                "give 1 iron to new1",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

			GiveOrder order = (GiveOrder)testModuleStack.Orders[0];
            Assert.That(order.Quantity, Is.EqualTo(1));
            Assert.That(order.ItemType.Name, Is.EqualTo("iron"));

			Console.WriteLine(order.ReceiverName);

			ModuleStack testModuleStack2 = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(testModuleStack2.IsFormed, Is.False);
            Assert.That(testModuleStack2.ItemStacks.ContainsKey(iron), Is.False);

            Assert.That(testModuleStack2.Capacity, Is.EqualTo(0));

            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(2));

			// give exact amount to undefined modulestack
			testModuleStack.Orders[0].Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron));
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack2.ItemStacks.ContainsKey(iron), Is.False);

			List<string> reportLines = testModuleStack2.EventReports.Report(testModuleStack.Owner);
			foreach (string reportLine in reportLines)
			{
				System.Console.WriteLine(reportLine);
			}
		}


		[Test]
		public void ExecuteHasOrder_unFormed()
		{
			//has 20 iron
			Faction testFaction = this.game.Factions["2"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack new1",
                // check having iron
                "move O00003",
                "+has 3 iron",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			ModuleStack testModuleStack = this.game.ModuleStacks[testFaction, "new1", true];

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

			HasOrder order = (HasOrder)testModuleStack.Orders[1];
            Assert.That(order.Quantity, Is.EqualTo(3));
			ItemType iron = ItemType.All["iron"];

            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);
			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
		}


		[Test]
		public void AssignHasOrder_HasPerson()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100012"];
			Person testPerson1 = this.game.People["200002"];
			Person testPerson2 = this.game.People["200003"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100012",
                // check having person
                "has person 200002",
                "has person 200003",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

			HasOrder order1 = (HasOrder)testModuleStack.Orders[0];
            Assert.That(order1.PersonName, Is.EqualTo(testPerson1.Name));

			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
            Assert.That(order2.PersonName, Is.EqualTo(testPerson2.Name));
		}


		[Test]
		public void ExecuteHasOrder_HasPerson()
		{
			//has person 200002
			this.AssignHasOrder_HasPerson();
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100012"];
			Person testPerson1 = this.game.People["200002"];
			Person testPerson2 = this.game.People["200003"];

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.People.ContainsKey("200002"));
            Assert.That(testModuleStack.People.ContainsKey("200003"), Is.False);
			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.People.ContainsKey("200002"));
            Assert.That(testModuleStack.People.ContainsKey("200003"), Is.False);
		}


        [Test]
        public void ExecuteHasOrder_countModules()
        {

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "has modules 3",
                "has modules 2",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.Quantity, Is.EqualTo(2));
            this.consoleOutReport("stack:", testModuleStack, testFaction);
            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);

            // secound counts should execute
            testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            this.consoleOutReport("stack:", testModuleStack, testFaction);
            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);

            testModuleStack.AddModule();
            // first counts should execute
            testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            this.consoleOutReport("stack:", testModuleStack, testFaction);
            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);
        }


        [Test]
        public void ExecuteHasOrder_ModuleType_triggersAfterCrossStackProduction()
        {
            this.game.ClearDictionaries();
            string fixtureDir = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "has-order-production");
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

            ModuleStack tanks = game.ModuleStacks["109"];
            Assert.That(tanks.Quantity, Is.EqualTo(2));
            Assert.That(tanks.Orders.Count, Is.GreaterThan(0));

            for (int week = 1; week <= 3; week++)
            {
                game.Week = week;
                game.ClearExecutedLongOrder();
                game.ClearExecutedImmediateOrders();
                game.ExecuteOrders();
            }

            Assert.That(tanks.Quantity, Is.EqualTo(3), "factory should deliver third tank on week 3");
            HasOrder hasOrder = tanks.Orders.Find(o => o is HasOrder) as HasOrder;
            Assert.That(hasOrder, Is.Null, "has 3 tanks should have executed on week 3");
            Assert.That(tanks.ItemStacks.ContainsKey(ItemType.All["terran"]), Is.True, "get terran should run after has");
            Assert.That(
                tanks.EventReports.Any(eventReport => eventReport.Week == 3 && eventReport.Description.Contains("departed from")),
                Is.False,
                "move is blocked on week 3 while production completes");
            Assert.That(
                tanks.EventReports.Any(eventReport => eventReport.Week == 3 && eventReport.Description.Contains("got 48 terrans")),
                Is.True,
                "refuel GETs should run on week 3");

            game.Week = 4;
            game.ClearExecutedLongOrder();
            game.ClearExecutedImmediateOrders();
            game.ExecuteOrders();

            Assert.That(
                tanks.EventReports.Any(eventReport => eventReport.Week == 4 && eventReport.Description.Contains("departed from")),
                Is.True,
                "move should start on week 4 once ExecutedLongOrder clears");

            for (int week = 5; week <= 13; week++)
            {
                game.Week = week;
                game.ClearExecutedLongOrder();
                game.ClearExecutedImmediateOrders();
                game.ExecuteOrders();
            }

            Assert.That(tanks.Parent, Is.SameAs(Region.All["R00002"]),
                "tanks should be a root stack in Northern Hemisphere after move completes");
            Assert.That(tanks.IsRootModuleStack, Is.True);

            Faction faction = game.Factions["3"];
            List<string> galaxyReport = game.Galaxy.Report(faction);
            int start = galaxyReport.FindIndex(line => line.Contains("+ Surrender or die! [109]"));
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "galaxy report should list tanks in Northern Hemisphere");
            int southern = galaxyReport.FindIndex(line => line.StartsWith("  Southern Hemisphere [R00003]"));
            Assert.That(start, Is.LessThan(southern), "tanks should appear under Northern Hemisphere, not Southern");
        }


        [Test]
        public void ExecuteGetOrder_differentLocations()
        {
            //@get from 000014 all food
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["000006"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "get 1 iron from 000006",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            testModuleStack.Parent = Region.All["R00002"];

            testModuleStack.Orders.Execute(this.game.Week);
            // should not execute - not the same location
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            testModuleStack.Parent = Region.All["R00001"];
            testModuleStack.Orders.Execute(this.game.Week);
            // should execute this time same location
        }


		[Test]
		public void InfiniteUseGiveLoop()
		{
			//+ small cargo bay [000019], small cargo bay [cargob].
			//	events:
			//		week 1: got 10 units of food [food] from farming complex
			//		[000020].
			//		week 3: got 10 units of food [food] from farming complex
			//		[000020].
			//		week 4: got 10 units of food [food] from farming complex
			//		[000020].
			//+ farming complex [000020], 2 farming complexes [farms].
			//	events:
			//		week 1: produced 10 units of food [food].
			//		week 1: given 10 units of food [food] to small cargo bay
			//		[000019].
			//		week 2: produced 10 units of food [food].
			//		week 3: given 10 units of food [food] to small cargo bay
			//		[000019].
			//		week 3: produced 10 units of food [food].
			//		week 4: given 10 units of food [food] to small cargo bay
			//		[000019].
			//		week 4: produced 10 units of food [food].

			//Assert.Fail("@produce food, @get all food by other module - immediate is not done on 2 week");

			Faction faction = this.game.Factions["1"];
			ModuleStack producer = this.game.ModuleStacks["000008"];
            Assert.That(producer.FullName, Is.EqualTo("Berlin farms"));
			ModuleStack receiver = this.game.ModuleStacks["000005"];
            Assert.That(receiver.FullName, Is.EqualTo("Berlin"));

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000005",
                "@get all food from 000008",
                "#modulestack 000008",
                "@use farmng",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(producer.Orders.Count, Is.EqualTo(1));
            Assert.That(receiver.Orders.Count, Is.EqualTo(1));

			UseOrder useOrder = (UseOrder)producer.Orders[0];
			GetOrder getOrder = (GetOrder)receiver.Orders[0];
			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology farming = Technology.All["farmng"];
            Assert.That(farming.UseTime, Is.EqualTo(1), "if that change, the below test would be invalid");

            // check if the world state changes correctly	
            Assert.That(receiver.ItemStacks.ContainsKey(food), Is.False);
			ModuleStacks stacks = new ModuleStacks
            {
                producer,
                receiver
            };

			this.game.ExecuteOrdersByModuleStack(stacks);

            Assert.That(producer.Effects.IsProducing, Is.False, "Shouldn't be producing - farming is 1 duration order");
            Assert.That(receiver.ItemStacks.ContainsKey(food));
            Assert.That(receiver.ItemStacks[food].Quantity, Is.EqualTo(40 + producer.Quantity * farming.UseProduceItems[food].Quantity));
            Assert.That(producer.ItemStacks.ContainsKey(food), Is.False);
            Assert.That(producer.Orders.Count, Is.EqualTo(1));
            Assert.That(receiver.Orders.Count, Is.EqualTo(1));
            Assert.That(useOrder.Executing, Is.False);
            Assert.That(getOrder.Executing, Is.False);
            Assert.That(useOrder.Executed, Is.False);
            Assert.That(getOrder.Executed);

			this.game.ClearExecutedLongOrder();
			//this.game.ClearFailedToExecuteImmediateOrders();
			this.game.ClearExecutedImmediateOrders();
			this.game.ClearUnformed();

			this.game.Week++;
			this.game.ExecuteOrdersByModuleStack(stacks);
            Assert.That(receiver.ItemStacks[food].Quantity, Is.EqualTo(40 + 2 * producer.Quantity * farming.UseProduceItems[food].Quantity));
            Assert.That(producer.ItemStacks.ContainsKey(food), Is.False);
            Assert.That(producer.Orders.Count, Is.EqualTo(1));
            Assert.That(receiver.Orders.Count, Is.EqualTo(1));
            Assert.That(useOrder.Executing, Is.False);
            Assert.That(getOrder.Executing, Is.False);
            Assert.That(useOrder.Executed, Is.False);
            Assert.That(getOrder.Executed);

			this.consoleOutReport("producer: ", producer, faction);
			this.consoleOutReport("receiver: ", receiver, faction);
		}

	}
}
