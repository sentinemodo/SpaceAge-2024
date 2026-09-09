using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMoveDuration : TTest
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
		public void MovementDuration_SameBodyRegionOrbit_IsOneWeek()
		{
			this.LoadSpaceMoveWorld();
			ModuleStack shuttle = ModuleStack.All["s00002"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00001");
			MoveOrder order = (MoveOrder)shuttle.Orders[0];

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.space));
			Assert.That(shuttle.Parent, Is.EqualTo(Orbit.All["O00001"]));
			Assert.That(shuttle.MovingTo, Is.Null);
			Assert.That(order.Executed, Is.True);
		}

		[Test]
		public void MovementDuration_PlanetOrbits_UsesDeltaAuAndDriveSpeed()
		{
			this.LoadSpaceMoveWorld();
			ModuleStack shuttle = ModuleStack.All["s00001"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00002");
			MoveOrder order = (MoveOrder)shuttle.Orders[0];

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			int expected = SpaceTransit.DurationWeeks(1.0, SpaceTransit.EffectiveSpaceSpeed(shuttle));
			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.space));
			Assert.That(shuttle.Parent, Is.EqualTo(Orbit.All["O00001"]));
			Assert.That(shuttle.MovingTo, Is.EqualTo(Orbit.All["O00002"]));
			Assert.That(order.DurationLeft, Is.EqualTo(expected - 1));
			Assert.That(expected, Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public void MovementDuration_FasterDrive_ShortensEta()
		{
			this.LoadSpaceMoveWorld();
			ModuleType.All["shuttl"].MoveModes[EMoveMode.space].Speed = 4;
			ModuleStack shuttle = ModuleStack.All["s00001"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "O00002");
			MoveOrder order = (MoveOrder)shuttle.Orders[0];

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			int chemical = SpaceTransit.DurationWeeks(1.0, 1 * SpaceTransit.MassFactor(SpaceTransit.SpaceThrust(shuttle), shuttle.Mass));
			int fast = SpaceTransit.DurationWeeks(1.0, SpaceTransit.EffectiveSpaceSpeed(shuttle));
			Assert.That(fast, Is.LessThan(chemical));
			Assert.That(order.DurationLeft, Is.EqualTo(fast - 1));
		}

		[Test]
		public void MovementDuration_PlanetToGateOrbit_ChemicalAbout13Weeks()
		{
			this.LoadSpaceMoveWorld();
			ModuleStack shuttle = ModuleStack.All["s00001"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "P00009");
			MoveOrder order = (MoveOrder)shuttle.Orders[0];

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			int expected = SpaceTransit.DurationWeeks(79, SpaceTransit.EffectiveSpaceSpeed(shuttle));
			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.space));
			Assert.That(shuttle.MovingTo, Is.EqualTo(Alderson.All["P00009"].Orbit));
			Assert.That(order.DurationLeft, Is.EqualTo(expected - 1));
			Assert.That(expected, Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public void MovementDuration_RegionToRegion_SpaceExitUsesExitDuration()
		{
			this.LoadSpaceMoveWorld();
			ModuleStack shuttle = ModuleStack.All["s00002"];
			this.Prefuel(shuttle);
			this.AssignMove(shuttle, "R00002");
			MoveOrder order = (MoveOrder)shuttle.Orders[0];

			shuttle.ExecutedLongOrder = false;
			shuttle.Orders.Execute(this.game.Week);

			int expected = SpaceTransit.ExitDurationWeeks(8, shuttle);
			Assert.That(order.MoveMode, Is.EqualTo(EMoveMode.space));
			Assert.That(shuttle.Parent, Is.EqualTo(Region.All["R00001"]));
			Assert.That(shuttle.MovingTo, Is.EqualTo(Region.All["R00002"]));
			Assert.That(order.DurationLeft, Is.EqualTo(expected - 1));
		}

		[Test]
		public void MovementDuration_HeavierStack_TakesLongerUnderSameDrive()
		{
			this.LoadSpaceMoveWorld();
			ModuleStack light = ModuleStack.All["s00001"];
			ModuleStack heavy = new ModuleStack(Orbit.All["O00001"], light.Owner, ModuleType.All["shuttl"], "s00009");
			heavy.AddModule();
			heavy.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 2));
			heavy.ItemStacks.Add(new ItemStack(ItemType.All["h2o2"], 20));
			heavy.ItemStacks.Add(new ItemStack(ItemType.All["uraniu"], 2));
			heavy.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 400));
			this.Prefuel(light);
			this.Prefuel(heavy);
			this.AssignMove(light, "O00002");
			this.AssignMove(heavy, "O00002");

			light.ExecutedLongOrder = false;
			heavy.ExecutedLongOrder = false;
			light.Orders.Execute(this.game.Week);
			heavy.Orders.Execute(this.game.Week);

			MoveOrder lightOrder = (MoveOrder)light.Orders[0];
			MoveOrder heavyOrder = (MoveOrder)heavy.Orders[0];
			Assert.That(heavy.Mass, Is.GreaterThan(light.Mass));
			Assert.That(heavyOrder.DurationLeft, Is.GreaterThan(lightOrder.DurationLeft));
		}

		[Test]
		public void DistanceTo_SameSystemPlanets_IsAbsDeltaAu()
		{
			this.LoadSpaceMoveWorld();
			Planet inner = Planet.All["P00001"];
			Planet outer = Planet.All["P00002"];
			Assert.That(inner.DistanceTo(outer), Is.EqualTo(1.0).Within(0.0000001));
			Assert.That(inner.DistanceTo(Orbit.All["O00002"]), Is.EqualTo(1.0).Within(0.0000001));
			Assert.That(inner.DistanceTo(Alderson.All["P00009"]), Is.EqualTo(79.0).Within(0.0000001));
		}

		private string SpaceMoveFixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "space-move");
		}

		private void LoadSpaceMoveWorld()
		{
			string fixtureDir = this.SpaceMoveFixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private void AssignMove(ModuleStack stack, string destination)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				"move " + destination,
				"#end"
			});
		}

		private void Prefuel(ModuleStack stack)
		{
			new Fuelled(stack, 13);
		}
	}
}
