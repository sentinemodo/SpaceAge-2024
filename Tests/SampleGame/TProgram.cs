using System;
using System.IO;
using NUnit.Framework;
using SpaceAge;
using UnitTests;

namespace IntegrationTests
{
	[TestFixture]
	public class TProgram : TTest
	{
		private string runRoot;
		private string dataDir;
		private string turnDir;

		[SetUp]
		public void setup()
		{
			this.runRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "campaign-reports-run");
			this.dataDir = Path.Combine(this.runRoot, "data");
			this.turnDir = Path.Combine(this.runRoot, "turn");
			if (Directory.Exists(this.runRoot))
			{
				Directory.Delete(this.runRoot, true);
			}
			Directory.CreateDirectory(this.dataDir);
			Directory.CreateDirectory(this.turnDir);

			string campaignDir = TCampaign.CampaignDir();
			File.Copy(Path.Combine(campaignDir, "data.xml"), Path.Combine(this.dataDir, "data.xml"));
			File.Copy(Path.Combine(campaignDir, "gamein.1.xml"), Path.Combine(this.dataDir, "gamein.xml"));
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
			new Game().ClearDictionaries();
			if (this.runRoot != null && Directory.Exists(this.runRoot))
			{
				try
				{
					Directory.Delete(this.runRoot, true);
				}
				catch (IOException)
				{
				}
			}
		}

		[Test]
		public void Main_ReportsFlag_WritesTurn1ReportsWithoutExecute()
		{
			Program.Main(new string[]
			{
				"/data", this.dataDir,
				"/turn-dir", this.turnDir,
				"/reports"
			});

			Assert.That(File.Exists(Path.Combine(this.dataDir, "gamein.xml")), Is.True);
			Assert.That(File.Exists(Path.Combine(this.turnDir, "gamein.xml")), Is.False);
			Assert.That(File.Exists(Path.Combine(this.dataDir, "gameout.1.xml")), Is.False);
			Assert.That(File.Exists(Path.Combine(this.dataDir, "gameout.2.xml")), Is.False);

			for (int faction = 1; faction <= 13; faction++)
			{
				string stem = "report.1." + faction;
				Assert.That(File.Exists(Path.Combine(this.turnDir, stem + ".txt")), Is.True, stem + ".txt");
				Assert.That(File.Exists(Path.Combine(this.turnDir, stem + ".xml")), Is.True, stem + ".xml");
				Assert.That(File.Exists(Path.Combine(this.turnDir, "report.2." + faction + ".txt")), Is.False);
			}
		}
	}
}
