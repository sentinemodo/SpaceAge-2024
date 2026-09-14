using System.Collections.Generic;
using NUnit.Framework;
using SpaceAge;
using UnitTests;

namespace UnitTests
{
	[TestFixture]
	public class TFaunaRumors : TTest
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
		public void FaunaRumors_NeighboringSettlement_IssuesRumorOnPlanet()
		{
			Faction fauna = new Faction("14", "Arbor Fauna");
			Region faunaRegion = this.game.Regions["R00002"];
			ModuleStack stack = new ModuleStack(faunaRegion, fauna, ModuleType.All["inftry"], "140099");
			stack.AddModule();

			FaunaRumors.IssueAll();

			Assert.That(PressRelease.All.Count, Is.EqualTo(1));
			Assert.That(PressRelease.All[0].Anonymous, Is.True);
			Assert.That(PressRelease.All[0].PlanetId, Is.EqualTo("P00002"));
			Assert.That(PressRelease.All[0].Title, Is.EqualTo("Hostile fauna in Eastern Europe"));
			Assert.That(PressRelease.All[0].Flavour, Does.Contain("[140099]"));
			Assert.That(PressRelease.All[0].Flavour, Does.Contain("Western Europe"));

			List<string> report = this.game.Factions["2"].Report();
			string reportText = string.Join("\n", report.ToArray());
			Assert.That(reportText, Does.Contain("Rumors:"));
			Assert.That(reportText, Does.Contain("Hostile fauna in Eastern Europe"));
		}

		[Test]
		public void FaunaRumors_NoNeighboringSettlement_NoRumor()
		{
			Faction fauna = new Faction("14", "Arbor Fauna");
			Region isolated = this.game.Regions["R10005"];
			ModuleStack stack = new ModuleStack(isolated, fauna, ModuleType.All["inftry"], "140100");
			stack.AddModule();

			FaunaRumors.IssueAll();

			Assert.That(PressRelease.All.Count, Is.EqualTo(0));
		}

		[Test]
		public void FaunaRumors_Dedup_DoesNotIssueTwice()
		{
			Faction fauna = new Faction("14", "Arbor Fauna");
			Region faunaRegion = this.game.Regions["R00002"];
			ModuleStack stack = new ModuleStack(faunaRegion, fauna, ModuleType.All["inftry"], "140099");
			stack.AddModule();

			FaunaRumors.IssueAll();
			FaunaRumors.IssueAll();

			Assert.That(PressRelease.All.Count, Is.EqualTo(1));
		}
	}
}
