using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBattle
	{
		private DataFile datafile;
		private Game game;

		public TBattle()
		{			
		}
			
		[SetUp]
		public void setupReport()
		{
			this.datafile = new DataFile(Directory.GetCurrentDirectory());
			this.datafile.LoadConfiguration();
			this.datafile.LoadGame();
			this.game = this.datafile.Game;
		}

		[TearDown]
		public void teardownReport()
		{
			this.game.Week = 1;
			this.game.ClearDictionaries();
			this.game = null;
			this.datafile = null;			
		}

		[Test]
		public void SetupTeardown()
		{
			ClassicAssert.IsTrue(true);
		}


		[Test]
		public void FindAllies_singleModulestack()
		{
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];
			Battle battle = new Battle(frigate, station);
			ClassicAssert.AreEqual(1, battle.Attackers.Count);
			ClassicAssert.IsTrue(battle.Attackers.Contains("100011"));
			ClassicAssert.AreEqual(1, battle.Defenders.Count);
			ClassicAssert.IsTrue(battle.Defenders.Contains("100021"));
		}


	}
}
