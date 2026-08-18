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
	public class TOrder : TTest
	{
		public TOrder()
		{			
		}
			
		[SetUp]
		public void setupOrder()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownOrder()
		{
			this.ClearGame();
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
		public void AgrplxSpike()
		{
			Technology technology = Technology.All["agrplx"];
            Assert.That(technology.UseProduceItems, Is.Null);
            Assert.That(technology.UseProduceModules, Is.Not.Null);
            Assert.That(technology.UseTime, Is.EqualTo(4));
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
		public void AssignOrders_UnlimitedLeftover_DoesNotDuplicateWhenReissuedFromFile()
		{
			ModuleStack stack = this.game.ModuleStacks["000004"];
			List<string> leftover = new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"@produce cash",
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(leftover);
			Assert.That(stack.Orders.Count, Is.EqualTo(1));

			List<string> copiedTemplate = new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"@produce cash",
				"@produce energy",
				"#end"
			};
			ordersReader.AssignOrders(copiedTemplate);

			Assert.That(stack.Orders.Count, Is.EqualTo(2));
			int produceCash = 0;
			int produceEnergy = 0;
			foreach (Order order in stack.Orders)
			{
				ProduceOrder produce = order as ProduceOrder;
				Assert.That(produce, Is.Not.Null);
				if (produce.ProduceType == EProduceType.Items && produce.ItemType.Name == "cash")
				{
					produceCash++;
				}
				if (produce.ProduceType == EProduceType.Energy)
				{
					produceEnergy++;
				}
			}
			Assert.That(produceCash, Is.EqualTo(1));
			Assert.That(produceEnergy, Is.EqualTo(1));
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
