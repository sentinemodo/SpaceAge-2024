using System.Collections.Generic;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TSkillGate : TTest
	{
		[Test]
		public void TrainSkill_FailsWhenFactionHasNotDiscoveredSkill()
		{
			Faction faction = this.game.Factions["2"];
			Person trainee = Person.All["000101"];
			Assert.That(faction.SkillsSeen.Contains("arpldr"), Is.False);

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#person 000101",
				"train skill arpldr",
				"#end"
			};
			new OrdersReader(this.game).AssignOrders(commands);
			trainee.Orders[0].Execute(this.game.Week);

			Assert.That(trainee.Skills.ContainsKey(SkillType.All["arpldr"]), Is.False);
			string events = string.Join(" ", trainee.EventReports.Report(faction));
			Assert.That(events, Does.Contain("TRAIN failed: skill"));
		}

		[Test]
		public void TechnologyReport_IncludesGrantedSkillLikeProducedItem()
		{
			Technology armcbt = Technology.All["armcbt"];
			Faction faction = this.game.Factions["2"];
			faction.TechnologiesToShow.Add(armcbt);

			List<string> lines = faction.TechnologiesToShow.ReportDescriptions(faction, 0);
			string joined = string.Join("\n", lines);
			Assert.That(joined, Does.Contain("armor platoon leader [arpldr]"));
			Assert.That(joined, Does.Contain("Training: 4 weeks"));
		}

		[Test]
		public void ResearchBreakthrough_RevealsGrantedSkillsToFaction()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack lab = ModuleStack.All["100012"];
			faction.TechnologiesSeen.Clear();
			faction.SkillsSeen.Clear();

			lab.ReceiveTechnologyCopy(Technology.All["armcbt"], this.game.Week, null);

			Assert.That(faction.SkillsSeen.Contains("arpldr"), Is.False);
			Assert.That(faction.SkillsToShow.Contains("arpldr"), Is.True);
			faction.AllShown();
			Assert.That(faction.SkillsSeen.Contains("arpldr"), Is.True);
		}
	}
}
