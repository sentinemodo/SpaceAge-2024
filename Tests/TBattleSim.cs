using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBattleSim
	{
		private string fixturePath;

		[SetUp]
		public void setup()
		{
			this.fixturePath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"fixtures",
				"battle-sim",
				"inftry-skirmish.xml");
		}

		[TearDown]
		public void teardown()
		{
			new Game().ClearDictionaries();
			Sequence.Reset();
		}

		[Test]
		public void Run_ProducesSimulatorHeaderAndResultBlock()
		{
			BattleSimulatorRunner runner = new BattleSimulatorRunner(Directory.GetCurrentDirectory());
			string output = runner.Run(this.fixturePath);

			Assert.That(output, Does.Contain("SpaceAge Battle Simulator v"));
			Assert.That(output, Does.Contain("Seed: 42"));
			Assert.That(output, Does.Contain("SIMULATION RESULT:"));
			Assert.That(output, Does.Contain("Round 1:"));
		}

		[Test]
		public void Run_SameSeedProducesIdenticalOutput()
		{
			BattleSimulatorRunner runner = new BattleSimulatorRunner(Directory.GetCurrentDirectory());
			string first = runner.Run(this.fixturePath);
			new Game().ClearDictionaries();
			Sequence.Reset();

			string second = runner.Run(this.fixturePath);

			Assert.That(second, Is.EqualTo(first));
		}

		[Test]
		public void Run_DoesNotModifyGameinXml()
		{
			string gameinPath = Path.Combine(Directory.GetCurrentDirectory(), "gamein.xml");
			DateTime before = File.GetLastWriteTimeUtc(gameinPath);
			long sizeBefore = new FileInfo(gameinPath).Length;

			BattleSimulatorRunner runner = new BattleSimulatorRunner(Directory.GetCurrentDirectory());
			runner.Run(this.fixturePath);

			Assert.That(File.GetLastWriteTimeUtc(gameinPath), Is.EqualTo(before));
			Assert.That(new FileInfo(gameinPath).Length, Is.EqualTo(sizeBefore));
		}

		[Test]
		public void SerializeTemplate_RoundTripsSimInputXml()
		{
			BattleSimTemplate template = BattleSimulatorTemplates.Get("infantry-battalion");
			string xml = BattleSimulatorTemplates.ToSimInputXml(template, seed: 99);

			Assert.That(xml, Does.Contain("<battle-sim seed=\"99\""));
			Assert.That(xml, Does.Contain("type=\"inftry\""));
			Assert.That(xml, Does.Contain("tactic=\"capture\""));

			BattleSimulatorRunner runner = new BattleSimulatorRunner(TCampaign.CampaignDir());
			string tempPath = Path.Combine(Path.GetTempPath(), "battle-sim-template-" + Guid.NewGuid() + ".xml");
			try
			{
				File.WriteAllText(tempPath, xml, Encoding.GetEncoding(1251));
				string output = runner.Run(tempPath);
				Assert.That(output, Does.Contain("Seed: 99"));
				Assert.That(output, Does.Contain("SIMULATION RESULT:"));
			}
			finally
			{
				if (File.Exists(tempPath))
				{
					File.Delete(tempPath);
				}
			}
		}

		[Test]
		public void SerializeTemplate_CampaignCorvetteTypesResolve()
		{
			BattleSimTemplate template = BattleSimulatorTemplates.Get("system-patrol-corvette");
			string xml = BattleSimulatorTemplates.ToSimInputXml(template, seed: 7, locationType: "orbit");

			Assert.That(xml, Does.Contain("type=\"corhul\""));
			Assert.That(xml, Does.Contain("type=\"pdltur\""));
			Assert.That(xml, Does.Contain("type=\"cermpl\""));

			BattleSimulatorRunner runner = new BattleSimulatorRunner(TCampaign.CampaignDir());
			string tempPath = Path.Combine(Path.GetTempPath(), "battle-sim-corvette-" + Guid.NewGuid() + ".xml");
			try
			{
				File.WriteAllText(tempPath, xml, Encoding.GetEncoding(1251));
				string output = runner.Run(tempPath);
				Assert.That(output, Does.Contain("Seed: 7"));
				Assert.That(output, Does.Contain("SIMULATION RESULT:"));
			}
			finally
			{
				if (File.Exists(tempPath))
				{
					File.Delete(tempPath);
				}
			}
		}
	}
}
