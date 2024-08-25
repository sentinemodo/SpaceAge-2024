using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class ItemTypes : Dictionary<string, ItemType>
	{
		new public ItemType this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup ItemType [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

		public ItemType Cash
		{
			get
			{
				return ItemType.All["cash"];
			}
		}
	}
}
