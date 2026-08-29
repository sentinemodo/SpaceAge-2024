using System;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TCampaign : TTest
	{
		public static string RepoRoot()
		{
			string dir = TestContext.CurrentContext.TestDirectory;
			for (int i = 0; i < 10; i++)
			{
				if (File.Exists(Path.Combine(dir, "campaign", "data.xml")))
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
			throw new DirectoryNotFoundException("Could not find campaign/data.xml from " + TestContext.CurrentContext.TestDirectory);
		}

		public static string CampaignDir()
		{
			return Path.Combine(RepoRoot(), "campaign");
		}

		[SetUp]
		public void setup()
		{
			this.dataFile = new DataFile(CampaignDir());
			this.game = new Game();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void LoadConfiguration_CampaignCatalog_LoadsAdpnt()
		{
			this.dataFile.LoadConfiguration(CampaignDir(), "data.xml");
			this.game = this.dataFile.Game;

			Assert.That(this.game.PlanetTypes.ContainsKey("adpnt"));
			Assert.DoesNotThrow(() => this.dataFile.ValidateTypeNameUniqueness());
			foreach (ModuleType moduleType in ModuleType.All.Values)
			{
				Assert.That(ModuleTypeGroupXml.ToToken(moduleType.Group), Is.Not.Null,
					"unknown module group on " + moduleType.Name);
			}
		}
	}
}
