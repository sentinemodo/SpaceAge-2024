using System;
using System.Collections.Generic;

namespace SpaceAge
{
	// Research feature mechanics, kept in one file for easy management:
	//  - technology cost model (per-level default, overridable in the datafile),
	//  - weekly research-point output of a research modulestack,
	//  - breakthrough probability (constant weekly hazard).
	//
	// Costs double per level (L1=8, L2=16, L3=32, ...). Level 0 technologies are
	// the free starting set and are never researched. With a single base research
	// lab (research-output 1, one module -> output 1) the constant weekly hazard
	// p = 1 - (1 - 1/cost)^output gives a cumulative breakthrough chance of ~80%
	// after 1 quarter (L1), 2 quarters (L2) and 4 quarters (L3).
	public static class Research
	{
		public const int BaseCost = 8;

		public static int DefaultCostForLevel(int level)
		{
			if (level <= 0)
			{
				return 0;
			}
			return BaseCost * (1 << (level - 1));
		}

		public static int CostOf(Technology technology)
		{
			return technology.Cost;
		}

		// Research points produced per week = lab research-output x number of modules.
		public static int WeeklyOutput(ModuleStack researcher)
		{
			if (researcher == null || researcher.ModuleType == null)
			{
				return 0;
			}
			return researcher.ModuleType.ResearchOutput * researcher.Modules.Count;
		}

		// Weekly breakthrough roll against the cheapest available technology: each of
		// the `output` research points produced this week is an independent 1/cost
		// chance, i.e. p_week = 1 - (1 - 1/cost)^output. Rolls go through Sequence so
		// tests are deterministic (push 0 to force a breakthrough).
		public static bool RollBreakthrough(Technologies available, int output)
		{
			if (available == null || available.Count == 0 || output <= 0)
			{
				return false;
			}

			int cheapestCost = int.MaxValue;
			foreach (Technology technology in available)
			{
				int cost = CostOf(technology);
				if (cost > 0 && cost < cheapestCost)
				{
					cheapestCost = cost;
				}
			}
			if (cheapestCost == int.MaxValue)
			{
				return false;
			}

			for (int point = 0; point < output; point++)
			{
				if (Sequence.GenerateRandomInt(0, cheapestCost, "research breakthrough") == 0)
				{
					return true;
				}
			}
			return false;
		}

		// Technologies a researcher may currently break through to: level 1..maxLevel+1,
		// within the lab's remaining technology capacity, and not already held.
		public static Technologies AvailableTechnologies(ModuleStack researcher)
		{
			Technologies available = new Technologies();
			foreach (Technology technology in Technology.All)
			{
				if (technology.Level >= 1
					&& technology.Level <= researcher.Owner.MaxTechnologyLevel + 1
					&& technology.Level <= researcher.TechnologyCapacity - researcher.TechnologyCapacityUsed
					&& researcher.Technologies[technology.Name] == null)
				{
					available.Add(technology);
				}
			}
			return available;
		}

		// Pick a technology weighted by 1/level (lower-level technologies are likelier).
		public static Technology WeightedRandom(Technologies technologies)
		{
			int totalArea = 0;
			foreach (Technology technology in technologies)
			{
				totalArea += 100 / technology.Level;
			}
			if (totalArea <= 0)
			{
				return null;
			}
			int roll = Sequence.GenerateRandomInt(0, totalArea, "research technology selection");
			foreach (Technology technology in technologies)
			{
				roll -= 100 / technology.Level;
				if (roll <= 0)
				{
					return technology;
				}
			}
			return null;
		}

		// On a breakthrough, choose which technology is discovered. When the order carries a
		// preference, there is a ~50% chance to pick from the preferred subset (if any),
		// otherwise a technology is chosen from all available ones.
		public static Technology SelectResearchedTechnology(ResearchOrder order, Technologies available)
		{
			if (order.ResearchType == EResearchType.Feature)
			{
				return Technology.All[order.ResearchToken];
			}
			if (order.ResearchType != EResearchType.Any)
			{
				Technologies preferred = PreferredTechnologies(order, available);
				if (preferred.Count > 0 && Sequence.GenerateRandomInt(0, 100, "research preference") <= 50)
				{
					return WeightedRandom(preferred);
				}
			}
			return WeightedRandom(available);
		}

