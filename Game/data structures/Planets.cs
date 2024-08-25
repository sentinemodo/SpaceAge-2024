using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Planets : Dictionary<string, Planet>
	{
		new public Planet this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup Planet [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}	
	}
}
