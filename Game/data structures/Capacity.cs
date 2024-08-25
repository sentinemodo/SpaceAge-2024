using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Capacity
	{
		private EModuleTypesGroup group;
		public EModuleTypesGroup Group
		{
			get { return this.group; }
			set { this.group = value; }
		}

		private int quantity;
		public int Quantity
		{
			get { return this.quantity; }
			set { this.quantity = value; }
		}
	}
}
