using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ModuleType : NamedType, IMultiple, IProducable
	{
		public static ModuleTypes All = new ModuleTypes();

		public ModuleType(string name)
			: base(name) 
		{			
			ModuleType.All.Add(this.name, this);
		}

		protected string fullNameMultiple = string.Empty;
		public string FullNameMultiple
		{
			get { return this.fullNameMultiple; }
			set { this.fullNameMultiple = value; }
		}

		public string ReportNameMultiple
		{
			get { return this.fullNameMultiple + " [" + this.name + "]"; }
		}

		private EModuleTypesGroup group;
		public EModuleTypesGroup Group
		{
			get { return this.group; }
			set { this.group = value; }
		}

		private double mass;
		public double Mass
		{
			get { return this.mass; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.mass = value;
			}
		}

		private double size;
		public double Size
		{
			get { return this.size; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.size = value;
			}
		}

		private double capacity;
		public double Capacity
		{
			get { return this.capacity; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.capacity = value;
			}
		}

        #region research

        private int researchOutput = 0;
        public int ResearchOutput
        {
            get { return this.researchOutput; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.researchOutput = value;
            }
        }

        private int technologyCapacity;
		public int TechnologyCapacity
		{
			get { return this.technologyCapacity; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.technologyCapacity = value;
			}
		}

        #endregion

        #region requirements
        private int crewRequired;
		public int CrewRequired
		{
			get { return this.crewRequired; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.crewRequired = value;
			}
		}
		
		private int energyRequired;
		public int EnergyRequired
		{
			get { return this.energyRequired; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.energyRequired = value;
			}
		}

		private ItemStacks upkeep = new ItemStacks();
		public ItemStacks Upkeep
		{
			get { return this.upkeep; }
		}

		public string NoUpkeepEffect { get; set; }
		public int NoUpkeepChance { get; set; }
		public string NoConsumeEffect { get; set; }
		public int NoConsumeChance { get; set; }

		public string HealTarget { get; set; }
		public int HealQuantity { get; set; }
		public int HealWeeks { get; set; }
		public int HealQuantityWithItem { get; set; }
		public int HealWeeksWithItem { get; set; }
		public string HealConsumeItem { get; set; }
		public int HealConsumeQuantity { get; set; }
		#endregion

		#region movement
		private MoveModes moveModes = new MoveModes();
		public MoveModes MoveModes
		{
			get { return this.moveModes; }
		}
		#endregion

		#region settlement modules
		private int populationMaximum;
		public int PopulationMaximum
		{
			get { return this.populationMaximum; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				this.populationMaximum = value;
			}
		}
		#endregion

		#region production modules

		// this properties are used for modules that produce something automaticaly without additional use of technology
		// examples are: automated factories, energy modules, drive modules etc
		private ItemStacks consume = new ItemStacks();
		public ItemStacks Consume
		{
			get { return this.consume; }
		}

		private ItemStacks itemsProduction = new ItemStacks();
		public ItemStacks ItemsProduction
		{
			get { return this.itemsProduction; }
		}

		private int energyProduction;
		public int EnergyProduction
		{
			get { return this.energyProduction; }
			set { this.energyProduction = value; }
		}

		private int produceDuration;
		public int ProduceDuration
		{
			get { return this.produceDuration; }
			set { this.produceDuration = value; }
		}
        
        private ItemStacks produceEnergyConsume = new ItemStacks();
        public ItemStacks ProduceEnergyConsume
        {
            get { return this.produceEnergyConsume; }
        }
        
        private double massCapacity;
		public double MassCapacity
		{
			get { return this.massCapacity; }
			set { this.massCapacity = value; }
		}

		private List<EProducableEffects> effectsProduction = new List<EProducableEffects>();
		public List<EProducableEffects> EffectsProduction
		{
			get { return this.effectsProduction; }
		}

		#endregion

		#region combat
		private int damageCapacity;
		public int DamageCapacity
		{
			get { return this.damageCapacity; }
			set { this.damageCapacity = value; }
		}

		private int damage;
		public int Damage
		{
			get { return this.damage; }
			set { this.damage = value; }
		}

		private int attack;
		public int Attack
		{
			get { return this.attack; }
			set { this.attack = value; }
		}

		private int defense;
		public int Defense
		{
			get { return this.defense; }
			set { this.defense = value; }
		}

		#endregion


		#region conditions
		private LocationTypes operationCondition_LocationTypes = new LocationTypes();
        public LocationTypes OperationCondition_LocationTypes
		{
            get { return this.operationCondition_LocationTypes; }
		}

		private ItemTypes operationCondition_AtmosphereResources = new ItemTypes();
		public ItemTypes OperationCondition_AtmosphereResources
		{
			get { return this.operationCondition_AtmosphereResources; }
		}

        private LocationTypes useCondition_LocationTypes = new LocationTypes();
        public LocationTypes UseCondition_LocationTypes
        {
            get { return this.useCondition_LocationTypes; }
        }

        private bool useCondition_RequireFuel = false;
        public bool UseCondition_RequireFuel
        {
            get { return this.useCondition_RequireFuel; }
            set { this.useCondition_RequireFuel = value; }
        }

        private double useCondition_EfficiencyMultiplier = 1;
        public double UseCondition_EfficiencyMultiplier
        {
            get { return this.useCondition_EfficiencyMultiplier; }
            set { this.useCondition_EfficiencyMultiplier = value; }
        }

        private ItemStacks fuel = new ItemStacks();
        public ItemStacks Fuel
        {
            get { return this.fuel; }
            set { this.fuel = value; }
        }

        private int fuelDuration;
        public int FuelDuration
        {
            get { return this.fuelDuration; }
            set { this.fuelDuration = value; }
        }


		#endregion
	}
}
