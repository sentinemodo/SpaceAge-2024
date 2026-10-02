using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public static class StackVisibility
	{
		public static bool IsVisibleToFaction(ModuleStack target, Faction faction)
		{
			if (faction == null || target == null)
			{
				return true;
			}
			if (faction.FullName == "NPC")
			{
				return true;
			}
			if (target.Owner == faction)
			{
				return true;
			}
			if (target.Size <= 0)
			{
				return false;
			}
			SpaceSystem targetSystem = GetSpaceSystem(target.Location);
			if (targetSystem == null)
			{
				return false;
			}
			if (!FactionHasObserverInSystem(faction, targetSystem))
			{
				return false;
			}
			foreach (ModuleStack observer in EnumerateObservers(faction, targetSystem))
			{
				if (CanDetect(observer, target))
				{
					return true;
				}
			}
			return false;
		}

		public static bool CanDetect(ModuleStack observer, ModuleStack target)
		{
			if (observer == null || target == null || !observer.IsFormed || observer.Size <= 0)
			{
				return false;
			}
			int detection = ObserverDetection(observer, target);
			int stealth = EffectiveStealth(target, observer);
			return detection > stealth;
		}

		public static int EffectiveStealth(ModuleStack target, ModuleStack observer)
		{
			int stealth = 0;
			if (target.ModuleType != null)
			{
				stealth += target.ModuleType.Stealth;
			}
			stealth += target.NestDepth;
			if (target.IsUnderwaterStealthy)
			{
				stealth += 1;
			}
			stealth += RegionalStealth(observer, target);
			stealth += GeoStealth(observer, target);
			if (IsFaunaFaction(target.Owner) && OrbitDetectionBonusApplies(observer, target))
			{
				stealth += 1;
			}
			return stealth;
		}

		public static int ObserverDetection(ModuleStack observer, ModuleStack target)
		{
			int detection = 0;
			if (observer.ModuleType != null)
			{
				detection += observer.ModuleType.Detection;
			}
			detection += TechnologyDetectionBonus(observer);
			if (OrbitDetectionBonusApplies(observer, target) && !target.IsUnderwaterStealthy)
			{
				detection += 1;
			}
			detection += UnderwaterChannelBonus(observer, target);
			return detection;
		}

		public static int NestDepth(ModuleStack stack)
		{
			int depth = 0;
			IHolder parent = stack.Parent;
			while (parent is ModuleStack)
			{
				depth++;
				parent = ((ModuleStack)parent).Parent;
			}
			return depth;
		}

		private static int TechnologyDetectionBonus(ModuleStack stack)
		{
			int bonus = 0;
			foreach (Technology technology in stack.Technologies)
			{
				bonus += technology.DetectionBonus;
			}
			return bonus;
		}

		private static bool OrbitDetectionBonusApplies(ModuleStack observer, ModuleStack target)
		{
			Orbit observerOrbit = observer.Location as Orbit;
			if (observerOrbit == null)
			{
				return false;
			}
			IOrbitHolder targetBody = GetOrbitHolderForStack(target);
			return targetBody != null && ReferenceEquals(observerOrbit.OrbitHolder, targetBody);
		}

		private static int UnderwaterChannelBonus(ModuleStack observer, ModuleStack target)
		{
			if (!target.IsUnderwaterStealthy || observer.Owner == null)
			{
				return 0;
			}
			Location targetLocation = target.Location;
			Region targetRegion = targetLocation as Region;
			if (targetRegion != null && targetRegion.HasUnderwaterPresence(observer.Owner))
			{
				return 1;
			}
			if (HasObserverSpaceshipOnTargetBodyOrbit(observer.Owner, target))
			{
				return 1;
			}
			return 0;
		}

		private static bool HasObserverSpaceshipOnTargetBodyOrbit(Faction faction, ModuleStack target)
		{
			Orbit orbit = FindBodyOrbit(target);
			if (orbit == null)
			{
				return false;
			}
			foreach (ModuleStack stack in orbit.ModuleStacks.Values)
			{
				if (stack.Owner == faction
					&& stack.ModuleType != null
					&& stack.ModuleType.IsShipHullType
					&& stack.Quantity > 0)
				{
					return true;
				}
			}
			return false;
		}

		private static Orbit FindBodyOrbit(ModuleStack stack)
		{
			Location location = stack.Location;
			Region region = location as Region;
			if (region != null && region.RegionHolder != null)
			{
				Planet planet = region.RegionHolder as Planet;
				if (planet != null)
				{
					return planet.Orbit;
				}
				Moon moon = region.RegionHolder as Moon;
				if (moon != null)
				{
					return moon.Orbit;
				}
			}
			Orbit orbit = location as Orbit;
			if (orbit != null)
			{
				return orbit;
			}
			Belt belt = location as Belt;
			if (belt != null && belt.Planet != null)
			{
				return belt.Planet.Orbit;
			}
			return null;
		}

		private static int RegionalStealth(ModuleStack observer, ModuleStack target)
		{
			Region observerRegion = GetHostRegion(observer.Location);
			Region targetRegion = GetHostRegion(target.Location);
			if (observerRegion == null || targetRegion == null)
			{
				return 0;
			}
			if (observerRegion == targetRegion)
			{
				return 0;
			}
			if (!SameRegionHolder(observerRegion, targetRegion))
			{
				return 0;
			}
			return RegionHopCount(observerRegion, targetRegion);
		}

		private static int GeoStealth(ModuleStack observer, ModuleStack target)
		{
			VisibilityAnchor observerAnchor = GetAnchor(observer.Location);
			VisibilityAnchor targetAnchor = GetAnchor(target.Location);
			int stealth = 0;
			if (observerAnchor.SpaceSystem != null
				&& targetAnchor.SpaceSystem != null
				&& !ReferenceEquals(observerAnchor.SpaceSystem, targetAnchor.SpaceSystem))
			{
				stealth += 10;
			}
			if (observerAnchor.Planet != null
				&& targetAnchor.Planet != null
				&& !ReferenceEquals(observerAnchor.Planet, targetAnchor.Planet))
			{
				stealth += 1;
			}
			if (observerAnchor.Moon != null
				&& targetAnchor.Moon != null
				&& !ReferenceEquals(observerAnchor.Moon, targetAnchor.Moon))
			{
				stealth += 1;
			}
			stealth += BeltGeoStealth(observer, target, observerAnchor, targetAnchor);
			return stealth;
		}

		private static int BeltGeoStealth(
			ModuleStack observer,
			ModuleStack target,
			VisibilityAnchor observerAnchor,
			VisibilityAnchor targetAnchor)
		{
			if (targetAnchor.Belt == null)
			{
				return 0;
			}
			if (ReferenceEquals(observerAnchor.Belt, targetAnchor.Belt))
			{
				return 0;
			}
			if (observerAnchor.Region != null
				&& targetAnchor.Belt != null
				&& targetAnchor.Planet != null
				&& ReferenceEquals(observerAnchor.Planet, targetAnchor.Planet))
			{
				return 2;
			}
			return 1;
		}

		private static VisibilityAnchor GetAnchor(Location location)
		{
			VisibilityAnchor anchor = new VisibilityAnchor();
			if (location == null)
			{
				return anchor;
			}
			Region region = location as Region;
			if (region != null)
			{
				anchor.Region = region;
				anchor.Moon = region.RegionHolder as Moon;
				Planet planet = region.RegionHolder as Planet;
				if (planet != null)
				{
					anchor.Planet = planet;
					anchor.SpaceSystem = planet.SpaceSystem;
				}
				else if (anchor.Moon != null)
				{
					anchor.Planet = anchor.Moon.Planet;
					anchor.SpaceSystem = anchor.Moon.SpaceSystem;
				}
				return anchor;
			}
			Orbit orbit = location as Orbit;
			if (orbit != null)
			{
				anchor.Orbit = orbit;
				Planet planet = orbit.OrbitHolder as Planet;
				if (planet != null)
				{
					anchor.Planet = planet;
					anchor.SpaceSystem = planet.SpaceSystem;
				}
				Moon moon = orbit.OrbitHolder as Moon;
				if (moon != null)
				{
					anchor.Moon = moon;
					anchor.Planet = moon.Planet;
					anchor.SpaceSystem = moon.SpaceSystem;
				}
				return anchor;
			}
			Belt belt = location as Belt;
			if (belt != null)
			{
				anchor.Belt = belt;
				anchor.Planet = belt.Planet;
				anchor.SpaceSystem = belt.SpaceSystem;
				return anchor;
			}
			return anchor;
		}

		private static Region GetHostRegion(Location location)
		{
			return location as Region;
		}

		private static bool SameRegionHolder(Region a, Region b)
		{
			return ReferenceEquals(a.RegionHolder, b.RegionHolder);
		}

		private static int RegionHopCount(Region from, Region to)
		{
			if (ReferenceEquals(from, to))
			{
				return 0;
			}
			Queue<Region> queue = new Queue<Region>();
			Dictionary<Region, int> distance = new Dictionary<Region, int>();
			queue.Enqueue(from);
			distance[from] = 0;
			while (queue.Count > 0)
			{
				Region current = queue.Dequeue();
				int currentDistance = distance[current];
				foreach (Exit exit in current.Exits)
				{
					Region neighbour = exit.To as Region;
					if (neighbour == null || !SameRegionHolder(current, neighbour))
					{
						continue;
					}
					if (distance.ContainsKey(neighbour))
					{
						continue;
					}
					int next = currentDistance + 1;
					if (ReferenceEquals(neighbour, to))
					{
						return next;
					}
					distance[neighbour] = next;
					queue.Enqueue(neighbour);
				}
			}
			return int.MaxValue / 4;
		}

		private static IOrbitHolder GetOrbitHolderForStack(ModuleStack stack)
		{
			Location location = stack.Location;
			Region region = location as Region;
			if (region != null)
			{
				return region.RegionHolder as IOrbitHolder;
			}
			Orbit orbit = location as Orbit;
			if (orbit != null)
			{
				return orbit.OrbitHolder;
			}
			Belt belt = location as Belt;
			if (belt != null && belt.Planet != null)
			{
				return belt.Planet;
			}
			return null;
		}

		private static SpaceSystem GetSpaceSystem(Location location)
		{
			return GetAnchor(location).SpaceSystem;
		}

		private static bool FactionHasObserverInSystem(Faction faction, SpaceSystem system)
		{
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Owner != faction || !stack.IsRootModuleStack || !stack.IsFormed || stack.Size <= 0)
				{
					continue;
				}
				SpaceSystem stackSystem = GetSpaceSystem(stack.Location);
				if (ReferenceEquals(stackSystem, system))
				{
					return true;
				}
			}
			return false;
		}

		private static IEnumerable<ModuleStack> EnumerateObservers(Faction faction, SpaceSystem system)
		{
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Owner != faction || !stack.IsRootModuleStack || !stack.IsFormed || stack.Size <= 0)
				{
					continue;
				}
				if (ReferenceEquals(GetSpaceSystem(stack.Location), system))
				{
					yield return stack;
				}
			}
		}

		private static bool IsFaunaFaction(Faction faction)
		{
			if (faction == null || string.IsNullOrEmpty(faction.Name))
			{
				return false;
			}
			int id;
			if (!int.TryParse(faction.Name, out id))
			{
				return false;
			}
			return id >= 14 && id <= 17;
		}

		public static bool CanSeeModuleForSeeOrder(
			ModuleStack observer,
			ModuleStack target,
			ESeeScope scope,
			Region atRegion)
		{
			if (observer == null || target == null || !observer.IsFormed || !target.IsFormed)
			{
				return false;
			}
			if (!MatchesSeeLocationScope(observer.Location, target.Location, scope, atRegion, observer.Location))
			{
				return false;
			}
			if (target.Owner == observer.Owner)
			{
				return true;
			}
			return CanDetect(observer, target);
		}

		public static bool CanSeePersonForSeeOrder(
			ModuleStack observer,
			Person target,
			ESeeScope scope,
			Region atRegion)
		{
			if (observer == null || target == null || !observer.IsFormed || !target.IsFormed)
			{
				return false;
			}
			if (!MatchesSeeLocationScope(observer.Location, target.Location, scope, atRegion, observer.Location))
			{
				return false;
			}
			if (target.Owner == observer.Owner)
			{
				return true;
			}
			return ModuleStack.All[observer.Location, true].Contains(target.Name);
		}

		private static bool MatchesSeeLocationScope(
			Location observerLocation,
			Location targetLocation,
			ESeeScope scope,
			Region atRegion,
			Location defaultLocation)
		{
			switch (scope)
			{
				case ESeeScope.Anywhere:
					return true;
				case ESeeScope.AtRegion:
					return atRegion != null && ReferenceEquals(targetLocation, atRegion);
				default:
					return ReferenceEquals(targetLocation, defaultLocation);
			}
		}

		private struct VisibilityAnchor
		{
			public SpaceSystem SpaceSystem;
			public Planet Planet;
			public Moon Moon;
			public Region Region;
			public Orbit Orbit;
			public Belt Belt;
		}
	}
}
