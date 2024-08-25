using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

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
			this.datafile = new DataFile(Directory.GetCurrentDirectory());
			this.datafile.LoadConfiguration();
			this.datafile.LoadGame();
			this.game = this.datafile.Game;
		}

		[TearDown]
		public void teardownOrder()
		{
			this.game.ClearDictionaries();
			this.game = null;
			this.datafile = null;
		}

		[Test]
		public void SetupTeardown()
		{
			Assert.IsTrue(true);
		}

		[Test]
		public void ReadOrdersFile()
		{
			OrdersReader ordersReader = new OrdersReader(game);
			List<string> testlines = new List<string>();
			testlines.Add("#faction 2");
			testlines.Add("");
			testlines.Add("#modulestack 100001");
			testlines.Add("move R00002");
			testlines.Add("");
			testlines.Add("#end");
			List<string> lines = ordersReader.ReadOrdersFile(Path.Combine(Directory.GetCurrentDirectory(), "orders.move.txt"));
			Assert.AreEqual(testlines.Count, lines.Count);
			for (int i = 0; i < lines.Count ; i++)
			{
				Assert.AreEqual(testlines[i], lines[i]);
			}			
		}

		[Test]
		public void AssignMoveOrder()
		{			
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100001");
			testcommands.Add("move R00002");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			MoveOrder order = (MoveOrder) testModuleStack.Orders[0];
			Assert.AreEqual(1, order.Route.Count);
		}

		[Test]
		public void AssignOrderSpike()
		{
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			MoveOrder mo = new MoveOrder(testModuleStack);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(mo, testModuleStack.Orders[0]);			
		}

		[Test]
		public void FactionTagSpike()
		{
			string command = "#faction 2";
			string token = LineParser.GetToken(ref command);
			Assert.AreEqual("#faction", token);
			Assert.AreEqual("2", command);
		}

		[Test]
		public void ExecuteMoveOrder()
		{
			this.AssignMoveOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];			
			MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];
			Assert.AreEqual(3, region1.Exits[0].ExitModes[EMoveMode.ground].Duration);
 
			// check if the world state changes corretly	
			Assert.IsNull(testModuleStack.MovingTo);
			Assert.AreEqual(region1, testModuleStack.Parent); 
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.AreEqual(region2, testModuleStack.MovingTo);
			Assert.AreEqual(region1, testModuleStack.Parent);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(5, order.DurationLeft);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.AreEqual(region2, testModuleStack.MovingTo);
			Assert.AreEqual(region1, testModuleStack.Parent);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(4, order.DurationLeft);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.AreEqual(region2, testModuleStack.MovingTo);
			Assert.AreEqual(region1, testModuleStack.Parent);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(3, order.DurationLeft);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.AreEqual(region2, testModuleStack.MovingTo);
			Assert.AreEqual(region1, testModuleStack.Parent);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(2, order.DurationLeft);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.AreEqual(region2, testModuleStack.MovingTo);
			Assert.AreEqual(region1, testModuleStack.Parent);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(1, order.DurationLeft);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.IsNull (testModuleStack.MovingTo);
			Assert.AreEqual(region2, testModuleStack.Parent);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsTrue(order.Executed);

			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(0, testModuleStack.Orders.Count);			
		}

		[Test]
		public void AssignUseOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
            
			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			Assert.AreEqual(Technology.All["agrplx"], useOrder.Technology);
            Assert.AreEqual(1, useOrder.Repeat);
		}

		[Test]
		public void AgrplxSpike()
		{
			Technology technology = Technology.All["agrplx"];
			Assert.IsNull(technology.UseProduceItems);
			Assert.IsNotNull(technology.UseProduceModules);
			Assert.AreEqual(4, technology.UseTime);
		}

		[Test]
		public void ExecuteUseOrder()
		{
			this.AssignUseOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			UseOrder order = (UseOrder)testModuleStack.Orders[0];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];

			Assert.AreEqual(4, Technology.All["agrplx"].UseTime);

			// check if the world state changes correctly	
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false;
			Assert.AreEqual(1, order.Repeat);
			testModuleStack.Execute(this.game.Week);
			Assert.IsTrue(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.AreEqual(3, order.DurationLeft, "Producer is having " + testModuleStack.Quantity.ToString() + " modules");
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			this.consoleOutReport("producer: ", testModuleStack, Faction.All["2"]);

			Assert.AreEqual(2, order.DurationLeft);
			testModuleStack.Effects.RemoveExecuted();
			Assert.IsTrue(testModuleStack.Effects.IsProducing);

			testModuleStack.ExecutedLongOrder = false;
			Assert.AreEqual(1, order.Repeat);
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(1, order.DurationLeft);
			Assert.IsTrue(testModuleStack.Effects.IsProducing);

			testModuleStack.ExecutedLongOrder = false;
			Assert.AreEqual(1, order.Repeat);
			testModuleStack.Execute(this.game.Week);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7 + 1, testModuleStack.Parent.ModuleStacks.Count);

			Assert.IsFalse(order.Executing);
			Assert.IsTrue(order.Executed);
			Assert.AreEqual(0, order.Repeat);
			Assert.AreEqual(0, testModuleStack.Orders.Count);
		}

		[Test]
		public void AssignBuyOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("buy 5 terran at 5");
			testcommands.Add("#end");

			Assert.AreEqual(9, Offer.All.Count);

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is BuyOrder);
			BuyOrder buyOrder = (BuyOrder)testModuleStack.Orders[0];
			Assert.AreEqual(ItemType.All["terran"], buyOrder.ItemType);
			Assert.AreEqual(1, buyOrder.Repeat);

			Assert.AreEqual(9, Offer.All.Count, "assign shouldn't change number of offers");
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
			Assert.AreEqual(10, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(300, testModuleStack.ItemStacks[cash].Quantity);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);
			Assert.AreEqual(0, testModuleStack.Effects.Count);
			Console.WriteLine("existing offers");
			List<string> lines = testModuleStack.Location.Market.Report(testFaction);

			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}

			Assert.AreEqual(4, Offer.All[market].Count);

			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(10, testModuleStack.ItemStacks[terran].Quantity, "shouldn'teardownOrder execute - offers are to expensive");
			Assert.AreEqual(300, testModuleStack.ItemStacks[cash].Quantity);
            Assert.AreEqual(20000, testModuleStack.Owner.Bank.AvailableFunds);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing, "it's immediate, can't be continously executing");
			Assert.IsFalse(order.Executed, "offer is placed but not completed - not executed");

			Console.WriteLine("existing offers stage 2");
			lines = testModuleStack.Location.Market.Report(testFaction);
			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}

			Assert.AreEqual(5, Offer.All[market].Count);


			Offers testOffers = Offer.All[testModuleStack][EOfferType.BuyItems];
			Assert.AreEqual(1, testOffers.Count);
			foreach (Offer offer in testOffers)
			{
				Assert.AreEqual(testModuleStack, offer.Offerent);
			}

			order.Buy.Price = 50;
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(15, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(300, testModuleStack.ItemStacks[cash].Quantity);
            Assert.AreEqual(19750, testModuleStack.Owner.Bank.AvailableFunds); // used bank account for transaction
			Assert.AreEqual(0, testModuleStack.Effects.Count, "there was some kind of effect planned");
			Assert.AreEqual(4, Offer.All[market].Count);
		}

		[Test]
		public void ExecuteMoveOrder_fuel()
		{
			this.AssignMoveOrder();
            Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			ItemType oil = ItemType.All["oil"];

			Assert.IsNotNull(oil);
			Assert.AreEqual(2, testModuleStack.ItemStacks.Count);

			// check if the world state changes correctly				
			Assert.AreEqual(2, testModuleStack.ItemStacks[oil].Quantity);
			Assert.AreEqual(0, testModuleStack.Effects.Count);

			testModuleStack.Orders.Execute(this.game.Week);
            this.consoleOutReport("itemstacks: ", testModuleStack.ItemStacks, testFaction);
            this.consoleOutReport("modulestack: ", testModuleStack, testFaction);
            Assert.AreEqual(1, testModuleStack.ItemStacks.Count);
			Assert.AreEqual(2, testModuleStack.Effects.Count); // moving and fuelled
            Assert.IsTrue(testModuleStack.Effects.IsFuelled);
            Assert.IsTrue(testModuleStack.Effects.IsMoving);
            Assert.IsTrue(testModuleStack.Effects[1] is Fuelled);
            Assert.AreEqual(13, testModuleStack.Effects[1].Duration);

			testModuleStack.Effects.Execute(this.game.Week);
			Assert.AreEqual(12, testModuleStack.Effects[1].Duration);
		}

		[Test]
		public void AssignGetOrder()
		{			
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100001");
			// get exact amount from defined modulestack
			testcommands.Add("get 1 iron from 000006");
			// get all except exact amount from defined modulestack
			testcommands.Add("get -5 iron from 000006");
			// get all iron from defined modulestack
			testcommands.Add("get all iron from 000006");
			// get all resources from defined modulestack
			testcommands.Add("get all from 000006");
			// get all resources from all modulestacks
			testcommands.Add("get all iron");
			// get all resources from all modulestacks
			testcommands.Add("get all");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(6, testModuleStack.Orders.Count);

			GetOrder order = (GetOrder) testModuleStack.Orders[0];
			Assert.AreEqual(1, order.Quantity);
			Assert.AreEqual("iron", order.ItemType.Name);
			Assert.AreEqual("core drill [000006]", order.Transferer.ReportName);
		}

		[Test]
		public void ExecuteGetOrder()
		{
			this.AssignGetOrder();
			ModuleStack receiver = this.game.ModuleStacks["100001"];
			ModuleStack holder = this.game.ModuleStacks["000006"];
			ItemType iron = ItemType.All["iron"];
			ItemType terran = ItemType.All["terran"];

			Assert.IsNotNull(iron);
			Assert.AreEqual(2, receiver.ItemStacks.Count);

			// check if the world state changes corretly				
			Assert.IsFalse(receiver.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(10, holder.ItemStacks[iron].Quantity);
			Assert.AreEqual(1, receiver.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, holder.ItemStacks[terran].Quantity);
			// get exact amount from defined modulestack
			// testcommands.Add("get 1 iron from 000006");
            this.consoleOutReport("before get 1 iron holder", holder, holder.Owner);
            this.consoleOutReport("before get 1 iron receiver", receiver, receiver.Owner);
            this.consoleOutReport("location", holder.Location, holder.Owner);
            this.consoleOutReport("order", receiver.Orders[0], receiver.Owner); 
            
            receiver.Orders[0].Execute(this.game.Week);
            this.consoleOutReport("after get 1 iron", holder, holder.Owner);
            Assert.AreEqual(3, receiver.ItemStacks.Count);
			Assert.IsTrue(receiver.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(1, receiver.ItemStacks[iron].Quantity);
			Assert.AreEqual(9, holder.ItemStacks[iron].Quantity);
			Assert.AreEqual(1, receiver.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, holder.ItemStacks[terran].Quantity);
			// get all except exact amount from defined modulestack
			// testcommands.Add("get -5 iron from 000006");
			receiver.Orders[1].Execute(this.game.Week);
			Assert.AreEqual(3, receiver.ItemStacks.Count);
			Assert.AreEqual(5, receiver.ItemStacks[iron].Quantity);
			Assert.AreEqual(5, holder.ItemStacks[iron].Quantity);
			Assert.AreEqual(1, receiver.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, holder.ItemStacks[terran].Quantity);
			// get all iron from defined modulestack
			// testcommands.Add("get all iron from 000006");
			receiver.Orders[2].Execute(this.game.Week);
			Assert.AreEqual(3, receiver.ItemStacks.Count);
			Assert.AreEqual(10, receiver.ItemStacks[iron].Quantity);
			Assert.IsFalse(holder.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(1, receiver.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, holder.ItemStacks[terran].Quantity);
			// get all resources from defined modulestack
			// testcommands.Add("get all from 000006");
			receiver.Orders[3].Execute(this.game.Week);
			Assert.AreEqual(3, receiver.ItemStacks.Count);
			Assert.AreEqual(10, receiver.ItemStacks[iron].Quantity);
			Assert.AreEqual(13, receiver.ItemStacks[terran].Quantity);
			// get all resources from all modulestacks
			// testcommands.Add("get all iron");
			receiver.Orders[4].Execute(this.game.Week);
			Assert.AreEqual(3, receiver.ItemStacks.Count);
			Assert.AreEqual(15, receiver.ItemStacks[iron].Quantity);
			Assert.AreEqual(13, receiver.ItemStacks[terran].Quantity);
			// get all resources from all modulestacks
			// testcommands.Add("get all");
			receiver.Orders[5].Execute(this.game.Week); 
			Assert.AreEqual(6, receiver.ItemStacks.Count);

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

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000006");
			// give exact amount to defined modulestack
			testcommands.Add("give 1 iron to 100001");
			// give all except exact amount to defined modulestack
			testcommands.Add("give -5 iron to 100001");
			// give all iron to  defined modulestack
			testcommands.Add("give all iron to 100001");
			// give all resources to defined modulestack
			testcommands.Add("give all to 100001");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(4, testModuleStack.Orders.Count);

			GiveOrder order = (GiveOrder)testModuleStack.Orders[0];
			Assert.AreEqual(1, order.Quantity);
			Assert.AreEqual("iron", order.ItemType.Name);
			Assert.AreEqual("trucks [100001]", ModuleStack.All[order.ReceiverName].ReportName);
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

			Assert.IsNotNull(iron);
			Assert.AreEqual(2, testModuleStack.ItemStacks.Count);

			// check if the world state changes corretly				
			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(10, testModuleStack2.ItemStacks[iron].Quantity);
			Assert.AreEqual(1, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, testModuleStack2.ItemStacks[terran].Quantity);
			// get exact amount from defined modulestack
			//testcommands.Add("give 1 iron to 100001");			
			testModuleStack2.Orders[0].Execute(this.game.Week);
			Assert.AreEqual(3, testModuleStack.ItemStacks.Count);
			Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(1, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(9, testModuleStack2.ItemStacks[iron].Quantity);
			Assert.AreEqual(1, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, testModuleStack2.ItemStacks[terran].Quantity);
			// get all except exact amount from defined modulestack
			//testcommands.Add("give -5 iron to 100001");
			testModuleStack2.Orders[1].Execute(this.game.Week);
			Assert.AreEqual(3, testModuleStack.ItemStacks.Count);
			Assert.AreEqual(5, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(5, testModuleStack2.ItemStacks[iron].Quantity);
			Assert.AreEqual(1, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, testModuleStack2.ItemStacks[terran].Quantity);
			// get all iron from defined modulestack
			//testcommands.Add("give all iron to 100001");
			testModuleStack2.Orders[2].Execute(this.game.Week);
			Assert.AreEqual(3, testModuleStack.ItemStacks.Count);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.IsFalse(testModuleStack2.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(1, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(12, testModuleStack2.ItemStacks[terran].Quantity);
			// get all resources from defined modulestack
			//testcommands.Add("give all to 100001"); 
			testModuleStack2.Orders[3].Execute(this.game.Week);
			Assert.AreEqual(3, testModuleStack.ItemStacks.Count);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(13, testModuleStack.ItemStacks[terran].Quantity);

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

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("name \"test name\"");
			testcommands.Add("name R00002 \"test region name\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(2, testModuleStack.Orders.Count);

			NameOrder order = (NameOrder)testModuleStack.Orders[0];
			Assert.AreEqual("test name", order.Description);
		}

		[Test]
		public void ExecuteNameOrder()
		{
			//@name "Caste Prime valiant explorer"
			this.AssignNameOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			Region testRegion = this.game.Regions["R00002"];

			Assert.AreEqual(2, testModuleStack.Orders.Count);

			// check if the world state changes corretly				
			Assert.AreEqual("factory [000004]", testModuleStack.ReportName);
			Assert.AreEqual("Eastern Europe [R00002] (1,4)", testRegion.ReportName);
			testModuleStack.Orders.Execute(this.game.Week);
			Assert.AreEqual("test name [000004]", testModuleStack.ReportName);
			Assert.AreEqual("test region name [R00002] (1,4)", testRegion.ReportName);
		}

		[Test]
		public void AssignHasOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100001");
			// check having iron
			testcommands.Add("has 3 iron");
			testcommands.Add("get 1 iron from 000006");
			testcommands.Add("get 2 iron from 000006");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(3, testModuleStack.Orders.Count);

			HasOrder order = (HasOrder)testModuleStack.Orders[0];
			Assert.AreEqual(3, order.Quantity);
		}

		[Test]
		public void ExecuteHasOrder()
		{
			//has 20 iron
			this.AssignHasOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			ItemType iron = ItemType.All["iron"];

			// check if the world state changes corretly				
			Assert.AreEqual(3, testModuleStack.Orders.Count);
			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));
			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(3, testModuleStack.Orders.Count);
			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));
			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(1, testModuleStack.ItemStacks[iron].Quantity);

			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(1, testModuleStack.ItemStacks[iron].Quantity);

			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(3, testModuleStack.ItemStacks[iron].Quantity);

			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(0, testModuleStack.Orders.Count);
		}

		[Test]
		public void AssignRepeatableOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("2 use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			UseOrder order = (UseOrder)testModuleStack.Orders[0];
			Assert.AreEqual(2, order.Repeat);
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
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			Assert.IsTrue(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(3, order.DurationLeft);
			Assert.AreEqual(2, order.Repeat);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed); 
			
			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);			
			}

			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7 + 1, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			Assert.IsTrue(testModuleStack.Effects.IsProducing);
			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(7 + 1, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(3, order.DurationLeft);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);
			}

			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));
			// no change here - both farms are to be produced into single modulestack
			Assert.AreEqual(7 + 1, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(0, testModuleStack.Orders.Count);
			Assert.IsFalse(order.Executing);
			Assert.IsTrue(order.Executed);
		}

		[Test]
		public void AssignMinusConditionedOrders()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("has 30 iron");
			testcommands.Add("-use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(2, testModuleStack.Orders.Count);			
			HasOrder order1 = (HasOrder)testModuleStack.Orders[0];
			UseOrder order2 = (UseOrder)testModuleStack.Orders[1];
			Assert.AreEqual(1, order2.ConditionalOrders.Count);
			Assert.AreSame(order1, order2.ConditionalOrders[0]);
		}

		[Test]
		public void AssignMinusConditionedOrders2()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("get 10 iron from 000002");
			testcommands.Add("has 30 iron");
			testcommands.Add("-use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(3, testModuleStack.Orders.Count);
			GetOrder order1 = (GetOrder)testModuleStack.Orders[0];
			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
			UseOrder order3 = (UseOrder)testModuleStack.Orders[2];
			Assert.AreEqual(1, order3.ConditionalOrders.Count);
			Assert.AreSame(order2, order3.ConditionalOrders[0]);
			Assert.AreEqual(1, order2.ConditionedOrders.Count);
			Assert.AreSame(order3, order2.ConditionedOrders[0]);
		}

		[Test]
		public void AssignMinusConditionedOrders3()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("@get 10 iron from 000002");
			testcommands.Add("has 30 iron");
			testcommands.Add("-use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(3, testModuleStack.Orders.Count);
			GetOrder order1 = (GetOrder)testModuleStack.Orders[0];
			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
			UseOrder order3 = (UseOrder)testModuleStack.Orders[2];
			Assert.AreEqual(1, order3.ConditionalOrders.Count);
			Assert.AreSame(order2, order3.ConditionalOrders[0]);
			Assert.AreEqual(1, order2.ConditionedOrders.Count);
			Assert.AreSame(order3, order2.ConditionedOrders[0]);
		}

		[Test]
		public void ExecuteMinusConditionedOrders()
		{
			//has 30 iron
			//-use agrplx
			// use can be executed only if has been executed
			this.AssignMinusConditionedOrders();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];		
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			HasOrder order1 = (HasOrder)testModuleStack.Orders[0];
			UseOrder order2 = (UseOrder)testModuleStack.Orders[1];

			ItemType iron = ItemType.All["iron"];

			// check if the world state changes corretly				
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[iron].Quantity);
			
			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			Assert.IsFalse(order2.Executing);
			Assert.IsFalse(order2.Executed);
			testModuleStack.ItemStacks[iron].Quantity += 10;
			Assert.AreEqual(30, testModuleStack.ItemStacks[iron].Quantity);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			Assert.IsTrue(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(3, order2.DurationLeft);
			Assert.IsTrue(order1.Executed);
			Assert.IsTrue(order2.Executing);
			Assert.IsFalse(order2.Executed);

			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);
			}

			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(7 + 1, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(0, testModuleStack.Orders.Count);
			Assert.IsFalse(order2.Executing);
			Assert.IsTrue(order2.Executed);
		}

		[Test]
		public void AssignPlusConditionedOrders()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("use agrplx");
			testcommands.Add("+has 30 iron");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			UseOrder order1 = (UseOrder)testModuleStack.Orders[0];
			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];			
			Assert.AreEqual(1, order1.ConditionalOrders.Count);
			Assert.AreSame(order2, order1.ConditionalOrders[0]);
		}

		[Test]
		public void ExecutePlusConditionedOrders()
		{
			//use agrplx
			//+has 30 iron
			// use can be executed only if all plus conditioned are executed before

			this.AssignPlusConditionedOrders();
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			HasOrder order1 = (HasOrder)testModuleStack.Orders[1];
			UseOrder order2 = (UseOrder)testModuleStack.Orders[0];

			ItemType iron = ItemType.All["iron"];

			// check if the world state changes corretly				
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[iron].Quantity);
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			Assert.IsFalse(order2.Executing);
			Assert.IsFalse(order2.Executed);
			testModuleStack.ItemStacks[iron].Quantity += 10;
			Assert.AreEqual(30, testModuleStack.ItemStacks[iron].Quantity);

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			Assert.IsTrue(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(7, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(3, order2.DurationLeft);
			Assert.IsTrue(order1.Executed);
			Assert.IsTrue(order2.Executing);
			Assert.IsFalse(order2.Executed);

			for (int i = 0; i < 3; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(this.game.Week);
			}

			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(7 + 1, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(0, testModuleStack.Orders.Count);
			Assert.IsFalse(order2.Executing);
			Assert.IsTrue(order2.Executed);
		}

		[Test]
		public void AssignAliasOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("alias \"new1\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is AliasOrder);
			AliasOrder aliasOrder = (AliasOrder)testModuleStack.Orders[0];
			Assert.AreEqual("new1", aliasOrder.Alias);
		}

		[Test]
		public void AssignAliasOrder_noquotes()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("alias new1");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is AliasOrder);
			AliasOrder aliasOrder = (AliasOrder)testModuleStack.Orders[0];
			Assert.AreEqual("new1", aliasOrder.Alias);
		}

		[Test]
		public void ExecuteAliasOrder()
		{
			this.AssignAliasOrder();
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			AliasOrder order = (AliasOrder)testModuleStack.Orders[0];

			// check if the world state changes correctly				
			Assert.IsNull(this.game.ModuleStacks["2_new1"]);
			Assert.AreEqual("2_000004", testModuleStack.Alias);
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual("2_new1", testModuleStack.Alias);
			Assert.AreSame(testModuleStack, this.game.ModuleStacks[testFaction, "new1", true]);

		}


		[Test]
		public void AssignUseOrder_Alias()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("use agrplx as \"new1\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			Assert.AreEqual(Technology.All["agrplx"], useOrder.Technology);
			Assert.AreEqual("100", useOrder.Receiver.Name);
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
			//Assert.IsNull(producedModuleStack);
			testModuleStack.Execute(this.game.Week);
			ProducingModule effect = (ProducingModule) testModuleStack.Effects[0];

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week);
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(producedModuleStack);

			Assert.AreSame(effect.Produced, producedModuleStack);
		}

		[Test]
		public void AssignFormOrder()
		{
            Sequence.Ints.Push(100);

			Console.WriteLine("TEST: AssignFormOrder");
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000009");
			testcommands.Add("form new with 2 as \"new1\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is FormOrder);
			FormOrder formOrder = (FormOrder)testModuleStack.Orders[0];
			Assert.AreEqual(2, formOrder.Quantity);
			Assert.AreEqual("new1", formOrder.Alias);
		}

		[Test]
		public void ExecuteFormOrder()
		{
			this.AssignFormOrder();
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];
			Assert.IsNotNull(testModuleStack);
			FormOrder order = (FormOrder)testModuleStack.Orders[0];

			// check if the world state changes correctly				
			ModuleStack formedModuleStack;
			formedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsFalse(formedModuleStack.IsFormed);
			testModuleStack.Execute(this.game.Week);
			formedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsTrue(formedModuleStack.IsFormed);
		}

		[Test]
		public void AssignMoveOrder_PlanetDestination()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100011"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100011");
			testcommands.Add("move P00001 P00002 O00003 S00001");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
			Assert.AreEqual(4, order.Route.Count);
		}

		[Test]
		public void RemoveCommentsAndEmptyLines()
		{
			OrdersReader ordersReader = new OrdersReader(game);
			List<string> testlines = new List<string>();
			testlines.Add("#faction 2");
			testlines.Add("");
			testlines.Add("#modulestack 100001");
			testlines.Add("; some modulestack");
			testlines.Add("move R00002");
			testlines.Add("");
			testlines.Add("#modulestack 100002");
			testlines.Add("; some other modulestack");
			testlines.Add("");
			testlines.Add("#end");
			
			List<string> testlines2 = new List<string>();
			testlines2.Add("#faction 2");
			testlines2.Add("#modulestack 100001");
			testlines2.Add("move R00002");
			testlines2.Add("#modulestack 100002");
			testlines2.Add("#end");

			List<string> lines = ordersReader.RemoveCommentsAndEmptyLines(testlines);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			Assert.AreEqual(testlines2.Count, lines.Count);
			for (int i = 0; i < lines.Count; i++)
			{
				Assert.AreEqual(testlines2[i], lines[i]);
			}
		}

		[Test]
		public void AssignOrder_UnFormed()
		{
			Faction testFaction = this.game.Factions["2"];
			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack new1");
			// check if active - should execute
			testcommands.Add("active 000002");
			testcommands.Add("#end");

			Console.WriteLine("Count: " + ModuleStack.All.Count);
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				Console.WriteLine(stack.ReportName);
			}
			Assert.AreEqual(25, ModuleStack.All.Count);
			
			Assert.IsNull(ModuleStack.All[testFaction, "new1", true]);

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			
			ModuleStack testModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.IsNotNull(testModuleStack, "should find it");
            Console.WriteLine("Name: " + testModuleStack.Name + " Alias: " + testModuleStack.Alias);
			
			Assert.AreNotEqual("new1", testModuleStack.Name);
			Assert.AreEqual("2_new1", testModuleStack.Alias);
			Assert.IsFalse(testModuleStack.IsFormed, testModuleStack.ReportName + " is formed and it should not since this is new test.");

			Assert.AreEqual(1, testModuleStack.Orders.Count);

			ActiveOrder order = (ActiveOrder)testModuleStack.Orders[0];

			ModuleStack observed = ModuleStack.All[testFaction, order.Observed.Name];
			Assert.AreEqual("core drill [000002]", observed.ReportName);
			Assert.IsTrue(observed.IsFormed);
		}


		[Test]
		public void CleanUpVirtualModulestacksAndPeople()
		{
			//Assert.Fail("I don't know why, but this stuck");
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000009"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000009");
			testcommands.Add("form new with 2 as \"new1\"");
			testcommands.Add("active new1");
			testcommands.Add("active new2");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			// check if the world state changes correctly				
			ModuleStack formedModuleStack1, formedModuleStack2;

			formedModuleStack1 = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(formedModuleStack1);
			Assert.IsFalse(formedModuleStack1.IsFormed);
			formedModuleStack2 = this.game.ModuleStacks[testFaction, "new2", true];
			Assert.IsNotNull(formedModuleStack2);
			Assert.IsFalse(formedModuleStack2.IsFormed);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
			formedModuleStack1 = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(formedModuleStack1);
			Assert.IsTrue(formedModuleStack1.IsFormed);
			formedModuleStack2 = this.game.ModuleStacks[testFaction, "new2", true];
			Assert.IsNotNull(formedModuleStack2);
			Assert.IsFalse(formedModuleStack2.IsFormed);

			this.game.ClearUnformed();
			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);
			formedModuleStack1 = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(formedModuleStack1);
			Assert.IsTrue(formedModuleStack1.IsFormed);
			formedModuleStack2 = this.game.ModuleStacks[testFaction, "new2", true];
			Assert.IsNull(formedModuleStack2);
		}

		[Test]
		public void AssignStackOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["100002"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100002");
			testcommands.Add("stack 000004");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			// 2 because the first one is imported with gamein file
			Assert.AreEqual(2, testModuleStack.Orders.Count);

			StackOrder order = (StackOrder)testModuleStack.Orders[1];
			Assert.AreEqual("factory [000004]", ModuleStack.All[order.ParentName].ReportName);
		}

		[Test]
		public void AssignActiveOrder_MultipleConditionedOrders()
		{			
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("active new1");
			testcommands.Add("-give 20 iron to new1");
			testcommands.Add("-give 20 titani to new1");
			testcommands.Add("-give 20 silici to new1");
			testcommands.Add("-give 20 copper to new1");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(5, testModuleStack.Orders.Count);

			ModuleStack observed;
			ActiveOrder order1 = (ActiveOrder) testModuleStack.Orders[0];
			observed = order1.Observed;
			Assert.IsNotNull(observed);
			Assert.AreEqual("2_new1", observed.Alias);
			Assert.AreEqual(4, order1.ConditionedOrders.Count);

			ModuleStack receiver;
			GiveOrder order2 = (GiveOrder)testModuleStack.Orders[1];
			receiver = ModuleStack.All[order2.Receiver.Name];
            this.consoleOutReport("location", testModuleStack.Location, testFaction);
            this.consoleOutReport("observed", observed, testFaction);
            Console.WriteLine("observed alias: " + observed.Alias);
            this.consoleOutReport("receiver", receiver, testFaction);
            Console.WriteLine("receiver alias: " + receiver.Alias);
            Assert.AreEqual(observed.Name, receiver.Name);			
		}

		[Test]
		public void AssignOrder_UnFormed2()
		{
			Faction testFaction = this.game.Factions["2"];
			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("active new1");
			testcommands.Add("#modulestack new1");
			testcommands.Add("active new1");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			ModuleStack testModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.AreNotEqual("new1", testModuleStack.Name);
			Assert.AreEqual("2_new1", testModuleStack.Alias);
			Assert.IsFalse(testModuleStack.IsFormed);

			Assert.AreEqual(1, testModuleStack.Orders.Count);

			ActiveOrder order = (ActiveOrder)testModuleStack.Orders[0];

			ModuleStack observed = ModuleStack.All[testFaction, order.Observed.Name];
			Assert.AreEqual("2_new1", observed.Alias);
			Assert.IsFalse(observed.IsFormed);
		}

		[Test]
		public void ExecuteGiveOrder_toUnformed()
		{
			//@give 20 iron to new1
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000006"];

			ItemType iron = ItemType.All["iron"];
			Assert.IsNotNull(iron);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000006");
			// give exact amount to unformed modulestack
			testcommands.Add("give 1 iron to new1");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			GiveOrder order = (GiveOrder)testModuleStack.Orders[0];
			Assert.AreEqual(1, order.Quantity);
			Assert.AreEqual("iron", order.ItemType.Name);

			Console.WriteLine(order.ReceiverName);

			ModuleStack testModuleStack2 = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsFalse(testModuleStack2.IsFormed);
			Assert.IsFalse(testModuleStack2.ItemStacks.ContainsKey(iron));

			Assert.AreEqual(0, testModuleStack2.Capacity);

			Assert.AreEqual(2, testModuleStack.ItemStacks.Count);

			// give exact amount to undefined modulestack
			testModuleStack.Orders[0].Execute(this.game.Week);
			Assert.AreEqual(2, testModuleStack.ItemStacks.Count);
			Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(iron));
			Assert.AreEqual(10, testModuleStack.ItemStacks[iron].Quantity);
			Assert.IsFalse(testModuleStack2.ItemStacks.ContainsKey(iron));

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
			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack new1");
			// check having iron
			testcommands.Add("move O00003");
			testcommands.Add("+has 3 iron");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			ModuleStack testModuleStack = this.game.ModuleStacks[testFaction, "new1", true];

			Assert.AreEqual(2, testModuleStack.Orders.Count);

			HasOrder order = (HasOrder)testModuleStack.Orders[1];
			Assert.AreEqual(3, order.Quantity);
			ItemType iron = ItemType.All["iron"];

			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));
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

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100012");
			// check having person
			testcommands.Add("has person 200002");
			testcommands.Add("has person 200003");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(2, testModuleStack.Orders.Count);

			HasOrder order1 = (HasOrder)testModuleStack.Orders[0];
			Assert.AreEqual(testPerson1.Name, order1.PersonName);

			HasOrder order2 = (HasOrder)testModuleStack.Orders[1];
			Assert.AreEqual(testPerson2.Name, order2.PersonName);
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
			Assert.AreEqual(2, testModuleStack.Orders.Count);
			Assert.IsTrue(testModuleStack.People.ContainsKey("200002"));
			Assert.IsFalse(testModuleStack.People.ContainsKey("200003"));
			testModuleStack.Orders[0].Execute(this.game.Week);
			testModuleStack.Orders[1].Execute(this.game.Week);
			testModuleStack.Orders.RemoveExecuted();
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsTrue(testModuleStack.People.ContainsKey("200002"));
			Assert.IsFalse(testModuleStack.People.ContainsKey("200003"));
		}

		[Test]
		public void AssignUseOrder_unlimited()
		{
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
			Assert.AreEqual("Berlin farms", testModuleStack.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000008");
			testcommands.Add("@use farmng");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			Assert.AreEqual(Technology.All["farmng"], useOrder.Technology);
			Assert.AreEqual(-1, useOrder.Repeat);
		}

		[Test, ExpectedException(typeof(Exception))]
		public void AssignOrder_wrongFaction()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
			Assert.AreEqual("Berlin farms", testModuleStack.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000008");
			testcommands.Add("@use farmng");
			testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);

            try
            {
                ordersReader.AssignOrders(testcommands);
            }
            catch (Exception)
            {
                // exception should appear                
                throw;
            }
            			
			Assert.AreEqual(0, testModuleStack.Orders.Count);
		}

		[Test]
		public void ExecuteUseOrder_unlimited()
		{
			this.AssignUseOrder_unlimited();
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];
			Assert.AreEqual("Berlin farms", testModuleStack.FullName);
			UseOrder order = (UseOrder)testModuleStack.Orders[0];
			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology farming = Technology.All["farmng"];
			Assert.AreEqual(1, farming.UseTime);

			// check if the world state changes correctly	
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(40, testModuleStack.ItemStacks[food].Quantity);
			Assert.AreEqual(3, testModuleStack.Quantity);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(-1, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Execute(this.game.Week);

			Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing - farming is 1 duration order");
			Assert.AreEqual(40 + testModuleStack.Quantity * farming.UseProduceItems[food].Quantity, testModuleStack.ItemStacks[food].Quantity, " MSQ " + testModuleStack.Quantity + " FUPIQ " + farming.UseProduceItems[food].Quantity);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(-2, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);
			testModuleStack.Effects.RemoveExecuted();

			testModuleStack.ExecutedLongOrder = false; 
			testModuleStack.Execute(this.game.Week + 1);
			Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing - farming is 1 duration order");
			Assert.AreEqual(40 + 2 * testModuleStack.Quantity * farming.UseProduceItems[food].Quantity, testModuleStack.ItemStacks[food].Quantity);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(-3, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);
			testModuleStack.Effects.RemoveExecuted();
		}

		[Test]
		public void AssignUseOrder_AliasRepeatable()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("5 use agrplx as \"new1\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			Assert.AreEqual(Technology.All["agrplx"], useOrder.Technology);
			Assert.AreEqual("100", useOrder.Receiver.Name);
			Assert.AreEqual(5, useOrder.Repeat);
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
			
			Assert.IsTrue(testModuleStack.Technologies.Contains(farming));
			testModuleStack.Technologies.Clear();
            Assert.IsFalse(testModuleStack.Technologies.Contains(farming));

			// check if the world state changes correctly 
			
			// SHOULD be producing, ZERO level technologies, DON'T need to be loaded
			Assert.AreEqual(40, testModuleStack.ItemStacks[food].Quantity);
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(40 + testModuleStack.Quantity * farming.UseProduceItems[food].Quantity, testModuleStack.ItemStacks[food].Quantity, " MSQ " + testModuleStack.Quantity + " FUPIQ " + farming.UseProduceItems[food].Quantity);
		}

		[Test]
		public void ExecuteUseOrder_lackOfTechnologyLevel1()
		{
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000008");
			testcommands.Add("@use afrmng");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology advancedFarming = Technology.All["afrmng"];

            Assert.IsFalse(testModuleStack.Technologies.Contains(advancedFarming));

			// check if the world state changes correctly 

			// SHOULDN'T be producing, advanced technologies NEED to be loaded
			Assert.AreEqual(40, testModuleStack.ItemStacks[food].Quantity);
			testModuleStack.Execute(this.game.Week);
			Assert.AreEqual(40, testModuleStack.ItemStacks[food].Quantity, " MSQ " + testModuleStack.Quantity + " FUPIQ " + advancedFarming.UseProduceItems[food].Quantity);
		}

		[Test]
		public void ExecuteUseOrder_usedInWrongModuleType()
		{
			ModuleStack testModuleStack = this.game.ModuleStacks["000008"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000008");
			testcommands.Add("@use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ItemType food = ItemType.All["food"];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];
			Technology agriculturalComplex = Technology.All["agrplx"];

            Assert.IsFalse(testModuleStack.Technologies.Contains(agriculturalComplex));
			Assert.AreEqual(4, agriculturalComplex.UseTime);
			Assert.IsFalse(testModuleStack.ItemStacks.ContainsKey(iron));

			testModuleStack.ItemStacks.Add(new ItemStack(iron, 50));

			// check if the world state changes correctly	
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(50, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(4, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsFalse(useOrder.Executing);
			Assert.IsFalse(useOrder.Executed);

			testModuleStack.Execute(this.game.Week);
			// SHOULDN'T be producing, production technologies can be used in production modules only

			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(50, testModuleStack.ItemStacks[iron].Quantity);
			Assert.AreEqual(4, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
 			Assert.IsFalse(useOrder.Executing);
			Assert.IsFalse(useOrder.Executed);
		}

		[Test]
		public void ExecuteUseOrder_AliasRepeatable()
		{
            Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

            int stackBefore1 = testModuleStack.Parent.ModuleStacks.Count;
            int stackBefore2 = this.game.ModuleStacks.Count;

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("2 use agrplx as \"new1\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

            // modulestack created during assign
            Assert.AreEqual(stackBefore1 + 0, testModuleStack.Parent.ModuleStacks.Count);
            Assert.AreEqual(stackBefore2 + 1, this.game.ModuleStacks.Count);

            UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];

			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			
			Assert.AreEqual(Technology.All["agrplx"], useOrder.Technology);
			Assert.AreEqual("100", useOrder.Receiver.Name);
			//Assert.AreEqual(null, useOrder.Receiver);
			Assert.AreEqual(2, useOrder.Repeat);
			Assert.AreEqual(4, Technology.All["agrplx"].UseTime);

			// check if the world state changes correctly				

			int week = this.game.Week;
			for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(week++);
			}

			// produced the second module into new stack, that was created during assign
			Assert.AreEqual(stackBefore1 + 1, testModuleStack.Parent.ModuleStacks.Count);

			ModuleStack producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(producedModuleStack);
			Assert.AreEqual(1, producedModuleStack.Quantity);
			Assert.AreEqual(producedModuleStack, useOrder.Receiver);

			for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
			{
				testModuleStack.ExecutedLongOrder = false; 
				testModuleStack.Execute(week++);
			}

			// produced the second module into the same stack
			Assert.AreEqual(stackBefore1 + 1, testModuleStack.Parent.ModuleStacks.Count);
			Assert.AreEqual(2, producedModuleStack.Quantity);
		}

		[Test]
		public void ExecuteLongOrder()
		{
			// shouldn't execute the same week
			// preventing running long orders in units that already eecuted long order in a given week (eg. use)
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("2 use agrplx as \"new1\"");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];

			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreSame(testModuleStack, useOrder.Subject);
			Assert.IsFalse(useOrder.Subject.ExecutedLongOrder);
			Assert.IsFalse(testModuleStack.ExecutedLongOrder);

			// check if the world state changes correctly				

			int week = this.game.Week;
			testModuleStack.Execute(week++);
			Assert.AreSame(testModuleStack, useOrder.Subject);
			Assert.IsTrue(useOrder.Subject.ExecutedLongOrder);
			Assert.IsTrue(testModuleStack.ExecutedLongOrder);
		}

		[Test]
		public void ExecuteUseOrder_TwoInstancesOfAlias()
		{
			Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack1 = this.game.ModuleStacks["000002"];
			ModuleStack testModuleStack2 = this.game.ModuleStacks["000004"];
			Assert.AreEqual(25, this.game.ModuleStacks.Count);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000002");
			testcommands.Add("give 6 terran to new1");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("use agrplx as new1");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(25 + 1, this.game.ModuleStacks.Count);


			Assert.AreEqual(1, testModuleStack1.Orders.Count);
			Assert.AreEqual(1, testModuleStack2.Orders.Count);

			Assert.IsTrue(testModuleStack1.Orders[0] is GiveOrder);
			Assert.IsTrue(testModuleStack2.Orders[0] is UseOrder);
			GiveOrder giveOrder = (GiveOrder)testModuleStack1.Orders[0];
			UseOrder useOrder = (UseOrder)testModuleStack2.Orders[0];

			Assert.AreEqual("empty stack [100]", giveOrder.Receiver.ReportName);
			Assert.AreEqual("empty stack [100]", useOrder.Receiver.ReportName);

			Assert.AreEqual(giveOrder.Receiver.ReportName, useOrder.Receiver.ReportName);

			// check if the world state changes correctly				
			ModuleStack producedModuleStack;
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(producedModuleStack);

			testModuleStack1.Execute(this.game.Week);
			Assert.IsFalse(giveOrder.Executed);

			ItemType terran = ItemType.All["terran"];

			Assert.AreEqual(6, testModuleStack1.ItemStacks.Quantity(terran));
			Assert.AreEqual(0, producedModuleStack.ItemStacks.Quantity(terran));

			testModuleStack2.Execute(this.game.Week);
			ProducingModule effect = (ProducingModule)testModuleStack2.Effects[0];
			testModuleStack1.Execute(this.game.Week);
			Assert.IsFalse(giveOrder.Executed);
			Assert.AreEqual(6, testModuleStack1.ItemStacks.Quantity(terran));
			Assert.AreEqual(0, producedModuleStack.ItemStacks.Quantity(terran));

			testModuleStack2.ExecutedLongOrder = false;
			testModuleStack2.Execute(this.game.Week + 1);
			testModuleStack2.ExecutedLongOrder = false;
			testModuleStack2.Execute(this.game.Week + 2);
			testModuleStack2.ExecutedLongOrder = false;
			testModuleStack2.Execute(this.game.Week + 3);
			producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
			Assert.IsNotNull(producedModuleStack);

			Assert.AreSame(effect.Produced, producedModuleStack);

			testModuleStack1.Execute(this.game.Week);
			Assert.IsTrue(giveOrder.Executed);
			Assert.AreEqual(0, testModuleStack1.ItemStacks.Quantity(terran));
			Assert.AreEqual(6, producedModuleStack.ItemStacks.Quantity(terran));
		}

        [Test]
        public void ExecuteUseAndGetOrder_unlimited()
        {
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["1"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["000008"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["000005"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 1");
            testcommands.Add("#modulestack 000008");
            testcommands.Add("@use farmng");
            testcommands.Add("#modulestack 000005");
            testcommands.Add("@get all food from 000008");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(1, testModuleStack1.Orders.Count);
            Assert.AreEqual(1, testModuleStack2.Orders.Count);

            Assert.IsTrue(testModuleStack1.Orders[0] is UseOrder);
            UseOrder useOrder = (UseOrder)testModuleStack1.Orders[0];
            Assert.AreEqual(Technology.All["farmng"], useOrder.Technology);
            Assert.AreEqual(-1, useOrder.Repeat);

            Assert.IsTrue(testModuleStack2.Orders[0] is GetOrder);
            GetOrder getOrder = (GetOrder)testModuleStack2.Orders[0];
            Assert.AreEqual(-1, getOrder.Repeat); 

            Assert.AreEqual(40, testModuleStack1.ItemStacks.Quantity("food"));
            Assert.AreEqual(0, testModuleStack2.ItemStacks.Quantity("food"));

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
                    Assert.AreEqual(0, testModuleStack1.ItemStacks.Quantity("food"));
                    Assert.AreEqual(55, testModuleStack2.ItemStacks.Quantity("food"));
                }
                else if (week == 2)
                {
                    Assert.AreEqual(0, testModuleStack1.ItemStacks.Quantity("food"));
                    Assert.AreEqual(70, testModuleStack2.ItemStacks.Quantity("food"));
                }
			}

            Assert.AreEqual(0, testModuleStack1.ItemStacks.Quantity("food"));
            Assert.AreEqual(40 + 13 * 15, testModuleStack2.ItemStacks.Quantity("food"));
        }

        [Test]
        public void ExecuteUseOrder_RepeatableEffects()
        {
            Sequence.Ints.Push(100);


            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000004");
            testcommands.Add("2 use agrplx as new1");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack1 = this.game.ModuleStacks["000004"];
            ModuleStack testModuleStack2 = this.game.ModuleStacks["100"];

            Assert.IsTrue(testModuleStack1.Orders[0] is UseOrder);
            UseOrder useOrder = (UseOrder)testModuleStack1.Orders[0];

            // check if the world state changes correctly				

            for (int i = 1; i <= 8; i++)
            {
                testModuleStack1.ExecutedLongOrder = false;
                testModuleStack1.Execute(i);
            }

            Assert.AreEqual(2, testModuleStack2.Quantity);
            Assert.AreEqual(3, testModuleStack2.EventReports.Count);
            Assert.AreEqual("formed by factory [000004] with farming complex [farms].", testModuleStack2.EventReports[0].Description);
            Assert.AreEqual("received farming complex [farms] produced by factory [000004].", testModuleStack2.EventReports[1].Description);
            Assert.AreEqual("received farming complex [farms] produced by factory [000004].", testModuleStack2.EventReports[2].Description);

            for (int i = 1; i <= testModuleStack1.EventReports.Count; i++)
            {
                Console.WriteLine(testModuleStack1.EventReports[i-1].Report(testFaction)[0]);
            }

            Assert.AreEqual(4, testModuleStack1.EventReports.Count);
            Assert.AreEqual("week 1: consumed 10 units of iron [iron] to produce farming complex [farms] module.", testModuleStack1.EventReports[0].Report(testFaction)[0]);
            Assert.AreEqual("week 4: produced farming complex [farms] into farming complex [100].", testModuleStack1.EventReports[1].Report(testFaction)[0]);
            Assert.AreEqual("week 5: consumed 10 units of iron [iron] to produce farming complex [farms] module.", testModuleStack1.EventReports[2].Report(testFaction)[0]);
            Assert.AreEqual("week 8: produced farming complex [farms] into farming complex [100].", testModuleStack1.EventReports[3].Report(testFaction)[0]);
        }

        [Test]
        public void AssignSeeOrder()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100011"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100011");
            // check not seeing officer
            testcommands.Add("see person 200001");
            // check seeing officer
            testcommands.Add("see person 200002");
            // check not seeing modulestack
            testcommands.Add("see 100001");
            // check seeing modulestack
            testcommands.Add("see 100012");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(4, testModuleStack.Orders.Count);

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
            Assert.AreEqual(4, testModuleStack1.Orders.Count);
            Assert.IsFalse(Person.All[testModuleStack1.Location, true].ContainsKey("200001"));
            Assert.IsFalse(Person.All[testModuleStack1.Location, false].ContainsKey("200002"));
            Assert.IsTrue(Person.All[testModuleStack1.Location, true].ContainsKey("200002"));

            Assert.IsFalse(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100001"));
            Assert.IsFalse(ModuleStack.All[testModuleStack1.Location, false].ContainsKey("100012"));
            Assert.IsTrue(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100012"));

            testModuleStack1.Orders[0].Execute(this.game.Week);
            testModuleStack1.Orders[1].Execute(this.game.Week);
            testModuleStack1.Orders[2].Execute(this.game.Week);
            testModuleStack1.Orders[3].Execute(this.game.Week);
            testModuleStack1.Orders.RemoveExecuted();
            Assert.AreEqual(2, testModuleStack1.Orders.Count);
        }

        [Test]
        public void AssignSeeOrder_person()
        {
            Faction testFaction = this.game.Factions["2"];
            Person testPerson = this.game.People["200002"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#person 200002");
            // check not seeing officer
            testcommands.Add("see person 200001");
            // check seeing officer
            testcommands.Add("see person 200002");
            // check not seeing modulestack
            testcommands.Add("see 100001");
            // check seeing modulestack
            testcommands.Add("see 100012");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(4, testPerson.Orders.Count);

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
            Assert.AreEqual(4, testPerson2.Orders.Count);
            Assert.IsFalse(Person.All[testModuleStack1.Location, true].ContainsKey("200001"));
            Assert.IsFalse(Person.All[testModuleStack1.Location, false].ContainsKey("200002"));
            Assert.IsTrue(Person.All[testModuleStack1.Location, true].ContainsKey("200002"));

            Assert.IsFalse(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100001"));
            Assert.IsFalse(ModuleStack.All[testModuleStack1.Location, false].ContainsKey("100012"));
            Assert.IsTrue(ModuleStack.All[testModuleStack1.Location, true].ContainsKey("100012"));

            testPerson2.Orders[0].Execute(this.game.Week);
            testPerson2.Orders[1].Execute(this.game.Week);
            testPerson2.Orders[2].Execute(this.game.Week);
            testPerson2.Orders[3].Execute(this.game.Week);
            testPerson2.Orders.RemoveExecuted();
            Assert.AreEqual(2, testPerson2.Orders.Count);
        }

        [Test]
        public void AssignSeeOrder_alias()
        {
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100001");
            // check not seeing alias
            testcommands.Add("see new1");
            testcommands.Add("form new with 1 as new1");
            // check seeing alias
            testcommands.Add("-see new1");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(3, testModuleStack.Orders.Count);

            SeeOrder order = (SeeOrder)testModuleStack.Orders[0];

            List<string> report = testModuleStack.Orders.Report(testFaction);
            Console.WriteLine(string.Concat("#modulestack ", testModuleStack.Name));
            Console.WriteLine(string.Concat("; ", testModuleStack.ReportName));

            for (int i = 0; i < report.Count; i++)
            {
                Console.WriteLine(report[i]);
            }

            Assert.AreEqual("see new100", report[0]);
            Assert.AreEqual("form new with 1 as new100", report[1]);
            Assert.AreEqual("-see new100", report[2]);
        }

        [Test]
        public void ExecuteSeeOrder_alias()
        {
            this.AssignSeeOrder_alias();
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
            ModuleStack testModuleStack_formed = this.game.ModuleStacks["100"];

            // check if the world state changes corretly				
            Assert.AreEqual(3, testModuleStack.Orders.Count);
            Assert.IsTrue(ModuleStack.All[testModuleStack.Location].ContainsKey("100001"));
            Assert.IsFalse(ModuleStack.All[testModuleStack.Location].ContainsKey("100"));
            Assert.IsFalse(testModuleStack_formed.IsFormed);

            // shouldn't execute, since the stack wasn't formed yet
            testModuleStack.Orders[0].Execute(this.game.Week);
            testModuleStack.Orders.RemoveExecuted();
            Assert.AreEqual(3, testModuleStack.Orders.Count);

            // shouldn't execute, since the order is conditioned
            testModuleStack.Orders[2].Execute(this.game.Week);
            testModuleStack.Orders.RemoveExecuted();
            Assert.AreEqual(3, testModuleStack.Orders.Count);

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
            Assert.AreEqual(2, testModuleStack.Orders.Count);
            Assert.IsTrue(ModuleStack.All[testModuleStack.Location].ContainsKey("100001"));
            Assert.IsTrue(testModuleStack_formed.IsFormed);
            Assert.AreEqual(testModuleStack.Location, testModuleStack_formed.Location);
            Assert.IsTrue(ModuleStack.All[testModuleStack.Location].ContainsKey("100"));
            

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
            Assert.AreEqual(0, testModuleStack.Orders.Count);
        }

        [Test]
        public void AssignStackOrder_person_alias()
        {
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            Person testPerson = this.game.People["200002"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100013"];            

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100013");
            testcommands.Add("form new with 1 as new1");
            testcommands.Add("#person 200002");
            testcommands.Add("stack new1");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(1, testPerson.Orders.Count);
            Assert.IsTrue(testPerson.Orders[0] is StackOrder);
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            Assert.IsTrue(testModuleStack.Orders[0] is FormOrder);
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

            Assert.AreNotEqual(testModuleStack_formed, testPerson.Parent);
            testPerson.Orders[0].Execute(this.game.Week);
            Assert.AreEqual(testModuleStack_formed, testPerson.Parent);
        }

        [Test]
        public void ExecuteMoveOrder_space()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100011"]; // frigate
            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100011");
            testcommands.Add("move O00004");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // check if the world state changes corretly				
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            Assert.IsFalse(testModuleStack.Effects.IsMoving);

            Orbit orbit1 = Orbit.All["O00003"];
            Orbit orbit2 = Orbit.All["O00004"];
            Moon moon = (Moon)orbit2.OrbitHolder;
            moon.AU = 0.04;
            MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
            Assert.AreEqual(0.04, orbit1.OrbitHolder.DistanceTo(orbit2));

            // check if the world state changes correctly	
            Assert.IsFalse(testModuleStack.Effects.IsMoving);
            Assert.IsNull(testModuleStack.MovingTo);
            Assert.AreEqual(orbit1, testModuleStack.Parent);
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            Assert.IsFalse(order.Executing);
            Assert.IsFalse(order.Executed);

            testModuleStack.Execute(this.game.Week);
            Assert.IsTrue(testModuleStack.Effects.IsMoving);
            Assert.AreEqual(orbit2, testModuleStack.MovingTo);
            Assert.AreEqual(orbit1, testModuleStack.Parent);
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            Assert.AreEqual(1, order.DurationLeft);
            Assert.IsTrue(order.Executing);
            Assert.IsFalse(order.Executed);

            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 1);
            Assert.IsFalse(testModuleStack.Effects.IsMoving);
            Assert.IsNull(testModuleStack.MovingTo);
            Assert.AreEqual(orbit2, testModuleStack.Parent);
            Assert.AreEqual(0, testModuleStack.Orders.Count);
            Assert.IsFalse(order.Executing);
            Assert.IsTrue(order.Executed);
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

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100011");
            testcommands.Add("move O00004");
            testcommands.Add("+has 10 iron");
            testcommands.Add("+has 10 terran");
            testcommands.Add("-has 5 silici");
            testcommands.Add("--move O00003");            
            testcommands.Add("--+has 5 titani");
            testcommands.Add("---move O00004");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // check if the world state changes corretly				
            Assert.AreEqual(7, testModuleStack.Orders.Count);
            Assert.AreEqual(2, testModuleStack.Orders[0].ConditionalOrders.Count);
            Assert.AreEqual(4, testModuleStack.Orders[0].ConditionedOrders.Count);
            Assert.AreEqual(1, testModuleStack.Orders[1].ConditionedOrders.Count);
            Assert.AreEqual(1, testModuleStack.Orders[2].ConditionedOrders.Count);
            Assert.AreEqual(1, testModuleStack.Orders[3].ConditionalOrders.Count);
            Assert.AreEqual(3, testModuleStack.Orders[4].ConditionalOrders.Count);
            Assert.AreEqual(1, testModuleStack.Orders[4].ConditionedOrders.Count);
            Assert.AreEqual(2, testModuleStack.Orders[5].ConditionalOrders.Count);
            Assert.AreEqual(1, testModuleStack.Orders[5].ConditionedOrders.Count);
            Assert.AreEqual(3, testModuleStack.Orders[6].ConditionalOrders.Count);

            Assert.IsFalse(testModuleStack.Effects.IsMoving);

            // stage one - has only one resource, should complete has 10 terrans
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week);
            //this.consoleOutReport("orbit after week 1", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 1", testModuleStack.Orders, testFaction);
            Assert.AreEqual(6, testModuleStack.Orders.Count);
            Assert.IsFalse(testModuleStack.Effects.IsProducing);
            Assert.IsFalse(testModuleStack.Effects.IsMoving);


            // stage two - added one more resource, should start moving
            factories.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 10));
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 1);
            //this.consoleOutReport("orbit after week 2", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 2", testModuleStack.Orders, testFaction);
            Assert.AreEqual(5, testModuleStack.Orders.Count);
            Assert.IsTrue(testModuleStack.Effects.IsMoving);
            
            // stage three - moving, should complete movement, should find silici
            factories.ItemStacks.Add(new ItemStack(ItemType.All["silici"], 5));
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 2);
            //this.consoleOutReport("orbit after week 3", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 3", testModuleStack.Orders, testFaction);
            Assert.AreEqual(3, testModuleStack.Orders.Count);
            Assert.IsFalse(testModuleStack.Effects.IsMoving);

            // stage four - added resource, should find tita, should start moving back
            factories.ItemStacks.Add(new ItemStack(ItemType.All["titani"], 5));
            testModuleStack.ExecutedLongOrder = false; 
            testModuleStack.Execute(this.game.Week + 3);
            //this.consoleOutReport("orbit after week 4", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 4", testModuleStack.Orders, testFaction);
            Assert.AreEqual(2, testModuleStack.Orders.Count);
            Assert.IsTrue(testModuleStack.Effects.IsMoving);

            // stage five - should complete movement           
            testModuleStack.ExecutedLongOrder = false; 
            testModuleStack.Execute(this.game.Week + 4);
            //this.consoleOutReport("orbit after week 5", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 5", testModuleStack.Orders, testFaction);
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            Assert.IsFalse(testModuleStack.Effects.IsMoving);
 
            // stage six - should start moving again
            testModuleStack.ExecutedLongOrder = false; 
            testModuleStack.Execute(this.game.Week + 5);
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            //this.consoleOutReport("orbit after week 6", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 6", testModuleStack.Orders, testFaction);
            Assert.IsTrue(testModuleStack.Effects.IsMoving);

            // stage seven - should complete movement
            testModuleStack.ExecutedLongOrder = false;
            testModuleStack.Execute(this.game.Week + 6);
            Assert.AreEqual(0, testModuleStack.Orders.Count);
            //this.consoleOutReport("orbit after week 7", testModuleStack.Location, testFaction);
            this.consoleOutReport("orders after week 7", testModuleStack.Orders, testFaction);
            Assert.IsFalse(testModuleStack.Effects.IsMoving);
        }

        [Test]
        public void ExecuteHasOrder_countModules()
        {

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["100001"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100001");
            testcommands.Add("has modules 3");
            testcommands.Add("has modules 2");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            Assert.AreEqual(2, testModuleStack.Orders.Count);
            Assert.AreEqual(2, testModuleStack.Quantity);
            this.consoleOutReport("stack:", testModuleStack, testFaction);
            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);

            // secound counts should execute
            testModuleStack.Execute(this.game.Week);
            Assert.AreEqual(1, testModuleStack.Orders.Count);
            this.consoleOutReport("stack:", testModuleStack, testFaction);
            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);

            testModuleStack.AddModule();
            // first counts should execute
            testModuleStack.Execute(this.game.Week);
            Assert.AreEqual(0, testModuleStack.Orders.Count);
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

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100001");
            testcommands.Add("get 1 iron from 000006");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(1, testModuleStack.Orders.Count);

            testModuleStack.Parent = Region.All["R00002"];

            testModuleStack.Orders.Execute(this.game.Week);
            // should not execute - not the same location
            Assert.AreEqual(1, testModuleStack.Orders.Count);

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
            Assert.AreEqual(5, shuttles.Quantity);

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
            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100");
            testcommands.Add("use urfiss");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(1, shuttles.Orders.Count);

            shuttles.Orders.Execute(this.game.Week);
            
            // should not execute - in region
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.AreEqual(1, shuttles.Orders.Count);
            Assert.IsFalse(shuttles.Effects.IsProducing);

            // let's move it into the orbit and try again
            shuttles.Parent = orbit;

            shuttles.Orders[0].Execute(this.game.Week + 1);

            // should not execute - no fuel
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.AreEqual(1, shuttles.Orders.Count);
            Assert.IsFalse(shuttles.Effects.IsProducing);
            Assert.IsFalse(shuttles.Effects.IsFuelled);

            // let's give it some fuel
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["uraniu"], 5));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["h2o2"], 5));

            shuttles.Orders[0].Execute(this.game.Week + 2);

            // should execute this time
            this.consoleOutReport("shuttles: ", shuttles, faction);

            Assert.AreEqual(1, shuttles.Orders.Count);
            Assert.IsTrue(shuttles.Effects.IsProducing);
            Assert.IsTrue(shuttles.Effects.IsFuelled);
            
            Assert.AreEqual(15, shuttles.Effects.Producing.Duration);
            Assert.AreEqual(13, shuttles.Effects.Fuelled.Duration);
            this.consoleOutReport("shuttles", shuttles, faction);


            // should use fuel over the time
            shuttles.ExecutedLongOrder = false;
            shuttles.Orders[0].Execute(this.game.Week + 3);
            shuttles.Orders.RemoveExecuted();
            shuttles.Effects.Execute(this.game.Week + 3);
            shuttles.Effects.RemoveExecuted();

            Assert.AreEqual(1, shuttles.Orders.Count);
            Assert.IsTrue(shuttles.Effects.IsProducing);
            Assert.IsTrue(shuttles.Effects.IsFuelled);

            Assert.AreEqual(14, shuttles.Effects.Producing.Duration);
            Assert.AreEqual(12, shuttles.Effects.Fuelled.Duration);
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
            Assert.AreEqual(5, shuttles.Quantity);

            // resources to operate the shuttles (terrans, terair, food, fuel)
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 10));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["terair"], 20));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["food"], 20));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["uraniu"], 5));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["h2o2"], 5));
            this.consoleOutReport("shuttles", shuttles, faction);
 
            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 100");
            testcommands.Add("use urfiss");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(1, shuttles.Orders.Count);

            shuttles.Orders.Execute(this.game.Week);

            // should not execute - no resource
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.AreEqual(1, shuttles.Orders.Count);
            Assert.IsFalse(shuttles.Effects.IsProducing);

            // resources to build fission reactor
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 2));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["titani"], 8));
            shuttles.ItemStacks.Add(new ItemStack(ItemType.All["copper"], 5));

            shuttles.Orders[0].Execute(this.game.Week + 1);

            // should execute 
            this.consoleOutReport("shuttles: ", shuttles.Parent, faction);
            Assert.AreEqual(1, shuttles.Orders.Count);
            Assert.IsTrue(shuttles.Effects.IsProducing);
        }

        [Test]
        public void ExecuteLongOrderAfterForm_FormImmediately()
        {
            // should execute the same week
            Sequence.Ints.Push(100);

            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000006"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000006");
            testcommands.Add("form new with 1 as new1");
            testcommands.Add("-give 6 terran to new1");
            testcommands.Add("#modulestack new1");
            testcommands.Add("use hcdril"); 
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(2, testModuleStack.Orders.Count);

            Assert.IsTrue(testModuleStack.Orders[0] is FormOrder);
            FormOrder formOrder = (FormOrder)testModuleStack.Orders[0];

            Assert.AreEqual(2, testModuleStack.Quantity);
            this.game.ClearExecutedLongOrder();
            //this.game.ClearFailedToExecuteImmediateOrders();
            this.game.ClearExecutedImmediateOrders();
            this.game.ExecuteOrders();

            ModuleStack newModuleStack = this.game.ModuleStacks["100"];
            ItemType carbon = ItemType.All["carbon"];
            
            this.consoleOutReport("formed unit: ", newModuleStack, testFaction);

            Assert.AreEqual(1, newModuleStack.Quantity);
            Assert.AreEqual(1, newModuleStack.ItemStacks[carbon].Quantity);
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

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000006");
            testcommands.Add("use hcdril"); 
            testcommands.Add("-form new with 2 as new1");
            testcommands.Add("-give 6 terran to new1");
            testcommands.Add("#modulestack new1");
            testcommands.Add("use hcdril"); 
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(3, testModuleStack.Orders.Count);

            Assert.IsTrue(testModuleStack.Orders[1] is FormOrder);
            FormOrder formOrder = (FormOrder)testModuleStack.Orders[1];

            Assert.AreEqual(2, testModuleStack.Quantity);

            this.game.Execute();

            ModuleStack newModuleStack = this.game.ModuleStacks["100"];
            ItemType carbon = ItemType.All["carbon"];

            this.consoleOutReport("old unit: ", testModuleStack, testFaction);
            this.consoleOutReport("formed unit: ", newModuleStack, testFaction);

            Assert.AreEqual(2, newModuleStack.Quantity);
            Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(carbon));
            Assert.AreEqual(2, testModuleStack.ItemStacks[carbon].Quantity);

            //TODO: the itemstacks should drop to the ground from empty stack
        }

		[Test]
		public void AssignProduceOrder_unlimited()
		{
			Faction testFaction = this.game.Factions["1"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000011"];
			Assert.AreEqual("Berlin wind powerplants", testModuleStack.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000011");
			testcommands.Add("@produce energy");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is ProduceOrder);
			ProduceOrder produceOrder = (ProduceOrder)testModuleStack.Orders[0];
			Assert.AreEqual(-1, produceOrder.Repeat);
		}

		[Test]
		public void ExecuteProduceOrder_unlimited()
		{
			this.AssignProduceOrder_unlimited();
			ModuleStack testModuleStack = this.game.ModuleStacks["000011"];
			Assert.AreEqual("Berlin wind powerplants", testModuleStack.FullName);
			ProduceOrder order = (ProduceOrder)testModuleStack.Orders[0];
			ModuleType windplants = ModuleType.All["wnplnt"];
			Assert.AreEqual(4, windplants.EnergyProduction);

			// check if the world state changes correctly	
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(-1, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

			Assert.IsTrue(testModuleStack.Effects.IsProducing, "Should be producing - it's a 13 weeks duration order for wind powerplants");
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(12, order.DurationLeft);
			Assert.AreEqual(-1, order.Repeat);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			for (int i = 0; i < 12;i++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders[0].Execute(this.game.Week + i);
				testModuleStack.Orders.RemoveExecuted();
				testModuleStack.Effects.Execute(this.game.Week + i);
				testModuleStack.Effects.RemoveExecuted();
			}
			
			Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing - it's a 13 weeks duration order for wind powerplants");
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(-2, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);
		}

		[Test]
		public void AssignProduceCash()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			Assert.AreEqual("Caste Prime Headquarters", testModuleStack.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000112");
			testcommands.Add("2 produce cash");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is ProduceOrder);
			ProduceOrder produceOrder = (ProduceOrder)testModuleStack.Orders[0];
			Assert.AreEqual(2, produceOrder.Repeat);
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
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(2, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed, "Shouldn't be true, 2 to go");

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

			Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing - it's a 1 weeks duration order for corporate HQ");
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed, "Shouldn't be true, 1 to go");

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 1);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 1);
			testModuleStack.Effects.RemoveExecuted();

			Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing - it's a 1 weeks duration order for corporate HQ");
			Assert.AreEqual(0, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(0, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsTrue(order.Executed, "Should be true, 0 repeats");

			Assert.IsTrue(testModuleStack.ItemStacks.ContainsKey(cash), "cash should appear in modulestack");
			Assert.AreEqual(testModuleStack.ItemStacks[cash].Quantity, 200, "cash should appear in modulestack");
		}

		[Test]
		public void AssignTrain_officer()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			Assert.AreEqual("Caste Prime Headquarters", testModuleStack.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000112");
			testcommands.Add("train terran officer as new3");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is TrainOrder);
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
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(20, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(1, testModuleStack.People.Count);
			Assert.AreEqual(6, order.Race.OfficerTrainingDuration);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

			Assert.IsTrue(testModuleStack.Effects.IsTraining, "Should be training  - it's a 6 weeks duration order for terran officers");
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(5, order.DurationLeft);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

			for (int i = 0; i < 5; i++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders[0].Execute(this.game.Week + i);
				testModuleStack.Orders.RemoveExecuted();
				testModuleStack.Effects.Execute(this.game.Week + i);
				testModuleStack.Effects.RemoveExecuted();
			}

			Assert.IsFalse(testModuleStack.Effects.IsTraining, "Shouldn't be training - it's a 6 weeks duration order for terran officers");
			Assert.AreEqual(0, testModuleStack.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(0, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsTrue(order.Executed);

			Assert.AreEqual(20 - 1, testModuleStack.ItemStacks[terran].Quantity);
			Assert.AreEqual(2, testModuleStack.People.Count);

			foreach (Person person in testModuleStack.People.Values)
			{
				Console.WriteLine(person.ReportName);
			}

			Person trainedOfficer = Person.All["2_new3"];
			Assert.IsNotNull(trainedOfficer);
			Assert.AreEqual("100", trainedOfficer.Name);
			Assert.AreEqual(testModuleStack, trainedOfficer.Parent);
		}

		[Test]
		public void AssignTrain_skill()
		{
			Faction testFaction = this.game.Factions["2"];
			Person testPerson = this.game.People["000101"];
			Assert.AreEqual("Caste Prime CEO", testPerson.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#person 000101");
			testcommands.Add("train skill arpldr");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testPerson.Orders.Count);

			Assert.IsTrue(testPerson.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testPerson.Orders[0];
			Assert.IsFalse(trainOrder.TrainingOfficer);
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
			Assert.IsFalse(testPerson.Skills.ContainsKey(skill));
			Assert.IsFalse(testPerson.Effects.IsProducing);
			Assert.AreEqual(4, order.SkillType.TrainingDuration);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testPerson.ExecutedLongOrder = false;
			testPerson.Orders[0].Execute(this.game.Week + 0);
			testPerson.Orders.RemoveExecuted();
			testPerson.Effects.Execute(this.game.Week + 0);
			testPerson.Effects.RemoveExecuted();

			Assert.IsTrue(testPerson.Effects.IsTraining, "Should be training - it's a 4 weeks duration order for terran officers");
			Assert.AreEqual(1, testPerson.Orders.Count);
			Assert.AreEqual(3, order.DurationLeft);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsTrue(order.Executing);
			Assert.IsFalse(order.Executed);

            Assert.IsFalse(testPerson.Skills.ContainsKey(skill), "shouldn't have skill yet");

			for (int i = 0; i < 3; i++)
			{
				testPerson.ExecutedLongOrder = false;
				testPerson.Orders[0].Execute(this.game.Week + i);
				testPerson.Orders.RemoveExecuted();
				testPerson.Effects.Execute(this.game.Week + i);
				testPerson.Effects.RemoveExecuted();
			}

			Assert.IsFalse(testPerson.Effects.IsTraining, "Shouldn't be training - it's a 4 weeks duration order for terran officers");
			Assert.AreEqual(0, testPerson.Orders.Count);
			//Assert.AreEqual(0, order.DurationLeft);
			Assert.AreEqual(0, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsTrue(order.Executed);

			Assert.IsTrue(testPerson.Skills.ContainsKey(skill), "should have skill now");

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
			Assert.AreEqual("Berlin farms", producer.FullName);
			ModuleStack receiver = this.game.ModuleStacks["000005"];
			Assert.AreEqual("Berlin", receiver.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 1");
			testcommands.Add("#modulestack 000005");
			testcommands.Add("@get all food from 000008");
			testcommands.Add("#modulestack 000008");
			testcommands.Add("@use farmng");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, producer.Orders.Count);
			Assert.AreEqual(1, receiver.Orders.Count);

			UseOrder useOrder = (UseOrder)producer.Orders[0];
			GetOrder getOrder = (GetOrder)receiver.Orders[0];
			ItemType food = ItemType.All["food"];
			ModuleType farms = ModuleType.All["farms"];
			Technology farming = Technology.All["farmng"];
			Assert.AreEqual(1, farming.UseTime, "if that change, the below test would be invalid");

			// check if the world state changes correctly	
			Assert.IsFalse(receiver.ItemStacks.ContainsKey(food));
			ModuleStacks stacks = new ModuleStacks();
			stacks.Add(producer);
			stacks.Add(receiver);

			this.game.ExecuteOrdersByModuleStack(stacks);

			Assert.IsFalse(producer.Effects.IsProducing, "Shouldn't be producing - farming is 1 duration order");
			Assert.IsTrue(receiver.ItemStacks.ContainsKey(food));
			Assert.AreEqual(40 + producer.Quantity * farming.UseProduceItems[food].Quantity, receiver.ItemStacks[food].Quantity);
			Assert.IsFalse(producer.ItemStacks.ContainsKey(food));
			Assert.AreEqual(1, producer.Orders.Count);
			Assert.AreEqual(1, receiver.Orders.Count);
			Assert.IsFalse(useOrder.Executing);
			Assert.IsFalse(getOrder.Executing);
			Assert.IsFalse(useOrder.Executed);
			Assert.IsTrue(getOrder.Executed);

			this.game.ClearExecutedLongOrder();
			//this.game.ClearFailedToExecuteImmediateOrders();
			this.game.ClearExecutedImmediateOrders();
			this.game.ClearUnformed();

			this.game.Week++;
			this.game.ExecuteOrdersByModuleStack(stacks);
			Assert.AreEqual(40 + 2 * producer.Quantity * farming.UseProduceItems[food].Quantity, receiver.ItemStacks[food].Quantity);
			Assert.IsFalse(producer.ItemStacks.ContainsKey(food));
			Assert.AreEqual(1, producer.Orders.Count);
			Assert.AreEqual(1, receiver.Orders.Count);
			Assert.IsFalse(useOrder.Executing);
			Assert.IsFalse(getOrder.Executing);
			Assert.IsFalse(useOrder.Executed);
			Assert.IsTrue(getOrder.Executed);

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

			Assert.AreEqual(testModuleStack, trainedOfficer.Parent);

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
			Assert.AreEqual("Caste Prime Headquarters", testModuleStack.FullName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000112");
			testcommands.Add("train terran officer as new3");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);

			Assert.IsTrue(testModuleStack.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testModuleStack.Orders[0];
			ItemType terran = ItemType.All["terran"];

			testModuleStack.ItemStacks[terran].Quantity = 10;
			Assert.IsFalse(testModuleStack.IsActive);

			TrainOrder order = (TrainOrder)testModuleStack.Orders[0];

			// check if the world state changes correctly	
			Assert.IsFalse(testModuleStack.Effects.IsProducing);
			Assert.AreEqual(1, testModuleStack.People.Count);
			Assert.AreEqual(1, order.Repeat);
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

			Assert.IsFalse(testModuleStack.Effects.IsTraining, "Shouldn't be training - it's a disabled module");
			Assert.AreEqual(1, testModuleStack.Orders.Count, "order failed due to lack of energy shouldn't be treated as executed");
			Assert.AreEqual(1, order.Repeat, "order failed due to lack of energy shouldn't be treated as executed");
			Assert.IsFalse(order.Executing);
			Assert.IsFalse(order.Executed);

			Assert.AreEqual(1, testModuleStack.People.Count);

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
			Assert.AreEqual("factory [000004]", testModuleStack.ReportName);

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("2 use armcbt as new1 for 000001");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];

			Assert.AreEqual(2, useOrder.Repeat);
			Assert.AreEqual("100", useOrder.Receiver.Name);
			Assert.AreEqual("000001", useOrder.ReceiverParent.Name);

            this.executeOrder(testModuleStack, useOrder, 0);
			this.consoleOutReport("trainer: ", testModuleStack, testFaction);
			this.consoleOutReport("orders: ", testModuleStack.Orders, testFaction);

			Assert.IsTrue(testModuleStack.Effects.IsProducing, "Should be producing - order takes 4 weekse");
			Assert.AreEqual(1, testModuleStack.Orders.Count);
			Assert.AreEqual(2, useOrder.Repeat, "order takes time to execute");
			Assert.IsTrue(useOrder.Executing);
			Assert.IsFalse(useOrder.Executed);

            this.executeOrder(testModuleStack, useOrder, 1);
            this.executeOrder(testModuleStack, useOrder, 2);
            this.executeOrder(testModuleStack, useOrder, 3);

			Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing - order takes 4 weekse");
			Assert.AreEqual(1, testModuleStack.Orders.Count, "the order is still valid only one less repeat");
			Assert.AreEqual(1, useOrder.Repeat, "order takes time to execute");
			Assert.IsFalse(useOrder.Executing);
			Assert.IsFalse(useOrder.Executed, "executed means ready to be removed - there is one more repeat before doing so");

			ModuleStack trainee = ModuleStack.All["100"];
			ModuleStack traineeParent = ModuleStack.All["000001"];

			this.consoleOutReport("trainer: ", testModuleStack, testFaction);

            // assert weeks are proper
            Assert.AreEqual("    week 1: consumed 4 units of iron [iron] to produce tanks [tanks] module.", testModuleStack.Report(testFaction)[5]);
            Assert.AreEqual("    week 4: produced tanks [tanks] into tanks [100].", testModuleStack.Report(testFaction)[6]);      

			this.consoleOutReport("orders: ", testModuleStack.Orders, testFaction);

			Assert.IsNotNull(trainee);
			Assert.AreEqual("100", trainee.Name);
			Assert.AreEqual(0, trainee.ModuleStacks.Count, "freshly trained stack shouldn't have stacked modulestacks");
			this.consoleOutReport("trainee: ", trainee, testFaction);
            Assert.AreEqual(7, trainee.Report(testFaction).Count);
			this.consoleOutReport("trainee parent: ", traineeParent, testFaction);

            for (int i = 1; i <= Technology.All["armcbt"].UseTime; i++)
            {
                this.executeOrder(testModuleStack, useOrder, 3 + i);
            }

            Assert.IsFalse(testModuleStack.Effects.IsProducing, "Shouldn't be producing");
            Assert.AreEqual(0, testModuleStack.Orders.Count, "the order is not valid and removed");
            Assert.AreEqual(0, useOrder.Repeat, "order repeated as much is it was planned");
            Assert.IsFalse(useOrder.Executing);
            Assert.IsTrue(useOrder.Executed, "executed means ready to be removed");

            // assert weeks are proper

            this.consoleOutReport("trainer: ", testModuleStack, testFaction);
            Assert.AreEqual("    week 1: consumed 4 units of iron [iron] to produce tanks [tanks] module.", testModuleStack.Report(testFaction)[5]);
            Assert.AreEqual("    week 4: produced tanks [tanks] into tanks [100].", testModuleStack.Report(testFaction)[6]);
            Assert.AreEqual("    week 5: consumed 4 units of iron [iron] to produce tanks [tanks] module.", testModuleStack.Report(testFaction)[7]);
            Assert.AreEqual("    week 8: produced tanks [tanks] into tanks [100].", testModuleStack.Report(testFaction)[8]);      

			this.consoleOutReport("trainee: ", trainee, testFaction);
			this.consoleOutReport("trainee parent: ", traineeParent, testFaction);
		}

        [Test]
        public void AssignSellOrder()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000004");
            testcommands.Add("sell 5 terran at 5");
            testcommands.Add("#end");

            Assert.AreEqual(9, Offer.All.Count);

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.AreEqual(1, testModuleStack.Orders.Count);

            Assert.IsTrue(testModuleStack.Orders[0] is SellOrder);
            SellOrder sellOrder = (SellOrder)testModuleStack.Orders[0];
            Assert.AreEqual(ItemType.All["terran"], sellOrder.ItemType);
            Assert.AreEqual(1, sellOrder.Repeat);

            Assert.AreEqual(9, Offer.All.Count, "assign shouldn't change number of offers");
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

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000004");
            testcommands.Add("1 use agrplx as \"new1\" for 000001");
            testcommands.Add("1 use agrplx as \"new2\" for 000112");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            UseOrder useOrder2 = (UseOrder)testModuleStack.Orders[1];
            ModuleType farms = ModuleType.All["farms"];

            Assert.AreEqual(2, testModuleStack.Orders.Count);

            Assert.IsTrue(testModuleStack.Orders[0] is UseOrder);

            Assert.AreEqual(Technology.All["agrplx"], useOrder.Technology);
            Assert.AreEqual("100", useOrder.Receiver.Name);
            Assert.AreEqual("000001", useOrder.ReceiverParent.Name);
            Assert.AreEqual(1, useOrder.Repeat);
            Assert.AreEqual(4, Technology.All["agrplx"].UseTime);

            // check if the world state changes correctly				

            int week = this.game.Week;
            for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
            {
                testModuleStack.ExecutedLongOrder = false;
                testModuleStack.Execute(week++);
            }

            // produced the module into new stack, that was created during assign under parent
            Assert.AreEqual(stackBefore1 + 1, testModuleStack.Parent.ModuleStacks.Count);

            ModuleStack producedModuleStack = this.game.ModuleStacks[testFaction, "new1", true];
            Assert.IsNotNull(producedModuleStack);
            Assert.AreEqual(1, producedModuleStack.Quantity);
            Assert.AreEqual(producedModuleStack, useOrder.Receiver);

            //for (int i = 1; i <= Technology.All["agrplx"].UseTime; i++)
            //{
            //    testModuleStack.ExecutedLongOrder = false;
            //    testModuleStack.Execute(week++);
            //}

            this.consoleOutReport("new parent", producedModuleStack.Parent, producedModuleStack.Owner);

            // produced the second module into the same stack
            Assert.AreEqual(stackBefore1 + 1, testModuleStack.Parent.ModuleStacks.Count);
            Assert.AreEqual(1, producedModuleStack.Quantity);

            Assert.AreEqual("000001", producedModuleStack.Parent.Name);

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
            Assert.IsNotNull(producedModuleStack);
            Assert.AreEqual(producedModuleStack, useOrder2.Receiver);

            // no change from above, the new modulestack is under hq
            Assert.AreEqual(stackBefore1 + 1, testModuleStack.Parent.ModuleStacks.Count);
            Assert.AreEqual(1, producedModuleStack.Quantity);

            Assert.AreEqual("000112", producedModuleStack.Parent.Name);
        }

        // research
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
				