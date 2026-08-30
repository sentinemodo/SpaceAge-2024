using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public static class RepairScope
	{
		public static ModuleStack For(ModuleStack repairer)
		{
			ModuleStack parent = repairer.Parent as ModuleStack;
			if (parent != null)
			{
				return parent;
			}
			return repairer;
		}

		public static int TotalDamage(ModuleStack stack)
		{
			int damage = stack.Damage;
			foreach (ModuleStack nested in nestedSorted(stack))
			{
				damage += TotalDamage(nested);
			}
			return damage;
		}

		public static int ApplyRepair(ModuleStack stack, int points)
		{
			int remaining = points;
			foreach (Module module in stack.Modules)
			{
				if (remaining < 1)
				{
					break;
				}
				if (module.Damage < 1)
				{
					continue;
				}
				int repair = module.Damage;
				if (repair > remaining)
				{
					repair = remaining;
				}
				module.Damage -= repair;
				remaining -= repair;
			}
			foreach (ModuleStack nested in nestedSorted(stack))
			{
				if (remaining < 1)
				{
					break;
				}
				remaining -= ApplyRepair(nested, remaining);
			}
			return points - remaining;
		}

		private static List<ModuleStack> nestedSorted(ModuleStack stack)
		{
			List<ModuleStack> nested = new List<ModuleStack>();
			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				nested.Add(child);
			}
			nested.Sort(ModuleStacks.CompareByNames);
			return nested;
		}
	}
}
