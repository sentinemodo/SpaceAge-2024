using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		private bool energyPriorityOverrideSet = false;
		private int energyPriorityOverride = 0;

		public int EffectiveEnergyPriority
		{
			get
			{
				if (this.energyPriorityOverrideSet)
				{
					return this.energyPriorityOverride;
				}
				if (this.ModuleType == null)
				{
					return ModuleEnergyPriority.DefaultForGroup(EModuleTypesGroup.all);
				}
				return ModuleEnergyPriority.DefaultForGroup(this.ModuleType.Group);
			}
		}

		public bool HasNonDefaultEnergyPriority
		{
			get
			{
				if (!this.energyPriorityOverrideSet || this.ModuleType == null)
				{
					return false;
				}
				return this.energyPriorityOverride
					!= ModuleEnergyPriority.DefaultForGroup(this.ModuleType.Group);
			}
		}

		public void SetEnergyPriority(int priority)
		{
			this.energyPriorityOverride = priority;
			this.energyPriorityOverrideSet = true;
		}

		public int NominalUnitEnergyProduction()
		{
			int total = 0;
			if (this.IsFormed && this.ModuleType != null)
			{
				total = this.QuantityActive * this.ModuleType.EnergyProduction;
			}
			foreach (ModuleStack nested in this.ModuleStacks.Values)
			{
				total += nested.NominalUnitEnergyProduction();
			}
			return total;
		}

		public int NominalUnitEnergyRequired()
		{
			int total = 0;
			if (this.IsFormed && this.ModuleType != null)
			{
				total = this.QuantityActive * this.ModuleType.EnergyRequired;
			}
			foreach (ModuleStack nested in this.ModuleStacks.Values)
			{
				total += nested.NominalUnitEnergyRequired();
			}
			return total;
		}

		public int UnitEnergyProduction()
		{
			int total = 0;
			if (this.IsFormed && this.ModuleType != null)
			{
				total = this.energyOperableModuleCount() * this.ModuleType.EnergyProduction;
			}
			foreach (ModuleStack nested in this.ModuleStacks.Values)
			{
				total += nested.UnitEnergyProduction();
			}
			return total;
		}

		public int UnitEnergyRequired()
		{
			int total = 0;
			if (this.IsFormed && this.ModuleType != null)
			{
				total = this.energyOperableModuleCount() * this.ModuleType.EnergyRequired;
			}
			foreach (ModuleStack nested in this.ModuleStacks.Values)
			{
				total += nested.UnitEnergyRequired();
			}
			return total;
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

		public int RegionalNominalEnergyProduction()
		{
			int total = 0;
			foreach (ModuleStack unit in this.regionalEnergyUnits())
			{
				total += unit.NominalUnitEnergyProduction();
			}
			return total;
		}

		public int RegionalNominalEnergyRequired()
		{
			int total = 0;
			foreach (ModuleStack unit in this.regionalEnergyUnits())
			{
				total += unit.NominalUnitEnergyRequired();
			}
			return total;
		}

		public bool HasRegionalEnergySurplus()
		{
			return this.RegionalNominalEnergyRequired() <= 0
				|| this.RegionalNominalEnergyProduction() >= this.RegionalNominalEnergyRequired();
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

			foreach (ModuleStack nestedProducer in this.nestedEnergyProducers(root))
			{
				yield return nestedProducer;
			}
		}

		private IEnumerable<ModuleStack> nestedEnergyProducers(ModuleStack holder)
		{
			if (holder == null)
			{
				yield break;
			}

			foreach (ModuleStack nested in holder.ModuleStacks.Values)
			{
				if (nested == null || !nested.IsFormed)
				{
					continue;
				}
				if (nested.NominalUnitEnergyProduction() > 0)
				{
					yield return nested;
				}
				foreach (ModuleStack deeper in this.nestedEnergyProducers(nested))
				{
					yield return deeper;
				}
			}
		}

		private int energyOperableModuleCount()
		{
			if (!this.IsFormed || !this.online || this.ModuleType == null)
			{
				return 0;
			}

			if (this.ModuleType.EnergyRequired <= 0 && this.ModuleType.EnergyProduction <= 0)
			{
				return this.QuantityActive;
			}

			Dictionary<ModuleStack, int> allocation = this.getRegionalEnergyAllocation();
			if (allocation.TryGetValue(this, out int operable))
			{
				return operable;
			}

			return this.QuantityActive;
		}

		private static Dictionary<ModuleStack, int> getRegionalEnergyAllocation(ModuleStack context)
		{
			List<ModuleStack> stacks = new List<ModuleStack>();
			foreach (ModuleStack unit in context.regionalEnergyUnits())
			{
				collectEnergyStacks(unit, stacks);
			}

			Dictionary<ModuleStack, int> operable = new Dictionary<ModuleStack, int>();
			foreach (ModuleStack stack in stacks)
			{
				operable[stack] = stack.QuantityActive;
			}

			for (int guard = 0; guard < 10000; guard++)
			{
				int production = sumRegionalEnergy(operable, true);
				int required = sumRegionalEnergy(operable, false);
				if (required <= 0 || production >= required)
				{
					break;
				}

				ModuleStack victim = pickEnergyShutdownStack(operable);
				if (victim == null)
				{
					break;
				}

				operable[victim]--;
			}

			return operable;
		}

		private Dictionary<ModuleStack, int> getRegionalEnergyAllocation()
		{
			return getRegionalEnergyAllocation(this);
		}

		private static void collectEnergyStacks(ModuleStack stack, List<ModuleStack> stacks)
		{
			if (stack == null || !stack.IsFormed || !stack.online || stack.ModuleType == null)
			{
				return;
			}

			if (stack.ModuleType.EnergyRequired > 0 || stack.ModuleType.EnergyProduction > 0)
			{
				stacks.Add(stack);
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				collectEnergyStacks(nested, stacks);
			}
		}

		private static int sumRegionalEnergy(Dictionary<ModuleStack, int> operable, bool production)
		{
			int total = 0;
			foreach (KeyValuePair<ModuleStack, int> entry in operable)
			{
				ModuleStack stack = entry.Key;
				int count = entry.Value;
				if (count <= 0)
				{
					continue;
				}

				if (production)
				{
					total += count * stack.ModuleType.EnergyProduction;
				}
				else
				{
					total += count * stack.ModuleType.EnergyRequired;
				}
			}
			return total;
		}

		private static ModuleStack pickEnergyShutdownStack(Dictionary<ModuleStack, int> operable)
		{
			ModuleStack victim = null;
			foreach (KeyValuePair<ModuleStack, int> entry in operable)
			{
				if (entry.Value <= 0)
				{
					continue;
				}

				ModuleStack stack = entry.Key;
				if (stack.ModuleType.EnergyRequired <= 0)
				{
					continue;
				}

				if (victim == null)
				{
					victim = stack;
					continue;
				}

				int priority = stack.EffectiveEnergyPriority;
				int victimPriority = victim.EffectiveEnergyPriority;
				if (priority > victimPriority)
				{
					victim = stack;
				}
				else if (priority == victimPriority
					&& string.Compare(stack.Name, victim.Name, StringComparison.Ordinal) > 0)
				{
					victim = stack;
				}
			}

			return victim;
		}
	}
}
