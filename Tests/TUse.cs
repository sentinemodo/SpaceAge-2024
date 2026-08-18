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
	public class TUse : TTest
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

            // this would execute in 6 weeks using single factory, or in two weeks using 5 factories
            // this shouldn't execute in region, without fuel
            // with the above provided it should take 6 * 10 -> 60 / 5 -> 12 weeks
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

            Assert.That(shuttles.Effects.Producing.Duration, Is.EqualTo(11));
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

            Assert.That(shuttles.Effects.Producing.Duration, Is.EqualTo(10));
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
		public void ExecuteProduceEnergy_AutoGetsFuelFromOwnedStackInSameLocation()
		{
			ModuleStack plant = this.game.ModuleStacks["000003"];
			ModuleStack drill = this.game.ModuleStacks["000002"];
			ItemType carbon = ItemType.All["carbon"];
			int carbonBefore = drill.ItemStacks[carbon].Quantity;
			Assert.That(plant.ItemStacks.Has(carbon), Is.False);

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 000003",
				"produce energy",
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			plant.ExecutedLongOrder = false;
			plant.Orders[0].Execute(this.game.Week);
			plant.Orders.RemoveExecuted();
			plant.Effects.Execute(this.game.Week);
			plant.Effects.RemoveExecuted();

			Assert.That(drill.ItemStacks[carbon].Quantity, Is.EqualTo(carbonBefore - 5));
			Assert.That(plant.ItemStacks.Has(carbon), Is.False);
			Assert.That(plant.Effects.IsFuelled, Is.True);
			Assert.That(plant.Effects.IsProducing, Is.True);
			Assert.That(this.containsEvent(drill.EventReports, "given 5 units of carbon [carbon] to coal-burning plant [000003]."), Is.True);
			Assert.That(this.containsEvent(plant.EventReports, "got 5 units of carbon [carbon] from core drill [000002]."), Is.True);
			Assert.That(this.containsEvent(plant.EventReports, "consumed 5 units of carbon [carbon] as fuel."), Is.True);
			Assert.That(this.containsEvent(plant.EventReports, "out of fuel."), Is.False);
		}


		[Test]
		public void ExecuteProduceEnergy_ReportsOutOfFuel()
		{
			ModuleStack plant = this.game.ModuleStacks["000003"];
			ModuleStack drill = this.game.ModuleStacks["000002"];
			drill.ItemStacks.Minus(ItemType.All["carbon"], drill.ItemStacks[ItemType.All["carbon"]].Quantity);

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 000003",
				"produce energy",
				"#end"
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);

			plant.ExecutedLongOrder = false;
			plant.Orders[0].Execute(this.game.Week);
			plant.Orders.RemoveExecuted();

			Assert.That(plant.Effects.IsProducing, Is.False);
			Assert.That(this.containsEvent(plant.EventReports, "out of fuel."), Is.True);
			Assert.That(this.containsEvent(plant.EventReports, "is out of fuel for coal-burning plant [000003]."), Is.False);
		}

		private bool containsEvent(EventReports events, string description)
		{
			foreach (EventReport eventReport in events)
			{
				if (eventReport.Description == description)
				{
					return true;
				}
			}
			return false;
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
		public void ExecuteUseOrder_ContinuesSavedProducingModuleWithoutConsumingAgain()
		{
			Sequence.Ints.Push(109);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack factory = this.game.ModuleStacks["000004"];
			ModuleStack parent = this.game.ModuleStacks["000001"];
			Technology armcbt = Technology.All["armcbt"];
			Assert.That(factory.Technologies.Contains(armcbt));

			ModuleStack receiver = ModuleStack.All.GetOrCreateNewModuleStack(factory.Owner, "109");
			new ProducingModule(factory, armcbt, 2, receiver, parent);
			factory.ItemStacks.Minus(ItemType.All["iron"], 19);
			Assert.That(factory.ItemStacks.Quantity("iron"), Is.EqualTo(1));

			List<string> testcommands = new List<string>
			{
				"#faction 2",
				"#modulestack 000004",
				"use armcbt as 109 for 000001",
				"#end"
			};

			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(testcommands);
			UseOrder useOrder = (UseOrder)factory.Orders[0];

			this.executeOrder(factory, useOrder, 0);

			Assert.That(factory.Effects.IsProducing, Is.True);
			Assert.That(factory.Effects.Producing.Duration, Is.EqualTo(1));
			Assert.That(useOrder.Producing, Is.SameAs(factory.Effects.Producing));
			foreach (EventReport eventReport in factory.EventReports)
			{
				Assert.That(eventReport.Description, Does.Not.Contain("USE failed"));
			}
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
		public void ExecuteUseOrder_PharmsOnSickBay_IsUsable()
		{
			ModuleStack bay = new ModuleStack(
				Region.All["R00002"],
				this.game.Factions["2"],
				ModuleType.All["sckbay"],
				"100410");
			bay.AddModule();

			UseOrder order = new UseOrder(bay);
			order.Technology = Technology.All["pharms"];

			Assert.That(order.Usable(1), Is.True);
		}

		[Test]
		public void ExecuteUseOrder_PharmsOnCrewQuarters_Fails()
		{
			ModuleStack quarters = this.game.ModuleStacks["100016"];
			int foodBefore = quarters.ItemStacks[ItemType.All["food"]].Quantity;
			quarters.Technologies.Add(Technology.All["pharms"]);
			quarters.ItemStacks.Add(new ItemStack(ItemType.All["food"], 1));

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100016",
				"use pharms",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);

			quarters.Execute(this.game.Week);

			Assert.That(quarters.Effects.IsProducing, Is.False);
			Assert.That(quarters.ItemStacks[ItemType.All["food"]].Quantity, Is.EqualTo(foodBefore + 1));
		}

	}
}
