using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ModuleStack : NamedObject, IHolder, IItemStacksHolder, IOfferent, IReporting, IEventReporting, IEffectable, IMoveable
	{
		public const int NameLength = NamedObject.MaxNameLength;

        public ModuleStack(IHolder parent, string name)
            : base(name)
        {
            if (ModuleStack.All.ContainsKey(name))
                throw new Exception("Modulestack with name [" + name + "] already exists");

            ModuleStack.All.Add(this.name, this);
            this.parent = parent;
        }

		public ModuleStack(IHolder parent, Faction owner, ModuleType type, string name)
			: base(name)
		{
			if (ModuleStack.All.ContainsKey(name))
				throw new Exception("Modulestack with name [" + name + "] already exists");

			ModuleStack.All.Add(this.name, this);

			this.alias = this.name; 
			this.parent = parent;
			this.owner = owner;
			this.moduleType = type;			
		}

		public ModuleStack(IHolder parent, Faction owner, ModuleType type)
			: base("")
		{
			this.name = this.GenerateRandomIdentifier();
			while (ModuleStack.All.ContainsKey(name))
			{
				this.name = this.GenerateRandomIdentifier();
			}

			ModuleStack.All.Add(this.name, this);

			this.alias = this.name;
			this.parent = parent;
			this.owner = owner;
			this.moduleType = type;			
		}

		public ModuleStack(IHolder parent, Faction owner)
			: base("")
		{
			this.name = this.GenerateRandomIdentifier();
			while (ModuleStack.All.ContainsKey(name))
			{
				this.name = this.GenerateRandomIdentifier();
			}

			ModuleStack.All.Add(this.name, this);

			this.parent = parent;
			this.owner = owner;
			this.moduleType = null;
			this.modules = new Modules();
		}

		public ModuleStack(Faction owner, string name)
			: base("")
		{
            if (name.StartsWith("new"))
            {
                // okay if it's a new alias we need to create new identifier
				string generatedRandomIdentifier = this.GenerateRandomIdentifier();

				// now we need to check if we by accident didn't randomly created the non-unique identifier
                if (ModuleStack.All.ContainsKey(generatedRandomIdentifier))
                    throw new Exception("Randomly generated modulestack name of [" + generatedRandomIdentifier + "] already existed.");
				this.name = generatedRandomIdentifier;
            } else
            {
                // if it's not a new alias we need to check if we don't try to create a duplicate
                if (ModuleStack.All.ContainsKey(name))
                    throw new Exception("Modulestack with name [" + name + "] already exists");

                this.name = name;
            }

            while (ModuleStack.All.ContainsKey(this.name))
            {
                this.name = this.GenerateRandomIdentifier();
            }

            ModuleStack.All.Add(this.name, this);

            this.alias = name;

            this.owner = owner;
			this.moduleType = null;
			this.modules = new Modules();
		}

		#region relations

		public static ModuleStacks All = new ModuleStacks();		

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

		#endregion

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
					return this.Quantity * this.moduleType.Capacity;
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
                if (this.CrewRequired > this.CrewCurrent)
                    return false;
                //if (this.CrewRequired + this.ModuleStacks.CrewRequired() > this.CrewCurrent + this.ModuleStacks.CrewCurrent() )
                //    return false;
				// TODO: priorities of shutdown on lack of energy and lack of crew
				if (this.IsRootModuleStack)
				{
					if (this.EnergyRequired + this.ModuleStacks.EnergyRequired() > this.EnergyProduction + this.ModuleStacks.EnergyProduction())
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool IsImmobile
		{
			get
			{
                // if it is isn't it cannot move 
                if (!this.IsFormed)
                    return true;

				// is moveable unit TODO: verify fuel
				if (this.moduleType.Group == EModuleTypesGroup.vehicle
					| this.ModuleType.Group == EModuleTypesGroup.infantry
					| this.ModuleType.Group == EModuleTypesGroup.spacecraft)
				{
					return false;
				}

				// is propulsion type modulestack or has some TODO: verify fuel
				if (this.moduleType.Group == EModuleTypesGroup.propulsion
					| this.ModuleStacks.Contains(EModuleTypesGroup.propulsion))
				{
					return false;
				}

				// has some moveModes and at least one does not need fuel 
				foreach(MoveMode moveMode in this.moduleType.MoveModes.Values)
				{
					if (!this.NeedFuel(moveMode))
					{
						return false;
					}
				}

				// yes, it's immobile
				return true;
			}
		}

		public bool NeedFuel(MoveMode moveMode)
		{
			// need no fuel at all
			if (this.Fuel.Count == 0)
			{
				return false;
			}

			// need fuel but is already fueled
			if (this.Effects.IsFuelled)
			{
				return false;
			} 
			
			// need fuel but has some
			if (this.ItemStacksSumRecursive.Has(this.Fuel))
			{
				return false;
			}

			// need fuel
			return true;
		}

		public bool IsArmed
		{
			get
			{
                if (this.IsFormed)
                {
                    if (this.moduleType.Group == EModuleTypesGroup.military
                        | this.ModuleStacks.Contains(EModuleTypesGroup.military))
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

		#region combat
		public int HitPoints
		{
			get
			{
                if (this.IsFormed)
                {
                    return this.Quantity * this.moduleType.HitPoints;
                }
                return 0;
			}
		}

		private Modules modules = new Modules();
		public Modules Modules
		{
			get { return this.modules; }
		}

		private bool isAvoiding = false;
		public bool IsAvoiding
		{
			get { return this.isAvoiding; }
			set { this.isAvoiding = value; }
		}

		public int Damage
		{
			get
			{
				int damage = 0;
				foreach (Module module in this.modules)
				{
					damage += module.Damage;
				}
				return damage;
			}
		}

		public int Attack
		{
			get
			{
				int attack = 0;
                if (this.IsFormed)
                {
                    attack += this.QuantityActive * this.moduleType.Attack;
                    attack += this.technologies.Attack;
                    attack += this.People.Attack;
                }
				return attack;
			}
		}

		public int Defense
		{
			get
			{
				int defense = 0;
                if (this.IsFormed)
                {
                    defense += this.QuantityActive * this.moduleType.Defense;
                    defense += this.technologies.Defense;
                    defense += this.People.Defense;
                }
				return defense;
			}
		}

		public int Initiative
		{
			get
			{
				return this.InitiativeBonus + this.ModuleStacks.InitiativeBonus();
			}
		}

		public int InitiativeManeuverabilityBonus
		{
			get
			{
				double energyReserves = 0;
				double massCapacityReserves = 0;

                if (this.EnergyRequired + this.ModuleStacks.EnergyRequired() > 0)
				{
					energyReserves = (this.EnergyProduction + this.ModuleStacks.EnergyProduction())
						/ System.Convert.ToDouble(this.EnergyRequired + this.ModuleStacks.EnergyRequired());
				}
				if (this.Mass > 0)
				{
					massCapacityReserves = (this.MassCapacity + this.ModuleStacks.MassCapacity) / this.Mass;
				}
				return System.Convert.ToInt32(energyReserves + massCapacityReserves) * 10;
			}
		}

		public int InitiativeBonus
		{
			get 
			{
				int initiativeBonus = 0;
				if (this.IsRootModuleStack)
				{
					initiativeBonus += this.InitiativeManeuverabilityBonus;
				}
				initiativeBonus += this.technologies.Initiative;
				initiativeBonus += this.People.Initiative;
				return initiativeBonus; 
			}
		}

		private Tactics tactics = new Tactics();
		public Tactics Tactics
		{
			get { return this.tactics; }
		}

		#endregion


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

		#region IItemStacksHolder Members

		private ItemStacks itemStacks = new ItemStacks();
		public ItemStacks ItemStacks
		{
			get { return this.itemStacks; }
		}

        public ItemStack ItemStackSumRecursive(ItemType itemType)
        {
            ItemStack stack = new ItemStack(itemType, 0);
            if (this.ItemStacks.ContainsKey(itemType))
            {
                stack.Quantity += this.ItemStacks[itemType].Quantity;
            }
            if (this.HasModuleStacks())
            {
                foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                {
                    stack.Quantity += moduleStack.ItemStackSumRecursive(itemType).Quantity;
                }
            }
            if (this.HasPeople)
            {
                foreach (Person person in this.People.Values)
                {
                    stack.Quantity += person.ItemStackSumRecursive(itemType).Quantity;
                }
            }
            return stack;
        }

        public ItemStacks ItemStacksSumRecursive
        {
            get
            {
                ItemStacks stacks = new ItemStacks();
                stacks.Sum(this.ItemStacks);

                if (this.HasModuleStacks())
                {
                    foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                    {
                        stacks.Sum(moduleStack.ItemStacksSumRecursive);
                    }
                }
                if (this.HasPeople)
                {
                    foreach (Person person in this.People.Values)
                    {
                        stacks.Sum(person.ItemStacks);
                    }
                }
                return stacks;
            }
        }

		public People People
		{
			get { return Person.All[this, false]; }
		}

        public bool HasPeople
        {
            get
            {
                if (this.People.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
		#endregion




		#region movement

        private IHolder movingTo = null;
        public IHolder MovingTo
		{
			get { return this.movingTo; }
			set { this.movingTo = value; }
		}

		public bool IsRoot
		{
			get 
			{ 
				if (this.parent is Region) 
					return true; 
				else
					return false;  
			}
		}

        public MoveModes MoveModes
        {
            get
            {
                MoveModes moveModes = new MoveModes();
                if (this.IsFormed)
                {
                    MoveMode multipliedMoveMode;
                    foreach (MoveMode moveMode in this.moduleType.MoveModes.Values)
                    {
                        multipliedMoveMode = new MoveMode();
                        multipliedMoveMode.Mode = moveMode.Mode;
                        multipliedMoveMode.Speed = moveMode.Speed;
                        multipliedMoveMode.MassCapacity = this.Quantity * moveMode.MassCapacity;

                        moveModes.Add(multipliedMoveMode.Mode, multipliedMoveMode);
                    }
                }
                return moveModes;
            }
        }
		#endregion


		#region IOrderable Members

		private Orders orders = new Orders();
		public Orders Orders
		{
			get { return this.orders; }
		}

		#endregion

		#region report

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		#endregion

		protected string alias = string.Empty;
		public override string Alias
		{
			get 
            {
                if (this.Owner != null)
                {
                    return string.Format("{0}_{1}",
                        this.owner.Name,
                        this.alias == string.Empty ? this.name : this.alias);
                } else
                {
                    return string.Format("{1}",
                        this.alias == string.Empty ? this.name : this.alias);
                }
            }
			set { this.alias = value; }
		}
        
		public override string ReportName
		{
			get 
			{
				if (!this.IsFormed)
				{
					return string.Format("{0} [{1}]",
						"empty stack",
						this.name);
				}
				return string.Format("{0} [{1}]",
					string.IsNullOrEmpty(this.FullName) ? this.moduleType.FullName : this.FullName,
					this.name);
			}
		}

		public string BattleReportName
		{
			get { return this.ReportName; }
		}

		public string ReportHeader(Faction faction)
		{
			string line;
			if (this.Quantity > 1)
			{
				line = string.Format("{0} {1}, {2} {3}", 
					(this.owner == faction) ? "+" : "-", 
					this.ReportName, 
					this.Quantity, 
					this.moduleType.ReportNameMultiple);
			}
			else
			{
                if (this.IsFormed)
                {
                    line = string.Format("{0} {1}, {2}",
                        (this.owner == faction) ? "+" : "-",
                        this.ReportName,
                        this.moduleType.ReportName);
                }
                else
                {
                    line = string.Format("{0} {1}, {2}",
                         (this.owner == faction) ? "+" : "-",
						"empty stack",
						this.name);
                }
			}
			if (!this.IsActive)
			{
				line = this.reportActive(line);
			}
			if (this.IsRootModuleStack)
			{
				line = string.Format("{0}{1}", line, this.IsImmobile ? ", immobile" : "");
				line = string.Format("{0}{1}", line, this.IsArmed ? ", military" : "");
			}

            if (this.Owner != null & this.Owner != faction)
            {
                line = string.Concat(line, ", owned by ", this.Owner.ReportName);
            }

			return string.Concat(line, ".");
		}

		private string reportDetails(Faction faction)
		{
			string line = "";

			line = this.reportSize(line);
			line = this.reportMass(faction, line);
			line = this.reportCapacity(faction, line);
            line = this.reportResearchPoints(faction, line);
            line = this.reportEnergyUsage(faction, line);
			line = this.reportCrew(faction, line);
			line = this.reportUpkeep(faction, line);
			line = this.reportConsume(faction, line);

			return string.Concat(line, ".");
		}

		private string reportConsume(Faction faction, string line)
		{
			if (this.owner == faction && this.Consume.Count > 0)
			{
				line = string.Format("{0}consume:",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "));

				int netto;
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Consume.Values)
				{
					netto = this.ConsumeNetto.Quantity(itemstack.ItemType);
					line = string.Format("{0} {1}{2}{3}",
						(firstAdded == true) ? string.Concat(line, ",") : line,
						(itemstack.Quantity > 1) ? string.Concat(itemstack.Quantity, " ") : "",
						(netto != itemstack.Quantity) ? string.Concat("(", netto, ") ") : "",
						(netto > 1 || itemstack.Quantity > 1) ? itemstack.ItemType.ReportNameMultiple : itemstack.ItemType.ReportName);

					firstAdded = true;
				}
			}
			return line;
		}

        private string reportProduceEnergyConsume(string line)
        {
            line = string.Empty;
            if (this.ProduceEnergyConsume.Count > 0)
            {
                line = string.Format(" (consume: {0} for {1} weeks)",
                    this.ProduceEnergyConsume.ReportList,
                    this.ModuleType.ProduceDuration);
            }
            return line;
        }
        
        private string reportUpkeep(Faction faction, string line)
		{
			if (this.owner == faction && this.modules.Count > 0 && this.Upkeep.Count > 0)
			{
				line = string.Format("{0}upkeep:",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "));

				int netto;
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Upkeep.Values)
				{
					netto = this.UpkeepNetto.Quantity(itemstack.ItemType);
					line = string.Format("{0} {1}{2}{3}",
						(firstAdded == true) ? string.Concat(line, ",") : line,
						(itemstack.Quantity > 1) ? string.Concat(itemstack.Quantity, " ") : "",
						(netto != itemstack.Quantity) ? string.Concat("(", netto, ") ") : "",
						(netto > 1 || itemstack.Quantity > 1) ? itemstack.ItemType.ReportNameMultiple : itemstack.ItemType.ReportName);

					firstAdded = true;
				}
			}
			return line;
		}

		private string reportCapacity(Faction faction, string line)
		{
			if (this.Capacity > 0)
			{
				if (this.owner == faction | this.ModuleStacks.Contains(faction))
				{
					line = string.Format("{0}, capacity: {1}/{2}", line, this.Capacity, this.CapacityUsed);
				}
			}
			return line;
		}

		public string reportMoveModes()
		{
			string line = "movement speed:";			
			foreach (MoveMode moveMode in this.moduleType.MoveModes.Values) 
			{
                line = string.Format("{0} {1}", 
                    line, 
                    (moveMode.Mode == EMoveMode.ground) ? string.Concat(moveMode.Speed.ToString("F1"), " on ground") : string.Concat((moveMode.MassCapacity / this.Mass).ToString("F1"), " in space"));
			}
			line = string.Concat(line, ".");
			return line;
		}

        public string reportFuel()
        {
            string line = "fuel requirements:";
            bool firstAdded = false;

            foreach (ItemStack fuel in this.Fuel.Values)
            {
                line = string.Format("{0} {1}", line, (firstAdded) ? ", " : "");
                firstAdded = true;

                if (fuel.Quantity > 1)
                {
                    line = string.Format("{0}{1} {2}", line, fuel.Quantity, fuel.ItemType.ReportNameMultiple);
                }
                else
                {
                    line = string.Concat(line, " ", fuel.ItemType.ReportName);
                }
            }
            
            if (this.ModuleType.FuelDuration > 1)
            {
                line = string.Format("{0} per {1} weeks", line, this.ModuleType.FuelDuration);
            }
            else
            {
                line = string.Concat(line, " per week");
            }

            line = string.Concat(line, ".");
            return line;
        }

        private string reportResearchPoints(Faction faction, string line)
        {
            if (this.ResearchPoints > 0)
            {
                if (this.owner == faction)
                {
                    line = string.Format("{0}, research points: {1}", line, this.ResearchPoints);
                }
            }
            return line;
        }

        public List<string> Report(Faction faction, int level)
		{
			ReportLines reportLines = new ReportLines
            {
                { this.ReportHeader(faction), level },
                { this.reportDetails(faction), level + 1 }
            };

            if (this.IsFormed)
            {
                if (this.owner == faction && this.moduleType.MoveModes.Count > 0)
                {
                    reportLines.Add(this.reportMoveModes(), level + 1);
                }

                if (this.owner == faction && this.moduleType.Fuel.Count > 0)
                {
                    reportLines.Add(this.reportFuel(), level + 1);
                }
            }

            if (this.owner == faction && this.technologies.Count > 0)
			{
				reportLines.Add(this.technologies.Report(faction, level + 1));
			}

			if (this.owner == faction && this.itemStacks.Count > 0)
			{
				reportLines.Add(this.itemStacks.Report(faction, level + 1));
			}

			if (this.owner == faction && this.effects.Count > 0)
			{
				reportLines.Add(this.effects.Report(faction, level + 1));
			}

			if (this.owner == faction && this.eventReports.Count > 0)
			{
				reportLines.Add(this.eventReports.Report(faction, level + 1));
			}
	
			foreach (Person person in this.People.Values) 
			{
				if (person.Visible(faction))
				{
					reportLines.Add(person.Report(faction, level + 1));
				}
			}	
			
			// submodulestacks
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values) 
			{
				reportLines.Add(moduleStack.Report(faction, level + 1));
			}
			return reportLines.IndentedLines;
		}

		public bool Visible(Faction faction)
		{
            if (faction == null)
            {
                return true;
            }
            if (faction.FullName == "NPC")
            {
                return true;
            }
			if (this.Owner == faction)
			{
				return true;
			}
			if (this.Size > 0)
			{
				return this.Location.HasPresence(faction);
			}
			return false;
		}

		#endregion

		#region economy

		public bool HasBankAccess
		{
			get { return true; }
		}

        public Offers Offers
        {
            get { return Offer.All[this];  }
        }

        #endregion

        #region research
        private int researchPoints = 0;
        public int ResearchPoints
        {
            get { return this.researchPoints; }
            set { this.researchPoints = value; }
        }
        #endregion

        #region IEffectable Members

        private Effects effects = new Effects();
		public Effects Effects
		{
			get { return this.effects; }
		}

		#endregion

		#region IEventReporting Members

		private EventReports eventReports = new EventReports();
		public EventReports EventReports
		{
			get { return this.eventReports; }
		}

		#endregion

		private bool executedLongOrder = false;
		public bool ExecutedLongOrder
		{
			get { return this.executedLongOrder; }
			set { this.executedLongOrder = value; }
		}

		public bool Execute(int week)
		{
			bool executedOrderByModuleStack;
			
			executedOrderByModuleStack = this.Orders.Execute(week);
			this.Orders.RemoveExecuted();
			this.Effects.Execute(week);
			this.Effects.RemoveExecuted();

			return executedOrderByModuleStack;
		}

		#region battle report

		public List<string> BattleReport(Faction faction)
		{
			ReportLines lines = new ReportLines
            {
                this.ReportHeader(faction),
                { this.battleReportDetails(faction), 1 }
            };

			// submodulestacks
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				lines.Add(moduleStack.BattleReport(faction), 1);
			}
			return lines.IndentedLines;
		}

		private List<string> battleReportDetails(Faction faction)
		{
			ReportLines lines = new ReportLines();

			string line;
			line = string.Empty;
			line = this.reportSize(line);
			line = this.reportMass(faction, line);
			line = this.reportEnergyUsage(faction, line, false);
			line = this.reportCrew(faction, line);
			lines.Add(string.Concat(line, "."));

			line = string.Empty;
			line = this.reportHitPoints(faction, line);
			line = this.reportAttack(faction, line);
			line = this.reportDefense(faction, line);
			line = this.reportInitiative(line);
			lines.Add(string.Concat(line, "."));
			
			if (this.owner == faction & this.tactics.Count > 0)
			{
				lines.Add(string.Format("tactics: {0}.", this.Tactics.ReportList));
			}

			if (this.owner == faction & this.technologies.CountBattleTechnologies > 0)
			{
				line = string.Empty;
				line = this.reportBattleTechnologies(line);
				lines.Add(string.Concat(line, "."));
			}

			//if (this.owner == faction && this.effects.Count > 0)
			//{
			//    reportLines.Add(this.effects.Report(faction, level + 1));
			//}
			lines.Add(this.modules.BattleReport(faction), 1);

			if (this.People.CountBattleSkilled > 0)
			{
				foreach (Person person in this.People.Values)
				{
					if (person.IsBattleSkilled)
					{
						lines.Add(person.BattleReport(faction));
					}
				}
			}
			return lines.IndentedLines;
		}

		private string reportBattleTechnologies(string line)
		{
			//TODO: change as method, pass modulestack
			// change to calculate the effect of a technology use against enemy modulestack it as to be ... er ... used

			line = string.Format("{0}technologies: ",
			   (line == string.Empty) ? string.Empty : string.Concat(line, ", "));

			bool firstAdded = false;
			foreach (Technology technology in this.technologies)
			{
				if (technology.IsBattleTechnology)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;
					line = string.Concat(line, technology.ReportDetails());
				}
			}
			return line;
		}

		private string reportInitiative(string line)
		{
			if (this.Initiative != 0)
			{
				line = string.Format("{0}initiative: {1}",
				   (line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				   this.Initiative);
				if (this.Initiative != (this.InitiativeBonus - this.People.Initiative))
				{
					line = string.Format("{0} ({1})", 
						line,	
						this.InitiativeBonus - this.People.Initiative);
				}
			}
			return line;
		}

		private string reportHitPoints(Faction faction, string line)
		{
			int totalHitPoints = this.HitPoints + this.ModuleStacks.HitPoints();
			line = string.Format("{0}hit points: {1}/{2}",
				(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				totalHitPoints,
				totalHitPoints - this.Damage - this.ModuleStacks.Damage());
			if (totalHitPoints > this.HitPoints)
			{
				line = string.Format("{0} ({1}/{2})",
					line,
					this.HitPoints,
					this.HitPoints - this.Damage);
			}
			return line;
		}

		private string reportDefense(Faction faction, string line)
		{
			if (this.Defense + this.ModuleStacks.Defense() > 0)
			{
				line = string.Format("{0}defense: {1}",
				   (line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				   this.Defense + this.ModuleStacks.Defense());
			}
			if (this.ModuleStacks.Defense() > 0)
			{
				line = string.Format("{0} ({1})", line, this.Defense);
			}
			return line;
		}

		private string reportAttack(Faction faction, string line)
		{
			if (this.Attack + this.ModuleStacks.Attack() > 0)
			{
				line = string.Format("{0}attack: {1}",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
					this.Attack + this.ModuleStacks.Attack());
			}
			if (this.ModuleStacks.Attack() > 0)
			{
				line = string.Format("{0} ({1})", line, this.Attack);
			}
			return line;
		}

		private string reportSize(string line)
		{
			return string.Format("{0}size: {1}", line, this.Size);
		}

		private string reportMass(Faction faction, string line)
		{
			if (this.owner == faction)
			{                
                if ( this.MassCapacity + this.ModuleStacks.MassCapacity > 0)
                {
                    line = string.Format("{0}, mass: {1}/{2}",
                        line,
                        this.MassCapacity + this.ModuleStacks.MassCapacity,
                        this.Mass);
                } else
                {
                    line = string.Format("{0}, mass: {1}",
                        line,
                        this.Mass);
                }

                if (this.Mass > this.ModuleStacks.Mass)
                {
                    line = string.Format("{0} ({1})", line, this.MassNetto);
                }
            } 
			return line;
		}

		private string reportEnergyUsage(Faction faction, string line, bool showConsume = true)
		{
			if (this.owner == faction || (this.ModuleStacks.Contains(faction)))
			{
				if (this.EnergyProduction + this.ModuleStacks.EnergyProduction() > 0)
				{
					line = string.Format("{0}, energy: {1}/{2}{3}",
						line,
						this.EnergyProduction + this.ModuleStacks.EnergyProduction(),
						this.EnergyRequired + this.ModuleStacks.EnergyRequired(),
                        (showConsume & this.ProduceEnergyConsume.Count > 0) ? string.Format(" (consume: {0} for {1} weeks)",
                            this.ProduceEnergyConsume.ReportList,
                            this.ModuleType.ProduceDuration) : string.Empty);
				}
				else if (this.EnergyRequired + this.ModuleStacks.EnergyRequired() > 0)
				{
					line = string.Format("{0}, energy: {1}",
						line,
						this.EnergyRequired + this.ModuleStacks.EnergyRequired());
				}

				if (this.ModuleStacks.EnergyRequired() > 0)
				{
					line = string.Format("{0} ({1})", line, this.EnergyRequired);
				}
			}
			return line;
		}

		private string reportCrew(Faction faction, string line)
		{
			if (this.owner == faction)
			{
				if (this.CrewRequired + this.ModuleStacks.CrewRequired() > 0)
				{
					line = string.Format("{0}, crew: {1}/{2}",
						line,
						this.CrewRequired + this.ModuleStacks.CrewRequired(),
						this.CrewCurrent + this.ModuleStacks.CrewCurrent());
				}
				if (this.CrewRequired + this.CrewCurrent > 0 & this.ModuleStacks.CrewRequired() > 0)
				{
					line = string.Format("{0} ({1}/{2})", line, this.CrewRequired, this.CrewCurrent);
				}
			}
			return line;
		}

		private string reportActive(string line)
		{
			if (line != string.Empty)
				line = string.Concat(line, ", ");
			if (!this.online)
			{
				line = string.Concat(line, "deactivated");
			}
			else if (this.IsActive)
			{
				line = string.Concat(line, "active");
			}
			else
			{
				line = string.Concat(line, "disabled");
			}
			return line;
		}

		public List<string> ReportOrdersTemplateHeader(Faction faction)
		{
			ReportLines reportLines = new ReportLines
            {
                this.ReportHeader(faction)
            };

			if (this.Technologies.Count > 0)
			{
				reportLines.Add(string.Concat("technologies: ", this.Technologies.ReportList, "."));
			}
			if (this.ItemStacks.Count > 0)
			{
				reportLines.Add(string.Concat("items: ", this.ItemStacks.ReportList, "."));
			}
			if (this.Effects.Count > 0)
			{
				foreach (string line in this.Effects.Report(faction))
				{
					reportLines.Add(line);
				}
			}

			return reportLines.IndentedLines;
		}

		# endregion

		public Modules GetModules(EModuleTypesGroup group)
		{			
			Modules groupModules = new Modules();
			if (this.moduleType.Group == group)
			{
				groupModules.AddRange(this.modules);
			}
			foreach (ModuleStack modulestack in this.ModuleStacks.Values)
			{
				groupModules.AddRange(modulestack.GetModules(group));
			}
			return groupModules;
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

        public override void LoadXml(XmlElement elModuleStack)
        {
            base.LoadXml(elModuleStack);
            this.Owner = Faction.All[elModuleStack.GetAttribute("faction")];
            this.ModuleType = ModuleType.All[elModuleStack.GetAttribute("type")];
            
            if (elModuleStack.HasAttribute("description"))
            {
                this.Description = elModuleStack.GetAttribute("description");
            }

            int modules = this.XMLAssignInteger(elModuleStack.GetAttribute("quantity"), 1); 
            
            // TODO: here need to implement damage that currently does not transfers between turns
            for (int i = 0; i < modules; i++)
            {
                this.AddModule();
            }

            this.People.LoadXml(elModuleStack, this);                
            this.ItemStacks.LoadXml(elModuleStack, this);
            this.Upkeep.LoadXml(elModuleStack, this, "upkeep");

            this.ModuleStacks.LoadXml(elModuleStack, this);
            this.Technologies.LoadXml(elModuleStack, this);
			this.ResearchPoints = this.XMLAssignInteger(elModuleStack.GetAttribute("research-points"), 0);

            this.Tactics.LoadXml(elModuleStack, this);

            this.Offers.LoadXml(elModuleStack, this.Location.Market, this);
            this.Effects.LoadXml(elModuleStack, this);
            this.EventReports.LoadXml(elModuleStack, this);

        }

        public XmlElement SaveXml(XmlDocument doc, Faction faction = null)
        {
            base.SaveXml(doc, "modulestack");
            
            this.xmlElement.SetAttribute("type", this.ModuleType.Name);
            this.xmlElement.SetAttribute("quantity", this.Quantity.ToString());
            this.xmlElement.SetAttribute("faction", this.Owner.Name);

            if (this.Description != null & this.Description != string.Empty)                
            {
                this.xmlElement.SetAttribute("description", this.Description);
            }

            this.People.SaveXml(doc, this.xmlElement, faction);
            this.Technologies.SaveXml(doc, this.xmlElement);
			if (this.ResearchPoints > 0)
			{
				this.xmlElement.SetAttribute("research-points", this.ResearchPoints.ToString());
			}
            this.ItemStacks.SaveXml(doc, this.xmlElement, faction);
            this.Upkeep.SaveXml(doc, this.xmlElement, faction, "upkeep");
            this.Tactics.SaveXml(doc, this.xmlElement);

            this.Offers.SaveXml(doc, this.xmlElement);
            this.Effects.SaveXml(doc, this.xmlElement, faction);
            this.EventReports.SaveXml(doc, this.xmlElement, faction);
            return this.xmlElement;
        }
	}
}
