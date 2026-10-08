using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TPasswordOrder : TTest
	{
		[SetUp]
		public void Setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void Teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void PasswordOrder_ParseUnderFactionHeader()
		{
			Faction faction = this.game.Factions["2"];
			faction.Password = "xyzzy";

			List<string> commands = new List<string>
			{
				"#faction 2 \"xyzzy\"",
				"password \"new-secret\"",
				"#end",
			};
			new OrdersReader(this.game).AssignOrders(commands);

			Assert.That(faction.Orders.Count, Is.EqualTo(1));
			PasswordOrder order = (PasswordOrder)faction.Orders[0];
			Assert.That(order.NewPassword, Is.EqualTo("new-secret"));
		}

		[Test]
		public void PasswordOrder_ExecuteSchedulesChangeUntilTurnEnd()
		{
			Faction faction = this.game.Factions["2"];
			faction.Password = "xyzzy";

			List<string> commands = new List<string>
			{
				"#faction 2 \"xyzzy\"",
				"password new-secret",
				"#end",
			};
			new OrdersReader(this.game).AssignOrders(commands);

			PasswordOrder order = (PasswordOrder)faction.Orders[0];
			order.Execute(1);
			Assert.That(order.Executed, Is.True);
			Assert.That(faction.Password, Is.EqualTo("xyzzy"));
			Assert.That(faction.PendingPassword, Is.EqualTo("new-secret"));

			this.game.ApplyPendingFactionPasswords();
			Assert.That(faction.Password, Is.EqualTo("new-secret"));
			Assert.That(faction.PendingPassword, Is.Null);
		}

		[Test]
		public void PasswordOrder_RejectsEmptyPassword()
		{
			Faction faction = this.game.Factions["2"];
			faction.Password = "xyzzy";

			List<string> commands = new List<string>
			{
				"#faction 2 \"xyzzy\"",
				"password \"\"",
				"#end",
			};

			Assert.Throws<System.Exception>(() => new OrdersReader(this.game).AssignOrders(commands));
		}

		[Test]
		public void PasswordOrder_FailsOnModuleStackSubject()
		{
			Faction faction = this.game.Factions["2"];
			faction.Password = "xyzzy";
			ModuleStack stack = this.game.ModuleStacks["100001"];

			List<string> commands = new List<string>
			{
				"#faction 2 \"xyzzy\"",
				"#modulestack 100001",
				"password new-secret",
				"#end",
			};
			new OrdersReader(this.game).AssignOrders(commands);

			PasswordOrder order = (PasswordOrder)stack.Orders[stack.Orders.Count - 1];
			order.Execute(1);
			Assert.That(order.Executed, Is.False);
			Assert.That(faction.Password, Is.EqualTo("xyzzy"));
		}
	}
}
