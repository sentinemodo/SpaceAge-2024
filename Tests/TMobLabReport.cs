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
	[TestFixture]
	public class TMobLabReport : TTest
	{
		private const string FactoryId = "901001";
		private const string HeadquartersId = "901002";
		private const string CargoBayId = "901003";
		private const string PowerPlantId = "901004";
		private const string MobLabId = "901010";
		private const string OriginRegionId = "R00001";
		private const string DestinationRegionId = "R00002";

		[SetUp]
		public void setupMobLabReport()
		{
			Battle.All.Clear();
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardownMobLabReport()
		{
			this.game.Week = 1;
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
		}

		private sealed class BetaLikeMobLabScenario
		{
			public ModuleStack Factory { get; set; }
			public ModuleStack Headquarters { get; set; }
			public ModuleStack CargoBay { get; set; }
			public ModuleStack MobLab { get; set; }
			public Region Origin { get; set; }
			public Region Destination { get; set; }
		}

		private BetaLikeMobLabScenario CreateBetaLikeGrant(Faction owner)
		{
			Region origin = Region.All[OriginRegionId];
			Region destination = Region.All[DestinationRegionId];

			ModuleStack settlement = ModuleStack.All["000005"];

			ModuleStack power = ModuleStack.All.GetOrCreateNewModuleStack(owner, PowerPlantId);
			power.Parent = origin;
			power.ModuleType = ModuleType.All["wnplnt"];
			power.AddModule();
			power.AddModule();
			power.AddModule();
			power.AddModule();
			power.AddModule();

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

			return new BetaLikeMobLabScenario
			{
				Factory = factory,
				Headquarters = hq,
				CargoBay = cargo,
				Origin = origin,
				Destination = destination,
			};
		}

		private void AssignBetaLikeOrders(Game game, Faction owner)
		{
			List<string> commands = new List<string>
			{
				"#faction " + owner.Name,
				"#modulestack " + PowerPlantId,
				"@produce energy",
				"#modulestack " + FactoryId,
				"use msrvtm as " + MobLabId + " for " + HeadquartersId,
				"#modulestack " + MobLabId,
				"@move " + DestinationRegionId,
				"get 1 terran from " + HeadquartersId,
				"get 2 oil from " + CargoBayId,
				"get 2 food from " + CargoBayId,
				"#end",
			};

			OrdersReader reader = new OrdersReader(game);
			reader.AssignOrders(commands);
		}

		private static void RunWeek(Game game)
		{
			game.ClearExecutedLongOrder();
			game.ClearExecutedImmediateOrders();
			game.ExecuteOrders();
			game.ExecuteMaintenance();
		}

		private BetaLikeMobLabScenario RunBetaLikeMobLabQuarter(Faction owner)
		{
			BetaLikeMobLabScenario scenario = this.CreateBetaLikeGrant(owner);
			this.AssignBetaLikeOrders(this.game, owner);

			for (this.game.Week = 1; this.game.Week <= 13; this.game.Week++)
			{
				RunWeek(this.game);
			}

			scenario.MobLab = this.FindMobLabStack(owner);
			return scenario;
		}

		private ModuleStack FindMobLabStack(Faction owner)
		{
			ModuleStack stack = ModuleStack.All[MobLabId];
			if (stack != null && stack.Owner == owner)
			{
				return stack;
			}

			return ModuleStack.All[owner, MobLabId, true];
		}

		private static string DiagnoseMobLabReportState(ModuleStack moblab, Faction faction)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("moblab report diagnostics:");
			sb.AppendFormat("  location: {0}", moblab.Location == null ? "(null)" : moblab.Location.ReportName);
			sb.AppendLine();
			sb.AppendFormat("  parent: {0}", moblab.Parent == null ? "(null)" : moblab.Parent.ReportName);
			sb.AppendLine();
			sb.AppendFormat("  IsRootModuleStack: {0}", moblab.IsRootModuleStack);
			sb.AppendLine();
			sb.AppendFormat("  Online (stack): {0}", moblab.Online);
			sb.AppendLine();
			sb.AppendFormat("  IsActive: {0}", moblab.IsActive);
			sb.AppendLine();
			sb.AppendFormat("  IsImmobile: {0}", moblab.IsImmobile);
			sb.AppendLine();
			sb.AppendFormat("  HasOperationalModules: {0}", moblab.HasOperationalModules);
			sb.AppendLine();
			sb.AppendFormat("  IsPartiallyDisabled: {0}", moblab.IsPartiallyDisabled);
			sb.AppendLine();
			sb.AppendFormat("  QuantityActive: {0}, QuantityOperational: {1}", moblab.QuantityActive, moblab.QuantityOperational);
			sb.AppendLine();
			sb.AppendFormat("  CrewRequired: {0}, CrewCurrent: {1}", moblab.CrewRequired, moblab.CrewCurrent);
			sb.AppendLine();
			sb.AppendFormat("  Effects.IsFuelled: {0}, NeedFuel(ground): {1}",
				moblab.Effects.IsFuelled,
				moblab.MoveModes.ContainsKey(EMoveMode.ground)
					? moblab.NeedFuel(moblab.MoveModes[EMoveMode.ground])
					: moblab.NeedFuel(null));
			sb.AppendLine();
			sb.AppendFormat("  ReportHeader: {0}", moblab.ReportHeader(faction));
			sb.AppendLine();
			if (moblab.EventReports.Count > 0)
			{
				sb.AppendLine("  events:");
				foreach (EventReport eventReport in moblab.EventReports.OrderBy(e => e.Week))
				{
					sb.AppendFormat("    week {0}: {1}", eventReport.Week, eventReport.Description);
					sb.AppendLine();
				}
			}

			for (int i = 0; i < moblab.Modules.Count; i++)
			{
				Module module = moblab.Modules[i];
				sb.AppendFormat("  module #{0}: Activated={1}, Online={2}, Damage={3}/{4}, IsActive={5}, ReportActive={6}",
					i + 1,
					module.Activated,
					module.Online,
					module.Damage,
					module.HitPoints,
					module.IsActive,
					module.ReportActive);
				sb.AppendLine();
			}

			return sb.ToString();
		}

		[Test]
		public void MobLab_BetaLikeFactoryBuildStagingAndMove_EndOfQuarterReportState()
		{
			Faction faction = this.game.Factions["2"];
			BetaLikeMobLabScenario scenario = this.RunBetaLikeMobLabQuarter(faction);
			ModuleStack moblab = scenario.MobLab;

			Assert.That(moblab, Is.Not.Null, "moblab stack should exist after USE msrvtm");
			Assert.That(moblab.IsFormed, Is.True);
			Assert.That(moblab.ModuleType.Name, Is.EqualTo("moblab"));

			string diagnostics = DiagnoseMobLabReportState(moblab, faction);
			string header = moblab.ReportHeader(faction);

			Assert.That(
				moblab.EventReports.Any(e => e.Week == 1 && e.Description.Contains("GET failed") && e.Description.Contains("capacity")),
				Is.True,
				"week 1 staging onto empty forming stack should fail like beta\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("formed by")),
				Is.True,
				"factory USE msrvtm should start moblab production\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Week == 2 && e.Description.Contains("received") && e.Description.Contains("produced by")),
				Is.True,
				"msrvtm use-time 2 completes on week 2 like beta\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("stacked under")),
				Is.True,
				"USE ... FOR HQ should nest moblab under headquarters like beta turn\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("got terran")),
				Is.True,
				"crew terran should stage from HQ after production\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("got 2 units of oil") && e.Description.Contains(CargoBayId)),
				Is.True,
				"fuel oil should stage from cargo bay like beta turn 2\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("MOVE failed") && e.Description.Contains("disabled")),
				Is.True,
				"move should fail at least once while the stack is still disabled\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("consumed") && e.Description.Contains("oil") && e.Description.Contains("fuel")),
				Is.True,
				"oil fuel should be consumed before travel resumes\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Description.Contains("arrived at")),
				Is.True,
				"move between regions should complete within the quarter\n" + diagnostics);
			Assert.That(moblab.Parent, Is.EqualTo(scenario.Destination), diagnostics);
			Assert.That(moblab.Effects.IsFuelled, Is.True, diagnostics);
			Assert.That(moblab.CrewCurrent, Is.GreaterThanOrEqualTo(1), diagnostics);
		}

		[Test]
		public void MobLab_BetaLikeTurn_EarlyMoveFailsWhileDisabledBeforeFuelConsumed()
		{
			Faction faction = this.game.Factions["2"];
			this.CreateBetaLikeGrant(faction);
			this.AssignBetaLikeOrders(this.game, faction);

			for (this.game.Week = 1; this.game.Week <= 5; this.game.Week++)
			{
				RunWeek(this.game);
			}

			ModuleStack moblab = this.FindMobLabStack(faction);
			Assert.That(moblab, Is.Not.Null);

			string diagnostics = DiagnoseMobLabReportState(moblab, faction);
			Assert.That(
				moblab.EventReports.Any(e => e.Week == 1 && e.Description.Contains("MOVE failed") && e.Description.Contains("disabled")),
				Is.True,
				"week 1 move onto forming stack should fail while disabled\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Week == 2 && e.Description.Contains("got terran")),
				Is.True,
				"week 2 staging from HQ after msrvtm completes\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Week == 3 && e.Description.Contains("consumed") && e.Description.Contains("oil") && e.Description.Contains("fuel")),
				Is.True,
				"fuel oil should be consumed once the stack can move\n" + diagnostics);
			Assert.That(
				moblab.EventReports.Any(e => e.Week >= 4 && e.Week <= 5 && e.Description.Contains("moving from")),
				Is.True,
				"ground move toward destination should start after fuel\n" + diagnostics);
		}

		[Test]
		public void MobLab_FuelledEffectDoesNotImplyIsActive_WhenModuleCopiesDeactivated()
		{
			Faction faction = this.game.Factions["2"];
			Region region = Region.All[OriginRegionId];
			ModuleStack moblab = ModuleStack.All.GetOrCreateNewModuleStack(faction, "901099");
			moblab.Parent = region;
			moblab.ModuleType = ModuleType.All["moblab"];
			moblab.AddModule();
			moblab.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 1));
			new Fuelled(moblab, 7);
			foreach (Module module in moblab.Modules)
			{
				module.Activated = false;
			}

			string diagnostics = DiagnoseMobLabReportState(moblab, faction);
			string header = moblab.ReportHeader(faction);

			Assert.That(moblab.Effects.IsFuelled, Is.True, diagnostics);
			Assert.That(moblab.NeedFuel(moblab.MoveModes[EMoveMode.ground]), Is.False, diagnostics);
			Assert.That(moblab.IsActive, Is.False, "Fuelled does not activate deactivated module copies\n" + diagnostics);
			Assert.That(moblab.QuantityActive, Is.EqualTo(0), diagnostics);
			Assert.That(header, Does.Contain("disabled"), diagnostics);
		}
	}
}
