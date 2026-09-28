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
		public void ExecuteBetweenTurn_GrantTechnology_WithoutFactionPriorKnowledge()
		{
			Faction faction = Faction.All["2"];
			ModuleStack target = ModuleStack.All["100002"];
			Technology indust = Technology.All["indust"];
			Assert.That(indust, Is.Not.Null);

			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Owner == faction && stack.HasTechnology(indust))
				{
					Assert.Fail("fixture should not start with indust on faction 2");
				}
			}

			double balanceBefore = faction.Bank.Balance;
			List<string> commands = new List<string>
			{
				"#faction 2",
				"GRANT technology indust to 100002",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			this.game.ExecuteBetweenTurnOrders();

			Assert.That(
				target.Technologies.Contains("indust"),
				"GRANT should deliver a tech copy without prior faction knowledge");
			Assert.That(faction.Bank.Balance, Is.EqualTo(balanceBefore - GrantCost.TechnologyCredits(indust)));
		}

		[Test]
		public void Execute_GrantItem_ToNewAlias_DebitsBankAndDelivers()
		{
			Faction faction = Faction.All["2"];
			ModuleStack target = ModuleStack.All.GetOrCreateNewModuleStack(faction, "new2");
			target.ModuleType = ModuleType.All["cargob"];
			target.AddModule();
			double balanceBefore = faction.Bank.Balance;

			GrantOrder grant = new GrantOrder(faction);
			grant.Parse("item 10 titani to new2");
			grant.Execute(1);

			Assert.That(grant.Executed, Is.True);
			Assert.That(faction.Bank.Balance, Is.EqualTo(balanceBefore - GrantCost.ItemCredits(ItemType.All["titani"], 10)));
			Assert.That(target.ItemStacks[ItemType.All["titani"]].Quantity, Is.EqualTo(10));
		}

		[Test]
		public void Execute_GrantItem_ToNewAliasBeforeFormed_FailsWithMessage()
		{
			Faction faction = Faction.All["2"];
			ModuleStack placeholder = ModuleStack.All.GetOrCreateNewModuleStack(faction, "new3", true);
			Assert.That(placeholder.IsFormed, Is.False);

			GrantOrder grant = new GrantOrder(faction);
			grant.Parse("item 10 titani to new3");
			grant.Execute(1);

			Assert.That(grant.Executed, Is.False);
			bool reportedNotFormed = false;
			foreach (EventReport eventReport in faction.EventReports)
			{
				if (eventReport.Week == 1
					&& eventReport.Description.Contains("GRANT failed. Target stack is not formed."))
				{
					reportedNotFormed = true;
					break;
				}
			}
			Assert.That(reportedNotFormed, Is.True);
		}

		[Test]
		public void Execute_GrantItem_UnderModuleStackSubject_DebitsOwnerFaction()
		{
			Faction faction = Faction.All["2"];
			ModuleStack target = ModuleStack.All["100001"];
			int ironBefore = target.ItemStacks.ContainsKey(ItemType.All["iron"])
				? target.ItemStacks[ItemType.All["iron"]].Quantity
				: 0;
			double balanceBefore = faction.Bank.Balance;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"GRANT item 5 iron to 100001",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			GrantOrder grant = (GrantOrder)target.Orders[target.Orders.Count - 1];
			grant.Execute(1);

			Assert.That(faction.Bank.Balance, Is.EqualTo(balanceBefore - 40));
			Assert.That(target.ItemStacks[ItemType.All["iron"]].Quantity, Is.EqualTo(ironBefore + 5));
			Assert.That(grant.Executed, Is.True);
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
