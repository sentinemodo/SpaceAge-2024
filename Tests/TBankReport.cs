using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBankReport : TTest
	{
		[SetUp]
		public void Setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void Teardown()
		{
			this.game.Week = 1;
			this.ClearGame();
		}

		[Test]
		public void FactionReport_ListsBankActivityUnderBankReport_NotGeneralEvents()
		{
			Faction faction = Faction.All["2"];
			faction.Bank.Debit(1, 50, "GRANT item 20 food to 270003.");
			faction.EventReports.Add(1, "GRANT item 20 food to 270003.");

			List<string> report = faction.Report();
			string text = string.Join("\n", report.ToArray());

			Assert.That(text, Does.Contain("Bank report:"));
			Assert.That(text, Does.Contain("Account activity:"));
			Assert.That(text, Does.Contain("week 1: Bank account debited: 50 cash (GRANT item 20 food to 270003.). Balance:"));
			Assert.That(text, Does.Not.Contain("Events this quarter:\n  week 1: Bank account debited"));
		}

		[Test]
		public void ContractCompletion_AppearsInContractEventsSection()
		{
			DestroyStackTrigger trigger = new DestroyStackTrigger(ModuleStack.All["000001"]);
			trigger.Killer = Faction.All["2"];
			Contract contract = new Contract(
				"CT0099",
				Region.All["R00002"],
				Faction.All["1"],
				trigger,
				Technology.All["rckter"]);
			contract.RewardCash = 1000;
			contract.Title = "Test cull";

			contract.Award(5);

			List<string> report = Faction.All["2"].Report();
			string text = string.Join("\n", report.ToArray());

			Assert.That(text, Does.Contain("Contract events:"));
			Assert.That(text, Does.Contain("week 5: completed CT0099 (Test cull); reward: 1000 cash credited to bank"));
			Assert.That(
				text,
				Does.Not.Contain("Events this quarter:\n  week 5: completed contract CT0099"));
		}
	}
}
