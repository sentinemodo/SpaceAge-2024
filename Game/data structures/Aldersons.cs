using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public class Aldersons : Dictionary<string, Alderson>
	{
		new public Alderson this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Alderson [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}

		public Aldersons this[SpaceSystem system]
		{
			get
			{
				Aldersons list = new Aldersons();
				foreach (Alderson alderson in this.Values)
				{
					if (alderson.SpaceSystem == system)
					{
						list.Add(alderson.Name, alderson);
					}
				}
				return list;
			}
		}
	}
}
