using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TNpcFactionSupply : TTest
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
		public void NpcOwnedStack_StaysActiveWithoutCrewOrFuel()
		{
			ModuleStack berlin = this.game.ModuleStacks["000001"];
			ModuleStack wind = this.game.ModuleStacks["000009"];
			Assert.That(berlin.Owner.Name, Is.EqualTo("1"));
			Assert.That(berlin.IsActive, Is.True);
			Assert.That(wind.IsActive, Is.True);
		}

		[Test]
		public void NpcOwnedStack_SkipsQuarterlyMaintenance()
		{
			ModuleStack berlin = this.game.ModuleStacks["000001"];
			ItemType cash = ItemType.All["cash"];
			int before = berlin.ItemStacks.Quantity(cash);
			berlin.ExecuteMaintenance(13);
			Assert.That(berlin.ItemStacks.Quantity(cash), Is.EqualTo(before));
		}

		[Test]
		public void PlayerOwnedStack_StillPaysMaintenance()
		{
			ModuleStack hq = this.game.ModuleStacks["000112"];
			Assert.That(hq, Is.Not.Null, "player HQ stack 000112 must exist in Tests/gamein.xml");
			Assert.That(ContractScope.IsPlayerFaction(hq.Owner), Is.True);
			int bankBefore = (int)hq.Owner.Bank.Balance;
			this.game.Week = 1;
			this.game.ExecuteMaintenance();
			Assert.That((int)hq.Owner.Bank.Balance, Is.LessThan(bankBefore));
		}
	}
}
