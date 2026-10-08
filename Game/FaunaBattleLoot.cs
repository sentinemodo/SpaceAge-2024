using System.Collections.Generic;

namespace SpaceAge
{
	public static class FaunaBattleLoot
	{
		private static readonly string[] FaunaFactionIds = { "14", "15", "16", "17" };

		private const string ArborPlanetId = "P00001";
		private const string AnvilPlanetId = "P00005";

		public static bool IsWildFaunaFaction(Faction faction)
		{
			if (faction == null || string.IsNullOrEmpty(faction.Name))
			{
				return false;
			}

			string ownerName = faction.Name;
			foreach (string faunaFactionId in FaunaFactionIds)
			{
				if (ownerName == faunaFactionId)
				{
					return true;
				}
			}
			return false;
		}

		public static List<ItemStack> LootForDestroyedModule(Planet planet)
		{
			List<ItemStack> loot = new List<ItemStack>();
			int food = 12;
			if (planet != null && planet.Name == ArborPlanetId)
			{
				food = 25;
			}

			loot.Add(new ItemStack(ItemType.All["food"], food));

			if (planet != null && planet.Name == AnvilPlanetId)
			{
				loot.Add(new ItemStack(ItemType.All["copper"], 5));
				loot.Add(new ItemStack(ItemType.All["iron"], 5));
				loot.Add(new ItemStack(ItemType.All["titani"], 3));
			}

			return loot;
		}

		public static Planet ResolvePlanet(Location location)
		{
			Region region = location as Region;
			if (region == null)
			{
				return null;
			}

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

		public static void DropFromDestroyedModule(
			Battle battle,
			ModuleStack faunaStack,
			List<ModuleStack> recipients,
			int week)
		{
			if (faunaStack == null || faunaStack.Owner == null || !IsWildFaunaFaction(faunaStack.Owner))
			{
				return;
			}

			if (recipients == null || recipients.Count == 0)
			{
				return;
			}

			Planet planet = ResolvePlanet(faunaStack.Location);
			List<ItemStack> templates = LootForDestroyedModule(planet);
			foreach (ItemStack template in templates)
			{
				if (template.Quantity <= 0)
				{
					continue;
				}
				battle.SplitLootAmongRecipients(recipients, template);
				battle.ReportFaunaLoot(template);
			}
		}
	}
}
