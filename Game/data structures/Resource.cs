using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Resource
	{
		public bool Visible (Faction faction)
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

		public string ReportName
		{
			get
			{
				if (this.quantity > 1)
				{
					return string.Format("{0} {1}", this.quantity, this.itemType.ReportNameMultiple);
				}
				else
				{
					return this.itemType.ReportName;
				}
			}
		}
	}
}
