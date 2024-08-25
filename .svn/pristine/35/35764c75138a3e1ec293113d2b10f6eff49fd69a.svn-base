using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class RegionTypes : Dictionary<string, RegionType>
	{
		new public RegionType this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup RegionType [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}
	}
}
