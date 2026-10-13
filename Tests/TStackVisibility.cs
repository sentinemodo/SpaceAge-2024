using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TStackVisibility
	{
		private SpaceSystem system;
		private Planet planet;
		private Orbit orbit;
		private Region regionA;
		private Region regionB;
		private Faction owner;
		private Faction observer;
		private ModuleType scoutType;
		private ModuleType garrisonType;

		[SetUp]
		public void setup()
		{
			this.system = new SpaceSystem("visys");
			this.planet = new Planet(this.system, "vispln");
			this.orbit = new Orbit(this.planet, "visorb");
			this.regionA = new Region(this.planet, "visA");
			this.regionB = new Region(this.planet, "visB");
			this.regionA.Exits.Add(new Exit { To = this.regionB });
			this.regionB.Exits.Add(new Exit { To = this.regionA });

			this.owner = new Faction("20", "Owner");
			this.observer = new Faction("21", "Observer");

			this.scoutType = new ModuleType("scout");
			this.scoutType.Size = 10;
			this.scoutType.Detection = 1;

			this.garrisonType = new ModuleType("inftry");
			this.garrisonType.Size = 10;
		}

		[TearDown]
		public void teardown()
		{
			ModuleStack.All.Clear();
			ModuleType.All.Clear();
			Region.All.Clear();
			Planet.All.Clear();
			Orbit.All.Clear();
			Belt.All.Clear();
			Faction.All.Clear();
		}

		private ModuleStack form(IHolder parent, Faction faction, ModuleType type, string name)
		{
			ModuleStack stack = new ModuleStack(parent, faction, type, name);
			stack.AddModule();
			return stack;
		}

		[Test]
		public void SameRegion_RootVisible_NestedHidden()
		{
			ModuleStack town = this.form(this.regionA, this.owner, this.scoutType, "town1");
			ModuleStack nested = this.form(this.regionA, this.owner, this.garrisonType, "gar1");
			nested.Parent = town;
			this.form(this.regionA, this.observer, this.scoutType, "obs1");

			Assert.That(town.Visible(this.observer), Is.True);
			Assert.That(nested.Visible(this.observer), Is.False);
		}

		[Test]
		public void AdjacentRegion_NotVisibleWithDetectionOne()
		{
			ModuleStack foreign = this.form(this.regionB, this.owner, this.scoutType, "for1");
			this.form(this.regionA, this.observer, this.scoutType, "obs2");

			Assert.That(foreign.Visible(this.observer), Is.False);
		}

		[Test]
		public void OrbitObserver_SeesRootsOnPlanet_NotUnderwater()
		{
			ModuleType trucks = new ModuleType("trucks");
			trucks.Size = 10;
			trucks.Detection = 1;

			ModuleType uscty = new ModuleType("uscty");
			uscty.Size = 10;
			uscty.Underwater = true;

			ModuleStack surface = this.form(this.regionB, this.owner, trucks, "surf1");
			ModuleStack underwater = this.form(this.regionB, this.owner, uscty, "sub1");
			this.form(this.orbit, this.observer, trucks, "orb1");

			Assert.That(surface.Visible(this.observer), Is.True);
			Assert.That(underwater.Visible(this.observer), Is.False);
		}

		[Test]
		public void SameOrbit_RootVisible_NestedHidden()
		{
			ModuleStack hull = this.form(this.orbit, this.owner, this.scoutType, "hull1");
			ModuleStack nested = this.form(this.orbit, this.owner, this.garrisonType, "bridge1");
			nested.Parent = hull;
			this.form(this.orbit, this.observer, this.scoutType, "shut1");

			Assert.That(hull.Visible(this.observer), Is.True);
			Assert.That(nested.Visible(this.observer), Is.False);
		}

		[Test]
		public void OrbitObserver_DoesNotSeeBeltUnit()
		{
			Belt belt = new Belt(this.system, this.planet, "visbl");
			ModuleStack asteroid = this.form(belt, this.owner, this.scoutType, "ast1");
			this.form(this.orbit, this.observer, this.scoutType, "orb2");

			Assert.That(asteroid.Visible(this.observer), Is.False);
		}

		[Test]
		public void SeeOrder_Anywhere_DetectsAcrossRegionsFromOrbit()
		{
			ModuleStack foreign = this.form(this.regionB, this.owner, this.scoutType, "for2");
			ModuleStack observer = this.form(this.orbit, this.observer, this.scoutType, "obs3");

			SeeOrder order = new SeeOrder(observer);
			order.Parse("for2 anywhere");
			order.Execute(1);

			Assert.That(order.Executed, Is.True);
		}

		[Test]
		public void NestedObserver_RevealsForeignSettlementRoot_NotNestedGarrison()
		{
			ModuleType city = new ModuleType("city");
			city.Size = 1000;
			city.Detection = 0;

			ModuleStack town = this.form(this.regionA, this.owner, city, "town1");
			ModuleStack garrison = this.form(this.regionA, this.owner, this.garrisonType, "gar1");
			garrison.Parent = town;

			ModuleStack nestedScout = this.form(this.regionA, this.observer, this.scoutType, "obs5");
			nestedScout.Parent = town;

			Assert.That(town.Visible(this.observer), Is.True);
			Assert.That(garrison.Visible(this.observer), Is.False);
			Assert.That(nestedScout.Visible(this.observer), Is.True);
		}

		[Test]
		public void OrbitStack_RevealsSurfaceAndSeaRegions_NotUnderground()
		{
			RegionType grass = new RegionType("grassl");
			grass.LocationType = ELocationType.solidSurface;
			RegionType sea = new RegionType("sea");
			sea.LocationType = ELocationType.liquidSurface;
			RegionType buriedType = new RegionType("underg");
			buriedType.LocationType = ELocationType.underground;
			this.regionA.RegionType = grass;
			this.regionB.RegionType = sea;
			Region buried = new Region(this.planet, "visU");
			buried.RegionType = buriedType;

			this.form(this.orbit, this.observer, this.scoutType, "shut2");

			Assert.That(this.regionA.Visible(this.observer), Is.True);
			Assert.That(this.regionB.Visible(this.observer), Is.True);
			Assert.That(buried.Visible(this.observer), Is.False);
		}

		[Test]
		public void Region_HiddenWithoutLocalOrOrbitalStack()
		{
			Assert.That(this.regionB.Visible(this.observer), Is.False);
		}

		[Test]
		public void UndergroundRegion_VisibleWhenFactionHasStackThere()
		{
			RegionType buriedType = new RegionType("underg");
			buriedType.LocationType = ELocationType.underground;
			Region buried = new Region(this.planet, "visU2");
			buried.RegionType = buriedType;
			this.form(buried, this.observer, this.scoutType, "dig1");

			Assert.That(buried.Visible(this.observer), Is.True);
		}

		[Test]
		public void SeeOrder_CurrentRegion_FailsAcrossRegions()
		{
			ModuleStack foreign = this.form(this.regionB, this.owner, this.scoutType, "for3");
			ModuleStack observer = this.form(this.regionA, this.observer, this.scoutType, "obs4");

			SeeOrder order = new SeeOrder(observer);
			order.Parse("for3");
			order.Execute(1);

			Assert.That(order.Executed, Is.False);
		}
	}
}
