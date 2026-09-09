using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region requirements

		private bool online = true;
		public bool Online
		{
			get { return this.online; }
			set { this.online = value; }
		}

		public bool IsActive
		{
			get
			{
				// TODO: autotransfers of crew
                if (!this.IsFormed)
					return false;
                if (this.Modules.Count == 0)
                    return false;
				if (this.QuantityOperational == 0)
                    return false;
                //if (this.CrewRequired + this.ModuleStacks.CrewRequired() > this.CrewCurrent + this.ModuleStacks.CrewCurrent() )
                //    return false;
				// TODO: priorities of shutdown on lack of energy and lack of crew
				if (this.IsRootModuleStack && this.QuantityOperational < this.QuantityActive)
				{
					if (this.EnergyRequired + this.ModuleStacks.EnergyRequired() > this.EnergyProduction + this.ModuleStacks.EnergyProduction())
					{
						return false;
					}
				}
				if (this.QuantityOperational < this.QuantityActive)
				{
					return false;
				}
				return true;
			}
		}

		public bool IsImmobile
		{
			get
			{
				if (!this.IsFormed)
				{
					return true;
				}
				if (!this.IsRootModuleStack)
				{
					return this.RootModuleStack.IsImmobile;
				}
				return !this.canRelocate();
			}
		}

		private bool canRelocate()
		{
			if (!this.IsActive)
			{
				return false;
			}

			foreach (ModuleStack mover in this.spaceMovers())
			{
				if (mover.HasOperationalModules && !mover.NeedFuel(null))
				{
					return true;
				}
			}

			if (this.MoveModes.ContainsKey(EMoveMode.ground))
			{
				return !this.NeedFuel(this.MoveModes[EMoveMode.ground]);
			}

			return false;
		}

		private List<ModuleStack> spaceMovers()
		{
			List<ModuleStack> movers = new List<ModuleStack>();
			this.collectSpaceMovers(this, movers);
			return movers;
		}

		private void collectSpaceMovers(ModuleStack stack, List<ModuleStack> movers)
		{
			if (stack.MoveModes.ContainsKey(EMoveMode.space))
			{
				movers.Add(stack);
			}
			if (stack.ModuleStacks.Count < 1)
			{
				return;
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.collectSpaceMovers(nested, movers);
			}
		}

		public bool NeedFuel(MoveMode moveMode)
		{
			if (this.Fuel.Count == 0)
			{
				return false;
			}

			if (this.Effects.IsFuelled)
			{
				return false;
			}

			if (this.RootModuleStack.ItemStacksSumRecursive.Has(this.Fuel))
			{
				return false;
			}

			return true;
		}

		public string GenerateUniqueModuleStackIdentifier()
		{
			string generated = this.GenerateRandomIdentifier();
			while (ModuleStack.IsInvalidGeneratedName(generated) || ModuleStack.All.ContainsKey(generated))
			{
				generated = this.GenerateRandomIdentifier();
			}
			return generated;
		}

		public static bool IsInvalidGeneratedName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return true;
			}
			int numeric;
			if (int.TryParse(name, out numeric) && numeric == 0)
			{
				return true;
			}
			return false;
		}

		private bool moduleTypeIsCombatArmed(ModuleType type)
		{
			if (type == null)
			{
				return false;
			}
			if (type.Group == EModuleTypesGroup.military
				|| type.Group == EModuleTypesGroup.shuttle)
			{
				return true;
			}
			if (type.Attack > 0
				&& (type.Group == EModuleTypesGroup.vehicle
					|| type.Group == EModuleTypesGroup.infantry))
			{
				return true;
			}
			return false;
		}

		public bool IsArmed
		{
			get
			{
                if (!this.IsFormed)
                {
                    return false;
                }

                if (this.moduleTypeIsCombatArmed(this.moduleType))
                {
                    return true;
                }

                foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                {
                    if (moduleStack.IsArmed)
                    {
                        return true;
                    }
                }

				return false;
			}
		}

		public int EnergyRequired
		{
            get
            {
                if (this.IsFormed)
                { 
                    return this.QuantityActive * this.moduleType.EnergyRequired; 
                }
                return 0;
            }
		}

		public int CrewRequired
		{
			get 
            {
                if (this.IsFormed)
                {
                    return this.QuantityActive * this.moduleType.CrewRequired;
                }
                return 0;
            }
		}

		public int CrewCurrent
		{
			get { return this.ItemStacks.CountCrew() + this.People.Count; }
		}

		#endregion

		#region settlement modules
		public int PopulationMaximum
		{
			get { return this.Quantity * this.moduleType.PopulationMaximum; }
		}

		private int populationCurrent = 0;
		public int PopulationCurrent
		{
			get { return this.populationCurrent; }
			set 
			{
				if (value < 0 || value > this.PopulationMaximum)
				{
					throw new ArgumentOutOfRangeException(); 
				}
				this.populationCurrent = value;
			}
		}

		private double taxRate = 0;
		public double TaxRate
		{
			get { return this.taxRate; }
			set { this.taxRate = value; }
		}

		private double growthRate = 0;
		public double GrowthRate
		{
			get { return this.growthRate; }
			set { this.growthRate = value; }
		}
		#endregion

		public ItemStacks ConsumeNetto
		{
			get
			{
				// the difference between Consume an Upkeep is that you can stop upkeeping (risking damage)
				// but you can't stop consuming by disabling some of the modules
				ItemStacks counsumeItemStacks = new ItemStacks();
                if (this.IsFormed)
                {
                    counsumeItemStacks.Sum(this.ModuleType.Consume);
                    counsumeItemStacks.Multiply(this.Quantity);
                }
				return counsumeItemStacks;
			}
		}

		public ItemStacks Consume
		{
			get
			{
				ItemStacks counsumeItemStacks = new ItemStacks();
				counsumeItemStacks.Sum(this.ConsumeNetto);
				counsumeItemStacks.Sum(this.ItemStacks.Consume);
				counsumeItemStacks.Sum(this.People.Consume);
				counsumeItemStacks.Sum(this.ModuleStacks.Consume);
				return counsumeItemStacks;
			}
		}
		#region energy modules
		public int EnergyProduction
		{
			get
			{
                if (this.IsFormed)
                {
                    return this.QuantityActive * this.moduleType.EnergyProduction;
                }
                return 0;
			}
		}

        public ItemStacks ProduceEnergyConsume
        {
            get
            {
                ItemStacks counsumeItemStacks = new ItemStacks();
                if (this.IsFormed)
                {
                    counsumeItemStacks.Sum(this.ModuleType.ProduceEnergyConsume);
                    counsumeItemStacks.Multiply(this.QuantityActive);
                }
                return counsumeItemStacks;
            }
        }

		#endregion
	}
}
