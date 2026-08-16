using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TOrder : TTest
	{
		public TOrder()
		{			
		}
			
		[SetUp]
		public void setupOrder()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardownOrder()
		{
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
		}

		[Test]
		public void SetupTeardown()
		{
            Assert.That(true);
		}

		[Test]
		public void ReadOrdersFile()
		{
			OrdersReader ordersReader = new OrdersReader(game);
			List<string> testlines = new List<string>
            {
                "#faction 2",
                "",
                "#modulestack 100001",
                "move R00002",
                "",
                "#end"
            };
			List<string> lines = ordersReader.ReadOrdersFile(Path.Combine(Directory.GetCurrentDirectory(), "orders.move.txt"));
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));
			for (int i = 0; i < lines.Count ; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]));
			}			
		}

		[Test]
		public void AssignMoveOrder()
		{			
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "move R00002",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

			MoveOrder order = (MoveOrder) testModuleStack.Orders[0];
            Assert.That(order.Route.Count, Is.EqualTo(1));
		}

		[Test]
		public void AssignOrderSpike()
		{
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			MoveOrder mo = new MoveOrder(testModuleStack);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[0], Is.EqualTo(mo));			
		}

		[Test]
		public void FactionTagSpike()
		{
			string command = "#faction 2";
			string token = LineParser.GetToken(ref command);
            Assert.That(token, Is.EqualTo("#faction"));
            Assert.That(command, Is.EqualTo("2"));
		}

		[Test]
		public void ExecuteMoveOrder()
		{
			this.AssignMoveOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];			
			MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];
            Assert.That(region1.Exits[0].ExitModes[EMoveMode.ground].Duration, Is.EqualTo(3));

            // check if the world state changes corretly	
            Assert.That(testModuleStack.MovingTo, Is.Null);
            Assert.That(testModuleStack.Parent, Is.EqualTo(region1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(region2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(region1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(5));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(region2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(region1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(4));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(region2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(region1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(3));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(region2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(region1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(2));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(region2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(region1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.MovingTo, Is.Null);
            Assert.That(testModuleStack.Parent, Is.EqualTo(region2));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);

			testModuleStack.Orders.RemoveExecuted();
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));			
		}

		[Test]
		public void AssignUseOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["agrplx"]));
            Assert.That(useOrder.Repeat, Is.EqualTo(1));
		}

		[Test]
		public void AgrplxSpike()
		{
			Technology technology = Technology.All["agrplx"];
            Assert.That(technology.UseProduceItems, Is.Null);
            Assert.That(technology.UseProduceModules, Is.Not.Null);
            Assert.That(technology.UseTime, Is.EqualTo(4));
		}

		[Test]
		public void ExecuteUseOrder()
		{
			this.AssignUseOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			UseOrder order = (UseOrder)testModuleStack.Orders[0];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];

            Assert.That(Technology.All["agrplx"].UseTime, Is.EqualTo(4));

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(20));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
            Assert.That(order.Repeat, Is.EqualTo(1));
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsProducing);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(8));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(order.DurationLeft, Is.EqualTo(3), "Producer is having " + testModuleStack.Quantity.ToString() + " modules");
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(8));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
			this.consoleOutReport("producer: ", testModuleStack, Faction.All["2"]);

            Assert.That(order.DurationLeft, Is.EqualTo(2));
			testModuleStack.Effects.RemoveExecuted();
            Assert.That(testModuleStack.Effects.IsProducing);

			testModuleStack.ExecutedLongOrder = false;
            Assert.That(order.Repeat, Is.EqualTo(1));
			testModuleStack.Execute(this.game.Week);
            Assert.That(order.DurationLeft, Is.EqualTo(1));
            Assert.That(testModuleStack.Effects.IsProducing);

			testModuleStack.ExecutedLongOrder = false;
            Assert.That(order.Repeat, Is.EqualTo(1));
			testModuleStack.Execute(this.game.Week);
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(8));

            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);
            Assert.That(order.Repeat, Is.EqualTo(0));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
		}

		[Test]
		public void AssignBuyOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "buy 5 terran at 5",
                "#end"
            };

            Assert.That(Offer.All.Count, Is.EqualTo(9));

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is BuyOrder);
			BuyOrder buyOrder = (BuyOrder)testModuleStack.Orders[0];
            Assert.That(buyOrder.ItemType, Is.EqualTo(ItemType.All["terran"]));
            Assert.That(buyOrder.Repeat, Is.EqualTo(1));

            Assert.That(Offer.All.Count, Is.EqualTo(9), "assign shouldn't change number of offers");
		}

		[Test]
		public void ExecuteBuyOrder_ImmediateRange()
		{
			this.AssignBuyOrder();

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			Market market = testModuleStack.Location.Market;
			BuyOrder order = (BuyOrder)testModuleStack.Orders[0];
			ItemType terran = ItemType.All["terran"];
			ItemType cash = ItemType.All["cash"];

            // check if the world state changes corretly	
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(300));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);
            Assert.That(testModuleStack.Effects.Count, Is.EqualTo(0));
			Console.WriteLine("existing offers");
			List<string> lines = testModuleStack.Location.Market.Report(testFaction);

			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}

            Assert.That(Offer.All[market].Count, Is.EqualTo(4));

			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(10), "shouldn'teardownOrder execute - offers are to expensive");
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(300));
            Assert.That(testModuleStack.Owner.Bank.AvailableFunds, Is.EqualTo(20000));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False, "it's immediate, can't be continously executing");
            Assert.That(order.Executed, Is.False, "offer is placed but not completed - not executed");

			Console.WriteLine("existing offers stage 2");
			lines = testModuleStack.Location.Market.Report(testFaction);
			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}

            Assert.That(Offer.All[market].Count, Is.EqualTo(5));


			Offers testOffers = Offer.All[testModuleStack][EOfferType.BuyItems];
            Assert.That(testOffers.Count, Is.EqualTo(1));
			foreach (Offer offer in testOffers)
			{
                Assert.That(offer.Offerent, Is.EqualTo(testModuleStack));
			}

			order.Buy.Price = 50;
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(15));
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(300));
            Assert.That(testModuleStack.Owner.Bank.AvailableFunds, Is.EqualTo(19750)); // used bank account for transaction
            Assert.That(testModuleStack.Effects.Count, Is.EqualTo(0), "there was some kind of effect planned");
            Assert.That(Offer.All[market].Count, Is.EqualTo(4));
		}

		[Test]
		public void ExecuteMoveOrder_fuel()
		{
			this.AssignMoveOrder();
            Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			ItemType oil = ItemType.All["oil"];

            Assert.That(oil, Is.Not.Null);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(2));

            // check if the world state changes correctly				
            Assert.That(testModuleStack.ItemStacks[oil].Quantity, Is.EqualTo(2));
            Assert.That(testModuleStack.Effects.Count, Is.EqualTo(0));

			testModuleStack.Orders.Execute(this.game.Week);
            this.consoleOutReport("itemstacks: ", testModuleStack.ItemStacks, testFaction);
            this.consoleOutReport("modulestack: ", testModuleStack, testFaction);
            Assert.That(testModuleStack.ItemStacks.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Effects.Count, Is.EqualTo(2)); // moving and fuelled
            Assert.That(testModuleStack.Effects.IsFuelled);
            Assert.That(testModuleStack.Effects.IsMoving);
            Assert.That(testModuleStack.Effects[1] is Fuelled);
            Assert.That(testModuleStack.Effects[1].Duration, Is.EqualTo(13));

			testModuleStack.Effects.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects[1].Duration, Is.EqualTo(12));
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
		public void AssignNameOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "name \"test name\"",
                "name R00002 \"test region name\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

			NameOrder order = (NameOrder)testModuleStack.Orders[0];
            Assert.That(order.Description, Is.EqualTo("test name"));
		}

		[Test]
		public void ExecuteNameOrder()
		{
			//@name "Caste Prime valiant explorer"
			this.AssignNameOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			Region testRegion = this.game.Regions["R00002"];

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

            // check if the world state changes corretly				
            Assert.That(testModuleStack.ReportName, Is.EqualTo("factory [000004]"));
            Assert.That(testRegion.ReportName, Is.EqualTo("Eastern Europe [R00002] (1,4)"));
			testModuleStack.Orders.Execute(this.game.Week);
            Assert.That(testModuleStack.ReportName, Is.EqualTo("test name [000004]"));
            Assert.That(testRegion.ReportName, Is.EqualTo("test region name [R00002] (1,4)"));
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
		public void AssignRepeatableOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "2 use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

			UseOrder order = (UseOrder)testModuleStack.Orders[0];
            Assert.That(order.Repeat, Is.EqualTo(2));
		}

		[Test]
		public void ExecuteRepeatableOrder()
		{
			this.AssignRepeatableOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			UseOrder order = (UseOrder)testModuleStack.Orders[0];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(20));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsProducing);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(3));
            Assert.That(order.Repeat, Is.EqualTo(2));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False); 
			
			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);			
			}

            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsProducing);
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(8));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(3));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);
			}

            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);
            // no change here - both farms are to be produced into single modulestack
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);
		}

		[Test]
		public void AssignMinusConditionedOrders()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "has 30 iron",
                "-use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));			
			HasOrder order1 = (HasOrder)testModuleStack.Orders[0];
			UseOrder order2 = (UseOrder)testModuleStack.Orders[1];
            Assert.That(order2.ConditionalOrders.Count, Is.EqualTo(1));
            Assert.That(order2.ConditionalOrders[0], Is.SameAs(order1));
		}

		[Test]
		public void AssignMinusConditionedOrders2()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "get 10 iron from 000002",
                "has 30 iron",
                "-use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));
			GetOrder order1 = (GetOrder)testModuleStack.Orders[0];
			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
			UseOrder order3 = (UseOrder)testModuleStack.Orders[2];
            Assert.That(order3.ConditionalOrders.Count, Is.EqualTo(1));
            Assert.That(order3.ConditionalOrders[0], Is.SameAs(order2));
            Assert.That(order2.ConditionedOrders.Count, Is.EqualTo(1));
            Assert.That(order2.ConditionedOrders[0], Is.SameAs(order3));
		}

		[Test]
		public void AssignMinusConditionedOrders3()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "@get 10 iron from 000002",
                "has 30 iron",
                "-use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));
			GetOrder order1 = (GetOrder)testModuleStack.Orders[0];
			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
			UseOrder order3 = (UseOrder)testModuleStack.Orders[2];
            Assert.That(order3.ConditionalOrders.Count, Is.EqualTo(1));
            Assert.That(order3.ConditionalOrders[0], Is.SameAs(order2));
            Assert.That(order2.ConditionedOrders.Count, Is.EqualTo(1));
            Assert.That(order2.ConditionedOrders[0], Is.SameAs(order3));
		}

		[Test]
		public void ExecuteMinusConditionedOrders()
		{
			//has 30 iron
			//-use agrplx
			// use can be executed only if has been executed
			this.AssignMinusConditionedOrders();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
			HasOrder order1 = (HasOrder)testModuleStack.Orders[0];
			UseOrder order2 = (UseOrder)testModuleStack.Orders[1];

			ItemType iron = ItemType.All["iron"];

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(20));
			
			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(order2.Executing, Is.False);
            Assert.That(order2.Executed, Is.False);
			testModuleStack.ItemStacks[iron].Quantity += 10;
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(30));

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsProducing);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(20));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order2.DurationLeft, Is.EqualTo(3));
            Assert.That(order1.Executed);
            Assert.That(order2.Executing);
            Assert.That(order2.Executed, Is.False);

			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);
			}

            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            Assert.That(order2.Executing, Is.False);
            Assert.That(order2.Executed);
		}

		[Test]
		public void AssignPlusConditionedOrders()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "use agrplx",
                "+has 30 iron",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
			UseOrder order1 = (UseOrder)testModuleStack.Orders[0];
			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
            Assert.That(order1.ConditionalOrders.Count, Is.EqualTo(1));
            Assert.That(order1.ConditionalOrders[0], Is.SameAs(order2));
		}

		[Test]
		public void ExecutePlusConditionedOrders()
		{
			//use agrplx
			//+has 30 iron
			// use can be executed only if all plus conditioned are executed before

			this.AssignPlusConditionedOrders();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
			HasOrder order1 = (HasOrder)testModuleStack.Orders[1];
			UseOrder order2 = (UseOrder)testModuleStack.Orders[0];

			ItemType iron = ItemType.All["iron"];

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(20));
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(order2.Executing, Is.False);
            Assert.That(order2.Executed, Is.False);
			testModuleStack.ItemStacks[iron].Quantity += 10;
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(30));

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsProducing);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(20));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order2.DurationLeft, Is.EqualTo(3));
            Assert.That(order1.Executed);
            Assert.That(order2.Executing);
            Assert.That(order2.Executed, Is.False);

			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);
			}

            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(7 + 1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            Assert.That(order2.Executing, Is.False);
            Assert.That(order2.Executed);
		}

		[Test]
		public void AssignAliasOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "alias \"new1\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is AliasOrder);
			AliasOrder aliasOrder = (AliasOrder)testModuleStack.Orders[0];
            Assert.That(aliasOrder.Alias, Is.EqualTo("new1"));
		}

		[Test]
		public void AssignAliasOrder_noquotes()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "alias new1",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is AliasOrder);
			AliasOrder aliasOrder = (AliasOrder)testModuleStack.Orders[0];
            Assert.That(aliasOrder.Alias, Is.EqualTo("new1"));
		}

		[Test]
		public void ExecuteAliasOrder()
		{
			this.AssignAliasOrder();
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			AliasOrder order = (AliasOrder)testModuleStack.Orders[0];

            // check if the world state changes correctly				
            Assert.That(this.game.ModuleStacks["2_new1"], Is.Null);
            Assert.That(testModuleStack.Alias, Is.EqualTo("2_000004"));
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Alias, Is.EqualTo("2_new1"));
            Assert.That(this.game.ModuleStacks[testFaction, "new1", true], Is.SameAs(testModuleStack));

		}


		[Test]
		public void AssignUseOrder_Alias()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "use agrplx as \"new1\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["agrplx"]));
            Assert.That(useOrder.Receiver.Name, Is.EqualTo("100"));
		}

		[Test]
		public void ExecuteUseOrder_Alias()
		{
			this.AssignUseOrder_Alias();
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			UseOrder order = (UseOrder)testModuleStack.Orders[0];

			// check if the world state changes correctly				
			ModuleStack producedModuleStack;
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			//ClassicAssert.IsNull(producedModuleStack);
			testModuleStack.Execute(this.game.Week);
			ProducingModule effect = (ProducingModule) testModuleStack.Effects[0];

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(producedModuleStack, Is.Not.Null);

            Assert.That(producedModuleStack, Is.SameAs(effect.Produced));
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
		public void AssignMoveOrder_PlanetDestination()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100011"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "move P00001 P00002 O00003 S00001",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

			MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
            Assert.That(order.Route.Count, Is.EqualTo(4));
		}

		[Test]
		public void RemoveCommentsAndEmptyLines()
		{
			OrdersReader ordersReader = new OrdersReader(game);
			List<string> testlines = new List<string>
            {
                "#faction 2",
                "",
                "#modulestack 100001",
                "; some modulestack",
                "move R00002",
                "",
                "#modulestack 100002",
                "; some other modulestack",
                "",
                "#end"
            };
			
			List<string> testlines2 = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "move R00002",
                "#modulestack 100002",
                "#end"
            };

			List<string> lines = ordersReader.RemoveCommentsAndEmptyLines(testlines);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

            Assert.That(lines.Count, Is.EqualTo(testlines2.Count));
			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines2[i]));
			}
		}

		[Test]
		public void RemoveCommentsAndEmptyLines_DoubleSlashComments()
		{
			OrdersReader ordersReader = new OrdersReader(game);
			List<string> testlines = new List<string>
            {
                "#faction 2",
                "// full-line double-slash comment",
                "#modulestack 100001",
                "move R00002 // trailing double-slash comment",
                "; full-line semicolon comment",
                "move R00003 ; trailing semicolon comment",
                "use filidx // build research lab // second slash run",
                "#end"
            };

			List<string> expected = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "move R00002",
                "move R00003",
                "use filidx",
                "#end"
            };

			List<string> lines = ordersReader.RemoveCommentsAndEmptyLines(testlines);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			Assert.That(lines.Count, Is.EqualTo(expected.Count));
			for (int i = 0; i < lines.Count; i++)
			{
				Assert.That(lines[i], Is.EqualTo(expected[i]));
			}
		}

		[Test]
		public void AssignUseOrder_ForClauseWithoutAs()
		{
			// "use <tech> for <id>" produces the module into an existing stack without
			// naming a new receiver alias (the 'as' clause is optional).
			ModuleStack factory = this.game.ModuleStacks["100001"];
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "use wndtrb for 000021",
                "#end"
            };
			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			UseOrder order = (UseOrder)factory.Orders[0];
			Assert.That(order.Technology.Name, Is.EqualTo("wndtrb"));
			Assert.That(order.Receiver, Is.Not.Null); // auto-generated receiver
			Assert.That(order.ReceiverParent, Is.Not.Null);
			Assert.That(((ModuleStack)order.ReceiverParent).Name, Is.EqualTo("000021"));
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
		public void AssignActiveOrder_MultipleConditionedOrders()
		{			
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "active new1",
                "-give 20 iron to new1",
                "-give 20 titani to new1",
                "-give 20 silici to new1",
                "-give 20 copper to new1",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(5));

			ModuleStack observed;
			ActiveOrder order1 = (ActiveOrder) testModuleStack.Orders[0];
			observed = order1.Observed;
            Assert.That(observed, Is.Not.Null);
            Assert.That(observed.Alias, Is.EqualTo("2_new1"));
            Assert.That(order1.ConditionedOrders.Count, Is.EqualTo(4));

			ModuleStack receiver;
			GiveOrder order2 = (GiveOrder)testModuleStack.Orders[1];
			receiver = ModuleStack.All[order2.Receiver.Name];
            this.consoleOutReport("location", testModuleStack.Location, testFaction);
            this.consoleOutReport("observed", observed, testFaction);
            Console.WriteLine("observed alias: " + observed.Alias);
            this.consoleOutReport("receiver", receiver, testFaction);
            Console.WriteLine("receiver alias: " + receiver.Alias);
            Assert.That(receiver.Name, Is.EqualTo(observed.Name));			
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
		public void AssignUseOrder_unlimited()
		{
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Berlin farms"));

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000008",
                "@use farmng",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["farmng"]));
            Assert.That(useOrder.Repeat, Is.EqualTo(-1));
		}

		[Test]
		public void AssignOrder_wrongFaction()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Berlin farms"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000008",
                "@use farmng",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			Assert.Throws<Exception>(
				delegate
				{
					try
					{
						ordersReader.AssignOrders(testcommands);
					}
					catch (Exception ex)
					{
						// exception should appear                
						throw ex;
					}
				});

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
		}

		[Test]
		public void ExecuteUseOrder_unlimited()
		{
			this.AssignUseOrder_unlimited();
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Berlin farms"));
			UseOrder order = (UseOrder)testModuleStack.Orders[0];
			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology farming = Technology.All["farmng"];
            Assert.That(farming.UseTime, Is.EqualTo(1));

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40));
            Assert.That(testModuleStack.Quantity, Is.EqualTo(3));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(-1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - farming is 1 duration order");
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40 + testModuleStack.Quantity * farming.UseProduceItems[food].Quantity), " MSQ " + testModuleStack.Quantity + " FUPIQ " + farming.UseProduceItems[food].Quantity);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(-2));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);
			testModuleStack.Effects.RemoveExecuted();

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week + 1);
            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - farming is 1 duration order");
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40 + 2 * testModuleStack.Quantity * farming.UseProduceItems[food].Quantity));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(-3));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);
			testModuleStack.Effects.RemoveExecuted();
		}

		[Test]
		public void AssignUseOrder_AliasRepeatable()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "5 use agrplx as \"new1\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["agrplx"]));
            Assert.That(useOrder.Receiver.Name, Is.EqualTo("100"));
            Assert.That(useOrder.Repeat, Is.EqualTo(5));
		}

		[Test]
		public void ExecuteUseOrder_lackOfTechnologyLevel0()
		{
			this.AssignUseOrder_unlimited();
			
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
			UseOrder order = (UseOrder)testModuleStack.Orders[0];

			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology farming = Technology.All["farmng"];

            Assert.That(testModuleStack.Technologies.Contains(farming));
			testModuleStack.Technologies.Clear();
            Assert.That(testModuleStack.Technologies.Contains(farming), Is.False);

            // check if the world state changes correctly 

            // SHOULD be producing, ZERO level technologies, DON'T need to be loaded
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40));
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40 + testModuleStack.Quantity * farming.UseProduceItems[food].Quantity), " MSQ " + testModuleStack.Quantity + " FUPIQ " + farming.UseProduceItems[food].Quantity);
		}

		[Test]
		public void ExecuteUseOrder_lackOfTechnologyLevel1()
		{
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000008",
                "@use afrmng",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology advancedFarming = Technology.All["afrmng"];

            Assert.That(testModuleStack.Technologies.Contains(advancedFarming), Is.False);

            // check if the world state changes correctly 

            // SHOULDN'T be producing, advanced technologies NEED to be loaded
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40));
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[food].Quantity, Is.EqualTo(40), " MSQ " + testModuleStack.Quantity + " FUPIQ " + advancedFarming.UseProduceItems[food].Quantity);
		}

		[Test]
		public void ExecuteUseOrder_usedInWrongModuleType()
		{
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000008",
                "@use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ItemType food = ItemType.All["food"];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];
			Technology agriculturalComplex = Technology.All["agrplx"];

            Assert.That(testModuleStack.Technologies.Contains(agriculturalComplex), Is.False);
            Assert.That(agriculturalComplex.UseTime, Is.EqualTo(4));
            Assert.That(testModuleStack.ItemStacks.ContainsKey(iron), Is.False);

			testModuleStack.ItemStacks.Add(new ItemStack(iron, 50));

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(50));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(4));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(useOrder.Executing, Is.False);
            Assert.That(useOrder.Executed, Is.False);

			testModuleStack.Execute(this.game.Week);
            // SHOULDN'T be producing, production technologies can be used in production modules only

            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[iron].Quantity, Is.EqualTo(50));
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(4));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(useOrder.Executing, Is.False);
            Assert.That(useOrder.Executed, Is.False);
		}

		[Test]
		public void ExecuteUseOrder_AliasRepeatable()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

            int stackBefore1 = testModuleStack.Parent.ModuleStacks.Count;
            int stackBefore2 = this.game.ModuleStacks.Count;

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "2 use agrplx as \"new1\"",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

            // modulestack created during assign
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(stackBefore1 + 0));
            Assert.That(this.game.ModuleStacks.Count, Is.EqualTo(stackBefore2 + 1));

            UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);

            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["agrplx"]));
            Assert.That(useOrder.Receiver.Name, Is.EqualTo("100"));
            //ClassicAssert.AreEqual(null, useOrder.Receiver);
            Assert.That(useOrder.Repeat, Is.EqualTo(2));
            Assert.That(Technology.All["agrplx"].UseTime, Is.EqualTo(4));

			// check if the world state changes correctly				

			int week = this.game.Week;
			for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(week++);
			}

            // produced the second module into new stack, that was created during assign
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(stackBefore1 + 1));

			ModuleStack producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(producedModuleStack, Is.Not.Null);
            Assert.That(producedModuleStack.Quantity, Is.EqualTo(1));
            Assert.That(useOrder.Receiver, Is.EqualTo(producedModuleStack));

			for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(week++);
			}

            // produced the second module into the same stack
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(stackBefore1 + 1));
            Assert.That(producedModuleStack.Quantity, Is.EqualTo(2));
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
		public void ExecuteUseOrder_TwoInstancesOfAlias()
		{
			Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack1 = this.game.ModuleStacks["000002"];
			ModuleStack testModuleStack2 = this.game.ModuleStacks["000004"];
            Assert.That(this.game.ModuleStacks.Count, Is.EqualTo(25));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000002",
                "give 6 terran to new1",
                "#modulestack 000004",
                "use agrplx as new1",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(this.game.ModuleStacks.Count, Is.EqualTo(25 + 1));


            Assert.That(testModuleStack1.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack2.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack1.Orders[0] is GiveOrder);
            Assert.That(testModuleStack2.Orders[0] is UseOrder);
			GiveOrder giveOrder = (GiveOrder)testModuleStack1.Orders[0];
			UseOrder useOrder = (UseOrder)testModuleStack2.Orders[0];

            Assert.That(giveOrder.Receiver.ReportName, Is.EqualTo("empty stack [100]"));
            Assert.That(useOrder.Receiver.ReportName, Is.EqualTo("empty stack [100]"));

            Assert.That(useOrder.Receiver.ReportName, Is.EqualTo(giveOrder.Receiver.ReportName));

			// check if the world state changes correctly				
			ModuleStack producedModuleStack;
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(producedModuleStack, Is.Not.Null);

			testModuleStack1.Execute(this.game.Week);
            Assert.That(giveOrder.Executed, Is.False);

			ItemType terran = ItemType.All["terran"];

            Assert.That(testModuleStack1.ItemStacks.Quantity(terran), Is.EqualTo(6));
            Assert.That(producedModuleStack.ItemStacks.Quantity(terran), Is.EqualTo(0));

			testModuleStack2.Execute(this.game.Week);
			ProducingModule effect = (ProducingModule)testModuleStack2.Effects[0];
			testModuleStack1.Execute(this.game.Week);
            Assert.That(giveOrder.Executed, Is.False);
            Assert.That(testModuleStack1.ItemStacks.Quantity(terran), Is.EqualTo(6));
            Assert.That(producedModuleStack.ItemStacks.Quantity(terran), Is.EqualTo(0));

			testModuleStack2.ExecutedLongOrder = false;
			testModuleStack2.Execute(this.game.Week + 1);
			testModuleStack2.ExecutedLongOrder = false;
			testModuleStack2.Execute(this.game.Week + 2);
			testModuleStack2.ExecutedLongOrder = false;
			testModuleStack2.Execute(this.game.Week + 3);
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(producedModuleStack, Is.Not.Null);

            Assert.That(producedModuleStack, Is.SameAs(effect.Produced));

			testModuleStack1.Execute(this.game.Week);
            Assert.That(giveOrder.Executed);
            Assert.That(testModuleStack1.ItemStacks.Quantity(terran), Is.EqualTo(0));
            Assert.That(producedModuleStack.ItemStacks.Quantity(terran), Is.EqualTo(6));
		}

        [Test]
        public void ExecuteUseAndGetOrder_unlimited()
        {
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["1"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["000008"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["000005"];

            List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000008",
                "@use farmng",
                "#modulestack 000005",
                "@get all food from 000008",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack1.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack2.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack1.Orders[0] is UseOrder);
            UseOrder useOrder = (UseOrder)testModuleStack1.Orders[0];
            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["farmng"]));
            Assert.That(useOrder.Repeat, Is.EqualTo(-1));

            Assert.That(testModuleStack2.Orders[0] is GetOrder);
            GetOrder getOrder = (GetOrder)testModuleStack2.Orders[0];
            Assert.That(getOrder.Repeat, Is.EqualTo(-1));

            Assert.That(testModuleStack1.ItemStacks.Quantity("food"), Is.EqualTo(40));
            Assert.That(testModuleStack2.ItemStacks.Quantity("food"), Is.EqualTo(0));

            for (int week = 1; week <= 13; week++)
			{
				this.game.ClearExecutedLongOrder();
                //this.game.ClearFailedToExecuteImmediateOrders();
                this.game.ClearExecutedImmediateOrders();
                bool executedOrderByModuleStack = true;
                while (executedOrderByModuleStack)
                {
                    executedOrderByModuleStack = false;
                    if (testModuleStack1.Execute(week))
                    {
                        executedOrderByModuleStack = true;
                    }
                    if (testModuleStack2.Execute(week))
                    {
                        executedOrderByModuleStack = true;
                    }
                }
                if (week == 1)
                {
                    Assert.That(testModuleStack1.ItemStacks.Quantity("food"), Is.EqualTo(0));
                    Assert.That(testModuleStack2.ItemStacks.Quantity("food"), Is.EqualTo(55));
                }
                else if (week == 2)
                {
                    Assert.That(testModuleStack1.ItemStacks.Quantity("food"), Is.EqualTo(0));
                    Assert.That(testModuleStack2.ItemStacks.Quantity("food"), Is.EqualTo(70));
                }
			}

            Assert.That(testModuleStack1.ItemStacks.Quantity("food"), Is.EqualTo(0));
            Assert.That(testModuleStack2.ItemStacks.Quantity("food"), Is.EqualTo(40 + 13 * 15));
        }

        [Test]
        public void ExecuteUseOrder_RepeatableEffects()
        {
            Sequence.Ints.Push(100);


            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "2 use agrplx as new1",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["000004"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["100"];

            Assert.That(testModuleStack1.Orders[0] is UseOrder);
            UseOrder useOrder = (UseOrder)testModuleStack1.Orders[0];

            // check if the world state changes correctly				

            for (int i = 1; i <= 8; i++)
            {
                testModuleStack1.ExecutedLongOrder = false;
                testModuleStack1.Execute(i);
            }

            Assert.That(testModuleStack2.Quantity, Is.EqualTo(2));
            Assert.That(testModuleStack2.EventReports.Count, Is.EqualTo(3));
            Assert.That(testModuleStack2.EventReports[0].Description, Is.EqualTo("formed by factory [000004] with farming complex [farms]."));
            Assert.That(testModuleStack2.EventReports[1].Description, Is.EqualTo("received farming complex [farms] produced by factory [000004]."));
            Assert.That(testModuleStack2.EventReports[2].Description, Is.EqualTo("received farming complex [farms] produced by factory [000004]."));

            for (int i = 1; i <= testModuleStack1.EventReports.Count; i++)
            {
                Console.WriteLine(testModuleStack1.EventReports[i-1].Report(testFaction)[0]);
            }

            Assert.That(testModuleStack1.EventReports.Count, Is.EqualTo(4));
            Assert.That(testModuleStack1.EventReports[0].Report(testFaction)[0], Is.EqualTo("week 1: consumed 10 units of iron [iron] to produce farming complex [farms] module."));
            Assert.That(testModuleStack1.EventReports[1].Report(testFaction)[0], Is.EqualTo("week 4: produced farming complex [farms] into farming complex [100]."));
            Assert.That(testModuleStack1.EventReports[2].Report(testFaction)[0], Is.EqualTo("week 5: consumed 10 units of iron [iron] to produce farming complex [farms] module."));
            Assert.That(testModuleStack1.EventReports[3].Report(testFaction)[0], Is.EqualTo("week 8: produced farming complex [farms] into farming complex [100]."));
        }

        [Test]
        public void ExecuteUseOrder_RepeatableEffects_UnformedRemoval()
        {
            // duirng the turn end reporting the unformed modulestacks are removed
            // the stack underproduction is already defined and can be given orders so it should not be removed

            Sequence.Ints.Push(100);

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "2 use agrplx as new1",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["000004"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["100"];

            Assert.That(testModuleStack1.Orders[0] is UseOrder);
            UseOrder useOrder = (UseOrder)testModuleStack1.Orders[0];

            // check if the world state changes correctly				
            // execute only one turn to initiate producing modulestack effect
            for (int i = 1; i <= 1; i++)
            {
                testModuleStack1.ExecutedLongOrder = false;
                testModuleStack1.Execute(i);
                testModuleStack2.Execute(i);
            }

            Assert.That(ModuleStack.All.ContainsKey(testModuleStack2.Name), Is.True, "It should be there as it was created during order parsing and executing.");

            ModuleStack.All.RemoveNonReporting();

            Assert.That(ModuleStack.All.ContainsKey(testModuleStack2.Name), Is.True, "It should still be there");
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
        public void ExecuteMoveOrder_space()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100011"]; // frigate
            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "move O00004",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);

            Orbit orbit1 = Orbit.All["O00003"];
            Orbit orbit2 = Orbit.All["O00004"];
            Moon moon = (Moon)orbit2.OrbitHolder;
            moon.AU = 0.04;
            MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
            Assert.That(orbit1.OrbitHolder.DistanceTo(orbit2), Is.EqualTo(0.04));

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);
            Assert.That(testModuleStack.MovingTo, Is.Null);
            Assert.That(testModuleStack.Parent, Is.EqualTo(orbit1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

            testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsMoving);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(orbit2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(orbit1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 1);
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);
            Assert.That(testModuleStack.MovingTo, Is.Null);
            Assert.That(testModuleStack.Parent, Is.EqualTo(orbit2));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);
        }

        [Test]
        public void ExecuteConditionedOrders_plus_minus_move()
        {
            //move is possible only when having items, use is possible onlu when moved, move back possible only when use completed
            //move O00004; move to the moon as soon as you have resources
            //+has 10 iron
            //+has 10 terran
            //-has 5 silici; should check after arriving to destination
            //--move O00003; move back 
            //--+has 5 tita; should check before starting producing, but after checking after arrival
            //---move O00004; move back 

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100011"]; // frigate

            // adding necessary modulestacks and items            
            ModuleStack factories = new ModuleStack(testModuleStack, testFaction, ModuleType.All["factry"], "100");
            factories.AddModule();
            factories.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 10));
            this.consoleOutReport("factory", factories, testFaction);

            // pushing the moon a little farther so that the move will last two weeks
            Orbit orbit2 = Orbit.All["O00004"];
            Moon moon = (Moon)orbit2.OrbitHolder;
            moon.AU = 0.04;

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100011",
                "move O00004",
                "+has 10 iron",
                "+has 10 terran",
                "-has 5 silici",
                "--move O00003",
                "--+has 5 titani",
                "---move O00004",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // check if the world state changes corretly				
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(7));
            Assert.That(testModuleStack.Orders[0].ConditionalOrders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.Orders[0].ConditionedOrders.Count, Is.EqualTo(4));
            Assert.That(testModuleStack.Orders[1].ConditionedOrders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[2].ConditionedOrders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[3].ConditionalOrders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[4].ConditionalOrders.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.Orders[4].ConditionedOrders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[5].ConditionalOrders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.Orders[5].ConditionedOrders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[6].ConditionalOrders.Count, Is.EqualTo(3));

            Assert.That(testModuleStack.Effects.IsMoving, Is.False);

            // stage one - has only one resource, should complete has 10 terrans
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week);
            //this.consoleOutReport("orbit after week 1", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 1", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(6));
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);


            // stage two - added one more resource, should start moving
            factories.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 10));
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 1);
            //this.consoleOutReport("orbit after week 2", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 2", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(5));
            Assert.That(testModuleStack.Effects.IsMoving);
            
            // stage three - moving, should complete movement, should find silici
            factories.ItemStacks.Add(new ItemStack(ItemType.All["silici"], 5));
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 2);
            //this.consoleOutReport("orbit after week 3", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 3", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(3));
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);

            // stage four - added resource, should find tita, should start moving back
            factories.ItemStacks.Add(new ItemStack(ItemType.All["titani"], 5));
            testModuleStack.ExecutedLongOrder = false; 
            testModuleStack.Execute(this.game.Week + 3);
            //this.consoleOutReport("orbit after week 4", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 4", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));
            Assert.That(testModuleStack.Effects.IsMoving);

            // stage five - should complete movement           
            testModuleStack.ExecutedLongOrder = false; 
            testModuleStack.Execute(this.game.Week + 4);
            //this.consoleOutReport("orbit after week 5", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 5", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);
 
            // stage six - should start moving again
            testModuleStack.ExecutedLongOrder = false; 
            testModuleStack.Execute(this.game.Week + 5);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            //this.consoleOutReport("orbit after week 6", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 6", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Effects.IsMoving);

            // stage seven - should complete movement
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 6);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            //this.consoleOutReport("orbit after week 7", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 7", testModuleStack.Orders, testFaction);
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);
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
        public void ExecuteUseOrder_shuttles()
        {
            //shuttles have limited efficiency (produce ten times longer), require fuel to use technologies and can produce only in orbits
            Faction faction = this.game.Factions["2"];
            Region region = Region.All["R00001"];
            Orbit orbit = Orbit.All["O00003"];


            // adding necessary modulestacks and items            
            ModuleStack shuttles = new ModuleStack(region, faction, ModuleType.All["shuttl"], "100");
            shuttles.AddModules(5);
            Assert.That(shuttles.Quantity, Is.EqualTo(5));

            // resources to operate the shuttles (terrans, terair, food) but no fuel            
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 10));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 20));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["food"], 20));
            // resources to build fission reactor
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 2));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["titani"], 8));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["copper"], 5));
            this.consoleOutReport("shuttles", shuttles, faction);

            // this would execute in 8 weeks using single factory, or in two weeks using 5 factories
            // this shouldn't execute in region, without fuel
            // with the above provided it should take 8 * 10 -> 80 / 5 -> 16 weeks
            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100",
                "use urfiss",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));

            shuttles.Orders.Execute(this.game.Week);
            
            // should not execute - in region
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));
            Assert.That(shuttles.Effects.IsProducing, Is.False);

            // let's move it into the orbit and try again
            shuttles.Parent = orbit;

            shuttles.Orders[0].Execute(this.game.Week + 1);

            // should not execute - no fuel
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));
            Assert.That(shuttles.Effects.IsProducing, Is.False);
            Assert.That(shuttles.Effects.IsFuelled, Is.False);

            // let's give it some fuel
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["uraniu"], 5));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["h2o2"], 5));

            shuttles.Orders[0].Execute(this.game.Week + 2);

            // should execute this time
            this.consoleOutReport("shuttles: ", shuttles, faction);

            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));
            Assert.That(shuttles.Effects.IsProducing);
            Assert.That(shuttles.Effects.IsFuelled);

            Assert.That(shuttles.Effects.Producing.Duration, Is.EqualTo(15));
            Assert.That(shuttles.Effects.Fuelled.Duration, Is.EqualTo(13));
            this.consoleOutReport("shuttles", shuttles, faction);


            // should use fuel over the time
            shuttles.ExecutedLongOrder = false;
            shuttles.Orders[0].Execute(this.game.Week + 3);
            shuttles.Orders.RemoveExecuted();
            shuttles.Effects.Execute(this.game.Week + 3);
            shuttles.Effects.RemoveExecuted();

            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));
            Assert.That(shuttles.Effects.IsProducing);
            Assert.That(shuttles.Effects.IsFuelled);

            Assert.That(shuttles.Effects.Producing.Duration, Is.EqualTo(14));
            Assert.That(shuttles.Effects.Fuelled.Duration, Is.EqualTo(12));
            this.consoleOutReport("shuttles", shuttles, faction);            
        }

        [Test]
        public void ExecuteUseOrder_lackOfResources()
        {
            //shuttles (like any other producing stack, require resources to produce things)
            Faction faction = this.game.Factions["2"];
            Orbit orbit = Orbit.All["O00003"];

            // adding necessary modulestacks and items            
            ModuleStack shuttles = new ModuleStack(orbit, faction, ModuleType.All["shuttl"], "100");
            shuttles.AddModules(5);
            Assert.That(shuttles.Quantity, Is.EqualTo(5));

            // resources to operate the shuttles (terrans, terair, food, fuel)
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 10));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 20));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["food"], 20));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["uraniu"], 5));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["h2o2"], 5));
            this.consoleOutReport("shuttles", shuttles, faction);
 
            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100",
                "use urfiss",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));

            shuttles.Orders.Execute(this.game.Week);

            // should not execute - no resource
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));
            Assert.That(shuttles.Effects.IsProducing, Is.False);

            // resources to build fission reactor
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 2));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["titani"], 8));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["copper"], 5));

            shuttles.Orders[0].Execute(this.game.Week + 1);

            // should execute 
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.That(shuttles.Orders.Count, Is.EqualTo(1));
            Assert.That(shuttles.Effects.IsProducing);
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

		[Test]
		public void AssignProduceOrder_unlimited()
		{
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Warsaw wind powerplants"));

			List<string> testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000009",
                "@produce energy",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is ProduceOrder);
			ProduceOrder produceOrder = (ProduceOrder)testModuleStack.Orders[0];
            Assert.That(produceOrder.Repeat, Is.EqualTo(-1));
		}

		[Test]
		public void ExecuteProduceOrder_unlimited()
		{
			this.AssignProduceOrder_unlimited();
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Warsaw wind powerplants"));
			ProduceOrder order = (ProduceOrder)testModuleStack.Orders[0];
			ModuleType windplants = ModuleType.All["wnplnt"];
            Assert.That(windplants.EnergyProduction, Is.EqualTo(4));

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(-1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsProducing, "Should be producing - it's a 13 weeks duration order for wind powerplants");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(12));
            Assert.That(order.Repeat, Is.EqualTo(-1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			for (int i = 0; i < 12;i++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders[0].Execute(this.game.Week + i);
				testModuleStack.Orders.RemoveExecuted();
				testModuleStack.Effects.Execute(this.game.Week + i);
				testModuleStack.Effects.RemoveExecuted();
			}

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - it's a 13 weeks duration order for wind powerplants");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(-2));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);
		}

		[Test]
		public void AssignProduceCash()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Caste Prime Headquarters"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "2 produce cash",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is ProduceOrder);
			ProduceOrder produceOrder = (ProduceOrder)testModuleStack.Orders[0];
            Assert.That(produceOrder.Repeat, Is.EqualTo(2));
		}

		[Test]
		public void ExecuteProduceCash()
		{
			this.AssignProduceCash();
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			ProduceOrder order = (ProduceOrder)testModuleStack.Orders[0];
			ModuleType corphq  = ModuleType.All["corphq"];
			ItemType cash = ItemType.All["cash"];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(2));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False, "Shouldn't be true, 2 to go");

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - it's a 1 weeks duration order for corporate HQ");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False, "Shouldn't be true, 1 to go");

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 1);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 1);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - it's a 1 weeks duration order for corporate HQ");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, "Should be true, 0 repeats");

            Assert.That(testModuleStack.ItemStacks.ContainsKey(cash), "cash should appear in modulestack");
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(200), "cash should appear in modulestack");
		}

        public void AssignProduceCash_unlimited()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Caste Prime Headquarters"));

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "@produce cash",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is ProduceOrder);
            ProduceOrder produceOrder = (ProduceOrder)testModuleStack.Orders[0];
            Assert.That(produceOrder.IsUnlimited, Is.True);
        }

        [Test]
        public void ExecuteProduceCash_unlimited()
        {
            this.AssignProduceCash_unlimited();
            ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            ProduceOrder order = (ProduceOrder)testModuleStack.Orders[0];
            ModuleType corphq = ModuleType.All["corphq"];
            ItemType cash = ItemType.All["cash"];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(order.IsUnlimited, Is.True);
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False, "Shouldn't be true, it is unlimited");

            List<string> testlinesReport = new List<string>
            {
                "@produce cash"
            };

            List<string> generatedReport = null;
            
            generatedReport = order.Report(testModuleStack.Owner);
            for (int i = 0; i < testlinesReport.Count; i++)
            {
                Console.WriteLine(testlinesReport[i]);
                Assert.That(generatedReport[i], Is.EqualTo(testlinesReport[i]));
            }
            Assert.That(generatedReport.Count, Is.EqualTo(testlinesReport.Count));

            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Orders[0].Execute(this.game.Week + 0);
            testModuleStack.Orders.RemoveExecuted();
            testModuleStack.Effects.Execute(this.game.Week + 0);
            testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - it's a 1 weeks duration order for corporate HQ");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.IsUnlimited, Is.True);
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False, "Shouldn't be true, it is still unlimited");

            generatedReport = order.Report(testModuleStack.Owner);
            for (int i = 0; i < testlinesReport.Count; i++)
            {
                Console.WriteLine(testlinesReport[i]);
                Assert.That(generatedReport[i], Is.EqualTo(testlinesReport[i]));
            }
            Assert.That(generatedReport.Count, Is.EqualTo(testlinesReport.Count));

            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Orders[0].Execute(this.game.Week + 1);
            testModuleStack.Orders.RemoveExecuted();
            testModuleStack.Effects.Execute(this.game.Week + 1);
            testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - it's a 1 weeks duration order for corporate HQ");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.IsUnlimited, Is.True);
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False, "Shouldn't be true, it is still unlimited");

            generatedReport = order.Report(testModuleStack.Owner);
            for (int i = 0; i < testlinesReport.Count; i++)
            {
                Console.WriteLine(testlinesReport[i]);
                Assert.That(generatedReport[i], Is.EqualTo(testlinesReport[i]));
            }
            Assert.That(generatedReport.Count, Is.EqualTo(testlinesReport.Count));

            Assert.That(testModuleStack.ItemStacks.ContainsKey(cash), "cash should appear in modulestack");
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(200), "cash should appear in modulestack");
        }

        [Test]
		public void AssignTrain_officer()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Caste Prime Headquarters"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "train terran officer as new3",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testModuleStack.Orders[0];
		}

		[Test]
		public void ExecuteTrain_officer()
		{
			Sequence.Ints.Push(100);

			this.AssignTrain_officer();

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			TrainOrder order = (TrainOrder)testModuleStack.Orders[0];
			ModuleType corphq = ModuleType.All["corphq"];
			ItemType terran = ItemType.All["terran"];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(20));
            Assert.That(testModuleStack.People.Count, Is.EqualTo(1));
            Assert.That(order.Race.OfficerTrainingDuration, Is.EqualTo(6));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsTraining, "Should be training  - it's a 6 weeks duration order for terran officers");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(5));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			for (int i = 0; i < 5; i++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders[0].Execute(this.game.Week + i);
				testModuleStack.Orders.RemoveExecuted();
				testModuleStack.Effects.Execute(this.game.Week + i);
				testModuleStack.Effects.RemoveExecuted();
			}

            Assert.That(testModuleStack.Effects.IsTraining, Is.False, "Shouldn't be training - it's a 6 weeks duration order for terran officers");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);

            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(20 - 1));
            Assert.That(testModuleStack.People.Count, Is.EqualTo(2));

			foreach (Person person in testModuleStack.People.Values)
			{
				Console.WriteLine(person.ReportName);
			}

			Person trainedOfficer = Person.All["2_new3"];
            Assert.That(trainedOfficer, Is.Not.Null);
            Assert.That(trainedOfficer.Name, Is.EqualTo("100"));
            Assert.That(trainedOfficer.Parent, Is.EqualTo(testModuleStack));
		}

		[Test]
		public void AssignTrain_skill()
		{
			Faction testFaction = this.game.Factions["2"];
			Person testPerson = this.game.People["000101"];
            Assert.That(testPerson.FullName, Is.EqualTo("Caste Prime CEO"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#person 000101",
                "train skill arpldr",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testPerson.Orders.Count, Is.EqualTo(1));

            Assert.That(testPerson.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testPerson.Orders[0];
            Assert.That(trainOrder.TrainingOfficer, Is.False);
		}

		[Test]
		public void ExecuteTrain_skill()
		{
			this.AssignTrain_skill();
			Faction testFaction = this.game.Factions["2"];
			Person testPerson = this.game.People["000101"];
			TrainOrder order = (TrainOrder)testPerson.Orders[0];
			SkillType skill = SkillType.All["arpldr"];

            // check if the world state changes correctly	
            Assert.That(testPerson.Skills.ContainsKey(skill), Is.False);
            Assert.That(testPerson.Effects.IsProducing, Is.False);
            Assert.That(order.SkillType.TrainingDuration, Is.EqualTo(4));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testPerson.ExecutedLongOrder = false;
			testPerson.Orders[0].Execute(this.game.Week + 0);
			testPerson.Orders.RemoveExecuted();
			testPerson.Effects.Execute(this.game.Week + 0);
			testPerson.Effects.RemoveExecuted();

            Assert.That(testPerson.Effects.IsTraining, "Should be training - it's a 4 weeks duration order for terran officers");
            Assert.That(testPerson.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(3));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

            Assert.That(testPerson.Skills.ContainsKey(skill), Is.False, "shouldn't have skill yet");

			for (int i = 0; i < 3; i++)
			{
				testPerson.ExecutedLongOrder = false;
				testPerson.Orders[0].Execute(this.game.Week + i);
				testPerson.Orders.RemoveExecuted();
				testPerson.Effects.Execute(this.game.Week + i);
				testPerson.Effects.RemoveExecuted();
			}

            Assert.That(testPerson.Effects.IsTraining, Is.False, "Shouldn't be training - it's a 4 weeks duration order for terran officers");
            Assert.That(testPerson.Orders.Count, Is.EqualTo(0));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);

            Assert.That(testPerson.Skills.ContainsKey(skill), "should have skill now");

            this.consoleOutReport("trainign progress", testPerson, testFaction);
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

		[Test]
		public void TrainOfficer_stacking()
		{
			//+ Gelvaren complex [000026], city [city], disabled, immobile.
			//	events:
			//	+ terran officer [108], terran [terran].
			//		events:
			//			week 1: started training of terran [terran] officer.
			//			week 6: trained by Gelvaren Headquarters [000018].
			//			week 6: trained by Gelvaren Headquarters [000018].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//	+ Gelvaren Headquarters [000018], corporate headquarters [corphq],
			//		disabled.
			//		events:
			//			week 6: trained terran [terran] into terran officer [108].
			//			week 6: trained terran [terran] into terran officer [108].

			//Assert.Fail("when training officer by module stacked under module, officer should stack under training module not his parent");

			this.ExecuteTrain_officer();

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			Person trainedOfficer = Person.All["2_new3"];

            Assert.That(trainedOfficer.Parent, Is.EqualTo(testModuleStack));

			//manual verification if there are no double event report
			this.consoleOutReport("trainer:", testModuleStack, testFaction);
			this.consoleOutReport("trainee:", trainedOfficer, testFaction);
		}

		[Test]
		public void TrainOfficer_byDisabled()
		{
			//	+ Gelvaren Headquarters [000018], corporate headquarters [corphq],
			//		disabled.
			//		events:
			//			week 6: trained terran [terran] into terran officer [108].
			
			//Assert.Fail("training officer should fail, if there is not enough energy");

			Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Caste Prime Headquarters"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "train terran officer as new3",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testModuleStack.Orders[0];
			ItemType terran = ItemType.All["terran"];

			testModuleStack.ItemStacks[terran].Quantity = 10;
            Assert.That(testModuleStack.IsActive, Is.False);

			TrainOrder order = (TrainOrder)testModuleStack.Orders[0];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.People.Count, Is.EqualTo(1));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsTraining, Is.False, "Shouldn't be training - it's a disabled module");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1), "order failed due to lack of energy shouldn't be treated as executed");
            Assert.That(order.Repeat, Is.EqualTo(1), "order failed due to lack of energy shouldn't be treated as executed");
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

            Assert.That(testModuleStack.People.Count, Is.EqualTo(1));

			this.consoleOutReport("trainer: ", testModuleStack, testFaction);
			//this.consoleOutReport("trainee: ", this.game.People["100"], testFaction);
		}

		[Test]
		public void UseOrder_MultipleUseForAlias()
		{
			//ModuleStack: factory [000023]
			//	Long Order: 2 use armcbt as 110 for 000025
			//		started to execute
			// hang after first execute

			Sequence.Ints.Push(100);

            this.game.Week = 1;
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
            Assert.That(testModuleStack.ReportName, Is.EqualTo("factory [000004]"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "2 use armcbt as new1 for 000001",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];

            Assert.That(useOrder.Repeat, Is.EqualTo(2));
            Assert.That(useOrder.Receiver.Name, Is.EqualTo("100"));
            Assert.That(useOrder.ReceiverParent.Name, Is.EqualTo("000001"));

            this.executeOrder(testModuleStack, useOrder, 0);
			this.consoleOutReport("trainer: ", testModuleStack, testFaction);
			this.consoleOutReport("orders: ", testModuleStack.Orders, testFaction);

            Assert.That(testModuleStack.Effects.IsProducing, "Should be producing - order takes 4 weekse");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(useOrder.Repeat, Is.EqualTo(2), "order takes time to execute");
            Assert.That(useOrder.Executing);
            Assert.That(useOrder.Executed, Is.False);

            this.executeOrder(testModuleStack, useOrder, 1);
            this.executeOrder(testModuleStack, useOrder, 2);
            this.executeOrder(testModuleStack, useOrder, 3);

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing - order takes 4 weekse");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1), "the order is still valid only one less repeat");
            Assert.That(useOrder.Repeat, Is.EqualTo(1), "order takes time to execute");
            Assert.That(useOrder.Executing, Is.False);
            Assert.That(useOrder.Executed, Is.False, "executed means ready to be removed - there is one more repeat before doing so");

			ModuleStack trainee = ModuleStack.All["100"];
			ModuleStack traineeParent = ModuleStack.All["000001"];

			this.consoleOutReport("trainer: ", testModuleStack, testFaction);

            // assert weeks are proper
            Assert.That(testModuleStack.Report(testFaction)[5], Is.EqualTo("    week 1: consumed 4 units of iron [iron] to produce tanks [tanks] module."));
            Assert.That(testModuleStack.Report(testFaction)[6], Is.EqualTo("    week 4: produced tanks [tanks] into tanks [100]."));      

			this.consoleOutReport("orders: ", testModuleStack.Orders, testFaction);

            Assert.That(trainee, Is.Not.Null);
            Assert.That(trainee.Name, Is.EqualTo("100"));
            Assert.That(trainee.ModuleStacks.Count, Is.EqualTo(0), "freshly trained stack shouldn't have stacked modulestacks");
			this.consoleOutReport("trainee: ", trainee, testFaction);
            Assert.That(trainee.Report(testFaction).Count, Is.EqualTo(7));
			this.consoleOutReport("trainee parent: ", traineeParent, testFaction);

            for (int i = 1; i <= Technology.All["armcbt"].UseTime; i++)
            {
                this.executeOrder(testModuleStack, useOrder, 3 + i);
            }

            Assert.That(testModuleStack.Effects.IsProducing, Is.False, "Shouldn't be producing");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0), "the order is not valid and removed");
            Assert.That(useOrder.Repeat, Is.EqualTo(0), "order repeated as much is it was planned");
            Assert.That(useOrder.Executing, Is.False);
            Assert.That(useOrder.Executed, "executed means ready to be removed");

            // assert weeks are proper

            this.consoleOutReport("trainer: ", testModuleStack, testFaction);
            Assert.That(testModuleStack.Report(testFaction)[5], Is.EqualTo("    week 1: consumed 4 units of iron [iron] to produce tanks [tanks] module."));
            Assert.That(testModuleStack.Report(testFaction)[6], Is.EqualTo("    week 4: produced tanks [tanks] into tanks [100]."));
            Assert.That(testModuleStack.Report(testFaction)[7], Is.EqualTo("    week 5: consumed 4 units of iron [iron] to produce tanks [tanks] module."));
            Assert.That(testModuleStack.Report(testFaction)[8], Is.EqualTo("    week 8: produced tanks [tanks] into tanks [100]."));      

			this.consoleOutReport("trainee: ", trainee, testFaction);
			this.consoleOutReport("trainee parent: ", traineeParent, testFaction);
		}

        [Test]
        public void AssignSellOrder()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "sell 5 terran at 5",
                "#end"
            };

            Assert.That(Offer.All.Count, Is.EqualTo(9));

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is SellOrder);
            SellOrder sellOrder = (SellOrder)testModuleStack.Orders[0];
            Assert.That(sellOrder.ItemType, Is.EqualTo(ItemType.All["terran"]));
            Assert.That(sellOrder.Repeat, Is.EqualTo(1));

            Assert.That(Offer.All.Count, Is.EqualTo(9), "assign shouldn't change number of offers");
        }

        [Test]
        public void ExecuteUseOrder_AliasFor()
        {
            Sequence.Ints.Push(101);
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

            int stackBefore1 = testModuleStack.Parent.ModuleStacks.Count;
            int stackBefore2 = this.game.ModuleStacks.Count;

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "1 use agrplx as \"new1\" for 000001",
                "1 use agrplx as \"new2\" for 000112",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            UseOrder useOrder2 = (UseOrder)testModuleStack.Orders[1];
            ModuleType farms = ModuleType.All["farms"];

            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(2));

            Assert.That(testModuleStack.Orders[0] is UseOrder);

            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["agrplx"]));
            Assert.That(useOrder.Receiver.Name, Is.EqualTo("100"));
            Assert.That(useOrder.ReceiverParent.Name, Is.EqualTo("000001"));
            Assert.That(useOrder.Repeat, Is.EqualTo(1));
            Assert.That(Technology.All["agrplx"].UseTime, Is.EqualTo(4));

            // check if the world state changes correctly				

            int week = this.game.Week;
            for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
            {
                testModuleStack.ExecutedLongOrder = false;
                testModuleStack.Execute(week++);
            }

            // produced the module into new stack, that was created during assign under parent
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(stackBefore1 + 1));

            ModuleStack producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.That(producedModuleStack, Is.Not.Null);
            Assert.That(producedModuleStack.Quantity, Is.EqualTo(1));
            Assert.That(useOrder.Receiver, Is.EqualTo(producedModuleStack));

            //for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
            //{
            //    testModuleStack.ExecutedLongOrder = false;
            //    testModuleStack.Execute(week++);
            //}

            this.consoleOutReport("new parent", producedModuleStack.Parent, producedModuleStack.Owner);

            // produced the second module into the same stack
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(stackBefore1 + 1));
            Assert.That(producedModuleStack.Quantity, Is.EqualTo(1));

            Assert.That(producedModuleStack.Parent.Name, Is.EqualTo("000001"));

            for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
            {
                testModuleStack.ExecutedLongOrder = false;
                testModuleStack.Execute(week++);
            }
            //for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
            //{
            //    testModuleStack.ExecutedLongOrder = false;
            //    testModuleStack.Execute(week++);
            //}

            this.consoleOutReport("new parent", producedModuleStack.Parent, producedModuleStack.Owner);

            producedModuleStack = this.game.ModuleStacks[testFaction, "new2", true];
            Assert.That(producedModuleStack, Is.Not.Null);
            Assert.That(useOrder2.Receiver, Is.EqualTo(producedModuleStack));

            // no change from above, the new modulestack is under hq
            Assert.That(testModuleStack.Parent.ModuleStacks.Count, Is.EqualTo(stackBefore1 + 1));
            Assert.That(producedModuleStack.Quantity, Is.EqualTo(1));

            Assert.That(producedModuleStack.Parent.Name, Is.EqualTo("000112"));
        }

        [Test] 
        public void FactionPassword()
        {
            Faction testFaction = this.game.Factions["2"];
            testFaction.Password = "xyzzy";
            Assert.That(testFaction.Password, Is.EqualTo("xyzzy"));

            List<string> testcommands = new List<string>
            {
                "#faction 2 \"xyzzy\"",
                "#modulestack 000004",
                "use agrplx",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // should go fine
            ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
        }

        [Test]
        public void FactionPasswordWrong()
        {
            Faction testFaction = this.game.Factions["2"];
            testFaction.Password = "xyzzy";
            Assert.That(testFaction.Password, Is.EqualTo("xyzzy"));

            List<string> testcommands = new List<string>
            {
                "#faction 2 \"wrong password\"",
                "#modulestack 000004",
                "use agrplx",
                "#end"
            };

            Assert.Throws<Exception>(
                delegate
                {
                    OrdersReader ordersReader = new OrdersReader(game);
                    ordersReader.AssignOrders(testcommands);
                }, "Should throw exception due to wrong password: ");

            // shouldn't go fine
            ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
        }


		// upkeep
        // cash in | out437
        // bank operations
        // at
        // describe
        // register
        // email
        // password
        // resign
        // convert
        // erase technology
        // launch (alias for move)
        // land	 (alias for move)
        // enter (alias for stack)
        // eject (alias for stack)
        // leave (alias for stack)
        // shutdown
        // activate
        // * transfer (split & join)
        // synchro
        // receive
        // wait
        // attack
        // hack (view, disrupt, control)
        // show technologies, modules, skills, races, items, all
        // embargo - block possibility to execute market offers
    }
}
				