using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TTurnGmLog : TTest
	{
		[SetUp]
		public void setupTurnGmLog()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardownTurnGmLog()
		{
			TurnGmLog.Clear();
			this.ClearGame();
		}

		[Test]
		public void ParseChannels_AllAndExplicitTokens()
		{
			ETurnGmLogChannel channels = TurnGmLog.ParseChannels(new[] { "all" });
			Assert.That((channels & ETurnGmLogChannel.Research) != 0, Is.True);
			Assert.That((channels & ETurnGmLogChannel.Battles) != 0, Is.True);
			Assert.That((channels & ETurnGmLogChannel.Market) != 0, Is.True);

			channels = TurnGmLog.ParseChannels(new[] { "research,battles" });
			Assert.That((channels & ETurnGmLogChannel.Research) != 0, Is.True);
			Assert.That((channels & ETurnGmLogChannel.Battles) != 0, Is.True);
			Assert.That((channels & ETurnGmLogChannel.Market) != 0, Is.False);
		}

		[Test]
		public void ResearchBreakthrough_WritesGmLogFile()
		{
			TurnGmLog.Begin(ETurnGmLogChannel.Research);
			ModuleStack lab = ModuleStack.All.GetOrCreateNewModuleStack(this.game.Factions["2"], "100000");
			lab.Parent = ModuleStack.All["000005"];
			lab.ModuleType = ModuleType.All["cmplib"];
			lab.AddModule();
			lab.ResearchPoints = 7;

			List<string> commands = new List<string>
			{
				"#faction 2",
				"#modulestack " + lab.Name,
				"research",
				"#end",
			};
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(commands);
			ResearchOrder order = (ResearchOrder)lab.Orders[0];

			Sequence.Ints.Push(1);
			Sequence.Ints.Push(0);
			order.Execute(this.game.Week);

			string turnDir = Path.Combine(Path.GetTempPath(), "gmturn-log-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(turnDir);
			try
			{
				TurnGmLog.Current.WriteFile(turnDir, 2);
				string path = Path.Combine(turnDir, "gmturn-log.2.txt");
				Assert.That(File.Exists(path), Is.True);
				string text = File.ReadAllText(path);
				Assert.That(text, Does.Contain("research breakthrough"));
				Assert.That(text, Does.Contain("lab_stack_rp=7"));
				Assert.That(text, Does.Contain("selected="));
			}
			finally
			{
				Directory.Delete(turnDir, true);
			}
		}

		[Test]
		public void BattleCreated_WritesParticipationLines()
		{
			TurnGmLog.Begin(ETurnGmLogChannel.Battles);
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];
			Battle battle = new Battle(frigate, station);
			battle.Week = this.game.Week;

			string turnDir = Path.Combine(Path.GetTempPath(), "gmturn-log-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(turnDir);
			try
			{
				TurnGmLog.Current.WriteFile(turnDir, 1);
				string text = File.ReadAllText(Path.Combine(turnDir, "gmturn-log.1.txt"));
				Assert.That(text, Does.Contain("battle created"));
				Assert.That(text, Does.Contain("join attacker"));
				Assert.That(text, Does.Contain("join defender"));
			}
			finally
			{
				Directory.Delete(turnDir, true);
			}
		}

		[Test]
		public void DefaultOff_NoCurrentLog()
		{
			Assert.That(TurnGmLog.Current, Is.Null);
		}
	}
}
