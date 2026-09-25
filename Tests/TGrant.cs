using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TGrant : TTest
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
		public void GrantCost_MatchesEconomyFormulas()
		{
			Assert.That(GrantCost.TechnologyCredits(Technology.All["armcbt"]), Is.EqualTo(1000));
			Assert.That(GrantCost.SkillCredits(SkillType.All["hmedic"]), Is.EqualTo(900));
			Assert.That(GrantCost.ItemCredits(ItemType.All["iron"], 30), Is.EqualTo(240));
			Assert.That(GrantCost.ItemCredits(ItemType.All["cash"], 50), Is.EqualTo(50));
			Assert.That(GrantCost.ModuleCredits(ModuleType.All["farms"], 1), Is.EqualTo(250));
		}

		[Test]
		public void Parse_GrantTechnology_BetweenTurns()
		{
			List<string> commands = new List<string>
			{
				"#faction 2",
				"GRANT technology armcbt to 100002",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);
			GrantOrder grant = (GrantOrder)Faction.All["2"].Orders[Faction.All["2"].Orders.Count - 1];
			Assert.That(grant.AllowedBetweenTurns, Is.True);
			Assert.That(grant.GrantKind, Is.EqualTo(EGrantKind.technology));
			Assert.That(grant.Technology.Name, Is.EqualTo("armcbt"));
			Assert.That(grant.TargetName, Is.EqualTo("100002"));
			Assert.That(grant.CostCredits(), Is.EqualTo(1000));
		}

		[Test]
		public void ExecuteBetweenTurn_GrantTechnology_DebitsBankAndDeliversCopy()
		{
			Faction faction = Faction.All["2"];
			ModuleStack target = ModuleStack.All["100002"];
			double balanceBefore = faction.Bank.Balance;
			Assert.That(target.Technologies.Contains("armcbt"), Is.False);

			List<string> commands = new List<string>
			{
				"#faction 2",
				"GRANT technology armcbt to 100002",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			this.game.ExecuteBetweenTurnOrders();

			Assert.That(faction.Bank.Balance, Is.EqualTo(balanceBefore - 1000));
			Assert.That(target.Technologies.Contains("armcbt"), Is.True);
		}

		[Test]
		public void ExecuteBetweenTurn_GrantSkill_ToPerson()
		{
			Faction faction = Faction.All["2"];
			Person trainee = Person.All["200001"];
			Assert.That(trainee.Skills.Has(SkillType.All["hmedic"]), Is.False);

			List<string> commands = new List<string>
			{
				"#faction 2",
				"GRANT skill hmedic to 200001",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			this.game.ExecuteBetweenTurnOrders();

			Assert.That(faction.Bank.Balance, Is.EqualTo(9100));
			Assert.That(trainee.Skills.Has(SkillType.All["hmedic"]), Is.True);
		}

		[Test]
		public void ExecuteBetweenTurn_GrantItem_DebitsNominalCost()
		{
			Faction faction = Faction.All["2"];
			ModuleStack target = ModuleStack.All["100001"];
			int ironBefore = target.ItemStacks.ContainsKey(ItemType.All["iron"])
				? target.ItemStacks[ItemType.All["iron"]].Quantity
				: 0;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"GRANT item 5 iron to 100001",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			this.game.ExecuteBetweenTurnOrders();

			Assert.That(faction.Bank.Balance, Is.EqualTo(9960));
			Assert.That(target.ItemStacks[ItemType.All["iron"]].Quantity, Is.EqualTo(ironBefore + 5));
		}

		[Test]
		public void ExecuteBetweenTurn_GrantFails_WhenInsufficientFunds()
		{
			Faction faction = Faction.All["2"];
			faction.Bank.Balance = 100;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"GRANT technology armcbt to 100002",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			this.game.ExecuteBetweenTurnOrders();

			Assert.That(ModuleStack.All["100002"].Technologies.Contains("armcbt"), Is.False);
			Assert.That(faction.Bank.Balance, Is.EqualTo(100));
		}
	}
}
