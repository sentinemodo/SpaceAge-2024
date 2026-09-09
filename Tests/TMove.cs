using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMove : TTest
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

            int expected = SpaceTransit.DurationWeeks(0.04, SpaceTransit.EffectiveSpaceSpeed(testModuleStack));
            testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.Effects.IsMoving);
            Assert.That(testModuleStack.MovingTo, Is.EqualTo(orbit2));
            Assert.That(testModuleStack.Parent, Is.EqualTo(orbit1));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(expected - 1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

            for (int week = 1; week < expected; week++)
            {
                testModuleStack.ExecutedLongOrder = false;
                testModuleStack.Execute(this.game.Week + week);
            }
            Assert.That(testModuleStack.Effects.IsMoving, Is.False);
            Assert.That(testModuleStack.MovingTo, Is.Null);
            Assert.That(testModuleStack.Parent, Is.EqualTo(orbit2));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);
        }


		[Test]
		public void ExecuteMoveOrder_InProgress_ReportAndSaveHaveSingleDestination()
		{
			this.AssignMoveOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			MoveOrder order = (MoveOrder)testModuleStack.Orders[0];
			Faction faction = this.game.Factions["2"];

			for (int week = 0; week < 3; week++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders.Execute(this.game.Week);
				testModuleStack.Effects.Execute(this.game.Week);
			}

			Assert.That(order.Route.Count, Is.EqualTo(0));
			Assert.That(order.Report(faction)[0], Is.EqualTo("move R00002"));
			Assert.That(order.DurationLeft, Is.EqualTo(3));

			XmlDocument doc = new XmlDocument();
			XmlElement elOrder = order.SaveXml(doc, "modulestack");
			Assert.That(elOrder.GetAttribute("duration-left"), Is.EqualTo("3"));
			Assert.That(elOrder.SelectNodes("move/destination").Count, Is.EqualTo(1));
			Assert.That(((XmlElement)elOrder.SelectNodes("move/destination")[0]).GetAttribute("destination"), Is.EqualTo("R00002"));
		}


		[Test]
		public void ExecuteMoveOrder_Reload_ContinuesDurationWithoutDeparting()
		{
			this.AssignMoveOrder();
			ModuleStack testModuleStack = this.game.ModuleStacks["100001"];
			MoveOrder order = (MoveOrder)testModuleStack.Orders[0];

			for (int week = 0; week < 3; week++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders.Execute(this.game.Week);
				testModuleStack.Effects.Execute(this.game.Week);
			}

			XmlDocument doc = new XmlDocument();
			XmlElement elOrder = order.SaveXml(doc, "modulestack");

			testModuleStack.MovingTo = null;
			testModuleStack.Orders.Clear();
			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.EventReports.Clear();

			MoveOrder loaded = new MoveOrder(testModuleStack);
			loaded.LoadXml(elOrder);

			Assert.That(loaded.DurationLeft, Is.EqualTo(3));

			loaded.Execute(this.game.Week);
			Assert.That(loaded.DurationLeft, Is.EqualTo(2));
			Assert.That(testModuleStack.EventReports.Exists(eventReport => eventReport.Description.IndexOf("departed from") >= 0), Is.False);
			Assert.That(testModuleStack.EventReports.Exists(eventReport => eventReport.Description.IndexOf("moving from") >= 0), Is.True);
			Assert.That(testModuleStack.EventReports.Exists(eventReport => eventReport.Description.IndexOf("ETA 2") >= 0), Is.True);
		}

	}
}
