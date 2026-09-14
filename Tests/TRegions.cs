using System;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TRegions : TTest
	{
		[SetUp]
		public void setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void TryGroundDistance_SameRegion_ReturnsZero()
		{
			Region region = Region.All["R00001"];
			int distance;
			Assert.That(Region.All.TryGroundDistance(region, region, out distance), Is.True);
			Assert.That(distance, Is.EqualTo(0));
		}

		[Test]
		public void TryGroundDistance_NeighbouringRegions_ReturnsExitDuration()
		{
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];
			int distance;
			Assert.That(Region.All.TryGroundDistance(region1, region2, out distance), Is.True);
			Assert.That(distance, Is.EqualTo(3));
		}

		[Test]
		public void TryGroundDistance_ThreeRegionChain_ReturnsShortestPathSum()
		{
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];
			Region region3 = new Region(Planet.All["P00002"], "R90001");
			Exit exit = new Exit();
			exit.To = region3;
			exit.ExitModes.Add(EMoveMode.ground, new ExitMode { Duration = 2 });
			region2.Exits.Add(exit);
			Exit returnExit = new Exit();
			returnExit.To = region2;
			returnExit.ExitModes.Add(EMoveMode.ground, new ExitMode { Duration = 2 });
			region3.Exits.Add(returnExit);

			int distance;
			Assert.That(Region.All.TryGroundDistance(region1, region3, out distance), Is.True);
			Assert.That(distance, Is.EqualTo(5));
		}

		[Test]
		public void TryGroundDistance_NoGroundPath_ReturnsFalse()
		{
			Region region1 = Region.All["R00001"];
			Region isolated = new Region(Planet.All["P00002"], "R90002");
			int distance;
			Assert.That(Region.All.TryGroundDistance(region1, isolated, out distance), Is.False);
		}

		[Test]
		public void DistanceBetween_NeighbouringRegions_MatchesTryGroundDistance()
		{
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];
			Assert.That(Region.All.DistanceBetween(region1, region2), Is.EqualTo(3));
		}
	}
}
