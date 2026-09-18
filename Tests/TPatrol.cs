using System.Collections.Generic;
using System.IO;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TPatrol : TTest
	{
		[SetUp]
		public void setupPatrol()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownPatrol()
		{
			this.ClearGame();
		}

		[Test]
		public void SetPatrolTrue_SetsFlagAndPersistsXml()
		{
			ModuleStack stack = this.game.ModuleStacks["100011"];
			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100011",
				"set patrol true",
				"#end"
			};

			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);
			SetOrder order = (SetOrder)stack.Orders[0];
			order.Execute(this.game.Week);

			Assert.That(stack.IsPatrolling, Is.True);
			Assert.That(order.Executed, Is.True);

			XmlDocument doc = new XmlDocument();
			XmlElement elStack = stack.SaveXml(doc, this.game.Factions["2"]);
			Assert.That(elStack.GetAttribute("patrol"), Is.EqualTo("true"));
		}

		[Test]
		public void PatrolBlocksHostileMove()
		{
			ModuleStack patroller = this.game.ModuleStacks["100011"];
			ModuleStack mover = this.game.ModuleStacks["100001"];
			Region target = Region.All["R00002"];

			patroller.Owner = this.game.Factions["1"];
			patroller.Parent = target;
			patroller.IsPatrolling = true;
			mover.Parent = Region.All["R00001"];
			this.game.Factions["2"].Attitudes["1"] = FactionAttitude.Hostile;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"move R00002",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			MoveOrder move = (MoveOrder)mover.Orders[0];
			mover.Execute(this.game.Week);

			Assert.That(move.Executed, Is.True);
			Assert.That(mover.MovingTo, Is.Null);
			Assert.That(PatrolGuard.FindBlocker(target, this.game.Factions["2"]), Is.SameAs(patroller));
		}

		[Test]
		public void PatrolAllowsNeutralMove()
		{
			ModuleStack patroller = this.game.ModuleStacks["100011"];
			ModuleStack mover = this.game.ModuleStacks["100001"];
			Region target = Region.All["R00002"];

			patroller.Owner = this.game.Factions["1"];
			patroller.Parent = target;
			patroller.IsPatrolling = true;
			mover.Parent = Region.All["R00001"];
			this.game.Factions["2"].Attitudes["1"] = FactionAttitude.Neutral;

			Assert.That(PatrolGuard.FindBlocker(target, this.game.Factions["2"]), Is.Null);
		}

		[Test]
		public void PatrolRequiresArmedOperational()
		{
			ModuleStack patroller = this.game.ModuleStacks["100001"];
			Region target = Region.All["R00002"];

			patroller.Parent = target;
			patroller.IsPatrolling = true;
			this.game.Factions["2"].Attitudes["1"] = FactionAttitude.Hostile;

			Assert.That(patroller.IsArmed, Is.False);
			Assert.That(PatrolGuard.FindBlocker(target, this.game.Factions["2"]), Is.Null);
		}

		[Test]
		public void PatrolBlocksNonAllyRename()
		{
			ModuleStack patroller = this.game.ModuleStacks["100011"];
			ModuleStack namer = this.game.ModuleStacks["100001"];
			Region target = Region.All["R00002"];

			patroller.Owner = this.game.Factions["1"];
			patroller.Parent = target;
			patroller.IsPatrolling = true;
			namer.Parent = target;

			NameOrder order = new NameOrder(namer, target, "Blocked Name");
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.False);
			Assert.That(target.FullName, Is.Not.EqualTo("Blocked Name"));
		}

		[Test]
		public void PatrolAllowsAllyRename()
		{
			ModuleStack patroller = this.game.ModuleStacks["100011"];
			ModuleStack namer = this.game.ModuleStacks["100002"];
			Region target = Region.All["R00002"];

			patroller.Owner = this.game.Factions["1"];
			patroller.Parent = target;
			patroller.IsPatrolling = true;
			namer.Parent = target;
			this.game.Factions["2"].Attitudes["1"] = FactionAttitude.Ally;

			NameOrder order = new NameOrder(namer, target, "Allied Rename");
			order.Execute(this.game.Week);

			Assert.That(order.Executed, Is.True);
			Assert.That(target.FullName, Is.EqualTo("Allied Rename"));
		}

		[Test]
		public void AttackRegionDeclaresAndMoves()
		{
			ModuleStack attacker = this.game.ModuleStacks["100001"];
			Region target = Region.All["R00002"];
			ModuleStack patroller = this.game.ModuleStacks["100011"];
			patroller.Owner = this.game.Factions["1"];
			patroller.Parent = target;
			attacker.Parent = Region.All["R00001"];
			this.game.Factions["2"].Attitudes["1"] = FactionAttitude.Enemy;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"attack region R00002",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			AttackOrder attack = (AttackOrder)attacker.Orders[0];
			Assert.That(attack.IsRegionTarget, Is.True);
			Assert.That(attack.TargetRegion, Is.SameAs(target));
			Assert.That(attacker.Location.Name, Is.EqualTo("R00001"));

			attack.Execute(this.game.Week);
			Assert.That(attacker.Orders.Count, Is.EqualTo(2));

			attacker.Execute(this.game.Week);

			Assert.That(attacker.Owner.UnitAttitudes.ContainsKey("100011"), Is.True);
			Assert.That(attacker.Owner.UnitAttitudes["100011"], Is.EqualTo(FactionAttitude.Enemy));
			Assert.That(attacker.MovingTo, Is.EqualTo(target));
		}

		[Test]
		public void CaptureRegionSetsTacticAndMoves()
		{
			ModuleStack unit = this.game.ModuleStacks["100001"];
			Region target = Region.All["R00002"];
			unit.Parent = Region.All["R00001"];

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"capture region R00002",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			CaptureOrder capture = (CaptureOrder)unit.Orders[0];
			Assert.That(capture.IsRegionTarget, Is.True);

			capture.Execute(this.game.Week);
			Assert.That(unit.Orders.Count, Is.EqualTo(2));

			unit.Execute(this.game.Week);

			Assert.That(unit.HasCapture, Is.True);
			Assert.That(unit.PreferredTargetName, Is.EqualTo("all"));
			Assert.That(unit.MovingTo, Is.EqualTo(target));
		}

		[Test]
		public void GiveFailsBelowNeutralAttitude()
		{
			ModuleStack giver = this.game.ModuleStacks["100001"];
			ModuleStack receiver = this.game.ModuleStacks["000010"];
			giver.Parent = Region.All["R00002"];
			this.game.Factions["2"].Attitudes["1"] = FactionAttitude.Hostile;

			ItemType food = ItemType.All["food"];
			giver.ItemStacks.Add(new ItemStack(food, 5));
			int before = giver.ItemStacks[food].Quantity;

			GiveOrder give = new GiveOrder(giver, receiver.Name, food, 1);
			give.Execute(this.game.Week);

			Assert.That(give.Executed, Is.False);
			Assert.That(giver.ItemStacks[food].Quantity, Is.EqualTo(before));
		}
	}
}
