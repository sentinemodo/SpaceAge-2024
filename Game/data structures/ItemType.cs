using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ItemType : NamedType, IMultiple
	{
		public static ItemTypes All = new ItemTypes();		

		public ItemType(string name)
			: base(name) 
		{
            if (!ItemType.All.ContainsKey(this.name))
            {
                ItemType.All.Add(this.name, this);
            }
		}

		private EItemTypesGroup group = EItemTypesGroup.resource;
		public EItemTypesGroup Group
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

		private ItemStacks upkeep = new ItemStacks();
		public ItemStacks Upkeep
		{
			get { return this.upkeep; }
		}

		private ItemStacks consume = new ItemStacks();
		public ItemStacks Consume
		{
			get { return this.consume; }
		}

		private int attack;
		public int Attack
		{
			get { return this.attack; }
			set { this.attack = value; }
		}

		private int damage;
		public int Damage
		{
			get { return this.damage; }
			set { this.damage = value; }
		}

		private EModuleTypesGroup? useAllowedModuleTypesGroup = null;
		public EModuleTypesGroup? UseAllowedModuleTypesGroup
		{
			get { return this.useAllowedModuleTypesGroup; }
			set { this.useAllowedModuleTypesGroup = value; }
		}

		public string ReportDescriptionForTechnology()
		{
			StringBuilder description = new StringBuilder(this.Description);
			bool isEquipment = this.UseAllowedModuleTypesGroup != null || this.Attack > 0 || this.Damage > 0;
			if (isEquipment && (this.Size > 0 || this.Mass > 0))
			{
				description.AppendFormat(" Size: {0}, mass: {1}.", this.Size, this.Mass);
			}
			if (this.Attack > 0 || this.Damage > 0)
			{
				description.AppendFormat(" Attack: {0}, damage: {1}.", this.Attack, this.Damage);
				if (this.UseAllowedModuleTypesGroup == EModuleTypesGroup.infantry)
				{
					description.AppendFormat(
						" An infantry battalion carrying one gains +{0} attack and +{1} damage.",
						this.Attack,
						this.Damage);
				}
			}
			return description.ToString();
		}

		public ItemStack ItemStack(int quantity = 1)
		{
			return new ItemStack(this, quantity);
		}

	}
}
