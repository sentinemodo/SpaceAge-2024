using System;

namespace SpaceAge
{
	public static class ContractScope
	{
		public static string ResolvePlanetOrMoonId(Region region)
		{
			return PressRelease.ResolveScopeId(region);
		}

		public static bool FactionHasPresenceOnBody(Faction faction, Region anchorRegion)
		{
			if (faction == null || anchorRegion == null)
			{
				return false;
			}

			if (anchorRegion.ModuleStacks.Contains(faction))
			{
				return true;
			}

			IRegionHolder holder = anchorRegion.RegionHolder;
			Planet planet = holder as Planet;
			if (planet != null)
			{
				return FactionHasPresenceOnPlanet(faction, planet);
			}

			Moon moon = holder as Moon;
			if (moon != null)
			{
				return FactionHasPresenceOnMoon(faction, moon);
			}

			return false;
		}

		public static bool FactionHasPresenceOnPlanet(Faction faction, Planet planet)
		{
			if (faction == null || planet == null)
			{
				return false;
			}

			foreach (Region region in planet.Regions.Values)
			{
				if (region.ModuleStacks.Contains(faction))
				{
					return true;
				}
			}

			if (planet.Orbit != null && planet.Orbit.ModuleStacks.Contains(faction))
			{
				return true;
			}

			return false;
		}

		public static bool FactionHasPresenceOnMoon(Faction faction, Moon moon)
		{
			if (faction == null || moon == null)
			{
				return false;
			}

			foreach (Region region in moon.Regions.Values)
			{
				if (region.ModuleStacks.Contains(faction))
				{
					return true;
				}
			}

			if (moon.Orbit != null && moon.Orbit.ModuleStacks.Contains(faction))
			{
				return true;
			}

			return false;
		}

		public static bool StackCountsAsPresence(ModuleStack stack, Region anchorRegion)
		{
			if (stack == null || anchorRegion == null || !stack.IsRootModuleStack)
			{
				return false;
			}

			IHolder location = stack.Location;
			if (location == anchorRegion)
			{
				return true;
			}

			Orbit orbit = location as Orbit;
			if (orbit == null)
			{
				return false;
			}

			IRegionHolder holder = anchorRegion.RegionHolder;
			Planet planet = holder as Planet;
			if (planet != null && orbit.OrbitHolder == planet)
			{
				return true;
			}

			Moon moon = holder as Moon;
			if (moon != null && orbit.OrbitHolder == moon)
			{
				return true;
			}

			return false;
		}

		public static bool IsPlayerFaction(Faction faction)
		{
			if (faction == null)
			{
				return false;
			}

			int id;
			if (!int.TryParse(faction.Name, out id))
			{
				return false;
			}

			return id >= 2 && id <= 11;
		}
	}
}
