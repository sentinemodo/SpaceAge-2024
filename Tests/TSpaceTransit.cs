using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TSpaceTransit
	{
		[Test]
		public void DurationWeeks_MoonScaleDelta_IsOneToTwo()
		{
			int weeks = SpaceTransit.DurationWeeks(0.04, 1);
			Assert.That(weeks, Is.GreaterThanOrEqualTo(1));
			Assert.That(weeks, Is.LessThanOrEqualTo(2));
		}

		[Test]
		public void DurationWeeks_DefaultWorkshop_IsTwoSixThirteenThirtyNine()
		{
			Assert.That(SpaceTransit.DurationWeeks(0.04, 1), Is.EqualTo(2));
			Assert.That(SpaceTransit.DurationWeeks(1.7, 1), Is.EqualTo(6));
			Assert.That(SpaceTransit.DurationWeeks(4.2, 1), Is.EqualTo(13));
			Assert.That(SpaceTransit.DurationWeeks(79, 1), Is.EqualTo(39));
		}

		[Test]
		public void DurationWeeks_FasterDrive_ShortensBeltAndGate()
		{
			Assert.That(SpaceTransit.DurationWeeks(1.7, 4), Is.EqualTo(2));
			Assert.That(SpaceTransit.DurationWeeks(79, 4), Is.EqualTo(10));
		}

		[Test]
		public void DurationWeeks_MissingSpeed_DefaultsToOne()
		{
			Assert.That(SpaceTransit.DurationWeeks(1.0, 0), Is.EqualTo(SpaceTransit.DurationWeeks(1.0, 1)));
		}

		[Test]
		public void MassFactor_DefaultWorkshop_IsOne()
		{
			Assert.That(SpaceTransit.MassFactor(40000, 4150), Is.EqualTo(1.0).Within(0.0001));
		}

		[Test]
		public void MassFactor_Scout_ClampsToMax()
		{
			Assert.That(SpaceTransit.MassFactor(40000, 2430), Is.EqualTo(1.50).Within(0.0001));
		}

		[Test]
		public void MassFactor_Cargo_ClampsToMin()
		{
			Assert.That(SpaceTransit.MassFactor(40000, 14420), Is.EqualTo(0.67).Within(0.0001));
		}

		[Test]
		public void MassFactor_ZeroThrust_IsOne()
		{
			Assert.That(SpaceTransit.MassFactor(0, 4150), Is.EqualTo(1.0));
		}

		[Test]
		public void DurationWeeks_ApprovedMassTable_ScoutDefaultCargo()
		{
			Assert.That(SpaceTransit.DurationWeeks(0.04, 1.50), Is.EqualTo(2));
			Assert.That(SpaceTransit.DurationWeeks(0.04, 1.00), Is.EqualTo(2));
			Assert.That(SpaceTransit.DurationWeeks(0.04, 0.67), Is.EqualTo(3));

			Assert.That(SpaceTransit.DurationWeeks(1.7, 1.50), Is.EqualTo(4));
			Assert.That(SpaceTransit.DurationWeeks(1.7, 1.00), Is.EqualTo(6));
			Assert.That(SpaceTransit.DurationWeeks(1.7, 0.67), Is.EqualTo(9));

			Assert.That(SpaceTransit.DurationWeeks(4.2, 1.50), Is.EqualTo(9));
			Assert.That(SpaceTransit.DurationWeeks(4.2, 1.00), Is.EqualTo(13));
			Assert.That(SpaceTransit.DurationWeeks(4.2, 0.67), Is.EqualTo(19));

			Assert.That(SpaceTransit.DurationWeeks(79, 1.50), Is.EqualTo(26));
			Assert.That(SpaceTransit.DurationWeeks(79, 1.00), Is.EqualTo(39));
			Assert.That(SpaceTransit.DurationWeeks(79, 0.67), Is.EqualTo(59));
		}

		[Test]
		public void ExitDurationWeeks_BakedEightWeekHop_UsesMassFactor()
		{
			Assert.That(SpaceTransit.ExitDurationWeeks(8, 1.50), Is.EqualTo(6));
			Assert.That(SpaceTransit.ExitDurationWeeks(8, 1.00), Is.EqualTo(8));
			Assert.That(SpaceTransit.ExitDurationWeeks(8, 0.67), Is.EqualTo(12));
		}
	}
}
