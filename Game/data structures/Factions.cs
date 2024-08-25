using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Factions : Dictionary<string, Faction>
	{
		new public Faction this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Faction [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}
	}
}
