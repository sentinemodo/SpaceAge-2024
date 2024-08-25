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

		public ItemStack ItemStack(int quantity = 1)
		{
			return new ItemStack(this, quantity);
		}

	}
}
