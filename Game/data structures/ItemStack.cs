using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ItemStack : XMLProcessing
	{
		public static ItemStack Cash(int amount)
		{
			return new ItemStack(ItemType.All.Cash, amount);
		}

		public bool Visible(Faction faction)
		{
			return true;
		}

		private ItemType itemType;
		public ItemType ItemType
		{
			get { return this.itemType; }
			set { this.itemType = value; }
		}

		private int quantity;
		public int Quantity
		{
			get { return this.quantity; }
			set { this.quantity = value; }
		}

		public double Mass
		{
			get { return this.quantity * this.itemType.Mass; }
		}

		public double Size
		{
			get { return this.quantity * this.itemType.Size; }
		}

		public ItemStack(ItemType type)
		{
			this.itemType = type;
			this.quantity = 1;
		}

		public ItemStack(ItemType type, int quantity)
		{
			this.itemType = type;
			this.quantity = quantity;
		}

		public ItemStacks Upkeep
		{
			get
			{
				ItemStacks upkeepItemStacks = new ItemStacks();
				upkeepItemStacks.Sum(this.itemType.Upkeep);
				upkeepItemStacks.Multiply(this.quantity);
				return upkeepItemStacks;
			}
		}

		public ItemStacks Consume
		{
			get
			{
				ItemStacks consumeItemStacks = new ItemStacks();
				consumeItemStacks.Sum(this.itemType.Consume);
				consumeItemStacks.Multiply(this.quantity);
				return consumeItemStacks;
			}
		}

		public string ReportName
		{
			get
			{
				string line = string.Empty;
				if (this.quantity > 1)
				{
					line = string.Format("{0} {1}", this.quantity, this.itemType.ReportNameMultiple);
				}
				else
				{
					line = this.itemType.ReportName;
				}
				return line;
			}
		}


        public override void LoadXml(XmlElement elItemStack)
        {
            this.Quantity = this.XMLAssignInteger(elItemStack.GetAttribute("quantity"), 1);
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            this.xmlElement = doc.CreateElement("itemstack");

            this.xmlElement.SetAttribute("type", this.ItemType.Name);
            this.xmlElement.SetAttribute("quantity", this.Quantity.ToString());

            return this.xmlElement;
        }

        public XmlElement SaveXml(XmlDocument doc, string itemStackGroup)
        {
            this.xmlElement = doc.CreateElement(itemStackGroup);

            this.xmlElement.SetAttribute("type", this.ItemType.Name);
            this.xmlElement.SetAttribute("quantity", this.Quantity.ToString());

            return this.xmlElement;
        }
    }
}