		// Resolve the order's parameter into the preferred subset of available technologies.
		public static Technologies PreferredTechnologies(ResearchOrder order, Technologies available)
		{
			Technologies preferred = new Technologies();
			switch (order.ResearchType)
			{
				case EResearchType.Technology:
					// technologies enabled by (requiring) the named technology
					foreach (Technology technology in available)
					{
						if (technology.Requires == order.Technology)
						{
							preferred.Add(technology);
						}
					}
					break;
				case EResearchType.Tag:
					foreach (Technology technology in available)
					{
						if (technology.HasTag(order.ResearchToken))
						{
							preferred.Add(technology);
						}
					}
					break;
				case EResearchType.ItemType:
					foreach (Technology technology in available)
					{
						if (UsesItem(technology, order.ItemType))
						{
							preferred.Add(technology);
						}
					}
					break;
				case EResearchType.ModuleType:
					foreach (Technology technology in available)
					{
						if (UsesModule(technology, order.ModuleType))
						{
							preferred.Add(technology);
						}
					}
					break;
				case EResearchType.Group:
					foreach (Technology technology in available)
					{
						if (technology.UseCondition_ModuleTypesGroup.ToString() == order.ResearchToken
							|| (technology.UseProduceModules != null && technology.UseProduceModules.Group.ToString() == order.ResearchToken))
						{
							preferred.Add(technology);
						}
					}
					break;
				case EResearchType.SpaceObject:
					HashSet<ItemType> resources = ResourcesOf(order.ResearchToken);
					foreach (Technology technology in available)
					{
						if (UsesAnyItem(technology, resources))
						{
							preferred.Add(technology);
						}
					}
					break;
			}
			return preferred;
		}

		public static bool IsTag(string token)
		{
			foreach (Technology technology in Technology.All)
			{
				if (technology.HasTag(token))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsSpaceObject(string token)
		{
			return Region.All.ContainsKey(token)
				|| Orbit.All.ContainsKey(token)
				|| Planet.All.ContainsKey(token)
				|| Moon.All.ContainsKey(token)
				|| Star.All.ContainsKey(token);
		}

		private static bool UsesItem(Technology technology, ItemType item)
		{
			return (technology.UseProduceItems != null && technology.UseProduceItems.ContainsKey(item))
				|| (technology.UseConsumeItems != null && technology.UseConsumeItems.ContainsKey(item));
		}

		private static bool UsesModule(Technology technology, ModuleType module)
		{
			return (technology.UseProduceModules != null && technology.UseProduceModules == module)
				|| (technology.UseConsumeModules != null && technology.UseConsumeModules == module);
		}

		private static bool UsesAnyItem(Technology technology, HashSet<ItemType> items)
		{
			foreach (ItemType item in items)
			{
				if (UsesItem(technology, item))
				{
					return true;
				}
			}
			return false;
		}

		private static HashSet<ItemType> ResourcesOf(string name)
		{
			HashSet<ItemType> items = new HashSet<ItemType>();
			if (Region.All.ContainsKey(name))
			{
				AddResources(Region.All[name], items);
			}
			else if (Orbit.All.ContainsKey(name))
			{
				AddResources(Orbit.All[name], items);
			}
			else if (Planet.All.ContainsKey(name))
			{
				AddHolderResources(Planet.All[name], items);
			}
			else if (Moon.All.ContainsKey(name))
			{
				AddHolderResources(Moon.All[name], items);
			}
			else if (Star.All.ContainsKey(name))
			{
				AddHolderResources(Star.All[name], items);
			}
			return items;
		}

		private static void AddResources(IResourcesHolder holder, HashSet<ItemType> items)
		{
			foreach (Resource resource in holder.Resources)
			{
				items.Add(resource.ItemType);
			}
		}

		private static void AddHolderResources(SpaceSystemObject spaceObject, HashSet<ItemType> items)
		{
			IRegionHolder regionHolder = spaceObject as IRegionHolder;
			if (regionHolder != null)
			{
				foreach (Region region in regionHolder.Regions.Values)
				{
					AddResources(region, items);
				}
			}
			IOrbitHolder orbitHolder = spaceObject as IOrbitHolder;
			if (orbitHolder != null && orbitHolder.Orbit != null)
			{
				AddResources(orbitHolder.Orbit, items);
			}
		}
	}
}
