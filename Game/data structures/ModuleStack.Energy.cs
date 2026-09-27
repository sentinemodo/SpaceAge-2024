using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		public int UnitEnergyProduction()
		{
			return this.EnergyProduction + this.ModuleStacks.EnergyProduction();
		}

		public int UnitEnergyRequired()
		{
			return this.EnergyRequired + this.ModuleStacks.EnergyRequired();
		}

		public int RegionalEnergyProduction()
		{
			int total = 0;
			foreach (ModuleStack unit in this.regionalEnergyUnits())
			{
				total += unit.UnitEnergyProduction();
			}
			return total;
		}

		public int RegionalEnergyRequired()
		{
			int total = 0;
			foreach (ModuleStack unit in this.regionalEnergyUnits())
			{
				total += unit.UnitEnergyRequired();
			}
			return total;
		}

		public bool HasRegionalEnergySurplus()
		{
			return this.RegionalEnergyRequired() <= 0
				|| this.RegionalEnergyProduction() >= this.RegionalEnergyRequired();
		}

		private IEnumerable<ModuleStack> regionalEnergyUnits()
		{
			ModuleStack root = this.RootModuleStack;
			yield return root;
			Location location = root.Location;
			if (location == null)
			{
				yield break;
			}

			List<ModuleStack> siblings = new List<ModuleStack>();
			foreach (ModuleStack stack in location.ModuleStacks.Values)
			{
				if (stack == null || stack == root || !stack.IsFormed || !stack.IsRootModuleStack)
				{
					continue;
				}
				if (!stack.Sharing || stack.Owner != this.Owner)
				{
					continue;
				}
				if (this.isUnderSharingAncestor(stack))
				{
					continue;
				}
				siblings.Add(stack);
			}
			siblings.Sort(delegate(ModuleStack a, ModuleStack b)
			{
				return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
			});
			foreach (ModuleStack sibling in siblings)
			{
				yield return sibling;
			}
		}

		private int regionalEnergyOperableCap(int quantityActive)
		{
			if (quantityActive <= 0)
			{
				return 0;
			}

			int regionalAvailable = this.RegionalEnergyProduction();
			int regionalRequired = this.RegionalEnergyRequired();
			if (regionalRequired <= 0 || regionalAvailable >= regionalRequired)
			{
				return quantityActive;
			}

			return (int)((long)quantityActive * regionalAvailable / regionalRequired);
		}
	}
}
