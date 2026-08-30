using System;

namespace SpaceAge
{
	public static class SurveyReports
	{
		public static void SeedTurnOneHomeBlurbs(Game game, Faction faction)
		{
			if (game == null || faction == null || game.Turn != 1)
			{
				return;
			}

			int factionId;
			if (!int.TryParse(faction.Name, out factionId) || factionId < 2 || factionId > 11)
			{
				return;
			}

			Planet homePlanet = FindHomePlanet(faction);
			Star homeStar = FindHomeStar(homePlanet);
			QueueIfNew(faction, homeStar);
			QueueIfNew(faction, homePlanet);
		}

		public static void QueueIfNew(Faction faction, NamedObject spaceObject)
		{
			if (faction == null || spaceObject == null || string.IsNullOrEmpty(spaceObject.Description))
			{
				return;
			}
			if (faction.ObjectsSeen.Contains(spaceObject.Name) || faction.ObjectsToShow.Contains(spaceObject.Name))
			{
				return;
			}
			faction.ObjectsToShow.Add(spaceObject);
		}

		private static Planet FindHomePlanet(Faction faction)
		{
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Owner != faction
					|| stack.ModuleType == null
					|| stack.ModuleType.Name != "corphq")
				{
					continue;
				}
				return PlanetFromLocation(stack.Location);
			}
			return null;
		}

		private static Planet PlanetFromLocation(Location location)
		{
			if (location == null)
			{
				return null;
			}

			Region region = location as Region;
			if (region != null)
			{
				Planet planet = region.RegionHolder as Planet;
				if (planet != null)
				{
					return planet;
				}
				Moon moon = region.RegionHolder as Moon;
				if (moon != null)
				{
					return moon.Planet;
				}
			}

			Orbit orbit = location as Orbit;
			if (orbit != null)
			{
				Planet planet = orbit.OrbitHolder as Planet;
				if (planet != null)
				{
					return planet;
				}
			}

			return null;
		}

		private static Star FindHomeStar(Planet planet)
		{
			if (planet == null || planet.SpaceSystem == null)
			{
				return null;
			}

			foreach (SpaceSystemObject spaceSystemObject in planet.SpaceSystem.Objects.Values)
			{
				Star star = spaceSystemObject as Star;
				if (star != null)
				{
					return star;
				}
			}
			return null;
		}
	}
}
