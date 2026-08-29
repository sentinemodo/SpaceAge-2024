using System;

namespace SpaceAge
{
	public static class SpaceTransit
	{
		public const double WeeksPerAuAtSpeedOne = 8.0;

		public static int DurationWeeks(double deltaAu, double speed)
		{
			if (speed <= 0)
			{
				speed = 1;
			}
			double distance = Math.Abs(deltaAu);
			if (distance < 0.0001)
			{
				return 1;
			}
			return Math.Max(1, (int)Math.Ceiling(distance * WeeksPerAuAtSpeedOne / speed));
		}

		public static double BodyAu(object holder)
		{
			if (holder == null)
			{
				return 0;
			}
			if (holder is Orbit)
			{
				return BodyAu(((Orbit)holder).OrbitHolder);
			}
			if (holder is Region)
			{
				return BodyAu(((Region)holder).RegionHolder);
			}
			Planet planet = holder as Planet;
			if (planet != null)
			{
				return planet.AU;
			}
			Moon moon = holder as Moon;
			if (moon != null)
			{
				return moon.Planet != null ? moon.Planet.AU + moon.AU : moon.AU;
			}
			return 0;
		}

		public static double SpaceSpeed(ModuleStack stack)
		{
			if (stack == null)
			{
				return 1;
			}
			if (stack.MoveModes.ContainsKey(EMoveMode.space))
			{
				double speed = stack.MoveModes[EMoveMode.space].Speed;
				return speed > 0 ? speed : 1;
			}
			return 1;
		}
	}
}
