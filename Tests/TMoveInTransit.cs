using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMoveInTransit : TTest
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

		private MoveOrder StartMoveTowardR00002(int executeWeeks)
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack stack = this.game.ModuleStacks["100001"];
			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"move R00002",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			MoveOrder order = (MoveOrder)stack.Orders[0];
			for (int i = 0; i < executeWeeks; i++)
			{
				stack.ExecutedLongOrder = false;
				stack.Orders.Execute(this.game.Week);
				stack.Effects.Execute(this.game.Week);
			}
			return order;
		}

		[Test]
		public void MovingEffect_ReportNamesOriginAndDestination()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			Region origin = Region.All["R00001"];
			Region dest = Region.All["R00002"];
			this.StartMoveTowardR00002(1);

			Moving moving = stack.Effects.Moving;
			ClassicAssert.NotNull(moving);
			ClassicAssert.NotNull(moving.Origin);
			StringAssert.Contains(origin.Name, moving.Description);
			StringAssert.Contains(dest.Name, moving.Description);
			StringAssert.Contains("weeks remaining", moving.Description);
		}

		[Test]
		public void MoveOrder_InProgressReport_IncludesOriginAndDestination()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			Faction faction = this.game.Factions["2"];
			MoveOrder order = this.StartMoveTowardR00002(2);

			string line = order.Report(faction)[0];
			StringAssert.Contains("R00001", line);
			StringAssert.Contains("R00002", line);
		}

		[Test]
		public void MoveBack_AfterTwoWeeks_ArrivesOriginInTwoWeeks()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			Region origin = Region.All["R00001"];
			this.StartMoveTowardR00002(2);

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"move back",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			ClassicAssert.AreEqual(1, stack.Orders.Count);
			MoveOrder order = (MoveOrder)stack.Orders[0];
			ClassicAssert.AreEqual(origin, stack.MovingTo);
			ClassicAssert.AreEqual(2, order.DurationLeft);

			for (int i = 0; i < 2; i++)
			{
				stack.ExecutedLongOrder = false;
				stack.Orders.Execute(this.game.Week);
				stack.Effects.Execute(this.game.Week);
			}

			ClassicAssert.AreEqual(origin, stack.Parent);
			ClassicAssert.IsNull(stack.MovingTo);
			ClassicAssert.IsFalse(stack.Effects.IsMoving);
		}

		[Test]
		public void MoveToOriginRegionId_EquivalentToMoveBack()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			Region origin = Region.All["R00001"];
			this.StartMoveTowardR00002(2);

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"move R00001",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			MoveOrder order = (MoveOrder)stack.Orders[0];
			ClassicAssert.AreEqual(origin, order.Destination);
			ClassicAssert.AreEqual(2, order.DurationLeft);
		}

		[Test]
		public void MoveThirdRegionWhileInTransit_FailsThenContinuesToDestination()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			MoveOrder order = this.StartMoveTowardR00002(1);
			int durationBefore = order.DurationLeft;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"move O00003",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);

			stack.ExecutedLongOrder = false;
			stack.Orders.Execute(this.game.Week);
			stack.Effects.Execute(this.game.Week);
			ClassicAssert.IsTrue(
				stack.EventReports.Exists(r => r.Description.IndexOf("MOVE failed") >= 0));
			ClassicAssert.AreEqual(Region.All["R00002"], stack.MovingTo);
			ClassicAssert.AreEqual(durationBefore - 1, order.DurationLeft);
			ClassicAssert.IsTrue(stack.Effects.IsMoving);
		}

		[Test]
		public void GetOrder_WhileInTransit_SilentFail()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			ModuleStack depot = this.game.ModuleStacks["100002"];
			this.StartMoveTowardR00002(1);

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"get 1 iron from 100002",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);

			stack.ExecutedLongOrder = false;
			stack.Orders.Execute(this.game.Week);
			GetOrder get = (GetOrder)stack.Orders.Find(o => o is GetOrder);
			ClassicAssert.NotNull(get);
			ClassicAssert.IsFalse(get.Executed);
			ClassicAssert.IsFalse(
				stack.EventReports.Exists(r => r.Description.IndexOf("GET") >= 0));
		}

		[Test]
		public void LongOrder_WhileInTransit_DoesNotExecute()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			this.StartMoveTowardR00002(1);

			UseOrder use = new UseOrder(stack);
			use.Technology = Technology.All["agrplx"];
			stack.Orders.Add(use);

			stack.ExecutedLongOrder = false;
			stack.Orders.Execute(this.game.Week);
			ClassicAssert.IsFalse(use.Executed);
			ClassicAssert.IsFalse(use.Executing);
		}

		[Test]
		public void TravelEffectiveRegion_BeforeHalfWay_IsOrigin()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			Region origin = Region.All["R00001"];
			Region dest = Region.All["R00002"];
			this.StartMoveTowardR00002(1);

			Moving moving = stack.Effects.Moving;
			ClassicAssert.AreEqual(origin, stack.TravelEffectiveRegion(moving));
		}

		[Test]
		public void TravelEffectiveRegion_AfterHalfWay_IsDestination()
		{
			ModuleStack stack = this.game.ModuleStacks["100001"];
			Region dest = Region.All["R00002"];
			this.StartMoveTowardR00002(4);

			Moving moving = stack.Effects.Moving;
			ClassicAssert.AreEqual(dest, stack.TravelEffectiveRegion(moving));
		}
	}
}
