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

		public static bool HasEnvironmentAttrs(object location)
		{
			Planet planet;
			Moon moon;
			if (!TryGetBody(location, out planet, out moon))
			{
				return false;
			}
			return moon != null ? moon.HasEnvironmentAttrs : planet.HasEnvironmentAttrs;
		}

		public static int SurfaceOrbitSurcharge(object location)
		{
			if (!HasEnvironmentAttrs(location))
			{
				return 0;
			}
			return LaunchSurcharge(GravityAt(location), AtmosphereAt(location));
		}

		public static bool IsSurfaceOrbitHop(Location from, Location to)
		{
			if (from == null || to == null)
			{
				return false;
			}
			bool regionToOrbit = from is Region && to is Orbit;
			bool orbitToRegion = from is Orbit && to is Region;
			if (!regionToOrbit && !orbitToRegion)
			{
				return false;
			}
			return from.LocationParent == to.LocationParent;
		}

		public static bool BansNonShuttleSurfaceHop(ModuleType moduleType, Location from, Location to)
		{
			if (moduleType == null || !IsSurfaceOrbitHop(from, to))
			{
				return false;
			}
			if (!moduleType.IsShipHullType || moduleType.IsShuttleUnit)
			{
				return false;
			}
			Location surface = from is Region ? from : to;
			if (!HasEnvironmentAttrs(surface))
			{
				return false;
			}
			return AtmosphereAt(surface) != EAtmosphereBand.none;
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

		public static ELocationType EffectiveLocationType(Location location)
		{
			if (location == null)
			{
				return ELocationType.solidSurface;
			}
			Orbit orbit = location as Orbit;
			if (orbit != null)
			{
				Planet planet = orbit.OrbitHolder as Planet;
				if (planet != null
					&& planet.PlanetType != null
					&& planet.PlanetType.Name == "gasgnt"
					&& planet.AtmosphereBand != EAtmosphereBand.none)
				{
					return ELocationType.atmosphere;
				}
			}
			return location.LocationType;
		}

		public static PlanetType PlanetTypeAt(object location)
		{
			Planet planet;
			Moon moon;
			if (!TryGetBody(location, out planet, out moon))
			{
				return null;
			}
			if (moon != null)
			{
				return moon.Planet != null ? moon.Planet.PlanetType : null;
			}
			return planet.PlanetType;
		}

		public static bool HasAtmosphereResource(Location location, ItemType required)
		{
			if (required == null)
			{
				return true;
			}
			IResourcesHolder resourcesHolder = location as IResourcesHolder;
			if (resourcesHolder != null && resourcesHolder.Resources.Contains(required))
			{
				return true;
			}
			return AtmosphereToken(AtmosphereAt(location)) == required.Name;
		}

		public static bool HasAtmosphereResources(Location location, ItemTypes required)
		{
			if (required == null || required.Count == 0)
			{
				return true;
			}
			bool hasExplicitEnvironment = HasEnvironmentAttrs(location);
			foreach (ItemType item in required.Values)
			{
				if (HasAtmosphereResource(location, item))
				{
					continue;
				}
				if (!hasExplicitEnvironment)
				{
					continue;
				}
				return false;
			}
			return true;
		}

		public static bool MatchesPlanetType(Location location, PlanetTypes required)
		{
			if (required == null || required.Count == 0)
			{
				return true;
			}
			PlanetType planetType = PlanetTypeAt(location);
			if (planetType == null)
			{
				return false;
			}
			return required.ContainsKey(planetType.Name);
		}
	}
}
