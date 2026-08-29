using System;

namespace SpaceAge
{
	public enum EGravityBand
	{
		low,
		normal,
		high
	}

	public enum ETemperatureBand
	{
		habitable,
		cold,
		hot
	}

	public enum EAtmosphereBand
	{
		none,
		thin,
		terair,
		hostile
	}

	public static class BodyEnvironment
	{
		public static EGravityBand ParseGravity(string token)
		{
			switch ((token ?? string.Empty).ToLowerInvariant())
			{
				case "low":
					return EGravityBand.low;
				case "high":
					return EGravityBand.high;
				default:
					return EGravityBand.normal;
			}
		}

		public static ETemperatureBand ParseTemperature(string token)
		{
			switch ((token ?? string.Empty).ToLowerInvariant())
			{
				case "habitable":
					return ETemperatureBand.habitable;
				case "hot":
					return ETemperatureBand.hot;
				default:
					return ETemperatureBand.cold;
			}
		}

		public static EAtmosphereBand ParseAtmosphere(string token)
		{
			switch ((token ?? string.Empty).ToLowerInvariant())
			{
				case "thin":
					return EAtmosphereBand.thin;
				case "terair":
					return EAtmosphereBand.terair;
				case "hostile":
					return EAtmosphereBand.hostile;
				default:
					return EAtmosphereBand.none;
			}
		}

		public static string GravityToken(EGravityBand band)
		{
			return band.ToString().ToLowerInvariant();
		}

		public static string TemperatureToken(ETemperatureBand band)
		{
			return band.ToString().ToLowerInvariant();
		}

		public static string AtmosphereToken(EAtmosphereBand band)
		{
			return band.ToString().ToLowerInvariant();
		}

		public static int LaunchSurcharge(EGravityBand gravity, EAtmosphereBand atmosphere)
		{
			int surcharge;
			if (gravity == EGravityBand.high)
			{
				surcharge = 16;
			}
			else if (gravity == EGravityBand.low)
			{
				surcharge = atmosphere == EAtmosphereBand.none ? 0 : 2;
			}
			else
			{
				surcharge = atmosphere == EAtmosphereBand.none ? 4 : 8;
			}
			if (atmosphere == EAtmosphereBand.hostile)
			{
				surcharge += 4;
			}
			return surcharge;
		}

		public static bool TryGetBody(object location, out Planet planet, out Moon moon)
		{
			planet = null;
			moon = null;
			if (location is Region)
			{
				object holder = ((Region)location).RegionHolder;
				planet = holder as Planet;
				moon = holder as Moon;
				return planet != null || moon != null;
			}
			if (location is Orbit)
			{
				object holder = ((Orbit)location).OrbitHolder;
				planet = holder as Planet;
				moon = holder as Moon;
				return planet != null || moon != null;
			}
			planet = location as Planet;
			moon = location as Moon;
			return planet != null || moon != null;
		}

		public static EGravityBand GravityAt(object location)
		{
			Planet planet;
			Moon moon;
			if (!TryGetBody(location, out planet, out moon))
			{
				return EGravityBand.normal;
			}
			return moon != null ? moon.GravityBand : planet.GravityBand;
		}

		public static ETemperatureBand TemperatureAt(object location)
		{
			Planet planet;
			Moon moon;
			if (!TryGetBody(location, out planet, out moon))
			{
				return ETemperatureBand.cold;
			}
			return moon != null ? moon.TemperatureBand : planet.TemperatureBand;
		}

		public static EAtmosphereBand AtmosphereAt(object location)
		{
			Planet planet;
			Moon moon;
			if (!TryGetBody(location, out planet, out moon))
			{
				return EAtmosphereBand.none;
			}
			return moon != null ? moon.AtmosphereBand : planet.AtmosphereBand;
		}

		public static bool AllowsSettlement(string moduleTypeName, ETemperatureBand temperature)
		{
			if (temperature == ETemperatureBand.habitable)
			{
				return true;
			}
			if (temperature == ETemperatureBand.cold)
			{
				return moduleTypeName == "clddom" || moduleTypeName == "cryhab";
			}
			if (temperature == ETemperatureBand.hot)
			{
				return moduleTypeName == "hotdom";
			}
			return true;
		}
	}
}
