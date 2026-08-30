using System;

namespace SpaceAge
{
	public static class SpaceTransit
	{
		private const double MoonAuLimit = 0.1;
		private const double MoonWeeksPerAu = 50.0;
		private const double BeltAnchorAu = 1.7;
		private const double BeltAnchorWeeks = 6.0;
		private const double GateAnchorWeeks = 39.0;

		public const double ReferenceThrust = 40000.0;
		public const double ReferenceMass = 4150.0;
		public const double MassFactorMin = 0.67;
		public const double MassFactorMax = 1.50;

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
			double weeksAtSpeedOne;
			if (distance < MoonAuLimit)
			{
				weeksAtSpeedOne = distance * MoonWeeksPerAu;
			}
			else
			{
				double beltArg = 1.0 + BeltAnchorAu;
				double logSpan = Math.Log((1.0 + 79.0) / beltArg);
				weeksAtSpeedOne = BeltAnchorWeeks
					+ (GateAnchorWeeks - BeltAnchorWeeks) * Math.Log((1.0 + distance) / beltArg) / logSpan;
			}
			return DurationWeeksFromRaw(weeksAtSpeedOne, speed);
		}

		private static int DurationWeeksFromRaw(double weeksAtSpeedOne, double speed)
		{
			if (weeksAtSpeedOne >= 1.0 - 1e-9 && weeksAtSpeedOne <= 1.0 + 1e-9)
			{
				return 1;
			}
			return RoundUpWeeks(weeksAtSpeedOne / speed);
		}

		public static double MassFactor(double thrust, double mass)
		{
			if (thrust <= 0)
			{
				return 1;
			}
			double load = thrust / Math.Max(mass, 1);
			double referenceLoad = ReferenceThrust / ReferenceMass;
			double factor = load / referenceLoad;
			if (factor < MassFactorMin)
			{
				return MassFactorMin;
			}
			if (factor > MassFactorMax)
			{
				return MassFactorMax;
			}
			return factor;
		}

		public static double SpaceThrust(ModuleStack stack)
		{
			if (stack == null)
			{
				return 0;
			}
			return stack.MassCapacity + stack.ModuleStacks.MassCapacity;
		}

		public static double SpaceSpeed(ModuleStack stack)
		{
			double speed = spaceSpeedRecursive(stack);
			return speed > 0 ? speed : 1;
		}

		public static double EffectiveSpaceSpeed(ModuleStack stack)
		{
			if (stack == null)
			{
				return 1;
			}
			return SpaceSpeed(stack) * MassFactor(SpaceThrust(stack), stack.Mass);
		}

		public static int ExitDurationWeeks(int exitDuration, double speed)
		{
			if (speed <= 0)
			{
				speed = 1;
			}
			return RoundUpWeeks(exitDuration / speed);
		}

		private static int RoundUpWeeks(double fractionalWeeks)
		{
			return Math.Max(1, (int)Math.Ceiling(fractionalWeeks - 1e-9));
		}

		public static int ExitDurationWeeks(int exitDuration, ModuleStack stack)
		{
			return ExitDurationWeeks(exitDuration, EffectiveSpaceSpeed(stack));
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
			Alderson alderson = holder as Alderson;
			if (alderson != null)
			{
				return alderson.AU;
			}
			Belt belt = holder as Belt;
			if (belt != null)
			{
				return belt.AU;
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

		private static double spaceSpeedRecursive(ModuleStack stack)
		{
			if (stack == null)
			{
				return 0;
			}
			double speed = 0;
			if (stack.MoveModes.ContainsKey(EMoveMode.space))
			{
				speed = stack.MoveModes[EMoveMode.space].Speed;
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				double nestedSpeed = spaceSpeedRecursive(nested);
				if (nestedSpeed > speed)
				{
					speed = nestedSpeed;
				}
			}
			return speed;
		}
	}
}
