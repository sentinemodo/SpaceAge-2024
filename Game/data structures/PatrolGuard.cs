using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public static class PatrolGuard
	{
		public static ModuleStack FindBlocker(Region region, Faction moverOwner)
		{
			if (region == null || moverOwner == null)
			{
				return null;
			}

			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Location != region)
				{
					continue;
				}
				if (stack.Owner == moverOwner)
				{
					continue;
				}
				if (!stack.EnforcesPatrol)
				{
					continue;
				}
				if (moverOwner.AttitudeToward(stack.Owner) <= FactionAttitude.Hostile)
				{
					return stack;
				}
			}
			return null;
		}

		public static ModuleStack FindRenameBlocker(NamedObject named, Faction namerOwner)
		{
			if (named == null || namerOwner == null)
			{
				return null;
			}

			Region region = named as Region;
			if (region != null)
			{
				return findRenameBlockerInRegion(region, namerOwner);
			}

			IRegionHolder body = named as IRegionHolder;
			if (body != null)
			{
				foreach (Region bodyRegion in body.Regions.Values)
				{
					ModuleStack blocker = findRenameBlockerInRegion(bodyRegion, namerOwner);
					if (blocker != null)
					{
						return blocker;
					}
				}
			}

			Orbit orbit = named as Orbit;
			if (orbit != null)
			{
				return findRenameBlockerAtLocation(orbit, namerOwner);
			}

			return null;
		}

		private static ModuleStack findRenameBlockerInRegion(Region region, Faction namerOwner)
		{
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Location != region || !stack.EnforcesPatrol || stack.Owner == namerOwner)
				{
					continue;
				}
				if (namerOwner.AttitudeToward(stack.Owner) < FactionAttitude.Ally)
				{
					return stack;
				}
			}
			return null;
		}

		private static ModuleStack findRenameBlockerAtLocation(Location location, Faction namerOwner)
		{
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Location != location || !stack.EnforcesPatrol || stack.Owner == namerOwner)
				{
					continue;
				}
				if (namerOwner.AttitudeToward(stack.Owner) < FactionAttitude.Ally)
				{
					return stack;
				}
			}
			return null;
		}

		public static void DeclareRegionEntryEnemies(ModuleStack attacker, Region region)
		{
			if (attacker == null || region == null || attacker.Owner == null)
			{
				return;
			}

			Faction owner = attacker.Owner;
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Location != region || !stack.IsRootModuleStack || stack.Owner == owner)
				{
					continue;
				}
				if (owner.AttitudeToward(stack.Owner) > FactionAttitude.Hostile)
				{
					continue;
				}
				if (stack.EnforcesPatrol
					|| (stack.IsArmed && stack.HasOperationalModules))
				{
					owner.UnitAttitudes[stack.Name] = FactionAttitude.Enemy;
				}
			}
		}

		public static MoveOrder QueueMoveToRegion(ModuleStack unit, Region region)
		{
			if (unit == null || region == null)
			{
				return null;
			}

			Location here = unit.Location;
			if (here != null && here.Name == region.Name)
			{
				return null;
			}

			MoveOrder move = new MoveOrder(unit);
			move.Route.Add(region);
			return move;
		}
	}
}
