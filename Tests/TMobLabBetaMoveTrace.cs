using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;
using UnitTests;

namespace IntegrationTests
{
	/// <summary>
	/// Mirrors play/runs beta-1 faction 11 moblab block: USE msrvtm FOR HQ, then move with +get staging.
	/// Uses Game.ExecuteOrders per week (no mid-quarter maintenance) to match live turns.
	/// </summary>
	[TestFixture]
	public class TMobLabBetaMoveTrace : TTest
	{
		private const string FixtureDir = "fixtures/moblab-beta-move";
		private const string FactoryId = "901001";
		private const string HeadquartersId = "901002";
		private const string CargoBayId = "901003";
		private const string PowerPlantId = "901004";
		private const string MobLabId = "901010";
		private const string OriginRegionId = "R00001";
		private const string DestinationRegionId = "R00002";

		[SetUp]
		public void Setup()
		{
			Battle.All.Clear();
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void Teardown()
		{
			this.game.Week = 1;
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
		}

		private void BuildGrant(Faction owner)
		{
			Region origin = Region.All[OriginRegionId];
			ModuleStack settlement = ModuleStack.All["000005"];

			ModuleStack power = ModuleStack.All.GetOrCreateNewModuleStack(owner, PowerPlantId);
			power.Parent = origin;
			power.ModuleType = ModuleType.All["wnplnt"];
			for (int i = 0; i < 5; i++)
			{
				power.AddModule();
			}

			ModuleStack factory = ModuleStack.All.GetOrCreateNewModuleStack(owner, FactoryId);
			factory.Parent = settlement;
			factory.ModuleType = ModuleType.All["factry"];
			factory.AddModule();
			factory.Technologies.Add(Technology.All["msrvtm"]);
			factory.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 10));
			factory.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 20));
			factory.ItemStacks.Add(new ItemStack(ItemType.All["silici"], 10));

			ModuleStack hq = ModuleStack.All.GetOrCreateNewModuleStack(owner, HeadquartersId);
			hq.Parent = origin;
			hq.ModuleType = ModuleType.All["corphq"];
			hq.AddModule();
			hq.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 20));

			ModuleStack cargo = ModuleStack.All.GetOrCreateNewModuleStack(owner, CargoBayId);
			cargo.Parent = origin;
			cargo.ModuleType = ModuleType.All["cargob"];
			cargo.AddModule();
			cargo.ItemStacks.Add(new ItemStack(ItemType.All["food"], 50));
			cargo.ItemStacks.Add(new ItemStack(ItemType.All["oil"], 10));

			ModuleStack.All.GetOrCreateNewModuleStack(owner, MobLabId);
		}

		private void LoadFixtureOrders()
		{
			string path = Path.Combine(Directory.GetCurrentDirectory(), FixtureDir, "orders.txt");
			List<string> lines = new List<string>();
			foreach (string raw in File.ReadAllLines(path))
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith(";"))
				{
					continue;
				}
				lines.Add(line);
			}
			new OrdersReader(this.game).AssignOrders(lines);
		}

		private static void RunTurnWeek(Game game)
		{
			game.ClearExecutedLongOrder();
			game.ClearExecutedImmediateOrders();
			game.ExecuteOrders();
			game.ExecuteMedicalConsume();
		}

		private static void RunTurnWeekWithMaintenance(Game game)
		{
			RunTurnWeek(game);
			foreach (ModuleStack stack in game.ModuleStacks.Values)
			{
				stack.ExecuteMaintenance(game.Week);
			}
		}

		private sealed class WeekSnapshot
		{
			public int Week;
			public bool IsActive;
			public bool HasOperationalModules;
			public int Quantity;
			public int QuantityActive;
			public int QuantityOperational;
			public int CrewCurrent;
			public int CrewRequired;
			public bool IsFuelled;
			public bool NeedFuelGround;
			public bool IsInTransit;
			public bool ExecutedLongOrder;
			public bool IsRootModuleStack;
			public string ParentLabel;
			public List<string> WeekEvents = new List<string>();
			public string ModuleDiag;
		}

		private static WeekSnapshot Capture(ModuleStack moblab, int week)
		{
			MoveMode ground = moblab.MoveModes.ContainsKey(EMoveMode.ground)
				? moblab.MoveModes[EMoveMode.ground]
				: null;
			StringBuilder moduleDiag = new StringBuilder();
			for (int i = 0; i < moblab.Modules.Count; i++)
			{
				Module m = moblab.Modules[i];
				moduleDiag.AppendFormat(
					"#{0} Activated={1} Online={2} Dmg={3}/{4} IsActive={5}; ",
					i + 1,
					m.Activated,
					m.Online,
					m.Damage,
					m.HitPoints,
					m.IsActive);
			}

			return new WeekSnapshot
			{
				Week = week,
				IsActive = moblab.IsActive,
				HasOperationalModules = moblab.HasOperationalModules,
				Quantity = moblab.Quantity,
				QuantityActive = moblab.QuantityActive,
				QuantityOperational = moblab.QuantityOperational,
				CrewCurrent = moblab.CrewCurrent,
				CrewRequired = moblab.CrewRequired,
				IsFuelled = moblab.Effects.IsFuelled,
				NeedFuelGround = ground != null && moblab.NeedFuel(ground),
				IsInTransit = moblab.IsInTransit,
				ExecutedLongOrder = moblab.ExecutedLongOrder,
				IsRootModuleStack = moblab.IsRootModuleStack,
				ParentLabel = moblab.Parent == null ? "(null)" : moblab.Parent.ReportName,
				WeekEvents = moblab.EventReports
					.Where(e => e.Week == week)
					.Select(e => e.Description)
					.ToList(),
				ModuleDiag = moduleDiag.ToString(),
			};
		}

		private static string FormatTimeline(IList<WeekSnapshot> timeline)
		{
			StringBuilder sb = new StringBuilder();
			foreach (WeekSnapshot s in timeline)
			{
				sb.AppendFormat(
					"week {0}: Q={1} QAct={2} QOp={3} crew={4}/{5} IsActive={6} HasOp={7} fuelled={8} needFuel={9} inTransit={10} execLong={11} root={12} parent={13}",
					s.Week,
					s.Quantity,
					s.QuantityActive,
					s.QuantityOperational,
					s.CrewCurrent,
					s.CrewRequired,
					s.IsActive,
					s.HasOperationalModules,
					s.IsFuelled,
					s.NeedFuelGround,
					s.IsInTransit,
					s.ExecutedLongOrder,
					s.IsRootModuleStack,
					s.ParentLabel);
				sb.AppendLine();
				if (!string.IsNullOrEmpty(s.ModuleDiag))
				{
					sb.AppendFormat("  modules: {0}", s.ModuleDiag);
					sb.AppendLine();
				}
				foreach (string ev in s.WeekEvents)
				{
					sb.AppendFormat("  event: {0}", ev);
					sb.AppendLine();
				}
			}
			return sb.ToString();
		}

		private ModuleStack FindMobLab(Faction owner)
		{
			ModuleStack stack = ModuleStack.All[MobLabId];
			if (stack != null && stack.Owner == owner)
			{
				return stack;
			}
			return ModuleStack.All[owner, MobLabId, true];
		}

		[Test]
		public void MobLab_BetaPlusGetOrders_TurnLoopTrace_DocumentsMoveAndFuelOrdering()
		{
			Faction faction = this.game.Factions["2"];
			this.BuildGrant(faction);
			this.LoadFixtureOrders();

			List<WeekSnapshot> timeline = new List<WeekSnapshot>();
			for (this.game.Week = 1; this.game.Week <= 8; this.game.Week++)
			{
				RunTurnWeek(this.game);
				ModuleStack moblab = this.FindMobLab(faction);
				Assert.That(moblab, Is.Not.Null, "week " + this.game.Week);
				timeline.Add(Capture(moblab, this.game.Week));
			}

			string trace = FormatTimeline(timeline);
			TestContext.WriteLine(trace);
			ModuleStack finalLab = this.FindMobLab(faction);

			Assert.That(
				finalLab.EventReports.Any(e => e.Week == 2 && e.Description.Contains("got terran")),
				Is.True,
				"+get staging after production (week 2)\n" + trace);
			Assert.That(
				timeline.First(s => s.Week == 2).IsActive,
				Is.True,
				"module copy active once formed and crew staged\n" + trace);
			Assert.That(
				finalLab.EventReports.Any(e => e.Week == 3 && e.Description.Contains("consumed") && e.Description.Contains("fuel")),
				Is.True,
				"first move week burns oil after needFuel runs before canMove\n" + trace);
			Assert.That(
				finalLab.EventReports.Any(e => e.Week == 3 && (e.Description.Contains("departed from") || e.Description.Contains("moving from"))),
				Is.True,
				"movement starts week 3 after ExecutedLongOrder clears\n" + trace);
			Assert.That(
				finalLab.Parent, Is.SameAs(Region.All[DestinationRegionId]),
				"arrives within quarter\n" + trace);
		}

		[Test]
		public void MobLab_BetaPlusGetOrders_WeeklyMaintenance_DocumentsTimelineDifference()
		{
			Faction faction = this.game.Factions["2"];
			this.BuildGrant(faction);
			this.LoadFixtureOrders();

			List<WeekSnapshot> timeline = new List<WeekSnapshot>();
			for (this.game.Week = 1; this.game.Week <= 8; this.game.Week++)
			{
				RunTurnWeekWithMaintenance(this.game);
				timeline.Add(Capture(this.FindMobLab(faction), this.game.Week));
			}

			string trace = FormatTimeline(timeline);
			TestContext.WriteLine(trace);
			Assert.That(timeline.Any(s => s.WeekEvents.Any(ev => ev.Contains("got terran"))), Is.True, trace);
		}
	}

}


