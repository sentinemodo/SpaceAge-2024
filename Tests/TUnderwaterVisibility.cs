using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TUnderwaterVisibility
	{
		private SpaceSystem system;
		private Planet planet;
		private Region ocean;
		private Region land;
		private Faction owner;
		private Faction observer;
		private ModuleType usctyType;
		private ModuleType trucksType;
		private ModuleType uwtrukType;
		private ModuleType dmdctyType;
		private ModuleType hullType;

		[SetUp]
		public void setup()
		{
			this.system = new SpaceSystem("uwsys");
			this.planet = new Planet(this.system, "uwplnt");
			this.planet.TemperatureBand = ETemperatureBand.habitable;
			new Orbit(this.planet, "uworb");
			RegionType seaType = new RegionType("sea");
			seaType.LocationType = ELocationType.liquidSurface;
			RegionType grassType = new RegionType("grassl");
			grassType.LocationType = ELocationType.solidSurface;
			this.ocean = new Region(this.planet, "uwocea");
			this.ocean.RegionType = seaType;
			this.land = new Region(this.planet, "uwland");
			this.land.RegionType = grassType;
			this.owner = new Faction("10", "OwnerCorp");
			this.observer = new Faction("11", "ObserverCorp");

			this.usctyType = new ModuleType("uscty");
			this.usctyType.Size = 100;
			this.usctyType.Underwater = true;
			this.usctyType.Group = EModuleTypesGroup.settlement;

			this.uwtrukType = new ModuleType("uwtruk");
			this.uwtrukType.Size = 50;
			this.uwtrukType.Underwater = true;
			this.uwtrukType.Group = EModuleTypesGroup.vehicle;

			this.trucksType = new ModuleType("trucks");
			this.trucksType.Size = 50;
			this.trucksType.Underwater = false;
			this.trucksType.Group = EModuleTypesGroup.vehicle;

			this.dmdctyType = new ModuleType("dmdcty");
			this.dmdctyType.Size = 100;
			this.dmdctyType.Underwater = false;
			this.dmdctyType.Group = EModuleTypesGroup.settlement;
			this.dmdctyType.OperationCondition_LocationTypes.Add(ELocationType.solidSurface);
			this.dmdctyType.OperationCondition_LocationTypes.Add(ELocationType.liquidSurface);

			this.hullType = new ModuleType("corhul");
			this.hullType.Size = 200;
			this.hullType.Group = EModuleTypesGroup.corvette;
		}

		[TearDown]
		public void teardown()
		{
			ModuleStack.All.Clear();
			ModuleType.All.Clear();
			Technology.All.Clear();
			Person.All.Clear();
			Region.All.Clear();
			Planet.All.Clear();
			Orbit.All.Clear();
			Faction.All.Clear();
		}

		private ModuleStack form(IHolder parent, Faction faction, ModuleType type, string name)
		{
			ModuleStack stack = new ModuleStack(parent, faction, type, name);
			stack.AddModule();
			return stack;
		}

		[Test]
		public void UnderwaterCity_HiddenFromSurfacePresenceOnly()
		{
			ModuleStack city = this.form(this.ocean, this.owner, this.usctyType, "usc001");
			this.form(this.ocean, this.observer, this.trucksType, "trk001");

			Assert.That(city.IsUnderwaterStealthy, Is.True);
			Assert.That(city.Visible(this.observer), Is.False);
		}

		[Test]
		public void UnderwaterCity_VisibleWhenObserverHasUnderwaterUnitInRegion()
		{
			ModuleStack city = this.form(this.ocean, this.owner, this.usctyType, "usc002");
			this.form(this.ocean, this.observer, this.uwtrukType, "uwt001");

			Assert.That(city.Visible(this.observer), Is.True);
		}

		[Test]
		public void UnderwaterCity_VisibleWhenObserverHasSpaceshipOnOrbit()
		{
			ModuleStack city = this.form(this.ocean, this.owner, this.usctyType, "usc003");
			this.form(this.ocean, this.observer, this.trucksType, "trk002");
			this.form(this.planet.Orbit, this.observer, this.hullType, "shp001");

			Assert.That(city.Visible(this.observer), Is.True);
		}

		[Test]
		public void UnderwaterCity_AlwaysVisibleToOwner()
		{
			ModuleStack city = this.form(this.ocean, this.owner, this.usctyType, "usc004");
			Assert.That(city.Visible(this.owner), Is.True);
		}

		[Test]
		public void SurfaceStack_VisibleWithAnyPresence()
		{
			ModuleStack trucks = this.form(this.ocean, this.owner, this.trucksType, "trk003");
			this.form(this.ocean, this.observer, this.trucksType, "trk004");

			Assert.That(trucks.IsUnderwaterStealthy, Is.False);
			Assert.That(trucks.Visible(this.observer), Is.True);
		}

		[Test]
		public void DomeCity_StealthyOnlyOnLiquidSurface()
		{
			ModuleStack domeSea = this.form(this.ocean, this.owner, this.dmdctyType, "dmd001");
			ModuleStack domeLand = this.form(this.land, this.owner, this.dmdctyType, "dmd002");
			this.form(this.ocean, this.observer, this.trucksType, "trk005");
			this.form(this.land, this.observer, this.trucksType, "trk006");

			Assert.That(domeSea.IsUnderwaterStealthy, Is.True);
			Assert.That(domeSea.Visible(this.observer), Is.False);
			Assert.That(domeLand.IsUnderwaterStealthy, Is.False);
			Assert.That(domeLand.Visible(this.observer), Is.True);
		}
		[Test]
		public void UseUscty_FailsWithoutUnderwaterTenderInSeatRegion()
		{
			ModuleType factry = new ModuleType("factry");
			factry.Size = 100;
			factry.Group = EModuleTypesGroup.production;

			Technology usctyc = new Technology("usctyc");
			usctyc.UseProduceModules = this.usctyType;
			usctyc.UseCondition_ModuleTypesGroup = EModuleTypesGroup.production;
			usctyc.UseTime = 1;

			ModuleStack factory = this.form(this.ocean, this.owner, factry, "fac001");
			factory.Technologies.Add(usctyc);

			UseOrder order = new UseOrder(factory);
			order.Technology = usctyc;
			order.ReceiverParent = factory;

			Assert.That(order.Usable(1), Is.False);
		}

		[Test]
		public void UseUscty_SucceedsWithUnderwaterTenderInSeatRegion()
		{
			ModuleType factry = new ModuleType("factry");
			factry.Size = 100;
			factry.Group = EModuleTypesGroup.production;

			Technology usctyc = new Technology("usctyc");
			usctyc.UseProduceModules = this.usctyType;
			usctyc.UseCondition_ModuleTypesGroup = EModuleTypesGroup.production;
			usctyc.UseTime = 1;

			ModuleStack factory = this.form(this.ocean, this.owner, factry, "fac002");
			factory.Technologies.Add(usctyc);
			this.form(this.ocean, this.owner, this.uwtrukType, "uwt002");

			UseOrder order = new UseOrder(factory);
			order.Technology = usctyc;
			order.ReceiverParent = factory;

			Assert.That(order.Usable(1), Is.True);
		}
	}
}
