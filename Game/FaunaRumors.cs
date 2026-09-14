namespace SpaceAge
{
	public static class FaunaRumors
	{
		private static readonly string[] FaunaFactionIds = { "14", "15", "16", "17" };

		public static void IssueAll()
		{
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (!stack.IsRootModuleStack || !IsFaunaStack(stack))
				{
					continue;
				}

				Region region = stack.Location as Region;
				if (region == null)
				{
					continue;
				}

				Region settlementNeighbor = FindNeighboringSettlement(region);
				if (settlementNeighbor == null)
				{
					continue;
				}

				Planet planet = GetPlanet(region);
				if (planet == null)
				{
					continue;
				}

				if (RumorExists(planet.Name, stack.Name))
				{
					continue;
				}

				string title = string.Format("Hostile fauna in {0}", region.FullName);
				string flavour = string.Format(
					"Traders report {0} [{1}] in {2} [{3}], adjacent to {4}.",
					stack.ModuleType.FullName,
					stack.Name,
					region.FullName,
					region.Name,
					settlementNeighbor.FullName);
				new PressRelease(null, title, flavour, true, planet.Name);
			}
		}

		private static bool IsFaunaStack(ModuleStack stack)
		{
			if (stack.Owner == null || !stack.IsFormed)
			{
				return false;
			}

			string ownerName = stack.Owner.Name;
			foreach (string faunaFactionId in FaunaFactionIds)
			{
				if (ownerName == faunaFactionId)
				{
					return true;
				}
			}
			return false;
		}

		private static Region FindNeighboringSettlement(Region region)
		{
			foreach (Exit exit in region.Exits)
			{
				Region neighbor = exit.To as Region;
				if (neighbor != null && neighbor.HasSettlement)
				{
					return neighbor;
				}
			}
			return null;
		}

		private static Planet GetPlanet(Region region)
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
			return null;
		}

		private static bool RumorExists(string planetId, string stackId)
		{
			string marker = string.Format("[{0}]", stackId);
			foreach (PressRelease release in PressRelease.All)
			{
				if (!release.Anonymous || release.PlanetId != planetId)
				{
					continue;
				}
				if (release.Flavour.Contains(marker))
				{
					return true;
				}
			}
			return false;
		}
	}
}
