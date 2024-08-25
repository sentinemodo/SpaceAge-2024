using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class SpacePoints : Dictionary<string, SpacePoint>
	{
		new public SpacePoint this[string index]
		{
			get
			{
				try
				{
					return base[index];
				}
				catch (Exception ex)
				{
					throw new KeyNotFoundException("Tried to lookup SpacePoint [" + index + "]", ex);
				}
			}
			set { base[index] = value; }
		}
	}
}
