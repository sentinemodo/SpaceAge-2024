using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		private IHolder parent;
		public IHolder Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		public bool HasParent
		{
			get
			{
				if (parent is ModuleStack)
				{
					return true;
				}
				return false;
			}
		}

		public ModuleStacks ModuleStacks
		{
			get { return ModuleStack.All[this, false]; }
		}

		public bool HasModuleStacks(ModuleType moduleType = null)
		{
            if (moduleType == null | this.moduleType == moduleType)
			{
				if (this.Quantity > 0)
				{
					return true;
				}
				return false;
            }
            else if (this.ModuleStacks.Count > 0)
            {
                foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                {
                    return moduleStack.HasModuleStacks(moduleType);
                }
            }
            return false;
		}

        public bool HasTechnology(Technology technology = null)
        {
            if (technology == null)
            {
                if (this.technologies.Count > 0)
                {
                    return true;
                }
            }
            else
            {
                if (this.technologies.Contains(technology))
                {
                    return true;
                }
            }
            return false;
        }

		public bool HasOrders
		{
			get
			{
				if (this.Orders.Count > 0)
				{
					return true;
				}
				return false;
			}
		}

		public bool IsFormed
		{
			get 
			{
				if (this.moduleType == null)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		private ModuleType moduleType;
		public ModuleType ModuleType
		{
			get { return this.moduleType; }
			set { this.moduleType = value; }
		}

		private Faction owner;
		public Faction Owner
		{
			get { return this.owner; }
			set { this.owner = value; }
		}

		public List<Faction> Owners
		{
			get
			{
				List<Faction> owners = new List<Faction>
                {
                    this.owner
                };
				foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
				{
					foreach (Faction faction in moduleStack.Owners)
					{
						if (!owners.Contains(faction))
						{
							owners.Add(faction);
						}
					}
				}
				return owners;
			}
		}


		public int Quantity
		{
			get { return this.modules.Count; }
		}

		public int QuantityActive
		{
			get
			{
				int count = 0;
				foreach (Module module in this.modules)
				{
					if (module.IsActive)
					{
						count++;
					}
				}
				return count;
			}
		}

		public int QuantityOperational
		{
			get
			{
				if (!this.IsFormed || !this.online)
				{
					return 0;
				}
				int operable = this.QuantityActive;
				if (operable <= 0)
				{
					return 0;
				}
				if (this.ModuleType.CrewRequired > 0)
				{
					operable = Math.Min(operable, this.CrewCurrent / this.ModuleType.CrewRequired);
				}
				if (this.IsRootModuleStack && this.ModuleType.EnergyRequired > 0)
				{
					int availableEnergy = this.EnergyProduction + this.ModuleStacks.EnergyProduction();
					operable = Math.Min(operable, availableEnergy / this.ModuleType.EnergyRequired);
				}
				return Math.Max(operable, 0);
			}
		}

		public bool HasOperationalModules
		{
			get { return this.QuantityOperational > 0; }
		}

		public bool IsPartiallyDisabled
		{
			get
			{
				return this.HasOperationalModules && this.QuantityOperational < this.QuantityActive;
			}
		}

		public bool IsModuleOperational(Module module)
		{
			if (module == null || module.Parent != this || !module.IsActive || !this.IsFormed || !this.online)
			{
				return false;
			}
			int operationalLeft = this.QuantityOperational;
			foreach (Module candidate in this.modules)
			{
				if (!candidate.IsActive)
				{
					continue;
				}
				if (operationalLeft <= 0)
				{
					return false;
				}
				if (candidate == module)
				{
					return true;
				}
				operationalLeft--;
			}
			return false;
		}

		public void AddModule()
		{
			this.AddModule(0);
		}

        public void AddModules(int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                this.AddModule(0);
            }
        }
        
        public void AddModule(int damage)
		{
            int previousQuantityActive = this.QuantityActive;
			this.modules.Add(new Module(this, damage));
            if (this.QuantityActive != 0)
            {
                this.effects.RecalculateDuration(previousQuantityActive / this.QuantityActive);
            }
        }

		public void AddModule(Module module)
		{
			module.Parent = this;
			this.modules.Add(module);		
		}

		public Module RemoveModule(int index)
		{
            int previousQuantityActive = this.QuantityActive;
            Module module = this.modules[index];
			this.modules.RemoveAt(index);
            if (this.QuantityActive != 0)
            {
                this.effects.RecalculateDuration(previousQuantityActive / this.QuantityActive);
            }
            return module;
        }

        public double MassCapacity
        {
            get
            {
                if (this.MoveModes.ContainsKey(EMoveMode.space))
                {
                    return this.MoveModes[EMoveMode.space].MassCapacity;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double Speed
        {
            get
            {
                if (this.MoveModes.ContainsKey(EMoveMode.ground))
                {
                    return this.MoveModes[EMoveMode.ground].Speed;
                }
                if (this.MoveModes.ContainsKey(EMoveMode.naval))
                {
                    return this.MoveModes[EMoveMode.naval].Speed;
                }
                else
                {
                    return 0;
                }
            }
        }

        public ItemStacks Fuel
        {
            get 
            {
                ItemStacks fuelItemStacks = new ItemStacks();
                if (this.IsFormed)
                {
                    fuelItemStacks.Sum(this.moduleType.Fuel);
                    fuelItemStacks.Multiply(this.Quantity);
                }
                return fuelItemStacks;
            }
        }

        public int FuelDuration
        {
            get
            {
                if (this.IsFormed)
                {
                    return this.moduleType.FuelDuration;
                }
                return 0;
            }
        }

		public double MassNetto
		{
			get 
            {
                if (this.IsFormed)
                {
                    return this.Quantity * this.moduleType.Mass;
                }
                return 0;
            }
		}

		public double Mass
		{
			get { return this.MassNetto + this.ModuleStacks.Mass + this.ItemStacks.Mass() + this.People.Mass(); }
		}

		public double Size
		{
            get
            {
                if (this.IsFormed)
                { 
                    return this.Quantity * this.moduleType.Size; 
                }
                return 0;
            }
		}

		public double Capacity
		{
			get 
			{
				if (this.IsFormed)
				{
					int baseCapacity = (int)(this.Quantity * this.moduleType.Capacity);
					return SkillEffects.ApplyCapacityPercent(
						baseCapacity,
						SkillEffects.CapacityPercent(this));
				}
				else
				{
					return 0;
				}
			}
		}

		public double CapacityUsed
		{
			get { return this.ModuleStacks.Size() + this.ItemStacks.Size() + this.People.Size(); }
		}

		public ItemStacks UpkeepNetto
		{
			get
			{
				ItemStacks upkeepItemStacks = new ItemStacks();
                if (this.IsFormed)
                {
                    upkeepItemStacks.Sum(this.moduleType.Upkeep);
                    upkeepItemStacks.Multiply(this.Quantity);
                }
				return upkeepItemStacks;
			}
		}

		public ItemStacks Upkeep
		{
			get
			{
				ItemStacks upkeepItemStacks = new ItemStacks();
				upkeepItemStacks.Sum(this.UpkeepNetto);
				upkeepItemStacks.Sum(this.ItemStacks.Upkeep);
				upkeepItemStacks.Sum(this.People.Upkeep);
				upkeepItemStacks.Sum(this.ModuleStacks.Upkeep);

				return upkeepItemStacks;
			}
		}

		private Technologies technologies = new Technologies();
		public Technologies Technologies
		{
			get { return this.technologies; }
		}

		// Grant a technology instance onto this stack, or onto the first
		// same-owner stack in the location that has remaining technology
		// capacity (this stack first, then a pre-order walk of the region).
		// First time the owner sees it, queue the description for this turn's
		// report (TechnologiesToShow). Not a player COPY order: overflow relocates
		// instead of failing.
		public void ReceiveTechnologyCopy(Technology technology, int week, string eventDescription)
		{
			if (technology == null)
			{
				return;
			}

			ModuleStack host = this.FindTechnologyCopyHost(technology);
			if (host != this)
			{
				host.ReceiveTechnologyCopyOnThis(technology, week, eventDescription);
				return;
			}

			this.ReceiveTechnologyCopyOnThis(technology, week, eventDescription);
		}

		public bool CanHostTechnologyCopy(Technology technology)
		{
			if (technology == null || this.ModuleType == null)
			{
				return false;
			}
			if (this.Technologies.Contains(technology.Name))
			{
				return true;
			}
			return this.TechnologyCapacity >= this.TechnologyCapacityUsed + technology.Level;
		}

		public ModuleStack FindTechnologyCopyHost(Technology technology)
		{
			if (this.CanHostTechnologyCopy(technology))
			{
				return this;
			}

			Location location = this.Location;
			if (location != null)
			{
				foreach (ModuleStack root in location.ModuleStacks.Values)
				{
					ModuleStack found = this.findTechnologyCopyHostRecursive(root, this.Owner, technology);
					if (found != null)
					{
						return found;
					}
				}
			}

			return this;
		}

		private ModuleStack findTechnologyCopyHostRecursive(ModuleStack stack, Faction owner, Technology technology)
		{
			if (stack == null)
			{
				return null;
			}

			if (stack.Owner == owner && stack.CanHostTechnologyCopy(technology))
			{
				return stack;
			}

			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				ModuleStack nested = this.findTechnologyCopyHostRecursive(child, owner, technology);
				if (nested != null)
				{
					return nested;
				}
			}

			return null;
		}

		private void ReceiveTechnologyCopyOnThis(Technology technology, int week, string eventDescription)
		{
			if (!this.Technologies.Contains(technology.Name))
			{
				this.Technologies.Add(technology);
			}

			if (!string.IsNullOrEmpty(eventDescription))
			{
				this.EventReports.Add(week, eventDescription);
			}

			if (this.Owner != null
				&& !this.Owner.TechnologiesSeen.Contains(technology.Name)
				&& !this.Owner.TechnologiesToShow.Contains(technology.Name))
			{
				this.Owner.TechnologiesToShow.Add(technology);
			}
		}

		// technology capacity limits older equipment usage.
		// to use newer technologies one must build newer facilities 
		// otherwise it would be possible to build ten factories to build level ten modules
		public int TechnologyCapacity
		{
			get { return this.moduleType.TechnologyCapacity; }
		}

        public int TechnologyCapacityUsed
        {
            get 
            {
                int capacityUsed = 0;
                foreach (Technology technology in this.Technologies)
                {
                    capacityUsed += technology.Level;
                }
                return capacityUsed; 
            }
        }
		public bool IsRootModuleStack
		{
			get
			{
				if (this.parent is ModuleStack)
				{
					return false;
				}
				else
				{
					return true;
				}				
			}
		}

		public ModuleStack RootModuleStack
		{
			get
			{
				ModuleStack stack = this;
				while (!stack.IsRootModuleStack)
				{
					stack = (ModuleStack)stack.parent;
				}
				return stack;
			}
		}

		public override Location Location
		{
			get
			{
				return (Location)this.RootModuleStack.Parent;
			}
		}

        public bool IsLocation
        {
            get { return false; }
        }
		public bool HasPresence(Faction faction)
		{

			if (this.Owner == faction)
			{
				return true;
			}

			foreach (Person person in this.People.Values)
			{
				if (person.Owner == faction)
				{
					return true;
				}
			}

			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (moduleStack.HasPresence(faction) == true)
				{
					return true;
				}
			}

			return false;
		}
	}
}
