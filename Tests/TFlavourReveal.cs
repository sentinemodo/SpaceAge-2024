using System;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TFlavourReveal : TTest
	{
		[SetUp]
		public void setup()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.game = new Game();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void Research_IsSpaceObject_IncludesBelt()
		{
			this.LoadRevealWorld();
			Assert.That(Research.IsSpaceObject("P00003"), Is.True);
		}

		[Test]
		public void ResearchOrder_SpaceObjectAtPlanet_QueuesDescriptionToShow()
		{
			this.LoadRevealWorld();
			ModuleStack lab = ModuleStack.All["lab001"];
			ResearchOrder order = this.assignResearch(lab, "research P00001");

			Assert.That(order.ResearchType, Is.EqualTo(EResearchType.SpaceObject));
			Sequence.Ints.Push(5);
			order.Execute(this.game.Week);

			Assert.That(lab.Owner.ObjectsToShow.Contains("P00001"), Is.True);
			Assert.That(lab.ResearchPoints, Is.EqualTo(1));
		}

		[Test]
		public void ResearchOrder_SpaceObjectNotAtTarget_FailsWithoutResearchPoints()
		{
			this.LoadRevealWorld();
			ModuleStack lab = ModuleStack.All["lab001"];
			ResearchOrder order = this.assignResearch(lab, "research P00003");

			order.Execute(this.game.Week);

			Assert.That(lab.Owner.ObjectsToShow.Contains("P00003"), Is.False);
			Assert.That(lab.ResearchPoints, Is.EqualTo(0));
			Assert.That(this.eventWeek(lab.EventReports, "RESEARCH failed"), Is.EqualTo(this.game.Week));
		}

		[Test]
		public void ResearchOrder_SpaceObjectAtBelt_QueuesDescriptionToShow()
		{
			this.LoadRevealWorld();
			ModuleStack lab = ModuleStack.All["belt01"];
			ResearchOrder order = this.assignResearch(lab, "research P00003");

			Sequence.Ints.Push(5);
			order.Execute(this.game.Week);

			Assert.That(lab.Owner.ObjectsToShow.Contains("P00003"), Is.True);
		}

		[Test]
		public void ResearchOrder_SpaceObjectAtStar_NoProximityRequired()
		{
			this.LoadRevealWorld();
			ModuleStack lab = ModuleStack.All["lab001"];
			ResearchOrder order = this.assignResearch(lab, "research S00001");

			Sequence.Ints.Push(5);
			order.Execute(this.game.Week);

			Assert.That(lab.Owner.ObjectsToShow.Contains("S00001"), Is.True);
		}

		[Test]
		public void ResearchOrder_SpaceObjectAtRegion_QueuesRegionDescription()
		{
			this.LoadRevealWorld();
			ModuleStack lab = ModuleStack.All["lab001"];
			ResearchOrder order = this.assignResearch(lab, "research R00001");

			Sequence.Ints.Push(5);
			order.Execute(this.game.Week);

			Assert.That(lab.Owner.ObjectsToShow.Contains("R00001"), Is.True);
		}

		[Test]
		public void ResearchOrder_SpaceObject_DoesNotRepeatSeen()
		{
			this.LoadRevealWorld();
			ModuleStack lab = ModuleStack.All["lab001"];
			Faction faction = lab.Owner;
			faction.ObjectsSeen.Add(Planet.All["P00001"]);

			ResearchOrder order = this.assignResearch(lab, "research P00001");
			Sequence.Ints.Push(5);
			order.Execute(this.game.Week);

			Assert.That(faction.ObjectsToShow.Contains("P00001"), Is.False);
		}

		[Test]
		public void SurveyReport_AppearsBetweenBankAndGalaxy()
		{
			this.LoadDefaultGame();
			Faction faction = this.game.Factions["2"];
			faction.ObjectsToShow.Add(Planet.All["P00002"]);
			Planet.All["P00002"].Description = "Test survey blurb.";
			foreach (Faction f in this.game.Factions.Values)
			{
				f.Options.XmlReport = false;
			}

			ReportWriter writer = new ReportWriter(this.game, this.dataFile, Directory.GetCurrentDirectory());
			writer.GenerateReports(Directory.GetCurrentDirectory());

			string[] lines = File.ReadAllLines(
				Path.Combine(Directory.GetCurrentDirectory(), string.Format("report.{0}.2.txt", this.game.Turn)),
				System.Text.Encoding.GetEncoding(1251));

			int bankIndex = Array.FindIndex(lines, l => l.StartsWith("Bank report:"));
			int galaxyIndex = Array.FindIndex(lines, l => l.StartsWith("Galaxy report:"));
			int surveyIndex = Array.FindIndex(lines, l => l == "Survey reports:");

			Assert.That(surveyIndex, Is.GreaterThan(bankIndex));
			Assert.That(surveyIndex, Is.LessThan(galaxyIndex));
		}

		[Test]
		public void FactionAllShown_PromotesShownObjectsToSeen()
		{
			this.LoadRevealWorld();
			Faction faction = Faction.All["2"];
			faction.ObjectsToShow.Add(Planet.All["P00001"]);

			faction.AllShown();

			Assert.That(faction.ObjectsToShow.Count, Is.EqualTo(0));
			Assert.That(faction.ObjectsSeen.Contains("P00001"), Is.True);
		}

		[Test]
		public void ObjectsSeen_PersistThroughSaveAndLoad()
		{
			this.LoadRevealWorld();
			Faction faction = Faction.All["2"];
			faction.ObjectsSeen.Add(Planet.All["P00001"]);
			this.dataFile.SaveGame(Directory.GetCurrentDirectory(), "gameout.survey.test.xml");

			this.game.ClearDictionaries();

			DataFile reloaded = new DataFile(Directory.GetCurrentDirectory());
			reloaded.LoadConfiguration();
			reloaded.LoadGameDocument(Directory.GetCurrentDirectory(), "gameout.survey.test.xml");
			reloaded.LoadFactions();
			reloaded.LoadGalaxy();

			Assert.That(Faction.All["2"].ObjectsSeen.Contains("P00001"), Is.True);
		}

		[Test]
		public void Turn1Reports_SeedsHomeStarAndPlanet()
		{
			this.dataFile.LoadConfiguration(TCampaign.CampaignDir(), "data.xml");
			this.dataFile.LoadGameDocument(TCampaign.CampaignDir(), "gamein.1.xml");
			this.dataFile.LoadTurnNumber();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
			foreach (Faction faction in this.game.Factions.Values)
			{
				faction.Options.XmlReport = false;
			}

			ReportWriter writer = new ReportWriter(this.game, this.dataFile, Directory.GetCurrentDirectory());
			writer.GenerateReports(Directory.GetCurrentDirectory());

			string faction2Report = File.ReadAllText(
				Path.Combine(Directory.GetCurrentDirectory(), "report.1.2.txt"),
				System.Text.Encoding.GetEncoding(1251));
			string faction7Report = File.ReadAllText(
				Path.Combine(Directory.GetCurrentDirectory(), "report.1.7.txt"),
				System.Text.Encoding.GetEncoding(1251));

			Assert.That(faction2Report, Does.Contain("Survey reports:"));
			Assert.That(faction2Report, Does.Contain("Helios"));
			Assert.That(faction2Report, Does.Contain("Arbor"));
			Assert.That(faction7Report, Does.Contain("Fomal"));
			Assert.That(faction7Report, Does.Contain("Anvil"));
		}

		[Test]
		public void SampleGame_Report_HasNoSurveySection()
		{
			this.LoadDefaultGame();
			foreach (Faction faction in this.game.Factions.Values)
			{
				faction.Options.XmlReport = false;
			}

			ReportWriter writer = new ReportWriter(this.game, this.dataFile, Directory.GetCurrentDirectory());
			writer.GenerateReports(Directory.GetCurrentDirectory());

			string report = File.ReadAllText(
				Path.Combine(Directory.GetCurrentDirectory(), string.Format("report.{0}.2.txt", this.game.Turn)),
				System.Text.Encoding.GetEncoding(1251));
			Assert.That(report, Does.Not.Contain("Survey reports:"));
		}

		private static string TestsDir()
		{
			string dir = TestContext.CurrentContext.TestDirectory;
			for (int i = 0; i < 10; i++)
			{
				if (File.Exists(Path.Combine(dir, "data.xml"))
					&& Directory.Exists(Path.Combine(dir, "fixtures", "reveal")))
				{
					return dir;
				}
				DirectoryInfo parent = Directory.GetParent(dir);
				if (parent == null)
				{
					break;
				}
				dir = parent.FullName;
			}
			throw new DirectoryNotFoundException("Could not find Tests/fixtures/reveal from " + TestContext.CurrentContext.TestDirectory);
		}

		private string RevealFixtureDir()
		{
			return Path.Combine(TestsDir(), "fixtures", "reveal");
		}

		private void LoadRevealWorld()
		{
			string testsDir = TestsDir();
			string fixtureDir = this.RevealFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(testsDir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private ResearchOrder assignResearch(ModuleStack lab, string command)
		{
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(new System.Collections.Generic.List<string>
			{
				"#faction " + lab.Owner.Name,
				"#modulestack " + lab.Name,
				command,
				"#end"
			});
			return (ResearchOrder)lab.Orders[0];
		}

		private int eventWeek(EventReports events, string descriptionFragment)
		{
			int week = -1;
			foreach (EventReport eventReport in events)
			{
				if (eventReport.Description.IndexOf(descriptionFragment) >= 0)
				{
					week = eventReport.Week;
				}
			}
			return week;
		}
	}
}
